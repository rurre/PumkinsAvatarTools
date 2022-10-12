using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Pumkin.AvatarTools.Copiers
{
    public class CopyInstance
    {
        public GameObject from;
        public GameObject to;
        public List<CopierPropertyReference> propertyRefs;

        public CopyInstance(GameObject copyFrom, GameObject copyTo)
        {
            from = copyFrom;
            to = copyTo;
            propertyRefs = new List<CopierPropertyReference>();
        }
    }

    public class CopierPropertyReference
    {
        public SerializedObject serialiedObject;
        public string[] propertyPaths;
        public Transform[] targetTransforms;
    }
}