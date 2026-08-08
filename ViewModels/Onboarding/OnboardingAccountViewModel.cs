using System.ComponentModel.DataAnnotations;

namespace PocketFlow.ViewModels.Onboarding;

public class OnboardingAccountViewModel
{
    [Required(ErrorMessage = "El nombre de la cuenta es obligatorio.")]
    public string AccountName { get; set; } = "Principal";

    public string Currency { get; set; } = "EUR";

    [Required(ErrorMessage = "El ingreso mensual es obligatorio.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El ingreso debe ser mayor a 0.")]
    public decimal? MonthlyIncome { get; set; }

    [Required(ErrorMessage = "El día de cobro es obligatorio.")]
    [Range(1, 31, ErrorMessage = "El día debe estar entre 1 y 31.")]
    public int Payday { get; set; }
}
