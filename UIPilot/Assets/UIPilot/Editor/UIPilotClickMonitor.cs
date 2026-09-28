using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.LowLevel;
using UIPilot.Editor.Modules.ClickDebug;

namespace UIPilot.Editor
{
    // The coordinator behind the Click Debugger. In Play mode it adds one step to
    // the start of the game's Update, and each time the pointer goes down there it
    // asks the ClickDebug module what that click hit and why. It keeps the last few
    // answers for the window, and can log them to the Console, so it works with the
    // window closed. The step is added only in the Editor's Play mode and removed
    // when it ends; nothing goes into a build, and no scene is ever changed.
    [InitializeOnLoad]
    internal static class UIPilotClickMonitor
    {
        private const int Kept = 6;

        // Marks UIPilot's step in the player loop, so it can be found and removed.
        private struct ClickWatch { }

        internal static event Action Changed;

        // Newest first. Emptied when Play mode starts.
        internal static IReadOnlyList<ClickReport> Clicks => _clicks;

        internal static bool Watching { get; private set; }

        private static readonly List<ClickReport> _clicks = new List<ClickReport>();

        static UIPilotClickMonitor()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            Refresh(); // Play mode after a domain reload lands here
        }

        // Starts or stops watching to match Play mode and the Watch setting.
        internal static void Refresh()
        {
            Watch(EditorApplication.isPlaying);
        }

        private static void Watch(bool playing)
        {
            Watching = playing && ClickDebugSettings.Watch;
            SetStep(Watching);
            Changed?.Invoke();
        }

        // Unity still reports Play mode while it is exiting, so each change says which way it goes.
        private static void OnPlayModeStateChanged(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.EnteredPlayMode)
            {
                _clicks.Clear();
                Watch(true);
            }
            else if (change == PlayModeStateChange.ExitingPlayMode)
            {
                Watch(false);
            }
        }

        // On the press, not the release, and before the game's own scripts run
        // that frame: the UI is still as the player saw it when they clicked.
        private static void OnFrame()
        {
            if (!Application.isPlaying || !ClickPointer.WentDown(out var position)) return;

            Record(ClickDebugModule.Inspect(position));
        }

        private static void Record(ClickReport report)
        {
            _clicks.Insert(0, report);
            if (_clicks.Count > Kept) _clicks.RemoveAt(Kept);

            var subject = report.Targets.Length > 0 ? report.Targets[0] : null;
            if (ClickDebugSettings.LogToConsole)
                Debug.Log(ClickDebugModule.Describe(report), subject);
            if (ClickDebugSettings.SelectTarget && subject != null)
                Selection.activeObject = subject;

            Changed?.Invoke();
        }

        // ── The player loop step ─────────────────────────────────────────────

        // Removes any step of ours, then adds one at the start of Update if wanted.
        private static void SetStep(bool wanted)
        {
            var loop = PlayerLoop.GetCurrentPlayerLoop();
            var phases = loop.subSystemList;
            if (phases == null) return;

            for (var i = 0; i < phases.Length; i++)
            {
                var steps = new List<PlayerLoopSystem>(phases[i].subSystemList ?? new PlayerLoopSystem[0]);
                steps.RemoveAll(step => step.type == typeof(ClickWatch));

                if (wanted && phases[i].type == typeof(UnityEngine.PlayerLoop.Update))
                    steps.Insert(0, new PlayerLoopSystem { type = typeof(ClickWatch), updateDelegate = OnFrame });

                phases[i].subSystemList = steps.ToArray();
            }

            PlayerLoop.SetPlayerLoop(loop);
        }
    }
}
