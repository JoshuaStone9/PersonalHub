using FinanceManagerAspNet.Models;
using FinanceManagerAspNet.Services;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManagerAspNet.Controllers;

public sealed class MonthlyMoneyController(
    FinanceRepository repo,
    IMonthlyMoneyRequestPolicy requestPolicy,
    IMonthlyMoneyService monthlyMoneyService) : FinanceController
{
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveStartingAdjustment(StartingAdjustmentInput input)
    {
        MonthlyRequestValidation validation = requestPolicy.ValidateMonth(input.Year, input.Month);
        IActionResult? failure = ValidateWriteRequest(validation);
        if (failure is not null) return failure;

        MonthlyMoneyOperationResult result = await monthlyMoneyService.SaveStartingAdjustmentAsync(input);
        return OperationSucceeded(result, GetMonthOverviewUrl(input.Year, input.Month), input);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteStartingAdjustment(MonthlyRecordInput input)
    {
        MonthlyRequestValidation validation = requestPolicy.ValidateMonth(input.Year, input.Month);
        IActionResult? failure = ValidateWriteRequest(validation);
        if (failure is not null) return failure;

        MonthlyMoneyOperationResult result = await monthlyMoneyService.DeleteStartingAdjustmentAsync(input);
        return OperationSucceeded(result, GetMonthOverviewUrl(input.Year, input.Month), input);
    }

    [HttpGet]
    public async Task<IActionResult> Income(int? year, int? month)
    {
        var today = DateTime.Today;
        var selectedYear = year ?? today.Year;
        var selectedMonth = month ?? today.Month;
        if (selectedMonth is < 1 or > 12) return BadRequest("Month must be between 1 and 12.");

        var model = new MonthlyIncomeWorkspaceViewModel
        {
            Year = selectedYear,
            Month = selectedMonth,
            Entries = await repo.GetMonthlyIncomeEntriesAsync(selectedYear, selectedMonth),
            MissingRecurringEntries = await repo.GetMissingRecurringIncomeEntriesAsync(selectedYear, selectedMonth),
            PassiveIncomeEstimates = await repo.GetPassiveIncomeEstimatesAsync(selectedYear, selectedMonth)
        };
        return View("Income", model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RecordInterest(
        int year,
        int month,
        string sourceKey,
        string sourceName,
        decimal estimatedAmount,
        decimal actualAmount,
        DateTime receivedDate,
        string? notes,
        string interestHandling)
    {
        IActionResult? failure = ValidateWriteRequest(
            month is >= 1 and <= 12
            && !string.IsNullOrWhiteSpace(sourceKey)
            && !string.IsNullOrWhiteSpace(sourceName)
            && actualAmount >= 0m,
            "Enter a valid interest payment.");
        if (failure is not null) return failure;

        var date = receivedDate == default
            ? new DateTime(year, month, DateTime.DaysInMonth(year, month))
            : receivedDate;

        IActionResult? dateFailure = ValidateWriteRequest(
            date.Year == year && date.Month == month,
            "The received date must be inside the selected month.");
        if (dateFailure is not null) return dateFailure;

        if (string.Equals(interestHandling, "Ignore for now", StringComparison.OrdinalIgnoreCase))
        {
            TempData["Success"] = $"{sourceName} interest left awaiting confirmation.";
            var ignoredUrl = Url.Action(nameof(Income), new { year, month });
            return ignoredUrl is null ? RedirectToAction(nameof(Income), new { year, month }) : Redirect($"{ignoredUrl}#passive-income");
        }

        try
        {
            await repo.RecordInterestIncomeAsync(sourceKey, sourceName, estimatedAmount, actualAmount, date, notes, interestHandling);
            TempData["Success"] = string.Equals(interestHandling, "Add to Monthly Income", StringComparison.OrdinalIgnoreCase)
                ? $"{sourceName} interest added to this month's income and left ready for balance reconciliation."
                : $"{sourceName} interest recorded as kept invested and left ready for balance reconciliation.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        var url = Url.Action(nameof(Income), new { year, month });
        return url is null ? RedirectToAction(nameof(Income), new { year, month }) : Redirect($"{url}#passive-income");
    }


    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateInterestHandling(int id, int year, int month, string interestHandling)
    {
        IActionResult? failure = ValidateWriteRequest(
            id > 0 && month is >= 1 and <= 12,
            "Enter a valid interest payment.");
        if (failure is not null) return failure;

        try
        {
            await repo.UpdatePendingInterestHandlingAsync(id, interestHandling);
            TempData["Success"] = string.Equals(interestHandling, "Add to Monthly Income", StringComparison.OrdinalIgnoreCase)
                ? "Interest added to monthly income. Its handling is now locked."
                : "Interest handling updated.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        var url = Url.Action(nameof(Income), new { year, month });
        return url is null ? RedirectToAction(nameof(Income), new { year, month }) : Redirect($"{url}#passive-income");
    }


    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddIncome(MonthlyIncomeEntryInput input)
    {
        MonthlyRequestValidation validation = requestPolicy.ValidateIncome(input);
        IActionResult? failure = ValidateWriteRequest(validation);
        if (failure is not null) return failure;
        MonthlyMoneyOperationResult result = await monthlyMoneyService.AddIncomeAsync(input);
        string? redirectUrl = Url.Action(nameof(Income), new { year = input.Year, month = input.Month, focus = "quick-entry" });
        return OperationSucceeded(result, redirectUrl, input);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateIncome(MonthlyIncomeEntryInput input)
    {
        MonthlyRequestValidation validation = requestPolicy.ValidateIncome(input);
        IActionResult? failure = ValidateWriteRequest(
            validation.IsValid && input.Id > 0,
            validation.Error ?? "Enter a valid income entry.");
        if (failure is not null) return failure;
        MonthlyMoneyOperationResult result = await monthlyMoneyService.UpdateIncomeAsync(input);
        string? redirectUrl = Url.Action(nameof(Income), new { year = input.Year, month = input.Month });
        return OperationSucceeded(result, $"{redirectUrl}#entry-{input.Id}", input);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteIncome(MonthlyRecordInput input)
    {
        MonthlyRequestValidation validation = requestPolicy.ValidateMonth(input.Year, input.Month);
        IActionResult? failure = ValidateWriteRequest(
            validation.IsValid && input.Id > 0,
            validation.Error ?? "Enter a valid income entry.");
        if (failure is not null) return failure;
        MonthlyMoneyOperationResult result = await monthlyMoneyService.DeleteIncomeAsync(input);
        string? redirectUrl = Url.Action(nameof(Income), new { year = input.Year, month = input.Month });
        return OperationSucceeded(result, redirectUrl, input);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SetIncomeRecurring(IncomeRecurrenceInput input)
    {
        MonthlyRequestValidation validation = requestPolicy.ValidateMonth(input.Year, input.Month);
        IActionResult? failure = ValidateWriteRequest(
            validation.IsValid && input.Id > 0,
            validation.Error ?? "Enter a valid income entry.");
        if (failure is not null) return failure;
        MonthlyMoneyOperationResult result = await monthlyMoneyService.SetIncomeRecurringAsync(input);
        return OperationSucceeded(result, null, new { input.Id, input.IsRecurring });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SetupIncomeMonth(RecurringIncomeSetupInput input)
    {
        MonthlyRequestValidation validation = requestPolicy.ValidateMonth(input.Year, input.Month);
        IActionResult? failure = ValidateWriteRequest(validation);
        if (failure is not null) return failure;
        MonthlyMoneyOperationResult result = await monthlyMoneyService.SetupRecurringIncomeAsync(input);
        string? redirectUrl = Url.Action(nameof(Income), new { year = input.Year, month = input.Month });
        return OperationSucceeded(result, redirectUrl, input);
    }
    [HttpGet]
    public Task<IActionResult> EssentialBills(int? year, int? month)
        => BuildWorkspaceAsync(
            "bills",
            "EssentialBills",
            "Essential bills",
            "Fixed commitments",
            "Manage mortgage, council tax, utilities, insurance and other recurring commitments.",
            "receipt",
            "No essential bills recorded",
            "Add the first fixed commitment for this month.",
            "Add essential bill",
            "Choose a previous bill",
            "Bill name",
            "Description",
            supportsType: true,
            supportsLength: true,
            typeOptions: ["Mortgage", "Council Tax", "Utilities", "Insurance", "Subscription", "Loan", "Other"],
            isCarryOverEligible: true,
            year: year,
            month: month);

    [HttpGet]
    public Task<IActionResult> EverydaySpending(int? year, int? month)
        => BuildWorkspaceAsync(
            "everyday_spending",
            "EverydaySpending",
            "Everyday spending",
            "Normal monthly allowances",
            "Manage food, fuel and the flexible spending amounts used during the selected month.",
            "shopping-basket",
            "No everyday spending recorded",
            "Add the first normal monthly allowance or spending entry.",
            "Add everyday spending",
            "Choose a previous entry",
            "Entry name",
            "Notes",
            supportsCategory: true,
            supportsType: true,
            supportsLength: true,
            typeOptions: ["Food", "Fuel", "Travel", "Personal", "Entertainment", "Household", "Other"],
            isCarryOverEligible: true,
            year: year,
            month: month);

    [HttpGet]
    public Task<IActionResult> ExtraExpenses(int? year, int? month)
        => BuildWorkspaceAsync(
            "extra_expenses",
            "ExtraExpenses",
            "Extra expenses",
            "One-off monthly costs",
            "Keep unexpected and non-recurring costs separate from normal monthly spending.",
            "circle-plus",
            "No extra expenses this month",
            "One-off expenses are excluded from Carry over all.",
            "Add extra expense",
            "Copy a previous expense",
            "Expense name",
            "Reason or notes",
            supportsCategory: true,
            supportsType: true,
            supportsLength: true,
            typeOptions: ["Purchase", "Repair", "Health", "Travel", "Gift", "Other"],
            isCarryOverEligible: false,
            year: year,
            month: month);

    [HttpGet]
    public Task<IActionResult> Investments(int? year, int? month)
        => BuildWorkspaceAsync(
            "investments",
            "Investment contributions",
            "Monthly contributions",
            "Monthly contributions",
            "Record contributions made to investments without mixing them with current asset valuations.",
            "chart-pie",
            "No investment contributions recorded",
            "Add the first contribution for the selected month.",
            "Add investment contribution",
            "Choose a previous investment",
            "Investment name",
            "Provider or notes",
            supportsCategory: true,
            supportsType: true,
            supportsLength: true,
            typeOptions: ["ETF", "Fund", "Stock", "Pension", "Crypto", "Bond", "Other"],
            isCarryOverEligible: true,
            year: year,
            month: month);

    [HttpGet]
    public Task<IActionResult> MoneyPots(int? year, int? month)
        => BuildWorkspaceAsync(
            "savings",
            "MoneyPots",
            "Pot funding",
            "Monthly pot funding",
            "Record contributions to your existing pots while keeping pot goals and settings in Pots.",
            "piggy-bank",
            "No money pot contributions recorded",
            "Choose an active money pot and record the first contribution for this month.",
            "Add money pot contribution",
            "Choose a money pot",
            "Money pot",
            "Contribution notes",
            supportsType: true,
            typeOptions: ["Regular contribution", "One-off top-up", "Transfer", "Refund", "Other"],
            isCarryOverEligible: true,
            isMoneyPots: true,
            year: year,
            month: month);

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddPayment(MonthlyPaymentInput input)
    {
        IActionResult? failure = ValidateWriteRequest(
            IsWorkspaceSource(input.Source)
            && input.Month is >= 1 and <= 12
            && !string.IsNullOrWhiteSpace(input.Name)
            && input.Amount >= 0m,
            "Enter a valid monthly entry.");
        if (failure is not null) return failure;

        MonthlyMoneyOperationResult result = await monthlyMoneyService.AddPaymentAsync(input);
        return OperationSucceeded(result, GetWorkspaceUrl(input.Source, input.Year, input.Month, "quick-entry"), input);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeletePayment(MonthlyPaymentRecordInput input)
    {
        IActionResult? failure = ValidateWriteRequest(
            IsWorkspaceSource(input.Source) && input.Id > 0 && input.Month is >= 1 and <= 12,
            "Enter a valid monthly entry.");
        if (failure is not null) return failure;

        MonthlyMoneyOperationResult result = await monthlyMoneyService.DeletePaymentAsync(input);
        return OperationSucceeded(result, GetWorkspaceUrl(input.Source, input.Year, input.Month), input);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetPermanent(MonthlyPaymentRecurrenceInput input)
    {
        IActionResult? failure = ValidateWriteRequest(
            IsWorkspaceSource(input.Source) && input.Source != "extra_expenses" && input.Id > 0,
            "Extra expenses are always one-off entries.");
        if (failure is not null) return failure;

        MonthlyMoneyOperationResult result = await monthlyMoneyService.SetPaymentRecurringAsync(input);
        return OperationSucceeded(result, null, new { input.Id, input.IsPermanent });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetupMonth(MonthSetupInput input)
    {
        IActionResult? failure = ValidateWriteRequest(
            IsWorkspaceSource(input.Source) && input.Month is >= 1 and <= 12,
            "Month must be between 1 and 12.");
        if (failure is not null) return failure;

        MonthlyMoneyOperationResult result = await monthlyMoneyService.SetupMonthAsync(input);
        return OperationSucceeded(result, GetWorkspaceUrl(input.Source, input.Year, input.Month, "monthly-review"), input);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateAmount(MonthlyPaymentAmountInput input)
    {
        IActionResult? failure = ValidateWriteRequest(
            IsWorkspaceSource(input.Source) && input.Month is >= 1 and <= 12 && input.Amount >= 0m,
            input.Month is < 1 or > 12 ? "Month must be between 1 and 12." : "Amount cannot be negative.");
        if (failure is not null) return failure;

        MonthlyMoneyOperationResult result = await monthlyMoneyService.UpdateAmountAsync(input);
        return OperationSucceeded(result, $"{GetWorkspaceUrl(input.Source, input.Year, input.Month)}#entry-{input.Id}", input);
    }

    private static bool IsWorkspaceSource(string source)
        => source is "bills" or "everyday_spending" or "extra_expenses" or "investments" or "savings";

    private string? GetWorkspaceUrl(string source, int year, int month, string? focus = null)
        => Url.Action(GetWorkspaceAction(source), new { year, month, focus });

    private static string GetWorkspaceAction(string source)
        => source switch
        {
            "income" => nameof(Income),
            "bills" => nameof(EssentialBills),
            "everyday_spending" => nameof(EverydaySpending),
            "extra_expenses" => nameof(ExtraExpenses),
            "investments" => nameof(Investments),
            "savings" => nameof(MoneyPots),
            _ => throw new ArgumentOutOfRangeException(nameof(source), "Unknown monthly money workspace.")
        };

    private string GetMonthOverviewUrl(int year, int month)
    {
        string? destination = Url.RouteUrl(FinanceManagerAspNet.Navigation.FinanceRouteNames.MonthOverview, new { year, month });
        return $"{destination}#starting-adjustment";
    }

    private async Task<IActionResult> BuildWorkspaceAsync(
        string source,
        string actionName,
        string title,
        string eyebrow,
        string description,
        string icon,
        string emptyTitle,
        string emptyDescription,
        string addTitle,
        string existingLabel,
        string nameLabel,
        string notesLabel,
        bool supportsCategory = false,
        bool supportsType = false,
        bool supportsLength = false,
        IReadOnlyList<string>? typeOptions = null,
        bool isCarryOverEligible = false,
        bool isMoneyPots = false,
        int? year = null,
        int? month = null)
    {
        var today = DateTime.Today;
        var selectedYear = year ?? today.Year;
        var selectedMonth = month ?? today.Month;

        if (selectedMonth is < 1 or > 12)
            return BadRequest("Month must be between 1 and 12.");

        await repo.EnsureModernTablesAsync();

        List<MonthlyEntryTemplate> permanentTemplates = source == "extra_expenses"
            ? []
            : await repo.GetMonthlyEntryTemplatesAsync(source);
        List<MonthlyEntryTemplate> missingPermanentTemplates = source == "extra_expenses"
            ? []
            : await repo.GetMissingMonthlyEntryTemplatesAsync(source, selectedYear, selectedMonth);

        var model = new MonthlyMoneyWorkspaceViewModel
        {
            Year = selectedYear,
            Month = selectedMonth,
            Source = source,
            ActionName = actionName,
            Title = title,
            Eyebrow = eyebrow,
            Description = description,
            Icon = icon,
            EmptyTitle = emptyTitle,
            EmptyDescription = emptyDescription,
            AddTitle = addTitle,
            ExistingLabel = existingLabel,
            NameLabel = nameLabel,
            NotesLabel = notesLabel,
            SupportsCategory = supportsCategory,
            SupportsType = supportsType,
            SupportsLength = supportsLength,
            TypeOptions = typeOptions ?? [],
            IsCarryOverEligible = isCarryOverEligible,
            IsMoneyPots = isMoneyPots,
            Rows = await repo.GetRowsAsync(source, selectedMonth, selectedYear),
            Accounts = (await repo.GetAccountsAsync(await repo.GetEmergencyFundAsync())).OrderBy(x => x.Name).ToList(),
            ExistingOptions = await repo.GetExistingPaymentOptionsAsync(source),
            PermanentTemplates = permanentTemplates,
            MissingPermanentTemplates = missingPermanentTemplates,
            PotOptions = isMoneyPots
                ? (await repo.GetReservePotsAsync()).Where(x => x.IsActive).OrderBy(x => x.Priority).ThenBy(x => x.Name).ToList()
                : []
        };

        return View("Workspace", model);
    }
}
