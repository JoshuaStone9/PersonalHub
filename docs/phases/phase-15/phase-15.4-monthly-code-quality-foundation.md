# Phase 15.4 — Monthly code-quality foundation

## Status

In progress. The first controller-boundary slice was implemented on 29 October 2026: monthly read actions now declare GET explicitly, starting-adjustment and income writes use typed inputs, and shared request policy and controller guards own repeated validation and HTTP failures.

## Goal

Create clear controller, service, repository and Razor ownership boundaries for the monthly experience before further dashboard and navigation redesign. This is a behaviour-preserving refactor, not a feature phase.

## Controller conventions

- Mark read actions explicitly with `HttpGet` and write actions with `HttpPost` and antiforgery validation.
- Replace long action parameter lists with purpose-specific input models.
- Keep controllers responsible for HTTP concerns, orchestration, result selection and user-facing messages.
- Centralise selected-month validation and canonical redirect generation.
- Standardise unauthorised, invalid-input, not-found and read-only responses.

## Service and repository boundaries

- Move validation policies, workflow decisions and transaction orchestration into typed application services.
- Keep financial calculations in their owning domain services.
- Keep SQL and persistence operations inside repositories or EF-backed data services.
- Return explicit workflow results for expected outcomes rather than using exceptions as control flow.
- Add direct service tests before removing controller-owned behaviour.

## Razor structure

- Split large monthly Razor views into cohesive typed partials.
- Use view components only when server-side composition or data loading is required.
- Extract repeated month selectors, metrics, entry rows, validation summaries and destructive-action panels where they have genuinely shared behaviour.
- Reduce broad `ViewBag` state in favour of typed view models.
- Preserve accessible labels, keyboard focus, responsive layouts and progressive enhancement.

## Monthly AJAX write contract

Monthly entry forms use `data-ajax-redirect` as a project-specific marker. It has no built-in browser behaviour and does not contain a destination. The shared site script intercepts marked forms, submits their existing antiforgery-protected fields with `fetch`, and consumes a typed operation response containing:

- Whether the operation succeeded.
- A user-facing result message.
- The canonical URL for the next valid page state, when navigation is required.
- The submitted or persisted response data needed by an in-place interaction.

The application service performs the same persistence workflow used by the former form post. The controller chooses the canonical destination and returns the response; JavaScript stores the confirmation message briefly in `sessionStorage`, navigates to the returned URL and removes the message after displaying it. Recurrence toggles use the same contract but update their control in place when no redirect URL is returned.

This replaces the previous controller-owned repository call, `TempData` success message and HTTP redirect for the converted monthly forms. It does not change financial calculations or stored values.

## Delivery sequence

1. Record the existing monthly controller actions and their owning workflows.
2. Introduce typed input and result models without changing routes.
3. Extract application services one workflow at a time, with service tests.
4. Reduce controller actions to HTTP orchestration.
5. Extract repeated Razor structures and remove superseded markup and styles.
6. Verify route compatibility, stored values and calculated totals after each slice.

## Acceptance criteria

- Monthly controllers contain no financial calculation or direct SQL logic.
- Every monthly action has an explicit HTTP method and write actions retain antiforgery protection.
- Complex monthly writes use typed inputs and tested application services.
- Repeated authentication, month validation and redirect code is materially reduced.
- The largest monthly views are divided into cohesive typed units without changing behaviour.
- Existing routes, selected-month context, totals and stored records remain compatible.

## Relationship to Phase 17

This phase brings forward the monthly-experience portions of Phase 17.2 and Phase 17.3 because the dashboard work will build on them. Phase 17 retains the project-wide controller cleanup, remaining shared components and deeper repository decomposition.

## Out of scope

- Project-wide controller conversion outside the monthly and dashboard surfaces.
- Full `FinanceRepository` decomposition.
- Removing startup schema management.
- Dashboard content redesign, which remains Phase 15.5.
- Navigation and terminology rollout, which remains Phase 15.6.
