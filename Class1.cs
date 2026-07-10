using BoxroomPlus;
using HarmonyLib;
using MelonLoader;
using Newtonsoft.Json;
using SteamShelf;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;


[assembly: MelonInfo(typeof(BoxroomPlus.ModMain), "Boxroom Plus", "1.4.0", "MidgetBrony")]
[assembly: MelonGame("NestedLoop", "BOXROOM")]

namespace BoxroomPlus
{
    public class ModMain : MelonMod
    {
        public override void OnInitializeMelon()
        {
            MelonLogger.Msg("Boxroom Plus Loaded!");

            var type = AccessTools.TypeByName("SteamShelf.SteamGameCache");

            MelonLogger.Msg(type?.FullName ?? "NULL");
        }

    }

    public class LaunchData
    {
        public string Executable { get; set; }
        public string Arguments { get; set; }
        public string WorkingDirectory { get; set; }
        public bool? UseShellExecute { get; set; }
    }


    [HarmonyPatch]
    internal class SteamSubscribedPatch
    {
        static MethodBase TargetMethod()
        {
            var method = AccessTools.Method(
                typeof(SteamApps),
                nameof(SteamApps.IsSubscribedToApp),
                new[] { typeof(AppId) });

            if (method == null)
            {
                MelonLogger.Error("SteamApps.IsSubscribedToApp(AppId) not found!");
                return null;
            }

            MelonLogger.Msg("Patching SteamApps.IsSubscribedToApp(AppId)");
            return method;
        }

        static bool Prefix(AppId appid, ref bool __result)
        {
            int appId = (int)(uint)appid;

            if (CustomIds.IsCustom(appId))
            {
                MelonLogger.Msg($"Pretending custom AppID {appId} is owned.");
                __result = true;
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch]
    internal class InvalidateGamePatch
    {
        static MethodBase TargetMethod()
        {
            var type = AccessTools.TypeByName("SteamGameCache");

            if (type == null)
            {
                MelonLogger.Error("SteamGameCache not found!");
                return null;
            }

            var method = AccessTools.Method(type, "InvalidateGame");

            if (method == null)
            {
                MelonLogger.Error("InvalidateGame not found!");
                return null;
            }

            MelonLogger.Msg("Patching SteamGameCache.InvalidateGame");

            return method;
        }

        static bool Prefix(int appId)
        {
            MelonLogger.Msg($"InvalidateGame called: {appId}");

            if (CustomIds.IsCustom(appId))
            {
                MelonLogger.Msg($"Skipping cache invalidation for {appId}");
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch]
    internal class InvalidateGameMetaPatch
    {
        static MethodBase TargetMethod()
        {
            var type = AccessTools.TypeByName("SteamGameCache");

            if (type == null)
            {
                MelonLogger.Error("SteamGameCache type not found!");
                return null;
            }

            var method = AccessTools.Method(type, "InvalidateGameMeta");

            if (method == null)
            {
                MelonLogger.Error("InvalidateGameMeta method not found!");
                return null;
            }

            MelonLogger.Msg("Patching SteamGameCache.InvalidateGameMeta");

            return method;
        }

        static bool Prefix(int appId)
        {
            MelonLogger.Msg($"InvalidateGameMeta called: {appId}");

            if (CustomIds.IsCustom(appId))
            {
                MelonLogger.Msg($"Skipping meta invalidation for custom game {appId}");
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(BoxInspector), "LaunchGame")]
    class LaunchPatch
    {
        static bool Prefix(BoxInspector __instance)
        {
            try
            {
                var appId = __instance.heldInfo.AppId;

                string cachePath =
                    Path.Combine(
                        Environment.GetFolderPath(
                            Environment.SpecialFolder.LocalApplicationData),
                        "..",
                        "LocalLow",
                        "NestedLoop",
                        "BOXROOM",
                        "steam_cache_v2",
                        appId.ToString());

                cachePath = Path.GetFullPath(cachePath);

                string launchJson =
                    Path.Combine(cachePath, "launch.json");

                if (File.Exists(launchJson))
                {
                    MelonLogger.Msg($"Custom launcher found for {appId}");

                    var launchData =
                        JsonConvert.DeserializeObject<LaunchData>(
                            File.ReadAllText(launchJson));

                    if (!string.IsNullOrWhiteSpace(launchData.Executable))
                    {
                        var startInfo = new ProcessStartInfo
                        {
                            FileName = launchData.Executable,
                            Arguments = launchData.Arguments ?? "",
                            UseShellExecute = launchData.UseShellExecute ?? true
                        };

                        if (!string.IsNullOrWhiteSpace(launchData.WorkingDirectory))
                        {
                            startInfo.WorkingDirectory = launchData.WorkingDirectory;
                        }
                        else if (File.Exists(launchData.Executable))
                        {
                            startInfo.WorkingDirectory =
                                Path.GetDirectoryName(launchData.Executable);
                        }

                        MelonLogger.Msg($"Launching: {startInfo.FileName}");
                        MelonLogger.Msg($"Arguments: {startInfo.Arguments}");
                        MelonLogger.Msg($"WorkingDirectory: {startInfo.WorkingDirectory}");

                        Process.Start(startInfo);
                        return false;
                    }
                }

                MelonLogger.Msg(
                    $"No custom launcher found. Falling back to Steam.");

                return true;
            }
            catch (Exception ex)
            {
                MelonLogger.Error(ex.ToString());

                return true;
            }
        }
    }
}