using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace FinanceManagerAspNet.Navigation;

public static class FinanceRoutingExtensions
{
    public static IApplicationBuilder UseLegacyFinanceRouteRedirects(this IApplicationBuilder app)
    {
        return app.Use(async (context, next) =>
        {
            if ((HttpMethods.IsGet(context.Request.Method) || HttpMethods.IsHead(context.Request.Method))
                && LegacyFinanceRouteRedirects.TryGetDestination(context.Request.Path, out var destination))
            {
                context.Response.Redirect(destination + context.Request.QueryString, permanent: false);
                return;
            }

            await next();
        });
    }

    public static IEndpointRouteBuilder MapFinanceDestinationRoutes(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapControllerRoute(FinanceRouteNames.Home, "Home", new { controller = "Dashboard", action = "Index" });

        endpoints.MapControllerRoute(FinanceRouteNames.MonthEssentialBills, "Month/Essential-bills", new { controller = "MonthlyMoney", action = "EssentialBills" });
        endpoints.MapControllerRoute(FinanceRouteNames.MonthEverydaySpending, "Month/Everyday-spending", new { controller = "MonthlyMoney", action = "EverydaySpending" });
        endpoints.MapControllerRoute(FinanceRouteNames.MonthExtraExpenses, "Month/Extra-expenses", new { controller = "MonthlyMoney", action = "ExtraExpenses" });
        endpoints.MapControllerRoute(FinanceRouteNames.MonthInvestmentContributions, "Month/Investment-contributions", new { controller = "MonthlyMoney", action = "Investments" });
        endpoints.MapControllerRoute(FinanceRouteNames.MonthPotFunding, "Month/Pot-funding", new { controller = "MonthlyMoney", action = "MoneyPots" });
        endpoints.MapControllerRoute(FinanceRouteNames.Month, "Month", new { controller = "MonthlyMoney", action = "Income" });

        endpoints.MapControllerRoute(FinanceRouteNames.AccountStatements, "Accounts/Statements/{action=Index}/{id?}", new { controller = "StatementReconciliation" });
        endpoints.MapControllerRoute(FinanceRouteNames.AccountHoldings, "Accounts/Holdings/{action=Index}/{id?}", new { controller = "Assets" });
        endpoints.MapControllerRoute(FinanceRouteNames.Accounts, "Accounts/{action=Index}/{id?}", new { controller = "Reconciliation" });

        endpoints.MapControllerRoute(FinanceRouteNames.PotActivity, "Pots/Activity", new { controller = "MoneyPotActivity", action = "Index" });
        endpoints.MapControllerRoute(FinanceRouteNames.Pots, "Pots/{action=Index}/{id?}", new { controller = "SavingPots" });

        endpoints.MapControllerRoute(FinanceRouteNames.PlanWhatIf, "Plan/What-if", new { controller = "Forecast", action = "WhatIf" });
        endpoints.MapControllerRoute(FinanceRouteNames.PlanScenarios, "Plan/Scenarios", new { controller = "Forecast", action = "Scenarios" });
        endpoints.MapControllerRoute(FinanceRouteNames.PlanCompare, "Plan/Compare", new { controller = "Forecast", action = "CompareScenarios" });
        endpoints.MapControllerRoute(FinanceRouteNames.Plan, "Plan/{action=Index}/{id?}", new { controller = "Forecast" });

        endpoints.MapControllerRoute(FinanceRouteNames.InsightTrends, "Insights/Trends", new { controller = "FinancialTrends", action = "Index" });
        endpoints.MapControllerRoute(FinanceRouteNames.InsightHistory, "Insights/History/{action=Index}/{id?}", new { controller = "Events" });
        endpoints.MapControllerRoute(FinanceRouteNames.Insights, "Insights/{action=Index}/{id?}", new { controller = "Statistics" });

        return endpoints;
    }
}
