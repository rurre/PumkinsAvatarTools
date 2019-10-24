using Pumkin.DataStructures;
using Pumkin.Extensions;
using Pumkin.HelperFunctions;
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Pumkin.HelperFunctions
{
    public static class Helpers
    {
        public static Texture2D OpenImageTexture(ref string startPath)
        {
            Texture2D tex = null;
            string path = EditorUtility.OpenFilePanel("Pick an Image", startPath, "png,jpg,jpeg");
            if(File.Exists(path))
            {
                byte[] data = File.ReadAllBytes(path);
                try
                {
                    tex = new Texture2D(2, 2);
                    tex.LoadImage(data);
                    tex.alphaIsTransparency = true;
                    tex.name = path;
                }
                catch
                {
                    tex = null;
                }
            }
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

        public static bool TransformIsInDefaultPosition(Transform t, bool onlyRotation)
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

        public static void DrawGuiLine(float height = 1f)
        {
            EditorGUILayout.Space();
            GUILayout.Box(GUIContent.none, Styles.Editor_line, GUILayout.ExpandWidth(true), GUILayout.Height(height));
            EditorGUILayout.Space();
        }

        public static void DrawPropertyArrayScrolling(SerializedProperty property, string displayName, ref bool expanded, ref Vector2 scrollPosition, float minHeight, float maxHeight)
        {
            if(property == null)
                return;

            expanded = EditorGUILayout.Foldout(expanded, displayName);
            if(expanded)
            {
                SerializedProperty arraySizeProp = property.FindPropertyRelative("Array.size");
                EditorGUILayout.PropertyField(arraySizeProp, new GUIContent(Strings.Copier.Size ?? "Size"));

                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.MinHeight(Mathf.Clamp(arraySizeProp.intValue * 20, 0, maxHeight)), GUILayout.MaxHeight(maxHeight));
                EditorGUI.indentLevel++;
                for(int i = 0; i < arraySizeProp.intValue; i++)
                {
                    EditorGUILayout.PropertyField(property.GetArrayElementAtIndex(i));
                }
                EditorGUI.indentLevel--;
                EditorGUILayout.EndScrollView();
            }
        }

        public static void DrawPropertyArray(SerializedProperty property, string displayName, ref bool expanded)
        {
            if(property == null)
                return;

            expanded = EditorGUILayout.Foldout(expanded, displayName);
            if(expanded)
            {
                SerializedProperty arraySizeProp = property.FindPropertyRelative("Array.size");
                EditorGUILayout.PropertyField(arraySizeProp, new GUIContent(Strings.Copier.Size ?? "Size"));

                EditorGUI.indentLevel++;

                for(int i = 0; i < arraySizeProp.intValue; i++)
                {
                    EditorGUILayout.PropertyField(property.GetArrayElementAtIndex(i));
                }

                EditorGUI.indentLevel--;
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
    }        
}

namespace Pumkin.Extensions
{
    public static class Extensions
    {
        public static Transform Find(this Transform transform, string name, bool createIfMissing = false, Transform sourceTransform = null)
        {
            var t = transform.Find(name);

            if(t == null && createIfMissing)
            {
                var arr = Helpers.GetPathAsArray(name);
                if(arr.Length > 0)
                {
                    string path = "";
                    for(int i = 0; i < arr.Length; i++)
                    {
                        path += arr[i] + '/';
                        var tNew = transform.Find(path);

                        if(tNew == null)
                        {
                            string s = Helpers.GetPathNoName(path);
                            var parent = transform.Find(s);

                            if(!parent)
                                return null;

                            tNew = new GameObject(arr[i]).transform;
                            tNew.parent = parent;

                            var trans = sourceTransform.root.Find(s + arr[i]);
                            if(trans)
                            {
                                tNew.localScale = Vector3.one;
                                tNew.localPosition = trans.localPosition;
                                tNew.localRotation = trans.localRotation;
                                tNew.localEulerAngles = trans.localEulerAngles;
                                tNew.localScale = trans.localScale;

                                tNew.gameObject.tag = trans.gameObject.tag;
                                tNew.gameObject.layer = trans.gameObject.layer;
                                tNew.gameObject.SetActive(trans.gameObject.activeInHierarchy);

                            }
                            else
                            {
                                tNew.localPosition = Vector3.zero;
                                tNew.localRotation = Quaternion.identity;
                                tNew.localEulerAngles = Vector3.zero;
                                tNew.localScale = Vector3.one;
                            }
                            t = tNew;
                        }
                    }
                }
            }
            return t;
        }

        public static bool IsSameTexture(this Texture2D first, Texture2D second)
        {
            Color[] firstPix = first.GetPixels();
            Color[] secondPix = second.GetPixels();
            if(firstPix.Length != secondPix.Length)
            {
                return false;
            }
            for(int i = 0; i < firstPix.Length; i++)
            {
                if(firstPix[i] != secondPix[i])
                {
                    return false;
                }
            }
            return true;
        }
    }
}
