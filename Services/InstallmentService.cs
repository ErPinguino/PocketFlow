using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PocketFlow.Data;
using PocketFlow.Models;
using PocketFlow.Repositories;
using PocketFlow.ViewModels.Installments;
using PocketFlow.ViewModels.Shared;

namespace PocketFlow.Services;

public class InstallmentService : IInstallmentService
{
    private readonly ApplicationDbContext _context;
    private readonly IMonthlyPlanRepository _monthlyPlanRepository;
    private readonly IFinancialCalculationService _calcService;
    private readonly IAppClock _clock;
    private readonly ILogger<InstallmentService> _logger;

    public InstallmentService(
        ApplicationDbContext context,
        IMonthlyPlanRepository monthlyPlanRepository,
        IFinancialCalculationService calcService,
        IAppClock clock,
        ILogger<InstallmentService> logger)
    {
        _context = context;
        _monthlyPlanRepository = monthlyPlanRepository;
        _calcService = calcService;
        _clock = clock;
        _logger = logger;
    }

    public async Task<List<InstallmentPlanListItemViewModel>> GetActivePlansAsync(Guid accountId)
    {
        var activePlans = await _context.InstallmentPlans
            .Include(p => p.Payments)
            .Where(p => p.AccountId == accountId && p.Status == InstallmentStatus.Active)
            .OrderBy(p => p.CreatedAt)
            .ToListAsync();

        var viewModels = new List<InstallmentPlanListItemViewModel>();

        foreach (var plan in activePlans)
        {
            var paidAmount = plan.Payments.Sum(p => p.Amount);
            var nextPayment = plan.Payments.Count < plan.InstallmentCount 
                ? GetTheoreticalDueDate(plan.StartDate, plan.BillingDay, plan.Payments.Count + 1)
                : (DateTime?)null;

            viewModels.Add(new InstallmentPlanListItemViewModel
            {
                Id = plan.Id,
                Description = plan.Description,
                Category = plan.Category,
                Provider = plan.Provider,
                TotalAmount = plan.TotalAmount,
                BaseInstallmentAmount = plan.BaseInstallmentAmount,
                InstallmentCount = plan.InstallmentCount,
                PaidInstallmentsCount = plan.Payments.Count,
                PendingAmount = plan.TotalAmount - paidAmount,
                NextDueDate = nextPayment,
                Status = plan.Status
            });
        }

        return viewModels;
    }

    public async Task<ResultViewModel> CreatePlanAsync(Guid accountId, CreateInstallmentPlanViewModel model)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var activeMonthlyPlan = await _monthlyPlanRepository.GetActivePlanByAccountIdAsync(accountId);
            if (activeMonthlyPlan == null) return ResultViewModel.Failure("No hay plan mensual activo.");

            // Check if valid using calcService
            var schedule = _calcService.BuildInstallmentSchedule(model.TotalAmount, model.InstallmentCount, model.BaseInstallmentAmount);
            if (schedule.Any(x => x <= 0))
            {
                return ResultViewModel.Failure("La cuota personalizada genera un último pago negativo o nulo. Ajuste el importe.");
            }

            var plan = new InstallmentPlan
            {
                AccountId = accountId,
                Description = model.Description,
                Category = model.Category,
                Provider = model.Provider,
                TotalAmount = model.TotalAmount,
                InstallmentCount = model.InstallmentCount,
                BaseInstallmentAmount = model.BaseInstallmentAmount,
                BillingDay = model.BillingDay,
                StartDate = _clock.UtcNow,
                Status = InstallmentStatus.Active,
                CreatedAt = _clock.UtcNow,
                UpdatedAt = _clock.UtcNow
            };

            await _context.InstallmentPlans.AddAsync(plan);

            if (model.FirstInstallmentAlreadyPaid)
            {
                var firstAmount = schedule[0];
                var expense = new Expense
                {
                    MonthlyPlanId = activeMonthlyPlan.Id,
                    Amount = firstAmount,
                    Category = plan.Category,
                    Description = $"{plan.Description} (1/{plan.InstallmentCount})",
                    Date = _clock.UtcNow
                };
                
                await _context.Expenses.AddAsync(expense);

                var payment = new InstallmentPayment
                {
                    InstallmentPlanId = plan.Id,
                    Expense = expense,
                    InstallmentNumber = 1,
                    Amount = firstAmount,
                    DueDate = GetTheoreticalDueDate(plan.StartDate, plan.BillingDay, 1),
                    PaidAt = _clock.UtcNow,
                    PaymentType = InstallmentPaymentType.RegularInstallment,
                    CreatedAt = _clock.UtcNow
                };
                
                await _context.InstallmentPayments.AddAsync(payment);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return ResultViewModel.Success();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error creating installment plan");
            return ResultViewModel.Failure("Error al crear el pago a plazos.");
        }
    }

    public async Task<ResultViewModel> LiquidatePlanAsync(Guid accountId, Guid planId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var plan = await _context.InstallmentPlans
                .Include(p => p.Payments)
                .FirstOrDefaultAsync(p => p.Id == planId && p.AccountId == accountId);

            if (plan == null) return ResultViewModel.Failure("Plan no encontrado.");
            if (plan.Status != InstallmentStatus.Active) return ResultViewModel.Failure("Solo se pueden liquidar planes activos.");

            var activeMonthlyPlan = await _monthlyPlanRepository.GetActivePlanByAccountIdAsync(accountId);
            if (activeMonthlyPlan == null) return ResultViewModel.Failure("No hay plan mensual activo.");

            var paidAmount = plan.Payments.Sum(p => p.Amount);
            var pendingAmount = plan.TotalAmount - paidAmount;

            if (pendingAmount <= 0) return ResultViewModel.Failure("No hay cantidad pendiente para liquidar.");

            var expense = new Expense
            {
                MonthlyPlanId = activeMonthlyPlan.Id,
                Amount = pendingAmount,
                Category = plan.Category,
                Description = $"{plan.Description} (Liquidación)",
                Date = _clock.UtcNow
            };
            
            await _context.Expenses.AddAsync(expense);

            var nextInstallmentNumber = plan.Payments.Count > 0 ? plan.Payments.Max(p => p.InstallmentNumber) + 1 : 1;

            var payment = new InstallmentPayment
            {
                InstallmentPlanId = plan.Id,
                Expense = expense,
                InstallmentNumber = nextInstallmentNumber,
                Amount = pendingAmount,
                DueDate = _clock.UtcNow,
                PaidAt = _clock.UtcNow,
                PaymentType = InstallmentPaymentType.Liquidation,
                CreatedAt = _clock.UtcNow
            };
            
            await _context.InstallmentPayments.AddAsync(payment);

            plan.Status = InstallmentStatus.Liquidated;
            plan.UpdatedAt = _clock.UtcNow;
            _context.InstallmentPlans.Update(plan);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return ResultViewModel.Success();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error liquidating installment plan");
            return ResultViewModel.Failure("Error al liquidar el pago a plazos.");
        }
    }

    private DateTime GetTheoreticalDueDate(DateTime startDate, int billingDay, int installmentNumber)
    {
        var targetMonthDate = startDate.AddMonths(installmentNumber - 1);
        int maxDays = DateTime.DaysInMonth(targetMonthDate.Year, targetMonthDate.Month);
        int actualDay = Math.Min(billingDay, maxDays);
        return new DateTime(targetMonthDate.Year, targetMonthDate.Month, actualDay, 0, 0, 0, DateTimeKind.Local);
    }
}
