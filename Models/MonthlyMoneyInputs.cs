namespace FinanceManagerAspNet.Models;

public sealed class StartingAdjustmentInput
{
    public int Year { get; init; }
    public int Month { get; init; }
    public decimal Amount { get; init; }
    public string? Reason { get; init; }
}

public sealed class MonthlyIncomeEntryInput
{
    public int Id { get; init; }
    public int Year { get; init; }
    public int Month { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public DateTime Date { get; init; }
    public string? Category { get; init; }
    public string? Notes { get; init; }
    public bool IsRecurring { get; init; }
}

public class MonthlyRecordInput
{
    public int Id { get; init; }
    public int Year { get; init; }
    public int Month { get; init; }
}

public sealed class IncomeRecurrenceInput : MonthlyRecordInput
{
    public bool IsRecurring { get; init; }
}

public sealed class RecurringIncomeSetupInput
{
    public int Year { get; init; }
    public int Month { get; init; }
    public List<int> EntryIds { get; init; } = [];
}

public sealed class MonthlyPaymentInput
{
    public int Year { get; init; }
    public int Month { get; init; }
    public string Source { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public DateTime Date { get; init; }
    public string? Category { get; init; }
    public string? Type { get; init; }
    public string? Length { get; init; }
    public string? Notes { get; init; }
    public bool IsPermanent { get; init; }
    public int? AccountBalanceId { get; init; }
}

public class MonthlyPaymentRecordInput : MonthlyRecordInput
{
    public string Source { get; init; } = string.Empty;
}

public sealed class MonthlyPaymentAmountInput : MonthlyPaymentRecordInput
{
    public decimal Amount { get; init; }
}

public sealed class MonthlyPaymentRecurrenceInput : MonthlyPaymentRecordInput
{
    public bool IsPermanent { get; init; }
}
