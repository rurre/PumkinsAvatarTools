using Pumkin.HelperFunctions;
using System;
using System.Collections.Generic;
using UnityEngine;

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

        public static Transform FindDeepChild(this Transform aParent, string aName)
        {
            Queue<Transform> queue = new Queue<Transform>();
            queue.Enqueue(aParent);
            while(queue.Count > 0)
            {
                var c = queue.Dequeue();
                if(c.name == aName)
                    return c;
                foreach(Transform t in c)
                    queue.Enqueue(t);
            }
            return null;
        }

        /// <summary>
        /// Returns new array starting at startIndex and ending at the end of the array.
        /// </summary>
        /// <param name="startIndex"/>Index to start at. Inclusive</param>        
        public static T[] SubArray<T>(this T[] array, int startIndex)
        {
            if(array.Length == 0)
                return array;
            if(startIndex < 0 || startIndex > array.Length - 1)
                throw new Exception("Index out of range");

            T[] newArray = new T[array.Length - startIndex];
            for(int i = 0; i < newArray.Length; i++)
                newArray[i] = array[startIndex + i];
            return newArray;
        }

        /// <summary>
        /// Returns a new array starting at startIndex.
        /// </summary>        
        /// <param name="startIndex">Index to start at. Inclusive</param>
        /// <param name="count">Number of elements to include. If bigger than array size, returns elements until the end</param>        
        public static T[] SubArray<T>(this T[] array, int startIndex, int count)
        {
            if(array.Length == 0)
                return array;
            if(startIndex < 0 || startIndex > array.Length - 1)
                throw new Exception("Index out of range");

            count = Mathf.Clamp(count, count, array.Length - 1);

            T[] newArray = new T[count];
            for(int i = 0; i < count; i++)
                newArray[i] = array[startIndex + i];
            return newArray;
        }
    }
}