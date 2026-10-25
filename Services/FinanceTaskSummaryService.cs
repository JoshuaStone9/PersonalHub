namespace FinanceManagerAspNet.Services;

public interface IFinanceTaskSummaryService
{
    Task<int> GetDueCountAsync();
}

public sealed class FinanceTaskSummaryService(FinanceRepository repository) : IFinanceTaskSummaryService
{
    public Task<int> GetDueCountAsync() => repository.GetDueReminderCountAsync();
}
