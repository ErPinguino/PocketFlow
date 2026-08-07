using PocketFlow.Models;

namespace PocketFlow.ViewModels.Pocket;

public class ExpenseListItemViewModel
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public ExpenseCategory Category { get; set; }
    public string CategoryDisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAtLocal { get; set; }
    public string DateDisplay { get; set; } = string.Empty;
    public string IconClass { get; set; } = string.Empty;
}
