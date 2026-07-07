using HarmonyLib;
using UnityEngine;

namespace YqlossClientHarmony.Features.DisablePauseInAuto;

public static class Injections
{
    [HarmonyPatch(typeof(Input), nameof(Input.GetKeyDown), typeof(KeyCode))]
    public static class Inject_Input_GetKeyDown
    {
        public static bool Prefix(
            ref bool __result,
            KeyCode key
        )
        {
            if (
                !SettingsDisablePauseInAuto.Instance.Enabled
                || key != KeyCode.Space
                || !RDC.auto
                || !scnEditor.instance.playMode
            ) return true;
            __result = false;
            return false;
        }
    }
}