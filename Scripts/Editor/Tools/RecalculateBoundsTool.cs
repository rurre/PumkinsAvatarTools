using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Pumkin.Presets;
using UnityEditor;
using UnityEngine;

namespace Pumkin.AvatarTools
{
    public class RecalculateBoundsTool : PumkinTool
    {
        public override bool ShowInQuickSetup => true;
        public override string NameInUI => "Recalculate Bounds";

        struct RendererContainer
        {
            public SkinnedMeshRenderer renderer;
            public bool oldUpdateWhenOffscreenValue;
            public Bounds newBounds;
        }

        RendererContainer[] containers;

        List<AnimationClip> PoseAnims;

        protected override bool Prepare(GameObject avatar)
        {
            if(PoseAnims == null)
            {
                PoseAnims = new List<AnimationClip>();
                AnimationClip clip;
                int clipIndex = 1;
                while((clip = Resources.Load<AnimationClip>($"RecalculatePoses/pose{clipIndex++}")) != null)
                    PoseAnims.Add(clip);
            }

            var anim = avatar.GetComponent<Animator>();
            if(!anim || !anim.isHuman)
            {
                PumkinsAvatarTools.Log("Can only recalculate bounds of humanoid avatars", LogType.Error);
                return false;
            }

            var renderers = avatar.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            containers = new RendererContainer[renderers.Length];
            for(int i = 0; i < containers.Length; i++)
            {
                containers[i] = new RendererContainer
                {
                    renderer = renderers[i],
                    newBounds = renderers[i].localBounds,
                    oldUpdateWhenOffscreenValue = renderers[i].updateWhenOffscreen
                };
            }
            return true;
        }

        protected override bool DoAction(GameObject avatar)
        {
            foreach(var cont in containers)
                cont.renderer.updateWhenOffscreen = true;

            foreach(var clip in PoseAnims)
            {
                clip.SampleAnimation(avatar, 0);
                for(var i = 0; i < containers.Length; i++)
                {
                    var cont = containers[i];
                    var b = cont.newBounds;
                    b.Encapsulate(cont.renderer.bounds);
                    containers[i].newBounds = b;
                }
            }
            return true;
        }

        protected override void Finish(GameObject avatar)
        {
            foreach(var cont in containers)
            {
                cont.renderer.updateWhenOffscreen = cont.oldUpdateWhenOffscreenValue;
                cont.renderer.localBounds = cont.newBounds;
            }

            PumkinsAvatarTools.ResetToAvatarDefinition(avatar, false, true, true, false);
            
            base.Finish(avatar);
        }
    }
}