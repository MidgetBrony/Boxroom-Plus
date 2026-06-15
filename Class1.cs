using HarmonyLib;
using MelonLoader;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;

[assembly: MelonInfo(typeof(BoxroomPlus.ModMain), "Boxroom Plus", "1.0", "MidgetBrony")]
[assembly: MelonGame("NestedLoop", "BOXROOM")]

namespace BoxroomPlus
{
    public class ModMain : MelonMod
    {
        public override void OnInitializeMelon()
        {
            MelonLogger.Msg("Boxroom Plus Loaded!");

            HarmonyInstance.PatchAll();
        }
    }

    public class LaunchData
    {
        public string Executable { get; set; }
        public string Arguments { get; set; }
        public string WorkingDirectory { get; set; }
        public bool? UseShellExecute { get; set; }
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