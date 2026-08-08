using System.ComponentModel.DataAnnotations;

namespace PocketFlow.ViewModels.PiggyBanks;

public class CreatePiggyBankViewModel
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
    public string Name { get; set; } = string.Empty;

    public string? Icon { get; set; }

    [Required(ErrorMessage = "El objetivo es obligatorio")]
    [Range(0.01, (double)decimal.MaxValue, ErrorMessage = "El objetivo debe ser mayor a 0")]
    public decimal TargetAmount { get; set; }

    [Range(0, (double)decimal.MaxValue, ErrorMessage = "La cantidad inicial no puede ser negativa")]
    public decimal CurrentAmount { get; set; }

    [Range(0, (double)decimal.MaxValue, ErrorMessage = "La aportación no puede ser negativa")]
    public decimal MonthlyContribution { get; set; }
}
