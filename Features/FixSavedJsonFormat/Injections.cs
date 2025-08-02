using System;
using ADOFAI;
using GDMiniJSON;
using HarmonyLib;

namespace YqlossClientHarmony.Features.FixSavedJsonFormat;

public static class Injections
{
    [HarmonyPatch(typeof(LevelData), nameof(LevelData.Encode))]
    public static class Inject_LevelData_Encode
    {
        public static void Postfix(
            ref string __result
        )
        {
            if (!SettingsFixSavedJsonFormat.Instance.Enabled) return;

            try
            {
                __result = Json.Serialize(Json.Deserialize(__result));
                Main.Mod.Logger.Log("reformatted saved json");
            }
            catch (Exception exception)
            {
                Main.Mod.Logger.Warning($"failed to reformat saved json: {exception}");
            }
        }
    }
}