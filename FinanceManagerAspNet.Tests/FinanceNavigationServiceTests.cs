using FinanceManagerAspNet.Navigation;
using FinanceManagerAspNet.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FinanceManagerAspNet.Tests;

public sealed class FinanceNavigationServiceTests
{
    private readonly FinanceNavigationService _navigation = new();

    [Fact]
    public void PrimaryDestinations_AreTheSixApprovedDestinationsInOrder()
    {
        Assert.Equal(
            ["Home", "Month", "Accounts", "Pots", "Plan", "Insights"],
            _navigation.PrimaryDestinations.Select(x => x.Label));
    }

    [Theory]
    [InlineData("Dashboard", "Index", false, "home")]
    [InlineData("Dashboard", "Index", true, "month")]
    [InlineData("MonthlyMoney", "Investments", false, "month")]
    [InlineData("Reconciliation", "Index", false, "accounts")]
    [InlineData("StatementReconciliation", "Workspace", false, "accounts")]
    [InlineData("Assets", "Index", false, "accounts")]
    [InlineData("SavingPots", "Index", false, "pots")]
    [InlineData("MoneyPotActivity", "Index", false, "pots")]
    [InlineData("Forecast", "WhatIf", false, "plan")]
    [InlineData("Statistics", "Index", false, "insights")]
    [InlineData("FinancialTrends", "Index", false, "insights")]
    [InlineData("Events", "Index", false, "insights")]
    public void GetDestinationKey_MapsExistingPagesToTheirOwner(
        string controller,
        string action,
        bool isManageMonth,
        string expected)
    {
        Assert.Equal(expected, _navigation.GetDestinationKey(controller, action, isManageMonth));
    }

    [Fact]
    public void MonthTabs_UseApprovedTerminologyAndPreserveSelectedMonth()
    {
        var navigation = _navigation.BuildHubNavigation(
            "MonthlyMoney",
            "Investments",
            "2026",
            "10");

        Assert.NotNull(navigation);
        Assert.Contains(navigation.Tabs, x => x.Label == "Investment contributions" && x.IsActive);
        Assert.Contains(navigation.Tabs, x => x.Label == "Pot funding");
        Assert.All(navigation.Tabs, tab =>
        {
            Assert.Equal("2026", tab.RouteValues?["year"]);
            Assert.Equal("10", tab.RouteValues?["month"]);
        });
    }

    [Fact]
    public void SpecialistWorkspace_KeepsOwningHubTabActive()
    {
        var navigation = _navigation.BuildHubNavigation(
            "StatementReconciliation",
            "Workspace",
            null,
            null);

        var active = Assert.Single(navigation!.Tabs, tab => tab.IsActive);
        Assert.Equal("Statements", active.Label);
    }

    [Fact]
    public void MonthOverview_IsTheActiveTabForManagedDashboardRoute()
    {
        var navigation = _navigation.BuildHubNavigation(
            "Dashboard",
            "Index",
            "2026",
            "10",
            isManageMonth: true);

        var active = Assert.Single(navigation!.Tabs, tab => tab.IsActive);
        Assert.Equal("Overview", active.Label);
        Assert.Equal("2026", active.RouteValues!["year"]);
        Assert.Equal("10", active.RouteValues["month"]);
    }

    [Theory]
    [InlineData("/Dashboard/Index", "/Home")]
    [InlineData("/MonthlyMoney/Investments", "/Month/Investment-contributions")]
    [InlineData("/StatementReconciliation", "/Accounts/Statements")]
    [InlineData("/Assets/Index", "/Accounts/Holdings")]
    [InlineData("/MoneyPotActivity", "/Pots/Activity")]
    [InlineData("/Forecast/WhatIf", "/Plan/What-if")]
    [InlineData("/FinancialTrends", "/Insights/Trends")]
    [InlineData("/Events/Index", "/Insights/History")]
    [InlineData("/Reminders", "/Tasks")]
    public void LegacyRouteRedirects_MapRetiredEntryPoints(string oldPath, string expected)
    {
        Assert.True(LegacyFinanceRouteRedirects.TryGetDestination(new PathString(oldPath), out var destination));
        Assert.Equal(expected, destination);
    }

    [Fact]
    public void LegacyRouteRedirects_LeaveSpecialistActionsAvailable()
    {
        Assert.False(LegacyFinanceRouteRedirects.TryGetDestination(
            new PathString("/StatementReconciliation/Workspace/42"),
            out _));
    }

    [Fact]
    public void TasksRoute_HasOneCanonicalNamedEndpoint()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services
            .AddControllersWithViews()
            .AddApplicationPart(typeof(RemindersController).Assembly);

        using var provider = services.BuildServiceProvider();
        var actions = provider
            .GetRequiredService<IActionDescriptorCollectionProvider>()
            .ActionDescriptors.Items
            .OfType<ControllerActionDescriptor>();

        var route = Assert.Single(actions, action => action.AttributeRouteInfo?.Name == FinanceRouteNames.Tasks);
        Assert.Equal("Tasks", route.AttributeRouteInfo!.Template);
    }
}
