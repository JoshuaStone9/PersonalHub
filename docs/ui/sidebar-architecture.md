# Sidebar architecture

The finance shell is rendered by `Views/Shared/_Layout.cshtml` for non-Vault controllers. Its primary entries come from `IFinanceNavigationService` and are limited to Home, Month, Accounts, Pots, Plan and Insights.

Tasks remains a global utility entry with a due-count badge. Settings remains in the utility area. Personal Vault is an app switch and retains its specialist internal shell.

## State and breakpoints

- Desktop collapse class: `sidebar-collapsed` on `#fmAppShell`.
- Mobile open class: `open` on `#fmSidebar`.
- Persisted key: `fm-sidebar-collapsed`.
- Above 1080px: full sidebar unless manually collapsed.
- 761px–1080px: icon-only sidebar with labelled tooltips.
- 760px and below: off-canvas navigation with a mobile header; selecting a link closes it.

Destination tabs are rendered by `FinanceHubNavigationViewComponent`. The active destination and active tab are separate states. Tabs remain horizontally scrollable at narrow widths and preserve year/month query values where the destination supports them.
