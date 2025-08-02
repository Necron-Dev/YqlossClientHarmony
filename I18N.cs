using System.Collections.Generic;
using System.IO;
using System.Text;

namespace YqlossClientHarmony;

public static class I18N
{
    public const string DefaultLanguage = "en_US";

    static I18N()
    {
        Main.Mod.Logger.Log("loading languages");

        foreach (var file in Directory.EnumerateFiles(Path.Combine(Main.Mod.Path, "Languages")))
        {
            var fileName = Path.GetFileName(file);
            if (!fileName.ToLower().EndsWith(".lang")) continue;
            var language = new Language(fileName[..^5]);
            if (!language.NotLanguage) LanguageList.Add(language);
            LanguageMap[language.Code] = language;
            Main.Mod.Logger.Log($"found language: {language.Code} {language.Name}");
        }
    }

    public static List<Language> LanguageList { get; } = [];

    public static Dictionary<string, Language> LanguageMap { get; } = [];

    public static Language SelectedLanguage
    {
        get
        {
            var code = Main.Settings.Language;
            if (LanguageMap.TryGetValue(code, out var language)) return language;
            Main.Mod.Logger.Warning($"language {code} not found, defaulting to {DefaultLanguage}");
            Main.Settings.Language = DefaultLanguage;
            Main.Settings.Save(Main.Mod);
            return SelectedLanguage;
        }
    }

    public static string Translate(string key, params object?[] args)
    {
        return SelectedLanguage.Translate(key, args) ?? key;
    }

    public class Language
    {
        public Language(string code)
        {
            Code = code;

            var lines = File.ReadAllLines(Path.Combine(Main.Mod.Path, "Languages", $"{code}.lang"), Encoding.UTF8);

            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (trimmed.IsNullOrEmpty() || trimmed.StartsWith('#')) continue;
                var split = trimmed.Split('=', 2);
                Translations[split[0]] = split[1]
                    .Replace("\\n", "\n")
                    .Replace("\\s", " ")
                    .Replace("\\/", "\\");
            }

            Name = Translations.GetValueOrDefault("Name", code);

            NotLanguage = Translations.ContainsKey("NotLanguage");

            if (Translations.TryGetValue("Parents", out var parentsString))
                Parents.AddRange(parentsString.Split(' '));
        }

        public string Code { get; }

        public string Name { get; }

        public bool NotLanguage { get; }

        private Dictionary<string, string> Translations { get; } = [];

        private List<string> Parents { get; } = [];

        public string? Translate(string key, params object?[] args)
        {
            var translation = Translations.GetValueOrDefault(key);
            if (translation is not null) return string.Format(translation, args);

            foreach (var parent in Parents)
            {
                translation = LanguageMap.GetValueOrDefault(parent).Translate(key, args);
                if (translation is not null) return translation;
            }

            return null;
        }
    }
}