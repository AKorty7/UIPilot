# Branch: feature/settings-panel-controls

Task branch (Claude) — Settings panel UI controls.

- Base: `Dev` tip, commit `147418a`
- Task: add `UIPilot_Settings_MasterVolumeSlider`, `UIPilot_Settings_MusicVolumeSlider`,
  `UIPilot_Settings_FullscreenToggle`, `UIPilot_Settings_ResolutionDropdown`, and
  `UIPilot_Settings_Btn_Apply` to the Settings panel — UI structure only, no functional
  wiring (no AudioListener/Screen.fullScreen/Screen.SetResolution calls) — plus
  `IsPanelIntact()` validation for the new children.
- Scope: `UIGeneratorModule.cs` and `UIGeneratorContent.cs` only. MainMenu/PauseMenu
  generation, `ScriptSetupModule.cs`, and `BindingModule.cs` untouched.
- Status: implementation complete in the working tree but **not yet committed** —
  awaiting review/approval and an in-Editor compile/test pass first.

Not part of the Sonnet/Grok/Composer model-comparison set — this is a regular task
branch off Dev.
