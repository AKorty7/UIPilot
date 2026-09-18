using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace UIPilot.Editor.Modules.UIGenerator
{
    // Finds the art kit (Assets/UIPilot/Art) that the generated menus are drawn with.
    // Assets are found by name, not by path, so the kit still works after the buyer
    // moves the UIPilot folder. Anything missing comes back null and the generator
    // falls back to flat colour or the default font instead of failing.
    internal static class UIGeneratorArt
    {
        internal static Sprite Sprite(string assetName)
        {
            return Load<Sprite>(assetName, UIGeneratorContent.Art.SpriteFilter);
        }

        internal static Texture2D Texture(string assetName)
        {
            return Load<Texture2D>(assetName, UIGeneratorContent.Art.TextureFilter);
        }

        internal static TMP_FontAsset Font()
        {
            return Load<TMP_FontAsset>(UIGeneratorContent.Art.FontAsset, UIGeneratorContent.Art.FontFilter);
        }

        internal static Material GlowMaterial()
        {
            return Load<Material>(UIGeneratorContent.Art.FontGlowMaterial, UIGeneratorContent.Art.MaterialFilter);
        }

        private static T Load<T>(string assetName, string typeFilter) where T : UnityEngine.Object
        {
            foreach (var guid in AssetDatabase.FindAssets(assetName + typeFilter))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);

                // FindAssets matches substrings ("uipilot_icon_play" would also match a
                // longer name), so insist on the exact file name.
                if (Path.GetFileNameWithoutExtension(path) != assetName) continue;

                var asset = AssetDatabase.LoadAssetAtPath<T>(path);
                if (asset != null) return asset;
            }

            return null;
        }
    }
}
