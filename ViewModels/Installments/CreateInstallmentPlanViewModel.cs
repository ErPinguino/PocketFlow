using System;
using System.ComponentModel.DataAnnotations;
using PocketFlow.Models;

namespace PocketFlow.ViewModels.Installments;

public class CreateInstallmentPlanViewModel
{
    [Required(ErrorMessage = "La descripción es obligatoria.")]
    [StringLength(200, ErrorMessage = "La descripción no puede exceder los 200 caracteres.")]
    public string Description { get; set; } = string.Empty;

    public ExpenseCategory Category { get; set; }

    [StringLength(100, ErrorMessage = "El proveedor no puede exceder los 100 caracteres.")]
    public string? Provider { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "El importe total debe ser mayor a 0.")]
    public decimal TotalAmount { get; set; }

    [Required]
    [Range(2, 120, ErrorMessage = "El número de cuotas debe ser al menos 2.")]
    public int InstallmentCount { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "La cuota mensual debe ser mayor a 0.")]
    public decimal BaseInstallmentAmount { get; set; }

    [Required]
    [Range(1, 31, ErrorMessage = "El día de cobro debe estar entre 1 y 31.")]
    public int BillingDay { get; set; }

    public bool FirstInstallmentAlreadyPaid { get; set; }
}
