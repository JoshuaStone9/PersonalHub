# Phase 15.0 — Information architecture foundation

## Status

Implemented on 23 October 2026. The destination shell, canonical routes, compatibility redirects and shared hub tabs are in place; specialist page consolidation remains assigned to later subphases.

## Goal

Define a stable destination model before changing routes or deleting interfaces. Routine finance work should be organised around user goals rather than individual controllers or database categories.

## Primary destinations

- **Home** — current position, exceptions and next actions.
- **Month** — monthly income, spending, allocations and setup.
- **Accounts** — balances, statements and holdings.
- **Pots** — goals, funding and pot activity.
- **Plan** — forecasts, what-if modelling and saved scenarios.
- **Insights** — statistics, trends and financial history.

Settings remains in the utility area. Personal Vault becomes an app switch rather than a finance destination. Financial Tasks remain accessible globally and are surfaced from Home.

## Required work

- Record every existing sidebar destination and its future owner.
- Define canonical route names for the six destinations.
- Define redirects for routes that will later be retired.
- Add a shared hub-tab pattern that works on desktop, collapsed sidebar and mobile navigation.
- Define the language mappings used by subsequent phases.
- Preserve existing routes during this foundation step.

## Implemented destination map

| Destination | Current page ownership | Canonical entry route |
| --- | --- | --- |
| Home | Dashboard summary, exceptions and next actions | `/Home` |
| Month | Income, essential bills, everyday spending, extra expenses, investment contributions and pot funding | `/Month` |
| Accounts | Balances, statement reconciliation and holdings | `/Accounts` |
| Pots | Pot goals and pot activity, with pot funding linked to its primary Month workflow | `/Pots` |
| Plan | Forecast, what-if modelling and saved scenarios | `/Plan` |
| Insights | Snapshot statistics, financial trends and event history | `/Insights` |

Settings remains a utility item. Tasks is a global item at `/Tasks`, and Personal Vault is presented as an explicit app switch.

The shared hub-tab pattern is composed by `FinanceHubNavigationViewComponent` and rendered by its typed component view. It exposes the current specialist pages within their destination, communicates the active tab independently of the active sidebar destination, preserves selected month context where supported, scrolls horizontally on narrow screens and remains labelled when the sidebar is collapsed.

## Compatibility routes

Existing controller routes remain available for write actions and specialist deep links. Legacy GET entry routes redirect temporarily to their canonical destination while preserving the query string, including:

- `/Dashboard` to `/Home`.
- `/MonthlyMoney/*` workspace entries to the matching `/Month/*` tab.
- `/Reconciliation`, `/StatementReconciliation` and `/Assets` entries to Accounts tabs.
- `/SavingPots` and `/MoneyPotActivity` entries to Pots tabs.
- `/Forecast/*` entries to Plan tabs.
- `/Statistics`, `/FinancialTrends` and `/Events` entries to Insights tabs.
- `/Reminders` and `/FinancialTasks` to `/Tasks`.

Deep links such as a statement workspace, pot anchor or scenario comparison continue to resolve. Later phases may retire additional legacy routes only after their replacements are verified.

## Terminology

- Use **Investment contributions** for monthly money assigned to investments.
- Use **Holdings** for current assets and valuations.
- Use **Pot funding** for monthly allocations.
- Use **Pot goals** for targets, balances and configuration.
- Use **Starting adjustment** for a manual amount brought into a month's plan.

The destination shell and shared tabs now use these mappings. Broader copy cleanup remains part of Phase 15.5 so this foundation does not redesign specialist pages prematurely.

## Acceptance criteria

- Every current navigation item has one documented destination.
- No record type has two proposed primary editing paths.
- The six-destination model is usable at all supported breakpoints.
- Old URLs and saved bookmarks have a documented compatibility path.
- No data model, calculation or write workflow changes in this subphase.

## Verification

- Navigation service tests cover the six-destination order, controller ownership, approved terminology, month preservation, specialist-tab state and compatibility redirects.
- The application builds with the existing package and nullable warnings and no new compiler errors.
- Local browser verification confirms the responsive application shell renders the six destinations and canonical Home route against the existing database.

## Out of scope

- Removing controllers or database tables.
- Combining actual account balances with virtual pot allocations.
- Redesigning specialist pages before their hub phase.
