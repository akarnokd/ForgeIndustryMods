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
using Steamworks;
using BepInEx.Configuration;

namespace FixUnofficialPatches
{
    [BepInPlugin("akarnokd.forgeindustrymods.fixunofficialpatches", "(Fix) Unofficial Patches", PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {

        static ManualLogSource logger;

        static ConfigEntry<bool> achievementFix;

        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin is loaded!");

            logger = Logger;

            achievementFix = Config.Bind("General", "AchievementFix", false, "Enable this to apply the achievement unlock fix for 4 broken achievements.");

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

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MarketPlaceWindow), nameof(MarketPlaceWindow.Initialize))]
        static void MarketPlaceWindow_Initialize(GameObject ____orderTab)
        {
            var csf = ____orderTab.GetComponentInChildren<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var sr = ____orderTab.GetComponentInChildren<ScrollRect>();
            sr.scrollSensitivity = 5;
        }

        static readonly HashSet<string> BrokenAchievementIds = 
        [
            "MR_WORLD_WIDE",
            "NEW_VENTURES",
            "SHARPENED_STICK",
            "WELL_ROUNDED"
        ];

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SteamManager), nameof(SteamManager.UnlockAchievement))]
        static void SteamManager_UnlockAchievement(string key)
        {
            if (achievementFix.Value)
            {
                logger.LogInfo("UnlockAchievement " + key);
                if (!BrokenAchievementIds.Contains(key))
                {
                    return;
                }
                logger.LogInfo("UnlockAchievement for broken " + key);
                try
                {

                    SteamUserStats.RequestCurrentStats();

                    logger.LogInfo("  Get SteamUserStats.Internal property value");
                    var iSteamUserStats = AccessTools.PropertyGetter(typeof(SteamUserStats), "Internal").Invoke(null, []);
                    
                    logger.LogInfo("  Get ISteamUserStats.SetAchievement method.");
                    var mSetAchievement = AccessTools.Method(iSteamUserStats.GetType(), "SetAchievement", [typeof(string)]);

                    logger.LogInfo("  Invoke SetAchievement");
                    mSetAchievement.Invoke(iSteamUserStats, [key]);

                    logger.LogInfo("  Storing Stats");
                    SteamUserStats.StoreStats();
                }
                catch (Exception e)
                {
                    logger.LogError(e);
                }
            }
            logger.LogInfo("UnlockAchievement for broken " + key + " - done.");
        }
    }
}
