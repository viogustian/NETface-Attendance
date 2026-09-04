**Status:** unclaimed
**Type:** task

## Objective
Complete the visual redesign by applying the new aesthetics to Attendance Session management and the standalone Kiosk terminal, ensuring the end-user facing terminal matches the brand.

## Scope
- Restyle `SessionList`, `CreateSession`, and `SessionDetail` to match the new table and form aesthetics established in Ticket 2.
- Apply the new badge styling to session statuses.
- Redesign the `KioskHome` standalone interface (no sidebar/topbar) to use the new color palette, rounded camera feed, and clean overlays.

## Out of Scope
- Adding new filtering logic to sessions.
- Modifying the facial recognition algorithm, camera polling logic, or API calls.

## Acceptance Criteria
- [ ] Session list table displays consistently with the Employee table (borders, padding, badges).
- [ ] Session detail view lists attendees with the new visual hierarchy and status badges.
- [ ] Kiosk terminal displays a clean, fullscreen layout with a centered, rounded camera feed.
- [ ] Kiosk status overlays (Initializing, Ready, Recognized, Error) use the correct semantic brand colors (Success, Creamsicle, Floral Magenta).

## Files / Modules
- `src/pages/admin/sessions/SessionList.jsx`
- `src/pages/admin/sessions/CreateSession.jsx`
- `src/pages/admin/sessions/SessionDetail.jsx`
- `src/pages/kiosk/KioskHome.jsx`

## Dependencies
- #45
- #46

## Blocking Edges
- None. This completes the presentation layer redesign.

## Test Strategy
- Create a session, view the session list, and open session details, ensuring visual consistency with Ticket 2 patterns.
- Open the Kiosk route (`/kiosk`) and verify the fullscreen layout, camera presentation, and status overlays during a mock or real recognition attempt.
