using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Pumkin.PoseEditor
{
    public class PumkinsPoseEditorUtils
    {
        public static void HandleClickSelection(GameObject gameObject, Event evt)
        {
            if(evt.shift || EditorGUI.actionKey)
            {
                UnityEngine.Object[] existingSelection = Selection.objects;

                // For shift, we check if EXACTLY the active GO is hovered by mouse and then subtract. Otherwise additive.
                // For control/cmd, we check if ANY of the selected GO is hovered by mouse and then subtract. Otherwise additive.
                // Control/cmd takes priority over shift.
                bool subtractFromSelection = EditorGUI.actionKey ? Selection.Contains(gameObject) : Selection.activeGameObject == gameObject;
                if(subtractFromSelection)
                {
                    // subtract from selection
                    var newSelection = new UnityEngine.Object[existingSelection.Length - 1];

                    int index = Array.IndexOf(existingSelection, gameObject);

                    System.Array.Copy(existingSelection, newSelection, index);
                    System.Array.Copy(existingSelection, index + 1, newSelection, index, newSelection.Length - index);

                    Selection.objects = newSelection;
                }
                else
                {
                    // add to selection
                    var newSelection = new UnityEngine.Object[existingSelection.Length + 1];
                    System.Array.Copy(existingSelection, newSelection, existingSelection.Length);
                    newSelection[existingSelection.Length] = gameObject;

                    Selection.objects = newSelection;
                }
            }
            else
                Selection.activeObject = gameObject;
        }

        public static void DrawBones(List<PoseEditorBone> bones)
        {
            for(int i = 0; i < bones.Count; i++)
            {
                var bone = bones[i];
                if(!bone)
                    continue;
                Handles.DrawPolyLine(bone.root.position, bone.tip.position);
                Handles.SphereHandleCap(0, bone.root.position, bone.root.localRotation, 0.02f, EventType.Repaint);     
            }
        }
    }

    public struct PoseEditorBone
    {
        public Transform root, tip;        

        public PoseEditorBone(Transform root, Transform tip)
        {
            this.root = root;
            this.tip = tip;
        }

        public static implicit operator bool(PoseEditorBone bone)
        {
            if(bone.root && bone.tip)
                return true;
            return false;
        }
    }
}