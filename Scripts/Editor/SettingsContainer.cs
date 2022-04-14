using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pumkin.DataStructures;
using Pumkin.Presets;
using UnityEditor;
using UnityEngine;

namespace Pumkin.AvatarTools
{
    [Serializable]
    internal class SettingsContainer : ScriptableObject
    {
        internal SerializedObject SerializedSettings
        {
            get
            {
                if(_serializedSettings == null)
                    _serializedSettings = new SerializedObject(this);
                return _serializedSettings;
            }
        }

        private SerializedObject _serializedSettings;

        [SerializeField] internal CopierTabs.Tab _copier_selectedTab = CopierTabs.Tab.Common;
        [SerializeField] internal bool _tools_quickSetup_settings_expand = false;
        [SerializeField] internal bool _tools_quickSetup_fillVisemes = true;
        [SerializeField] internal bool _tools_quickSetup_setViewpoint = true;
        [SerializeField] internal bool _tools_quickSetup_forceTPose = false;
        [SerializeField] internal float _tools_quickSetup_viewpointZDepth = 0.06f;
        [SerializeField] internal bool _tools_quickSetup_setSkinnedMeshRendererAnchor = true;
        [SerializeField] internal bool _tools_quickSetup_setMeshRendererAnchor = true;
        
        [SerializeField] internal bool _tools_quicksetup_setMeshRendererAnchor_usePath = false;
        [SerializeField] internal string _tools_quickSetup_setRenderAnchor_path = "Armature/Hips/Spine";
        [SerializeField] internal HumanBodyBones _tools_quickSetup_setRenderAnchor_bone = HumanBodyBones.Spine;

        [SerializeField] internal bool _tools_avatar_resetpose_expand = false;
        [SerializeField] internal bool _tools_avatar_resetPose_position = true;
        [SerializeField] internal bool _tools_avatar_resetPose_rotation = true;
        [SerializeField] internal bool _tools_avatar_resetPose_scale = false;
        [SerializeField] internal bool _tools_avatar_resetPose_fullreset = false;
        [SerializeField] internal ResetPoseType _tools_avatar_resetPose_type = ResetPoseType.Prefab;
        internal enum ResetPoseType
        {
            Prefab,
            AvatarDefinition,
            TPose
        }

        //Copier
        [SerializeField] internal bool bCopier_transforms_copy = true;
        [SerializeField] internal bool bCopier_transforms_copyPosition = false;
        [SerializeField] internal bool bCopier_transforms_copyRotation = true;
        [SerializeField] internal bool bCopier_transforms_copyScale = true;
        [SerializeField] internal bool bCopier_transforms_createMissing = true;
        [SerializeField] internal bool bCopier_transforms_copyActiveState = true;
        [SerializeField] internal bool bCopier_transforms_copyLayerAndTag = true;

        [SerializeField] internal bool bCopier_physBones_copy = true;
        [SerializeField] internal bool bCopier_physBones_createObjects = false;
        [SerializeField] internal bool bCopier_physBones_adjustScale = true;

        [SerializeField] internal bool bCopier_physBones_copyColliders = true;
        [SerializeField] internal bool bCopier_physBones_removeOldColliders = false;
        [SerializeField] internal bool bCopier_physBones_removeOldBones = false;
        [SerializeField] internal bool bCopier_physBones_createObjectsColliders = true;
        [SerializeField] internal bool bCopier_physBones_adjustScaleColliders = true;

        [SerializeField] internal bool bCopier_dynamicBones_copy = true;
        [SerializeField] internal bool bCopier_dynamicBones_copySettings = true;
        [SerializeField] internal bool bCopier_dynamicBones_createMissing = true;
        [SerializeField] internal bool bCopier_dynamicBones_createObjects = false;
        [SerializeField] internal bool bCopier_dynamicBones_adjustScale = true;

        [SerializeField] internal bool bCopier_dynamicBones_copyColliders = true;
        [SerializeField] internal bool bCopier_dynamicBones_removeOldColliders = false;
        [SerializeField] internal bool bCopier_dynamicBones_removeOldBones = false;
        [SerializeField] internal bool bCopier_dynamicBones_createObjectsColliders = true;
        [SerializeField] internal bool bCopier_dynamicBones_adjustScaleColliders = true;

        [SerializeField] internal bool bCopier_descriptor_copy = true;
        [SerializeField] internal bool bCopier_descriptor_copySettings = true;
        [SerializeField] internal bool bCopier_descriptor_copyPipelineId = false;
        [SerializeField] internal bool bCopier_descriptor_copyViewpoint = true;
        [SerializeField] internal bool bCopier_descriptor_copyAvatarScale = true;

        [SerializeField] internal bool bCopier_descriptor_copyAnimationOverrides = true;

        [SerializeField] internal bool bCopier_descriptor_copyEyeLookSettings = true;
        [SerializeField] internal bool bCopier_descriptor_copyPlayableLayers = true;
        [SerializeField] internal bool bCopier_descriptor_copyExpressions = true;
        [SerializeField] internal bool bCopier_descriptor_copyColliders = true;

        [SerializeField] internal bool bCopier_colliders_copy = true;
        [SerializeField] internal bool bCopier_colliders_removeOld = false;
        [SerializeField] internal bool bCopier_colliders_copyBox = true;

        [SerializeField] internal bool bCopier_colliders_copyCapsule = true;
        [SerializeField] internal bool bCopier_colliders_copySphere = true;
        [SerializeField] internal bool bCopier_colliders_copyMesh = false;
        [SerializeField] internal bool bCopier_colliders_createObjects = true;
        [SerializeField] internal bool bCopier_colliders_adjustScale = true;

        [SerializeField] internal bool bCopier_skinMeshRender_copy = true;
        [SerializeField] internal bool bCopier_skinMeshRender_copySettings = true;
        [SerializeField] internal bool bCopier_skinMeshRender_copyBlendShapeValues = true;
        [SerializeField] internal bool bCopier_skinMeshRender_copyMaterials = false;
        [SerializeField] internal bool bCopier_skinMeshRender_copyBounds = false;

        [SerializeField] internal bool bCopier_particleSystems_copy = true;
        [SerializeField] internal bool bCopier_particleSystems_replace = false;
        [SerializeField] internal bool bCopier_particleSystems_createObjects = true;

        [SerializeField] internal bool bCopier_rigidBodies_copy = true;
        [SerializeField] internal bool bCopier_rigidBodies_copySettings = true;
        [SerializeField] internal bool bCopier_rigidBodies_createMissing = true;
        [SerializeField] internal bool bCopier_rigidBodies_createObjects = true;

        [SerializeField] internal bool bCopier_trailRenderers_copy = true;
        [SerializeField] internal bool bCopier_trailRenderers_copySettings = true;
        [SerializeField] internal bool bCopier_trailRenderers_createMissing = true;
        [SerializeField] internal bool bCopier_trailRenderers_createObjects = true;

        [SerializeField] internal bool bCopier_meshRenderers_copy = true;
        [SerializeField] internal bool bCopier_meshRenderers_copySettings = true;
        [SerializeField] internal bool bCopier_meshRenderers_createMissing = true;
        [SerializeField] internal bool bCopier_meshRenderers_createObjects = true;

        [SerializeField] internal bool bCopier_lights_copy = true;
        [SerializeField] internal bool bCopier_lights_copySettings = true;
        [SerializeField] internal bool bCopier_lights_createMissing = true;
        [SerializeField] internal bool bCopier_lights_createObjects = true;

        [SerializeField] internal bool bCopier_animators_copy = true;
        [SerializeField] internal bool bCopier_animators_copySettings = true;
        [SerializeField] internal bool bCopier_animators_createMissing = true;
        [SerializeField] internal bool bCopier_animators_createObjects = false;
        [SerializeField] internal bool bCopier_animators_copyMainAnimator = false;

        [SerializeField] internal bool bCopier_joints_copy = true;
        [SerializeField] internal bool bCopier_joints_fixed = true;
        [SerializeField] internal bool bCopier_joints_character = true;
        [SerializeField] internal bool bCopier_joints_configurable = true;
        [SerializeField] internal bool bCopier_joints_spring = true;
        [SerializeField] internal bool bCopier_joints_hinge = true;
        [SerializeField] internal bool bCopier_joints_createObjects = true;
        [SerializeField] internal bool bCopier_joints_removeOld = true;

        [SerializeField] internal bool bCopier_audioSources_copy = true;
        [SerializeField] internal bool bCopier_audioSources_copySettings = true;
        [SerializeField] internal bool bCopier_audioSources_createMissing = true;
        [SerializeField] internal bool bCopier_audioSources_createObjects = true;

        [SerializeField] internal bool bCopier_other_copy = true;
        [SerializeField] internal bool bCopier_other_copyIKFollowers = true;
        [SerializeField] internal bool bCopier_other_copyVRMSpringBones = true;
        [SerializeField] internal bool bCopier_other_createGameObjects = true;

        [SerializeField] internal bool bCopier_contactReceiver_copy = true;
        [SerializeField] internal bool bCopier_contactReceiver_removeOld = false;
        [SerializeField] internal bool bCopier_contactReceiver_createObjects = false;
        [SerializeField] internal bool bCopier_contactReceiver_adjustScale = true;

        [SerializeField] internal bool bCopier_contactSender_copy = true;
        [SerializeField] internal bool bCopier_contactSender_removeOld = false;
        [SerializeField] internal bool bCopier_contactSender_createObjects = false;
        [SerializeField] internal bool bCopier_contactSender_adjustScale = true;

        [SerializeField] internal bool bCopier_aimConstraint_copy = true;
        [SerializeField] internal bool bCopier_aimConstraint_replaceOld = true;
        [SerializeField] internal bool bCopier_aimConstraint_createObjects = true;
        [SerializeField] internal bool bCopier_aimConstraint_onlyIfHasValidSources = true;

        [SerializeField] internal bool bCopier_lookAtConstraint_replaceOld = true;
        [SerializeField] internal bool bCopier_lookAtConstraint_createObjects = true;
        [SerializeField] internal bool bCopier_lookAtConstraint_copy = true;
        [SerializeField] internal bool bCopier_lookAtConstraint_onlyIfHasValidSources = true;

        [SerializeField] internal bool bCopier_parentConstraint_replaceOld = true;
        [SerializeField] internal bool bCopier_parentConstraint_createObjects = true;
        [SerializeField] internal bool bCopier_parentConstraint_copy = true;
        [SerializeField] internal bool bCopier_parentConstraint_onlyIfHasValidSources = true;

        [SerializeField] internal bool bCopier_positionConstraint_replaceOld = true;
        [SerializeField] internal bool bCopier_positionConstraint_createObjects = true;
        [SerializeField] internal bool bCopier_positionConstraint_copy = true;
        [SerializeField] internal bool bCopier_positionConstraint_onlyIfHasValidSources = true;

        [SerializeField] internal bool bCopier_rotationConstraint_replaceOld = true;
        [SerializeField] internal bool bCopier_rotationConstraint_createObjects = true;
        [SerializeField] internal bool bCopier_rotationConstraint_copy = true;
        [SerializeField] internal bool bCopier_rotationConstraint_onlyIfHasValidSources = true;

        [SerializeField] internal bool bCopier_scaleConstraint_replaceOld = true;
        [SerializeField] internal bool bCopier_scaleConstraint_createObjects = true;
        [SerializeField] internal bool bCopier_scaleConstraint_copy = true;
        [SerializeField] internal bool bCopier_scaleConstraint_onlyIfHasValidSources = true;
        
        [SerializeField] internal bool bCopier_cameras_copy = true;
        [SerializeField] internal bool bCopier_cameras_createObjects = true;
        
        [SerializeField] internal bool bCopier_finalIK_copy = true;
        [SerializeField] internal bool bCopier_finalIK_createObjects = true;
        [SerializeField] internal bool bCopier_finalIK_copyCCDIK = true;
        [SerializeField] internal bool bCopier_finalIK_copyLimbIK = true;
        [SerializeField] internal bool bCopier_finalIK_copyRotationLimits = true;
        [SerializeField] internal bool bCopier_finalIK_copyFabrik = true;
        [SerializeField] internal bool bCopier_finalIK_copyAimIK = true;
        [SerializeField] internal bool bCopier_finalIK_copyFBTBipedIK = true;
        [SerializeField] internal bool bCopier_finalIK_copyVRIK = true;
        
        //Ignore Array
        [SerializeField] internal bool _copierIgnoreArray_expand = false;
        [SerializeField] internal SerializedProperty _serializedIgnoreArrayProp;
        [SerializeField] internal Transform[] _copierIgnoreArray = new Transform[0];
        [SerializeField] internal bool bCopier_ignoreArray_includeChildren = false;
        [SerializeField] internal Vector2 _copierIgnoreArrayScroll = Vector2.zero;


        [SerializeField] internal static GameObject _copierSelectedFrom;


        [SerializeField] internal bool bThumbnails_use_camera_overlay = false;
        [SerializeField] internal bool bThumbnails_use_camera_background = false;
        [SerializeField] internal bool shouldHideOtherAvatars = true;
        [SerializeField] internal bool lockSelectedCameraToSceneView = false;

        //Thumbnails
        [SerializeField] internal bool _centerCameraOffsets_expand = false;
        [SerializeField] internal int _presetToolbarSelectedIndex = 0;
        [SerializeField] internal CameraClearFlags _thumbsCameraBgClearFlagsOld = CameraClearFlags.Skybox;

        [SerializeField] internal int _selectedCameraPresetIndex = 0;
        [SerializeField] internal string _selectedCameraPresetString = "";

        [SerializeField] internal string _selectedPosePresetString = "";
        [SerializeField] internal int _selectedPosePresetIndex = 0;

        [SerializeField] internal string _selectedBlendshapePresetString = "";
        [SerializeField] internal int _selectedBlendshapePresetIndex = 0;

        [SerializeField] internal bool posePresetTryFixSinking = true;
        [SerializeField] internal bool posePresetApplyBodyPosition = true;
        [SerializeField] internal bool posePresetApplyBodyRotation = true;


        [SerializeField] internal bool centerCameraFixClippingPlanes = true;
#if VRC_SDK_VRCSDK2 || (VRC_SDK_VRCSDK3 && !UDON)
        [SerializeField] internal PumkinsCameraPreset.CameraOffsetMode centerCameraMode = PumkinsCameraPreset.CameraOffsetMode.Viewpoint;
#else
        [SerializeField] internal PumkinsCameraPreset.CameraOffsetMode centerCameraMode = PumkinsCameraPreset.CameraOffsetMode.Transform;
#endif
        [SerializeField] internal string centerCameraTransformPath = "Armature/Hips/Spine/Chest/Neck/Head";
        internal Transform centerCameraTransform = null;

        [SerializeField] internal Vector3 centerCameraPositionOffsetViewpoint = new Vector3(-0.096f, 0.025f, 0.269f);
        [SerializeField] internal Vector3 centerCameraRotationOffsetViewpoint = new Vector3(4.193f, 164.274f, 0);

        [SerializeField] internal Vector3 centerCameraPositionOffsetTransform = new Vector3(-0.096f, 0.025f, 0.269f);
        [SerializeField] internal Vector3 centerCameraRotationOffsetTransform = new Vector3(4.193f, 164.274f, 0);

        [SerializeField] internal Vector3 centerCameraPositionOffsetAvatar = new Vector3(-0.096f, 0.025f, 0.269f);
        [SerializeField] internal Vector3 centerCameraRotationOffsetAvatar = new Vector3(4.193f, 164.274f, 0);

        [SerializeField] internal string _lastOpenFilePath = default(string);
        [SerializeField] internal string _backgroundPath = "";
        [SerializeField] internal string _overlayPath = "";
        [SerializeField] internal Color cameraBackgroundImageTint = Color.white;
        [SerializeField] internal Color cameraOverlayImageTint = Color.white;

        [SerializeField] internal Color _thumbsCamBgColor = Colors.DarkCameraBackground;


        [SerializeField] internal HumanPose _tempHumanPose = new HumanPose();
        [SerializeField] internal float[] _tempHumanPoseMuscles;
        [SerializeField] internal SerializedProperty _serializedTempHumanPoseMuscles;

        //UI
        [SerializeField] public bool _avatar_testing_expand = false;
        [SerializeField] internal bool _tools_expand = true;
        [SerializeField] internal bool _tools_avatar_expand = true;
        [SerializeField] internal bool _tools_physBones_expand = true;
        [SerializeField] internal bool _tools_dynamicBones_expand = true;
        [SerializeField] internal bool _tools_removeAll_expand = false;

        [SerializeField] internal bool _avatarInfo_expand = false;
        [SerializeField] internal bool _thumbnails_expand = false;

        [SerializeField] internal bool _info_expand = true;
        [SerializeField] internal bool _thumbnails_useCameraOverlay_expand = true;
        [SerializeField] internal bool _thumbnails_useCameraBackground_expand = true;

        [SerializeField] internal bool _experimental_expand = false;

        [SerializeField] internal bool _copier_expand = false;
        [SerializeField] internal bool _copier_expand_transforms = false;
        [SerializeField] internal bool _copier_expand_physBones = false;
        [SerializeField] internal bool _copier_expand_physBoneColliders = false;
        [SerializeField] internal bool _copier_expand_dynamicBones = false;
        [SerializeField] internal bool _copier_expand_dynamicBoneColliders = false;
        [SerializeField] internal bool _copier_expand_avatarDescriptor = false;
        [SerializeField] internal bool _copier_expand_skinnedMeshRenderer = false;
        [SerializeField] internal bool _copier_expand_colliders = false;
        [SerializeField] internal bool _copier_expand_particleSystems = false;
        [SerializeField] internal bool _copier_expand_rigidBodies = false;
        [SerializeField] internal bool _copier_expand_trailRenderers = false;
        [SerializeField] internal bool _copier_expand_meshRenderers = false;
        [SerializeField] internal bool _copier_expand_lights = false;
        [SerializeField] internal bool _copier_expand_animators = false;
        [SerializeField] internal bool _copier_expand_audioSources = false;
        [SerializeField] internal bool _copier_expand_other = false;
        [SerializeField] internal bool _copier_expand_contactReceiver = false;
        [SerializeField] internal bool _copier_expand_contactSender = false;
        [SerializeField] internal bool _copier_expand_aimConstraints = false;
        [SerializeField] internal bool _copier_expand_lookAtConstraints = false;
        [SerializeField] internal bool _copier_expand_parentConstraints = false;
        [SerializeField] internal bool _copier_expand_positionConstraints = false;
        [SerializeField] internal bool _copier_expand_rotationConstraints = false;
        [SerializeField] internal bool _copier_expand_scaleConstraints = false;
        [SerializeField] internal bool _copier_expand_joints = false;
        [SerializeField] internal bool _copier_expand_cameras = false;
        [SerializeField] internal bool _copier_expand_finalIK = false;

        //Languages
        [SerializeField] internal string _selectedLanguageString = "English - Default";
        [SerializeField] internal int _selectedLanguageIndex = 0;

        //Misc
        [SerializeField] internal bool _openedSettings = false;
        [SerializeField] internal Vector2 _mainToolsScrollbar = Vector2.zero;
        [SerializeField] internal bool verboseLoggingEnabled = false;
        [SerializeField] internal bool showExperimental = false;
        [SerializeField] internal bool handlesUiWindowPositionAtBottom = false;

        [SerializeField] internal float _avatarScaleTemp;
        [SerializeField] internal SerializedProperty _serializedAvatarScaleTempProp;
        [SerializeField] internal bool editingScaleMovesViewpoint = true;

        [SerializeField] internal static GameObject _selectedAvatar; // use property

        [SerializeField] internal static bool _useSceneSelectionAvatar = false;
    }
}
