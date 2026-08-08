using System;
using System.ComponentModel.DataAnnotations;

namespace PocketFlow.ViewModels.Onboarding;

public class OnboardingPocketViewModel
{
    [Required(ErrorMessage = "El presupuesto de Vida es obligatorio.")]
    [Range(0, double.MaxValue, ErrorMessage = "No puede ser negativo.")]
    public decimal? LifeBudget { get; set; }

    [Required(ErrorMessage = "El presupuesto de Caprichos es obligatorio.")]
    [Range(0, double.MaxValue, ErrorMessage = "No puede ser negativo.")]
    public decimal? WhimBudget { get; set; }

    // Calculated fields to display
    public decimal? MonthlyIncome { get; set; }
    public decimal? FixedExpenses { get; set; }
    public decimal? TotalMonthlySavings { get; set; }
    public decimal? AvailableFreePocket { get; set; }

    // Derived semantic properties for presentation
    public bool HasDeficit => AvailableFreePocket < 0;
    public decimal DeficitAmount => HasDeficit ? Math.Abs(AvailableFreePocket ?? 0m) : 0m;
    public bool IsZero => AvailableFreePocket == 0;
    public bool HasSurplus => AvailableFreePocket > 0;
    
    // UI state
    public string RepartoMode { get; set; } = "Recommended";
}
