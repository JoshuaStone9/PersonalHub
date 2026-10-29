namespace FinanceManagerAspNet.Navigation;

public static class LegacyManageMonthRoutes
{
    public static string GetDestination(string? source)
        => source switch
        {
            "income" => FinanceRouteNames.Month,
            "bills" => FinanceRouteNames.MonthEssentialBills,
            "everyday_spending" or "everyday" => FinanceRouteNames.MonthEverydaySpending,
            "extra_expenses" or "extras" => FinanceRouteNames.MonthExtraExpenses,
            "investments" => FinanceRouteNames.MonthInvestmentContributions,
            "savings" => FinanceRouteNames.MonthPotFunding,
            _ => FinanceRouteNames.MonthOverview
        };
}
