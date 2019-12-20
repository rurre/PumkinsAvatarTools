using Pumkin.AvatarTools;
using Pumkin.DataStructures;
using Pumkin.Extensions;
using Pumkin.HelperFunctions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Pumkin.HelperFunctions
{
    public static class Helpers
    {
        #region GUI
        
        /// <summary>
        /// Arbitrarely try to guess the width of a text string. For now
        /// </summary>        
        public static float CalculateTextWidth(string text, GUIStyle style = null)
        {
            if(style == null)
                style = new GUIStyle("label");

            return text.Length * style.font.fontSize * 0.8f;
        }

        public static void DrawGUILine(float height = 1f, bool spacedOut = true)
        {
            if(spacedOut)
                EditorGUILayout.Space();
            GUILayout.Box(GUIContent.none, Styles.Editor_line, GUILayout.ExpandWidth(true), GUILayout.Height(height));
            if(spacedOut)
                EditorGUILayout.Space();
        }

        public static void DrawDropdownWithToggle(ref bool expandBool, ref bool toggleBool, string label, Texture2D icon = null)
        {
            float oldWidth = EditorGUIUtility.labelWidth;
            float newWidth = Helpers.CalculateTextWidth(label);
            GUIContent content = new GUIContent(icon);

            float iconWidth;
            if(icon != null)
                iconWidth = 28f;
            else
                iconWidth = 12f;

            EditorGUILayout.BeginHorizontal();
            {
                expandBool = GUILayout.Toggle(expandBool, content, Styles.Foldout, GUILayout.ExpandHeight(true), GUILayout.MaxWidth(iconWidth), GUILayout.MaxHeight(10));
                toggleBool = GUILayout.Toggle(toggleBool, label, GUILayout.ExpandWidth(true), GUILayout.MinWidth(20), GUILayout.MaxWidth(Helpers.CalculateTextWidth(label)));
                expandBool = GUILayout.Toggle(expandBool, GUIContent.none, GUIStyle.none, GUILayout.MinHeight(20f));
            }
            EditorGUILayout.EndHorizontal();
            EditorGUIUtility.labelWidth = oldWidth;
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

                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.MinHeight(Mathf.Clamp(arraySizeProp.intValue * 20, 0, maxHeight)), GUILayout.MaxHeight(maxHeight));
                                
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


                    if(GUILayout.Button(Strings.Buttons.revertRenderer))
                    {
                        var prefRender = PrefabUtility.GetCorrespondingObjectFromSource(renderer);
                        if(prefRender)
                        {
                            rendererHolders[i] = (PumkinsRendererBlendshapesHolder)prefRender;                            
                            rendererShapeChanged[i] = true;
                        }
                        else
                        {
                            PumkinsAvatarTools.Log(Strings.Warning.cantRevertRendererWithoutPrefab, LogType.Warning, renderer.gameObject.name);
                        }
                    }

                    if(GUILayout.Button(Strings.Buttons.resetRenderer))
                    {
                        foreach(var shape in rendererHolders[i].blendshapes)
                            shape.weight = 0;

                        rendererShapeChanged[i] = true;
                    }

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

            if(rendererHolders.Count > 0)
                Helpers.DrawGUILine();

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
                                    DrawStringListAsTextFields(ref rendererHolders[i].blendshapes[j].otherNames, Strings.Preset.otherNames, ref rendererHolders[i].blendshapes[j].otherNamesExpandedInUI, 1, false);                                    
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

        public static Texture2D OpenImageTexture(ref string startPath)
        {
            Texture2D tex = null;
            string path = EditorUtility.OpenFilePanel("Pick an Image", startPath, "png,jpg,jpeg");
            tex = GetImageTextureFromPath(path);
            if(tex)
                startPath = path;

            return tex;
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

        internal static void DrawPropertyArrayWithNames(SerializedProperty pMuscles, object muscles, string[] defaultMusclesNames, ref bool muscles_expand, bool v1, int v2)
        {
            throw new NotImplementedException();
        }

        public static bool JointsAreIdentical(Joint j1, Joint j2)
        {
            throw new NotImplementedException();

            //if(j1 == null && j2 == null)
            //    return true;
            //else if(j1.GetType() != j2.GetType())
            //    return false;            

            //if(j1 is FixedJoint)
            //{
            //    var j = (FixedJoint)j1;
            //    var jj = (FixedJoint)j2;                
            //}

            //if(j1 is HingeJoint)
            //{
            //    var j = (HingeJoint)j1;
            //    var jj = (HingeJoint)j2;
            //}

            //if(j1 is SpringJoint)
            //{
            //    var j = (SpringJoint)j1;
            //    var jj = (SpringJoint)j2;
            //}

            //if(j1 is CharacterJoint)
            //{
            //    var j = (CharacterJoint)j1;
            //    var jj = (CharacterJoint)j2;
            //}

            //if(j1 is ConfigurableJoint)
            //{
            //    var j = (ConfigurableJoint)j1;
            //    var jj = (ConfigurableJoint)j2;
            //}            
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

            string tPath = GetGameObjectPath(t.gameObject);

#if UNITY_2017
            var pref = PrefabUtility.GetPrefabParent(t.root.gameObject) as GameObject;
#else
			var pref = PrefabUtility.GetCorrespondingObjectFromSource(t.root.gameObject) as GameObject;
#endif            
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
        /// <param name="skipRoot">Whether to skip the root transform in the hierarchy. We want to 99% of the time.</param>        
        public static string GetGameObjectPath(Transform trans, bool skipRoot = true)
        {
            if(trans != null)
                return GetGameObjectPath(trans.gameObject, skipRoot);
            return null;
        }

        /// <summary>
        /// Get path of GameObject's transform in hierarchy (ex. Armature/Hips/Chest/Neck/Head/Hair01)
        /// </summary>
        /// <param name="obj">GameObject to get path of</param>
        /// <param name="skipRoot">Whether to skip the root transform in the hierarchy. We want to 99% of the time.</param>        
        public static string GetGameObjectPath(GameObject obj, bool skipRoot = true)
        {
            if(!obj)
                return string.Empty;

            string path = string.Empty;
            if(obj.transform != obj.transform.root)
            {
                if(!skipRoot)
                    path = obj.transform.root.name + "/";
                path += (AnimationUtility.CalculateTransformPath(obj.transform, obj.transform.root));
            }
            else
            {
                if(!skipRoot)
                    path = obj.transform.root.name;
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
        /// Get path without object name.
        /// </summary>        
        /// <returns>Returns all text before last / or \. A paths ending like this (Armature/Hips/) will return Armature/ </returns>
        public static string GetPathNoName(string path)
        {
            if(string.IsNullOrEmpty(path))
                return path;

            for(int i = path.Length - 1; i >= 0; i--)
            {
                if((path[i] == '\\' || path[i] == '/'))
                {
                    if(i + 1 < path.Length - 1 && (path[i + 1] != '\r' && path[i + 1] != '\n'))
                    {
                        return path.Substring(0, i) + '/';
                    }
                }
            }
            return path;
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
        /// Looks for object in another object's child hierarchy. Can create if missing.
        /// </summary>                
        /// <returns>Transform of found object</returns>
        public static Transform FindTransformInAnotherHierarchy(Transform trans, Transform otherHierarchyTrans, bool createIfMissing)
        {
            if(!trans || !otherHierarchyTrans)
                return null;

            if(trans == trans.root)
                return otherHierarchyTrans.root;

            var childPath = GetGameObjectPath(trans);
            var childTrans = otherHierarchyTrans.Find(childPath, createIfMissing, trans);

            return childTrans;
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
    }        
}
