using PocketFlow.ViewModels.Dashboard;

namespace PocketFlow.Services;

public interface IDashboardService
{
    Task<DashboardViewModel?> GetDashboardAsync();
}
