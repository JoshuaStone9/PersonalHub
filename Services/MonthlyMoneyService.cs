using FinanceManagerAspNet.Models;

namespace FinanceManagerAspNet.Services;

public sealed record MonthlyMoneyOperationResult(string Message);

public interface IMonthlyMoneyService
{
    Task<MonthlyMoneyOperationResult> SaveStartingAdjustmentAsync(StartingAdjustmentInput input);
    Task<MonthlyMoneyOperationResult> DeleteStartingAdjustmentAsync(MonthlyRecordInput input);
    Task<MonthlyMoneyOperationResult> AddIncomeAsync(MonthlyIncomeEntryInput input);
    Task<MonthlyMoneyOperationResult> UpdateIncomeAsync(MonthlyIncomeEntryInput input);
    Task<MonthlyMoneyOperationResult> DeleteIncomeAsync(MonthlyRecordInput input);
    Task<MonthlyMoneyOperationResult> SetIncomeRecurringAsync(IncomeRecurrenceInput input);
    Task<MonthlyMoneyOperationResult> SetupRecurringIncomeAsync(RecurringIncomeSetupInput input);
    Task<MonthlyMoneyOperationResult> AddPaymentAsync(MonthlyPaymentInput input);
    Task<MonthlyMoneyOperationResult> DeletePaymentAsync(MonthlyPaymentRecordInput input);
    Task<MonthlyMoneyOperationResult> SetPaymentRecurringAsync(MonthlyPaymentRecurrenceInput input);
    Task<MonthlyMoneyOperationResult> SetupMonthAsync(MonthSetupInput input);
    Task<MonthlyMoneyOperationResult> UpdateAmountAsync(MonthlyPaymentAmountInput input);
}

public sealed class MonthlyMoneyService(
    FinanceRepository repository,
    IMonthlyMoneyRequestPolicy requestPolicy) : IMonthlyMoneyService
{
    public async Task<MonthlyMoneyOperationResult> SaveStartingAdjustmentAsync(StartingAdjustmentInput input)
    {
        await repository.SaveCarryForwardOverrideAsync(input.Year, input.Month, input.Amount, input.Reason);
        return new("Starting adjustment saved.");
    }

    public async Task<MonthlyMoneyOperationResult> DeleteStartingAdjustmentAsync(MonthlyRecordInput input)
    {
        await repository.DeleteCarryForwardAsync(input.Year, input.Month);
        return new("The starting adjustment was removed.");
    }

    public async Task<MonthlyMoneyOperationResult> AddIncomeAsync(MonthlyIncomeEntryInput input)
    {
        DateTime selectedDate = requestPolicy.ResolveEntryDate(input.Year, input.Month, input.Date);
        await repository.AddMonthlyIncomeEntryAsync(
            input.Name,
            input.Amount,
            selectedDate,
            input.Category,
            input.Notes,
            input.IsRecurring);
        return new($"{input.Name.Trim()} income added.");
    }

    public async Task<MonthlyMoneyOperationResult> UpdateIncomeAsync(MonthlyIncomeEntryInput input)
    {
        await repository.UpdateMonthlyIncomeEntryAsync(
            input.Id,
            input.Name,
            input.Amount,
            input.Date,
            input.Category,
            input.Notes,
            input.IsRecurring);
        return new($"{input.Name.Trim()} income updated.");
    }

    public async Task<MonthlyMoneyOperationResult> DeleteIncomeAsync(MonthlyRecordInput input)
    {
        await repository.DeleteMonthlyIncomeEntryAsync(input.Id);
        return new("Income entry deleted.");
    }

    public async Task<MonthlyMoneyOperationResult> SetIncomeRecurringAsync(IncomeRecurrenceInput input)
    {
        await repository.SetMonthlyIncomeRecurringAsync(input.Id, input.IsRecurring);
        return new(input.IsRecurring ? "Income now repeats monthly." : "Income is now one-off.");
    }

    public async Task<MonthlyMoneyOperationResult> SetupRecurringIncomeAsync(RecurringIncomeSetupInput input)
    {
        int added = await repository.SetupRecurringIncomeAsync(input.Year, input.Month, input.EntryIds);
        string message = added == 0
            ? "No new recurring income was added."
            : $"{added} recurring income entr{(added == 1 ? "y" : "ies")} added.";
        return new(message);
    }

    public async Task<MonthlyMoneyOperationResult> AddPaymentAsync(MonthlyPaymentInput input)
    {
        DateTime selectedDate = requestPolicy.ResolveEntryDate(input.Year, input.Month, input.Date);
        await repository.AddPaymentAsync(
            input.Source, input.Name, input.Amount, selectedDate,
            input.Category, input.Type, input.Length, input.Notes, input.AccountBalanceId);

        if (input.Source == "savings" && input.Amount > 0m)
            await repository.ApplyReserveAllocationAsync(input.Name, input.Amount);

        if (input.IsPermanent && input.Source != "extra_expenses")
            await repository.UpsertMonthlyEntryTemplateAsync(
                input.Source, input.Name, input.Amount, input.Category, input.Type, input.Length, input.Notes);

        return new(input.Source == "savings"
            ? $"Contribution added to {input.Name.Trim()}."
            : $"{input.Name.Trim()} added.");
    }

    public async Task<MonthlyMoneyOperationResult> DeletePaymentAsync(MonthlyPaymentRecordInput input)
    {
        PaymentRow? existing = await repository.GetPaymentAsync(input.Source, input.Id);
        await repository.DeletePaymentAsync(input.Source, input.Id);
        if (input.Source == "savings" && existing is not null)
            await repository.ApplyReserveAllocationAsync(existing.Name, -existing.Amount);
        return new(existing is null ? "Entry removed." : $"{existing.DisplayName} removed.");
    }

    public async Task<MonthlyMoneyOperationResult> SetPaymentRecurringAsync(MonthlyPaymentRecurrenceInput input)
    {
        PaymentRow? entry = await repository.GetPaymentAsync(input.Source, input.Id);
        if (entry is null) return new("Entry not found.");

        if (input.IsPermanent)
            await repository.UpsertMonthlyEntryTemplateAsync(
                input.Source, entry.Name, entry.Amount, entry.Category, entry.Type, entry.Length, entry.Notes);
        else
            await repository.SetMonthlyEntryTemplateActiveAsync(input.Source, entry.Name, false);

        return new(input.IsPermanent
            ? $"{entry.DisplayName} now repeats monthly."
            : $"{entry.DisplayName} is now a one-off entry.");
    }

    public async Task<MonthlyMoneyOperationResult> SetupMonthAsync(MonthSetupInput input)
    {
        int added = await repository.SetupMonthFromTemplatesAsync(input.Year, input.Month, input.Source, input.Items);
        return new(added == 0
            ? "No new recurring entries were added. Existing entries were left unchanged."
            : $"{added} recurring entr{(added == 1 ? "y" : "ies")} added to {new DateTime(input.Year, input.Month, 1):MMMM yyyy}.");
    }

    public async Task<MonthlyMoneyOperationResult> UpdateAmountAsync(MonthlyPaymentAmountInput input)
    {
        PaymentRow? existing = await repository.GetPaymentAsync(input.Source, input.Id);
        if (existing is null) return new("Entry not found.");

        await repository.UpdatePaymentAsync(
            input.Source, input.Id, existing.Name, input.Amount, existing.Date,
            existing.Category, existing.Type, existing.Length, existing.Notes, existing.AccountBalanceId);

        if (input.Source == "savings")
            await repository.ApplyReserveAllocationAsync(existing.Name, input.Amount - existing.Amount);

        return new($"{existing.DisplayName} amount updated.");
    }
}
