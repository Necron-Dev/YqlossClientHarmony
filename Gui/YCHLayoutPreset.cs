using static YqlossClientHarmony.Gui.YCHLayout;
using static YqlossClientHarmony.Utilities.SettingUtil;

namespace YqlossClientHarmony.Gui;

public static class YCHLayoutPreset
{
    public static void OptionNameDescription(
        string name,
        bool description
    )
    {
        if (description)
        {
            Begin(ContainerDirection.Vertical, options: WidthMin);
            {
                Text(I18N.Translate(name), options: WidthMin);
                Text(I18N.Translate($"{name}.Description"), TextStyle.Secondary, WidthMin);
            }
            End();
        }
        else
        {
            Text(I18N.Translate(name), options: WidthMin);
        }
    }

    public static void SwitchOption(
        Sizes sizes,
        ref bool option,
        string name,
        bool description = false,
        bool save = true
    )
    {
        Begin(ContainerDirection.Horizontal, sizes: sizes, options: WidthMax);
        PushAlign(0.5);
        {
            OptionNameDescription(name, description);
            Fill();
            var result = Switch(ref option);
            if (save) Save |= result;
        }
        PopAlign();
        End();
    }

    public static void DoubleOption(
        Sizes sizes,
        ref double option,
        string name,
        IStructFormat<double>? format = null,
        bool description = false,
        bool save = true
    )
    {
        Begin(ContainerDirection.Horizontal, sizes: sizes, options: WidthMax);
        PushAlign(0.5);
        {
            OptionNameDescription(name, description);
            Fill();
            var result = StructField(ref option, format ?? DoubleFormat(), WidthMin);
            if (save) Save |= result;
        }
        PopAlign();
        End();
    }

    public static void TextOption(
        Sizes sizes,
        ref string option,
        string name,
        bool description = false,
        bool save = true
    )
    {
        Begin(ContainerDirection.Horizontal, sizes: sizes, options: WidthMax);
        PushAlign(0.5);
        {
            OptionNameDescription(name, description);
            Fill();
            var result = TextField(ref option, options: WidthMin);
            if (save) Save |= result;
        }
        PopAlign();
        End();
    }

    public static void CheckboxTextOption(
        Sizes sizes,
        ref bool enabled,
        ref string option,
        string name,
        bool description = false,
        bool save = true
    )
    {
        Begin(ContainerDirection.Horizontal, sizes: sizes, options: WidthMax);
        PushAlign(0.5);
        {
            object? result = null;
            result ??= Checkbox(ref enabled);
            OptionNameDescription(name, description);
            Fill();
            result ??= TextField(ref option, options: WidthMin);
            if (save) Save |= result;
        }
        PopAlign();
        End();
    }

    public static void CheckboxSwitchOption(
        Sizes sizes,
        ref bool enabled,
        ref bool option,
        string name,
        bool description = false,
        bool save = true
    )
    {
        Begin(ContainerDirection.Horizontal, sizes: sizes, options: WidthMax);
        PushAlign(0.5);
        {
            object? result = null;
            result ??= Checkbox(ref enabled);
            OptionNameDescription(name, description);
            Fill();
            result ??= Switch(ref option, WidthMin);
            if (save) Save |= result;
        }
        PopAlign();
        End();
    }

    public static void CheckboxDoubleOption(
        Sizes sizes,
        ref bool enabled,
        ref double option,
        string name,
        bool description = false,
        IStructFormat<double>? format = null,
        bool save = true
    )
    {
        Begin(ContainerDirection.Horizontal, sizes: sizes, options: WidthMax);
        PushAlign(0.5);
        {
            object? result = null;
            result ??= Checkbox(ref enabled);
            OptionNameDescription(name, description);
            Fill();
            result ??= StructField(ref option, format ?? DoubleFormat(), WidthMin);
            if (save) Save |= result;
        }
        PopAlign();
        End();
    }

    public static void CheckboxIntOption(
        Sizes sizes,
        ref bool enabled,
        ref int option,
        string name,
        bool description = false,
        IStructFormat<int>? format = null,
        bool save = true
    )
    {
        Begin(ContainerDirection.Horizontal, sizes: sizes, options: WidthMax);
        PushAlign(0.5);
        {
            object? result = null;
            result ??= Checkbox(ref enabled);
            OptionNameDescription(name, description);
            Fill();
            result ??= StructField(ref option, format ?? IntFormat(), WidthMin);
            if (save) Save |= result;
        }
        PopAlign();
        End();
    }

    public static bool IconText(
        Sizes sizes,
        IconStyle icon,
        string text
    )
    {
        var result = false;

        Begin(ContainerDirection.Horizontal, sizes: sizes, options: WidthMax);
        PushAlign(0.5);
        {
            result |= Icon(icon);
            result |= Text(I18N.Translate(text), options: WidthMax);
        }
        PopAlign();
        End();

        return result;
    }

    public static bool Collapse(
        Sizes sizes,
        ref bool expanded,
        string text,
        TextStyle style = TextStyle.Normal
    )
    {
        Begin(ContainerDirection.Horizontal, sizes: sizes, options: WidthMax);
        PushAlign(0.5);
        {
            if (ArrowButton(expanded ? ArrowStyle.Down : ArrowStyle.Right)) expanded = !expanded;
            Text(I18N.Translate(text), style, WidthMax);
        }
        PopAlign();
        End();

        return expanded;
    }
}