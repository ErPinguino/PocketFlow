using System.ComponentModel.DataAnnotations;

namespace PocketFlow.ViewModels.Onboarding;

public class OnboardingFixedExpensesViewModel
{
    [Required(ErrorMessage = "El valor de gastos fijos es obligatorio.")]
    [Range(0, double.MaxValue, ErrorMessage = "Los gastos fijos no pueden ser negativos.")]
    public decimal FixedExpenses { get; set; }
}
