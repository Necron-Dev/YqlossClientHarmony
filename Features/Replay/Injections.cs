using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using ADOFAI;
using HarmonyLib;
using MonsterLove.StateMachine;
using SkyHook;
using UnityEngine;
using UnityEngine.EventSystems;
using EventType = SkyHook.EventType;

namespace YqlossClientHarmony.Features.Replay;

public static class Injections
{
    private static long? TickToDspOffset { get; set; }

    private static Dictionary<KeyCode, bool> SyncKeyDownMap { get; } = [];

    private static bool IsInSwitchChosen { get; set; }

    private static bool IsInMainIgnoreActiveAndPlayingReplay { get; set; }

    private static ConcurrentQueue<SkyHookEvent> KeyQueue { get; } = [];

    public static double TickToDsp(long ticks)
    {
        var tickToDspOffsetNullable = TickToDspOffset;
        long tickToDspOffset;

        if (tickToDspOffsetNullable is null)
        {
            List<long> samples = [];

            for (var i = 0; i < 10; i++)
                samples.Add((long)(AudioSettings.dspTime * 10000000.0) - DateTime.Now.Ticks);

            samples.Sort();
            samples.RemoveRange(8, 2);
            samples.RemoveRange(0, 2);

            var total = (long)0;

            foreach (var sample in samples) total += sample;

            TickToDspOffset = tickToDspOffset = total / samples.Count;
        }
        else
        {
            tickToDspOffset = tickToDspOffsetNullable.Value;
        }

        return (ticks + tickToDspOffset) / 10000000.0;
    }

    public static double DspToSong(double dsp, double offset)
    {
        return Adofai.Conductor.song.pitch * (
                   dsp
                   - Adofai.Conductor.dspTimeSongPosZero
                   - scrConductor.calibration_i
                   + offset
               )
               - Adofai.Conductor.adjustedCountdownTicks * Adofai.Conductor.crotchetAtStart
               + Adofai.Conductor.addoffset;
    }

    [HarmonyPatch(typeof(scnGame), nameof(scnGame.Play))]
    public static class Inject_scnGame_Play
    {
        public static void Prefix(
            int seqID
        )
        {
            if (!Settings.Instance.Enabled) return;
            if (ADOBase.isOfficialLevel) return;
            if (RDC.auto)
            {
                Main.Mod.Logger.Log("skipping recording and playing replay: auto mode");
                return;
            }

            SyncKeyDownMap.Clear();
            TickToDspOffset = null;
            ReplayRecorder.StartRecording(seqID);
            ReplayPlayer.StartPlaying(seqID);
        }
    }

    [HarmonyPatch(typeof(scrController), nameof(scrController.FailAction))]
    public static class Inject_scrController_FailAction
    {
        public static void Prefix(
            scrController __instance,
            bool hitbox = false
        )
        {
            if (!__instance.gameworld || ADOBase.isOfficialLevel) return;

            if (
                !hitbox && (
                    RDC.auto ||
                    (
                        __instance.currFloor.nextfloor != null &&
                        __instance.currFloor.nextfloor.auto &&
                        !RDC.useOldAuto
                    ) ||
                    (!__instance.gameworld && !__instance.currFloor.freeroam) ||
                    __instance.noFail ||
                    (__instance.currFloor.isSafe && GCS.hitMarginLimit == HitMarginLimit.None)
                )
            ) return;

            ReplayRecorder.EndRecording();
            ReplayPlayer.EndPlaying();
        }
    }

    [HarmonyPatch(typeof(scrController), nameof(scrController.QuitToMainMenu))]
    public static class Inject_scrController_QuitToMainMenu
    {
        public static void Prefix()
        {
            ReplayPlayer.UnloadReplay();
            if (!Adofai.Controller.gameworld || ADOBase.isOfficialLevel) return;
            ReplayRecorder.EndRecording();
            ReplayPlayer.EndPlaying();
        }
    }

    [HarmonyPatch(typeof(scrController), nameof(scrController.OnLandOnPortal))]
    public static class Inject_scrController_OnLandOnPortal
    {
        public static void Prefix()
        {
            if (!Adofai.Controller.gameworld || ADOBase.isOfficialLevel) return;
            ReplayRecorder.EndRecording();
            ReplayPlayer.EndPlaying();
        }
    }

    [HarmonyPatch(typeof(scnEditor), "ResetScene")]
    public static class Inject_scnEditor_ResetScene
    {
        public static void Prefix()
        {
            if (!Adofai.Controller.gameworld || ADOBase.isOfficialLevel) return;
            ReplayRecorder.EndRecording();
            ReplayPlayer.EndPlaying();
        }
    }

    [HarmonyPatch(typeof(scrMistakesManager), nameof(scrMistakesManager.AddHit))]
    public static class Inject_scrMistakesManager_AddHit
    {
        public static void Prefix(
            ref HitMargin hit
        )
        {
            if (Interoperation.ReplayIgnoreJudgement) return;
            if (!Adofai.Controller.gameworld || ADOBase.isOfficialLevel) return;
            ReplayRecorder.OnHitMargin(hit);
            ReplayPlayer.OnHitMargin(ref hit);
        }
    }

    [HarmonyPatch(typeof(scrPlanet), nameof(scrPlanet.SwitchChosen))]
    public static class Inject_scrPlanet_SwitchChosen
    {
        public static void Prefix()
        {
            IsInSwitchChosen = true;
        }
    }

    [HarmonyPatch(typeof(scrMisc), nameof(scrMisc.GetHitMargin))]
    public static class Inject_scrMisc_GetHitMargin
    {
        public static void Postfix(
            ref HitMargin __result
        )
        {
            if (!IsInSwitchChosen) return;
            IsInSwitchChosen = false;
            if (Interoperation.ReplayIgnoreJudgement) return;
            if (!Adofai.Controller.gameworld || ADOBase.isOfficialLevel) return;
            ReplayPlayer.OnGetHitMargin(ref __result);
        }
    }

    [HarmonyPatch(typeof(scrHitErrorMeter), nameof(scrHitErrorMeter.AddHit))]
    public static class Inject_scrHitErrorMeter_AddHit
    {
        public static void Prefix(
            ref float angleDiff
        )
        {
            if (Interoperation.ReplayIgnoreJudgement) return;
            if (!Adofai.Controller.gameworld || ADOBase.isOfficialLevel) return;
            double result = angleDiff;
            ReplayRecorder.OnErrorMeter(result);
            ReplayPlayer.OnErrorMeter(ref result);
            angleDiff = (float)result;
        }
    }

    [HarmonyPatch(typeof(scrController), "UpdateInput")]
    public static class Inject_scrController_UpdateInput
    {
        private static AccessTools.FieldRef<StateEngine, StateMapping> DestinationStateField { get; } =
            AccessTools.FieldRefAccess<StateEngine, StateMapping>("destinationState");

        public static void Prefix()
        {
            var replayToRecord = ReplayRecorder.Replay;

            if (
                replayToRecord == null ||
                !Adofai.Controller.gameworld ||
                ADOBase.isOfficialLevel ||
                Adofai.Controller.paused ||
                !AsyncInputManager.isActive ||
                !Persistence.GetChosenAsynchronousInput() ||
                !Application.isFocused ||
                (Adofai.CurrentFloorId <= replayToRecord.Metadata.StartingFloorId && (
                    Adofai.Controller.state != States.PlayerControl ||
                    (States)DestinationStateField(Adofai.Controller.stateMachine).state != States.PlayerControl ||
                    !Adofai.Controller.responsive
                ))
            )
            {
                KeyQueue.Clear();
                return;
            }

            ReplayRecorder.OnIterationStart(Adofai.Controller.keyTimes.Count);

            var sortedKeyQueue = new PriorityQueue<SkyHookEvent, long>();

            while (KeyQueue.TryDequeue(out var key))
                sortedKeyQueue.Enqueue(key, key.GetTimeInTicks());

            while (sortedKeyQueue.TryDequeue(out var key, out var ticks))
                ReplayRecorder.OnKeyEvent(
                    0x1000 + key.Key,
                    key.Type == EventType.KeyReleased,
                    DspToSong(TickToDsp(ticks), Settings.Instance.AsyncRecordingOffset / 1000.0)
                );
        }
    }

    [HarmonyPatch(typeof(scrController), "PlayerControl_Update")]
    public static class Inject_scrController_PlayerControl_Update
    {
        private static AccessTools.FieldRef<StateEngine, StateMapping> DestinationStateField { get; } =
            AccessTools.FieldRefAccess<StateEngine, StateMapping>("destinationState");

        private static AccessTools.FieldRef<KeyCode[]> MainKeysField { get; } =
            AccessTools.StaticFieldRefAccess<KeyCode[]>(AccessTools.DeclaredField(
                typeof(RDInputType_Keyboard),
                "mainKeys")
            );

        public static void Prefix()
        {
            if (!Adofai.Controller.gameworld || ADOBase.isOfficialLevel || Adofai.Controller.paused)
            {
                KeyQueue.Clear();
                SyncKeyDownMap.Clear();
                return;
            }

            ReplayPlayer.UpdateReplayKeyStates();

            if (AsyncInputManager.isActive || Persistence.GetChosenAsynchronousInput()) return;

            KeyQueue.Clear();

            var replayToRecord = ReplayRecorder.Replay;

            if (
                replayToRecord == null ||
                (Adofai.CurrentFloorId <= replayToRecord.Metadata.StartingFloorId && (
                    Adofai.Controller.state != States.PlayerControl ||
                    (States)DestinationStateField(Adofai.Controller.stateMachine).state != States.PlayerControl ||
                    !Adofai.Controller.responsive
                ))
            )
            {
                SyncKeyDownMap.Clear();
                return;
            }

            ReplayRecorder.OnIterationStart(Adofai.Controller.keyTimes.Count);

            var mainKeys = MainKeysField()!;

            foreach (var mainKey in mainKeys)
            {
                var wentDown = Input.GetKeyDown(mainKey);
                var wentUp = Input.GetKeyUp(mainKey);

                if (wentDown)
                    CallEvent(false);
                else if (wentUp)
                    CallEvent(true);

                continue;

                void CallEvent(bool isKeyUp)
                {
                    ReplayRecorder.OnKeyEvent(
                        (int)mainKey,
                        isKeyUp,
                        DspToSong(Adofai.Conductor.dspTime, Settings.Instance.SyncRecordingOffset / 1000.0)
                    );
                    SyncKeyDownMap[mainKey] = !isKeyUp;
                }
            }
        }
    }

    [HarmonyPatch]
    public static class Inject_Input_GetKey
    {
        public static MethodInfo TargetMethod()
        {
            return AccessTools.DeclaredMethod(typeof(Input), "GetKey", [typeof(KeyCode)]);
        }

        public static bool Prefix(
            ref bool __result,
            KeyCode key
        )
        {
            if (!ReplayPlayer.PlayingReplay) return true;
            if (!Adofai.Controller.gameworld || ADOBase.isOfficialLevel || Adofai.Controller.paused) return true;
            return ReplayPlayer.OnGetKey(key, ref __result);
        }
    }

    [HarmonyPatch]
    public static class Inject_Input_GetKeyDown
    {
        public static MethodInfo TargetMethod()
        {
            return AccessTools.DeclaredMethod(typeof(Input), "GetKeyDown", [typeof(KeyCode)]);
        }

        public static bool Prefix(
            ref bool __result,
            KeyCode key
        )
        {
            if (!ReplayPlayer.PlayingReplay) return true;
            if (!Adofai.Controller.gameworld || ADOBase.isOfficialLevel || Adofai.Controller.paused) return true;
            return ReplayPlayer.OnGetKeyDown(key, ref __result);
        }
    }

    [HarmonyPatch]
    public static class Inject_Input_GetKeyUp
    {
        public static MethodInfo TargetMethod()
        {
            return AccessTools.DeclaredMethod(typeof(Input), "GetKeyUp", [typeof(KeyCode)]);
        }

        public static bool Prefix(
            ref bool __result,
            KeyCode key
        )
        {
            if (!ReplayPlayer.PlayingReplay) return true;
            if (!Adofai.Controller.gameworld || ADOBase.isOfficialLevel || Adofai.Controller.paused) return true;
            return ReplayPlayer.OnGetKeyUp(key, ref __result);
        }
    }

    [HarmonyPatch(typeof(scrCalibrationPlanet), "GetInputTypeForDown")]
    public static class Inject_scrCalibrationPlanet_GetInputTypeForDown
    {
        public static bool Prefix(
            ref object __result
        )
        {
            if (!ReplayPlayer.PlayingReplay) return true;
            if (!Adofai.Controller.gameworld || ADOBase.isOfficialLevel || Adofai.Controller.paused) return true;

            __result = GetInputTypeForDown();
            return false;

            int GetInputTypeForDown()
            {
                if (Input.touchCount > 0)
                {
                    foreach (var touch in Input.touches)
                        if (touch.phase == TouchPhase.Began)
                            return 4;
                }
                else if (ReplayPlayer.OnGetAnyKeyDown())
                {
                    for (var key = 1; key < 600; ++key)
                    {
                        if (Input.GetKeyDown(KeyCode.Space))
                            return 1;
                        if (Input.GetKeyDown(KeyCode.Mouse0))
                            return 2;
                        if (!Input.GetKeyDown((KeyCode)key)) continue;
                        if (key < 323) return 1;
                        return key < 350 ? 2 : 3;
                    }
                }

                return 0;
            }
        }
    }

    [HarmonyPatch(typeof(scrController), nameof(scrController.ValidInputWasTriggered))]
    public static class Inject_scrController_ValidInputWasTriggered
    {
        public static bool Prefix(
            ref bool __result,
            bool ___exitingToMainMenu,
            scrDpadInputChecker ___dpadInputChecker
        )
        {
            if (!ReplayPlayer.PlayingReplay) return true;
            if (!Adofai.Controller.gameworld || ADOBase.isOfficialLevel || Adofai.Controller.paused) return true;

            __result = ValidInputWasTriggered();
            return false;

            bool ValidInputWasTriggered()
            {
                if (___exitingToMainMenu) return false;

                var mouseOverAButton = false;
                if (Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Mouse1))
                    mouseOverAButton = EventSystem.current.IsPointerOverGameObject();
                if (Adofai.Controller.isCutscene)
                    mouseOverAButton = false;

                return (ReplayPlayer.OnGetAnyKeyDown() || ___dpadInputChecker.anyDirDown ||
                        RDInput.GetMain(ButtonState.IsDown) > 0) &&
                       !mouseOverAButton &&
                       Adofai.Controller.CountValidKeysPressed() > 0;
            }
        }
    }

    [HarmonyPatch(typeof(LevelData), nameof(LevelData.LoadLevel))]
    public static class Inject_LevelData_LoadLevel
    {
        public static void Prefix()
        {
            ReplayPlayer.UnloadReplay();
        }
    }

    [HarmonyPatch(typeof(scrPlanet), nameof(scrPlanet.AsyncRefreshAngles))]
    public static class Inject_scrPlanet_AsyncRefreshAngles
    {
        public static bool Prefix(
            scrPlanet __instance
        )
        {
            if (!Adofai.Controller.gameworld || ADOBase.isOfficialLevel || Adofai.Controller.paused) return true;
            return ReplayPlayer.OnAngleCorrection(ref __instance.angle);
        }

        public static void Postfix(
            scrPlanet __instance
        )
        {
            if (!Adofai.Controller.gameworld || ADOBase.isOfficialLevel || Adofai.Controller.paused) return;
            if (!Persistence.GetChosenAsynchronousInput()) return;
            ReplayRecorder.OnAngleCorrection(__instance.angle);
        }
    }

    [HarmonyPatch(typeof(AsyncInputUtils), nameof(AsyncInputUtils.AdjustAngle))]
    public static class Inject_AsyncInputUtils_AdjustAngle
    {
        public static bool Prefix()
        {
            if (!Adofai.Controller.gameworld || ADOBase.isOfficialLevel || Adofai.Controller.paused) return true;
            if (!ReplayPlayer.PlayingReplay) return true;
            Adofai.Controller.chosenPlanet.AsyncRefreshAngles();
            return false;
        }
    }

    [HarmonyPatch(typeof(scrController), nameof(scrController.Simulated_PlayerControl_Update))]
    public static class Inject_scrController_Simulated_PlayerControl_Update
    {
        public static bool Prefix()
        {
            if (!Adofai.Controller.gameworld || ADOBase.isOfficialLevel || Adofai.Controller.paused) return true;
            if (!Persistence.GetChosenAsynchronousInput())
                ReplayRecorder.OnAngleCorrection(Adofai.Controller.chosenPlanet.angle);
            return !ReplayPlayer.PlayingReplay || ReplayPlayer.AllowGameToUpdateInput;
        }
    }

    [HarmonyPatch(typeof(RDInputType_Keyboard), nameof(RDInputType_Keyboard.MainIgnoreActive))]
    public static class Inject_RDInputType_Keyboard_MainIgnoreActive
    {
        public static void Prefix(
            RDInputType_Keyboard __instance
        )
        {
            if (!ReplayPlayer.PlayingReplay) return;
            if (!Adofai.Controller.gameworld || ADOBase.isOfficialLevel || Adofai.Controller.paused) return;
            IsInMainIgnoreActiveAndPlayingReplay = true;

            if (!ReplayPlayer.UpdateKeyboardMainKeys) return;
            ReplayPlayer.UpdateKeyboardMainKeys = false;
            var forceUpdate = Time.frameCount - 1;
            __instance.dummyCount.lastFrameUpdated = forceUpdate;
            __instance.pressCount.lastFrameUpdated = forceUpdate;
            __instance.releaseCount.lastFrameUpdated = forceUpdate;
            __instance.heldCount.lastFrameUpdated = forceUpdate;
            __instance.isReleaseCount.lastFrameUpdated = forceUpdate;
        }

        public static Exception? Finalizer(
            Exception? __exception
        )
        {
            IsInMainIgnoreActiveAndPlayingReplay = false;
            return __exception;
        }
    }

    [HarmonyPatch(typeof(scrController), "CheckPostHoldFail")]
    public static class Inject_scrController_CheckPostHoldFail
    {
        public static bool Prefix(
            scrController __instance
        )
        {
            if (!ReplayPlayer.PlayingReplay) return true;
            if (!Adofai.Controller.gameworld || ADOBase.isOfficialLevel || Adofai.Controller.paused) return true;
            var continueExecution = ReplayPlayer.NextCheckFailMiss;
            ReplayPlayer.NextCheckFailMiss = false;
            return continueExecution;
        }
    }

    [HarmonyPatch(typeof(scrController), nameof(scrController.Hit))]
    public static class Inject_scrController_Hit
    {
        public static void Prefix(
            scrController __instance
        )
        {
            if (!ReplayPlayer.PlayingReplay) return;
            if (!__instance.noFailInfiniteMargin) return;
            if (!Adofai.Controller.gameworld || ADOBase.isOfficialLevel || Adofai.Controller.paused) return;
            ReplayPlayer.NextCheckFailMiss = false;
        }
    }

    [HarmonyPatch(typeof(AsyncInputManager), "Setup")]
    public static class Inject_AsyncInputManager_Setup
    {
        public static void Prefix(
            List<KeyLabel> ___MouseKeys
        )
        {
            SkyHookManager.KeyUpdated.AddListener(keyEvent =>
            {
                try
                {
                    if (___MouseKeys.Contains(keyEvent.Label)) return;
                    KeyQueue.Enqueue(keyEvent);
                }
                catch (Exception exception)
                {
                    Main.Mod.Logger.Log($"async exception: {exception}");
                }
            });
        }
    }

    [HarmonyPatch(typeof(scrPlanet), nameof(scrPlanet.AutoShouldHitNow))]
    public static class Inject_scrPlanet_AutoShouldHitNow
    {
        public static bool Prefix(
            ref bool __result
        )
        {
            if (!ReplayPlayer.PlayingReplay) return true;
            if (!Adofai.Controller.gameworld || ADOBase.isOfficialLevel || Adofai.Controller.paused) return true;
            var continueExecution = ReplayPlayer.AllowAuto && ReplayPlayer.NextCheckAuto;
            ReplayPlayer.AllowAuto = false;
            if (!continueExecution) __result = false;
            return continueExecution;
        }
    }

    [HarmonyPatch(typeof(scrController), "UpdateHoldKeys")]
    public static class Inject_scrController_UpdateHoldKeys
    {
        private static readonly Func<scrController, bool> NextTileIsHoldGetter =
            AccessTools.MethodDelegate<Func<scrController, bool>>(
                AccessTools.DeclaredPropertyGetter(typeof(scrController), "_nextTileIsHold"));

        private static readonly Func<scrController, double> HoldMarginGetter =
            AccessTools.MethodDelegate<Func<scrController, double>>(
                AccessTools.DeclaredPropertyGetter(typeof(scrController), "_holdMargin"));

        public static void Prefix(
            scrController __instance
        )
        {
            if (!Adofai.Controller.gameworld || ADOBase.isOfficialLevel || Adofai.Controller.paused) return;

            if (__instance.keyTimes.Count <= 0 ||
                GCS.d_stationary ||
                GCS.d_freeroam ||
                (!((__instance.currFloor.holdLength > -1 && !__instance.strictHolds) ||
                   NextTileIsHoldGetter(__instance)) &&
                 __instance.currFloor.holdLength != -1 &&
                 __instance.currFloor.holdCompletion >= HoldMarginGetter(__instance)) ||
                (__instance.gameworld &&
                 __instance.currFloor.seqID >= ADOBase.lm.listFloors.Count - 1)
               ) return;

            var nextFloor = Adofai.Controller.currFloor.nextfloor;
            var autoFloor = nextFloor != null && nextFloor.auto;

            ReplayRecorder.OnMarkKeyEvent(autoFloor, __instance.responsive);
            ReplayRecorder.OnKeysProcessed(1);
        }
    }

    [HarmonyPatch(typeof(RDInputType_Keyboard), nameof(RDInputType_Keyboard.CountSpecialInput))]
    public static class Inject_RDInputType_Keyboard_CountSpecialInput
    {
        public static bool Prefix(
            RDInputType_Keyboard __instance,
            ref List<KeyCode> __result
        )
        {
            if (!IsInMainIgnoreActiveAndPlayingReplay) return true;
            __result = __instance.Cancel() ? [KeyCode.Escape] : [];
            return false;
        }
    }
}