using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pumkin.DataStructures;
using Pumkin.Translations;
using System.Linq;
using UnityEditor;
using Pumkin.AvatarTools;
using System.IO;
using UnityEditor.Presets;
using Pumkin.HelperFunctions;
using Pumkin.Dependencies;

public static class PumkinsLanguageManager
{
    static readonly string resourceTranslationPath = "Translations/";
    public static readonly string translationPath = PumkinsAvatarTools.ResourceFolderPath + '/' + resourceTranslationPath;
    public static readonly string translationPathLocal = PumkinsAvatarTools.ResourceFolderPathLocal + '/' + resourceTranslationPath;

    static List<PumkinsTranslation> _languages = new List<PumkinsTranslation>();

    public static List<PumkinsTranslation> Languages
    {
        get
        {
            return _languages;
        }
        private set
        {
            _languages = value;
        }
    }

    static string translationScriptGUID;

    public static void LoadTranslations()
    {
        var guids = AssetDatabase.FindAssets(typeof(PumkinsTranslation).Name);
        translationScriptGUID = guids[0];

        var def = PumkinsTranslation.GetOrCreateDefaultTranslation();
        for(int i = Languages.Count - 1; i >= 0; i--)
        {
            var lang = Languages[i];
            if(i == 0 && def.Equals(lang))
                break;

            if(!Helpers.IsAssetInAssets(lang))
            {
                Helpers.DestroyAppropriate(lang, true); //careful with allow destroying assets here
                Languages.RemoveAt(i);
            }
        }

        //LoadTranslationPresets();
        FixTranslationAssets();

        if(Languages.Count == 0 || !def.Equals(Languages[0]))
            Languages.Insert(0, PumkinsTranslation.Default);

        var loaded = Resources.LoadAll<PumkinsTranslation>(resourceTranslationPath);

        foreach(var l in loaded)
        {
            int i = Languages.IndexOf(l);
            if(i != -1)
                Languages[i] = l;
            else
                Languages.Add(l);
        }

        string langs = "Loaded languages: {";
        for(int i = 0; i < Languages.Count; i++)
        {
            langs += (" " + Languages[i].languageName);
            langs += (i != Languages.Count - 1) ? "," : "";
        }
        langs += " }";
        PumkinsAvatarTools.LogVerbose(langs);
    }

    private static void LoadTranslationPresets()
    {
        var presets = Resources.LoadAll<Preset>(resourceTranslationPath);
        var translations = Resources.LoadAll<PumkinsTranslation>(resourceTranslationPath);

        foreach(var p in presets)
        {
            var mods = p.PropertyModifications;
            string langName = mods.FirstOrDefault(m => m.propertyPath == "languageName").value;
            string author = mods.FirstOrDefault(m => m.propertyPath == "author").value;

            if(Helpers.StringIsNullOrWhiteSpace(langName) || Helpers.StringIsNullOrWhiteSpace(author))
            {
                PumkinsAvatarTools.Log(Strings.Log.invalidTranslation, LogType.Error, p.name);
                continue;
            }

            var tr = translations.FirstOrDefault(t => t.author == author && t.languageName == langName);
            if(tr == default)
                tr = ScriptableObjectUtility.CreateAndSaveAsset<PumkinsTranslation>("language_" + langName, translationPath);

            if(p.CanBeAppliedTo(tr))
                p.ApplyTo(tr);
            else
                PumkinsAvatarTools.Log(Strings.Log.cantApplyPreset, LogType.Error);
        }
    }

    public static int GetIndexOfLanguage(string nameAndAuthor)
    {
        for(int i = 0; i < Languages.Count; i++)
        {
            if(Languages[i] != null && Languages[i].ToString().ToLower() == nameAndAuthor.ToLower())
                return i;
        }
        return 0;
    }

    public static PumkinsTranslation FindLanguageFile(string nameAndAuthor)
    {
        string s = nameAndAuthor.ToLower();
        return Languages.FirstOrDefault(o => o.ToString().ToLower() == s);
    }

    public static void SetLanguage(PumkinsTranslation translationFile)
    {
        Strings.Translation = translationFile;
    }

    public static void SetLanguage(string languageName, string author)
    {
        if(string.IsNullOrEmpty(languageName))
            return;
        SetLanguage((languageName + " - " + author).ToLower());
    }

    public static void SetLanguage(string languageAndAuthor)
    {
        if(string.IsNullOrEmpty(languageAndAuthor))
            return;
        var s = languageAndAuthor.ToLower();
        for(int i = 0; i < Languages.Count; i++)
        {
            if(Languages[i] && s == Languages[i].ToString().ToLower())
            {
                SetLanguage(Languages[i]);
                return;
            }
        }
        SetLanguage(PumkinsTranslation.Default);
    }

    public static bool LanguageExists(PumkinsTranslation translation)
    {
        if(translation == null)
            return false;
        return LanguageExists(translation.languageName, translation.author);
    }

    public static bool LanguageExists(string languageName, string author)
    {
        if(Helpers.StringIsNullOrWhiteSpace(languageName) || Helpers.StringIsNullOrWhiteSpace(author))
            return false;
        var lang = Languages.FirstOrDefault(l => (l.author == author) && (l.languageName == languageName));
        return lang != default(PumkinsTranslation) ? true : false;
    }

    public static void OpenFileImportLanguagePreset()
    {
        var filterStrings = ExtensionStrings.GetFilterString(typeof(PumkinsTranslation));
        string filePath = EditorUtility.OpenFilePanelWithFilters("Pick a Translation", "", filterStrings);
        var asset = ImportLanguagePreset(filePath);

        if(asset != null)
            EditorGUIUtility.PingObject(asset);
    }

    public static Preset ImportLanguagePreset(string path)
    {
        if(Helpers.StringIsNullOrWhiteSpace(path))
            return null;

        string newPath = translationPath + Path.GetFileName(path);

        bool shouldDelete = false;
        if(File.Exists(newPath))
            if(EditorUtility.DisplayDialog(Strings.Warning.warn, Strings.Warning.languageAlreadyExistsOverwrite, Strings.Buttons.ok, Strings.Buttons.cancel))
                shouldDelete = true;

        if(!Helpers.PathsAreEqual(path, newPath))
        {
            if(shouldDelete)
                File.Delete(newPath);
            File.Copy(path, newPath);
        }
        ReplaceTranslationGUIDTemp(newPath, "m_ManagedTypePPtr", translationScriptGUID);

        string newPathLocal = Helpers.AbsolutePathToLocalAssetsPath(newPath);
        AssetDatabase.ImportAsset(newPathLocal);

        LoadTranslations();

        return AssetDatabase.LoadAssetAtPath<Preset>(newPathLocal);
    }

    /// <summary>
    /// TODO: Replace with one that reads only the needed lines
    /// </summary>    
    static bool ReplaceTranslationGUIDTemp(string filePath, string lineIdentifier, string newGUID)
    {
        bool replaced = false;
        var lines = File.ReadAllLines(filePath);
        for(int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];

            if(!line.Contains(lineIdentifier))
                continue;

            lines[i] = Helpers.ReplaceGUIDInLine(line, newGUID, out replaced);
            break;
        }
        if(replaced)
            File.WriteAllLines(filePath, lines);
        return replaced;
    }

    static void FixTranslationAssets()
    {
        var files = Directory.GetFiles(Helpers.LocalAssetsPathToAbsolutePath(translationPathLocal));
        foreach(var path in files)
        {
            string localPath = Helpers.AbsolutePathToLocalAssetsPath(path);
            if(ReplaceTranslationGUIDTemp(path, "m_Script", translationScriptGUID))
                AssetDatabase.ImportAsset(localPath);

            PumkinsTranslation translation = AssetDatabase.LoadAssetAtPath<PumkinsTranslation>(localPath);
            translation?.FixEmptyFields();
        }
    }
}

