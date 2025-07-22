using System;
using System.Collections.Generic;
using UnityEngine;

namespace YqlossClientHarmony.Features.Replay;

public static class ReplayPlayer
{
    public static bool PlayingReplay { get; set; }

    public static Replay? Replay { get; set; }

    private static MyQueue<(HitMargin, int)>? HitMargins { get; set; }

    private static MyQueue<(double, int)>? ErrorMeters { get; set; }

    private static MyQueue<(Replay.KeyEventType, int)>? KeyEvents { get; set; }

    private static MyQueue<(double, int)>? AngleCorrections { get; set; }

    private static Dictionary<int, bool>? KeyStates { get; set; }

    private static Dictionary<int, bool> IsKeyDown { get; } = [];

    private static Dictionary<int, bool> IsKeyUp { get; } = [];

    private static bool AnyKeyDown { get; set; }

    private static bool ConsumeSingleAngleCorrection { get; set; }

    private static double? CachedAngleCorrection { get; set; }

    public static bool AllowGameToUpdateInput { get; set; }

    public static bool UpdateKeyboardMainKeys { get; set; }

    public static bool NextCheckFailMiss { get; set; }

    public static bool AllowAuto { get; set; }

    public static bool NextCheckAuto { get; set; }

    public static void StartPlaying(int floorId)
    {
        var replay = Replay;
        if (replay is null) return;

        PlayingReplay = true;

        KeyStates = [];
        ConsumeSingleAngleCorrection = false;
        AllowGameToUpdateInput = false;
        UpdateKeyboardMainKeys = true;
        NextCheckFailMiss = false;
        AllowAuto = false;

        {
            var accumulatedFloorId = replay.Metadata.StartingFloorId;
            MyQueue<(Replay.KeyEventType, int)> keyEvents = new();
            MyQueue<(double, int)> angleCorrections = new();
            KeyEvents = keyEvents;
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

                keyEvents.Enqueue((keyEvent, accumulatedFloorId));
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

        Main.Mod.Logger.Log("starting to play replay");
    }

    public static void EndPlaying()
    {
        if (PlayingReplay) Main.Mod.Logger.Log("stopped playing replay");
        PlayingReplay = false;
        HitMargins = null;
        ErrorMeters = null;
        KeyEvents = null;
        KeyStates = null;
        NextCheckFailMiss = false;
        AllowAuto = false;
    }

    private static void WithReplay(Action<Replay> receiver)
    {
        var replay = Replay;

        if (replay is null || !PlayingReplay)
        {
            PlayingReplay = false;
            HitMargins = null;
            ErrorMeters = null;
            KeyEvents = null;
            KeyStates = null;
            NextCheckFailMiss = false;
            AllowAuto = false;
        }
        else
        {
            receiver(replay);
        }
    }

    private static bool WithReplay<T>(ref T t, Func<Replay, T?> receiver) where T : struct
    {
        var replay = Replay;

        if (replay is null || !PlayingReplay)
        {
            PlayingReplay = false;
            HitMargins = null;
            ErrorMeters = null;
            KeyEvents = null;
            KeyStates = null;
            NextCheckFailMiss = false;
            AllowAuto = false;
            return true;
        }

        var newT = receiver(replay);
        if (newT is not null) t = newT.Value;
        return newT is null;
    }

    public static void OnHitMargin(ref HitMargin result)
    {
        WithReplay(ref result, _ =>
        {
            var floorId = Adofai.CurrentFloorId;
            var hitMargins = HitMargins;

            if (hitMargins?.Count == 0)
            {
                Main.Mod.Logger.Warning($"[Floor {floorId}] hit margins are drained");
                hitMargins = HitMargins = null;
            }

            if (hitMargins is null) return null;

            var (hitMargin, eventFloorId) = hitMargins.Dequeue();

            if (floorId != eventFloorId)
                Main.Mod.Logger.Warning(
                    $"[Floor {floorId}] hit margin floor id mismatch {eventFloorId}. judgements may be incorrect");

            if (Settings.Instance.Verbose)
                Main.Mod.Logger.Log($"[Floor {floorId}] acc: {eventFloorId} margin: {hitMargin}");

            if (!Adofai.Controller.midspinInfiniteMargin) return hitMargin;

            var errorMeters = ErrorMeters;

            if (errorMeters is null || errorMeters.Count == 0) return hitMargin;

            var (nextMeter, nextMeterFloor) = errorMeters.Dequeue();

            if (Settings.Instance.Verbose)
                Main.Mod.Logger.Log($"[Floor {floorId}] acc: {eventFloorId} midspin meter");

            if (floorId != nextMeterFloor)
                Main.Mod.Logger.Warning(
                    $"[Floor {floorId}] midspin error meter floor id mismatch {eventFloorId}. judgements may be incorrect");

            if (!double.IsNaN(nextMeter))
                Main.Mod.Logger.Warning(
                    $"[Floor {floorId}] midspin error meter has value {nextMeter}. judgements may be incorrect");

            return hitMargin;
        });
    }

    public static void OnErrorMeter(ref double result)
    {
        WithReplay(ref result, _ =>
        {
            var floorId = Adofai.CurrentFloorId;
            var errorMeters = ErrorMeters;

            if (errorMeters?.Count == 0)
            {
                Main.Mod.Logger.Warning($"[Floor {floorId}] error meters are drained");
                errorMeters = ErrorMeters = null;
            }

            if (errorMeters is null) return null;

            var (errorMeter, eventFloorId) = errorMeters.Dequeue();

            if (floorId != eventFloorId)
                Main.Mod.Logger.Warning(
                    $"[Floor {floorId}] error meter floor id mismatch {eventFloorId}. judgements may be incorrect");

            if (Settings.Instance.Verbose)
                Main.Mod.Logger.Log($"[Floor {floorId}] acc: {eventFloorId} meter: {errorMeter}");

            return errorMeter;
        });
    }

    public static void OnGetHitMargin(ref HitMargin result)
    {
        WithReplay(ref result, _ =>
        {
            var floorId = Adofai.CurrentFloorId;
            var hitMargins = HitMargins;

            if (hitMargins?.Count == 0)
            {
                Main.Mod.Logger.Warning($"[Floor {floorId}] hit margins are drained");
                hitMargins = HitMargins = null;
            }

            if (hitMargins is null) return null;

            var (hitMargin, eventFloorId) = hitMargins.Peek();

            if (floorId != eventFloorId)
                Main.Mod.Logger.Warning(
                    $"[Floor {floorId}] get hit margin floor id mismatch {eventFloorId}. judgements may be incorrect");

            if (Settings.Instance.Verbose)
                Main.Mod.Logger.Log($"[Floor {floorId}] get acc: {eventFloorId} margin: {hitMargin}");

            return hitMargin switch
            {
                HitMargin.FailMiss => HitMargin.TooLate,
                HitMargin.FailOverload => HitMargin.TooEarly,
                HitMargin.Auto => HitMargin.Perfect,
                _ => hitMargin
            };
        });
    }

    private static bool ProcessAutoFloorAndFailMiss()
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
                UpdateKeyboardMainKeys = true;
                AllowAuto = true;
                NextCheckAuto = true;
                Adofai.Controller.Simulated_PlayerControl_Update();
                AllowGameToUpdateInput = false;
                if (Adofai.CurrentFloorId == floorId) break;
                if (Settings.Instance.Verbose) Main.Mod.Logger.Log("auto floor");
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
                    UpdateKeyboardMainKeys = true;
                    AllowAuto = false;
                    Adofai.Controller.Simulated_PlayerControl_Update();
                    NextCheckFailMiss = false;
                    AllowGameToUpdateInput = false;
                }
                else
                {
                    break;
                }

                if (HitMargins?.Count == hitMarginCount) return true;
                if (Settings.Instance.Verbose) Main.Mod.Logger.Log("fail miss");
            }
        }

        return false;
    }

    public static void UpdateReplayKeyStates()
    {
        WithReplay(_ =>
        {
            var songSeconds =
                Injections.DspToSong(Adofai.Conductor.dspTime, Settings.Instance.PlayingOffset / 1000.0);

            var isKeyDown = IsKeyDown;
            var isKeyUp = IsKeyUp;

            var lastKeyStates = KeyStates;

            isKeyDown.Clear();
            isKeyUp.Clear();
            AnyKeyDown = false;
            Adofai.Controller.keyTimes.Clear();

            try
            {
                var onlyAllowRelease = ProcessAutoFloorAndFailMiss();

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
                    UpdateKeyboardMainKeys = true;
                    AllowAuto = true;
                    NextCheckAuto = true;
                    Adofai.Controller.Simulated_PlayerControl_Update();
                    AllowGameToUpdateInput = false;
                    return;
                }

                if (lastKeyStates is null) return;

                while (keyEvents.Count != 0)
                {
                    var floorId = Adofai.CurrentFloorId;

                    var (key, keyFloor) = keyEvents.Peek();
                    var syncKeyCode = KeyCodeMapping.GetSyncKeyCode(key.KeyCode);

                    if (onlyAllowRelease && !key.IsKeyUp) break;

                    var nextFloor = Adofai.Controller.currFloor.nextfloor;
                    var autoFloor = nextFloor != null && nextFloor.auto;

                    if (!key.Version1)
                    {
                        if (!key.IsAutoFloor && autoFloor) break;
                        if (!key.IsInputLocked && !Adofai.Controller.responsive) break;
                    }

                    if (key.SongSeconds > songSeconds)
                        break;

                    if (Settings.Instance.Verbose)
                        Main.Mod.Logger.Log(
                            $"[Floor {floorId}] acc: {keyFloor} simulate key: {syncKeyCode}({key.KeyCode}) up: {key.IsKeyUp} dseq: {key.FloorIdIncrement} pos: {key.SongSeconds} auto: {key.IsAutoFloor} locked: {key.IsInputLocked}"
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
                            isKeyDown[keyCode] = true;
                            AnyKeyDown = true;
                        }

                        if (!state && last) isKeyUp[keyCode] = true;
                    }

                    Adofai.Controller.keyTimes.Clear();
                    NextCheckFailMiss = false;
                    AllowAuto = false;

                    if (Settings.Instance.Verbose) Main.Mod.Logger.Log("begin simulation");

                    if (key is { IsAutoFloor: false, IsInputLocked: false })
                    {
                        AllowGameToUpdateInput = true;
                        UpdateKeyboardMainKeys = true;
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

                        if (Settings.Instance.Verbose) Main.Mod.Logger.Log("end simulation");
                    }
                    else
                    {
                        var angleCorrections = AngleCorrections;

                        if (angleCorrections is not null && angleCorrections.Count != 0)
                        {
                            angleCorrections.Dequeue();
                            if (Settings.Instance.Verbose) Main.Mod.Logger.Log("consume angle correction");
                        }

                        if (Settings.Instance.Verbose) Main.Mod.Logger.Log("skip simulation");
                    }

                    isKeyDown.Clear();
                    isKeyUp.Clear();
                    AnyKeyDown = false;
                    Adofai.Controller.keyTimes.Clear();
                    AllowGameToUpdateInput = false;
                    ConsumeSingleAngleCorrection = false;
                    CachedAngleCorrection = null;
                    lastKeyStates = keyStates;

                    if (ProcessAutoFloorAndFailMiss()) onlyAllowRelease = true;
                }
            }
            finally
            {
                isKeyDown.Clear();
                isKeyUp.Clear();
                Adofai.Controller.keyTimes.Clear();
                AnyKeyDown = false;
                KeyStates = lastKeyStates;
            }
        });
    }

    public static bool OnAngleCorrection(ref double result)
    {
        return WithReplay(ref result, _ =>
        {
            if (!ConsumeSingleAngleCorrection) return CachedAngleCorrection;
            ConsumeSingleAngleCorrection = false;

            var floorId = Adofai.CurrentFloorId;
            var angleCorrections = AngleCorrections;

            if (angleCorrections?.Count == 0)
            {
                Main.Mod.Logger.Warning($"[Floor {floorId}] angle corrections are drained");
                angleCorrections = AngleCorrections = null;
            }

            if (angleCorrections is null) return null;

            var (angleCorrection, eventFloorId) = angleCorrections.Dequeue();

            if (Settings.Instance.Verbose)
                Main.Mod.Logger.Log($"[Floor {floorId}] acc: {eventFloorId} angle: {angleCorrection}");

            return CachedAngleCorrection = angleCorrection;
        });
    }

    public static bool OnGetKeyDown(KeyCode keyCode, ref bool result)
    {
        return keyCode == KeyCode.Escape ||
               WithReplay(ref result, _ => IsKeyDown.GetValueOrDefault((int)keyCode, false));
    }

    public static bool OnGetKeyUp(KeyCode keyCode, ref bool result)
    {
        return keyCode == KeyCode.Escape ||
               WithReplay(ref result, _ => IsKeyUp.GetValueOrDefault((int)keyCode, false));
    }

    public static bool OnGetKey(KeyCode keyCode, ref bool result)
    {
        return keyCode == KeyCode.Escape ||
               WithReplay(ref result, _ => KeyStates.GetValueOrDefault((int)keyCode, false));
    }

    public static bool OnGetAnyKeyDown()
    {
        return AnyKeyDown;
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