using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using ADOFAI;
using GDMiniJSON;
using HarmonyLib;

namespace YqlossClientHarmony.Features.ModifyLoadingLevel;

public static class Injections
{
    private static bool IsInLoadLevelMethod { get; set; }

    private static bool IsLevelModified { get; set; }

    private static bool ConfirmedSaving { get; set; }

    private static bool ShowConfirmSavingDialog()
    {
        return ConfirmedSaving = MessageBox.Show(
            I18N.Translate("Dialog.ModifyLoadingLevel.SaveModifiedLevel.Text"),
            I18N.Translate("Dialog.ModifyLoadingLevel.SaveModifiedLevel.Title"),
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning,
            MessageBoxDefaultButton.Button2
        ) == DialogResult.Yes;
    }

    [HarmonyPatch(typeof(LevelData), nameof(LevelData.LoadLevel))]
    public static class Inject_LevelData_LoadLevel
    {
        public static void Prefix()
        {
            IsInLoadLevelMethod = true;
            IsLevelModified = false;
            ConfirmedSaving = false;
        }

        public static Exception? Finalizer(
            Exception? __exception
        )
        {
            IsInLoadLevelMethod = false;
            return __exception;
        }
    }

    [HarmonyPatch(typeof(RDFile), nameof(RDFile.ReadAllText))]
    public static class Inject_RDFile_ReadAllText
    {
        public static void Postfix(
            ref string __result
        )
        {
            if (!SettingsModifyLoadingLevel.Instance.Enabled) return;
            if (!IsInLoadLevelMethod) return;

            try
            {
                var levelObject = Json.Deserialize(__result);

                if (levelObject is not Dictionary<string, object?> level) return;

                if (level.TryGetValue("settings", out var settingsObject))
                    if (settingsObject is Dictionary<string, object?> settings)
                        ModifySettings(settings);

                if (level.TryGetValue("actions", out var actionsObject))
                    if (actionsObject is List<object?> actions)
                        RemoveEvents(actions);

                if (level.TryGetValue("decorations", out var decorationsObject))
                    if (decorationsObject is List<object?> decorations)
                        RemoveEvents(decorations);

                __result = Json.Serialize(level);

                IsLevelModified = true;
            }
            catch
            {
                // ignored
            }
            finally
            {
                IsInLoadLevelMethod = false;
            }

            return;

            void ModifySettings(Dictionary<string, object?> settings)
            {
                foreach (var levelSettingType in LevelSettingType.Types) levelSettingType.Modify(settings);
            }

            void RemoveEvents(List<object?> events)
            {
                events.RemoveAll(e =>
                    e is Dictionary<string, object?> actionOrDecoration &&
                    EventType.Types.Any(t => t.Matches(actionOrDecoration))
                );
            }
        }
    }

    [HarmonyPatch(typeof(scnEditor), nameof(scnEditor.SaveLevel))]
    public static class Inject_scnEditor_SaveLevel
    {
        public static bool Prefix()
        {
            return !IsLevelModified || ConfirmedSaving || ShowConfirmSavingDialog();
        }
    }

    [HarmonyPatch(typeof(scnEditor), "SaveAndQuit")]
    public static class Inject_scnEditor_SaveAndQuit
    {
        public static bool Prefix()
        {
            return !IsLevelModified || ConfirmedSaving || ShowConfirmSavingDialog();
        }
    }
}