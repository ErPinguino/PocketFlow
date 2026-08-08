using PocketFlow.ViewModels.Dashboard;
using PocketFlow.ViewModels.History;

namespace PocketFlow.Services;

public interface IMonthlyHistoryService
{
    Task<HistoryListViewModel?> GetHistoryListAsync();
    Task<DashboardViewModel?> GetHistoricalDashboardAsync(Guid planId);
}
