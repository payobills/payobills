# Claude Code Guidelines

## Worktrees

The `w/` folder contains git worktrees. Do not read files from `w/` unless explicitly asked to. Always prefer files in the main working directory.

## Rebasing

Always rebase against `origin/main`. Do not check `develop` or `master`.
