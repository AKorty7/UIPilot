using UnityEngine;

namespace UIPilot.Editor.Core
{
    internal static class UIPilotSceneQuery
    {
        // GameObject.Find skips inactive objects, and developers routinely untick
        // a full-screen panel to work on the one beneath it. Transform.Find does
        // not skip them, so look through the canvas first, then fall back to a
        // scene-wide search for objects that live elsewhere (legacy canvases).
        internal static GameObject FindInCanvas(string canvasName, string childName)
        {
            var canvas = GameObject.Find(canvasName);
            var child  = canvas != null ? canvas.transform.Find(childName) : null;

            return child != null ? child.gameObject : GameObject.Find(childName);
        }
    }
}
