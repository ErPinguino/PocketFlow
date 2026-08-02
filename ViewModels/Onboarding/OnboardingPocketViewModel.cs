using System.ComponentModel.DataAnnotations;

namespace PocketFlow.ViewModels.Onboarding;

public class OnboardingPocketViewModel
{
    [Required(ErrorMessage = "El presupuesto de Vida es obligatorio.")]
    [Range(0, double.MaxValue, ErrorMessage = "No puede ser negativo.")]
    public decimal LifeBudget { get; set; }

    [Required(ErrorMessage = "El presupuesto de Caprichos es obligatorio.")]
    [Range(0, double.MaxValue, ErrorMessage = "No puede ser negativo.")]
    public decimal WhimBudget { get; set; }

    // Calculated fields to display
    public decimal MonthlyIncome { get; set; }
    public decimal FixedExpenses { get; set; }
    public decimal TotalMonthlySavings { get; set; }
    public decimal AvailableFreePocket { get; set; }
}
