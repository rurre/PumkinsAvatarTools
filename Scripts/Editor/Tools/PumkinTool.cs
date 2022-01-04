using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Pumkin.AvatarTools
{
    [Serializable]
    public abstract class PumkinTool
    {
        public abstract string NameInUI { get; }
        public virtual bool EnabledInUI => PumkinsAvatarTools.SelectedAvatar != null;
        public virtual bool ShowInQuickSetup => false;

        [SerializeField] public bool quickSetupActive = false;

        public void DrawGUI()
        {
            EditorGUI.BeginDisabledGroup(!EnabledInUI);
            OnGUI();
            EditorGUI.EndDisabledGroup();
        }

        public void DrawQuickSetupGUI()
        {
            EditorGUI.BeginDisabledGroup(!EnabledInUI);
            OnQuickSetupGUI();
            EditorGUI.EndDisabledGroup();
        }

        protected virtual void OnGUI()
        {
            if(GUILayout.Button(NameInUI))
                TryExecute(PumkinsAvatarTools.SelectedAvatar);
        }

        protected virtual void OnQuickSetupGUI()
        {
            if(!ShowInQuickSetup)
                return;
            quickSetupActive = EditorGUILayout.ToggleLeft(NameInUI, quickSetupActive);
        }

        protected bool RegisterUndoThenPrepare(GameObject avatar)
        {
            Undo.RegisterFullObjectHierarchyUndo(avatar, NameInUI);
            return Prepare(avatar);
        }

        protected virtual bool Prepare(GameObject avatar)
        {
            return true;
        }

        protected virtual bool DoAction(GameObject avatar)
        {
            return true;
        }

        protected virtual void Finish(GameObject avatar)
        {
            PumkinsAvatarTools.Log($"{NameInUI} finished.");
        }

        public void TryExecute(GameObject avatar)
        {
            if(RegisterUndoThenPrepare(avatar))
                if(DoAction(avatar))
                    Finish(avatar);
        }
    }
}