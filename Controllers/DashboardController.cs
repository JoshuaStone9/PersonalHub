using FinanceManagerAspNet.Models;
using FinanceManagerAspNet.Services;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManagerAspNet.Controllers;

public sealed class DashboardController(
    FinanceRepository repo,
    IDashboardSummaryService dashboardSummaryService,
    IDashboardExperienceService dashboardExperienceService,
    IMonthlyFinancialHealthService monthlyFinancialHealthService,
    IReserveAccountSelectionService reserveAccountSelectionService,
    IFinancialForecastService financialForecastService,
    IWhatIfForecastService whatIfForecastService,
    IForecastScenarioService forecastScenarioService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(int? year, int? month, bool manage = false, string? source = null, bool monthOverview = false)
    {
        var now = DateTime.Today;
        var y = year ?? now.Year;
        var m = month ?? now.Month;

        if (manage)
            return RedirectLegacyManageMonth(y, m, source);

        ViewBag.IsMonthOverview = monthOverview;
        await repo.EnsureModernTablesAsync();

        var incomeEntriesTotal = await repo.GetMonthlyIncomeEntriesTotalAsync(y, m);
        var monthlyIncome = incomeEntriesTotal ?? 0m;
        var passiveRecords = await repo.GetPassiveIncomeRecordsAsync(y, m);
        var passiveIncome = passiveRecords.Sum(x => x.ActualAmount);
        var passiveIncomeIncludedInMonthlyIncome = passiveRecords.Where(x => x.IncomeEntryId.HasValue).Sum(x => x.ActualAmount);
        var operatingIncome = Math.Max(0m, monthlyIncome - passiveIncomeIncludedInMonthlyIncome);
        var carryForwardInfo = await repo.GetCarryForwardInfoAsync(y, m);
        var carryForward = carryForwardInfo.EffectiveAmount;

        var bills = await repo.GetRowsAsync("bills", m, y);
        var everyday = await repo.GetRowsAsync("everyday_spending", m, y);
        var extras = await repo.GetRowsAsync("extra_expenses", m, y);
        var investments = await repo.GetRowsAsync("investments", m, y);
        var reserveAllocations = await repo.GetRowsAsync("savings", m, y);
        var reserve = await repo.GetHouseholdReserveAsync();
        var reservePots = await repo.GetReservePotsAsync();

        var vm = new DashboardViewModel
        {
            Year = y, Month = m, MonthlyIncome = monthlyIncome, OperatingIncome = operatingIncome, PassiveIncome = passiveIncome, CarryForwardAmount = carryForward,
            CarryForwardCalculated = carryForwardInfo.CalculatedAmount, CarryForwardOverride = carryForwardInfo.OverrideAmount,
            CarryForwardOverrideReason = carryForwardInfo.OverrideReason, HasCarryForwardRecord = carryForwardInfo.Exists, SickDays = 0,
            Bills = bills, Expenses = everyday, ExtraExpenses = extras, Investments = investments, Savings = reserveAllocations,
            BillsTotal = bills.Sum(x => x.Amount), ExpensesTotal = everyday.Sum(x => x.Amount), ExtraExpensesTotal = extras.Sum(x => x.Amount),
            InvestmentsTotal = investments.Sum(x => x.Amount), SavingsTotal = reserveAllocations.Sum(x => x.Amount),
            TotalGoalBalance = reserve.Balance, Accounts = [],
            LastModified = [new LastModifiedInfo("Household reserve", reserve.UpdatedAt)]
        };

        ViewBag.ReserveProvider = reserve.Provider;
        ViewBag.ReserveInterestRate = reserve.InterestRate;
        var reserveAllocated = reservePots.Where(p => p.IsActive).Sum(p => p.AllocatedAmount);
        ViewBag.ReserveAllocated = reserveAllocated;
        ViewBag.ReserveUnallocated = reserve.Balance - reserveAllocated;
        vm.Intelligence = await dashboardSummaryService.BuildAsync(reserve, reservePots);

        var accountSummary = await reserveAccountSelectionService.BuildSummaryAsync();
        var preferredScenario = await forecastScenarioService.GetPreferredAsync();
        FinancialForecastResult twelveMonthForecast;
        if (preferredScenario is not null)
        {
            var preferredInput = preferredScenario.ToInput();
            preferredInput.Months = 12;
            twelveMonthForecast = whatIfForecastService.Build(preferredInput, accountSummary, reservePots).ScenarioPlan;
        }
        else
        {
            var investmentStages = await repo.GetReservePotInvestmentStagesAsync();
            twelveMonthForecast = financialForecastService.Build(new FinancialForecastRequest
            {
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddMonths(12),
                ProtectedReserveBaseline = accountSummary.Baseline,
                Accounts = accountSummary.SelectedAccounts,
                Pots = reservePots,
                InvestmentStages = investmentStages
            });
        }

        vm.ForecastSummary = new DashboardForecastSummary
        {
            ProjectedReserveBalance = twelveMonthForecast.ProjectedReserveBalance,
            ProjectedInterest = twelveMonthForecast.ProjectedInterest,
            GoalsAtRisk = twelveMonthForecast.AtRiskPotCount,
            GoalsExpectedToComplete = twelveMonthForecast.CompletedPotCount,
            ScenarioLabel = preferredScenario?.Name ?? "Live plan",
            PreferredScenarioId = preferredScenario?.Id
        };

        vm.Experience = dashboardExperienceService.Build(vm);

        var smartDashboardTasks = vm.Experience.UpcomingActions.ToList();
        var dashboardAccounts = await repo.GetAccountsAsync(reserve.Balance);
        smartDashboardTasks.AddRange(dashboardAccounts
            .Where(x => x.Amount > 0m && x.UpdatedAt.Date <= DateTime.Today.AddDays(-90))
            .Select(x => new DashboardActionItem(
                $"Check {x.Name} balance",
                $"Last updated {x.UpdatedAt:dd MMM yyyy}",
                "~/Accounts",
                x.UpdatedAt.Date <= DateTime.Today.AddDays(-180) ? "danger" : "warning",
                DateTime.Today)));

        var reconciliation = await repo.GetAccountReconciliationAsync();
        if (reconciliation.AccountsNeedingInterestReconciliation > 0)
        {
            smartDashboardTasks.Add(new DashboardActionItem(
                "Apply pending account interest",
                $"{reconciliation.TotalPendingInterest:C} across {reconciliation.AccountsNeedingInterestReconciliation} account{(reconciliation.AccountsNeedingInterestReconciliation == 1 ? string.Empty : "s")}",
                "~/Accounts",
                "warning",
                DateTime.Today));
        }

        var expectedPassiveIncome = await repo.GetPassiveIncomeEstimatesAsync(y, m);
        smartDashboardTasks.AddRange(expectedPassiveIncome
            .Where(x => !x.IsReceived && x.EstimatedAmount > 0m)
            .Select(x => new DashboardActionItem(
                $"Confirm {x.SourceName} interest",
                $"Approximately {x.EstimatedAmount:C} expected this month",
                $"~/Month?year={y}&month={m}",
                "warning",
                new DateTime(y, m, DateTime.DaysInMonth(y, m)))));

        vm.Experience.UpcomingActions = smartDashboardTasks
            .GroupBy(x => new { x.Title, x.Url })
            .Select(x => x.First())
            .OrderBy(x => x.Severity == "danger" ? 0 : x.Severity == "warning" ? 1 : 2)
            .ThenBy(x => x.DueDate ?? DateTime.MaxValue)
            .Take(6)
            .ToList();

        var previousMonth = new DateTime(y, m, 1).AddMonths(-1);
        var previousIncomeEntriesTotal = await repo.GetMonthlyIncomeEntriesTotalAsync(previousMonth.Year, previousMonth.Month);
        var previousIncome = previousIncomeEntriesTotal ?? 0m;
        var previousCarryForward = (await repo.GetCarryForwardInfoAsync(previousMonth.Year, previousMonth.Month)).EffectiveAmount;
        var previousBills = await repo.GetRowsAsync("bills", previousMonth.Month, previousMonth.Year);
        var previousEveryday = await repo.GetRowsAsync("everyday_spending", previousMonth.Month, previousMonth.Year);
        var previousExtras = await repo.GetRowsAsync("extra_expenses", previousMonth.Month, previousMonth.Year);
        var previousInvestments = await repo.GetRowsAsync("investments", previousMonth.Month, previousMonth.Year);
        var previousMoneyPots = await repo.GetRowsAsync("savings", previousMonth.Month, previousMonth.Year);

        var hasPreviousMonthData = previousIncomeEntriesTotal.HasValue
            || previousBills.Count > 0
            || previousEveryday.Count > 0
            || previousExtras.Count > 0
            || previousInvestments.Count > 0
            || previousMoneyPots.Count > 0;

        vm.FinancialHealth = monthlyFinancialHealthService.Build(
            new MonthlyFinancialSnapshot(
                monthlyIncome,
                carryForward,
                vm.BillsTotal,
                vm.ExpensesTotal,
                vm.ExtraExpensesTotal,
                vm.InvestmentsTotal,
                vm.SavingsTotal),
            hasPreviousMonthData
                ? new MonthlyFinancialSnapshot(
                    previousIncome,
                    previousCarryForward,
                    previousBills.Sum(x => x.Amount),
                    previousEveryday.Sum(x => x.Amount),
                    previousExtras.Sum(x => x.Amount),
                    previousInvestments.Sum(x => x.Amount),
                    previousMoneyPots.Sum(x => x.Amount))
                : null);

        return View("Index", vm);
    }

    [HttpGet]
    public IActionResult CarryOver(int year, int month)
        => RedirectToAction(nameof(PrepareNextMonth), new { year, month });

    [HttpGet]
    public async Task<IActionResult> PrepareNextMonth(int year, int month)
    {
        if (!CanEdit()) return LoginRedirect();
        if (year is < 1 or > 9998 || month is < 1 or > 12) return BadRequest("Invalid month or year.");

        var sections = await repo.GetMonthPreparationSectionsAsync(year, month);
        var startingAdjustment = await repo.GetCarryForwardInfoAsync(year, month);

        return View(new PrepareNextMonthViewModel
        {
            Year = year,
            Month = month,
            Sections = sections,
            StartingAdjustment = startingAdjustment.EffectiveAmount,
            HasStartingAdjustment = startingAdjustment.Exists
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmPrepareNextMonth(int year, int month, List<string>? selectedEntries, string? proposalsJson)
    {
        if (!CanEdit()) return LoginRedirect();
        if (year is < 1 or > 9998 || month is < 1 or > 12) return BadRequest("Invalid month or year.");

        var next = new DateTime(year, month, 1).AddMonths(1);
        var proposals = string.IsNullOrWhiteSpace(proposalsJson)
            ? new Dictionary<string, MonthPreparationProposal>()
            : System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, MonthPreparationProposal>>(proposalsJson) ?? [];
        var added = await repo.PrepareSelectedMonthEntriesAsync(year, month, selectedEntries ?? [], proposals);

        TempData["Success"] = added == 0
            ? $"No entries were added to {next:MMMM yyyy}. Existing entries were left unchanged."
            : $"{next:MMMM yyyy} prepared with {added} selected entr{(added == 1 ? "y" : "ies")}. No balance was carried forward.";

        return RedirectToAction(nameof(Index), new { year = next.Year, month = next.Month });
    }

    private IActionResult RedirectLegacyManageMonth(int year, int month, string? source)
    {
        return RedirectToRoute(
            FinanceManagerAspNet.Navigation.LegacyManageMonthRoutes.GetDestination(source),
            new { year, month });
    }
    private bool CanEdit() => User.Identity?.IsAuthenticated == true;
    private IActionResult LoginRedirect() => RedirectToAction("Login", "Auth", new { returnUrl = Request.Path.ToString() + Request.QueryString.ToString() });
}
