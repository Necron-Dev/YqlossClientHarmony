using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace YqlossClientHarmony.Features.Replay;

public static class ReplayPlayer
{
    public static bool PlayingReplay { get; set; }

    public static Replay? Replay { get; set; }

    private static MyQueue<(HitMargin, int)>? HitMargins { get; set; }

    private static MyQueue<(double, int)>? ErrorMeters { get; set; }

    private static MyQueue<Replay.KeyEventType>? KeyEvents { get; set; }

    private static MyQueue<Replay.KeyEventType>? KeyEventsForReceivers { get; set; }

    private static MyQueue<(double, int)>? AngleCorrections { get; set; }

    private static Dictionary<int, bool> KeyStates { get; set; } = [];

    private static Dictionary<int, bool> KeyStatesForReceivers { get; set; } = [];

    private static HashSet<int> IsKeyDown { get; } = [];

    private static HashSet<int> IsKeyUp { get; } = [];

    private static bool AnyKeyDown { get; set; }

    private static bool ConsumeSingleAngleCorrection { get; set; }

    private static double? CachedAngleCorrection { get; set; }

    public static bool AllowGameToUpdateInput { get; set; }

    public static bool NextCheckFailMiss { get; set; }

    public static bool AllowAuto { get; set; }

    public static void StartPlaying(int floorId)
    {
        _ = KeyEventReceiverManager.Instance;

        var replay = Replay;
        if (replay is null) return;

        PlayingReplay = true;

        KeyStates = [];
        ConsumeSingleAngleCorrection = false;
        AllowGameToUpdateInput = false;
        NextCheckFailMiss = false;
        AllowAuto = false;

        ReplayKeyboardInputType.Instance.MarkUpdate();

        {
            var accumulatedFloorId = replay.Metadata.StartingFloorId;
            MyQueue<Replay.KeyEventType> keyEvents = new();
            MyQueue<Replay.KeyEventType> keyEventsForReceivers = new();
            MyQueue<(double, int)> angleCorrections = new();
            KeyEvents = keyEvents;
            KeyEventsForReceivers = keyEventsForReceivers;
            AngleCorrections = angleCorrections;
            var keyIndex = 0;

            foreach (var keyEvent in replay.KeyEvents)
            {
                accumulatedFloorId += keyEvent.FloorIdIncrement;

                if (accumulatedFloorId < floorId)
                {
                    ++keyIndex;
                    continue;
                }

                keyEvents.Enqueue(keyEvent);
                keyEventsForReceivers.Enqueue(keyEvent);
                if (keyIndex < replay.AngleCorrections.Count)
                    angleCorrections.Enqueue((replay.AngleCorrections[keyIndex], accumulatedFloorId));
                ++keyIndex;
            }
        }

        {
            var accumulatedFloorId = replay.Metadata.StartingFloorId;
            MyQueue<(HitMargin, int)> hitMargins = new();
            MyQueue<(double, int)> errorMeters = new();
            HitMargins = hitMargins;
            ErrorMeters = errorMeters;
            foreach (var judgement in replay.Judgements)
            {
                accumulatedFloorId += judgement.FloorIdIncrement;
                if (accumulatedFloorId < floorId) continue;
                hitMargins.Enqueue((judgement.HitMargin, accumulatedFloorId));
                errorMeters.Enqueue((judgement.ErrorMeter, accumulatedFloorId));
            }
        }

        KeyEventReceiverManager.Instance.Begin();

        Main.Mod.Logger.Log("starting to play replay");
    }

    public static void EndPlaying()
    {
        if (PlayingReplay)
        {
            Main.Mod.Logger.Log("stopped playing replay");

            KeyEventReceiverManager.Instance.End();
        }

        PlayingReplay = false;
        HitMargins = null;
        ErrorMeters = null;
        KeyEvents = null;
        KeyStates.Clear();
        NextCheckFailMiss = false;
        AllowAuto = false;
    }

    public static void OnHitMargin(ref HitMargin result)
    {
        if (!PlayingReplay) return;

        var floorId = Adofai.CurrentFloorId;
        var hitMargins = HitMargins;

        if (hitMargins?.Count == 0)
        {
            Main.Mod.Logger.Warning($"[Floor {floorId}] hit margins are drained");
            hitMargins = HitMargins = null;
        }

        if (hitMargins is null) return;

        var (hitMargin, eventFloorId) = hitMargins.Dequeue();

        if (floorId != eventFloorId)
            Main.Mod.Logger.Warning(
                $"[Floor {floorId}] hit margin floor id mismatch {eventFloorId}. judgements may be incorrect");

        if (SettingsReplay.Instance.Verbose)
            Main.Mod.Logger.Log($"[Floor {floorId}] acc: {eventFloorId} margin: {hitMargin}");

        if (!Adofai.Controller.midspinInfiniteMargin)
        {
            result = hitMargin;
            return;
        }

        var errorMeters = ErrorMeters;

        if (errorMeters is null || errorMeters.Count == 0)
        {
            result = hitMargin;
            return;
        }

        var (nextMeter, nextMeterFloor) = errorMeters.Dequeue();

        if (SettingsReplay.Instance.Verbose)
            Main.Mod.Logger.Log($"[Floor {floorId}] acc: {eventFloorId} midspin meter");

        if (floorId != nextMeterFloor)
            Main.Mod.Logger.Warning(
                $"[Floor {floorId}] midspin error meter floor id mismatch {eventFloorId}. judgements may be incorrect");

        if (!double.IsNaN(nextMeter))
            Main.Mod.Logger.Warning(
                $"[Floor {floorId}] midspin error meter has value {nextMeter}. judgements may be incorrect");

        result = hitMargin;
    }

    public static void OnErrorMeter(ref double result)
    {
        if (!PlayingReplay) return;

        var floorId = Adofai.CurrentFloorId;
        var errorMeters = ErrorMeters;

        if (errorMeters?.Count == 0)
        {
            Main.Mod.Logger.Warning($"[Floor {floorId}] error meters are drained");
            errorMeters = ErrorMeters = null;
        }

        if (errorMeters is null) return;

        var (errorMeter, eventFloorId) = errorMeters.Dequeue();

        if (!double.IsFinite(errorMeter))
        {
            Main.Mod.Logger.Warning(
                $"[Floor {floorId}] error meter is not finite {errorMeter}. judgements may be incorrect");
            return;
        }

        if (floorId != eventFloorId)
            Main.Mod.Logger.Warning(
                $"[Floor {floorId}] error meter floor id mismatch {eventFloorId}. judgements may be incorrect");

        if (SettingsReplay.Instance.Verbose)
            Main.Mod.Logger.Log($"[Floor {floorId}] acc: {eventFloorId} meter: {errorMeter}");

        result = errorMeter;
    }

    public static void OnGetHitMargin(ref HitMargin result)
    {
        if (!PlayingReplay) return;

        var floorId = Adofai.CurrentFloorId;
        var hitMargins = HitMargins;

        if (hitMargins?.Count == 0)
        {
            Main.Mod.Logger.Warning($"[Floor {floorId}] hit margins are drained");
            hitMargins = HitMargins = null;
        }

        if (hitMargins is null) return;

        var (hitMargin, eventFloorId) = hitMargins.Peek();

        if (floorId != eventFloorId)
            Main.Mod.Logger.Warning(
                $"[Floor {floorId}] get hit margin floor id mismatch {eventFloorId}. judgements may be incorrect");

        if (SettingsReplay.Instance.Verbose)
            Main.Mod.Logger.Log($"[Floor {floorId}] get acc: {eventFloorId} margin: {hitMargin}");

        result = hitMargin switch
        {
            HitMargin.FailMiss => HitMargin.TooLate,
            HitMargin.FailOverload => HitMargin.TooEarly,
            HitMargin.Auto => HitMargin.Perfect,
            _ => hitMargin
        };
    }

    private static void ProcessAutoFloorAndFailMiss()
    {
        {
            for (
                var nextFloor = Adofai.Controller.currFloor.nextfloor;
                nextFloor != null && nextFloor.auto;
                nextFloor = Adofai.Controller.currFloor.nextfloor
            )
            {
                var floorId = Adofai.CurrentFloorId;
                NextCheckFailMiss = false;
                ConsumeSingleAngleCorrection = false;
                CachedAngleCorrection = null;
                AllowGameToUpdateInput = true;
                AllowAuto = true;
                ReplayKeyboardInputType.Instance.MarkUpdate();
                Adofai.Controller.Simulated_PlayerControl_Update();
                AllowGameToUpdateInput = false;
                if (Adofai.CurrentFloorId == floorId) break;
                if (SettingsReplay.Instance.Verbose) Main.Mod.Logger.Log("auto floor");
            }
        }

        {
            for (;;)
            {
                var hitMargins = HitMargins;

                if (hitMargins is null || hitMargins.Count == 0) break;

                var hitMarginCount = hitMargins.Count;
                var (hitMargin, _) = hitMargins.Peek();

                if (hitMargin is HitMargin.FailMiss)
                {
                    NextCheckFailMiss = true;
                    ConsumeSingleAngleCorrection = false;
                    CachedAngleCorrection = null;
                    AllowGameToUpdateInput = true;
                    AllowAuto = false;
                    ReplayKeyboardInputType.Instance.MarkUpdate();
                    Adofai.Controller.Simulated_PlayerControl_Update();
                    NextCheckFailMiss = false;
                    AllowGameToUpdateInput = false;
                }
                else
                {
                    break;
                }

                if (HitMargins?.Count == hitMarginCount) return;
                if (SettingsReplay.Instance.Verbose) Main.Mod.Logger.Log("fail miss");
            }
        }
    }

    public static void UpdateReplayKeyStates()
    {
        if (!PlayingReplay) return;

        var songSeconds =
            Injections.DspToSong(Adofai.Conductor.dspTime, SettingsReplay.Instance.PlayingOffset / 1000.0);

        var isKeyDown = IsKeyDown;
        var isKeyUp = IsKeyUp;

        var lastKeyStates = KeyStates;
        var lastKeyStatesForReceivers = KeyStatesForReceivers;

        isKeyDown.Clear();
        isKeyUp.Clear();
        AnyKeyDown = false;
        Adofai.Controller.keyTimes.Clear();

        try
        {
            ProcessAutoFloorAndFailMiss();

            var keyEvents = KeyEvents;

            if (keyEvents?.Count == 0)
            {
                Main.Mod.Logger.Warning($"[Floor {Adofai.CurrentFloorId}] key events are drained");
                keyEvents = KeyEvents = null;
            }

            if (keyEvents is null)
            {
                NextCheckFailMiss = true;
                ConsumeSingleAngleCorrection = false;
                CachedAngleCorrection = null;
                AllowGameToUpdateInput = true;
                AllowAuto = true;
                ReplayKeyboardInputType.Instance.MarkUpdate();
                Adofai.Controller.Simulated_PlayerControl_Update();
                AllowGameToUpdateInput = false;
                return;
            }

            while (keyEvents.Count != 0)
            {
                var floorId = Adofai.CurrentFloorId;

                var key = keyEvents.Peek();
                var syncKeyCode = KeyCodeMapping.GetSyncKeyCode(key.KeyCode);

                var nextFloor = Adofai.Controller.currFloor.nextfloor;
                var autoFloor = nextFloor != null && nextFloor.auto;

                if (!key.Version1)
                {
                    if (!key.IsAutoFloor && autoFloor) break;
                    if (!key.IsInputLocked && !Adofai.Controller.responsive) break;
                }

                if (key.SongSeconds > songSeconds) break;

                if (SettingsReplay.Instance.Verbose)
                    Main.Mod.Logger.Log(
                        $"[Floor {floorId}] simulate key: {syncKeyCode}({key.KeyCode}) up: {key.IsKeyUp} dseq: {key.FloorIdIncrement} pos: {key.SongSeconds} auto: {key.IsAutoFloor} locked: {key.IsInputLocked}"
                    );

                var keyStates = KeyStates = new Dictionary<int, bool>(lastKeyStates)
                {
                    [syncKeyCode] = !key.IsKeyUp
                };

                keyEvents.Dequeue();

                isKeyDown.Clear();
                isKeyUp.Clear();
                AnyKeyDown = false;

                foreach (var (keyCode, state) in keyStates)
                {
                    var last = lastKeyStates.GetValueOrDefault(keyCode, false);

                    if (state && !last)
                    {
                        isKeyDown.Add(keyCode);
                        AnyKeyDown = true;
                    }

                    if (!state && last) isKeyUp.Add(keyCode);
                }

                Adofai.Controller.keyTimes.Clear();
                NextCheckFailMiss = false;
                AllowAuto = false;

                if (SettingsReplay.Instance.Verbose) Main.Mod.Logger.Log("begin simulation");

                if (key is { IsAutoFloor: false, IsInputLocked: false })
                {
                    AllowGameToUpdateInput = true;
                    ReplayKeyboardInputType.Instance.MarkUpdate();
                    var angleCorrections = AngleCorrections;

                    if (angleCorrections is null || angleCorrections.Count == 0)
                    {
                        if (angleCorrections?.Count == 0)
                        {
                            Main.Mod.Logger.Warning($"[Floor {floorId}] angle corrections are drained");
                            AngleCorrections = null;
                        }

                        ConsumeSingleAngleCorrection = false;
                        CachedAngleCorrection = null;
                        Adofai.Controller.Simulated_PlayerControl_Update();
                    }
                    else
                    {
                        ConsumeSingleAngleCorrection = true;
                        CachedAngleCorrection = null;
                        Adofai.Controller.Simulated_PlayerControl_Update(1);
                    }

                    if (SettingsReplay.Instance.Verbose) Main.Mod.Logger.Log("end simulation");
                }
                else
                {
                    var angleCorrections = AngleCorrections;

                    if (angleCorrections is not null && angleCorrections.Count != 0)
                    {
                        angleCorrections.Dequeue();
                        if (SettingsReplay.Instance.Verbose) Main.Mod.Logger.Log("consume angle correction");
                    }

                    if (SettingsReplay.Instance.Verbose) Main.Mod.Logger.Log("skip simulation");
                }

                isKeyDown.Clear();
                isKeyUp.Clear();
                AnyKeyDown = false;
                Adofai.Controller.keyTimes.Clear();
                AllowGameToUpdateInput = false;
                ConsumeSingleAngleCorrection = false;
                CachedAngleCorrection = null;
                lastKeyStates = keyStates;

                ProcessAutoFloorAndFailMiss();
            }

            isKeyDown.Clear();
            isKeyUp.Clear();
            Adofai.Controller.keyTimes.Clear();
            AnyKeyDown = false;
            KeyStates = lastKeyStates;

            var keyEventsForReceivers = KeyEventsForReceivers;

            if (keyEventsForReceivers is null) return;

            while (keyEventsForReceivers.Count != 0)
            {
                var key = keyEventsForReceivers.Peek();
                var syncKeyCode = KeyCodeMapping.GetSyncKeyCode(key.KeyCode);

                if (key.SongSeconds > songSeconds) break;

                keyEventsForReceivers.Dequeue();

                lastKeyStatesForReceivers = KeyStatesForReceivers = new Dictionary<int, bool>(lastKeyStatesForReceivers)
                {
                    [syncKeyCode] = !key.IsKeyUp
                };

                KeyEventReceiverManager.Instance.OnKey((KeyCode)syncKeyCode, !key.IsKeyUp);
            }
        }
        finally
        {
            isKeyDown.Clear();
            isKeyUp.Clear();
            Adofai.Controller.keyTimes.Clear();
            AnyKeyDown = false;
            KeyStates = lastKeyStates;
            KeyStatesForReceivers = lastKeyStatesForReceivers;
        }
    }

    public static bool OnAngleCorrection(ref double result)
    {
        if (!PlayingReplay) return true;

        if (!ConsumeSingleAngleCorrection)
        {
            var cached = CachedAngleCorrection;
            if (cached is not null) result = cached.Value;
            return cached is null;
        }

        ConsumeSingleAngleCorrection = false;

        var floorId = Adofai.CurrentFloorId;
        var angleCorrections = AngleCorrections;

        if (angleCorrections?.Count == 0)
        {
            Main.Mod.Logger.Warning($"[Floor {floorId}] angle corrections are drained");
            angleCorrections = AngleCorrections = null;
        }

        if (angleCorrections is null) return true;

        var (angleCorrection, eventFloorId) = angleCorrections.Dequeue();

        if (!double.IsFinite(angleCorrection))
        {
            Main.Mod.Logger.Warning(
                $"[Floor {floorId}] angle correction is not finite {angleCorrection}. judgements may be incorrect");
            return true;
        }

        if (SettingsReplay.Instance.Verbose)
            Main.Mod.Logger.Log($"[Floor {floorId}] acc: {eventFloorId} angle: {angleCorrection}");

        CachedAngleCorrection = angleCorrection;
        result = angleCorrection;
        return false;
    }

    public static bool OnGetKeyDown(KeyCode keyCode, ref bool result)
    {
        if (!PlayingReplay) return true;
        if (keyCode == KeyCode.Escape) return true;
        result = IsKeyDown.Contains((int)keyCode);
        return false;
    }

    public static bool OnGetKeyUp(KeyCode keyCode, ref bool result)
    {
        if (!PlayingReplay) return true;
        if (keyCode == KeyCode.Escape) return true;
        result = IsKeyUp.Contains((int)keyCode);
        return false;
    }

    public static bool OnGetKey(KeyCode keyCode, ref bool result)
    {
        if (!PlayingReplay) return true;
        if (keyCode == KeyCode.Escape) return true;
        result = KeyStatesForReceivers.GetValueOrDefault((int)keyCode, false);
        return false;
    }

    public static bool OnGetAnyKeyDown()
    {
        return AnyKeyDown;
    }

    public static bool GetKeyDownUnchecked(KeyCode keyCode)
    {
        return keyCode != KeyCode.Escape && IsKeyDown.Contains((int)keyCode);
    }

    public static bool GetKeyUpUnchecked(KeyCode keyCode)
    {
        return keyCode != KeyCode.Escape && IsKeyUp.Contains((int)keyCode);
    }

    public static bool GetKeyUnchecked(KeyCode keyCode)
    {
        return keyCode != KeyCode.Escape && KeyStates.GetValueOrDefault((int)keyCode, false);
    }

    public static bool GetAnyKeyDownUnchecked(KeyCode[] keyCodes)
    {
        return keyCodes.Any(GetKeyDownUnchecked);
    }

    public static bool GetAnyKeyUpUnchecked(KeyCode[] keyCodes)
    {
        return keyCodes.Any(GetKeyUpUnchecked);
    }

    public static bool GetAnyKeyUnchecked(KeyCode[] keyCodes)
    {
        return keyCodes.Any(GetKeyUnchecked);
    }

    public static bool GetAnyKeyReleasedUnchecked(KeyCode[] keyCodes)
    {
        return keyCodes.Any(it => !GetKeyUnchecked(it));
    }

    public static IEnumerable<KeyCode> GetKeysDownUnchecked(KeyCode[] keyCodes)
    {
        return keyCodes.Where(GetKeyDownUnchecked);
    }

    public static IEnumerable<KeyCode> GetKeysUpUnchecked(KeyCode[] keyCodes)
    {
        return keyCodes.Where(GetKeyUpUnchecked);
    }

    public static IEnumerable<KeyCode> GetKeysUnchecked(KeyCode[] keyCodes)
    {
        return keyCodes.Where(GetKeyUnchecked);
    }

    public static IEnumerable<KeyCode> GetKeysReleasedUnchecked(KeyCode[] keyCodes)
    {
        return keyCodes.Where(it => !GetKeyUnchecked(it));
    }

    public static bool LoadReplay(string fileName)
    {
        var replay = ReplayDecoder.ReadCompressedReplay(fileName);

        if (replay is null) return false;

        ReplayRecorder.SaveAndResetReplay();

        Replay = replay;

        return true;
    }

    public static void UnloadReplay()
    {
        Replay = null;
        PlayingReplay = false;
    }
}