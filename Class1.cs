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


[assembly: MelonInfo(typeof(BoxroomPlus.ModMain), "Boxroom Plus", "1.5.0-Alpha", "MidgetBrony")]
[assembly: MelonGame("NestedLoop", "BOXROOM")]

namespace BoxroomPlus
{
    public class ModMain : MelonMod
    {
        public override void OnInitializeMelon()
        {
            MelonLogger.Msg("Boxroom Plus Loaded!");
        }
    }
}