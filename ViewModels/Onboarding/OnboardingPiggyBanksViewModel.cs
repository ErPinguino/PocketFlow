using System.ComponentModel.DataAnnotations;

namespace PocketFlow.ViewModels.Onboarding;

public class OnboardingPiggyBankItemViewModel
{
    public string TemporaryId { get; set; } = Guid.NewGuid().ToString();

    [Required(ErrorMessage = "El nombre de la hucha es obligatorio.")]
    public string Name { get; set; } = string.Empty;

    [Range(0, double.MaxValue, ErrorMessage = "El importe actual no puede ser negativo.")]
    public decimal? CurrentAmount { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "El objetivo debe ser mayor a 0.")]
    public decimal? TargetAmount { get; set; }

    [Required(ErrorMessage = "La aportación mensual es obligatoria.")]
    [Range(0, double.MaxValue, ErrorMessage = "La aportación mensual no puede ser negativa.")]
    public decimal? MonthlyContribution { get; set; }

    public string? Icon { get; set; }
}

public class OnboardingPiggyBanksViewModel
{
    public List<OnboardingPiggyBankItemViewModel> PiggyBanks { get; set; } = new();
}
