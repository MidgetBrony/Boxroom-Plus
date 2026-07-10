using HarmonyLib;
using MelonLoader;
using Newtonsoft.Json;
using SteamShelf;
using SteamShelf.Placeables;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    internal static class PlatformIconManager
    {
        private static readonly Dictionary<string, Texture2D> Cache =
            new Dictionary<string, Texture2D>(StringComparer.OrdinalIgnoreCase);

        private static readonly MethodInfo LoadImageMethod;

        static PlatformIconManager()
        {
            try
            {
                Assembly asm = AppDomain.CurrentDomain
                    .GetAssemblies()
                    .FirstOrDefault(a => a.GetName().Name == "UnityEngine.ImageConversionModule");

                if (asm == null)
                {
                    MelonLogger.Warning("[Boxroom Plus] ImageConversionModule not loaded.");
                    return;
                }

                Type imageConversion = asm.GetType("UnityEngine.ImageConversion");

                if (imageConversion == null)
                {
                    MelonLogger.Warning("[Boxroom Plus] UnityEngine.ImageConversion not found.");
                    return;
                }

                // Unity usually exposes:
                // LoadImage(Texture2D, byte[])
                // or
                // LoadImage(Texture2D, byte[], bool)

                foreach (MethodInfo method in imageConversion.GetMethods(BindingFlags.Public | BindingFlags.Static))
                {
                    if (method.Name != "LoadImage")
                        continue;

                    ParameterInfo[] p = method.GetParameters();

                    if (p.Length >= 2 &&
                        p[0].ParameterType == typeof(Texture2D) &&
                        p[1].ParameterType == typeof(byte[]))
                    {
                        LoadImageMethod = method;
                        break;
                    }
                }

                if (LoadImageMethod != null)
                    MelonLogger.Msg("[Boxroom Plus] ImageConversion.LoadImage hooked via reflection.");
                else
                    MelonLogger.Warning("[Boxroom Plus] Failed to locate ImageConversion.LoadImage.");
            }
            catch (Exception ex)
            {
                MelonLogger.Warning(ex.ToString());
            }
        }

        public static Texture2D GetPlatformIcon(int appId)
        {
            HelperMeta helper = HelperManager.Get(appId);

            string platform = "steam";

            if (helper != null && !string.IsNullOrWhiteSpace(helper.Platform))
                platform = helper.Platform.ToLowerInvariant();

            Texture2D texture;

            if (Cache.TryGetValue(platform, out texture))
                return texture;

            texture = LoadEmbedded(platform);

            if (texture == null && platform != "steam")
                texture = LoadEmbedded("steam");

            if (texture != null)
                Cache[platform] = texture;

            return texture;
        }

        private static Texture2D LoadEmbedded(string platform)
        {
            Assembly asm = Assembly.GetExecutingAssembly();

            string resource =
                "BoxroomPlus.Resources.PlatformIcons." +
                platform +
                ".png";

            using (Stream stream = asm.GetManifestResourceStream(resource))
            {
                if (stream == null)
                    return null;

                byte[] bytes = new byte[stream.Length];
                stream.Read(bytes, 0, bytes.Length);

                Texture2D tex = new Texture2D(2, 2);

                if (!LoadTexture(tex, bytes))
                {
                    UnityEngine.Object.Destroy(tex);
                    return null;
                }

                tex.wrapMode = TextureWrapMode.Clamp;
                tex.filterMode = FilterMode.Bilinear;
                tex.Apply(false, true);

                return tex;
            }
        }

        private static bool LoadTexture(Texture2D texture, byte[] bytes)
        {
            if (LoadImageMethod == null)
                return false;

            try
            {
                ParameterInfo[] p = LoadImageMethod.GetParameters();

                object result;

                if (p.Length == 2)
                {
                    result = LoadImageMethod.Invoke(null, new object[]
                    {
                    texture,
                    bytes
                    });
                }
                else
                {
                    result = LoadImageMethod.Invoke(null, new object[]
                    {
                    texture,
                    bytes,
                    false
                    });
                }

                return result is bool && (bool)result;
            }
            catch (Exception ex)
            {
                MelonLogger.Warning(ex.ToString());
                return false;
            }
        }
    }
    internal static class HelperManager
    {
        private const string CacheRootV2 = "steam_cache_v2";
        private const string HelperFile = "meta.helper.json";

        private static readonly Dictionary<Material, Color> OriginalColors = new Dictionary<Material, Color>();
        private static readonly Dictionary<int, HelperMeta> Helpers = new Dictionary<int, HelperMeta>();

        private static readonly Dictionary<string, string> PlatformColors = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "steam", "#2A475E" },
                { "gog", "#7B2CBF" },
                { "epic", "#313131" },
                { "ea", "#FF5A00" },
                { "ubisoft", "#00AEEF" },
                { "itch", "#FA5C5C" },
                { "emulator", "#D4AF37" },
                { "riot", "#D13639" },
                { "rom", "#D4AF37" },
                { "custom", "#D4AF37" }
            };

        public static HelperMeta Get(int appId)
        {
            if (Helpers.TryGetValue(appId, out var helper))
                return helper;

            helper = Load(appId);

            if (helper != null)
                Helpers[appId] = helper;

            return helper;
        }

        public static void ApplyAppearance(Renderer renderer, int appId)
        {
            ApplyCaseColor(renderer, appId);

            // Future:
             ApplyPlatformIcon(renderer, appId);
        }

        private static void ApplyPlatformIcon(Renderer renderer, int appId)
        {
            if (renderer == null)
                return;

            Texture2D icon = PlatformIconManager.GetPlatformIcon(appId);

            if (icon == null)
                return;

            MaterialHelpers.SetTexture(renderer, 3, icon);
        }

        public static void ApplyCaseColor(Renderer renderer, int appId)
        {
            try
            {
                if (renderer == null)
                    return;

                ApplyColorToRenderer(renderer);

                var helper = Get(appId);

                if (helper == null)
                    return;

                string colorHex = helper.CaseColor;

                if (string.IsNullOrWhiteSpace(colorHex) &&
                    !string.IsNullOrWhiteSpace(helper.Platform) &&
                    !helper.Platform.Equals("steam", StringComparison.OrdinalIgnoreCase))
                {
                    PlatformColors.TryGetValue(helper.Platform, out colorHex);
                }

                if (string.IsNullOrWhiteSpace(colorHex))
                    return;

                if (!ColorUtility.TryParseHtmlString(colorHex, out Color color))
                    return;

                ApplyColorToRenderer(renderer, color);
            }
            catch (Exception ex)
            {
                MelonLogger.Warning("[Boxroom Plus] ApplyAppearance failed: " + ex);
            }
        }

        private static HelperMeta Load(int appId)
        {
            string helperPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "AppData", "LocalLow", "NestedLoop", "BOXROOM",
                CacheRootV2, appId.ToString(), HelperFile);

            if (!File.Exists(helperPath))
                return null;

            return JsonConvert.DeserializeObject<HelperMeta>(File.ReadAllText(helperPath));
        }

        private static void ApplyColorToRenderer(Renderer renderer, Color? overrideColor = null)
        {
            Material mat = renderer.material;
            if (mat == null)
                return;

            if (!OriginalColors.ContainsKey(mat))
            {
                if (mat.HasProperty("_Color"))
                    OriginalColors[mat] = mat.color;
                else if (mat.HasProperty("_BaseColor"))
                    OriginalColors[mat] = mat.GetColor("_BaseColor");
                else
                    OriginalColors[mat] = Color.white;
            }

            Color original = OriginalColors[mat];

            if (mat.HasProperty("_Color"))
                mat.color = original;

            if (mat.HasProperty("_BaseColor"))
                mat.SetColor("_BaseColor", original);

            if (!overrideColor.HasValue)
                return;

            if (mat.HasProperty("_Color"))
                mat.color = overrideColor.Value;

            if (mat.HasProperty("_BaseColor"))
                mat.SetColor("_BaseColor", overrideColor.Value);
        }
    }
}

namespace Boxroom_Plus.Patches
{
    [HarmonyPatch(typeof(Box), "HandleMetadataReady")]
    internal static class BoxInspectAppearancePatch
    {
        private static readonly FieldInfo BoxRendererField =
            AccessTools.Field(typeof(Box), "boxRenderer");

        static void Postfix(Box __instance, SteamGameData game)
        {
            if (__instance == null || game == null)
                return;

            Renderer renderer = BoxRendererField?.GetValue(__instance) as Renderer;

            HelperManager.ApplyAppearance(renderer, game.AppId);
        }
    }

    [HarmonyPatch(typeof(PlacedBoxProp), "HandleMetadataReady")]
    internal static class PlacedBoxPropAppearancePatch
    {
        private static readonly FieldInfo RendField =
            AccessTools.Field(typeof(PlacedBoxProp), "rend");

        static void Postfix(PlacedBoxProp __instance, SteamGameData game)
        {
            if (__instance == null || game == null)
                return;

            Renderer renderer = RendField?.GetValue(__instance) as Renderer;

            HelperManager.ApplyAppearance(renderer, game.AppId);
        }
    }

    [HarmonyPatch(typeof(ShelfBox), "HandleMetadataReady")]
    internal static class ShelfBoxAppearancePatch
    {
        static void Postfix(ShelfBox __instance, SteamGameData game)
        {
            if (__instance == null || game == null)
                return;

            HelperManager.ApplyAppearance(__instance.rend, game.AppId);
        }
    }
}
