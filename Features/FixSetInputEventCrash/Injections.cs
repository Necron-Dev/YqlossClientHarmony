using System;
using System.Collections.Generic;
using ADOFAI;
using HarmonyLib;

namespace YqlossClientHarmony.Features.FixSetInputEventCrash;

public static class Injections
{
    [HarmonyPatch(typeof(LevelEvent), nameof(LevelEvent.GetString))]
    public static class Inject_LevelEvent_GetString
    {
        public static Exception? Finalizer(
            LevelEvent __instance,
            ref string __result,
            Exception? __exception,
            string key
        )
        {
            if (!Settings.Instance.Enabled) return __exception;
            if (__exception is null) return null;
            if (__exception is not KeyNotFoundException) return __exception;

            __result = __instance.Get<string>(key);
            return null;
        }
    }
}