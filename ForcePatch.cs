using HarmonyLib;
using MelonLoader;
using SteamShelf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace BoxroomPlus.Patches
{
    [HarmonyPatch]
    internal static class MergeCustomGamesPatch
    {
        private static MethodInfo _getOrCreate;

        static MethodBase TargetMethod()
        {
            var type = AccessTools.TypeByName("SteamLibrarySystem");

            if (type == null)
            {
                MelonLogger.Error("SteamLibrarySystem not found!");
                return null;
            }

            _getOrCreate = AccessTools.Method(
                type,
                "GetOrCreate",
                new[] { typeof(int), typeof(bool) });

            if (_getOrCreate == null)
            {
                MelonLogger.Error("SteamLibrarySystem.GetOrCreate not found!");
            }

            var merge = AccessTools.Method(type, "MergeLibraryCacheIds");

            if (merge == null)
            {
                MelonLogger.Error("MergeLibraryCacheIds not found!");
            }

            return merge;
        }

        static void Postfix(List<SteamGameData> games)
        {
            try
            {
                string cacheRoot = Path.Combine(
                    Application.persistentDataPath,
                    "steam_cache_v2");

                if (!Directory.Exists(cacheRoot))
                    return;

                foreach (string dir in Directory.EnumerateDirectories(cacheRoot))
                {
                    string folder = Path.GetFileName(dir);

                    if (!int.TryParse(folder, out int appId))
                        continue;

                    if (!CustomIds.IsCustom(appId))
                        continue;

                    if (!File.Exists(Path.Combine(dir, "meta.json")))
                        continue;

                    bool exists = false;

                    foreach (var game in games)
                    {
                        if (game.AppId == appId)
                        {
                            exists = true;
                            break;
                        }
                    }

                    if (exists)
                        continue;

                    if (_getOrCreate == null)
                        continue;

                    var data = (SteamGameData)_getOrCreate.Invoke(
                        null,
                        new object[] { appId, true });

                    games.Add(data);

                    MelonLogger.Msg($"[Boxroom Plus] Injected custom game {appId}");
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"MergeCustomGamesPatch failed:\n{ex}");
            }
        }
    }
}