using System.ComponentModel.DataAnnotations;
using PocketFlow.Models;

namespace PocketFlow.ViewModels.Expenses;

public class CreateExpenseViewModel
{
    [Required(ErrorMessage = "El importe es obligatorio.")]
    [Range(0.01, 999999999.99, ErrorMessage = "El importe debe ser mayor que cero.")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "La categoría es obligatoria.")]
    public ExpenseCategory Category { get; set; }

    [MaxLength(500, ErrorMessage = "La descripción no puede superar los 500 caracteres.")]
    public string? Description { get; set; }
}
