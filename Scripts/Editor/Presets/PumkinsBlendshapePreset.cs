using Pumkin.AvatarTools;
using Pumkin.DataStructures;
using Pumkin.Dependencies;
using Pumkin.HelperFunctions;
using System.Collections.Generic;
using UnityEngine;

namespace Pumkin.Presets
{
    public class PumkinsBlendshapePreset : PumkinPreset
    {           
        public List<PumkinsRendererBlendshapesHolder> renderers;        

        public static PumkinsBlendshapePreset CreatePreset(string presetName, GameObject avatar)
        {
            PumkinsBlendshapePreset preset = CreateInstance<PumkinsBlendshapePreset>();
            preset.SetupPreset(presetName, avatar);
            return preset;
        }

        public override bool ApplyPreset(GameObject avatar)
        {
            if(!avatar || renderers == null)
            {
                PumkinsAvatarTools.Log(Strings.Warning.invalidPreset, LogType.Warning, name);
                return false;
            }

            for(int i = 0; i < renderers.Count; i++)
            {
                if(renderers[i] == null)
                    continue;

                var t = avatar.transform.Find(renderers[i].rendererPath);                
                if(t)
                {
                    var render = t.GetComponent<SkinnedMeshRenderer>();
                    if(render)
                    {
                        for(int j = 0; j < renderers[i].blendshapes.Count; j++)
                        {
                            int index = render.sharedMesh.GetBlendShapeIndex(renderers[i].blendshapes[j].name);
                            if(index == -1)
                            {
                                foreach(string name in renderers[i].blendshapes[j].otherNames)
                                {
                                    index = render.sharedMesh.GetBlendShapeIndex(name);
                                    if(index != -1)
                                        break;
                                }
                            }

                            if(index == -1)
                                continue;

                            float weight = renderers[i].blendshapes[j].weight;                            
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
            renderers = new List<PumkinsRendererBlendshapesHolder>(renders.Length);

            for(int i = 0; i < renders.Length; i++)
            {
                var r = renders[i];
                var rPath = Helpers.GetTransformPath(r.transform, avatar.transform);
                List<PumkinsBlendshape> shapeList = new List<PumkinsBlendshape>(r.sharedMesh.blendShapeCount);                
                
                for(int j = 0; j < r.sharedMesh.blendShapeCount; j++)
                {
                    string name = r.sharedMesh.GetBlendShapeName(j);
                    float weight = r.GetBlendShapeWeight(j);

                    shapeList.Add(new PumkinsBlendshape(name, weight));
                }
                renderers.Add(new PumkinsRendererBlendshapesHolder(rPath, shapeList));                
            }
        }
    }
}