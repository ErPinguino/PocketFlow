using PocketFlow.Models;
using PocketFlow.ViewModels.MonthlyTransition;

namespace PocketFlow.Services;

public interface IMonthlyTransitionService
{
    Task<decimal> CalculateLeftoverAsync(Guid accountId);

    Task<MonthlyTransitionSummaryViewModel> PrepareTransitionSummaryAsync(
        Guid accountId, 
        RolloverDecisionViewModel rolloverDecision, 
        MonthlyPlanReviewViewModel planReview);

    Task<bool> ExecuteTransitionAsync(
        Guid accountId, 
        RolloverDecisionViewModel rolloverDecision, 
        MonthlyPlanReviewViewModel planReview);
}
