# Issue tracker: GitHub Issues

Issues and specs for this repo are tracked using GitHub Issues. Agents are expected to use the GitHub CLI (`gh issue`) to interact with tickets.

## Conventions

- Each discrete task or spec should be recorded as a GitHub Issue.
- Discussion, notes, and progress should be recorded as comments on the issue.
- **Note**: Triage labels are intentionally NOT configured for this project.

## When a skill says "publish to the issue tracker"

Create a new issue using the GitHub CLI:
`gh issue create --title "Your Title" --body "Your issue body"`

## When a skill says "fetch the relevant ticket"

Read the ticket from GitHub using the issue number:
`gh issue view <issue-number>`

For reading comments, append `--comments`:
`gh issue view <issue-number> --comments`

## Wayfinding operations

Used by `/wayfinder` or general agent navigation.

- **List active issues**: Scan the issue tracker for open tasks.
  `gh issue list`
- **Claim**: Assign the issue to yourself before working on it (assuming `@me` context).
  `gh issue edit <issue-number> --add-assignee @me`
- **Resolve**: Add your final answer/conclusion as a comment, then close the issue.
  `gh issue comment <issue-number> --body "Your resolution summary"`
  `gh issue close <issue-number>`
