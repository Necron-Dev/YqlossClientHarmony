using System;
using System.IO;
using System.Threading;
using HarmonyLib;
using MonsterLove.StateMachine;
using UnityEngine;
using UnityEngine.Networking;

namespace YqlossClientHarmony.Features.PlaySoundOnGameEnd;

public static class Injections
{
    private static Enum? State { get; set; }

    private static void TryPlaySound(string path)
    {
        if (path.IsNullOrEmpty())
        {
            Main.Mod.Logger.Log("no sound file specified");
            return;
        }

        try
        {
            var audioType = Path.GetExtension(path) switch
            {
                ".ogg" => AudioType.OGGVORBIS,
                ".wav" => AudioType.WAV,
                ".mp3" => AudioType.MPEG,
                ".aiff" => AudioType.AIFF,
                _ => AudioType.UNKNOWN
            };

            using var request = UnityWebRequestMultimedia.GetAudioClip(path, audioType);
            request.SendWebRequest();
            while (!request.isDone) Thread.Yield();

            if (request.error.IsNullOrEmpty())
            {
                var clip = DownloadHandlerAudioClip.GetContent(request);
                scrSfx.instance.PlaySfx(clip, MixerGroup.ConductorSfx);
            }
            else
            {
                Main.Mod.Logger.Log($"failed to load sound {path}: {request.error}");
            }
        }
        catch (Exception exception)
        {
            Main.Mod.Logger.Log($"failed to play sound {path}: {exception}");
        }
    }

    [HarmonyPatch(typeof(scrController), nameof(scrController.OnLandOnPortal))]
    public static class Inject_scrController_OnLandOnPortal
    {
        public static void Postfix()
        {
            if (!SettingsPlaySoundOnGameEnd.Instance.Enabled) return;

            if (!Equals(States.Won, State)) return;
            Main.Mod.Logger.Log("playing win sound");
            TryPlaySound(SettingsPlaySoundOnGameEnd.Instance.OnWin);
        }
    }

    [HarmonyPatch(typeof(scrController), nameof(scrController.FailAction))]
    public static class Inject_scrController_FailAction
    {
        public static void Postfix()
        {
            if (!SettingsPlaySoundOnGameEnd.Instance.Enabled) return;

            if (!Equals(States.Fail, State)) return;
            Main.Mod.Logger.Log("playing death sound");
            TryPlaySound(SettingsPlaySoundOnGameEnd.Instance.OnDeath);
        }
    }

    [HarmonyPatch(typeof(StateEngine), nameof(StateEngine.ChangeState))]
    public static class Inject_StateEngine_ChangeState
    {
        public static void Prefix(Enum newState)
        {
            State = newState;
        }
    }
}