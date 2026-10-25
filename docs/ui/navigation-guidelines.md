# Navigation guidelines

## Information architecture

Primary finance navigation contains exactly six destinations:

1. Home
2. Month
3. Accounts
4. Pots
5. Plan
6. Insights

Settings is a utility. Tasks is globally accessible. Personal Vault is a separate app context.

Specialist screens belong to one destination and appear as hub tabs or contextual links; they do not become additional primary sidebar entries. Each financial record type has one primary editing path.

## Routes and compatibility

- Add canonical routes in `FinanceRoutingExtensions` and route-name constants in `FinanceRouteNames`.
- Generate links by route name or controller/action; do not introduce new hard-coded application URLs in Razor.
- Keep old GET entry routes redirecting until their replacement is verified. Preserve query strings, selected month and record context.
- Do not redirect or remove a write route as part of presentation-only work.

## Behaviour

- Mark the active destination and active hub tab independently with `aria-current="page"`.
- Collapsed navigation must keep a unique accessible label and visible tooltip.
- Sidebar collapse preference remains local to the browser.
- Mobile navigation closes after a destination is selected.
- Keyboard focus remains visible and follows document order.

## Adding a screen

Assign the screen to an existing destination first. Extend the shared navigation service only when the screen needs persistent hub discovery; otherwise use a contextual link from its owner. A new primary destination requires an approved information-architecture phase change.
