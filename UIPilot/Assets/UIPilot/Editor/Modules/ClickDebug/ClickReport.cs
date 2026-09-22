using UnityEngine;

namespace UIPilot.Editor.Modules.ClickDebug
{
    // One click, explained: what took it, why, and what to select to fix it.
    internal sealed class ClickReport
    {
        internal readonly ClickVerdict Verdict;
        internal readonly string       Headline;
        internal readonly string[]     Details;
        internal readonly Object[]     Targets;   // the first is what the headline names
        internal readonly string       HitOrder;  // top first; null when nothing was hit
        internal readonly Vector2      Position;
        internal readonly float        PlayTime;

        internal ClickReport(ClickVerdict verdict, string headline, string[] details,
            Object[] targets, string hitOrder, Vector2 position)
        {
            Verdict  = verdict;
            Headline = headline;
            Details  = details ?? new string[0];
            Targets  = targets ?? new Object[0];
            HitOrder = hitOrder;
            Position = position;
            PlayTime = Time.time;
        }
    }
}
