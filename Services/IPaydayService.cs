using PocketFlow.Models;

namespace PocketFlow.Services;

public interface IPaydayService
{
    bool ShouldAskPaydayConfirmation(Account account);
}
