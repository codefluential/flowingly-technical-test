# Build Logs

This directory contains the implementation build log documenting the development progress of the Flowingly Parsing Service.

## What is the Build Log?

The Build Log (`BUILDLOG.md`) is a chronological record of implementation activities, capturing:

- **Date & Time**: When work was completed
- **Changes Made**: What was implemented, refactored, or fixed
- **Rationale**: Why specific approaches were chosen
- **Issues Encountered**: Problems faced and how they were resolved
- **Testing Notes**: Test results and coverage updates
- **Deployment Events**: Release milestones and production deployments

## Purpose

The Build Log serves several key purposes:

1. **Progress Tracking**: Maintain a clear record of development velocity and milestones
2. **Knowledge Transfer**: Help team members understand the evolution of the codebase
3. **Debugging Aid**: Provide context when investigating issues introduced in specific timeframes
4. **Audit Trail**: Document compliance with development processes and quality gates
5. **Retrospective Data**: Supply material for sprint reviews and retrospectives

## Format

Each entry in `BUILDLOG.md` follows this structure:

```markdown
## YYYY-MM-DD HH:MM - [Summary Title]

**Changes**:
- Bullet list of specific changes made
- Each change should be concrete and verifiable

**Rationale**:
Brief explanation of why these changes were made and what problem they solve.

**Issues/Blockers** (if any):
Description of problems encountered and resolution approach.

**Testing**:
What tests were added or updated; test results.

**Deployment** (if applicable):
Environment deployed to, deployment method, smoke test results.

---
```

## Main Build Log

The primary build log is maintained in: [`BUILDLOG.md`](./BUILDLOG.md)

## Best Practices

- **Update Frequently**: Add entries after completing significant work units
- **Be Specific**: Include file names, component names, and specific changes
- **Link to Commits**: Reference git commit SHAs for traceability
- **Link to ADRs**: Reference relevant Architecture Decision Records
- **Keep it Concise**: Focus on what changed and why, not implementation details
- **Use Conventional Commit Scopes**: Align with commit message conventions for consistency

## Relationship to ADRs

While ADRs document **architectural decisions**, the Build Log documents **implementation activities**. ADRs answer "why this architecture?" while the Build Log answers "what was built when?"

## Starting a New Entry

When beginning work, create a new entry with the current date/time and update it as you complete tasks. This ensures the log stays current and accurate.
