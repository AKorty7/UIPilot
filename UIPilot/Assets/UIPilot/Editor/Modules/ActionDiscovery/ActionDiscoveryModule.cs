using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace UIPilot.Editor.Modules.ActionDiscovery
{
    internal static class ActionDiscoveryModule
    {
        private static readonly BindingFlags ScanFlags =
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;

        // Unity message methods a MonoBehaviour may declare but are not user actions.
        private static readonly HashSet<string> LifecycleMethods = new HashSet<string>
        {
            "Awake", "OnEnable", "Start", "OnDisable", "OnDestroy",
            "Update", "FixedUpdate", "LateUpdate",
            "OnGUI",
            "OnApplicationFocus", "OnApplicationPause", "OnApplicationQuit",
            "OnBecameVisible", "OnBecameInvisible",
            "OnTriggerEnter", "OnTriggerExit", "OnTriggerStay",
            "OnCollisionEnter", "OnCollisionExit", "OnCollisionStay",
            "OnTriggerEnter2D", "OnTriggerExit2D", "OnTriggerStay2D",
            "OnCollisionEnter2D", "OnCollisionExit2D", "OnCollisionStay2D",
            "OnMouseDown", "OnMouseUp", "OnMouseEnter", "OnMouseExit",
            "OnMouseOver", "OnMouseDrag", "OnMouseUpAsButton",
            "OnTransformChildrenChanged", "OnTransformParentChanged",
            "OnValidate", "Reset",
            "OnDrawGizmos", "OnDrawGizmosSelected",
            "OnAnimatorIK", "OnAnimatorMove",
            "OnParticleCollision", "OnParticleTrigger",
            "OnPreCull", "OnPreRender", "OnPostRender",
            "OnRenderObject", "OnWillRenderObject", "OnRenderImage",
            "OnAudioFilterRead",
            "OnJointBreak", "OnJointBreak2D",
            "OnControllerColliderHit",
        };

        // ── Public entry point ───────────────────────────────────────────────

        internal static List<DiscoveredAction> Scan()
        {
            var results = new List<DiscoveredAction>();

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.IsDynamic)            continue;
                if (IsExcludedAssembly(assembly))  continue;

                Type[] types;
                try   { types = assembly.GetTypes(); }
                catch { continue; }

                foreach (var type in types)
                {
                    if (!type.IsClass || type.IsAbstract)               continue;
                    if (!typeof(MonoBehaviour).IsAssignableFrom(type))  continue;

                    foreach (var method in type.GetMethods(ScanFlags))
                    {
                        if (!IsValidAction(method)) continue;
                        results.Add(new DiscoveredAction(type.Name, method.Name));
                    }
                }
            }

            results.Sort((a, b) =>
                string.Compare(a.FullLabel, b.FullLabel, StringComparison.Ordinal));

            return results;
        }

        // ── Filters ──────────────────────────────────────────────────────────

        private static bool IsValidAction(MethodInfo method)
        {
            if (method.IsSpecialName)                 return false; // property accessors, operators
            if (method.GetParameters().Length != 0)   return false;
            if (LifecycleMethods.Contains(method.Name)) return false;
            return true;
        }

        private static bool IsExcludedAssembly(Assembly assembly)
        {
            var name = assembly.GetName().Name;
            return name.StartsWith("UnityEngine",   StringComparison.Ordinal)
                || name.StartsWith("UnityEditor",   StringComparison.Ordinal)
                || name.StartsWith("Unity.",         StringComparison.Ordinal)
                || name.StartsWith("com.unity",      StringComparison.Ordinal)
                || name.StartsWith("System",         StringComparison.Ordinal)
                || name.StartsWith("mscorlib",       StringComparison.Ordinal)
                || name.StartsWith("Mono.",          StringComparison.Ordinal)
                || name.StartsWith("nunit.",         StringComparison.Ordinal)
                || name.StartsWith("JetBrains.",     StringComparison.Ordinal)
                || name.StartsWith("Microsoft.",     StringComparison.Ordinal)
                || name.StartsWith("Newtonsoft.",    StringComparison.Ordinal)
                || name.StartsWith("ExCSS.",         StringComparison.Ordinal)
                || name.StartsWith("Bee.",           StringComparison.Ordinal)
                || name == "netstandard";
        }
    }
}
