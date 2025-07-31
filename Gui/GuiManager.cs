using System;
using YqlossClientHarmony.Gui.Pages;
using static YqlossClientHarmony.Gui.YCHLayout;

namespace YqlossClientHarmony.Gui;

public static class GuiManager
{
    private static int _currentPage;

    private static string[] PageNames =>
    [
        I18N.Translate("Page.Language.Name"),
        I18N.Translate("Page.Replay.Name"),
        I18N.Translate("Page.EffectRemover.Name"),
        I18N.Translate("Page.OtherFeatures.Name")
    ];

    private static Action[] PageRenderers { get; } =
    [
        LanguagePage.Draw,
        ReplayPage.Draw,
        EffectRemoverPage.Draw,
        OtherFeaturesPage.Draw
    ];

    public static void Draw()
    {
        EnsureTexturesAlive();

        Begin(ContainerDirection.Vertical, ContainerStyle.Padding);
        {
            Begin(ContainerDirection.Horizontal);
            {
                Space(4);
                Selector(ref _currentPage, PageNames, options: WidthMin);
                Fill();
            }
            End();

            Begin(ContainerDirection.Vertical, ContainerStyle.Background, options: WidthMax);
            {
                PageRenderers[_currentPage]();
            }
            End();
        }
        End();
    }
}