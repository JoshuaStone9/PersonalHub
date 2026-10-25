using Microsoft.AspNetCore.Http;

namespace FinanceManagerAspNet.Navigation;

public static class LegacyFinanceRouteRedirects
{
    private static readonly IReadOnlyDictionary<string, string> Redirects =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["/Dashboard"] = "/Home",
            ["/Dashboard/Index"] = "/Home",
            ["/MonthlyMoney/Income"] = "/Month",
            ["/MonthlyMoney/EssentialBills"] = "/Month/Essential-bills",
            ["/MonthlyMoney/EverydaySpending"] = "/Month/Everyday-spending",
            ["/MonthlyMoney/ExtraExpenses"] = "/Month/Extra-expenses",
            ["/MonthlyMoney/Investments"] = "/Month/Investment-contributions",
            ["/MonthlyMoney/MoneyPots"] = "/Month/Pot-funding",
            ["/Reconciliation"] = "/Accounts",
            ["/Reconciliation/Index"] = "/Accounts",
            ["/StatementReconciliation"] = "/Accounts/Statements",
            ["/StatementReconciliation/Index"] = "/Accounts/Statements",
            ["/Assets"] = "/Accounts/Holdings",
            ["/Assets/Index"] = "/Accounts/Holdings",
            ["/SavingPots"] = "/Pots",
            ["/SavingPots/Index"] = "/Pots",
            ["/MoneyPotActivity"] = "/Pots/Activity",
            ["/MoneyPotActivity/Index"] = "/Pots/Activity",
            ["/Forecast"] = "/Plan",
            ["/Forecast/Index"] = "/Plan",
            ["/Forecast/WhatIf"] = "/Plan/What-if",
            ["/Forecast/Scenarios"] = "/Plan/Scenarios",
            ["/Forecast/CompareScenarios"] = "/Plan/Compare",
            ["/Statistics"] = "/Insights",
            ["/Statistics/Index"] = "/Insights",
            ["/FinancialTrends"] = "/Insights/Trends",
            ["/FinancialTrends/Index"] = "/Insights/Trends",
            ["/Events"] = "/Insights/History",
            ["/Events/Index"] = "/Insights/History",
            ["/Reminders"] = "/Tasks",
            ["/Reminders/Index"] = "/Tasks",
            ["/FinancialTasks"] = "/Tasks"
        };

    public static bool TryGetDestination(PathString requestPath, out string destination)
    {
        var path = requestPath.Value?.TrimEnd('/') ?? string.Empty;
        return Redirects.TryGetValue(path, out destination!);
    }
}
