using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PocketFlow.Models;
using PocketFlow.Repositories;
using PocketFlow.ViewModels.PiggyBanks;
using PocketFlow.ViewModels.Shared;
using PocketFlow.Data;
using Microsoft.EntityFrameworkCore;

namespace PocketFlow.Services;

public class PiggyBankService : IPiggyBankService
{
    private readonly IPiggyBankRepository _piggyBankRepository;
    private readonly IAccountContextService _accountContextService;
    private readonly IAppClock _clock;
    private readonly ILogger<PiggyBankService> _logger;
    private readonly ApplicationDbContext _context;
    private readonly IMonthlyPlanRepository _monthlyPlanRepository;

    public PiggyBankService(
        IPiggyBankRepository piggyBankRepository,
        IAccountContextService accountContextService,
        IAppClock clock,
        ILogger<PiggyBankService> logger,
        ApplicationDbContext context,
        IMonthlyPlanRepository monthlyPlanRepository)
    {
        _piggyBankRepository = piggyBankRepository;
        _accountContextService = accountContextService;
        _clock = clock;
        _logger = logger;
        _context = context;
        _monthlyPlanRepository = monthlyPlanRepository;
    }

    public async Task<PiggyBanksViewModel> GetAllAsync()
    {
        var account = await _accountContextService.GetCurrentAccountAsync();
        if (account == null) return new PiggyBanksViewModel();

        var allPiggyBanks = await _piggyBankRepository.GetByAccountIdAsync(account.Id);
        var activeMonthlyPlan = await _monthlyPlanRepository.GetActivePlanByAccountIdAsync(account.Id);

        var viewModel = new PiggyBanksViewModel();
        
        List<PiggyBankContribution> contributions = new();
        if (activeMonthlyPlan != null)
        {
            contributions = await _context.PiggyBankContributions
                .Where(c => c.MonthlyPlanId == activeMonthlyPlan.Id)
                .ToListAsync();
        }

        foreach (var pb in allPiggyBanks)
        {
            var itemVM = MapToListItem(pb);
            if (activeMonthlyPlan != null)
            {
                var appliedPlanned = contributions
                    .Where(c => c.PiggyBankId == pb.Id && c.Type == ContributionType.Planned)
                    .Sum(c => c.Amount);
                itemVM.PendingPlanned = Math.Max(0, pb.MonthlyContribution - appliedPlanned);
                itemVM.AvailablePocketAmount = activeMonthlyPlan.FreePocketAmount;
            }
            
            if (pb.IsActive)
            {
                viewModel.ActivePiggyBanks.Add(itemVM);
            }
            else
            {
                viewModel.ArchivedPiggyBanks.Add(itemVM);
            }
        }

        return viewModel;
    }

    public async Task<UpdatePiggyBankViewModel?> GetForEditAsync(Guid id)
    {
        var account = await _accountContextService.GetCurrentAccountAsync();
        if (account == null) return null;

        var piggyBank = await _piggyBankRepository.GetByIdAndAccountIdAsync(id, account.Id);
        if (piggyBank == null) return null;

        return new UpdatePiggyBankViewModel
        {
            Id = piggyBank.Id,
            Name = piggyBank.Name,
            Icon = piggyBank.Icon,
            TargetAmount = piggyBank.TargetAmount,
            MonthlyContribution = piggyBank.MonthlyContribution
        };
    }

    public async Task<ResultViewModel> CreateAsync(CreatePiggyBankViewModel model)
    {
        try
        {
            var account = await _accountContextService.GetCurrentAccountAsync();
            if (account == null) return ResultViewModel.Failure("No se encontró la cuenta.");

            var newPiggyBank = new PiggyBank
            {
                AccountId = account.Id,
                Name = model.Name,
                Icon = model.Icon,
                TargetAmount = model.TargetAmount,
                CurrentAmount = model.CurrentAmount,
                MonthlyContribution = model.MonthlyContribution,
                IsActive = true,
                CreatedAt = _clock.UtcNow
            };

            await _piggyBankRepository.AddAsync(newPiggyBank);
            await _piggyBankRepository.SaveChangesAsync();

            return ResultViewModel.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating PiggyBank");
            return ResultViewModel.Failure("Ocurrió un error al crear la hucha.");
        }
    }

    public async Task<ResultViewModel> UpdateAsync(UpdatePiggyBankViewModel model)
    {
        try
        {
            var account = await _accountContextService.GetCurrentAccountAsync();
            if (account == null) return ResultViewModel.Failure("No se encontró la cuenta.");

            var piggyBank = await _piggyBankRepository.GetByIdAndAccountIdAsync(model.Id, account.Id);
            if (piggyBank == null) return ResultViewModel.Failure("Hucha no encontrada.");

            piggyBank.Name = model.Name;
            piggyBank.Icon = model.Icon;
            piggyBank.TargetAmount = model.TargetAmount;
            piggyBank.MonthlyContribution = model.MonthlyContribution;
            piggyBank.UpdatedAt = _clock.UtcNow;

            _piggyBankRepository.Update(piggyBank);
            await _piggyBankRepository.SaveChangesAsync();

            return ResultViewModel.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating PiggyBank {Id}", model.Id);
            return ResultViewModel.Failure("Ocurrió un error al actualizar la hucha.");
        }
    }

    public async Task<ResultViewModel> ArchiveAsync(Guid id)
    {
        try
        {
            var account = await _accountContextService.GetCurrentAccountAsync();
            if (account == null) return ResultViewModel.Failure("No se encontró la cuenta.");

            var piggyBank = await _piggyBankRepository.GetByIdAndAccountIdAsync(id, account.Id);
            if (piggyBank == null) return ResultViewModel.Failure("Hucha no encontrada.");

            piggyBank.IsActive = false;
            piggyBank.UpdatedAt = _clock.UtcNow;

            _piggyBankRepository.Update(piggyBank);
            await _piggyBankRepository.SaveChangesAsync();

            return ResultViewModel.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error archiving PiggyBank {Id}", id);
            return ResultViewModel.Failure("Ocurrió un error al archivar la hucha.");
        }
    }

    public async Task<ResultViewModel> ReactivateAsync(Guid id)
    {
        try
        {
            var account = await _accountContextService.GetCurrentAccountAsync();
            if (account == null) return ResultViewModel.Failure("No se encontró la cuenta.");

            var piggyBank = await _piggyBankRepository.GetByIdAndAccountIdAsync(id, account.Id);
            if (piggyBank == null) return ResultViewModel.Failure("Hucha no encontrada.");

            piggyBank.IsActive = true;
            piggyBank.UpdatedAt = _clock.UtcNow;

            _piggyBankRepository.Update(piggyBank);
            await _piggyBankRepository.SaveChangesAsync();

            return ResultViewModel.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reactivating PiggyBank {Id}", id);
            return ResultViewModel.Failure("Ocurrió un error al reactivar la hucha.");
        }
    }

    public async Task<ResultViewModel> ContributePlannedAsync(Guid id, decimal amount)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var account = await _accountContextService.GetCurrentAccountAsync();
            if (account == null) return ResultViewModel.Failure("No se encontró la cuenta.");

            var piggyBank = await _piggyBankRepository.GetByIdAndAccountIdAsync(id, account.Id);
            if (piggyBank == null) return ResultViewModel.Failure("Hucha no encontrada.");

            var activeMonthlyPlan = await _monthlyPlanRepository.GetActivePlanByAccountIdAsync(account.Id);
            if (activeMonthlyPlan == null) return ResultViewModel.Failure("No hay plan mensual activo.");

            var appliedPlanned = await _context.PiggyBankContributions
                .Where(c => c.PiggyBankId == id && c.MonthlyPlanId == activeMonthlyPlan.Id && c.Type == ContributionType.Planned)
                .SumAsync(c => c.Amount);
            
            var pendingPlanned = Math.Max(0, piggyBank.MonthlyContribution - appliedPlanned);
            if (amount <= 0 || amount > pendingPlanned)
                return ResultViewModel.Failure("El importe supera el ahorro planificado pendiente.");

            piggyBank.CurrentAmount += amount;
            piggyBank.UpdatedAt = _clock.UtcNow;

            _piggyBankRepository.Update(piggyBank);

            var contribution = new PiggyBankContribution
            {
                PiggyBankId = piggyBank.Id,
                MonthlyPlanId = activeMonthlyPlan.Id,
                Amount = amount,
                Type = ContributionType.Planned,
                CreatedAt = _clock.UtcNow
            };
            await _context.PiggyBankContributions.AddAsync(contribution);
            
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return ResultViewModel.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing planned contribution to PiggyBank {Id}", id);
            return ResultViewModel.Failure("Ocurrió un error al realizar la aportación planificada.");
        }
    }

    public async Task<ResultViewModel> ContributeExtraAsync(Guid id, decimal amount)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var account = await _accountContextService.GetCurrentAccountAsync();
            if (account == null) return ResultViewModel.Failure("No se encontró la cuenta.");

            var piggyBank = await _piggyBankRepository.GetByIdAndAccountIdAsync(id, account.Id);
            if (piggyBank == null) return ResultViewModel.Failure("Hucha no encontrada.");

            var activeMonthlyPlan = await _monthlyPlanRepository.GetActivePlanByAccountIdAsync(account.Id);
            if (activeMonthlyPlan == null) return ResultViewModel.Failure("No hay plan mensual activo.");

            if (amount <= 0 || amount > activeMonthlyPlan.FreePocketAmount)
                return ResultViewModel.Failure("El importe supera el saldo disponible en el bolsillo libre.");

            piggyBank.CurrentAmount += amount;
            piggyBank.UpdatedAt = _clock.UtcNow;

            _piggyBankRepository.Update(piggyBank);

            activeMonthlyPlan.FreePocketAmount -= amount;
            _context.MonthlyPlans.Update(activeMonthlyPlan);
            
            var contribution = new PiggyBankContribution
            {
                PiggyBankId = piggyBank.Id,
                MonthlyPlanId = activeMonthlyPlan.Id,
                Amount = amount,
                Type = ContributionType.Extra,
                CreatedAt = _clock.UtcNow
            };
            await _context.PiggyBankContributions.AddAsync(contribution);
            
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return ResultViewModel.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing extra contribution to PiggyBank {Id}", id);
            return ResultViewModel.Failure("Ocurrió un error al realizar la aportación extra.");
        }
    }

    private PiggyBankListItemViewModel MapToListItem(PiggyBank pb)
    {
        decimal? remaining = pb.TargetAmount.HasValue ? Math.Max(0, pb.TargetAmount.Value - pb.CurrentAmount) : null;
        int? percentage = null;
        if (pb.TargetAmount.HasValue && pb.TargetAmount.Value > 0)
        {
            percentage = (int)Math.Round((pb.CurrentAmount / pb.TargetAmount.Value) * 100);
            if (percentage > 100) percentage = 100;
        }

        return new PiggyBankListItemViewModel
        {
            Id = pb.Id,
            Name = pb.Name,
            Icon = pb.Icon,
            CurrentAmount = pb.CurrentAmount,
            TargetAmount = pb.TargetAmount,
            RemainingAmount = remaining,
            MonthlyContribution = pb.MonthlyContribution,
            ProgressPercentage = percentage,
            IsActive = pb.IsActive,
            IsCompleted = pb.TargetAmount.HasValue && pb.CurrentAmount >= pb.TargetAmount.Value
        };
    }
}
