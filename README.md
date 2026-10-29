# Personal Hub

Personal Hub is an ASP.NET Core MVC application for managing personal finances, organising possessions, and planning a move in one place. It combines **Finance Manager** for budgeting, savings goals, and financial forecasts; **Personal Vault** for tracking possessions, collections, and asset values; and **Move In Planner** for organising household purchases and comparing product choices. All three tools are accessible from a single welcome screen, with a shared login for editing records.

Finance Manager remains compatible with the existing WinForms data. Move In Planner is hosted at `/MoveInPlanner`, retains its separate `MoveInPlannerDb` database, and uses the shared login for editing.

## Finance Manager compatibility

It reads your existing SQL Server database `Finance_Manager` and keeps the existing table names used by the WinForms app:

- `dbo.bills`
- `dbo.extra_expenses`
- `dbo.investments`
- `dbo.savings`
- `dbo.emergency_fund`
- `dbo.monthly_allowance`

The dashboard calculations use the same core approach:

`remaining fund = monthly allowance - bills - extra expenses - investments + savings`

The monthly saving target is set to `£1,200`, and the global goal is set to `£20,000`.

## New optional tables

On startup/use, the repository creates small extra tables if missing:

- `finance_settings` for hidden/changeable settings such as interest rate
- `account_balances` for Lucy's ISA, Monzo pots and future pots/accounts
- `account_balance_history` for update pattern/statistics
- `monthly_income_stats` for manual money-in and sick day tracking

These are additive and do not replace your existing data.

## How to run

1. Open the folder in Visual Studio 2022.
2. Restore NuGet packages.
3. Check `appsettings.json` connection string.
4. Run the project.

You can also set an environment variable named `FM_CONNECTION_STRING` to override the connection string, matching your old app pattern.

## Main features added

- Phone-friendly dashboard.
- Month navigation with left/right arrows.
- Red/green carry-over result before each month.
- £1,200 monthly saving target.
- £20,000 global goal including emergency fund and extra accounts.
- Interest forecast for emergency fund, Lucy's ISA and Monzo pots.
- Hidden-ish advanced interest fields inside the account modal.
- Statistics page for income, sick days, account updates and goal estimates.
- One carry-over button with checklist sections for bills, investments and optional extra expenses.

## Interest forecast update

The Statistics page now calculates projected balances using monthly compounding:

1. Current account balance is taken from `emergency_fund` and `account_balances`.
2. Monthly contribution is added each month.
3. Monthly interest is applied using `annual rate / 12`.
4. The page shows normal projected money, projected money with interest, interest earned, and the estimated goal month.

The carry-over SQL has also been simplified to avoid INSERT column/value count errors.

## Personal finance updates

This version includes several improvements to make managing my finances easier:

- Improved the monthly carry-over process.
- Added a simpler dashboard flow for recording payments and bills.
- Added interest forecasts to help me understand how my savings may grow.
- Updated the £20,000 goal forecast to include my planned monthly saving target of £1,200, alongside my existing savings and projected interest.

## House and Moneybox planning

The Statistics page now includes a house savings planner that allows me to:

- Enter the total value of savings held outside the app.
- Track progress towards my £30,000 house deposit goal.
- Add my current Moneybox balance and interest rate.
- Include any applicable bonus when calculating the amount still needed.
- Compare the remaining amount with and without projected Moneybox interest.
- Calculate potential interest for a custom amount, rate, and saving period.
- Apply my £1,200 monthly saving target to the house forecast without including it in the Emergency Fund projection.
