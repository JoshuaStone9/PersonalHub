using FinanceManagerAspNet.Navigation;
using FinanceManagerAspNet.Services;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManagerAspNet.ViewComponents;

public sealed class FinanceSidebarViewComponent(
    IFinanceNavigationService navigation,
    IFinanceTaskSummaryService taskSummary) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        var controller = RouteData.Values["controller"]?.ToString();
        var action = RouteData.Values["action"]?.ToString();
        var isManageMonth = string.Equals(controller, "Dashboard", StringComparison.OrdinalIgnoreCase)
            && string.Equals(action, "Index", StringComparison.OrdinalIgnoreCase)
            && string.Equals(Request.Query["manage"], "true", StringComparison.OrdinalIgnoreCase);

        var model = new FinanceSidebarViewModel(
            navigation.PrimaryDestinations,
            navigation.GetDestinationKey(controller, action, isManageMonth),
            string.Equals(controller, "Reminders", StringComparison.OrdinalIgnoreCase),
            string.Equals(controller, "Settings", StringComparison.OrdinalIgnoreCase),
            User.Identity?.IsAuthenticated == true,
            await taskSummary.GetDueCountAsync());

        return View(model);
    }
}
