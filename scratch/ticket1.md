**Status:** unclaimed
**Type:** task

## Objective
Establish the new Light Mode design system foundation and apply it to the primary entry points (Login and Admin Application Shell). This ensures a clean base for all subsequent pages.

## Scope
- Define CSS custom properties for the new color palette (Creamsicle, Sparkling Silver, Floral Magenta, Galaxy Black).
- Update base typography, spacing, and component utilities (`.btn-primary`, `.btn-secondary`, `.card`, `.input-field`, `.badge`) in `index.css`.
- Restyle `AdminLayout.jsx` (Sidebar and Topbar) to a clean white theme.
- Restyle `Login.jsx` to match the new aesthetic.

## Out of Scope
- Restyling data tables or internal admin pages (Dashboard, Employees, Sessions).
- Modifying routing or authentication logic.

## Acceptance Criteria
- [ ] `index.css` contains the new light mode variables and base utility classes.
- [ ] Dark mode styles and gradients are completely removed.
- [ ] The Sidebar is white with a "NETFace" text logo and subtle active link highlighting.
- [ ] The Topbar is minimal with a bottom border and no dummy functional icons.
- [ ] The Login page renders correctly with the new styling and contrast rules.

## Files / Modules
- `src/index.css`
- `src/components/AdminLayout.jsx`
- `src/pages/admin/Login.jsx`

## Dependencies
- None

## Blocking Edges
- Blocks Ticket 2 and Ticket 3 as it provides the foundational CSS.

## Test Strategy
- Visually inspect the Login page.
- Log in and verify that the application shell renders correctly and navigation links work, ensuring the layout does not break when viewing unstyled inner content.
