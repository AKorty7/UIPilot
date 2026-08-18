# Branch: Sonnet-Test-Branch

Multi-model comparison branch — Sonnet's test lane.

- Base: an earlier Dev snapshot, commit `23c14ab` ("Refactor UIPilot demo UI: buttons & layout")
- Status: **stale relative to Dev** — missing 3 later Dev commits (dark text colour fix on
  generated titles/buttons, packages-lock cleanup, and the fix protecting
  `UIPilot_GameManager.cs` from silent regeneration)
- No Sonnet-specific work committed on top of that snapshot yet

Part of a set of per-tool test branches (`Sonnet-Test-Branch`, `Grok-Test-Branch`,
`Composer-Test-Branch`) used to compare how different AI coding tools handle the same
UIPilot task. Rebase or reset onto current `Dev` before starting new work here.
