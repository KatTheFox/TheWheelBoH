using BepInEx;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx.Logging;
using SecretHistories.UI;
using HarmonyLib;
using SecretHistories.Commands;
using SecretHistories.Constants;
using SecretHistories.Infrastructure;
using SecretHistories.Spheres;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms;

namespace TheWheelBoH
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class TheWheel : BaseUnityPlugin
    {
        public static bool speedreset = true;
        public static float fspeedStep = 0.15f;
        public static float vfspeedStep = 0.15f;
        public static float vvfspeedStep = 0.15f;
        public static string KB1Str = "P";
        public static string KB10Str = "Z";
        public static string KBNextVerb = "Slash";
        public static ResetSpeedOnDaybreakTracker speedresettracker;
        public static FSpeedSettingTracker ftracker;
        public static VFSpeedSettingTracker vftracker;
        public static VVFSpeedSettingTracker vvftracker;
        public static KB1SettingTracker tracker1;
        public static KB10SettingTracker tracker10;
        public static KBNextVerbSettingTracker trackerNextVerb;
        public void Start() => SceneManager.sceneLoaded += new UnityAction<Scene, LoadSceneMode>(AwakePrefix);

        public void OnDestroy() => SceneManager.sceneLoaded -= new UnityAction<Scene, LoadSceneMode>(AwakePrefix);

        public void Update()
        {
            if (((ButtonControl)Keyboard.current[(Key)Enum.Parse(typeof(Key), KB10Str)]).wasPressedThisFrame &&
                !Watchman.Get<LocalNexus>().PlayerInputDisabled())
            {
                for (int index = 0; index < 10; ++index)
                    Watchman.Get<Heart>().Beat(1f, 0.0f);
            }

            if (((ButtonControl)Keyboard.current[(Key)Enum.Parse(typeof(Key), KBNextVerb)]).wasPressedThisFrame &&
                !Watchman.Get<LocalNexus>().PlayerInputDisabled())
            {
                BeatToNextVerb();
            }

            if (((ButtonControl)Keyboard.current[(Key)Enum.Parse(typeof(Key), KB1Str)]).wasPressedThisFrame &&
                !Watchman.Get<LocalNexus>().PlayerInputDisabled())
            {
                Watchman.Get<Heart>().Beat(1f, 0.0f);
            }
        }

        async void BeatToNextVerb()
        {
            
            float timeToFF = GetNextVerbTime();
            Logger.LogWarning("TheWheel: Fast forwarding " + timeToFF + " seconds");
            if (timeToFF > 0.1f)
            {
                await Settler.AwaitSettled();
                Watchman.Get<Heart>().Beat(timeToFF - 0.1f, 0.0f);
                await Settler.AwaitSettled();
                Watchman.Get<Heart>().Beat(0.2f, 0.0f);
                await Settler.AwaitSettled();
            }
            else if (timeToFF != 0.0f)
            {
                await Settler.AwaitSettled();
                Watchman.Get<Heart>().Beat(0.1f, 0.1f);
                await Settler.AwaitSettled();
            }
        }
        bool IsUserFacingVerb(Situation verb)
        {
            return verb.GetAbsolutePath().Path.StartsWith("~/fixedverbs") ||
                   verb.GetAbsolutePath().Path.StartsWith("~/library") ||
                   verb.GetAbsolutePath().Path.StartsWith("~/arrivalverbs");
        }

        public float GetNextVerbTime()
        {
            List<Situation> verbList = Watchman.Get<HornedAxe>().GetRegisteredSituations();
            if (verbList.Count == 0)
                return 0.0f;
            if (!verbList.Any(IsUserFacingVerb))
            {
                return 0.0f;
            }

            float lowest = float.PositiveInfinity;
            Situation lowestVerb = null;
            foreach (Situation verb in verbList)
            {
                if (verb.TimeRemaining < lowest && verb.TimeRemaining >= 0.1f)
                {
                    lowest = verb.TimeRemaining;
                    lowestVerb = verb;
                }
            }

            if (float.IsPositiveInfinity(lowest))
                return 0.0f;
            if (IsUserFacingVerb(lowestVerb))
            {
                Logger.LogInfo("TheWheel: Next verb time is " + lowest + " seconds");
                return lowest;
            }

            Watchman.Get<Heart>().Beat(lowest - 0.1f, 0.1f);
            Watchman.Get<Heart>().Beat(0.2f, 0.1f);
            return GetNextVerbTime();
        }
        
        public void Awake()
        {
            Logger.LogInfo("TheWheel: Initialising");

            Harmony harmony = new Harmony("katthefox.thewheel");
            try
            {
                harmony.Patch(
                    original: GetMethodInvariant(typeof(Heart), "GetTimerMultiplierForSpeed"),
                    prefix: new HarmonyMethod(GetMethodInvariant(typeof(TheWheel),
                        (nameof(TheWheel.GetTimerMultiplierForSpeedPrefix)))));
                harmony.Patch(
                    original: typeof(LocalNexus).GetMethod("BroadcastFx", new Type[] {typeof(EnviroFxCommand)}),
                    prefix: new HarmonyMethod(GetMethodInvariant(typeof(TheWheel),
                        nameof(TheWheel.BroadcastFxPrefix))));
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());
            }
        }
        
        private static bool BroadcastFxPrefix(EnviroFxCommand enviroFxCommand)
        {
            int result;
            if (enviroFxCommand.MatchConcern("meta") && enviroFxCommand.MatchEffect("setspeed") &&
                int.TryParse(enviroFxCommand.Parameter, out result) && result == 1)
            {
                // this is only used in one place- that damn daybreak recipe.
                return TheWheel.speedreset;
            }

            return true;
        }

        MethodInfo GetMethodInvariant(Type definingClass, string methodName)
        {
            if (string.IsNullOrWhiteSpace(methodName))
                Logger.LogWarning($"Trying to find whitespace method for class {definingClass.Name} (don't!)");

            try
            {
                MethodInfo method = definingClass.GetMethod(methodName,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic);

                if (method == null)
                    throw new Exception("Method not found");

                return method;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Failed to find method '{methodName}'  in '{definingClass.Name}', reason: {ex.FormatException()}");
            }
        }

        void AwakePrefix(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "S3MenuUmber")
            {
                try
                {
                    Setting resetSpeedSetting = Watchman.Get<Compendium>().GetEntityById<Setting>("ResetSpeedDaily");
                    if (resetSpeedSetting == null)
                    {
                        Logger.LogWarning("Reset Speed On Daybreak Setting Missing");
                    }
                    else
                    {
                        resetSpeedSetting.AddSubscriber(
                            (ISettingSubscriber)(TheWheel.speedresettracker= new ResetSpeedOnDaybreakTracker()));
                        TheWheel.speedresettracker.WhenSettingUpdated(resetSpeedSetting.CurrentValue);
                    }
                    
                    Setting fSpeedMultSetting = Watchman.Get<Compendium>().GetEntityById<Setting>("FSpeedMultiplier");
                    if (fSpeedMultSetting == null)
                    {
                        Logger.LogWarning("Fast Speed Multiplier Setting Missing");
                    }
                    else
                    {
                        fSpeedMultSetting.AddSubscriber(
                                (ISettingSubscriber)(TheWheel.ftracker = new FSpeedSettingTracker()))
                            ;
                        TheWheel.ftracker.WhenSettingUpdated(fSpeedMultSetting.CurrentValue);
                    }
                    
                    Setting vfSpeedMultSetting = Watchman.Get<Compendium>().GetEntityById<Setting>("VFSpeedMultiplier");
                    if (vfSpeedMultSetting == null)
                    {
                        Logger.LogWarning("Very Fast Speed Multiplier Setting Missing");
                    }
                    else
                    {
                        vfSpeedMultSetting.AddSubscriber(
                                (ISettingSubscriber)(TheWheel.vftracker = new VFSpeedSettingTracker()))
                            ;
                        TheWheel.vftracker.WhenSettingUpdated(vfSpeedMultSetting.CurrentValue);
                    }
                    
                    Setting vvfSpeedMultSetting = Watchman.Get<Compendium>().GetEntityById<Setting>("VVFSpeedMultiplier");
                    if (vvfSpeedMultSetting == null)
                    {
                        Logger.LogWarning("Very Very Fast Speed Multiplier Setting Missing");
                    }
                    else
                    {
                        vvfSpeedMultSetting.AddSubscriber(
                                (ISettingSubscriber)(TheWheel.vvftracker = new VVFSpeedSettingTracker()))
                            ;
                        TheWheel.vvftracker.WhenSettingUpdated(vvfSpeedMultSetting.CurrentValue);
                    }

                    Setting kb1Sec = Watchman.Get<Compendium>().GetEntityById<Setting>("ff1sec");
                    if (kb1Sec == null)
                    {
                        Logger.LogWarning("one second keybind missing");
                    }
                    else
                    {
                        kb1Sec.AddSubscriber((ISettingSubscriber)(TheWheel.tracker1 = new KB1SettingTracker()))
                            ;
                        TheWheel.tracker1.WhenSettingUpdated(kb1Sec.CurrentValue);
                    }

                    Setting kb10Sec = Watchman.Get<Compendium>().GetEntityById<Setting>("ff10sec");
                    if (kb10Sec == null)
                    {
                        Logger.LogWarning("ten second keybind missing");
                    }
                    else
                    {
                        kb10Sec.AddSubscriber((ISettingSubscriber)(TheWheel.tracker10 = new KB10SettingTracker()))
                            ;
                        TheWheel.tracker10.WhenSettingUpdated(kb10Sec.CurrentValue);
                    }

                    Setting kbNextVerb = Watchman.Get<Compendium>().GetEntityById<Setting>("ffNextVerb");
                    if (kbNextVerb == null)
                    {
                        Logger.LogWarning("next verb keybind missing");
                    }
                    else
                    {
                        kbNextVerb.AddSubscriber(trackerNextVerb = new KBNextVerbSettingTracker())
                            ;
                        trackerNextVerb.WhenSettingUpdated(kbNextVerb.CurrentValue);
                    }
                }
                catch (Exception e)
                {
                    Logger.LogError(e.ToString());
                }
            }
        }

        private static bool GetTimerMultiplierForSpeedPrefix(Heart __instance, GameSpeed speed, ref GameSpeedState ___gameSpeedState,
            ref float __result)
        {
            switch (speed)
            {
                case GameSpeed.Paused:
                    __result = 0.0f;
                    break;
                case GameSpeed.Normal:
                    __result = 1.0f;
                    break;
                case GameSpeed.Fast:
                    __result = TheWheel.fspeedStep;
                    break;
                case GameSpeed.VeryFast:
                    __result = TheWheel.vfspeedStep;
                    break;
                case GameSpeed.VeryVeryFast:
                    __result = TheWheel.vvfspeedStep;
                    break;
                default:
                    NoonUtility.Log("Unknown game speed state: " + ___gameSpeedState.GetEffectiveGameSpeed().ToString());
                    __result=0.0f;
                    break;
            }

            return false;
        }
    }
}