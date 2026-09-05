# 0005. Visual Redesign Decisions

**Date:** 2026-09-05
**Status:** Proposed

## Context
A visual redesign of the existing NETFace Attendance frontend is required to match a provided screenshot reference. The application already has functional components (React Router, Admin Dashboard, Employee CRUD, Face Enrollment, Attendance Sessions, and Kiosk Mode) running against a C# ASP.NET Core backend.

The fundamental constraint is that **no features or backend logic can be changed or added**. The redesign must strictly be a presentation layer modification.

## Decisions

### 1. Palette & Typography
- **Creamsicle:** `#EF942E`
- **Sparkling Silver:** `#E2EAEF`
- **Floral Magenta:** `#CE3081`
- **Galaxy Black:** `#2A282A`
- **Surface/Background:** White
- **Typography:** `Inter` (or similar clean sans-serif) to establish a clear hierarchy.

### 2. Layout & Theme
- **Theme:** Forced Light Mode. The existing dark theme (`#0f172a`) will be completely replaced. No Light/Dark mode toggle will be implemented.
- **Sidebar:** Clean white background with active state highlighting (using Sparkling Silver and Galaxy Black). The brand logo at the top will be a simple text "NETFace".
- **Topbar:** Minimalist. It will **not** include dummy "Search anything", Chat, or Notification icons, preserving UI honesty.
- **Card Proportions & Shadows:** Use white surfaces with thin `Sparkling Silver` borders, minimal shadows (`box-shadow`), and rounded corners matching the screenshot.

### 3. Metric Cards (Dashboard)
- Only metrics that have existing data sources will be rendered. Currently, this includes:
  - Total Employees
  - Total Sessions
- These cards will use the exact visual styling (icon, large number, subtitle, border) from the screenshot but map to real NETFace data. We will not add dummy "On Leave", "Late", or "Absent" cards if the backend does not provide that data.

### 4. Data Tables (Employees & Sessions)
- Instead of building a "Sunday-Saturday" calendar view (which does not exist functionally), we will apply the screenshot's *table aesthetics* to our standard lists.
- This includes:
  - Striped or subtle colored header rows.
  - Thin borders and ample whitespace padding.
  - Converting plain text statuses into soft *pill badges* (transparent background, solid text, and optionally a dot icon) aligned with the palette.

### 5. Out of Scope
- No new API endpoints.
- No new routing logic or nested pages (e.g., Job Management, Payroll).
- No new filter dropdowns (existing Date filter will just be restyled).
- No Tailwind CSS (Vanilla CSS in `index.css` is retained).

## Consequences
- The frontend will look significantly cleaner and more modern without requiring backend collaboration.
- We maintain the simplicity of the architecture (no new libraries).
- The dashboard might look a bit emptier than the screenshot (fewer metric cards) but will represent actual application state accurately.
