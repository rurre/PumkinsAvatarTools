using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pumkin.DataStructures;
using Pumkin.Translations;
using System.Linq;
using UnityEditor;
using System;
using Pumkin.AvatarTools;
using System.IO;
using UnityEditor.Presets;
using Pumkin.HelperFunctions;

public static class PumkinsLanguageManager
{    
    public static readonly string translationsPath = "Translations/";
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
    
    public static void LoadTranslations()
    {
        foreach(var lang in Languages)
        {
            if(Helpers.IsAssetInAssets(lang))
                Helpers.DestroyAppropriate(lang);
        }
        
        Resources.LoadAll<PumkinsTranslation>(translationsPath);
        var all = Resources.FindObjectsOfTypeAll<PumkinsTranslation>();
        var orphans = all.Where(l => !Helpers.IsAssetInAssets(l)).ToList();
        foreach(var o in orphans)
        {
            PumkinsAvatarTools.LogVerbose("Destroying orphanned " + o.ToString());
            Helpers.DestroyAppropriate(o, true);
        }

        Languages.AddRange(all.Where(o => o != null));
        var def = PumkinsTranslation.GetOrCreateDefaultTranslation();

        if(Languages.Count == 0 || !def.Equals(Languages[0]))
            Languages.Insert(0, PumkinsTranslation.Default);

        //var duplicates = Languages.Where(l => PumkinsTranslation.Default.Equals(l)).Skip(1);
        
        //foreach(var d in duplicates)        
        //    Helpers.DestroyAppropriate(d);        
                
        string langs = "Loaded languages: { ";
        foreach (var l in Languages)        
            langs += (" " + l.languageName  + ",");
        langs += " }";
        PumkinsAvatarTools.LogVerbose(langs);
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
        if (translation == null)
            return false;
        return LanguageExists(translation.languageName, translation.author);
    }

    public static bool LanguageExists(string languageName, string author)
    {
        if (Helpers.StringIsNullOrWhiteSpace(languageName) || Helpers.StringIsNullOrWhiteSpace(author))
            return false;
        var lang = Languages.FirstOrDefault(l => (l.author == author) && (l.languageName == languageName));
        return lang != default(PumkinsTranslation) ? true : false;
    }

    public static void ImportLanguageAsset(string path)
    {        
        var lang = Helpers.OpenPathGetFile<PumkinsTranslation>(path, out _);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
    }    
}

