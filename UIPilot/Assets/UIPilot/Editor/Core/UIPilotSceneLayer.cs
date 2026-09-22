using System;
using UnityEngine;

namespace UIPilot.Editor.Core
{
    // One layer of a theme's scene, back to front: a sprite that covers the screen
    // and the material that colours it through the day. The sprite is white where
    // it is drawn (the material supplies every colour) and clear elsewhere; all of
    // a theme's layers share one frame, so they line up whatever the screen shape.
    [Serializable]
    public struct UIPilotSceneLayer
    {
        [Tooltip("White artwork on a clear background, the same frame as the theme's other layers.")]
        public Sprite sprite;

        [Tooltip("A UIPilot/Scene Layer material: the layer's colour at night, dawn, day and dusk, and whether it rises with the sun. Duplicate it to change the colours.")]
        public Material material;
    }
}
