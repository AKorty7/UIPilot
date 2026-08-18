# Branch: Composer-Test-Branch

Multi-model comparison branch — Cursor Composer's test lane.

- Base: same earlier Dev snapshot as Sonnet-Test-Branch, commit `23c14ab`
  ("Refactor UIPilot demo UI: buttons & layout")
- Extra commit on top: `4e6f077` "feat: panel show/hide logic — v1.0.0-alpha
  (test branch: sonnet / composer)"
- Status: **stale relative to Dev** — missing the same 3 later Dev commits as
  Sonnet-Test-Branch (dark text colour fix, packages-lock cleanup, GameManager
  regeneration-protection fix)

Part of a set of per-tool test branches (`Sonnet-Test-Branch`, `Grok-Test-Branch`,
`Composer-Test-Branch`) used to compare how different AI coding tools handle the same
UIPilot task. Rebase or reset onto current `Dev` before starting new work here.
