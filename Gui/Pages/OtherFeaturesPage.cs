using System;
using YqlossClientHarmony.Features.BlockUnintentionalEscape;
using YqlossClientHarmony.Features.PlaySoundOnGameEnd;
using static YqlossClientHarmony.Gui.YCHLayout;
using static YqlossClientHarmony.Gui.YCHLayoutPreset;
using static YqlossClientHarmony.Gui.SettingUtil;

namespace YqlossClientHarmony.Gui.Pages;

public static class OtherFeaturesPage
{
    private static SizesGroup.Holder Group { get; } = new();

    public static void Draw()
    {
        var group = Group.Begin();

        Begin(ContainerDirection.Vertical);
        {
            Text(I18N.Translate("Page.OtherFeatures.Name"), TextStyle.Title);
            Separator();
            SwitchOption(group, ref Main.Settings.EnableFixKillerDecorationsInNoFail, "Setting.FixKillerDecorationsInNoFail.Enabled");
            Separator();
            SwitchOption(group, ref Main.Settings.EnableFixSetInputEventCrash, "Setting.FixSetInputEventCrash.Enabled");
            Separator();
            SwitchOption(group, ref Main.Settings.EnableRevertCounterclockwiseUTurnPause, "Setting.RevertCounterclockwiseUTurnPause.Enabled");
            Separator();
            SwitchOption(group, ref Main.Settings.EnableFixSavedJsonFormat, "Setting.FixSavedJsonFormat.Enabled");
            Separator();

            var groupBlock = group.Group;
            Begin(ContainerDirection.Horizontal, sizes: groupBlock);
            PushAlign(0.5);
            {
                Save |= Checkbox(ref Main.Settings.EnableBlockUnintentionalEscape);
                Text(I18N.Translate("Setting.BlockUnintentionalEscape.Enabled"));
                Fill();
                var optionsString = I18N.Translate("Setting.BlockUnintentionalEscape.Options");
                var indexOfInSeconds = optionsString.IndexOf("[InSeconds]", StringComparison.Ordinal);
                var indexOfEscapesRequired = optionsString.IndexOf("[EscapesRequired]", StringComparison.Ordinal);
                if (indexOfInSeconds != -1 && indexOfEscapesRequired != -1)
                {
                    if (indexOfInSeconds < indexOfEscapesRequired)
                    {
                        Text(optionsString[..indexOfInSeconds]);
                        Save |= StructField(ref SettingsBlockUnintentionalEscape.Instance.InSeconds, DoubleFormat());
                        Text(optionsString[(indexOfInSeconds + 11)..indexOfEscapesRequired]);
                        Save |= StructField(ref SettingsBlockUnintentionalEscape.Instance.EscapesRequired, IntFormat());
                        Text(optionsString[(indexOfEscapesRequired + 17)..]);
                    }
                    else
                    {
                        Text(optionsString[..indexOfEscapesRequired]);
                        Save |= StructField(ref SettingsBlockUnintentionalEscape.Instance.EscapesRequired, IntFormat());
                        Text(optionsString[(indexOfEscapesRequired + 17)..indexOfInSeconds]);
                        Save |= StructField(ref SettingsBlockUnintentionalEscape.Instance.InSeconds, DoubleFormat());
                        Text(optionsString[(indexOfInSeconds + 11)..]);
                    }
                }
            }
            PopAlign();
            End();

            Separator();
            SwitchOption(group, ref Main.Settings.EnablePlaySoundOnGameEnd, "Setting.PlaySoundOnGameEnd.Enabled");
            Separator();
            TextOption(group, ref SettingsPlaySoundOnGameEnd.Instance.OnWin, "Setting.PlaySoundOnGameEnd.OnWin");
            Separator();
            TextOption(group, ref SettingsPlaySoundOnGameEnd.Instance.OnDeath, "Setting.PlaySoundOnGameEnd.OnDeath");
        }
        End();
    }
}