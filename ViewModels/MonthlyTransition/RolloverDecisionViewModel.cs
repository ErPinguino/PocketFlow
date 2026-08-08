using System.ComponentModel.DataAnnotations;
using PocketFlow.Models;

namespace PocketFlow.ViewModels.MonthlyTransition;

public class RolloverDecisionViewModel
{
    public decimal LeftoverAmount { get; set; }
    
    [Required(ErrorMessage = "Debes elegir un destino para el sobrante.")]
    public RolloverDestinationType DestinationType { get; set; }
    
    public Guid? DestinationPiggyBankId { get; set; }
    
    public List<PiggyBankSelectionItem> AvailablePiggyBanks { get; set; } = new();
}

public class PiggyBankSelectionItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Icon { get; set; }
}
