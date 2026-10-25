# Architecture and development conventions

Finance Manager is an **ASP.NET Core MVC** application. It uses controllers, typed view models and Razor views under `Views/`; it does not use Razor Pages (`PageModel`, `@page` or `MapRazorPages`). Razor is the view syntax in both architectures, so a `.cshtml` file alone does not make a feature a Razor Page.

## Request flow

```text
HTTP request → MVC controller → application/domain service → repository → SQL Server
                                  ↓
                         typed view model → Razor view
```

- Controllers own HTTP concerns: binding, authentication/authorisation decisions, status results, TempData and redirects.
- Services own financial rules, calculations and multi-step operations. Prefer interfaces where a boundary is tested or has more than one consumer.
- `FinanceRepository` and repository classes own SQL and persistence mapping only. They must not decide presentation or financial policy.
- View models shape data for one screen or component. Domain/database models should not acquire display-only state.
- Razor views render supplied state. Avoid SQL, financial calculations, service orchestration and large blocks of route-selection logic in views.

## MVC presentation conventions

- Use a full view for a page, a partial for passive reusable markup, and a view component when reusable UI needs server-side composition or dependencies. View components may depend on read services, not directly on repositories.
- Prefer typed models over new `ViewBag` state. Existing `ViewBag` usage is legacy and should be removed when its owning feature is changed.
- Keep forms on their existing controller/action until a phase explicitly migrates the write workflow. POST actions require antiforgery validation and the existing edit-authentication policy.
- Preserve the selected month, active section and relevant record focus after writes.
- Keep accessibility state explicit: landmark labels, `aria-current`, visible focus and text equivalents for badges or colour states.

## Navigation and routing

- Phase 15 navigation metadata belongs in `Navigation/FinanceNavigationService.cs` and is consumed through dependency injection.
- Canonical route names belong in `Navigation/FinanceRouteNames.cs`; generate links by route name instead of repeating URL strings in Razor.
- Canonical endpoint registration and temporary legacy redirects belong in the routing extensions under `Navigation/`, keeping `Program.cs` as application composition rather than a route catalogue.
- Compatibility redirects are GET/HEAD only, preserve the query string and must not intercept POST workflows or specialist deep links.
- The shared hub navigation is a view component because it derives state from the current request. Its Razor view contains markup only.

## Authentication and data safety

- The application uses one local owner password stored only as a password hash and issues a cookie identity after successful login.
- Do not add authentication bypasses, plaintext credentials or development-only write access.
- Navigation and presentation work must not change financial calculations, database schema, stored records or write semantics unless the owning phase explicitly requires it.

## CSS and JavaScript

- Put feature-specific styles and scripts in feature files. Reserve `site.css` and `site.js` for tokens, the application shell and genuinely shared behaviours.
- Do not add another unrelated section to a large shared asset when the code has a clear feature owner.
- JavaScript should progressively enhance server-rendered HTML; core navigation and forms must remain usable without it.

## Testing and review

- Pure calculations and navigation mappings receive direct unit tests.
- Route changes require coverage for canonical ownership, generated route metadata and compatibility redirects.
- A phase is not complete until the main project builds, relevant tests pass and its phase document records implementation and verification.
- Reviews should reject controllers that accumulate financial policy, services that return MVC results, repositories that make business decisions, views that query services, duplicated route strings and new untyped view state without a documented reason.

## Known legacy debt

Several controllers and Razor views are broad, `FinanceRepository` is a catch-all data-access class, and `site.css` contains accumulated feature styling. These are real maintainability issues, but they should be addressed through the scoped Phase 17 decomposition work rather than opportunistic rewrites during navigation changes.
