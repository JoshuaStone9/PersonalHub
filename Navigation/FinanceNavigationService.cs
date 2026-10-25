namespace FinanceManagerAspNet.Navigation;

public sealed record FinanceDestination(
    string Key,
    string Label,
    string Icon,
    string RouteName);

public sealed record FinanceHubTab(
    string Label,
    string RouteName,
    Dictionary<string, string>? RouteValues,
    bool IsActive);

public sealed record FinanceHubNavigation(
    string DestinationLabel,
    IReadOnlyList<FinanceHubTab> Tabs);

public interface IFinanceNavigationService
{
    IReadOnlyList<FinanceDestination> PrimaryDestinations { get; }

    string? GetDestinationKey(string? controller, string? action, bool isManageMonth = false);

    FinanceHubNavigation? BuildHubNavigation(
        string? controller,
        string? action,
        string? year,
        string? month,
        bool isManageMonth = false);
}

public sealed class FinanceNavigationService : IFinanceNavigationService
{
    public IReadOnlyList<FinanceDestination> PrimaryDestinations { get; } =
    [
        new("home", "Home", "house", FinanceRouteNames.Home),
        new("month", "Month", "calendar-range", FinanceRouteNames.Month),
        new("accounts", "Accounts", "landmark", FinanceRouteNames.Accounts),
        new("pots", "Pots", "piggy-bank", FinanceRouteNames.Pots),
        new("plan", "Plan", "route", FinanceRouteNames.Plan),
        new("insights", "Insights", "chart-no-axes-combined", FinanceRouteNames.Insights)
    ];

    public string? GetDestinationKey(string? controller, string? action, bool isManageMonth = false)
    {
        if (string.Equals(controller, "Dashboard", StringComparison.OrdinalIgnoreCase))
            return isManageMonth || !string.Equals(action, "Index", StringComparison.OrdinalIgnoreCase) ? "month" : "home";

        if (IsController(controller, "MonthlyMoney")) return "month";
        if (IsController(controller, "Reconciliation", "StatementReconciliation", "Assets")) return "accounts";
        if (IsController(controller, "SavingPots", "MoneyPotActivity")) return "pots";
        if (IsController(controller, "Forecast")) return "plan";
        if (IsController(controller, "Statistics", "FinancialTrends", "Events")) return "insights";

        return null;
    }

    public FinanceHubNavigation? BuildHubNavigation(
        string? controller,
        string? action,
        string? year,
        string? month,
        bool isManageMonth = false)
    {
        var destination = GetDestinationKey(controller, action, isManageMonth);
        var monthValues = BuildMonthRouteValues(year, month);

        return destination switch
        {
            "month" => new("Month",
            [
                Tab("Income", FinanceRouteNames.Month, monthValues, controller, action, "MonthlyMoney", "Income"),
                Tab("Essential bills", FinanceRouteNames.MonthEssentialBills, monthValues, controller, action, "MonthlyMoney", "EssentialBills"),
                Tab("Everyday spending", FinanceRouteNames.MonthEverydaySpending, monthValues, controller, action, "MonthlyMoney", "EverydaySpending"),
                Tab("Extra expenses", FinanceRouteNames.MonthExtraExpenses, monthValues, controller, action, "MonthlyMoney", "ExtraExpenses"),
                Tab("Investment contributions", FinanceRouteNames.MonthInvestmentContributions, monthValues, controller, action, "MonthlyMoney", "Investments"),
                Tab("Pot funding", FinanceRouteNames.MonthPotFunding, monthValues, controller, action, "MonthlyMoney", "MoneyPots")
            ]),
            "accounts" => new("Accounts",
            [
                Tab("Balances", FinanceRouteNames.Accounts, null, controller, action, "Reconciliation"),
                Tab("Statements", FinanceRouteNames.AccountStatements, null, controller, action, "StatementReconciliation"),
                Tab("Holdings", FinanceRouteNames.AccountHoldings, null, controller, action, "Assets")
            ]),
            "pots" => new("Pots",
            [
                Tab("Pot goals", FinanceRouteNames.Pots, null, controller, action, "SavingPots"),
                Tab("Pot funding", FinanceRouteNames.MonthPotFunding, monthValues, controller, action, null),
                Tab("Activity", FinanceRouteNames.PotActivity, monthValues, controller, action, "MoneyPotActivity")
            ]),
            "plan" => new("Plan",
            [
                Tab("Forecast", FinanceRouteNames.Plan, null, controller, action, "Forecast", "Index"),
                Tab("What-if", FinanceRouteNames.PlanWhatIf, null, controller, action, "Forecast", "WhatIf"),
                Tab("Scenarios", FinanceRouteNames.PlanScenarios, null, controller, action, "Forecast", "Scenarios", "CompareScenarios")
            ]),
            "insights" => new("Insights",
            [
                Tab("Snapshot", FinanceRouteNames.Insights, null, controller, action, "Statistics"),
                Tab("Trends", FinanceRouteNames.InsightTrends, null, controller, action, "FinancialTrends"),
                Tab("History", FinanceRouteNames.InsightHistory, null, controller, action, "Events")
            ]),
            _ => null
        };
    }

    private static FinanceHubTab Tab(
        string label,
        string routeName,
        Dictionary<string, string>? routeValues,
        string? controller,
        string? action,
        string? activeController,
        params string[] activeActions)
    {
        var controllerMatches = activeController is not null
            && string.Equals(controller, activeController, StringComparison.OrdinalIgnoreCase);
        var actionMatches = activeActions.Length == 0
            || activeActions.Any(candidate => string.Equals(action, candidate, StringComparison.OrdinalIgnoreCase));

        return new(label, routeName, routeValues, controllerMatches && actionMatches);
    }

    private static bool IsController(string? controller, params string[] candidates)
        => candidates.Any(candidate => string.Equals(controller, candidate, StringComparison.OrdinalIgnoreCase));

    private static Dictionary<string, string>? BuildMonthRouteValues(string? year, string? month)
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (!string.IsNullOrWhiteSpace(year)) values["year"] = year;
        if (!string.IsNullOrWhiteSpace(month)) values["month"] = month;
        return values.Count == 0 ? null : values;
    }
}
