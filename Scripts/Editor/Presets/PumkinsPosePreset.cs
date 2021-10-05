using Pumkin.AvatarTools;
using Pumkin.DataStructures;
using Pumkin.Dependencies;
using Pumkin.HelperFunctions;
using Pumkin.PoseEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Pumkin.Presets
{
    public class PumkinsPosePreset : PumkinPreset
    {
        public float[] muscles;

        public PosePresetMode presetMode = PosePresetMode.HumanPose;

        public List<string> transformPaths;
        public List<Vector3> transformRotations;

        public Vector3 bodyPosition = new Vector3(0, 1, 0);
        public Quaternion bodyRotation = Quaternion.identity;

        private PumkinsPosePreset() { }
        public enum PosePresetMode { HumanPose, TransformRotations }

        /// <summary>
        /// Creates and returns a PosePreset based on transform rotations only
        /// </summary>
        public static PumkinsPosePreset CreatePreset(string poseName, GameObject avatar, PosePresetMode mode)
        {
            PumkinsPosePreset pose = CreateInstance<PumkinsPosePreset>();
            pose.SetupPreset(poseName, avatar, mode);
            return pose;
        }

        /// <summary>
        /// Creates and returns a PosePreset based on humanoid muscle values
        /// </summary>
        public static PumkinsPosePreset CreatePreset(string poseName, Animator anim)
        {
            HumanPose hp = new HumanPose();
            HumanPoseHandler hph = new HumanPoseHandler(anim.avatar, anim.transform);
            hph.GetHumanPose(ref hp);

            return CreatePreset(poseName, hp);
        }

        /// <summary>
        /// Creates and returns a PosePreset based on humanoid muscle values
        /// </summary>
        public static PumkinsPosePreset CreatePreset(string poseName, HumanPose p)
        {
            PumkinsPosePreset pose = CreateInstance<PumkinsPosePreset>();
            pose.SetupPreset(poseName, p);
            return pose;
        }

        /// <summary>
        /// Creates and returns a PosePreset based on humanoid muscle values
        /// </summary>
        public static PumkinsPosePreset CreatePreset(string poseName, float[] muscles)
        {
            PumkinsPosePreset pose = CreateInstance<PumkinsPosePreset>();
            pose.SetupPreset(poseName, muscles);
            return pose;
        }

        public bool SetupPreset(string poseName, GameObject avatar, PosePresetMode mode)
        {
            if(!avatar)
                return false;

            if(mode == PosePresetMode.TransformRotations)
            {
                Transform[] trans = avatar.GetComponentsInChildren<Transform>();

                transformPaths = new List<string>(trans.Length);
                transformRotations = new List<Vector3>(trans.Length);

                for(int i = 0; i < trans.Length; i++)
                {
                    var t = trans[i];
                    string path = Helpers.GetTransformPath(t, avatar.transform, true);

                    if(t && !string.IsNullOrEmpty(path))
                    {
                        transformPaths.Add(path);
                        transformRotations.Add(t.localEulerAngles);
                    }
                }
                return true;
            }
            else
            {
                Animator anim = avatar.GetComponent<Animator>();

                if(!anim)
                    return false;

                SerialTransform tr = avatar.transform;
                avatar.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                HumanPose hp = new HumanPose();
                try
                {
                    HumanPoseHandler hph = new HumanPoseHandler(anim.avatar, anim.transform);
                    hph.GetHumanPose(ref hp);
                }
                catch
                {
                    avatar.transform.SetPositionAndRotation(tr.position, tr.rotation);
                    return false;
                }
                finally
                {
                    avatar.transform.SetPositionAndRotation(tr.position, tr.rotation);
                    SetupPreset(poseName, hp);
                }
                return true;
            }
        }

        public bool SetupPreset(string poseName, HumanPose p)
        {
            name = poseName;
            presetMode = PosePresetMode.HumanPose;
            muscles = p.muscles;
            bodyPosition = p.bodyPosition;
            bodyRotation = p.bodyRotation;

            if(muscles == null)
                muscles = new float[HumanTrait.MuscleCount];
            else
                Array.Resize(ref muscles, HumanTrait.MuscleCount);

            return true;
        }

        public bool SetupPreset(string poseName, float[] muscles)
        {
            if(muscles == null)
                muscles = new float[HumanTrait.MuscleCount];
            else
                Array.Resize(ref muscles, HumanTrait.MuscleCount);

            name = poseName;
            presetMode = PosePresetMode.HumanPose;
            this.muscles = muscles;
            return true;
        }

        public void SavePreset(bool overwriteExisting)
        {
            ScriptableObjectUtility.SaveAsset(this, name, PumkinsAvatarTools.MainFolderPath + "/Resources/Presets/Poses/", overwriteExisting);
            PumkinsPresetManager.LoadPresets<PumkinsPosePreset>();
        }

        public override bool ApplyPreset(GameObject avatar)
        {
            if(!avatar)
                return false;

            Undo.RegisterFullObjectHierarchyUndo(avatar, "Apply Pose");

            PumkinsAvatarTools.ResetPose(avatar);

            if(presetMode == PosePresetMode.HumanPose)
            {
                Animator anim = avatar.GetComponent<Animator>();
                if(anim && anim.avatar && anim.avatar.isHuman)
                {
                    Vector3 pos = avatar.transform.position;
                    Quaternion rot = avatar.transform.rotation;

                    avatar.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

                    var humanPoseHandler = new HumanPoseHandler(anim.avatar, avatar.transform);

                    var humanPose = new HumanPose();
                    humanPoseHandler.GetHumanPose(ref humanPose);

                    humanPose.muscles = muscles;

                    if(PumkinsAvatarTools.Settings.posePresetTryFixSinking)
                    {
                        if(humanPose.bodyPosition.y < 1 && !Mathf.Approximately(humanPose.bodyPosition.y, 0))
                        {
                            PumkinsAvatarTools.Log(Strings.PoseEditor.bodyPositionYTooSmall, LogType.Warning, humanPose.bodyPosition.y.ToString());
                            humanPose.bodyPosition.y = 1;
                        }
                    }

                    if(PumkinsAvatarTools.Settings.posePresetApplyBodyPosition)
                        humanPose.bodyPosition = bodyPosition;
                    if(PumkinsAvatarTools.Settings.posePresetApplyBodyRotation)
                        humanPose.bodyRotation = bodyRotation;

                    humanPoseHandler.SetHumanPose(ref humanPose);
                    avatar.transform.SetPositionAndRotation(pos, rot);

                    PumkinsPoseEditor.OnPoseWasChanged(PumkinsPoseEditor.PoseChangeType.Reset);
                    return true;
                }
                else
                {
                    PumkinsAvatarTools.Log(Strings.Log.cantSetPoseNonHumanoid, LogType.Error, name);
                    return false;
                }
            }
            else
            {
                if(!avatar)
                    return false;

                for(int i = 0; i < transformPaths.Count; i++)
                {
                    var t = avatar.transform.Find(transformPaths[i]);
                    if(t != null)
                        t.localEulerAngles = transformRotations[i];
                }
                return true;
            }
        }

        public HumanPose GetHumanPose(Animator anim)
        {
            if(!anim || !anim.isHuman)
                return default(HumanPose);

            HumanPose hp = new HumanPose
            {
                bodyPosition = anim.bodyPosition,
                bodyRotation = anim.bodyRotation,
                muscles = muscles
            };

            return hp;
        }
    }
}
