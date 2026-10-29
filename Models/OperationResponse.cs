namespace FinanceManagerAspNet.Models;

public sealed record OperationResponse<T>(
    bool Success,
    string Message,
    string? RedirectUrl,
    T? Data);
