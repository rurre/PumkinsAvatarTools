using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pumkin.PoseEditor
{    
    [AddComponentMenu("Pumkin/Pose Editor/Pose Position")] [ExecuteInEditMode]
    public class PumkinsPoseEditorPosition : MonoBehaviour
    {
        [SerializeField] public string positionName;
        [SerializeField] public string poseOverrideName;
        [SerializeField] public string blendshapePresetOverrideName;        
        
        void Awake()
        {
            if(string.IsNullOrEmpty(positionName))
                positionName = gameObject.name;            
        }        
    }
}