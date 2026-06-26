# Claude Code Guidelines

## Worktrees

The `w/` folder contains git worktrees. Do not read files from `w/` unless explicitly asked to. Always prefer files in the main working directory.

## Rebasing

Always rebase against `origin/main`. Do not check `develop` or `master`.

## CSS

Do not use negative margins or negative padding.

<!-- SPECKIT START -->
For additional context about technologies to be used, project structure,
shell commands, and other important information, read the current plan
at `specs/001-bill-types/plan.md`
<!-- SPECKIT END -->
