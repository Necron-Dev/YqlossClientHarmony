using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using ADOFAI;
using HarmonyLib;
using MonsterLove.StateMachine;
using SkyHook;
using UnityEngine;
using EventType = SkyHook.EventType;

namespace YqlossClientHarmony.Features.Replay;

public static class Injections
{
    private static ConcurrentCondition ConditionPlayingCustom { get; } = new(() =>
        Adofai.Controller.gameworld && !ADOBase.isOfficialLevel && !Adofai.Controller.paused
    );

    private static long? TickToDspOffset { get; set; }

    private static bool IsInSwitchChosen { get; set; }

    private static bool IsInUpdateHoldBehavior { get; set; }

    private static ConcurrentQueue<SkyHookEvent> KeyQueue { get; } = [];

    public static double TickToDsp(long ticks)
    {
        long tickToDspOffset;

        if (SettingsReplay.Instance.NoTickToDspCache)
        {
            TickToDspOffset = null;
            tickToDspOffset = (long)(AudioSettings.dspTime * 10000000.0) - DateTime.Now.Ticks;
        }
        else
        {
            var tickToDspOffsetNullable = TickToDspOffset;

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
        }

        return (ticks + tickToDspOffset) / 10000000.0;
    }

    public static double DspToSong(double dsp, double offset)
    {
        var conductor = Adofai.Conductor;

        return conductor.song.pitch * (
                   dsp
                   - conductor.dspTimeSongPosZero
                   - scrConductor.calibration_i
                   + offset
               )
               - conductor.adjustedCountdownTicks * conductor.crotchetAtStart
               + conductor.addoffset;
    }

    [HarmonyPatch(typeof(scnGame), nameof(scnGame.Play))]
    public static class Inject_scnGame_Play
    {
        public static void Prefix(
            int seqID
        )
        {
            if (ADOBase.isOfficialLevel) return;
            if (RDC.auto)
            {
                Main.Mod.Logger.Log("skipping recording and playing replay: auto mode");
                return;
            }

            TickToDspOffset = null;
            ReplayPlayer.StartPlaying(seqID);
            if (!SettingsReplay.Instance.Enabled) return;
            ReplayRecorder.StartRecording(seqID);
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
            ReplayRecorder.EndRecording();
            ReplayPlayer.EndPlaying();
            ReplayPlayer.ResetTrailingAnimation();
        }
    }

    [HarmonyPatch(typeof(scrController), nameof(scrController.OnLandOnPortal))]
    public static class Inject_scrController_OnLandOnPortal
    {
        public static void Prefix()
        {
            ReplayRecorder.EndRecording();
            ReplayPlayer.EndPlaying();
        }

        public static void Postfix(
            scrController __instance
        )
        {
            var now = DateTime.Now;
            if (now.Month != 7 || now.Day != 27) return;
            __instance.txtAllStrictClear.text = "727 WYSI!";
        }
    }

    [HarmonyPatch(typeof(scnEditor), "ResetScene")]
    public static class Inject_scnEditor_ResetScene
    {
        public static void Prefix()
        {
            ReplayRecorder.EndRecording();
            ReplayPlayer.EndPlaying();
            ReplayPlayer.ResetTrailingAnimation();
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

            if (ReplayRecorder.Replay is not null)
                ReplayRecorder.OnHitMargin(IsInUpdateHoldBehavior && hit == HitMargin.FailMiss ? ReplayConstants.HoldPreMiss : hit);

            if (ReplayPlayer.PlayingReplay) ReplayPlayer.OnHitMargin(ref hit);
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
            if (!ReplayPlayer.PlayingReplay) return;
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
            if (ReplayRecorder.Replay is not null) ReplayRecorder.OnErrorMeter(angleDiff);
            if (!ReplayPlayer.PlayingReplay) return;
            double result = angleDiff;
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

            if (replayToRecord == null) return;

            if (
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
                    DspToSong(TickToDsp(ticks), SettingsReplay.Instance.AsyncRecordingOffset / 1000.0)
                );
        }
    }

    [HarmonyPatch(typeof(scrController), "PlayerControl_Update")]
    public static class Inject_scrController_PlayerControl_Update
    {
        private static AccessTools.FieldRef<StateEngine, StateMapping> DestinationStateField { get; } =
            AccessTools.FieldRefAccess<StateEngine, StateMapping>("destinationState");

        private static AccessTools.FieldRef<KeyCode[]> MainKeysField { get; } =
            AccessTools.StaticFieldRefAccess<KeyCode[]>(AccessTools.DeclaredField(typeof(RDInputType_Keyboard), "mainKeys"));

        public static void Prefix()
        {
            if (!ConditionPlayingCustom)
            {
                KeyQueue.Clear();
                return;
            }

            using var _ = ConditionPlayingCustom;

            if (ReplayPlayer.PlayingReplay) ReplayPlayer.UpdateReplayKeyStates();

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
            ) return;

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
                        DspToSong(Adofai.Conductor.dspTime, SettingsReplay.Instance.SyncRecordingOffset / 1000.0)
                    );
                }
            }
        }
    }

    [HarmonyPatch(typeof(Input), nameof(Input.GetKey), typeof(KeyCode))]
    public static class Inject_Input_GetKey
    {
        public static bool Prefix(
            ref bool __result,
            KeyCode key
        )
        {
            return ReplayPlayer.OnGetKey(key, ref __result);
        }
    }

    [HarmonyPatch(typeof(Input), nameof(Input.GetKeyDown), typeof(KeyCode))]
    public static class Inject_Input_GetKeyDown
    {
        public static bool Prefix(
            ref bool __result,
            KeyCode key
        )
        {
            return !ReplayPlayer.PlayingReplay || ReplayPlayer.OnGetKeyDown(key, ref __result);
        }
    }

    [HarmonyPatch(typeof(Input), nameof(Input.GetKeyUp), typeof(KeyCode))]
    public static class Inject_Input_GetKeyUp
    {
        public static bool Prefix(
            ref bool __result,
            KeyCode key
        )
        {
            return !ReplayPlayer.PlayingReplay || ReplayPlayer.OnGetKeyUp(key, ref __result);
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

            __result = 1; // InputType.Keyboard
            return false;
        }
    }

    [HarmonyPatch(typeof(scrController), nameof(scrController.ValidInputWasTriggered))]
    public static class Inject_scrController_ValidInputWasTriggered
    {
        public static bool Prefix(
            ref bool __result,
            bool ___exitingToMainMenu
        )
        {
            if (!ReplayPlayer.PlayingReplay) return true;

            __result = !___exitingToMainMenu &&
                       (ReplayPlayer.OnGetAnyKeyDown() ||
                        RDInput.GetMain(ButtonState.IsDown) > 0) &&
                       Adofai.Controller.CountValidKeysPressed() > 0;

            return false;
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
            return !ReplayPlayer.PlayingReplay || ReplayPlayer.OnAngleCorrection(ref __instance.angle);
        }

        public static void Postfix(
            scrPlanet __instance
        )
        {
            if (ReplayRecorder.Replay is null) return;
            if (!Persistence.GetChosenAsynchronousInput()) return;
            ReplayRecorder.OnAngleCorrection(__instance.angle);
        }
    }

    [HarmonyPatch(typeof(AsyncInputUtils), nameof(AsyncInputUtils.AdjustAngle))]
    public static class Inject_AsyncInputUtils_AdjustAngle
    {
        public static bool Prefix()
        {
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
            if (!ConditionPlayingCustom) return true;
            using var _ = ConditionPlayingCustom;
            if (ReplayRecorder.Replay is not null && !Persistence.GetChosenAsynchronousInput())
                ReplayRecorder.OnAngleCorrection(Adofai.Controller.chosenPlanet.angle);
            if (!ReplayPlayer.PlayingReplay) return true;
            return !ReplayPlayer.PlayingReplay || ReplayPlayer.AllowGameToUpdateInput;
        }

        public static void Postfix()
        {
            if (!ReplayPlayer.PlayingReplay) return;
            scrController.shouldReplaceCamyToPos = false;
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
            ReplayPlayer.NextCheckFailMiss = false;
        }

        public static void Postfix()
        {
            if (ReplayRecorder.Replay is null) return;
            ReplayRecorder.OnPostHit();
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
            var continueExecution = ReplayPlayer.AllowAuto;
            ReplayPlayer.AllowAuto = false;
            if (!continueExecution) __result = false;
            return continueExecution;
        }
    }

    [HarmonyPatch(typeof(scrController), "UpdateHoldKeys")]
    public static class Inject_scrController_UpdateHoldKeys
    {
        private static readonly Func<scrController, bool> NextTileIsHoldGetter =
            AccessTools.MethodDelegate<Func<scrController, bool>>(AccessTools.DeclaredPropertyGetter(typeof(scrController), "_nextTileIsHold"));

        private static readonly Func<scrController, double> HoldMarginGetter =
            AccessTools.MethodDelegate<Func<scrController, double>>(AccessTools.DeclaredPropertyGetter(typeof(scrController), "_holdMargin"));

        public static void Prefix(
            scrController __instance
        )
        {
            if (ReplayRecorder.Replay is null) return;

            if (
                __instance.keyTimes.Count <= 0 ||
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

    [HarmonyPatch(typeof(RDInput), nameof(RDInput.GetState))]
    public static class Inject_RDInput_GetState
    {
        public static void Prefix(
            out List<RDInputType> __state
        )
        {
            __state = RDInput.inputs;
            if (!ReplayPlayer.PlayingReplay) return;
            RDInput.inputs = ReplayKeyboardInputType.SingletonList;
        }

        public static Exception? Finalizer(
            Exception? __exception,
            List<RDInputType> __state
        )
        {
            RDInput.inputs = __state;
            return __exception;
        }
    }

    [HarmonyPatch(typeof(RDInput), nameof(RDInput.GetMain))]
    public static class Inject_RDInput_GetMain
    {
        public static void Prefix(
            out List<RDInputType> __state
        )
        {
            __state = RDInput.inputs;
            if (!ReplayPlayer.PlayingReplay) return;
            RDInput.inputs = ReplayKeyboardInputType.SingletonList;
        }

        public static Exception? Finalizer(
            Exception? __exception,
            List<RDInputType> __state
        )
        {
            RDInput.inputs = __state;
            return __exception;
        }
    }

    [HarmonyPatch(typeof(RDInput), nameof(RDInput.GetStateKeys))]
    public static class Inject_RDInput_GetStateKeys
    {
        public static void Prefix(
            out List<RDInputType> __state
        )
        {
            __state = RDInput.inputs;
            if (!ReplayPlayer.PlayingReplay) return;
            RDInput.inputs = ReplayKeyboardInputType.SingletonList;
        }

        public static Exception? Finalizer(
            Exception? __exception,
            List<RDInputType> __state
        )
        {
            RDInput.inputs = __state;
            return __exception;
        }
    }

    [HarmonyPatch(typeof(scrController), "ProcessKeyInputs")]
    public static class Inject_scrController_ProcessKeyInputs
    {
        public static bool Prefix()
        {
            return !ReplayPlayer.PlayingReplay;
        }
    }

    [HarmonyPatch(typeof(scnEditor), "ClearAllFloorOffsets")]
    public static class Inject_scnEditor_ClearAllFloorOffsets
    {
        public static void Prefix()
        {
            ReplayPlayer.UnloadReplay();
            ReplayRecorder.EndRecording();
            ReplayPlayer.EndPlaying();
        }
    }

    [HarmonyPatch(typeof(scrController), "UpdateHoldBehavior")]
    public static class Inject_scrController_UpdateHoldBehavior
    {
        public static void Prefix()
        {
            IsInUpdateHoldBehavior = true;
        }

        public static Exception? Finalizer(
            Exception? __exception
        )
        {
            IsInUpdateHoldBehavior = false;
            return __exception;
        }
    }
}