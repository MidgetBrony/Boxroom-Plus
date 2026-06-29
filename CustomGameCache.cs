using System.IO;
using UnityEngine;

namespace BoxroomPlus
{
    internal static class CustomIds
    {
        public const int FirstCustomAppId = 900000000;

        public static bool IsCustom(int appId)
            => appId >= FirstCustomAppId;

        public static string SteamCacheRoot =>
            Path.Combine(Application.persistentDataPath, "steam_cache_v2");

        public static string CustomCacheRoot =>
            Path.Combine(Application.persistentDataPath, "custom_cache");

        public static string GetCacheRoot(int appId)
            => IsCustom(appId)
                ? CustomCacheRoot
                : SteamCacheRoot;

        public static string GetGameDirectory(int appId)
            => Path.Combine(GetCacheRoot(appId), appId.ToString());

        public static string GetGameFile(int appId, string fileName)
            => Path.Combine(GetGameDirectory(appId), fileName);
    }
}