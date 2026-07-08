using System;
using HarmonyLib;

namespace YqlossClientHarmony.Features.LimitRecolorTrack;

public static class Injections
{
    [HarmonyPatch(typeof(ffxRecolorFloorPlus), nameof(ffxRecolorFloorPlus.Decode))]
    public static class Inject_ffxRecolorFloorPlus_Decode
    {
        public static void Postfix(ffxRecolorFloorPlus __instance)
        {
            if (!SettingsLimitRecolorTrack.Instance.Enabled) return;

            var before = SettingsLimitRecolorTrack.Instance.AllowedBefore;
            var after = SettingsLimitRecolorTrack.Instance.AllowedAfter;

            var start = __instance.start;
            var end = __instance.end;
            var floor = __instance.floorID;

            if (start > end) (start, end) = (end, start);

            if (start <= floor) start = floor - Math.Min(floor - start, before);
            if (floor <= end) end = floor + Math.Min(end - floor, after);

            __instance.start = start;
            __instance.end = end;
        }
    }
}