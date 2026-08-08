using System.Collections.Generic;

namespace PocketFlow.ViewModels.PiggyBanks;

public class PiggyBanksViewModel
{
    public List<PiggyBankListItemViewModel> ActivePiggyBanks { get; set; } = new();
    public List<PiggyBankListItemViewModel> ArchivedPiggyBanks { get; set; } = new();
}
