using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pumkin.DataStructures;
using Pumkin.Translations;
using System.Linq;
using UnityEditor;
using System;
using Pumkin.AvatarTools;

public static class PumkinsLanguageManager
{    
    static readonly string translationsPath = "Translations/";
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
        Languages.Clear();
        Resources.LoadAll<PumkinsTranslation>(translationsPath);
        Languages.AddRange(Resources.FindObjectsOfTypeAll<PumkinsTranslation>());

        if(!Languages.Contains(PumkinsTranslation.Default))
            Languages.Insert(0, PumkinsTranslation.Default);
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
}

