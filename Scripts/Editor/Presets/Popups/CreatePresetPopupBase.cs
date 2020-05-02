using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Pumkin.DataStructures;
using Pumkin.HelperFunctions;
using Pumkin.AvatarTools;
using Pumkin.Presets;

public abstract class CreatePresetPopupBase : EditorWindow
{    
    static protected CreatePresetPopupBase _window;
    static protected PumkinPreset preset;

    static protected Vector2 minWindowSize = new Vector2(340, 520);

    static protected GameObject Avatar { get => PumkinsAvatarTools.SelectedAvatar; }
    static protected bool _overwriteFile = true;
    static protected bool _saveEdittedChanges = true;

    static protected Vector2 scroll = Vector2.zero;

    static protected bool editingExistingPreset = false;

    static protected PumkinPreset AssignOrCreatePreset<T>(PumkinPreset newPreset) where T : PumkinPreset
    {
        editingExistingPreset = true;
        PumkinsPresetManager.CleanupPresetsOfType<T>();
        if(!newPreset)
        {
            newPreset = CreateInstance<T>();
            newPreset.name = "";
            editingExistingPreset = false;
        }
        preset = newPreset;
        return preset;
    }

    protected abstract void RefreshSelectedPresetIndex();

    protected void OnDisable()
    {
        if(editingExistingPreset)
        {
            if(_saveEdittedChanges)
                EditorUtility.SetDirty(preset);

            AssetDatabase.SaveAssets();
            RefreshSelectedPresetIndex();
        }
    }
}