using Pumkin.AvatarTools;
using Pumkin.DataStructures;
using Pumkin.HelperFunctions;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pumkin.Presets
{
    public class PumkinsBlendshapePreset : PumkinPreset
    {        
        List<string> skinnedMeshRendererPaths;
        List<PumkinsBlendshape[]> blendshapeArrays;        

        public static PumkinsBlendshapePreset CreatePreset(string presetName, GameObject avatar)
        {
            PumkinsBlendshapePreset preset = CreateInstance<PumkinsBlendshapePreset>();
            preset.SetupPreset(presetName, avatar);
            return preset;
        }

        public override bool ApplyPreset(GameObject avatar)
        {
            if(!avatar)
                return false;

            for(int i = 0; i < skinnedMeshRendererPaths.Count; i++)
            {
                var t = avatar.transform.Find(skinnedMeshRendererPaths[i]);                
                if(t)
                {
                    var render = t.GetComponent<SkinnedMeshRenderer>();
                    if(render)
                    {
                        for(int j = 0; j < blendshapeArrays[i].Length; j++)
                        {
                            int index = render.sharedMesh.GetBlendShapeIndex(blendshapeArrays[i][j].Name);
                            float weight = blendshapeArrays[i][j].Weight;                            
                            render.SetBlendShapeWeight(index, weight);
                        }
                    }
                }
            }
            return true;
        }

        public void SavePreset(bool overwriteExisting)
        {
            ScriptableObjectUtility.SaveAsset(this, name, PumkinsAvatarTools.MainFolderPath + "/Resources/Presets/Blendshapes/", overwriteExisting);
            PumkinsPresetManager.LoadPresets<PumkinsBlendshapePreset>();
        }

        public void SetupPreset(string presetName, GameObject avatar)
        {
            if(!avatar)
                return;

            var renders = avatar.GetComponentsInChildren<SkinnedMeshRenderer>();
            skinnedMeshRendererPaths = new List<string>(renders.Length);
            blendshapeArrays = new List<PumkinsBlendshape[]>(renders.Length);

            for(int i = 0; i < renders.Length; i++)
            {
                var r = renders[i];
                skinnedMeshRendererPaths.Add(Helpers.GetGameObjectPath(r.gameObject, true));
                PumkinsBlendshape[] shapes = new PumkinsBlendshape[r.sharedMesh.blendShapeCount];
                for(int j = 0; j < r.sharedMesh.blendShapeCount; j++)
                {
                    string name = r.sharedMesh.GetBlendShapeName(j);
                    float weight = r.GetBlendShapeWeight(j);

                    shapes[j] = new PumkinsBlendshape(name, weight);
                }
                blendshapeArrays.Add(shapes);
            }
        }
    }
}