using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PocketFlow.ViewModels.Installments;
using PocketFlow.ViewModels.Shared;

namespace PocketFlow.Services;

public interface IInstallmentService
{
    Task<List<InstallmentPlanListItemViewModel>> GetActivePlansAsync(Guid accountId);
    Task<ResultViewModel> CreatePlanAsync(Guid accountId, CreateInstallmentPlanViewModel model);
    Task<ResultViewModel> LiquidatePlanAsync(Guid accountId, Guid planId);
}
