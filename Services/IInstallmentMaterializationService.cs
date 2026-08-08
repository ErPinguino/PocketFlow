using System;
using System.Threading.Tasks;

namespace PocketFlow.Services;

public interface IInstallmentMaterializationService
{
    Task MaterializePendingInstallmentsAsync(Guid accountId);
}
