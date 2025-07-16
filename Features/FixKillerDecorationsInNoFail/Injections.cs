using HarmonyLib;

namespace YqlossClientHarmony.Features.FixKillerDecorationsInNoFail;

public static class Injections
{
    [HarmonyPatch(typeof(scrDecoration), nameof(scrDecoration.HitboxTriggerAction))]
    public static class Inject_scrDecoration_HitboxTriggerAction
    {
        public static void Prefix(
            scrDecoration __instance,
            out HitboxType __state,
            scrPlanet? planet
        )
        {
            __state = __instance.hitbox;

            if (!Settings.Instance.Enabled) return;
            if (!Adofai.Controller.gameworld) return;
            if (__instance.hitbox != HitboxType.Kill) return;
            if (RDC.auto) return;
            if (!ADOBase.controller.noFail) return;

            __instance.hitbox = HitboxType.None;

            if (planet != null && planet.iFrames > 0) return;
            if (__instance.hitOnce) return;

            Adofai.Controller.mistakesManager.AddHit(HitMargin.FailMiss);
            Adofai.Controller.errorMeter.AddHit(float.PositiveInfinity);
            Adofai.Controller.chosenPlanet.MarkFail()?.BlinkForSeconds(3);
        }

        public static void Postfix(
            scrDecoration __instance,
            HitboxType __state
        )
        {
            __instance.hitbox = __state;
        }
    }
}