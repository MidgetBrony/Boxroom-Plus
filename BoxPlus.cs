using HarmonyLib;
using MelonLoader;
using Newtonsoft.Json;
using SteamShelf;
using SteamShelf.Placeables;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace Boxroom_Plus
{
    internal class HelperMeta
    {
        public string Type = "game";
        public string Platform = "steam";
        public string CaseColor = "";
    }

    internal static class HelperManager
    {
        private const string CacheRootV2 = "steam_cache_v2";
        private const string HelperFile = "meta.helper.json";

        private static readonly Dictionary<int, HelperMeta> Helpers =
            new Dictionary<int, HelperMeta>();

        private static readonly Dictionary<string, string> PlatformColors =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "steam", "#2A475E" },
            { "gog", "#7B2CBF" },
            { "epic", "#313131" },
            { "ea", "#FF5A00" },
            { "ubisoft", "#00AEEF" },
            { "itch", "#FA5C5C" },
            { "emulator", "#D4AF37" },
            { "rom", "#D4AF37" },
            { "custom", "#D4AF37" }
        };

        public static HelperMeta Get(int appId)
        {
            HelperMeta helper;

            if (Helpers.TryGetValue(appId, out helper))
                return helper;

            helper = Load(appId);

            if (helper != null)
                Helpers[appId] = helper;

            return helper;
        }

        public static void Set(int appId, HelperMeta helper)
        {
            if (helper == null)
                return;

            Helpers[appId] = helper;
        }

        public static void Clear(int appId)
        {
            if (Helpers.ContainsKey(appId))
                Helpers.Remove(appId);
        }

        public static void ApplyCaseColor(Renderer renderer, int appId)
        {
            try
            {
                if (renderer == null)
                    return;

                HelperMeta helper = Get(appId);

                if (helper == null)
                    return;

                string colorHex = helper.CaseColor;

                if (string.IsNullOrWhiteSpace(colorHex) &&
                    !string.IsNullOrWhiteSpace(helper.Platform))
                {
                    string platformColor;

                    if (PlatformColors.TryGetValue(helper.Platform, out platformColor))
                        colorHex = platformColor;
                }

                if (string.IsNullOrWhiteSpace(colorHex))
                    return;

                Color color;

                if (!ColorUtility.TryParseHtmlString(colorHex, out color))
                {
                    MelonLogger.Warning("[Boxroom Plus] Invalid CaseColor '" + colorHex + "' for AppID " + appId);
                    return;
                }

                ApplyColorToRenderer(renderer, color);
            }
            catch (Exception ex)
            {
                MelonLogger.Warning("[Boxroom Plus] ApplyCaseColor failed for AppID " + appId + ": " + ex);
            }
        }

        private static HelperMeta Load(int appId)
        {
            string helperPath = GetHelperPath(appId);

            if (string.IsNullOrEmpty(helperPath) || !File.Exists(helperPath))
                return null;

            try
            {
                HelperMeta helper = JsonConvert.DeserializeObject<HelperMeta>(
                    File.ReadAllText(helperPath));

                return helper;
            }
            catch (Exception ex)
            {
                MelonLogger.Warning("[Boxroom Plus] Failed to read meta.helper.json for AppID " + appId + ": " + ex.Message);
                return null;
            }
        }

        private static string GetHelperPath(int appId)
        {
            string root = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "AppData",
                "LocalLow",
                "NestedLoop",
                "BOXROOM",
                CacheRootV2);

            return Path.Combine(root, appId.ToString(), HelperFile);
        }

        private static void ApplyColorToRenderer(Renderer renderer, Color color)
        {
            Material mat = renderer.material;

            if (mat == null)
                return;

            // Built-in/Standard style shader.
            if (mat.HasProperty("_Color"))
                mat.color = color;

            // URP/HDRP style shader.
            if (mat.HasProperty("_BaseColor"))
                mat.SetColor("_BaseColor", color);
        }
    }
}

namespace Boxroom_Plus.Patches
{
    [HarmonyPatch(typeof(Box), nameof(Box.ApplyData))]
    internal static class BoxInspectCaseColorPatch
    {
        private static readonly FieldInfo BoxRendererField =
            AccessTools.Field(typeof(Box), "boxRenderer");

        private static void Postfix(Box __instance, SteamGameData game)
        {
            try
            {
                if (__instance == null || game == null || BoxRendererField == null)
                    return;

                Renderer renderer = BoxRendererField.GetValue(__instance) as Renderer;

                HelperManager.ApplyCaseColor(renderer, game.AppId);
            }
            catch (Exception ex)
            {
                MelonLogger.Warning("[Boxroom Plus] BoxInspectCaseColorPatch failed: " + ex);
            }
        }
    }

    [HarmonyPatch(typeof(PlacedBoxProp), "HandleMetadataReady")]
    internal static class PlacedBoxPropCaseColorPatch
    {
        private static readonly FieldInfo RendField =
            AccessTools.Field(typeof(PlacedBoxProp), "rend");

        private static void Postfix(PlacedBoxProp __instance, SteamGameData game)
        {

            MelonLogger.Msg("[PlacedBoxProp] " + game.AppId);
            try
            {
                if (__instance == null || game == null || RendField == null)
                    return;

                Renderer renderer = RendField.GetValue(__instance) as Renderer;

                HelperManager.ApplyCaseColor(renderer, game.AppId);
            }
            catch (Exception ex)
            {
                MelonLogger.Warning("[Boxroom Plus] PlacedBoxPropCaseColorPatch failed: " + ex);
            }
        }
    }

    [HarmonyPatch(typeof(ShelfBox), "HandleMetadataReady")]
    internal static class ShelfBoxCaseColorPatch
    {
        static void Postfix(ShelfBox __instance, SteamGameData game)
        {
            if (__instance == null || game == null)
                return;

            HelperManager.ApplyCaseColor(__instance.rend, game.AppId);
        }
    }
}
