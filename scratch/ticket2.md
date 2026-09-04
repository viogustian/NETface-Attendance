**Status:** unclaimed
**Type:** task

## Objective
Redesign the primary administrative workflows (Dashboard and Employee Management) to align with the new UI specification, ensuring data density and interactive forms are highly usable.

## Scope
- Restyle metric cards in the Dashboard to map existing data to the new visual style.
- Restyle the `EmployeeList` data table to include thin borders, ample padding, and new pill-shaped status badges.
- Restyle the `CreateEmployee` form to use updated input fields and card panels.
- Restyle the `FaceEnrollment` view, focusing on camera layout and state feedback.

## Out of Scope
- Changing backend fetch logic for employees or metrics.
- Modifying Attendance Session or Kiosk pages.

## Acceptance Criteria
- [ ] Dashboard metric cards match the new white-card aesthetic with large numeric values.
- [ ] Employee list table displays with correct density and pill badges for status.
- [ ] Create employee form uses the new `.input-field` styling and Galaxy Black text.
- [ ] Face Enrollment camera preview has rounded corners and clear visual states (processing, success, error).
- [ ] Delete/destructive actions distinctly use the Floral Magenta color.

## Files / Modules
- `src/pages/admin/Dashboard.jsx`
- `src/pages/admin/EmployeeList.jsx`
- `src/pages/admin/CreateEmployee.jsx`
- `src/pages/admin/FaceEnrollment.jsx`

## Dependencies
- #45

## Blocking Edges
- Must be completed to establish the pattern for Data Tables and Forms before tackling Sessions.

## Test Strategy
- Navigate to Dashboard and verify metric cards display correctly with live data.
- Test the full Employee CRUD flow visually (create form, view list, open delete prompt).
- Test Face Enrollment layout by triggering the camera and capturing a face to verify the feedback overlay.
