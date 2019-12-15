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

    static protected GameObject avatar;
    static protected bool _overwriteFile = true;

    static protected Vector2 scroll = Vector2.zero;

    static protected void AssignOrCreatePreset<T>(PumkinPreset newPreset) where T : PumkinPreset
    {
        PumkinsPresetManager.CleanupPresetsOfType<T>();
        if(!newPreset)
            newPreset = CreateInstance<T>();
        preset = newPreset;            
    }
}