# Web UI Rules

## Scope

- These rules apply to Razor pages, page models, components, CSS, and page-specific JavaScript in this project.

## UI / CSS Rules

- Preserve the existing site-wide CSS design system. Do not introduce a new visual style on a per-page basis.
- Prefer reuse of existing layout and page patterns over creating new page-specific structures.
- Do not introduce a component-library mindset unless the user explicitly asks for it.
- Keep the UI compact and suitable for a business or back-office system.
- Remove redundant explanatory copy when the page title, section title, and available actions already make the intent clear.
- Avoid `page-heading__meta`, long helper paragraphs, and verbose empty-state text unless the information is required for the user to complete the task.
- Prefer short section labels and action labels over repeating the full page name inside nested panels.
- Avoid marketing-style layouts, oversized headings, exaggerated whitespace, large shadows, or decorative styling.
- Minimize border radius. Prefer square or near-square corners unless there is a clear reason otherwise.
- Prefer borders, separators, and subtle background contrast over shadows for visual hierarchy.
- Keep spacing tight and consistent with the existing spacing scale. Do not invent new arbitrary spacing values without a reason.
- Reuse existing CSS variables and existing class conventions before adding new ones.
- Do not add inline styles unless the user explicitly asks for them or there is no reasonable alternative.
- Do not add page-specific CSS rules to global files unless they are genuinely reusable.
- New CSS should be placed in the correct layer or file according to the current CSS structure.
- Do not override global tokens or base rules inside page-specific styles.

## Layout Rules

- `html`, `body`, and the main app or root container must remain height-locked as designed by the existing layout system.
- Do not introduce page implementations that cause global document scrolling unless the user explicitly asks for it.
- Scrolling should remain inside designated internal scroll regions.
- In flex layouts, carefully handle `min-width: 0` and `min-height: 0` to avoid overflow and broken scrolling.
- Prefer existing layout primitives and page shells for split layouts, toolbars, sidebars, list-detail pages, and form pages.
- Do not create ad-hoc layout wrappers if an existing page shell or layout primitive already solves the problem.

## New Page Rules

- Every new page should follow an established page pattern whenever possible.
- Preferred page patterns are list page, split list/detail page, form or edit page, and dashboard or shell page.
- Before creating a new page structure, check whether an existing page can be used as the starting template.
- New pages should fit visually with existing pages without requiring global CSS changes unless the user explicitly wants a broader redesign.
- If a new page needs a new pattern, keep it minimal and aligned with the existing visual and layout rules.

## CSS Change Rules

- Prefer extension over duplication.
- Prefer adding small reusable rules over copying large blocks of page-specific CSS.
- Avoid one-off magic numbers unless required by a concrete layout constraint.
- Avoid deep selector nesting and fragile structure-dependent selectors.
- If adding new CSS variables, keep them consistent with the existing token naming scheme.
- If adding a new page stylesheet, keep it narrowly scoped to that page pattern.

## Page Review Checklist

When creating or modifying a page, verify:
- no accidental `body` scrolling,
- no broken internal scroll regions,
- no flex child overflow caused by missing `min-width: 0` or `min-height: 0`,
- no unnecessary border radius or shadow,
- no excessive whitespace,
- no inline-style-driven layout unless explicitly requested,
- no duplicated page structure that should have been abstracted from an existing pattern,
- no visual drift from the established back-office style.
