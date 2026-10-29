using FinanceManagerAspNet.Models;
using FinanceManagerAspNet.Services;
using Xunit;

namespace FinanceManagerAspNet.Tests;

public sealed class MonthlyMoneyRequestPolicyTests
{
    private readonly MonthlyMoneyRequestPolicy _policy = new();

    [Theory]
    [InlineData(2026, 1)]
    [InlineData(2026, 12)]
    public void ValidateMonth_AcceptsCalendarMonths(int year, int month)
        => Assert.True(_policy.ValidateMonth(year, month).IsValid);

    [Theory]
    [InlineData(2026, 0)]
    [InlineData(2026, 13)]
    [InlineData(0, 1)]
    public void ValidateMonth_RejectsInvalidSelections(int year, int month)
        => Assert.False(_policy.ValidateMonth(year, month).IsValid);

    [Fact]
    public void ValidateIncome_RejectsMissingNameAndNegativeAmount()
    {
        Assert.False(_policy.ValidateIncome(new MonthlyIncomeEntryInput
        {
            Year = 2026,
            Month = 10,
            Amount = 100m
        }).IsValid);

        Assert.False(_policy.ValidateIncome(new MonthlyIncomeEntryInput
        {
            Year = 2026,
            Month = 10,
            Name = "Salary",
            Amount = -1m
        }).IsValid);
    }

    [Fact]
    public void ResolveEntryDate_PreservesSubmittedDate()
    {
        var submitted = new DateTime(2026, 10, 15);
        Assert.Equal(submitted, _policy.ResolveEntryDate(2026, 10, submitted));
    }
}
