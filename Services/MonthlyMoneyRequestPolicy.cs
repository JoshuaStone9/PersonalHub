using FinanceManagerAspNet.Models;

namespace FinanceManagerAspNet.Services;

public sealed record MonthlyRequestValidation(bool IsValid, string? Error = null);

public interface IMonthlyMoneyRequestPolicy
{
    MonthlyRequestValidation ValidateMonth(int year, int month);
    MonthlyRequestValidation ValidateIncome(MonthlyIncomeEntryInput input);
    DateTime ResolveEntryDate(int year, int month, DateTime submittedDate);
}

public sealed class MonthlyMoneyRequestPolicy : IMonthlyMoneyRequestPolicy
{
    public MonthlyRequestValidation ValidateMonth(int year, int month)
        => year is < 1 or > 9998 || month is < 1 or > 12
            ? new(false, "Enter a valid month and year.")
            : new(true);

    public MonthlyRequestValidation ValidateIncome(MonthlyIncomeEntryInput input)
    {
        var month = ValidateMonth(input.Year, input.Month);
        if (!month.IsValid) return month;
        if (string.IsNullOrWhiteSpace(input.Name)) return new(false, "Enter an income source.");
        if (input.Amount < 0m) return new(false, "Income amount cannot be negative.");
        return new(true);
    }

    public DateTime ResolveEntryDate(int year, int month, DateTime submittedDate)
        => submittedDate == default
            ? new DateTime(year, month, Math.Min(DateTime.Today.Day, DateTime.DaysInMonth(year, month)))
            : submittedDate;
}
