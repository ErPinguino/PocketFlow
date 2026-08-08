using PocketFlow.Models;

namespace PocketFlow.ViewModels.History;

public class HistoryListViewModel
{
    public List<HistoryListItemViewModel> Plans { get; set; } = new();
}

public class HistoryListItemViewModel
{
    public Guid Id { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public int Year { get; set; }
    public PlanStatus Status { get; set; }
    public bool IsActive { get; set; }
    
    // Quick summary
    public decimal TotalSpent { get; set; }
    public decimal FinalBalance { get; set; }
}
