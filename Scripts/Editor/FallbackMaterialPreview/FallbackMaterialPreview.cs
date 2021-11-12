﻿using Pumkin.AvatarTools.MaterialPreview;
using Pumkin.HelperFunctions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Pumkin.AvatarTools
{
    public class FallbackMaterialPreview : IDisposable
    {
        MaterialCache cache = new MaterialCache();

        GameObject avatar;

        internal FallbackMaterialPreview()
        {
            PumkinsAvatarTools.AvatarSelectionChanged += OnAvatarSelectionChanged;
        }

        internal bool TogglePreview(GameObject target)
        {
            //Get renderers and check if any material names are GUIDs
            var renders = target.GetComponentsInChildren<Renderer>(true);
            bool hasFallbacks = HasCachedMaterials(renders);

            //If they are, restore materials, if they aren't set fallbacks
            if(hasFallbacks)
                RevertFallbacks(renders);
            else
                SetFallbacks(renders);

            return true;
        }

        /// <summary>
        /// Assigns back the original materials to the renderers
        /// </summary>
        /// <param name="renders"></param>
        internal void RevertFallbacks(Renderer[] renders)
        {
            foreach(var r in renders)
            {
                var serial = new SerializedObject(r);
                var materials = serial.FindProperty("m_Materials");

                for(int i = 0; i < r.sharedMaterials.Length; i++)
                {
                    var newMat = cache.GetOriginalMaterialFromCached(r.sharedMaterials[i]);
                    if(!newMat)
                    {
                        PumkinsAvatarTools.Log($"Can't find original material for material <b>slot {i}</b> on <b>{r.gameObject.name}</b>", LogType.Warning);
                        continue;
                    }
                    var mat = materials.GetArrayElementAtIndex(i);
                    mat.objectReferenceValue = newMat;
                }
                PumkinsAvatarTools.Log($"Restored materials for <b>{r.gameObject.name}</b>");
                serial.ApplyModifiedProperties();
            }
        }

        /// <summary>
        /// Sets all materials on the renderers to copies using fallback shaders
        /// </summary>
        /// <param name="renders"></param>
        internal void SetFallbacks(Renderer[] renders)
        {
            foreach(var r in renders)
            {
                var serial = new SerializedObject(r);
                var materials = serial.FindProperty("m_Materials");

                for(int i = 0; i < r.sharedMaterials.Length; i++)
                {
                    var oldMat = r.sharedMaterials[i];
                    if(AssetDatabaseHelpers.IsBuiltInAsset(oldMat))
                        continue;

                    var newMat = cache.GetCachedCopy(oldMat, out bool _);
                    var fallback = PumkinsMaterialManager.CreateFallbackMaterial(newMat, Color.white);

                    var mat = materials.GetArrayElementAtIndex(i);
                    mat.objectReferenceValue = newMat;
                }

                serial.ApplyModifiedProperties();
                AssetDatabase.SaveAssets();
                PumkinsAvatarTools.Log($"Set fallback materials for <b>{r.gameObject.name}</b>");
            }
        }

        void OnAvatarSelectionChanged(GameObject newSelection)
        {
            if(avatar)
            {
                var renders = avatar.GetComponentsInChildren<Renderer>(true);
                if(HasCachedMaterials(renders))
                    RevertFallbacks(renders);
            }
            avatar = newSelection;
        }

        /// <summary>
        /// Returns true if any material in renderers has a GUID as it's name
        /// </summary>
        /// <param name="renderers"></param>
        /// <returns></returns>
        static bool HasCachedMaterials(Renderer[] renderers)
        {
            return renderers
                .Select(r => GUID.TryParse(r.sharedMaterial.name, out GUID _))
                .Any(b => b);
        }

        public void Dispose()
        {
            PumkinsAvatarTools.AvatarSelectionChanged -= OnAvatarSelectionChanged;
        }
    }
}