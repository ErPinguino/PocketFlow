using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PocketFlow.Models;
using PocketFlow.Repositories;
using PocketFlow.ViewModels.PiggyBanks;
using PocketFlow.ViewModels.Shared;

namespace PocketFlow.Services;

public class PiggyBankService : IPiggyBankService
{
    private readonly IPiggyBankRepository _piggyBankRepository;
    private readonly IAccountContextService _accountContextService;
    private readonly IAppClock _clock;
    private readonly ILogger<PiggyBankService> _logger;

    public PiggyBankService(
        IPiggyBankRepository piggyBankRepository,
        IAccountContextService accountContextService,
        IAppClock clock,
        ILogger<PiggyBankService> logger)
    {
        _piggyBankRepository = piggyBankRepository;
        _accountContextService = accountContextService;
        _clock = clock;
        _logger = logger;
    }

    public async Task<PiggyBanksViewModel> GetAllAsync()
    {
        var account = await _accountContextService.GetCurrentAccountAsync();
        if (account == null) return new PiggyBanksViewModel();

        var allPiggyBanks = await _piggyBankRepository.GetByAccountIdAsync(account.Id);

        var viewModel = new PiggyBanksViewModel();

        foreach (var pb in allPiggyBanks)
        {
            var itemVM = MapToListItem(pb);
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
