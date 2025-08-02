using static YqlossClientHarmony.Gui.YCHLayout;
using static YqlossClientHarmony.Gui.YCHLayoutPreset;

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
        }
        End();
    }
}