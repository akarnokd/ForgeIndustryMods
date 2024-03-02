// Copyright (c) 2022-2024, David Karnok & Contributors
// Licensed under the Apache License, Version 2.0

using BepInEx;
using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using BepInEx.Logging;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Reflection;
using System.IO;
using System;
using TMPro;
using System.Globalization;
using BiteMeGames.Buildings;

namespace FixUnofficialPatches
{
    [BepInPlugin("akarnokd.forgeindustrymods.fixunofficialpatches", "(Fix) Unofficial Patches", PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {

        static ManualLogSource logger;

        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin is loaded!");

            logger = Logger;

            Harmony.CreateAndPatchAll(typeof(Plugin));
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ShipmentReturnedOverview), nameof(ShipmentReturnedOverview.Init))]
        static void ShipmentReturnedOverview_Init(
            Harbor harbor, ref Harbor ____harbor,
            HarborWindow harborWindow, ref HarborWindow ____harborWindow)
        {
            ____harbor = harbor;
            ____harborWindow = harborWindow;
        }
    }
}
