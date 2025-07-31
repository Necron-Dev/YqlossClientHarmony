using System.Collections.Generic;
using System.Linq;
using static YqlossClientHarmony.Gui.YCHLayout;
using static YqlossClientHarmony.Gui.SettingUtil;

namespace YqlossClientHarmony.Gui.Pages;

public static class LanguagePage
{
    private static List<(string, string)> Languages { get; } =
        I18N.LanguageList.Select(lang => (lang.Code, $"{lang.Code} {lang.Name}")).ToList();

    public static void Draw()
    {
        Begin(ContainerDirection.Vertical);
        {
            Text(I18N.Translate("Page.Language.Name"), TextStyle.Title);
            Separator();

            Begin(ContainerDirection.Horizontal);
            {
                Begin(ContainerDirection.Vertical, options: WidthMin);
                {
                    Save |= Selector(ref Main.Settings.Language, Languages, options: WidthMax);
                }
                End();
                Fill();
            }
            End();
        }
        End();
    }
}