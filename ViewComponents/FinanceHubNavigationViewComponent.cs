using FinanceManagerAspNet.Navigation;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManagerAspNet.ViewComponents;

public sealed class FinanceHubNavigationViewComponent(IFinanceNavigationService navigation) : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        var controller = RouteData.Values["controller"]?.ToString();
        var action = RouteData.Values["action"]?.ToString();
        var isManageMonth = string.Equals(controller, "Dashboard", StringComparison.OrdinalIgnoreCase)
            && string.Equals(action, "Index", StringComparison.OrdinalIgnoreCase)
            && (string.Equals(Request.Query["manage"], "true", StringComparison.OrdinalIgnoreCase)
                || string.Equals(RouteData.Values["monthOverview"]?.ToString(), "true", StringComparison.OrdinalIgnoreCase));

        var model = navigation.BuildHubNavigation(
            controller,
            action,
            Request.Query["year"].FirstOrDefault() ?? RouteData.Values["year"]?.ToString(),
            Request.Query["month"].FirstOrDefault() ?? RouteData.Values["month"]?.ToString(),
            isManageMonth);

        return model is null ? Content(string.Empty) : View(model);
    }
}
