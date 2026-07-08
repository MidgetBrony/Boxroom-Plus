using MelonLoader;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

internal static class UnityImageLoader
{
    private static MethodInfo LoadImageMethod;

    static UnityImageLoader()
    {
        try
        {
            Assembly asm = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == "UnityEngine.ImageConversionModule");

            if (asm == null)
                return;

            Type imageConversion = asm.GetType("UnityEngine.ImageConversion");

            if (imageConversion == null)
                return;

            LoadImageMethod = imageConversion.GetMethod(
                "LoadImage",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new Type[]
                {
                    typeof(Texture2D),
                    typeof(byte[])
                },
                null);

            if (LoadImageMethod == null)
            {
                // Unity 6 overload
                LoadImageMethod = imageConversion.GetMethod(
                    "LoadImage",
                    BindingFlags.Public | BindingFlags.Static);
            }

            MelonLogger.Msg("ImageConversion reflection initialized.");
        }
        catch (Exception ex)
        {
            MelonLogger.Warning(ex.ToString());
        }
    }

    public static bool LoadImage(Texture2D texture, byte[] bytes)
    {
        if (LoadImageMethod == null)
            return false;

        try
        {
            object[] args;

            if (LoadImageMethod.GetParameters().Length == 2)
                args = new object[] { texture, bytes };
            else
                args = new object[] { texture, bytes, false };

            return (bool)LoadImageMethod.Invoke(null, args);
        }
        catch (Exception ex)
        {
            MelonLogger.Warning(ex.ToString());
            return false;
        }
    }
}