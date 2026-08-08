using System;
using System.Threading.Tasks;
using PocketFlow.ViewModels.PiggyBanks;
using PocketFlow.ViewModels.Shared;

namespace PocketFlow.Services;

public interface IPiggyBankService
{
    Task<PiggyBanksViewModel> GetAllAsync();
    Task<ResultViewModel> CreateAsync(CreatePiggyBankViewModel model);
    Task<ResultViewModel> UpdateAsync(UpdatePiggyBankViewModel model);
    Task<ResultViewModel> ArchiveAsync(Guid id);
    Task<ResultViewModel> ReactivateAsync(Guid id);
    Task<UpdatePiggyBankViewModel?> GetForEditAsync(Guid id);
    Task<ResultViewModel> ContributePlannedAsync(Guid id, decimal amount);
    Task<ResultViewModel> ContributeExtraAsync(Guid id, decimal amount);
}
