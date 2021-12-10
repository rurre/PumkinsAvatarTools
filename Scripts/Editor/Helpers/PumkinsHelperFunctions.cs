using Pumkin.AvatarTools;
using Pumkin.DataStructures;
using Pumkin.Extensions;
using Pumkin.HelperFunctions;
using Pumkin.Presets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
#if VRC_SDK_VRCSDK2 || (VRC_SDK_VRCSDK3 && !UDON)
using VRC.Core;
using VRC.SDKBase;
#endif

namespace Pumkin.HelperFunctions
{
    public static class Helpers
    {
#region GUI

        public static void DrawGUILine(float height = 1f, bool spacedOut = true)
        {
            if(spacedOut)
                EditorGUILayout.Space();
            GUILayout.Box(GUIContent.none, Styles.Editor_line, GUILayout.ExpandWidth(true), GUILayout.Height(height));
            if(spacedOut)
                EditorGUILayout.Space();
        }

        /// <summary>
        /// Draws a expandable menu with a toggle checkbox and label
        /// </summary>
        /// <returns>Returns true if toggleBool was changed</returns>
        public static bool DrawDropdownWithToggle(ref bool expandBool, ref bool toggleBool, string label, Texture2D icon = null)
        {
            bool toggleChanged = false;
            GUIContent content = new GUIContent(icon);

            float iconWidth;
            if(icon != null)
                iconWidth = 28f;
            else
                iconWidth = 12f;

            EditorGUILayout.BeginHorizontal();
            {
                Vector2 size = EditorStyles.toggle.CalcSize(new GUIContent(label));
                expandBool = GUILayout.Toggle(expandBool, content, Styles.Foldout, GUILayout.ExpandHeight(true), GUILayout.MaxWidth(iconWidth), GUILayout.MaxHeight(10));
                EditorGUI.BeginChangeCheck();
                toggleBool = GUILayout.Toggle(toggleBool, label, GUILayout.ExpandWidth(true), GUILayout.MinWidth(20), GUILayout.MaxWidth(size.x));
                if(EditorGUI.EndChangeCheck())
                    toggleChanged = true;
                expandBool = GUILayout.Toggle(expandBool, GUIContent.none, GUIStyle.none, GUILayout.MinHeight(20f));
            }
            EditorGUILayout.EndHorizontal();

            return toggleChanged;
        }

        public static void DrawPropertyArrayScrolling(SerializedProperty property, string displayName, ref bool expanded,
            ref Vector2 scrollPosition, float minHeight, float maxHeight, int indentLevel = 0)
        {
            if(property == null)
                return;

            expanded = EditorGUILayout.Foldout(expanded, displayName);
            if(expanded)
            {
                SerializedProperty arraySizeProp = property.FindPropertyRelative("Array.size");
                EditorGUILayout.PropertyField(arraySizeProp, new GUIContent(Strings.Copier.size ?? "Size"));

                int heightMult = arraySizeProp.intValue > 2 ? 21 : 26;
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.MinHeight(Mathf.Clamp(arraySizeProp.intValue * heightMult, 0, maxHeight)), GUILayout.MaxHeight(maxHeight));

                EditorGUI.indentLevel += indentLevel;

                if(indentLevel == 0)
                    EditorGUILayout.Space();

                for(int i = 0; i < arraySizeProp.intValue; i++)
                    EditorGUILayout.PropertyField(property.GetArrayElementAtIndex(i));

                EditorGUI.indentLevel -= indentLevel;

                EditorGUILayout.EndScrollView();
            }
        }

        public static void DrawStringListAsTextFields(ref List<string> list, string displayName, ref bool expanded, int addIndent = 0, bool hasToggle = true)
        {
            if(list == null)
                return;

            int newSize = list.Count;
            EditorGUI.indentLevel += addIndent;

            if(hasToggle)
                expanded = EditorGUILayout.Foldout(expanded, displayName);
            else
                EditorGUILayout.LabelField(displayName);

            newSize = EditorGUILayout.IntField(new GUIContent(Strings.Copier.size), newSize);

            if(expanded || !hasToggle)
            {
                if(addIndent == 0)
                    EditorGUILayout.Space();

                for(int i = 0; i < list.Count; i++)
                    list[i] = EditorGUILayout.TextField(new GUIContent("Element " + i), list[i]);

                if(list.Count != newSize)
                    list.Resize(newSize, "");
            }

            EditorGUI.indentLevel -= addIndent;
            EditorGUILayout.Space();
        }

        public static void DrawPropertyArray(SerializedProperty property, string displayName, ref bool expanded, int indentLevel = 0)
        {
            if(property == null)
                return;

            expanded = EditorGUILayout.Foldout(expanded, displayName);
            if(expanded)
            {
                SerializedProperty arraySizeProp = property.FindPropertyRelative("Array.size");
                EditorGUILayout.PropertyField(arraySizeProp, new GUIContent(Strings.Copier.size ?? "Size"));

                EditorGUI.indentLevel += indentLevel;

                if(indentLevel == 0)
                    EditorGUILayout.Space();

                for(int i = 0; i < arraySizeProp.intValue; i++)
                {
                    EditorGUILayout.PropertyField(property.GetArrayElementAtIndex(i));
                }

                EditorGUI.indentLevel -= indentLevel;
            }
        }

        public static void DrawPropertyArrayWithNames(SerializedProperty property, string displayName, string[] labels, ref bool expanded,
            int indentLevel = 0, float labelWidthOverride = 0, bool allowResize = true)
        {
            if(property == null)
                return;

            expanded = EditorGUILayout.Foldout(expanded, displayName, Styles.Foldout);
            if(expanded)
            {
                SerializedProperty arraySizeProp = property.FindPropertyRelative("Array.size");

                if(allowResize)
                    EditorGUILayout.PropertyField(arraySizeProp, new GUIContent(Strings.Copier.size ?? "Size"));

                EditorGUI.indentLevel += indentLevel;
                if(indentLevel == 0)
                    EditorGUILayout.Space();

                float oldLabelWidth = EditorGUIUtility.labelWidth;
                if(labelWidthOverride > 0)
                    EditorGUIUtility.labelWidth = labelWidthOverride;

                for(int i = 0; i < arraySizeProp.intValue; i++)
                {
                    string label = i < labels.Length ? labels[i] : "Element " + i;
                    EditorGUILayout.PropertyField(property.GetArrayElementAtIndex(i), new GUIContent(label));
                }

                EditorGUI.indentLevel -= indentLevel;

                if(labelWidthOverride > 0)
                    EditorGUIUtility.labelWidth = oldLabelWidth;
            }
        }

        public static void DrawPropertyArrayWithNamesScrolling(SerializedProperty property, string displayName, string[] labels, ref bool expanded,
            ref Vector2 scrollPosition, float minHeight, float maxHeight, int indentLevel = 0, float labelWidthOverride = 0)
        {
            if(property == null)
                return;

            expanded = EditorGUILayout.Foldout(expanded, displayName);
            if(expanded)
            {
                SerializedProperty arraySizeProp = property.FindPropertyRelative("Array.size");
                EditorGUILayout.PropertyField(arraySizeProp, new GUIContent(Strings.Copier.size ?? "Size"));

                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.MinHeight(Mathf.Clamp(arraySizeProp.intValue * 20, 0, maxHeight)), GUILayout.MaxHeight(maxHeight));
                {
                    EditorGUI.indentLevel += indentLevel;
                    if(indentLevel == 0)
                        EditorGUILayout.Space();

                    float oldLabelWidth = EditorGUIUtility.labelWidth;
                    if(labelWidthOverride > 0)
                        EditorGUIUtility.labelWidth = labelWidthOverride;

                    for(int i = 0; i < arraySizeProp.intValue; i++)
                    {
                        string label = i < labels.Length ? labels[i] : "Element " + i;
                        EditorGUILayout.PropertyField(property.GetArrayElementAtIndex(i), new GUIContent(label));
                    }

                    if(labelWidthOverride > 0)
                        EditorGUIUtility.labelWidth = oldLabelWidth;

                    EditorGUI.indentLevel -= indentLevel;
                }
                EditorGUILayout.EndScrollView();
            }
        }

        public static void DrawPropertyArraysHorizontalWithDeleteAndAdd(SerializedProperty[] arrays, string displayName, ref bool expanded, float labelWidthOverride = 0)
        {
            if(arrays == null)
                return;

            HashSet<int> toDeleteIndex = new HashSet<int>();
            int toAdd = 0;

            expanded = EditorGUILayout.Foldout(expanded, displayName);
            if(expanded)
            {
                SerializedProperty arraySizeProp = arrays[0].FindPropertyRelative("Array.size");
                EditorGUI.BeginChangeCheck();
                {
                    EditorGUILayout.PropertyField(arraySizeProp, new GUIContent(Strings.Copier.size ?? "Size"));
                }
                if(EditorGUI.EndChangeCheck())
                {
                    foreach(var array in arrays)
                    {
                        array.arraySize = arraySizeProp.intValue;
                    }
                }

                EditorGUILayout.Space();

                for(int i = 0; i < arraySizeProp.intValue; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.BeginVertical();
                        for(int j = 0; j < arrays.Length; j++)
                        {
                            EditorGUILayout.PropertyField(arrays[j].GetArrayElementAtIndex(i), GUIContent.none);
                        }
                        EditorGUILayout.EndVertical();

                        if(GUILayout.Button(Icons.Delete, Styles.BigIconButton))
                            toDeleteIndex.Add(i);
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                }

                if(GUILayout.Button("Add Element"))
                    toAdd++;

                foreach(int i in toDeleteIndex)
                    foreach(var array in arrays)
                        array.DeleteArrayElementAtIndex(i);

                if(toAdd > 0)
                    foreach(var array in arrays)
                        array.arraySize += toAdd;
            }
        }

#if VRC_SDK_VRCSDK2 || (VRC_SDK_VRCSDK3 && !UDON)
        /// <summary>
        /// Destroys the avatar descriptor and pipeline manager components if they're present on the avatar
        /// </summary>
        public static void DestroyAvatarDescriptorAndPipeline(GameObject avatar)
        {
            if(!avatar)
                return;

            var desc = avatar.GetComponent<VRC_AvatarDescriptor>();
            var pipe = avatar.GetComponent<PipelineManager>();

            DestroyAppropriate(desc);
            DestroyAppropriate(pipe);
        }
#endif

        /// <summary>
        /// Destroys an object. If in edit mode DestroyImmediate is used, if in play mode Destroy is used
        /// </summary>
        public static void DestroyAppropriate(UnityEngine.Object obj, bool allowDestroyingAssets = false)
        {
            if(obj is null)
                return;
            if(EditorApplication.isPlaying)
                UnityEngine.Object.Destroy(obj);
            else
                UnityEngine.Object.DestroyImmediate(obj, allowDestroyingAssets);
        }

        public static float WrapToRange(float num, float min, float max)
        {
            return num = ((num - min) % (max - min)) + min;
        }

        public static void DrawBlendshapeSlidersWithLabels(ref List<PumkinsRendererBlendshapesHolder> rendererHolders, GameObject avatar, int indentLevel = 0, float labelWidthOverride = 0)
        {
            if(rendererHolders == null || avatar == null)
                return;

            EditorGUI.indentLevel += indentLevel;

            bool[] rendererShapeChanged = new bool[rendererHolders.Count];  //record changes in holder to apply to renderer later

            for(int i = 0; i < rendererHolders.Count; i++)
            {
                Transform renderTransform = avatar.transform.Find(rendererHolders[i].rendererPath);

                if(!renderTransform)
                    continue;

                SkinnedMeshRenderer renderer = renderTransform.GetComponent<SkinnedMeshRenderer>();

                if(!renderer)
                    continue;

                EditorGUILayout.Space();
                //Draw renderer dropdown toggle
                EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                {
                    float oldLabelWidth = EditorGUIUtility.labelWidth;
                    EditorGUIUtility.labelWidth = 0;
                    rendererHolders[i].expandedInUI = EditorGUILayout.Toggle(rendererHolders[i].expandedInUI, Styles.Foldout, GUILayout.MaxWidth(10));
                    EditorGUIUtility.labelWidth = oldLabelWidth;

                    EditorGUILayout.LabelField(rendererHolders[i].rendererPath);
                }
                EditorGUILayout.EndHorizontal();

                //Draw renderer dropdown
                if(rendererHolders[i].expandedInUI)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.BeginVertical();
                        for(int j = 0; j < rendererHolders[i].blendshapes.Count; j++)
                        {
                            EditorGUI.BeginChangeCheck();
                            {
                                rendererHolders[i].blendshapes[j].weight = EditorGUILayout.Slider(new GUIContent(rendererHolders[i].blendshapes[j].name), rendererHolders[i].blendshapes[j].weight, 0, 100);
                            }
                            if(EditorGUI.EndChangeCheck())
                            {
                                rendererShapeChanged[i] = true;
                            }
                        }
                        EditorGUILayout.EndVertical();
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.Space();

                    EditorGUILayout.BeginHorizontal();
                    {
                        if(!EditorApplication.isPlaying && GUILayout.Button(Strings.Buttons.revertRenderer) && renderer)
                        {
                            bool expanded = rendererHolders[i].expandedInUI;
                            PumkinsAvatarTools.ResetRendererBlendshapes(renderer, true);
                            rendererHolders[i] = (PumkinsRendererBlendshapesHolder)renderer;
                            rendererHolders[i].expandedInUI = expanded;
                        }
                        if(GUILayout.Button(Strings.Buttons.resetRenderer))
                        {
                            bool expanded = rendererHolders[i].expandedInUI;
                            PumkinsAvatarTools.ResetRendererBlendshapes(renderer, false);
                            rendererHolders[i] = (PumkinsRendererBlendshapesHolder)renderer;
                            rendererHolders[i].expandedInUI = expanded;
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.Space();
                }
            }

            //Apply changes from holder to actual skinned mesh renderer
            for(int i = 0; i < rendererShapeChanged.Length; i++)
            {
                if(!rendererShapeChanged[i])
                    continue;

                SkinnedMeshRenderer ren;
                Transform tRen = avatar.transform.Find(rendererHolders[i].rendererPath);
                if(!tRen)
                    continue;

                ren = tRen.GetComponent<SkinnedMeshRenderer>();

                if(!ren)
                    continue;

                foreach(PumkinsBlendshape blendshape in rendererHolders[i].blendshapes)
                {
                    string name = blendshape.name;
                    int index = ren.sharedMesh.GetBlendShapeIndex(name);

                    if(index == -1 && blendshape.otherNames.Count > 0)
                    {
                        for(int z = 0; z < blendshape.otherNames.Count; z++)
                        {
                            index = ren.sharedMesh.GetBlendShapeIndex(blendshape.otherNames[z]);
                            if(index != -1)
                                break;
                        }
                    }

                    if(index != -1)
                        ren.SetBlendShapeWeight(index, blendshape.weight);
                }
            }

            //if(rendererHolders.Count > 0)
                //DrawGUILine();

            EditorGUI.indentLevel -= indentLevel;
        }

        /// <summary>
        /// Draws sliders meant for renderers and blendshapes, with add and remove buttons for either.
        /// </summary>
        /// <param name="avatar">Avatar to references and apply blendshape changes to. If null changes will just be kept on the preset and not applied</param>
        public static void DrawBlendshapeSlidersWithDeleteAndAdd(ref List<PumkinsRendererBlendshapesHolder> rendererHolders, GameObject avatar, int indentLevel = 0, float labelWidthOverride = 0)
        {
            EditorGUI.indentLevel += indentLevel;
            if(rendererHolders == null)
                return;

            bool[] rendererShapeChanged = new bool[rendererHolders.Count];  //record changes in holder to apply to renderer later
            HashSet<int> toDeleteRendererIndex = new HashSet<int>();

            int toAddRenderer = 0;

            for(int i = 0; i < rendererHolders.Count; i++)
            {
                HashSet<int> toDeleteShapeIndex = new HashSet<int>();
                Transform renderTransform = null;
                SkinnedMeshRenderer renderer = null;

                if(avatar)
                    renderTransform = avatar.transform.Find(rendererHolders[i].rendererPath);

                if(renderTransform)
                    renderer = renderTransform.GetComponent<SkinnedMeshRenderer>();

                int toAddShape = 0;

                EditorGUILayout.Space();
                //Draw renderer dropdown toggle
                EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                {
                    float oldLabelWidth = EditorGUIUtility.labelWidth;
                    EditorGUIUtility.labelWidth = 0;
                    rendererHolders[i].expandedInUI = EditorGUILayout.Toggle(rendererHolders[i].expandedInUI, Styles.Foldout, GUILayout.MaxWidth(10));
                    EditorGUIUtility.labelWidth = oldLabelWidth;

                    rendererHolders[i].rendererPath = EditorGUILayout.TextField(rendererHolders[i].rendererPath);

                    if(GUILayout.Button(Icons.Delete, Styles.IconButton))
                        toDeleteRendererIndex.Add(i);
                }
                EditorGUILayout.EndHorizontal();

                //Draw rendere dropdown
                if(rendererHolders[i].expandedInUI)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.BeginVertical();
                        for(int j = 0; j < rendererHolders[i].blendshapes.Count; j++)
                        {
                            EditorGUI.BeginChangeCheck();
                            {
                                EditorGUILayout.BeginHorizontal();
                                {
                                    rendererHolders[i].blendshapes[j].otherNamesExpandedInUI = EditorGUILayout.Toggle(rendererHolders[i].blendshapes[j].otherNamesExpandedInUI, Styles.Foldout, GUILayout.MaxWidth(10));
                                    rendererHolders[i].blendshapes[j].name = EditorGUILayout.TextField(rendererHolders[i].blendshapes[j].name);
                                    rendererHolders[i].blendshapes[j].weight = EditorGUILayout.Slider(rendererHolders[i].blendshapes[j].weight, 0, 100);

                                    if(GUILayout.Button(Icons.Delete, Styles.IconButton))
                                        toDeleteShapeIndex.Add(j);
                                }
                                EditorGUILayout.EndHorizontal();

                                if(rendererHolders[i].blendshapes[j].otherNamesExpandedInUI)
                                {
                                    DrawStringListAsTextFields(ref rendererHolders[i].blendshapes[j].otherNames, Strings.Presets.otherNames, ref rendererHolders[i].blendshapes[j].otherNamesExpandedInUI, 1, false);
                                }
                            }
                            if(EditorGUI.EndChangeCheck())
                            {
                                rendererShapeChanged[i] = true;
                            }
                        }
                        EditorGUILayout.EndVertical();
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.Space();
                    EditorGUILayout.BeginHorizontal();
                    {
                        if(GUILayout.Button("Reset Renderer"))
                        {
                            foreach(var shape in rendererHolders[i].blendshapes)
                                shape.weight = 0;

                            rendererShapeChanged[i] = true;
                        }

                        if(GUILayout.Button("Add Blendshape"))
                            toAddShape++;
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.Space();

                    foreach(int z in toDeleteShapeIndex)
                        rendererHolders[i].blendshapes.RemoveAt(z);

                    //If we have to add new blendshape containers
                    for(int z = 0; z < toAddShape; z++)
                    {
                        string name = "";
                        float weight = 0;

                        if(renderer)
                        {
                            int count = rendererHolders[i].blendshapes.Count;
                            if(count < renderer.sharedMesh.blendShapeCount)
                            {
                                name = renderer.sharedMesh.GetBlendShapeName(count);
                                if(!string.IsNullOrEmpty(name))
                                    weight = renderer.GetBlendShapeWeight(count);
                            }
                        }
                        rendererHolders[i].blendshapes.Add(new PumkinsBlendshape(name, weight));
                    }
                }
            }

            if(avatar)
            {
                //Apply changes from holder to actual skinned mesh renderer
                for(int i = 0; i < rendererShapeChanged.Length; i++)
                {
                    if(!rendererShapeChanged[i])
                        continue;

                    SkinnedMeshRenderer ren;
                    Transform tRen = avatar.transform.Find(rendererHolders[i].rendererPath);
                    if(!tRen)
                        continue;

                    ren = tRen.GetComponent<SkinnedMeshRenderer>();

                    if(!ren)
                        continue;

                    foreach(PumkinsBlendshape blendshape in rendererHolders[i].blendshapes)
                    {
                        string name = blendshape.name;
                        int index = ren.sharedMesh.GetBlendShapeIndex(name);

                        if(index == -1 && blendshape.otherNames.Count > 0)
                        {
                            for(int z = 0; z < blendshape.otherNames.Count; z++)
                            {
                                index = ren.sharedMesh.GetBlendShapeIndex(blendshape.otherNames[z]);
                                if(index != -1)
                                    break;
                            }
                        }

                        if(index != -1)
                            ren.SetBlendShapeWeight(index, blendshape.weight);
                    }
                }
            }

            if(rendererHolders.Count > 0)
                Helpers.DrawGUILine();

            if(GUILayout.Button("Add Renderer"))
                toAddRenderer++;

            foreach(int i in toDeleteRendererIndex)
                rendererHolders.RemoveAt(i);

            for(int i = 0; i < toAddRenderer; i++)
                rendererHolders.Add(new PumkinsRendererBlendshapesHolder("", new List<PumkinsBlendshape>()));

            EditorGUI.indentLevel -= indentLevel;
        }

        public static void DrawPropertyArraysHorizontal(SerializedProperty[] arrays, string displayName, ref bool expanded, float labelWidthOverride = 0)
        {
            if(arrays == null)
                return;

            expanded = EditorGUILayout.Foldout(expanded, displayName);
            if(expanded)
            {
                SerializedProperty arraySizeProp = arrays[0].FindPropertyRelative("Array.size");
                EditorGUILayout.PropertyField(arraySizeProp, new GUIContent(Strings.Copier.size ?? "Size"));

                EditorGUILayout.Space();

                for(int i = 0; i < arraySizeProp.intValue; i++)
                {
                    foreach(var array in arrays)
                    {
                        EditorGUILayout.PropertyField(array.GetArrayElementAtIndex(i), GUIContent.none);
                    }
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                }
            }
        }

#endregion

        public static Texture2D GetImageTextureFromPath(string imagePath)
        {
            Texture2D tex = null;
            if(File.Exists(imagePath))
            {
                byte[] data = File.ReadAllBytes(imagePath);
                try
                {
                    tex = new Texture2D(2, 2);
                    tex.LoadImage(data);
                    tex.alphaIsTransparency = true;
                    tex.name = imagePath;
                }
                catch
                {
                    tex = null;
                }
            }
            return tex;
        }

        public static T OpenPathGetFile<T>(string startPath, out string filePath)
        {
            var filterStrings = ExtensionStrings.GetFilterString(typeof(T));
            filePath = EditorUtility.OpenFilePanelWithFilters("Pick a File", startPath, filterStrings);

            if(!File.Exists(filePath))
                return default;

            T result;
            byte[] data = File.ReadAllBytes(filePath);
            try
            {
                result = Deserialize<T>(data);
            }
            catch(Exception e)
            {
                Debug.LogError(e.Message);
                result = default;
            }
            return result;
        }

        public static T Deserialize<T>(byte[] param)
        {
            using(MemoryStream ms = new MemoryStream(param))
            {
                IFormatter br = new BinaryFormatter();
                return (T)br.Deserialize(ms);
            }
        }

        public static Transform GetAvatarArmature(GameObject avatar)
        {
            if(!avatar)
                return null;
            return GetAvatarArmature(avatar.transform);
        }

        public static Transform GetAvatarArmature(Transform avatarRoot)
        {
            if(!avatarRoot)
                return null;

            Transform armature = null;
            if((armature = avatarRoot.transform.Find("Armature")) == null)
            {
                //Get the GUID of the avatar, which should also identify the mesh data
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(avatarRoot.GetComponent<Animator>(), out string guid, out long _);
                var renders = avatarRoot.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                foreach(var ren in renders)
                {
                    AssetDatabase.TryGetGUIDAndLocalFileIdentifier(ren.sharedMesh, out string meshGuid, out long _);
                    if(guid != meshGuid)    //Find the correct SkinnedMeshRenderer by comparing this renderer's mesh to the one our animator is using
                        continue;

                    if(ren.rootBone != ren.transform.root) //Get the parent of the root bone, which should be the armature if root is hips
                        armature = ren.rootBone.parent;
                    else
                        armature = ren.rootBone;
                    break;
                }
            }

            return armature;
        }

        public static Texture2D OpenImageGetTexture(ref string startPath)
        {
            Texture2D tex = null;
            string newPath = EditorUtility.OpenFilePanel("Pick an Image", startPath, "png,jpg,jpeg");
            tex = GetImageTextureFromPath(newPath);
            if(tex && !string.IsNullOrEmpty(newPath))
                startPath = newPath;
            return tex;
        }

        public static Texture2D OpenImageGetTextureGUI(ref string path)
        {
            Texture2D texture = Helpers.OpenImageGetTexture(ref path);
            if(texture)
                texture.name = Path.GetFileNameWithoutExtension(path);

            return texture;
        }

        public static string OpenImageGetPath(string startPath = "")
        {
            return EditorUtility.OpenFilePanel("Pick an Image", startPath, "png,jpg,jpeg");
        }

        public static float GetDeltaMultiplier(float startValue, float endValue)
        {
            return (endValue - startValue) / startValue;
        }

        public static bool PhysMaterialsAreIdentical(PhysicMaterial mat1, PhysicMaterial mat2)
        {
            if(mat1 == null && mat2 == null)
                return true;

            if(mat1.bounceCombine == mat2.bounceCombine && mat1.bounciness == mat2.bounciness && mat1.dynamicFriction == mat2.dynamicFriction &&
                mat1.frictionCombine == mat2.frictionCombine && mat1.staticFriction == mat2.staticFriction)
                return true;
            else
                return false;
        }

        public static bool JointsAreIdentical(Joint j1, Joint j2)
        {
            return false;
            //if(j1 == null && j2 == null)
            //    return true;
            //else if(j1.GetType() != j2.GetType())
            //    return false;

            //var j1Body = j1.connectedBody;
            //var j1BodyTrans = j1Body.transform;

            //var j2BodyTrans = FindTransformInAnotherHierarchy(j1.transform, j2.transform.root, false);
            //var j2Body = j2BodyTrans.GetComponent<Rigidbody>();

            //if(!j1BodyTrans || !j2BodyTrans)
            //    return false;

            //return true;
        }

        public static bool CollidersAreIdentical(Collider col1, Collider col2)
        {
            if(col1 == null && col2 == null)
                return true;
            else if(col1.GetType() != col2.GetType())
                return false;

            if(!PhysMaterialsAreIdentical(col1.material, col2.material) ||
                !PhysMaterialsAreIdentical(col1.sharedMaterial, col2.sharedMaterial) || col1.isTrigger != col2.isTrigger)
                return false;

            if(col1 is BoxCollider)
            {
                var c = (BoxCollider)col1;
                var cc = (BoxCollider)col2;

                if(c.size != cc.size || c.center != c.center)
                    return false;
            }
            else if(col1 is CapsuleCollider)
            {
                var c = (CapsuleCollider)col1;
                var cc = (CapsuleCollider)col2;

                if(c.center != cc.center || c.radius != c.radius || c.height != cc.height || c.direction != cc.direction)
                    return false;

            }
            else if(col1 is SphereCollider)
            {
                var c = (SphereCollider)col1;
                var cc = (SphereCollider)col2;

                if(c.center != cc.center || c.radius != cc.radius)
                    return false;
            }
            return true;
        }

        public static Vector3 RoundVectorValues(Vector3 v, int decimals)
        {
            return new Vector3((float)Math.Round(v.x, decimals), (float)Math.Round(v.y, decimals), (float)Math.Round(v.z, decimals));
        }

        public static string NextAvailableFilename(string path)
        {
            string numberPattern = " {0}";

            // Short-cut if already available
            if(!File.Exists(path))
                return path;

            // If path has extension then insert the number pattern just before the extension and return next filename
            if(Path.HasExtension(path))
                return GetNextFilename(path.Insert(path.LastIndexOf(Path.GetExtension(path)), numberPattern));

            // Otherwise just append the pattern to the path and return next filename
            return GetNextFilename(path + numberPattern);
        }

        public static string GetNextFilename(string pattern)
        {
            string tmp = string.Format(pattern, 1);
            if(tmp == pattern)
                throw new System.ArgumentException("The pattern must include an index place-holder", "pattern");

            if(!File.Exists(tmp))
                return tmp; // short-circuit if no matches

            int min = 1, max = 2; // min is inclusive, max is exclusive/untested

            while(File.Exists(string.Format(pattern, max)))
            {
                min = max;
                max *= 2;
            }

            while(max != min + 1)
            {
                int pivot = (max + min) / 2;
                if(File.Exists(string.Format(pattern, pivot)))
                    min = pivot;
                else
                    max = pivot;
            }

            return string.Format(pattern, max);
        }

        public static bool TransformIsInDefaultPositionRotation(Transform t, bool onlyRotation)
        {
            if(t == null)
                return false;

            string tPath = GetTransformPath(t, t.root);

			var pref = PrefabUtility.GetCorrespondingObjectFromSource(t.root.gameObject) as GameObject;

            if(!pref)
                return false;

            Transform tP = pref.transform.Find(tPath);

            if(tP == null)
                return false;

            if(onlyRotation)
            {
                return t.localRotation == tP.localRotation;
            }
            else
            {
                if(t.localPosition == tP.localPosition && t.localRotation == tP.localRotation && t.localEulerAngles == tP.localEulerAngles)
                {
                    return true;
                }
                return false;
            }
        }
        

        /// <summary>
        /// Get path of transform in hierarchy (ex. Armature/Hips/Chest/Neck/Head/Hair01)
        /// </summary>
        /// <param name="trans">Transform to get path of</param>
        /// <param name="root">Hierarchy root</param>
        /// <param name="skipRoot">Whether to skip the root transform in the hierarchy. We want to 99% of the time.</param>
        public static string GetTransformPath(Transform trans, Transform root, bool skipRoot = true)
        {
            if(!trans)
                return string.Empty;

            string path = string.Empty;
            if(trans != root)
            {
                if(!skipRoot)
                    path = trans.name + "/";
                path += AnimationUtility.CalculateTransformPath(trans, root);
            }
            else
            {
                if(!skipRoot)
                    path = root.name;
            }
            return path;
        }

        /// <summary>
        /// Get name of object from path. Really just returns the text after last / or \
        /// </summary>
        public static string GetNameFromPath(string path)
        {
            if(string.IsNullOrEmpty(path))
                return path;

            for(int i = path.Length - 1; i >= 0; i--)
            {
                if(path[i] == '\\' || path[i] == '/')
                {
                    try
                    {
                        return path.Substring(i + 1);
                    }
                    catch
                    {
                        return path;
                    }
                }
            }
            return path;
        }

        /// <summary>
        /// Checks if string is null or whitespaces only. This is missing from some unity versions apparently.
        /// </summary>
        public static bool StringIsNullOrWhiteSpace(string value)
        {
            if (value != null)
            {
                for (int i = 0; i < value.Length; i++)
                {
                    if (!char.IsWhiteSpace(value[i]))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Get path without object name.
        /// </summary>
        /// <returns>Returns all text before last / or \. A paths ending like this (Armature/Hips/) will return Armature/ </returns>
        public static string GetPathNoName(string path)
        {
            if(StringIsNullOrWhiteSpace(path))
                return path;

            string reverse = new string(path.ToArray().Reverse().ToArray());
            char[] slashes = new char[] { '/', '\\' };
            int firstSlash = reverse.IndexOfAny(slashes);

            if (firstSlash == 0)
            {
                if (firstSlash + 1 < reverse.Length)
                    firstSlash = reverse.IndexOfAny(slashes, firstSlash + 1);
                else
                    return "";
            }

            if (firstSlash == -1)
                return "";


            reverse = reverse.Substring(firstSlash);
            string s = new string(reverse.ToArray().Reverse().ToArray());
            return s;

            //int count = path.Count(c => c == '/') + path.Count(c => c == '\\');
            //if (count <= 1) //Special case if we don't have any parents in the path
            //    return "";

            //for(int i = path.Length - 1; i >= 0; i--)
            //{
            //    if((path[i] == '\\' || path[i] == '/'))
            //    {
            //        if(i + 1 < path.Length - 1 )
            //            if(path[i + 1] != '\r')
            //                if(path[i + 1] != '\n')
            //                    return path.Substring(0, i) + '/';

            //    }
            //}
            //return path;
        }

        /// <summary>
        /// Returns path as a string array split by / or \
        /// </summary>
        public static string[] GetPathAsArray(string path)
        {
            if(string.IsNullOrEmpty(path))
                return null;

            return path.Split('\\', '/');
        }

        /// <summary>
        /// Looks for transform in another transform's child hierarchy. Can create if missing.
        /// </summary>
        /// <returns>Found or created transform</returns>
        public static Transform FindTransformInAnotherHierarchy(Transform trans, Transform otherHierarchyRoot, bool createIfMissing)
        {
            if(!trans || !otherHierarchyRoot)
                return null;

            if(trans == trans.root)
                return otherHierarchyRoot.root;

            var childPath = GetTransformPath(trans, trans.root);
            var childTrans = otherHierarchyRoot.Find(childPath, createIfMissing, trans);

            return childTrans;
        }

        /// <summary>
        /// Calculates Scale Multiplier.
        /// </summary>
        public static float GetScaleMultiplier(Transform from, Transform to)
        {
            return from.lossyScale.x / to.lossyScale.x;
        }

        /// <summary>
        /// Returns true if GameObject has no children or components
        /// </summary>
        public static bool GameObjectIsEmpty(GameObject obj)
        {
            if(obj.GetComponentsInChildren<Component>().Length > obj.GetComponentsInChildren<Transform>().Length)
                return false;
            return true;
        }

        /// <summary>
        /// Check whether or not we should ignore the object based on it being in the array.
        /// </summary>
        public static bool ShouldIgnoreObject(Transform trans, Transform[] ignoreArray, bool includeChildren = false)
        {
            if((!trans || ignoreArray == null) || trans == trans.root)
                return false;

            if(ignoreArray.Length > 0 && includeChildren)
            {
                var t = trans;
                do
                {
                    if(ignoreArray.Contains(t))
                        return true;
                    t = t.parent;
                } while(t != null && t != t.root);
                return false;
            }

            if(ignoreArray.Contains(trans))
                return true;
            return false;
        }

        public static void MakeHumanAvatar(string avatarPath, bool reimportModel = true)
        {
            var aEditorType = typeof(Editor).Assembly.GetType("UnityEditor.AvatarEditor");
            var editor = Editor.CreateEditor(AssetDatabase.LoadAssetAtPath<Avatar>(avatarPath), aEditorType);

            var nonPublicInstance = BindingFlags.Instance | BindingFlags.NonPublic;

            aEditorType.GetMethod("SwitchToEditMode", nonPublicInstance).Invoke(editor, null);

            var mapperField = aEditorType.GetField("m_MappingEditor", nonPublicInstance);
            var mapperType = mapperField.FieldType;
            var mapper = mapperField.GetValue(editor);

            mapperType.GetMethod("PerformAutoMapping", nonPublicInstance).Invoke(mapper, null);
            try
            {
                mapperType.GetMethod("MakePoseValid", nonPublicInstance).Invoke(mapper, new object[] { });
            }
            catch(Exception e)
            {
                throw e;
            }
            mapperType.GetMethod("Apply", nonPublicInstance).Invoke(mapper, null);

            aEditorType.GetMethod("SwitchToAssetMode", nonPublicInstance).Invoke(editor, new object[] { false });

            if(!reimportModel)
                return;

            var modelImporter = AssetImporter.GetAtPath(avatarPath) as ModelImporter;
            if(modelImporter != null)
                modelImporter.SaveAndReimport();
        }

        /// <summary>
        /// Sets the camera near clipping plane to 0.01 and far clipping plane to 1000 to prevent near clipping
        /// </summary>
        public static void FixCameraClippingPlanes(Camera cam)
        {
            if(!cam)
                return;

            cam.farClipPlane = 1000;
            cam.nearClipPlane = 0.01f;
        }

        public static GameObject FindGameObjectEvenIfDisabled(string name)
        {
            string lowerName = name.ToLower();
            foreach(GameObject obj in Resources.FindObjectsOfTypeAll(typeof(GameObject)))
                if(obj && obj.name.ToLower() == lowerName)
                    return obj;

            return null;
        }

        public static string ReplaceGUIDInLine(string line, string newGUID, out bool replaced)
        {
            replaced = false;

            string search = "guid: ";
            int guidStart = line.IndexOf(search) + search.Length;
            int guidEnd = line.IndexOf(',', guidStart);
            var lineArr = line.ToCharArray();
            int copyIndex = 0;
            for(int i = guidStart; i < guidEnd; i++)
            {
                if(!replaced && lineArr[i] != newGUID[copyIndex])
                    replaced = true;

                lineArr[i] = newGUID[copyIndex];
                copyIndex++;
            }
            return new string(lineArr);
        }

        public static Vector3 GetViewpointAtEyeLevel(Animator anim)
        {
            Vector3 view = PumkinsAvatarTools.DEFAULT_VIEWPOINT;
            if(anim && anim.isHuman)
            {
                var eye = anim.GetBoneTransform(HumanBodyBones.LeftEye);
                var head = anim.GetBoneTransform(HumanBodyBones.Head);
                if(eye && head)
                {
                    view = head.position - anim.transform.position;
                    float eyeHeight = eye.position.y - 0.005f - anim.transform.position.y;
                    view.y = eyeHeight;
                    view.z = PumkinsAvatarTools.DEFAULT_VIEWPOINT.z - 0.1f;
                }
                else
                {
                    PumkinsAvatarTools.Log("No Eye or Head bones assigned", LogType.Warning);
                    return PumkinsAvatarTools.DEFAULT_VIEWPOINT;
                }
            }
            else
                PumkinsAvatarTools.Log(Strings.Log.cantSetViewpointNonHumanoid);

            return RoundVectorValues(view, 3);
        }

        public static bool IsAssetInAssets(UnityEngine.Object obj)
        {
            string path = AssetDatabase.GetAssetPath(obj);
            if(string.IsNullOrEmpty(path))
                return false;

            if(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path) != null)
                return true;
            return false;
        }

        #if UNITY_2018
        public static bool DestroyMissingScriptsInGameObject(GameObject obj)
        {
            if(EditorApplication.isPlaying)
            {
                PumkinsAvatarTools.Log("Can't remove scripts in play mode. Unity will crash.", LogType.Warning);
                return false;
            }

            var components = obj.GetComponents<Component>();
            var r = 0;
            bool found = false;

            for(var i = 0; i < components.Length; i++)
            {
                if(components[i] != null)
                    continue;

                var serializedObject = new SerializedObject(obj);
                var prop = serializedObject.FindProperty("m_Component");
                prop.DeleteArrayElementAtIndex(i - r);
                r++;
                serializedObject.ApplyModifiedProperties();
                found = true;
            }
            return found;
        }
        #elif UNITY_2019
        public static bool DestroyMissingScriptsInGameObject(GameObject obj)
        {
            return GameObjectUtility.RemoveMonoBehavioursWithMissingScript(obj) > 0;
        }
        
        #endif

        /// <summary>
        /// Checks whether constraint has any valid sources
        /// </summary>
        /// <returns>Returns false if no valid source transforms in constraint</returns>
        internal static bool ConstraintHasValidSources(IConstraint constraint)
        {
            var src = new List<ConstraintSource>();
            constraint.GetSources(src);

            foreach(var c in src)
                if(c.sourceTransform)
                    return true;

            return false;
        }

        public static string NormalizePath(string path)
        {
            return Path.GetFullPath(new Uri(path).LocalPath)
                       .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                       .ToUpperInvariant();
        }

        public static bool PathsAreEqual(string path, string other)
        {
            if(string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(other))
                return false;

            return NormalizePath(path) == NormalizePath(other);
        }

        public static string AbsolutePathToLocalAssetsPath(string path)
        {
            if(path.StartsWith(Application.dataPath))
                path = "Assets" + path.Substring(Application.dataPath.Length);
            return path;
        }

        public static string LocalAssetsPathToAbsolutePath(string localPath)
        {
            localPath = NormalizePathSlashes(localPath);
            const string assets = "Assets/";
            if(localPath.StartsWith(assets))
            {
                localPath = localPath.Remove(0, assets.Length);
                localPath = $"{Application.dataPath}/{localPath}";
            }
            return localPath;
        }

        public static string NormalizePathSlashes(string path)
        {
            if(!string.IsNullOrEmpty(path))
                path = path.Replace('\\', '/');
            return path;
        }

        public static void SelectAndPing(UnityEngine.Object obj)
        {
            if(obj == null)
                return;

            Selection.activeObject = obj;
            EditorGUIUtility.PingObject(obj);
        }

        public static void SelectAndPing(string assetPath)
        {
            if(StringIsNullOrWhiteSpace(assetPath))
                return;

            if(assetPath[assetPath.Length - 1] == '/')
                assetPath = assetPath.Substring(0, assetPath.Length - 1);

            UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath(assetPath, typeof(UnityEngine.Object));
            SelectAndPing(obj);
        }

        /// <summary>
        /// Attempts to replace all object references with the same ones in a different hierarchy
        /// </summary>
        /// <param name="newHierarchyRoot"></param>
        /// <param name="properties"></param>
        /// <param name="setNullIfNotFound">Whether to set to null if not found. If false keep old reference</param>
        /// <typeparam name="T"></typeparam>
        public static void MakeReferencesLocal<T>(Transform newHierarchyRoot, bool setNullIfNotFound, params SerializedProperty[] properties) where T : UnityEngine.Component
        {
            if(newHierarchyRoot == null)
            {
                PumkinsAvatarTools.LogVerbose("localRoot is null");
                return;
            }
            else if(properties == null || properties.Length == 0)
                return;

            var sRoot = new SerializedObject(newHierarchyRoot.gameObject);

            foreach(var prop in properties)
            {
                if(prop == null || prop.propertyType != SerializedPropertyType.ObjectReference)
                    continue;

                Component newComp = null;

                var obj = prop.objectReferenceValue;
                if(obj != null)
                {
                    Transform trans = null;
                    if(obj as Component)
                    {
                        var o = obj as Component;
                        trans = o.transform;
                    }
                    else if(obj as GameObject)
                    {
                        var o = obj as GameObject;
                        trans = o.transform;
                    }
                    else if(obj as Transform)
                    {
                        var o = obj as Transform;
                        trans = o;
                    }
                    else
                    {
                        PumkinsAvatarTools.Log($"{prop.displayName} isn't a GameObject or a component.", LogType.Warning);
                        continue;
                    }

                    var newTrans = FindTransformInAnotherHierarchy(trans, newHierarchyRoot, false);

                    if(newTrans)
                        newComp = newTrans.GetComponent<T>();
                    else if(setNullIfNotFound)
                        PumkinsAvatarTools.Log($"Couldn't find object for <b>{prop.displayName}</b> in <b>{newHierarchyRoot.name}</b>'s hierarchy", LogType.Warning);
                    else
                        continue;
                }
                if(newComp || setNullIfNotFound)
                    prop.objectReferenceValue = newComp;
            }
            sRoot.ApplyModifiedProperties();
        }

        public static void ResetBoundingBoxes(GameObject avatar)
        {
            if(!avatar)
                return;

            var renders = avatar.GetComponentsInChildren<SkinnedMeshRenderer>(true);

            //Revert bounding box to prefab
            foreach(var render in renders)
            {
                var pref = PrefabUtility.GetCorrespondingObjectFromOriginalSource(render);
                if(!pref)
                    continue;

                render.localBounds = pref.localBounds;
            }
        }

        public static void DrawDebugBounds(Bounds b, float lifetime = 5)
        {
            // bottom
            var p1 = new Vector3(b.min.x, b.min.y, b.min.z);
            var p2 = new Vector3(b.max.x, b.min.y, b.min.z);
            var p3 = new Vector3(b.max.x, b.min.y, b.max.z);
            var p4 = new Vector3(b.min.x, b.min.y, b.max.z);

            Debug.DrawLine(p1, p2, Color.blue, lifetime);
            Debug.DrawLine(p2, p3, Color.red, lifetime);
            Debug.DrawLine(p3, p4, Color.yellow, lifetime);
            Debug.DrawLine(p4, p1, Color.magenta, lifetime);

            // top
            var p5 = new Vector3(b.min.x, b.max.y, b.min.z);
            var p6 = new Vector3(b.max.x, b.max.y, b.min.z);
            var p7 = new Vector3(b.max.x, b.max.y, b.max.z);
            var p8 = new Vector3(b.min.x, b.max.y, b.max.z);

            Debug.DrawLine(p5, p6, Color.blue, lifetime);
            Debug.DrawLine(p6, p7, Color.red, lifetime);
            Debug.DrawLine(p7, p8, Color.yellow, lifetime);
            Debug.DrawLine(p8, p5, Color.magenta, lifetime);

            // sides
            Debug.DrawLine(p1, p5, Color.white, lifetime);
            Debug.DrawLine(p2, p6, Color.gray, lifetime);
            Debug.DrawLine(p3, p7, Color.green, lifetime);
            Debug.DrawLine(p4, p8, Color.cyan, lifetime);
        }

        public static void DrawDebugTrianglesFromMesh(Mesh mesh, Transform reference = null)
        {
            bool firstCall = true;
            Vector3 last = Vector3.zero;
            foreach(var vert in mesh.vertices)
            {
                if(firstCall)
                {
                    firstCall = false;
                    last = vert;
                    continue;
                }
                Debug.DrawLine(last, vert, Color.white, 5);
                last = vert;
            }
        }

        /// <summary>
        /// Gets an axis aligned bound box around an array of game objects
        /// </summary>
        /// <param name="objs"></param>
        /// <returns></returns>
        public static Bounds GetBounds(GameObject[] objs)
        {
            if(objs == null || objs.Length == 0)
            {
                return new Bounds(Vector3.zero, Vector3.zero);
            }

            float minX = Mathf.Infinity;
            float maxX = -Mathf.Infinity;
            float minY = Mathf.Infinity;
            float maxY = -Mathf.Infinity;
            float minZ = Mathf.Infinity;
            float maxZ = -Mathf.Infinity;

            Vector3[] points = new Vector3[8];

            foreach(GameObject go in objs)
            {
                GetBoundsPointsNoAlloc(go, points);
                foreach(Vector3 v in points)
                {
                    if(v.x < minX) minX = v.x;
                    if(v.x > maxX) maxX = v.x;
                    if(v.y < minY) minY = v.y;
                    if(v.y > maxY) maxY = v.y;
                    if(v.z < minZ) minZ = v.z;
                    if(v.z > maxZ) maxZ = v.z;
                }
            }

            float sizeX = maxX - minX;
            float sizeY = maxY - minY;
            float sizeZ = maxZ - minZ;

            Vector3 center = new Vector3(minX + sizeX / 2.0f, minY + sizeY / 2.0f, minZ + sizeZ / 2.0f);

            return new Bounds(center, new Vector3(sizeX, sizeY, sizeZ));
        }

        /// <summary>
        /// Pass in a game object and a Vector3[8], and the corners of the mesh.bounds in world space are returned in the passed array
        /// </summary>
        /// <param name="go"></param>
        /// <param name="points"></param>
        public static void GetBoundsPointsNoAlloc(GameObject go, Vector3[] points)
        {
            if(points == null || points.Length < 8)
            {
                Debug.Log("Bad Array");
                return;
            }
            MeshFilter mf = go.GetComponent<MeshFilter>();
            SkinnedMeshRenderer smr = go.GetComponent<SkinnedMeshRenderer>();
            Mesh mesh;
            if(mf)
                mesh = mf.sharedMesh;
            else if(smr)
                mesh = smr.sharedMesh;
            else
            {
                for(int i = 0; i < points.Length; i++)
                    points[i] = go.transform.position;
                return;
            }

            Transform tr = go.transform;

            Vector3 v3Center = mesh.bounds.center;
            Vector3 v3ext = mesh.bounds.extents;

            points[0] = tr.TransformPoint(new Vector3(v3Center.x - v3ext.x, v3Center.y + v3ext.y, v3Center.z - v3ext.z));  // Front top left corner
            points[1] = tr.TransformPoint(new Vector3(v3Center.x + v3ext.x, v3Center.y + v3ext.y, v3Center.z - v3ext.z));  // Front top right corner
            points[2] = tr.TransformPoint(new Vector3(v3Center.x - v3ext.x, v3Center.y - v3ext.y, v3Center.z - v3ext.z));  // Front bottom left corner
            points[3] = tr.TransformPoint(new Vector3(v3Center.x + v3ext.x, v3Center.y - v3ext.y, v3Center.z - v3ext.z));  // Front bottom right corner
            points[4] = tr.TransformPoint(new Vector3(v3Center.x - v3ext.x, v3Center.y + v3ext.y, v3Center.z + v3ext.z));  // Back top left corner
            points[5] = tr.TransformPoint(new Vector3(v3Center.x + v3ext.x, v3Center.y + v3ext.y, v3Center.z + v3ext.z));  // Back top right corner
            points[6] = tr.TransformPoint(new Vector3(v3Center.x - v3ext.x, v3Center.y - v3ext.y, v3Center.z + v3ext.z));  // Back bottom left corner
            points[7] = tr.TransformPoint(new Vector3(v3Center.x + v3ext.x, v3Center.y - v3ext.y, v3Center.z + v3ext.z));  // Back bottom right corner
        }
    }
}
