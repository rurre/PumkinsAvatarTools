using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using System;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Pumkin.AvatarTools;

namespace Pumkin.PoseEditor
{
    public class PumkinsPoseEditor : EditorWindow
    {
        public static readonly string blendshapeExtension = "shapes";
        public static readonly string sceneExtension = "unity";
        public static readonly string poseExtension = "pose";

        static GameObject selectedAvatar;

        string _posePresetName, _scenePresetName, _blendshapePresetName;        

        bool overwriteExisting = true;

        bool useHumanPoses = false;

        bool _scene_expand = true;
        bool _blendshape_expand = true;
        bool _pose_expand = true;
        
        bool positionOverridesPose = true;
        bool positionOverridesShapes = true;
        bool overrideLights = true;

        bool poseOnlySaveChangedRotations = false;
        
        Vector2 _mainScroll = Vector2.zero;

        int selectedPoseIndex = 0;
        int selectedHumanPoseIndex = 0;
        int selectedShapeIndex = 0;
        int selectedSceneIndex = 0;
        int selectedPositionIndex = 0;
                
        static List<PosePreset> poses;
        static List<HumanPosePreset> humanPoses = new List<HumanPosePreset>();
        static List<PosePreset> defaultPoses = new List<PosePreset>()
        {
            new PosePreset("APose", new Dictionary<string, SerialTransform>
            {
                {"Armature", new SerialTransform(new Quaternion(-0.7071068f, 0f, 0f, 0.7071067f))},
                {"Armature/Hips", new SerialTransform(new Quaternion(0.7071068f, 0f, 0f, 0.7071067f))},                
                {"Armature/Hips/Left leg", new SerialTransform(new Quaternion(0.9986714f, 0.03588417f, 0.03587975f, 0.008972821f))},
                {"Armature/Hips/Left leg/Left knee", new SerialTransform(new Quaternion(0.004872414f, -0.04231054f, 0.04190284f, 0.9982135f))},
                {"Armature/Hips/Left leg/Left knee/Left ankle", new SerialTransform(new Quaternion(-0.4095502f, 0.008253491f, -0.003099248f, 0.912245f))},
                {"Armature/Hips/Left leg/Left knee/Left ankle/Left toe", new SerialTransform(new Quaternion(3.848644E-08f, 0.9361382f, -0.3516323f, 6.619196E-08f))},
                {"Armature/Hips/Left leg/Left knee/Left ankle/Left toe/Left toe_end", new SerialTransform(new Quaternion(0f, 0f, 0f, 1f))},
                {"Armature/Hips/Right leg", new SerialTransform(new Quaternion(0.9986714f, -0.03588417f, -0.03587975f, 0.008972821f))},
                {"Armature/Hips/Right leg/Right knee", new SerialTransform(new Quaternion(0.004872414f, 0.04231054f, -0.04190284f, 0.9982135f))},
                {"Armature/Hips/Right leg/Right knee/Right ankle", new SerialTransform(new Quaternion(-0.4095502f, -0.008253491f, 0.003099248f, 0.912245f))},
                {"Armature/Hips/Right leg/Right knee/Right ankle/Right toe", new SerialTransform(new Quaternion(-3.848644E-08f, 0.9361382f, -0.3516323f, -6.619196E-08f))},
                {"Armature/Hips/Right leg/Right knee/Right ankle/Right toe/Right toe_end", new SerialTransform(new Quaternion(0f, 0f, 0f, 1f))},
                {"Armature/Hips/Spine", new SerialTransform(new Quaternion(-0.05796075f, 1.614445E-13f, 1.614445E-13f, 0.9983189f))},
                {"Armature/Hips/Spine/Chest", new SerialTransform(new Quaternion(-0.001378456f, 1.969634E-09f, 2.212438E-09f, 0.999999f))},
                {"Armature/Hips/Spine/Chest/Breast", new SerialTransform(new Quaternion(-5.586176E-08f, 0.6639034f, 0.7478184f, -5.13642E-08f))},
                {"Armature/Hips/Spine/Chest/Breast/Breast_end", new SerialTransform(new Quaternion(0f, 0f, 0f, 1f))},
                {"Armature/Hips/Spine/Chest/Left shoulder", new SerialTransform(new Quaternion(0.5621743f, -0.4578156f, -0.5156817f, -0.4565495f))},
                {"Armature/Hips/Spine/Chest/Left shoulder/Left arm", new SerialTransform(new Quaternion(-0.2053409f, 0.2135349f, -0.09379838f, 0.9504945f))},
                {"Armature/Hips/Spine/Chest/Left shoulder/Left arm/Left elbow", new SerialTransform(new Quaternion(-0.01347903f, -0.002298978f, 0.02280845f, 0.9996464f))},
                {"Armature/Hips/Spine/Chest/Left shoulder/Left arm/Left elbow/Left wrist", new SerialTransform(new Quaternion(0.01777397f, 0.0001094748f, -0.02512324f, 0.9995263f))},
                {"Armature/Hips/Spine/Chest/Left shoulder/Left arm/Left elbow/Left wrist/IndexFinger1_L", new SerialTransform(new Quaternion(-0.0356151f, 0.005686217f, 0.03969931f, 0.9985605f))},
                {"Armature/Hips/Spine/Chest/Left shoulder/Left arm/Left elbow/Left wrist/IndexFinger1_L/IndexFinger2_L", new SerialTransform(new Quaternion(-0.02050746f, 0.03265196f, -0.02400833f, 0.9989679f))},
                {"Armature/Hips/Spine/Chest/Left shoulder/Left arm/Left elbow/Left wrist/IndexFinger1_L/IndexFinger2_L/IndexFinger3_L", new SerialTransform(new Quaternion(0.01886311f, -0.01968856f, 0.006647404f, 0.9996061f))},
                {"Armature/Hips/Spine/Chest/Left shoulder/Left arm/Left elbow/Left wrist/IndexFinger1_L/IndexFinger2_L/IndexFinger3_L/IndexFinger3_L_end", new SerialTransform(new Quaternion(0f, 0f, 0f, 1f))},
                {"Armature/Hips/Spine/Chest/Left shoulder/Left arm/Left elbow/Left wrist/LittleFinger1_L", new SerialTransform(new Quaternion(-0.06815263f, 0.05165451f, 0.002492888f, 0.9963337f))},
                {"Armature/Hips/Spine/Chest/Left shoulder/Left arm/Left elbow/Left wrist/LittleFinger1_L/LittleFinger2_L", new SerialTransform(new Quaternion(-0.005968827f, -0.005104264f, 0.01414687f, 0.9998691f))},
                {"Armature/Hips/Spine/Chest/Left shoulder/Left arm/Left elbow/Left wrist/LittleFinger1_L/LittleFinger2_L/LittleFinger3_L", new SerialTransform(new Quaternion(0.006916159f, -0.006350369f, 0.001030987f, 0.9999554f))},
                {"Armature/Hips/Spine/Chest/Left shoulder/Left arm/Left elbow/Left wrist/LittleFinger1_L/LittleFinger2_L/LittleFinger3_L/LittleFinger3_L_end", new SerialTransform(new Quaternion(0f, 0f, 0f, 1f))},
                {"Armature/Hips/Spine/Chest/Left shoulder/Left arm/Left elbow/Left wrist/MiddleFinger1_L", new SerialTransform(new Quaternion(-0.0153637f, -0.008300184f, 0.03650313f, 0.999181f))},
                {"Armature/Hips/Spine/Chest/Left shoulder/Left arm/Left elbow/Left wrist/MiddleFinger1_L/MiddleFinger2_L", new SerialTransform(new Quaternion(-0.03824675f, 0.04590584f, -0.02282737f, 0.9979523f))},
                {"Armature/Hips/Spine/Chest/Left shoulder/Left arm/Left elbow/Left wrist/MiddleFinger1_L/MiddleFinger2_L/MiddleFinger3_L", new SerialTransform(new Quaternion(0.04821414f, -0.05051146f, 0.01771069f, 0.9974018f))},
                {"Armature/Hips/Spine/Chest/Left shoulder/Left arm/Left elbow/Left wrist/MiddleFinger1_L/MiddleFinger2_L/MiddleFinger3_L/MiddleFinger3_L_end", new SerialTransform(new Quaternion(0f, 0f, 0f, 1f))},
                {"Armature/Hips/Spine/Chest/Left shoulder/Left arm/Left elbow/Left wrist/RingFinger1_L", new SerialTransform(new Quaternion(-0.08235166f, 0.06716307f, -0.005541536f, 0.9943222f))},
                {"Armature/Hips/Spine/Chest/Left shoulder/Left arm/Left elbow/Left wrist/RingFinger1_L/RingFinger2_L", new SerialTransform(new Quaternion(0.01983806f, -0.02340992f, 0.01137352f, 0.9994644f))},
                {"Armature/Hips/Spine/Chest/Left shoulder/Left arm/Left elbow/Left wrist/RingFinger1_L/RingFinger2_L/RingFinger3_L", new SerialTransform(new Quaternion(0.03805108f, -0.04579372f, 0.02312871f, 0.997958f))},
                {"Armature/Hips/Spine/Chest/Left shoulder/Left arm/Left elbow/Left wrist/RingFinger1_L/RingFinger2_L/RingFinger3_L/RingFinger3_L_end", new SerialTransform(new Quaternion(0f, 0f, 0f, 1f))},
                {"Armature/Hips/Spine/Chest/Left shoulder/Left arm/Left elbow/Left wrist/Thumb0_L", new SerialTransform(new Quaternion(-0.2454762f, 0.1082771f, 0.1491333f, 0.951723f))},
                {"Armature/Hips/Spine/Chest/Left shoulder/Left arm/Left elbow/Left wrist/Thumb0_L/Thumb1_L", new SerialTransform(new Quaternion(-0.02024357f, 0.03498593f, -0.008945947f, 0.9991428f))},
                {"Armature/Hips/Spine/Chest/Left shoulder/Left arm/Left elbow/Left wrist/Thumb0_L/Thumb1_L/Thumb2_L", new SerialTransform(new Quaternion(0.02221633f, -0.04790917f, 0.01709747f, 0.9984582f))},
                {"Armature/Hips/Spine/Chest/Left shoulder/Left arm/Left elbow/Left wrist/Thumb0_L/Thumb1_L/Thumb2_L/Thumb2_L_end", new SerialTransform(new Quaternion(0f, 0f, 0f, 1f))},
                {"Armature/Hips/Spine/Chest/Neck", new SerialTransform(new Quaternion(0.09886816f, -1.276232E-08f, -1.393427E-08f, 0.9951006f))},
                {"Armature/Hips/Spine/Chest/Neck/Head", new SerialTransform(new Quaternion(-0.03964783f, 1.127509E-08f, 1.127509E-08f, 0.9992138f))},                                
                {"Armature/Hips/Spine/Chest/Right shoulder", new SerialTransform(new Quaternion(0.5621742f, 0.4578156f, 0.5156817f, -0.4565495f))},
                {"Armature/Hips/Spine/Chest/Right shoulder/Right arm", new SerialTransform(new Quaternion(-0.2053409f, -0.2135349f, 0.0937984f, 0.9504945f))},
                {"Armature/Hips/Spine/Chest/Right shoulder/Right arm/Right elbow", new SerialTransform(new Quaternion(-0.01347904f, 0.002298978f, -0.02280846f, 0.9996464f))},
                {"Armature/Hips/Spine/Chest/Right shoulder/Right arm/Right elbow/Right wrist", new SerialTransform(new Quaternion(0.01777413f, -0.000109199f, 0.02512311f, 0.9995264f))},
                {"Armature/Hips/Spine/Chest/Right shoulder/Right arm/Right elbow/Right wrist/IndexFinger1_R", new SerialTransform(new Quaternion(-0.03561525f, -0.005686515f, -0.03969924f, 0.9985605f))},
                {"Armature/Hips/Spine/Chest/Right shoulder/Right arm/Right elbow/Right wrist/IndexFinger1_R/IndexFinger2_R", new SerialTransform(new Quaternion(-0.02050746f, -0.03265193f, 0.02400845f, 0.9989679f))},
                {"Armature/Hips/Spine/Chest/Right shoulder/Right arm/Right elbow/Right wrist/IndexFinger1_R/IndexFinger2_R/IndexFinger3_R", new SerialTransform(new Quaternion(0.01886313f, 0.01968858f, -0.006647416f, 0.9996061f))},
                {"Armature/Hips/Spine/Chest/Right shoulder/Right arm/Right elbow/Right wrist/IndexFinger1_R/IndexFinger2_R/IndexFinger3_R/IndexFinger3_R_end", new SerialTransform(new Quaternion(0f, 0f, 0f, 1f))},
                {"Armature/Hips/Spine/Chest/Right shoulder/Right arm/Right elbow/Right wrist/LittleFinger1_R", new SerialTransform(new Quaternion(-0.06815278f, -0.05165481f, -0.002492778f, 0.9963337f))},
                {"Armature/Hips/Spine/Chest/Right shoulder/Right arm/Right elbow/Right wrist/LittleFinger1_R/LittleFinger2_R", new SerialTransform(new Quaternion(-0.005968846f, 0.005104282f, -0.01414684f, 0.9998691f))},
                {"Armature/Hips/Spine/Chest/Right shoulder/Right arm/Right elbow/Right wrist/LittleFinger1_R/LittleFinger2_R/LittleFinger3_R", new SerialTransform(new Quaternion(0.006916152f, 0.006350345f, -0.00103096f, 0.9999554f))},
                {"Armature/Hips/Spine/Chest/Right shoulder/Right arm/Right elbow/Right wrist/LittleFinger1_R/LittleFinger2_R/LittleFinger3_R/LittleFinger3_R_end", new SerialTransform(new Quaternion(0f, 0f, 0f, 1f))},
                {"Armature/Hips/Spine/Chest/Right shoulder/Right arm/Right elbow/Right wrist/MiddleFinger1_R", new SerialTransform(new Quaternion(-0.01536454f, 0.008299073f, -0.03650245f, 0.999181f))},
                {"Armature/Hips/Spine/Chest/Right shoulder/Right arm/Right elbow/Right wrist/MiddleFinger1_R/MiddleFinger2_R", new SerialTransform(new Quaternion(-0.03824554f, -0.04590411f, 0.02282618f, 0.9979525f))},
                {"Armature/Hips/Spine/Chest/Right shoulder/Right arm/Right elbow/Right wrist/MiddleFinger1_R/MiddleFinger2_R/MiddleFinger3_R", new SerialTransform(new Quaternion(0.04821276f, 0.05050958f, -0.01770934f, 0.997402f))},
                {"Armature/Hips/Spine/Chest/Right shoulder/Right arm/Right elbow/Right wrist/MiddleFinger1_R/MiddleFinger2_R/MiddleFinger3_R/MiddleFinger3_R_end", new SerialTransform(new Quaternion(0f, 0f, 0f, 1f))},
                {"Armature/Hips/Spine/Chest/Right shoulder/Right arm/Right elbow/Right wrist/RingFinger1_R", new SerialTransform(new Quaternion(-0.0823547f, -0.06716713f, 0.005544453f, 0.9943216f))},
                {"Armature/Hips/Spine/Chest/Right shoulder/Right arm/Right elbow/Right wrist/RingFinger1_R/RingFinger2_R", new SerialTransform(new Quaternion(0.01984035f, 0.02341333f, -0.01137586f, 0.9994643f))},
                {"Armature/Hips/Spine/Chest/Right shoulder/Right arm/Right elbow/Right wrist/RingFinger1_R/RingFinger2_R/RingFinger3_R", new SerialTransform(new Quaternion(0.03805373f, 0.04579718f, -0.02313079f, 0.9979577f))},
                {"Armature/Hips/Spine/Chest/Right shoulder/Right arm/Right elbow/Right wrist/RingFinger1_R/RingFinger2_R/RingFinger3_R/RingFinger3_R_end", new SerialTransform(new Quaternion(0f, 0f, 0f, 1f))},
                {"Armature/Hips/Spine/Chest/Right shoulder/Right arm/Right elbow/Right wrist/Thumb0_R", new SerialTransform(new Quaternion(-0.2454735f, -0.1082745f, -0.1491345f, 0.9517239f))},
                {"Armature/Hips/Spine/Chest/Right shoulder/Right arm/Right elbow/Right wrist/Thumb0_R/Thumb1_R", new SerialTransform(new Quaternion(-0.02024736f, -0.03499207f, 0.008947278f, 0.9991424f))},
                {"Armature/Hips/Spine/Chest/Right shoulder/Right arm/Right elbow/Right wrist/Thumb0_R/Thumb1_R/Thumb2_R", new SerialTransform(new Quaternion(0.02221983f, 0.04791843f, -0.01710149f, 0.9984577f))},
                {"Armature/Hips/Spine/Chest/Right shoulder/Right arm/Right elbow/Right wrist/Thumb0_R/Thumb1_R/Thumb2_R/Thumb2_R_end", new SerialTransform(new Quaternion(0f, 0f, 0f, 1f))},        
            }),
            new PosePreset("TPose", new Dictionary<string, SerialTransform>
            {
                {"Armature", new SerialTransform(new Quaternion(-0.7071068f, 0f, 0f, 0.7071067f))},
                {"Armature/Hips", new SerialTransform(new Quaternion(0.7071068f, 0f, 0f, 0.7071067f))},
                {"Armature/Hips/Left leg", new SerialTransform(new Quaternion(1f, 6.837939E-08f, 7.54979E-08f, 1.947072E-07f))},
                {"Armature/Hips/Left leg/Left knee", new SerialTransform(new Quaternion(2.553765E-07f, -1.967755E-08f, 1.255907E-08f, 1f))},
                {"Armature/Hips/Left leg/Left knee/Left ankle", new SerialTransform(new Quaternion(-0.4950582f, 0.01679231f, -0.01679236f, 0.8685353f))},
                {"Armature/Hips/Left leg/Left knee/Left ankle/Left toe", new SerialTransform(new Quaternion(-6.171095E-07f, 0.9642062f, -0.2640882f, 0.02374722f))},
                {"Armature/Hips/Right leg", new SerialTransform(new Quaternion(1f, 1.367588E-07f, 7.54979E-08f, 1.947072E-07f))},
                {"Armature/Hips/Right leg/Right knee", new SerialTransform(new Quaternion(2.553765E-07f, -4.200568E-08f, 1.032666E-07f, 1f))},
                {"Armature/Hips/Right leg/Right knee/Right ankle", new SerialTransform(new Quaternion(-0.4950581f, -0.01679211f, 0.01679208f, 0.8685353f))},
                {"Armature/Hips/Right leg/Right knee/Right ankle/Right toe", new SerialTransform(new Quaternion(6.374848E-07f, 0.9642062f, -0.2640882f, -0.02374698f))},
                {"Armature/Hips/Spine", new SerialTransform(new Quaternion(0f, 0f, 0f, 1f))},
                {"Armature/Hips/Spine/Chest", new SerialTransform(new Quaternion(-0.01303699f, -2.334407E-08f, -2.334407E-08f, 0.999915f))},
                {"Armature/Hips/Spine/Chest/Left shoulder", new SerialTransform(new Quaternion(-0.5060819f, 0.4729586f, 0.4854548f, 0.5334089f))},
                {"Armature/Hips/Spine/Chest/Left shoulder/Left arm", new SerialTransform(new Quaternion(-0.04704045f, 0.2640885f, 0.03154722f, 0.9628339f))},
                {"Armature/Hips/Spine/Chest/Left shoulder/Left arm/Left elbow", new SerialTransform(new Quaternion(0.004182443f, 0.003751449f, -0.01474725f, 0.9998755f))},
                {"Armature/Hips/Spine/Chest/Left shoulder/Left arm/Left elbow/Left wrist", new SerialTransform(new Quaternion(-0.0177228f, -0.004817383f, 0.04033175f, 0.9990175f))},
                {"Armature/Hips/Spine/Chest/Left shoulder/Left arm/Left elbow/Left wrist/IndexFinger1_L", new SerialTransform(new Quaternion(0.7341435f, -0.4539575f, -0.4539577f, 0.2210843f))},
                {"Armature/Hips/Spine/Chest/Left shoulder/Left arm/Left elbow/Left wrist/IndexFinger1_L/IndexFinger2_L", new SerialTransform(new Quaternion(1.490116E-08f, 3.725291E-08f, 1.110222E-15f, 1f))},
                {"Armature/Hips/Spine/Chest/Left shoulder/Left arm/Left elbow/Left wrist/IndexFinger1_L/IndexFinger2_L/IndexFinger3_L", new SerialTransform(new Quaternion(4.301344E-23f, 8.881784E-15f, -4.440905E-16f, 1f))},
                {"Armature/Hips/Spine/Chest/Left shoulder/Left arm/Left elbow/Left wrist/LittleFinger1_L", new SerialTransform(new Quaternion(0.7341434f, -0.4539578f, -0.4539577f, 0.2210844f))},
                {"Armature/Hips/Spine/Chest/Left shoulder/Left arm/Left elbow/Left wrist/LittleFinger1_L/LittleFinger2_L", new SerialTransform(new Quaternion(-6.328271E-15f, 4.470348E-08f, -3.49246E-08f, 1f))},
                {"Armature/Hips/Spine/Chest/Left shoulder/Left arm/Left elbow/Left wrist/LittleFinger1_L/LittleFinger2_L/LittleFinger3_L", new SerialTransform(new Quaternion(-1.985233E-23f, 7.105427E-15f, 5.820766E-09f, 1f))},
                {"Armature/Hips/Spine/Chest/Left shoulder/Left arm/Left elbow/Left wrist/MiddleFinger1_L", new SerialTransform(new Quaternion(0.7341434f, -0.4539577f, -0.4539576f, 0.2210843f))},
                {"Armature/Hips/Spine/Chest/Left shoulder/Left arm/Left elbow/Left wrist/MiddleFinger1_L/MiddleFinger2_L", new SerialTransform(new Quaternion(-8.881784E-16f, -7.450581E-08f, -1.164154E-08f, 1f))},
                {"Armature/Hips/Spine/Chest/Left shoulder/Left arm/Left elbow/Left wrist/MiddleFinger1_L/MiddleFinger2_L/MiddleFinger3_L", new SerialTransform(new Quaternion(0f, -4.846701E-25f, 0f, 1f))},
                {"Armature/Hips/Spine/Chest/Left shoulder/Left arm/Left elbow/Left wrist/RingFinger1_L", new SerialTransform(new Quaternion(0.7341434f, -0.4539576f, -0.4539576f, 0.2210845f))},
                {"Armature/Hips/Spine/Chest/Left shoulder/Left arm/Left elbow/Left wrist/RingFinger1_L/RingFinger2_L", new SerialTransform(new Quaternion(1.385808E-06f, -2.004206E-06f, -0.06961256f, 0.9975742f))},
                {"Armature/Hips/Spine/Chest/Left shoulder/Left arm/Left elbow/Left wrist/RingFinger1_L/RingFinger2_L/RingFinger3_L", new SerialTransform(new Quaternion(7.610062E-23f, -2.220447E-16f, 5.820766E-09f, 1f))},
                {"Armature/Hips/Spine/Chest/Left shoulder/Left arm/Left elbow/Left wrist/Thumb0_L", new SerialTransform(new Quaternion(0.7629954f, -0.352358f, -0.4343732f, 0.3240399f))},
                {"Armature/Hips/Spine/Chest/Left shoulder/Left arm/Left elbow/Left wrist/Thumb0_L/Thumb1_L", new SerialTransform(new Quaternion(3.407985E-22f, 4.470348E-08f, -2.328305E-08f, 1f))},
                {"Armature/Hips/Spine/Chest/Left shoulder/Left arm/Left elbow/Left wrist/Thumb0_L/Thumb1_L/Thumb2_L", new SerialTransform(new Quaternion(-4.632212E-23f, 7.105427E-15f, -5.82077E-09f, 1f))},
                {"Armature/Hips/Spine/Chest/Right shoulder", new SerialTransform(new Quaternion(-0.5060809f, -0.4729587f, -0.4854544f, 0.53341f))},
                {"Armature/Hips/Spine/Chest/Right shoulder/Right arm", new SerialTransform(new Quaternion(-0.0470415f, -0.2640885f, -0.03154606f, 0.9628339f))},
                {"Armature/Hips/Spine/Chest/Right shoulder/Right arm/Right elbow", new SerialTransform(new Quaternion(0.004181627f, -0.003752453f, 0.01474767f, 0.9998755f))},
                {"Armature/Hips/Spine/Chest/Right shoulder/Right arm/Right elbow/Right wrist", new SerialTransform(new Quaternion(-0.01772276f, 0.004817458f, -0.04033176f, 0.9990175f))},
                {"Armature/Hips/Spine/Chest/Right shoulder/Right arm/Right elbow/Right wrist/IndexFinger1_R", new SerialTransform(new Quaternion(0.7341437f, 0.4539576f, 0.4539576f, 0.221084f))},
                {"Armature/Hips/Spine/Chest/Right shoulder/Right arm/Right elbow/Right wrist/IndexFinger1_R/IndexFinger2_R", new SerialTransform(new Quaternion(1.490116E-08f, -1.043081E-07f, -3.219647E-15f, 1f))},
                {"Armature/Hips/Spine/Chest/Right shoulder/Right arm/Right elbow/Right wrist/IndexFinger1_R/IndexFinger2_R/IndexFinger3_R", new SerialTransform(new Quaternion(0f, 1.449532E-29f, -4.76456E-22f, 1f))},
                {"Armature/Hips/Spine/Chest/Right shoulder/Right arm/Right elbow/Right wrist/LittleFinger1_R", new SerialTransform(new Quaternion(0.7341437f, 0.4539576f, 0.4539576f, 0.221084f))},
                {"Armature/Hips/Spine/Chest/Right shoulder/Right arm/Right elbow/Right wrist/LittleFinger1_R/LittleFinger2_R", new SerialTransform(new Quaternion(1.490115E-08f, -1.043081E-07f, -3.219647E-15f, 1f))},
                {"Armature/Hips/Spine/Chest/Right shoulder/Right arm/Right elbow/Right wrist/LittleFinger1_R/LittleFinger2_R/LittleFinger3_R", new SerialTransform(new Quaternion(6.948312E-23f, -1.111731E-21f, -2.38228E-22f, 1f))},
                {"Armature/Hips/Spine/Chest/Right shoulder/Right arm/Right elbow/Right wrist/MiddleFinger1_R", new SerialTransform(new Quaternion(0.7341438f, 0.4539575f, 0.4539576f, 0.2210839f))},
                {"Armature/Hips/Spine/Chest/Right shoulder/Right arm/Right elbow/Right wrist/MiddleFinger1_R/MiddleFinger2_R", new SerialTransform(new Quaternion(-1.490116E-08f, -1.117587E-07f, 1.164153E-08f, 1f))},
                {"Armature/Hips/Spine/Chest/Right shoulder/Right arm/Right elbow/Right wrist/MiddleFinger1_R/MiddleFinger2_R/MiddleFinger3_R", new SerialTransform(new Quaternion(3.308722E-24f, -1.196375E-22f, -2.220449E-16f, 1f))},
                {"Armature/Hips/Spine/Chest/Right shoulder/Right arm/Right elbow/Right wrist/RingFinger1_R", new SerialTransform(new Quaternion(0.7341437f, 0.4539576f, 0.4539576f, 0.2210839f))},
                {"Armature/Hips/Spine/Chest/Right shoulder/Right arm/Right elbow/Right wrist/RingFinger1_R/RingFinger2_R", new SerialTransform(new Quaternion(-9.926129E-24f, -3.72529E-08f, -1.164154E-08f, 1f))},
                {"Armature/Hips/Spine/Chest/Right shoulder/Right arm/Right elbow/Right wrist/RingFinger1_R/RingFinger2_R/RingFinger3_R", new SerialTransform(new Quaternion(1.323489E-23f, -7.105427E-15f, -5.820768E-09f, 1f))},
                {"Armature/Hips/Spine/Chest/Right shoulder/Right arm/Right elbow/Right wrist/Thumb0_R", new SerialTransform(new Quaternion(0.7629959f, 0.352356f, 0.4343733f, 0.3240407f))},
                {"Armature/Hips/Spine/Chest/Right shoulder/Right arm/Right elbow/Right wrist/Thumb0_R/Thumb1_R", new SerialTransform(new Quaternion(-1.164153E-08f, -8.195639E-08f, 2.328307E-08f, 1f))},
                {"Armature/Hips/Spine/Chest/Right shoulder/Right arm/Right elbow/Right wrist/Thumb0_R/Thumb1_R/Thumb2_R", new SerialTransform(new Quaternion(-5.820768E-09f, -4.20128E-17f, 5.820767E-09f, 1f))},
                {"Armature/Hips/Spine/Chest/Neck", new SerialTransform(new Quaternion(0.01834228f, 2.346763E-08f, 2.32181E-08f, 0.9998318f))},
                {"Armature/Hips/Spine/Chest/Neck/Head", new SerialTransform(new Quaternion(-0.005305921f, -6.2143E-15f, 1.364294E-12f, 0.9999859f))},
                {"Body", new SerialTransform(new Quaternion(-0.7071068f, 0f, 0f, 0.7071067f))},
        }),
        };
        static List<BlendshapePreset> shapes;
        static List<string> scenePaths;

        static List<PumkinsPoseEditorPosition> scenePositions;
        static List<PumkinsPoseEditorPosition> addedScenePositions;

        static Dictionary<Light, bool> addedSceneLightStates;
        static Dictionary<Light, bool> sceneLightStates;
                
        Scene addedScene;        

        public static string PosePresetPath
        {
            get { return PumkinsAvatarTools.MainScriptPath + "/Resources/Presets/Poses/"; }
        }

        public static string BlendshapesPresetPath
        {
            get { return PumkinsAvatarTools.MainScriptPath + "/Resources/Presets/Blendshapes/"; }
        }

        public static string ScenePresetPath
        {
            get { return PumkinsAvatarTools.MainScriptPath + "/Resources/Presets/Scenes/"; }
        }

        public Scene ThisScene
        {
            get { return EditorSceneManager.GetSceneAt(0); }
        }

        public bool InPlaymode
        {
            get { return EditorApplication.isPlaying; }
        }


        /// <summary>
        /// Check folders for scenes, serialized poses and blendshape files
        /// </summary>
        public static void CheckFolders(bool forceRefresh = false)
        {
            var pFiles = Directory.GetFiles(PosePresetPath, "*." + poseExtension, SearchOption.AllDirectories);
            var sFiles = Directory.GetFiles(BlendshapesPresetPath, "*." + blendshapeExtension, SearchOption.AllDirectories);
            var scFiles = Directory.GetFiles(ScenePresetPath, "*." + sceneExtension, SearchOption.AllDirectories);

            
            if(poses == null)
                poses = new List<PosePreset>();
            if(shapes == null)
                shapes = new List<BlendshapePreset>();
            if(scenePaths == null)
                scenePaths = new List<string>();

            if(forceRefresh || (poses.Count != pFiles.Length))
            {
                poses.Clear();

                foreach(var s in pFiles)
                {
                    try
                    {
                        string json = File.ReadAllText(s);
                        if(!string.IsNullOrEmpty(json))
                        {
                            var pose = JsonConvert.DeserializeObject<PosePreset>(json);

                            if(pose != null)
                            {
                                poses.Add(pose);
                                Debug.LogFormat(Strings.Log.LoadedPose, s);
                            }
                        }
                    }
                    catch(Exception e)
                    {
                        Debug.LogError(e.Message);
                        continue;
                    }                    
                }
            }
            if(forceRefresh || (shapes.Count != sFiles.Length))
            {
                shapes.Clear();

                foreach(var s in sFiles)
                {
                    try
                    {
                        string json = File.ReadAllText(s);
                        if(!string.IsNullOrEmpty(json))
                        {
                            var shape = JsonConvert.DeserializeObject<BlendshapePreset>(json);

                            if(shape != null)
                            {
                                shapes.Add(shape);
                                Debug.LogFormat(Strings.Log.LoadedBlendshapePreset, s);
                            }
                        }
                    }
                    catch(Exception e)
                    {
                        Debug.LogError(e.Message);
                        continue;
                    }
                }
            }
            if(forceRefresh || (scenePaths.Count != scFiles.Length))
            {
                scenePaths = scFiles.ToList();                
            }

            var sc = EditorBuildSettings.scenes.Select(x => x.path.Replace('\\', '/')).ToList();
            var scb = new List<EditorBuildSettingsScene>();

            foreach(var s in scFiles)
            {
                if(!sc.Contains(s.Replace('\\', '/')))
                    scb.Add(new EditorBuildSettingsScene(s, true));
            }
            if(scb.Count > 0)
                EditorBuildSettings.scenes = scb.ToArray();            
        }

        //[MenuItem("Tools/Pumkin/Pose Editor")]
        public static void ShowWindow()
        {
            //Show existing window instance. If one doesn't exist, make one.
            EditorWindow editorWindow = EditorWindow.GetWindow(typeof(PumkinsPoseEditor));
            editorWindow.autoRepaintOnSceneChange = true;

            editorWindow.Show();
            editorWindow.titleContent = new GUIContent("Pose Editor");            
        }
         
        private void OnEnable()
        {
            selectedAvatar = PumkinsAvatarTools.selectedAvatar;           

            CheckFolders();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField(Strings.PoseEditor.Title, Styles.Label_mainTitle, GUILayout.MinHeight(Styles.Label_mainTitle.fontSize + 6));
            EditorGUILayout.Space();

            selectedAvatar = (GameObject)EditorGUILayout.ObjectField(Strings.Main.Avatar, selectedAvatar, typeof(GameObject), true);

            if(GUILayout.Button(Strings.Buttons.SelectFromScene))
            {
                if(Selection.activeObject != null)
                {
                    try { selectedAvatar = Selection.activeGameObject.transform.root.gameObject; } catch {}
                }
            }

            if(selectedAvatar != null && selectedAvatar.gameObject.scene.name == null)
            {
                PumkinsAvatarTools.Log(Strings.Warning.SelectSceneObject, LogType.Warning);
                selectedAvatar = null;
            }

            EditorGUILayout.Space();

            _mainScroll = EditorGUILayout.BeginScrollView(_mainScroll);

            //===========================================================================
            //Scene======================================================================
            //===========================================================================

            if(_scene_expand = GUILayout.Toggle(_scene_expand, Strings.PoseEditor.Scene, Styles.Foldout_title))
            {
                EditorGUI.BeginDisabledGroup(!selectedAvatar);
                {
                    EditorGUILayout.Space();                    
                    
                    /*EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                    {
                        //_scenePresetName = EditorGUILayout.TextField("_Scene Preset" + ":", _scenePresetName);

                        //EditorGUILayout.Space();

                        //if(GUILayout.Button("_Save", GUILayout.ExpandWidth(true)))
                        //{

                        //}
                    }
                    EditorGUILayout.EndHorizontal();
                    */

                    EditorGUI.BeginChangeCheck();
                    {
                        selectedSceneIndex = EditorGUILayout.Popup(Strings.PoseEditor.SceneLoadAdditive + ":", selectedSceneIndex, scenePaths.Select(x => Path.GetFileNameWithoutExtension(x)).ToArray());
                    }
                    if(EditorGUI.EndChangeCheck() && shapes.Count > 0)
                    {   
                        if(InPlaymode)
                        {
                            if(addedScene.isLoaded)
                                SceneManager.UnloadSceneAsync(addedScene);

                            var guid = EditorBuildSettings.scenes.ToList().Find(x => x.path.Replace('/', '\\').ToLower() == scenePaths[selectedSceneIndex].Replace('/', '\\').ToLower()).guid;

                            //SceneManager.LoadScene(scenePaths[selectedSceneIndex], LoadSceneMode.Additive);
                            var path = AssetDatabase.GUIDToAssetPath(guid.ToString());
                            SceneManager.LoadScene(path, LoadSceneMode.Additive);
                            for(int i = 0; i < SceneManager.sceneCount; i++)
                            {
                                if(SceneManager.GetSceneAt(i).path == path)
                                {
                                    addedScene = SceneManager.GetSceneAt(i);
                                    break;
                                }
                            }

                            EditorCoroutine.Start(GetSceneAndSetupNextFrame_co(path));
                        }
                        else
                        {
                            EditorSceneManager.CloseScene(addedScene, true);
                            addedScene = EditorSceneManager.OpenScene(scenePaths[selectedSceneIndex], OpenSceneMode.Additive);

                            if(addedScenePositions == null)
                                addedScenePositions = new List<PumkinsPoseEditorPosition>();

                            addedScenePositions.Clear();
                            addedScenePositions.AddRange(GameObject.FindObjectsOfType<PumkinsPoseEditorPosition>());

                            if(overrideLights)
                            {
                                sceneLightStates = SetSceneLightsDisabledReturnStates(ThisScene);
                            }
                            else
                            {
                                addedSceneLightStates = SetSceneLightsDisabledReturnStates(addedScene);
                            }
                        }
                    }

                    EditorGUI.BeginChangeCheck();
                    {
                        overrideLights = EditorGUILayout.Toggle(Strings.PoseEditor.SceneOverrideLights, overrideLights);
                    }
                    if(addedScene.isLoaded && EditorGUI.EndChangeCheck())
                    {
                        if(overrideLights)
                        {
                            sceneLightStates = SetSceneLightsDisabledReturnStates(ThisScene);
                            SetSceneLightsEnabled(addedScene, addedSceneLightStates);
                        }
                        else
                        {
                            addedSceneLightStates = SetSceneLightsDisabledReturnStates(addedScene);
                            SetSceneLightsEnabled(ThisScene, sceneLightStates);
                        }
                    }

                    EditorGUILayout.Space();

                    EditorGUI.BeginDisabledGroup(addedScenePositions == null || addedScenePositions.Count == 0);
                    {
                        EditorGUI.BeginChangeCheck();
                        {
                            var s = new string[0];

                            if(addedScenePositions != null)
                            {
                                s = addedScenePositions.Select(x => x.positionName).ToArray();
                            }                            

                            selectedPositionIndex = EditorGUILayout.Popup(Strings.PoseEditor.AvatarPosition + ":", selectedPositionIndex, s);

                            positionOverridesPose = EditorGUILayout.Toggle(Strings.PoseEditor.AvatarPositionOverridePose, positionOverridesPose);
                            positionOverridesShapes = EditorGUILayout.Toggle(Strings.PoseEditor.AvatarPositionOverrideBlendshapes, positionOverridesShapes);

                            EditorGUILayout.Space();                            
                        }
                        if(EditorGUI.EndChangeCheck())
                        {
                            ApplyPosition(selectedAvatar, addedScenePositions[selectedPositionIndex], positionOverridesPose, positionOverridesShapes);
                        }
                    }
                    EditorGUI.EndDisabledGroup();

                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUI.BeginDisabledGroup(InPlaymode);
                        {
                            if(GUILayout.Button(Strings.PoseEditor.SceneSaveChanges))
                            {
                                EditorSceneManager.SaveScene(addedScene);
                            }
                        }
                        EditorGUI.EndDisabledGroup();

                        if(GUILayout.Button(Strings.PoseEditor.UnloadScene, GUILayout.ExpandWidth(true)))
                        {
                            SetSceneLightsEnabled(ThisScene, sceneLightStates);

                            if(InPlaymode)
                            {
                                SceneManager.UnloadSceneAsync(addedScene);
                            }
                            else
                            {
                                EditorSceneManager.CloseScene(addedScene, true);
                            }

                            if(addedScenePositions != null)
                                addedScenePositions.Clear();
                            if(addedSceneLightStates != null)
                                addedSceneLightStates.Clear();
                            if(sceneLightStates != null)
                                sceneLightStates.Clear();
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.Space();

                    if(GUILayout.Button(Strings.PoseEditor.ResetPosition))
                    {
                        selectedAvatar.transform.position = Vector3.zero;
                    }

                    /*if(GUILayout.Button("_Load Scene", GUILayout.ExpandWidth(true)))
                    {
                        EditorSceneManager.CloseScene(addedScene, true);

                        addedScene = EditorSceneManager.OpenScene(ScenePresetPath + _scenePresetName + ".unity", OpenSceneMode.Additive);
                    }
                    */                    

                    EditorGUILayout.Space();
                }
                EditorGUI.EndDisabledGroup();
            }
            
            DrawLine();
            EditorGUILayout.Space();

            //===========================================================================
            //Pose=======================================================================
            //===========================================================================

            if(_pose_expand = GUILayout.Toggle(_pose_expand, Strings.PoseEditor.Pose, Styles.Foldout_title))
            {
                EditorGUI.BeginDisabledGroup(!selectedAvatar);
                {
                    EditorGUILayout.Space();
                    //_posePresetName = EditorGUILayout.TextField("_Pose Preset" + ":", _posePresetName);

                    EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                    {
                        _posePresetName = EditorGUILayout.TextField(Strings.PoseEditor.NewPose, _posePresetName);
                        if(GUILayout.Button(Strings.PoseEditor.SaveButton, GUILayout.ExpandWidth(true)))
                        {
                            if(string.IsNullOrEmpty(_posePresetName))
                            {
                                PumkinsAvatarTools.Log(Strings.Log.NameIsEmpty, LogType.Warning);
                            }
                            else
                            {
                                if(useHumanPoses)
                                {
                                    var anim = selectedAvatar.GetComponent<Animator>();
                                    HumanPoseHandler hph = new HumanPoseHandler(anim.avatar, selectedAvatar.transform);
                                    HumanPose hp = new HumanPose();

                                    hph.GetHumanPose(ref hp);

                                    HumanPosePreset newPose = new HumanPosePreset(_posePresetName, anim.rootPosition, hp);

                                    int i = humanPoses.FindIndex(o => o != null && o.poseName.ToLower() == newPose.poseName);
                                    if(i != -1)
                                        humanPoses[i] = newPose;
                                    else
                                        humanPoses.Add(newPose);
                                }
                                else
                                {
                                    var ts = selectedAvatar.GetComponentsInChildren<Transform>();
                                    var settings = new Dictionary<string, SerialTransform>();

                                    foreach(var t in ts)
                                    {
                                        if(t.root != t && (!poseOnlySaveChangedRotations || !PumkinsAvatarTools.TransformIsInDefaultPosition(t, true)))
                                            settings.Add(PumkinsAvatarTools.GetGameObjectPath(t.gameObject), t);
                                    }

                                    var p = new PosePreset(_posePresetName, settings);
                                    p.SaveToFile(PosePresetPath, overwriteExisting);
                                }

                                CheckFolders();
                            }
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    poseOnlySaveChangedRotations = EditorGUILayout.Toggle(Strings.PoseEditor.OnlySavePoseChanges, poseOnlySaveChangedRotations);

                    EditorGUILayout.Space();

                    EditorGUI.BeginChangeCheck();
                    {
                        selectedPoseIndex = EditorGUILayout.Popup(Strings.PoseEditor.LoadPose + ":", selectedPoseIndex, poses.Select(x => x.poseName).ToArray());
                        //selectedHumanPoseIndex = EditorGUILayout.Popup(Strings.PoseEditor.LoadPose + ":", selectedHumanPoseIndex, humanPoses.Select(x => x.poseName).ToArray());
                    }
                    if(EditorGUI.EndChangeCheck() && poses.Count > 0)
                    {
                        ApplyPose(selectedAvatar, positionOverridesPose);
                        //ApplyHumanPose(selectedAvatar, humanPoses[selectedHumanPoseIndex].poseName);
                    }

                    EditorGUILayout.Space();

                    if(GUILayout.Button(Strings.Tools.ResetPose))
                    {
                        Undo.RegisterFullObjectHierarchyUndo(selectedAvatar, Strings.Tools.ResetPose);
                        if(!PumkinsAvatarTools.ResetPose(selectedAvatar))
                            SetTPose(selectedAvatar);
                    }

                    EditorGUILayout.Space();
                }
                EditorGUI.EndDisabledGroup();
            }

            DrawLine();
            EditorGUILayout.Space();

            //===========================================================================
            //Blendshapes================================================================
            //===========================================================================

            if(_blendshape_expand = GUILayout.Toggle(_blendshape_expand, Strings.PoseEditor.Blendshapes, Styles.Foldout_title))
            {
                EditorGUI.BeginDisabledGroup(!selectedAvatar);
                {
                    EditorGUILayout.Space();                    

                    EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                    {
                        _blendshapePresetName = EditorGUILayout.TextField(Strings.PoseEditor.NewPreset + ":", _blendshapePresetName);
                        if(GUILayout.Button(Strings.PoseEditor.SaveButton, GUILayout.ExpandWidth(true)))
                        {
                            if(string.IsNullOrEmpty(_blendshapePresetName))
                            {
                                PumkinsAvatarTools.Log(Strings.Log.NameIsEmpty, LogType.Warning);
                            }
                            var renders = selectedAvatar.GetComponentsInChildren<SkinnedMeshRenderer>();

                            if(renders.Length > 0)
                            {
                                var shapeDict = new Dictionary<string, List<PoseBlendshape>>();

                                foreach(var r in renders)
                                {
                                    string renderPath = PumkinsAvatarTools.GetGameObjectPath(r.gameObject);
                                    var shapeList = new List<PoseBlendshape>();

                                    for(int i = 0; i < r.sharedMesh.blendShapeCount; i++)
                                    {
                                        float weight = r.GetBlendShapeWeight(i);
                                        var b = new PoseBlendshape(r.sharedMesh.GetBlendShapeName(i), weight);

                                        if(weight > 0)
                                            shapeList.Add(b);
                                    }

                                    shapeDict.Add(renderPath, shapeList);
                                }

                                BlendshapePreset bp = new BlendshapePreset(_blendshapePresetName, shapeDict);

                                //PoseEditorSaveBlendshapesPopup.ShowWindow(bp);

                                bp.SaveToFile(BlendshapesPresetPath, overwriteExisting);

                                CheckFolders();                                                                
                            }
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.Space();

                    EditorGUI.BeginChangeCheck();
                    {
                        selectedShapeIndex = EditorGUILayout.Popup(Strings.PoseEditor.LoadPreset + ":", selectedShapeIndex, shapes.Select(x => x.presetName).ToArray());
                    }
                    if(EditorGUI.EndChangeCheck() && shapes.Count > 0)
                    {
                        ApplyBlendshapes(selectedAvatar, positionOverridesShapes);
                    }

                    EditorGUILayout.Space();

                    EditorGUILayout.BeginHorizontal();
                    {
                        if(GUILayout.Button(Strings.Tools.ZeroBlendshapes))
                        {
                            Undo.RegisterFullObjectHierarchyUndo(selectedAvatar, "Reset Blendshapes");
                            PumkinsAvatarTools.ResetBlendShapes(selectedAvatar, false);
                        }
                        if(GUILayout.Button(Strings.Tools.RevertBlendshapes))
                        {
                            Undo.RegisterFullObjectHierarchyUndo(selectedAvatar, "Revert Blendshapes to Prefab");
                            PumkinsAvatarTools.ResetBlendShapes(selectedAvatar, true);
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.Space();
                }
                EditorGUI.EndDisabledGroup();
            }

            #region Deadcode
            /*if(_blendshape_expand = GUILayout.Toggle(_blendshape_expand, "_Blendshapes", Styles.Foldout_title))
            {
                EditorGUI.BeginDisabledGroup(!selectedAvatar);
                {
                    EditorGUILayout.Space();
                    _blendshapePresetName = EditorGUILayout.TextField("_Blendshape Preset" + ":", _blendshapePresetName);

                    EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                    {
                        if(GUILayout.Button("_Save Blendshapes", GUILayout.ExpandWidth(true)))
                        {
                            if(string.IsNullOrEmpty(_blendshapePresetName))
                            {
                                PumkinsAvatarTools.Log("_Name is empty");
                            }
                            var renders = selectedAvatar.GetComponentsInChildren<SkinnedMeshRenderer>();

                            if(renders.Length > 0)
                            {
                                var shapeDict = new Dictionary<string, List<PoseBlendshape>>();

                                foreach(var r in renders)
                                {
                                    string renderPath = Functions.GetGameObjectPath(r.gameObject);
                                    var shapeList = new List<PoseBlendshape>();

                                    for(int i = 0; i < r.sharedMesh.blendShapeCount; i++)
                                    {
                                        float weight = r.GetBlendShapeWeight(i);
                                        var b = new PoseBlendshape(r.sharedMesh.GetBlendShapeName(i), weight);

                                        if(weight > 0)
                                            shapeList.Add(b);
                                    }

                                    shapeDict.Add(renderPath, shapeList);
                                }

                                BlendshapePreset bp = new BlendshapePreset(_blendshapePresetName, shapeDict);

                                bp.SaveToFile(BlendshapesPresetPath, overwriteExisting);
                            }
                        }
                        if(GUILayout.Button("_Load Blendshapes", GUILayout.ExpandWidth(true)))
                        {
                            string json = File.ReadAllText(BlendshapesPresetPath + _blendshapePresetName + '.' + blendshapeExtension);
                            if(!string.IsNullOrEmpty(json))
                            {
                                var blend = JsonConvert.DeserializeObject<BlendshapePreset>(json);
                                if(blend != null)
                                {
                                    Undo.RegisterFullObjectHierarchyUndo(selectedAvatar, "Load Blendshapes");
                                    PumkinsAvatarTools.ResetBlendShapes(selectedAvatar);

                                    blend.ApplyBlendshapes(selectedAvatar);
                                }
                            }
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    if(GUILayout.Button(Strings.Tools.ResetBlendshapes))
                    {
                        PumkinsAvatarTools.ResetBlendShapes(selectedAvatar);
                    }

                    EditorGUILayout.Space();
                }
                EditorGUI.EndDisabledGroup();
            }*/
            #endregion

            DrawLine();
            EditorGUILayout.Space();

            EditorGUI.BeginDisabledGroup(!selectedAvatar);
            {
                if(GUILayout.Button(Strings.PoseEditor.ReloadButton))
                {
                    CheckFolders(true);                    
                }
                //if(GUILayout.Button("Extract TPose"))
                //{
                //    var tpose = poses.First(o => o.poseName.ToLower() == "tpose");
                                        
                //    string s = tpose.ToHardcodedString();
                //    Debug.Log(s);
                //}
            }
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// Gets muscle values from a humanoid avatar and returns them as a string. Don't do this
        /// </summary>        
        static string GetHumanPoseValues(GameObject avatar)
        {
            var anim = avatar.GetComponent<Animator>();

            if(anim == null && !anim.isHuman)
                return null;

            HumanPoseHandler hph = new HumanPoseHandler(anim.avatar, avatar.transform);
            HumanPose hp = new HumanPose();
            hph.GetHumanPose(ref hp);

            string s = "";
            for(int i = 0; i < hp.muscles.Length; i++)
            {
                s += String.Format("humanPose.muscles[{0}] = {1}f;", i, hp.muscles[i]);
                s += "\n";
            }            
            return s;
        }

        /// <summary>
        /// Sets hardcoded TPose.
        /// </summary>        
        public static void SetTPose(GameObject avatar)
        {
            Undo.RegisterFullObjectHierarchyUndo(avatar, "Set TPose");            
                        
            var anim = avatar.GetComponent<Animator>();
                        
            Vector3 pos = avatar.transform.position;
            Quaternion rot = avatar.transform.rotation;

            avatar.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

            var humanPoseHandler = new HumanPoseHandler(anim.avatar, avatar.transform);

            var humanPose = new HumanPose();
            humanPoseHandler.GetHumanPose(ref humanPose);
                        
            if(humanPose.bodyPosition.y < 1)
            {
                PumkinsAvatarTools.Log(Strings.PoseEditor.BodyPositionYTooSmall, LogType.Warning, humanPose.bodyPosition.y.ToString());
                humanPose.bodyPosition.y = 1;                
            }

            #region Hardcoded TPose Values
            humanPose.muscles[0] = -6.830189E-07f;
            humanPose.muscles[1] = 4.268869E-08f;
            humanPose.muscles[2] = 4.268868E-08f;
            humanPose.muscles[3] = -8.537737E-08f;
            humanPose.muscles[4] = 0f;
            humanPose.muscles[5] = 0f;
            humanPose.muscles[6] = 0f;
            humanPose.muscles[7] = 0f;
            humanPose.muscles[8] = 0f;
            humanPose.muscles[9] = 4.268868E-08f;
            humanPose.muscles[10] = -8.537737E-08f;
            humanPose.muscles[11] = 4.268868E-08f;
            humanPose.muscles[12] = 3.415095E-07f;
            humanPose.muscles[13] = 0f;
            humanPose.muscles[14] = 0f;
            humanPose.muscles[15] = 0f;
            humanPose.muscles[16] = 0f;
            humanPose.muscles[17] = 0f;
            humanPose.muscles[18] = 0f;
            humanPose.muscles[19] = 0f;
            humanPose.muscles[20] = 0f;
            humanPose.muscles[21] = 0.5994893f;
            humanPose.muscles[22] = 0.004985309f;
            humanPose.muscles[23] = 0.00297395f;
            humanPose.muscles[24] = 0.9989594f;
            humanPose.muscles[25] = -0.02284526f;
            humanPose.muscles[26] = -3.449878E-05f;
            humanPose.muscles[27] = -0.0015229f;
            humanPose.muscles[28] = -4.781132E-07f;
            humanPose.muscles[29] = 0.599489f;
            humanPose.muscles[30] = 0.004985378f;
            humanPose.muscles[31] = 0.002974245f;
            humanPose.muscles[32] = 0.9989589f;
            humanPose.muscles[33] = -0.02284535f;
            humanPose.muscles[34] = -3.548912E-05f;
            humanPose.muscles[35] = -0.001522672f;
            humanPose.muscles[36] = -1.024528E-07f;
            humanPose.muscles[37] = -2.429621E-07f;
            humanPose.muscles[38] = 1.549688E-07f;
            humanPose.muscles[39] = 0.3847253f;
            humanPose.muscles[40] = 0.310061f;
            humanPose.muscles[41] = -0.103543f;
            humanPose.muscles[42] = 0.9925866f;
            humanPose.muscles[43] = 0.159403f;
            humanPose.muscles[44] = -0.01539368f;
            humanPose.muscles[45] = 0.01405432f;
            humanPose.muscles[46] = 5.680533E-08f;
            humanPose.muscles[47] = 2.701393E-07f;
            humanPose.muscles[48] = 0.3847259f;
            humanPose.muscles[49] = 0.3100605f;
            humanPose.muscles[50] = -0.1035404f;
            humanPose.muscles[51] = 0.9925874f;
            humanPose.muscles[52] = 0.1593992f;
            humanPose.muscles[53] = -0.01539393f;
            humanPose.muscles[54] = 0.01405326f;
            humanPose.muscles[55] = -0.7706841f;
            humanPose.muscles[56] = 0.423209f;
            humanPose.muscles[57] = 0.6456643f;
            humanPose.muscles[58] = 0.6362566f;
            humanPose.muscles[59] = 0.6677276f;
            humanPose.muscles[60] = -0.4597229f;
            humanPose.muscles[61] = 0.811684f;
            humanPose.muscles[62] = 0.8116837f;
            humanPose.muscles[63] = 0.6683907f;
            humanPose.muscles[64] = -0.5737826f;
            humanPose.muscles[65] = 0.8116839f;
            humanPose.muscles[66] = 0.8116843f;
            humanPose.muscles[67] = 0.6670681f;
            humanPose.muscles[68] = -0.6459302f;
            humanPose.muscles[69] = 0.8116837f;
            humanPose.muscles[70] = 0.8116839f;
            humanPose.muscles[71] = 0.666789f;
            humanPose.muscles[72] = -0.4676594f;
            humanPose.muscles[73] = 0.811684f;
            humanPose.muscles[74] = 0.8116839f;
            humanPose.muscles[75] = -0.7706831f;
            humanPose.muscles[76] = 0.4232127f;
            humanPose.muscles[77] = 0.6456538f;
            humanPose.muscles[78] = 0.6362569f;
            humanPose.muscles[79] = 0.6678051f;
            humanPose.muscles[80] = -0.4589976f;
            humanPose.muscles[81] = 0.8116843f;
            humanPose.muscles[82] = 0.8116842f;
            humanPose.muscles[83] = 0.668391f;
            humanPose.muscles[84] = -0.5737844f;
            humanPose.muscles[85] = 0.811684f;
            humanPose.muscles[86] = 0.8116837f;
            humanPose.muscles[87] = 0.6669571f;
            humanPose.muscles[88] = -0.6492739f;
            humanPose.muscles[89] = 0.8116841f;
            humanPose.muscles[90] = 0.8116843f;
            humanPose.muscles[91] = 0.6667888f;
            humanPose.muscles[92] = -0.4676568f;
            humanPose.muscles[93] = 0.8116842f;
            humanPose.muscles[94] = 0.8116836f;
            #endregion

            humanPoseHandler.SetHumanPose(ref humanPose);
            avatar.transform.SetPositionAndRotation(pos, rot);

            /*
            int i = poses.FindIndex(x => x.poseName.ToLower() == "tpose");
            if(i != -1)
            {
                poses[i].ApplyPose(avatar);
            }
            else
            {
                var p = defaultPoses.Find(x => x.poseName.ToLower() == "tpose");
                if(p != null)
                    p.ApplyPose(avatar);
            }*/
        }

        void ApplyHumanPose(GameObject avatar, HumanPosePreset hp)
        {
            Undo.RegisterFullObjectHierarchyUndo(avatar, "Load Human Pose");
            hp.ApplyPose(avatar);
        }

        void ApplyHumanPose(GameObject avatar, string poseName)
        {
            if(poseName != null)
            {
                int i = humanPoses.FindIndex(x => x.poseName.ToLower() == poseName.ToLower());
                if(i != -1)
                    selectedHumanPoseIndex = i;
            }

            ApplyHumanPose(avatar, humanPoses[selectedHumanPoseIndex]);
        }

        /// <summary>
        /// Applies pose to avatar from file
        /// </summary>        
        /// <param name="overridePose">I don't remember what this does</param>
        /// <param name="overrideName">Name of pose to apply</param>
        void ApplyPose(GameObject avatar, bool overridePose, string overrideName = null)
        {
            Undo.RegisterFullObjectHierarchyUndo(avatar, "Load Pose");
            PumkinsAvatarTools.ResetPose(avatar);

            if(overridePose && overrideName != null)
            {
                int i = poses.FindIndex(x => x.poseName.ToLower() == overrideName.ToLower());
                if(i != -1)
                    selectedPoseIndex = i;
            }
                
            poses[selectedPoseIndex].ApplyPose(avatar);
        }
        /// <summary>
        /// Applies Blendshapes from file
        /// </summary>        
        /// <param name="overrideShapes">I don't remember what this does either</param>
        /// <param name="overrideName">Name of blendshape to apply</param>
        void ApplyBlendshapes(GameObject avatar, bool overrideShapes, string overrideName = null)
        {
            Undo.RegisterFullObjectHierarchyUndo(avatar, "Load Blendshapes");
            PumkinsAvatarTools.ResetBlendShapes(avatar, false);

            if(overrideShapes && overrideName != null)
            {
                int i = shapes.FindIndex(x => x.presetName.ToLower() == overrideName.ToLower());
                if(i != -1)
                    selectedShapeIndex = i;
            }

            shapes[selectedShapeIndex].ApplyBlendshapes(avatar);
        }

        void DrawLine()
        {
            PumkinsAvatarTools.DrawGuiLine();
        }

        /// <summary>
        /// Applies position to avatar, for when loading a scene
        /// </summary>        
        /// <param name="pos">Position to move to</param>
        /// <param name="applyPose">Should the pose associated with the position be applied?</param>
        /// <param name="applyBlendshapes">Should the blendshapes associated with the position be applied?</param>
        void ApplyPosition(GameObject avatar, PumkinsPoseEditorPosition pos, bool applyPose, bool applyBlendshapes)
        {
            if(position == null || avatar == null || pos == null)
                return;

            avatar.transform.position = pos.transform.position;

            if(applyPose && !string.IsNullOrEmpty(pos.poseOverrideName))            
                ApplyPose(avatar, applyPose, pos.poseOverrideName);            

            if(applyBlendshapes && !string.IsNullOrEmpty(pos.blendshapePresetOverrideName))
                ApplyBlendshapes(avatar, applyBlendshapes, pos.blendshapePresetOverrideName);
        }

        /// <summary>
        /// Toggles between overriding scene lights with additively loaded scene and saves old states, by scene id
        /// </summary>
        /// <param name="sceneIndex">Index of scene, should be given by scene manager</param>
        /// <param name="lightsStates">Old light states use to toggle back old lights</param>
        void SetSceneLightsEnabled(int sceneIndex, Dictionary<Light, bool> lightsStates)
        {
            Scene sc;
            if(InPlaymode)
                sc = SceneManager.GetSceneAt(sceneIndex);
            else
                sc = EditorSceneManager.GetSceneAt(sceneIndex);
            
            SetSceneLightsEnabled(sc, lightsStates);
        }

        /// <summary>
        /// Toggles between overriding scene lights with additively loaded scene and saves old states, by scene name
        /// </summary>
        /// <param name="scene">Scene name</param>
        /// <param name="lightStates">Old light states use to toggle back old lights</param>
        void SetSceneLightsEnabled(Scene scene, Dictionary<Light, bool> lightStates)
        {
            if(lightStates != null && lightStates.Count > 0)
            {
                foreach(var kv in lightStates)
                {
                    kv.Key.enabled = kv.Value;
                }
            }
        }

        /// <summary>
        /// Saves current light states and disables all lights
        /// </summary>
        /// <param name="sceneIndex">Index of scene, should be given by the scene manager</param>
        /// <returns>Light states, used for toggles. Useful in case lights are already disabled so we don't turn them on when toggling</returns>
        Dictionary<Light, bool> SetSceneLightsDisabledReturnStates(int sceneIndex)
        {
            Scene sc;
            if(InPlaymode)
                sc = SceneManager.GetSceneAt(sceneIndex);
            else
                sc = EditorSceneManager.GetSceneAt(sceneIndex);

            return SetSceneLightsDisabledReturnStates(sc);
        }

        /// <summary>
        /// Saves current light states and disables all lights
        /// </summary>
        /// <param name="scene">Name of scene</param>
        /// <returns>Light states, used for toggles. Useful in case lights are already disabled so we don't turn them on when toggling</returns>
        Dictionary<Light, bool> SetSceneLightsDisabledReturnStates(Scene scene)
        {
            if(!scene.isLoaded)
                return null;

            Dictionary<Light, bool> d = new Dictionary<Light, bool>();

            var objs = scene.GetRootGameObjects();

            foreach(var o in objs)
            {
                var l = o.GetComponentsInChildren<Light>();
                for(int i = 0; i < l.Length; i++)
                {                    
                    d.Add(l[i], l[i].enabled);
                    l[i].enabled = false;
                }
            }

            if(d.Count == 0)
                return null;
            else
                return d;
        }

        /// <summary>
        /// Gets scene then toggles lights if needed
        /// </summary>
        /// <param name="path">Path of scene</param>
        /// <returns>Nothing, but is needed to run the couroutine to load everything next frame</returns>
        IEnumerator GetSceneAndSetupNextFrame_co(string path)
        {
            yield return new WaitForEndOfFrame();
            addedScene = SceneManager.GetSceneByPath(path);

            if(addedScenePositions == null)
                addedScenePositions = new List<PumkinsPoseEditorPosition>();

            addedScenePositions.Clear();
            addedScenePositions.AddRange(GameObject.FindObjectsOfType<PumkinsPoseEditorPosition>());

            if(overrideLights)
            {
                sceneLightStates = SetSceneLightsDisabledReturnStates(ThisScene);
            }
            else
            {
                addedSceneLightStates = SetSceneLightsDisabledReturnStates(addedScene);
            }
        }
    }       

    #region DataStructures

    /// <summary>
    /// Serializable Transform class
    /// </summary>
    [System.Serializable]
    public class SerialTransform
    {
        public SerialVector3 position, localPosition, scale, localScale, eulerAngles, localEulerAngles;
        public SerialQuaternion rotation, localRotation;            

        SerialTransform()
        {
            this.position = Vector3.zero;
            this.rotation = Quaternion.identity;
            this.scale = Vector3.one;
            this.localEulerAngles = Vector3.zero;
            this.localPosition = Vector3.zero;
            this.localRotation = Quaternion.identity;
            this.localScale = Vector3.one;            
        }

        [JsonConstructor]
        public SerialTransform(Vector3 position, Quaternion rotation, Vector3 scale, Vector3 eulerAngles, Vector3 localEulerAngles, Vector3 localScale, Vector3 localPosition, Quaternion localRotation, float version) : base()
        {
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
            this.localEulerAngles = localEulerAngles;
            this.localPosition = localPosition;
            this.localRotation = localRotation;
            this.localScale = localScale;            
        }

        public SerialTransform(Vector3 localPosition, Quaternion localRotation, Vector3 localScale, Vector3 localEulerAngles) : base()
        {
            this.localPosition = localPosition;
            this.localRotation = localRotation;
            this.localScale = localScale;
            this.localEulerAngles = localEulerAngles;
        }

        public SerialTransform(Quaternion localRotation) :  base()
        {
            this.localRotation = localRotation;
        }

        public SerialTransform(Transform t) : base()
        {
            position = t.position;
            localPosition = t.localPosition;

            scale = t.localScale;
            localScale = t.localScale;

            rotation = t.rotation;
            localRotation = t.localRotation;

            eulerAngles = t.eulerAngles;

            if(localEulerAngles != null)
                localEulerAngles = t.localEulerAngles;
        }

        public static implicit operator SerialTransform(Transform t)
        {
            return new SerialTransform(t);
        }
    }

    /// <summary>
    /// Serializable Quaternion class
    /// </summary>
    [System.Serializable]
    public class SerialQuaternion
    {
        public float x, y, z, w;        

        public SerialQuaternion(Quaternion q)
        {
            x = q.x;
            y = q.y;
            z = q.z;
            w = q.w;            
        }

        private SerialQuaternion(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public static implicit operator SerialQuaternion(Quaternion q)
        {
            return new SerialQuaternion(q);
        }

        public static implicit operator Quaternion(SerialQuaternion q)
        {
            return new Quaternion(q.x, q.y, q.z, q.w);
        }
    }

    /// <summary>
    /// Serializable Vector3 class
    /// </summary>
    [System.Serializable]
    public class SerialVector3
    {
        public float x, y ,z;

        public SerialVector3(Vector3 v)
        {
            x = v.x;
            y = v.y;
            z = v.z;
        }

        public SerialVector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static implicit operator SerialVector3(Vector3 v)
        {
            return new SerialVector3(v);
        }

        public static implicit operator Vector3(SerialVector3 v)
        {
            return new Vector3(v.x, v.y, v.z);
        }

        public static SerialVector3 operator * (SerialVector3 v, float f)
        {
            return new SerialVector3(new Vector3(v.x, v.y, v.z) * f);
        }
    }

    /// <summary>
    /// This might be much better than the transform-based PosePreset
    /// </summary>
    [System.Serializable]
    public class HumanPosePreset
    {
        public string poseName;        
        public Vector3 bodyPosition;        
        public Quaternion bodyRotation;
        public Vector3 rootPosition;
        public float[] muscles;

        public HumanPosePreset(string poseName, Vector3 rootPosition, HumanPose p)
        {
            this.poseName = poseName;
            bodyPosition = p.bodyPosition;
            bodyRotation = p.bodyRotation;
            muscles = p.muscles;
            this.rootPosition = rootPosition;
        }

        public HumanPosePreset(string poseName, Vector3 bodyPosition, Quaternion bodyRotation, Vector3 rootPosition, float[] muscles)
        {
            this.poseName = poseName;
            this.rootPosition = rootPosition;
            this.bodyPosition = bodyPosition;
            this.bodyRotation = bodyRotation;
            this.muscles = muscles;
        }

        internal bool ApplyPose(GameObject avatar)
        {
            Animator anim = avatar.GetComponent<Animator>();
            if(anim == null || !anim.isHuman)            
                return false;

            HumanPoseHandler hph = new HumanPoseHandler(anim.avatar, avatar.transform);
            HumanPose hp = this;   
            hph.SetHumanPose(ref hp);            
            
            return true;
        }

        public static implicit operator HumanPose(HumanPosePreset v)
        {
            HumanPose hp = new HumanPose();
            hp.bodyPosition = v.bodyPosition;
            hp.bodyRotation = v.bodyRotation;
            hp.muscles = v.muscles;

            return hp;
        }
    }

    /// <summary>
    /// Serializable Pose preset, used to store avatar transforms associated to a name
    /// </summary>
    [System.Serializable]
    public class PosePreset
    {
        public string poseName;
        public Dictionary<string, SerialTransform> transforms;        

        public PosePreset(string poseName, Dictionary<string, SerialTransform> transformSettings = null)
        {
            this.poseName = poseName;
            this.transforms = transformSettings;            
        }

        /// <summary>
        /// Adds or overwrites a transform to the pose dictionary.
        /// </summary>
        /// <param name="path">Path of the transform</param>
        /// <param name="transform">Transform for positoin and rotation settings</param>
        /// <param name="allowOverwrite">Whether to overwrite settings if transform exists or not</param>
        /// <returns>Returns true if successfully added or overwritten</returns>
        public bool AddTransform(string path, Transform transform, bool allowOverwrite = true)
        {
            if(transforms.ContainsKey(path))
            {
                if(allowOverwrite)
                    transforms[path] = transform;
                else
                    return false;
            }
            else
            {
                transforms.Add(path, transform);
            }
            return true;
        }        

        /// <summary>
        /// Serialize and save pose to file
        /// </summary>
        /// <param name="filePath">Path to folder where to save the file to, excluding filename</param>
        /// <param name="overwriteExisting">Overwrite file if one with the same name already exists, if not will add a number to the end and create new file</param>
        /// <returns></returns>
        public bool SaveToFile(string filePath, bool overwriteExisting)
        {
            if(transforms == null)
                transforms = new Dictionary<string, SerialTransform>();            

            string path = filePath + "/" + poseName + '.' + PumkinsPoseEditor.poseExtension;

            if(!overwriteExisting)
                path = PumkinsAvatarTools.NextAvailableFilename(filePath + "/" + poseName + '.' + PumkinsPoseEditor.poseExtension);

            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            
            if(!string.IsNullOrEmpty(json))
            {
                File.WriteAllText(path, json);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Don't hurt me, I need this for a default failsafe or two.
        /// </summary>
        /// <returns>C# code to declare and initialize this object</returns>
        public string ToHardcodedString()
        {
            if(transforms == null)
                transforms = new Dictionary<string, SerialTransform>();

            string s = string.Format("new PosePreset(\"{0}\", new Dictionary<string, SerialTransform>\n\t", poseName);
            s += "{\n";
            foreach(var t in transforms)
            {
                s += "\t\t{";
                s += string.Format("\"{0}\", new SerialTransform(new Quaternion({1}f, {2}f, {3}f, {4}f))", t.Key, t.Value.localRotation.x, t.Value.localRotation.y, t.Value.localRotation.z, t.Value.localRotation.w);
                s += "},\n";
            }
            s += "}),\n";

            return s;
        }

        /// <summary>
        /// Apply this pose to avatar
        /// </summary>        
        public void ApplyPose(GameObject avatar)//, bool childrenFirst = false)
        {
            if(!avatar)
                return;

            //if(childrenFirst)
            //{
            //    var ls = transforms.Keys.OrderBy(o => o.Count(c => c == '/'));

            //    foreach(var k in ls)
            //    {
            //        var t = avatar.transform.Find(k);
            //        var v = transforms[k];

            //        if(t != null && v != null)
            //        {
            //            t.localEulerAngles = v.localEulerAngles;
            //            t.localRotation = v.localRotation;
            //        }
            //    }
            //}
            //else
            //{
                foreach(var kv in transforms)
                {
                    var t = avatar.transform.Find(kv.Key);

                    if(t != null)
                    {
                        t.localEulerAngles = kv.Value.localEulerAngles;
                        t.localRotation = kv.Value.localRotation;
                    }
                }
            //}
        }
    }

    [System.Serializable]
    public class BlendshapePreset
    {
        public string presetName;
        public Dictionary<string, List<PoseBlendshape>> blendshapes;

        public BlendshapePreset(string presetName, Dictionary<string, List<PoseBlendshape>> blendshapes = null)
        {
            this.presetName = presetName;
            this.blendshapes = blendshapes;

            if(blendshapes == null)
                blendshapes = new Dictionary<string, List<PoseBlendshape>>();            
        }

        public bool AddBlendshape(string meshRendererPath, PoseBlendshape blend)
        {
            var d = blendshapes[meshRendererPath];

            if(d != null && d.Count > 0 && !d.Exists(x => x.Name.ToLower() == blend.Name.ToLower()))
            {
                d.Add(blend);
                return true;
            }
            return false;
        }

        public bool RemoveBlendshape(string meshRendererPath, string shapeName)
        {
            var d = blendshapes[meshRendererPath];
            var b = d.Find(x => x.Name.ToLower() == shapeName.ToLower());

            if(d != null && d.Count > 0 && b != null)
            {
                d.Remove(b);
                return true;
            }
            return false;
        }

        public bool SaveToFile(string filePath, bool overwriteExisting)
        {
            string path = filePath + "/" + presetName + '.' + PumkinsPoseEditor.blendshapeExtension;

            if(!overwriteExisting)
                path = PumkinsAvatarTools.NextAvailableFilename(filePath + "/" + presetName + '.' + PumkinsPoseEditor.blendshapeExtension);

            string json = JsonConvert.SerializeObject(this, Formatting.Indented);

            if(!string.IsNullOrEmpty(json))
            {
                File.WriteAllText(path, json);
                return true;
            }
            return false;
        }

        public void ApplyBlendshapes(GameObject avatar)
        {
            if(!avatar)
                return;

            foreach(var b in blendshapes)
            {
                var t = avatar.transform.Find(b.Key);
                if(t)
                {
                    var sr = t.GetComponent<SkinnedMeshRenderer>();
                    if(sr)
                    {
                        foreach(var shape in b.Value)
                        {
                            int index = sr.sharedMesh.GetBlendShapeIndex(shape.Name);

                            if(shape.AlternateNames.Count > 0)
                            {
                                for(int i = 0; index == -1 && i < shape.AlternateNames.Count; i++)
                                {
                                    index = sr.sharedMesh.GetBlendShapeIndex(shape.AlternateNames[i]);
                                }
                            }

                            if(index != -1)
                            {
                                sr.SetBlendShapeWeight(index, shape.Weight);
                            }
                        }
                    }
                }
            }
        }
    }

    [System.Serializable]
    public class CameraPreset
    {        
        SerialTransform transform;
        Camera camera;
    }

    [System.Serializable]
    public class ScenePreset
    {
        string presetName;

        public void Load()
        {
            Debug.Log("Doesn't do anything yet");
        }
    }

    [System.Serializable]
    public class PoseBlendshape
    {
        public string FriendlyName
        {
            get
            {
                return friendlyName;
            }
            set
            {
                if(string.IsNullOrEmpty(value))
                    friendlyName = Name;
            }
        }

        public string Name
        {
            get; set;
        }

        public float Weight
        {
            get; set;
        }

        public List<string> AlternateNames
        {
            get; set;
        }

        string friendlyName;

        public PoseBlendshape(string name, float weight = 0, string friendlyName = null, List<string> alternateNames = null)
        {
            Name = name;
            Weight = weight;

            FriendlyName = friendlyName;
            if(alternateNames != null)
                AlternateNames = alternateNames;
            else
                AlternateNames = new List<string>();
        }
    }

    #endregion
}
