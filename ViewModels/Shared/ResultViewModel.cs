namespace PocketFlow.ViewModels.Shared;

public class ResultViewModel
{
    public bool Succeeded { get; private set; }
    public string? ErrorMessage { get; private set; }

    public static ResultViewModel Success() => new ResultViewModel { Succeeded = true };
    public static ResultViewModel Failure(string error) => new ResultViewModel { Succeeded = false, ErrorMessage = error };
}
