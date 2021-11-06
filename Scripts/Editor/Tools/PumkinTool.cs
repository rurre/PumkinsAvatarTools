using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Pumkin.AvatarTools
{
    public class PumkinTool
    {
        public virtual bool EnabledInUI { get; } = true;

        public void OnGUI()
        {
            
        }

        protected virtual void DrawGUI()
        {
            EditorGUI.BeginDisabledGroup(!EnabledInUI);
            OnGUI();
            EditorGUI.EndDisabledGroup();
        }
        
        public virtual void OnQuickSetupGUI()
        {
        
        }
                
        public virtual void DoAction()
        {
        
        }
    }
}