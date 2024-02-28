using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using System;
using System.IO;
using UnityEngine;
using System.Linq;
using Pumkin.AvatarTools.Copiers;
using Pumkin.PoseEditor;
using UnityEngine.UI;
using Pumkin.DataStructures;
using Pumkin.HelperFunctions;
using Pumkin.Presets;
using UnityEngine.Animations;

namespace Pumkin.AvatarTools
{
    // Mistakes were made. Technical debt too high. At least some kind of quick separation for now...
    public partial class PumkinsAvatarTools
    {
        public void OnGUI()
        {
            Settings.SerializedSettings.Update();

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField(Strings.Main.title, Styles.Label_mainTitle);

                if(GUILayout.Button(Icons.KofiIcon, Styles.IconButton))
                    Application.OpenURL(Strings.LINK_DONATION);

                if(GUILayout.Button(Icons.DiscordIcon, Styles.IconButton))
                    Application.OpenURL(Strings.LINK_DISCORD);

                if(GUILayout.Button(Icons.GithubIcon, Styles.IconButton))
                    Application.OpenURL(Strings.LINK_GITHUB);

                GUILayout.Space(4);

                if(GUILayout.Button(Icons.Settings, Styles.IconButton))
                    Settings._openedSettings = !Settings._openedSettings;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField(Strings.Credits.version);

            if(Settings._openedSettings)
            {
                DrawSettingsGUI();
            }
            else
            {
                DrawMainGUI();
            }
        }

        void DrawSettingsGUI()
        {
            EditorGUILayout.Space();
            GUILayout.BeginVertical();

            GUILayout.Label(Strings.Credits.redundantStrings);

            EditorGUILayout.Space();

            GUILayout.Label(Strings.Credits.addMoreStuff);

            GUILayout.BeginHorizontal();

            GUILayout.Label(Strings.Credits.pokeOnDiscord);

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            if(PumkinsLanguageManager.Languages.Count == 0)
                PumkinsLanguageManager.LoadTranslations();

            EditorGUILayout.Space();
            Helpers.DrawGUILine();

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUI.BeginChangeCheck();
                {
                    if(Settings._selectedLanguageIndex >= PumkinsLanguageManager.Languages.Count)
                        Settings._selectedLanguageIndex = PumkinsLanguageManager.GetIndexOfLanguage(Settings._selectedLanguageString);

                    Settings._selectedLanguageIndex = EditorGUILayout.Popup(Strings.Settings.language, Settings._selectedLanguageIndex, PumkinsLanguageManager.Languages.Select(o => o.ToString()).ToArray(), Styles.Popup);
                }
                if(EditorGUI.EndChangeCheck() && PumkinsLanguageManager.Languages.Count > 1)
                {
                    PumkinsLanguageManager.SetLanguage(PumkinsLanguageManager.Languages[Settings._selectedLanguageIndex]);
                    Settings._selectedLanguageString = Strings.Translation.ToString();
                }

                if(GUILayout.Button(Icons.Refresh, Styles.IconButton))
                {
                    PumkinsLanguageManager.LoadTranslations();
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            {
                if(GUILayout.Button(Strings.Buttons.openFolder))
                    Helpers.SelectAndPing(PumkinsLanguageManager.translationPathLocal);
                //if(GUILayout.Button(Strings.Settings.importLanguage))
                //    PumkinsLanguageManager.OpenFileImportLanguagePreset();
            }
            EditorGUILayout.EndHorizontal();

            Helpers.DrawGUILine();
            GUILayout.Label(Strings.Settings.misc + ":");

            EditorGUILayout.Space();

            Settings.handlesUiWindowPositionAtBottom = GUILayout.Toggle(Settings.handlesUiWindowPositionAtBottom, Strings.Settings.sceneViewOverlayWindowsAtBottom);

            EditorGUILayout.Space();
            Settings.verboseLoggingEnabled = GUILayout.Toggle(Settings.verboseLoggingEnabled, Strings.Settings.enableVerboseLogging);

            EditorGUILayout.Space();

            //TODO: Improve fallback and restore
#if PUMKIN_DEV
            Helpers.DrawGUILine();
            EditorGUILayout.HelpBox(Strings.Settings.experimentalWarning, MessageType.Warning);
            Settings.showExperimental = GUILayout.Toggle(Settings.showExperimental, Strings.Settings.showExperimental);
#endif

            EditorGUILayout.Space();

            GUILayout.FlexibleSpace();

            if(GUILayout.Button(Strings.Settings.uwu, "label", GUILayout.ExpandWidth(false)))
            {
                if(Strings.Settings.uwu == "uwu")
                    Strings.Settings.uwu = "OwO";
                else
                    Strings.Settings.uwu = "uwu";
            }
        }

        void DrawMainGUI()
        {
            EditorGUIUtility.SetIconSize(new Vector2(15, 15));

            EditorGUILayout.Space();

            DrawAvatarSelectionWithButtonGUI(true);

            Helpers.DrawGUILine();

            Settings._mainToolsScrollbar = EditorGUILayout.BeginScrollView(Settings._mainToolsScrollbar);
            {
                DrawToolsMenuGUI();

                EditorGUILayout.Space();

                DrawCopierMenuGUI();

                EditorGUILayout.Space();

                DrawRemoveComponentsMenuGUI();

                EditorGUILayout.Space();

                DrawAvatarInfoMenuGUI();

#if PUMKIN_DEV && VRC_SDK_VRCSDK3

                if(Settings.showExperimental)
                {
                    EditorGUILayout.Space();
                }
#endif
                EditorGUILayout.Space();

                DrawThumbnailsMenuGUI();

                EditorGUILayout.Space();

                // It's mostly useless so we're moving buttons to the top
                //DrawInfoMenuGUI();

                Helpers.DrawGUILine();
            }
            EditorGUILayout.EndScrollView();

            if(GUI.changed)
            {
                Settings.SerializedSettings.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(Settings);
            }
        }

        #if VRC_SDK_VRCSDK3
        private void DrawEditingViewpointGUI()
        {
            if(!SelectedAvatar)
            {
                EndEditingViewpoint(null, true);
                return;
            }

            Vector2 windowSize = new Vector2(200, 68);

            Rect rect = SceneView.currentDrawingSceneView.camera.pixelRect;
            if(Settings.handlesUiWindowPositionAtBottom)
                rect = new Rect(10, rect.height - 10 - windowSize.y, windowSize.x, windowSize.y);
            else
                rect = new Rect(new Vector2(10, 10), windowSize);

            Handles.BeginGUI();
            {
                GUILayout.BeginArea(rect, Styles.Box);
                {
                    GUILayout.Label(Strings.Tools.editViewpoint);
                    if(GUILayout.Button(Strings.Buttons.moveToEyes, GUILayout.MinWidth(80)))
                    {
                        _viewPosTemp = Helpers.GetViewpointAtEyeLevel(SelectedAvatar.GetComponent<Animator>()) + SelectedAvatar.transform.position;
                    }
                    GUILayout.BeginHorizontal();
                    {
                        if(GUILayout.Button(Strings.Buttons.cancel, GUILayout.MinWidth(80)))
                        {
                            EndEditingViewpoint(SelectedAvatar, true);
                        }

                        if(GUILayout.Button(Strings.Buttons.apply, GUILayout.MinWidth(80)))
                        {
                            EndEditingViewpoint(SelectedAvatar, false);
                        }
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndArea();
            }
            Handles.EndGUI();

            if(_tempAvatarDescriptor)
            {
                _viewPosTemp = Handles.PositionHandle(_viewPosTemp, Quaternion.identity);
                Handles.color = Colors.BallHandle;
                Handles.SphereHandleCap(0, _viewPosTemp, Quaternion.identity, 0.02f, EventType.Repaint);
            }
        }
#endif

        private void DrawEditingScaleGUI()
        {
            if(!SelectedAvatar)
            {
                EndScalingAvatar(null, true);
                return;
            }

            bool propertyChanged = false;
#if VRC_SDK_VRCSDK3
            Vector2 windowSize = new Vector2(200, 85);
#else
            Vector2 windowSize = new Vector2(200, 70);
#endif

            Rect rect = SceneView.currentDrawingSceneView.camera.pixelRect;
            if(Settings.handlesUiWindowPositionAtBottom)
                rect = new Rect(10, rect.height - 10 - windowSize.y, windowSize.x, windowSize.y);
            else
                rect = new Rect(new Vector2(10, 10), windowSize);

            Handles.BeginGUI();
            {
                //GUILayout.BeginArea(new Rect(10, rect.height - 10 - windowSize.y, windowSize.x, windowSize.y), Styles.Box);
                GUILayout.BeginArea(rect, Styles.Box);
                {
                    GUILayout.Label(Strings.Tools.editScale);
                    if(SerializedScaleTemp != null)
                    {
                        EditorGUILayout.PropertyField(SerializedScaleTemp, GUIContent.none);
                        if(Settings.SerializedSettings.ApplyModifiedPropertiesWithoutUndo())
                            propertyChanged = true;
                    }
                    else
                    {
                        EditorGUILayout.LabelField(Settings._avatarScaleTemp.ToString());
                    }
#if VRC_SDK_VRCSDK3
                    Settings.editingScaleMovesViewpoint = GUILayout.Toggle(Settings.editingScaleMovesViewpoint, Strings.Tools.editScaleMoveViewpoint);
#else
                    EditorGUILayout.Space();
#endif
                    GUILayout.BeginHorizontal();
                    {
                        if(GUILayout.Button(Strings.Buttons.cancel, GUILayout.MinWidth(80)))
                            EndScalingAvatar(SelectedAvatar, true);

                        if(GUILayout.Button(Strings.Buttons.apply, GUILayout.MinWidth(80)))
                            EndScalingAvatar(SelectedAvatar, false);
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndArea();
            }
            Handles.EndGUI();

#if VRC_SDK_VRCSDK3
            if(_tempAvatarDescriptor)
            {
                EditorGUI.BeginChangeCheck();
                {
                    Settings._avatarScaleTemp = Handles.ScaleSlider(Settings._avatarScaleTemp, SelectedAvatar.transform.position, Vector3.up, Quaternion.identity, HandleUtility.GetHandleSize(SelectedAvatar.transform.position) * 2, 0.01f);
                }
                if(EditorGUI.EndChangeCheck() || propertyChanged)
                {
                    SetAvatarScaleAndMoveViewpoint(_tempAvatarDescriptor, Settings._avatarScaleTemp);
                }

                if(Settings.editingScaleMovesViewpoint)
                {
                    Handles.color = Colors.BallHandle;
                    Handles.SphereHandleCap(0, _viewPosTemp, Quaternion.identity, 0.02f, EventType.Repaint);
                }
            }
            else
            {
                EndScalingAvatar(null, true);
            }
#else
            EditorGUI.BeginChangeCheck();
            {
                Settings._avatarScaleTemp = Handles.ScaleSlider(Settings._avatarScaleTemp, SelectedAvatar.transform.position, Vector3.up, Quaternion.identity, HandleUtility.GetHandleSize(SelectedAvatar.transform.position) * 2, 0.01f);
            }
            if(EditorGUI.EndChangeCheck() || propertyChanged)
            {
                SetAvatarScale(Settings._avatarScaleTemp);
            }
#endif
        }

        private void HandleKeyboardInput()
        {
            Event current = Event.current;
            if(current.type != EventType.KeyDown)
                return;

            if(_editingScale)
            {
                if(current.keyCode == KeyCode.Return || current.keyCode == KeyCode.KeypadEnter)
                {
                    EndScalingAvatar(SelectedAvatar, false);
                    current.Use();
                }
                else if(current.keyCode == KeyCode.Escape)
                {
                    EndScalingAvatar(null, true);
                    current.Use();
                }
            }
#if VRC_SDK_VRCSDK3
            else if(_editingView)
            {
                if(current.keyCode == KeyCode.Return || current.keyCode == KeyCode.KeypadEnter)
                {
                    EndEditingViewpoint(SelectedAvatar, false);
                    current.Use();
                }
                else if(current.keyCode == KeyCode.Escape)
                {
                    EndEditingViewpoint(null, true);
                    current.Use();
                }
            }
#endif
        }

        public static void DrawAvatarSelectionWithButtonGUI(bool updateAvatarInfo, bool showSelectFromSceneButton = true, bool showSceneSelectionCheckBox = true)
        {
            SelectedAvatar = (GameObject)EditorGUILayout.ObjectField(Strings.Main.avatar, SelectedAvatar, typeof(GameObject), true);

            GameObject newAvatar;
            if(SettingsContainer._useSceneSelectionAvatar && Selection.activeObject != SelectedAvatar && GetAvatarFromSceneSelection(true, out newAvatar))
                SelectedAvatar = newAvatar;

            if(showSelectFromSceneButton && GUILayout.Button(Strings.Buttons.selectFromScene) && Selection.activeObject && GetAvatarFromSceneSelection(true, out newAvatar))
                SelectedAvatar = newAvatar;

            if(showSceneSelectionCheckBox)
            {
                EditorGUI.BeginChangeCheck();
                bool toggle = GUILayout.Toggle(SettingsContainer._useSceneSelectionAvatar, Strings.Main.useSceneSelection);
                if(EditorGUI.EndChangeCheck())
                {
                    SettingsContainer._useSceneSelectionAvatar = toggle;
                    if(toggle)
                    {
                        Selection.selectionChanged = () =>
                        {
                            if(GetAvatarFromSceneSelection(true, out newAvatar))
                                SelectedAvatar = newAvatar;
                        };
                    }
                    else
                    {
                        Selection.selectionChanged = null;
                    }
                }
            }
        }

        void DrawCopierMenuGUI()
        {
            if(Settings._copier_expand = GUILayout.Toggle(Settings._copier_expand, Strings.Main.copier, Styles.Foldout_title))
            {
                EditorGUILayout.Space();

                EditorGUI.BeginChangeCheck();
                {
                    CopierSelectedFrom = (GameObject)EditorGUILayout.ObjectField(Strings.Copier.copyFrom, CopierSelectedFrom, typeof(GameObject), true);

                    if(GUILayout.Button(Strings.Buttons.selectFromScene))
                        if(Selection.activeGameObject != null && GetAvatarFromSceneSelection(false, out GameObject avatar))
                            CopierSelectedFrom = avatar;

                    if(_copierArmatureScalesDontMatch == true)
                        EditorGUILayout.LabelField(Strings.Warning.armatureScalesDontMatch, Styles.HelpBox_OneLine);
                }
                if(EditorGUI.EndChangeCheck())
                {
                    _copierArmatureScalesDontMatch = null;
                }

                if(_copierArmatureScalesDontMatch == null)
                {
                    _copierArmatureScalesDontMatch = true;
                    Transform copyToArm = Helpers.GetAvatarArmature(CopierSelectedFrom);
                    Transform copyFromArm = Helpers.GetAvatarArmature(SelectedAvatar);

                    _copierArmatureScalesDontMatch = copyToArm && copyFromArm && copyToArm.localScale != copyFromArm.localScale;
                    if(_copierArmatureScalesDontMatch == true)
                        Log(Strings.Warning.armatureScalesDontMatch, LogType.Warning);
                }

                EditorGUILayout.Space();

                EditorGUI.BeginDisabledGroup(CopierSelectedFrom == null || SelectedAvatar == null);
                {
                    Helpers.DrawGUILine(1, false);

                    var toolbarContent = new GUIContent[]
                        { new GUIContent(Strings.Copier.showCommon), new GUIContent(Strings.Copier.showAll) };
                    Settings._copier_selectedTab =
                        (CopierTabs.Tab)GUILayout.Toolbar((int)Settings._copier_selectedTab, toolbarContent);

                    Helpers.DrawGUILine(1, false);

                    if(CopierTabs.ComponentIsInSelectedTab("vrcavatardescriptor", Settings._copier_selectedTab))
                    {
                        //AvatarDescriptor menu
                        Helpers.DrawDropdownWithToggle(ref Settings._copier_expand_avatarDescriptor,
                            ref Settings.bCopier_descriptor_copy, Strings.Copier.descriptor, Icons.Avatar);
                        if(Settings._copier_expand_avatarDescriptor)
                        {
                            EditorGUI.BeginDisabledGroup(!Settings.bCopier_descriptor_copy);
                            EditorGUILayout.Space();

                            using(var cHorizontalScope = new GUILayout.HorizontalScope())
                            {
                                GUILayout.Space(COPIER_SETTINGS_INDENT_SIZE); // horizontal indent size

                                using(var cVerticalScope = new GUILayout.VerticalScope())
                                {
#if VRC_SDK_VRCSDK3
                                    Settings.bCopier_descriptor_copySettings = GUILayout.Toggle(
                                        Settings.bCopier_descriptor_copySettings, Strings.Copier.copySettings,
                                        Styles.CopierToggle);
                                    Settings.bCopier_descriptor_copyViewpoint = GUILayout.Toggle(
                                        Settings.bCopier_descriptor_copyViewpoint,
                                        Strings.Copier.descriptor_copyViewpoint, Styles.CopierToggle);
#endif
                                    Settings.bCopier_descriptor_copyAvatarScale =
                                        GUILayout.Toggle(Settings.bCopier_descriptor_copyAvatarScale,
                                            Strings.Copier.transforms_avatarScale, Styles.CopierToggle);

#if VRC_SDK_VRCSDK3
                                    EditorGUILayout.Space();

                                    Settings.bCopier_descriptor_copyPlayableLayers =
                                        GUILayout.Toggle(Settings.bCopier_descriptor_copyPlayableLayers,
                                            Strings.Copier.descriptor_playableLayers, Styles.CopierToggle);
                                    Settings.bCopier_descriptor_copyEyeLookSettings =
                                        GUILayout.Toggle(Settings.bCopier_descriptor_copyEyeLookSettings,
                                            Strings.Copier.descriptor_eyeLookSettings, Styles.CopierToggle);
                                    Settings.bCopier_descriptor_copyExpressions =
                                        GUILayout.Toggle(Settings.bCopier_descriptor_copyExpressions,
                                            Strings.Copier.descriptor_expressions, Styles.CopierToggle);
                                    Settings.bCopier_descriptor_copyColliders = GUILayout.Toggle(
                                        Settings.bCopier_descriptor_copyColliders, Strings.Copier.descriptor_colliders,
                                        Styles.CopierToggle);
#endif
                                    EditorGUILayout.Space();

#if VRC_SDK_VRCSDK3
                                    Settings.bCopier_descriptor_copyPipelineId = GUILayout.Toggle(
                                        Settings.bCopier_descriptor_copyPipelineId,
                                        Strings.Copier.descriptor_pipelineId, Styles.CopierToggle);
#endif
                                }
                            }

                            EditorGUILayout.Space();
                            EditorGUI.EndDisabledGroup();
                        }

                        Helpers.DrawGUILine(1, false);
                    }

                    if(CopierTabs.ComponentIsInSelectedTab("prefab", Settings._copier_selectedTab))
                    {
                        //Prefabs menu
                        Helpers.DrawDropdownWithToggle(ref Settings._copier_expand_prefab,
                            ref Settings.bCopier_prefabs_copy, Strings.Copier.prefabs, Icons.Prefab);
                        if(Settings._copier_expand_prefab)
                        {

                            EditorGUI.BeginDisabledGroup(!Settings.bCopier_prefabs_copy);
                            EditorGUILayout.Space();

                            using(var cHorizontalScope = new GUILayout.HorizontalScope())
                            {
                                GUILayout.Space(COPIER_SETTINGS_INDENT_SIZE); // horizontal indent size

                                using(var cVerticalScope = new GUILayout.VerticalScope())
                                {
                                    Settings.bCopier_prefabs_adjustScale = GUILayout.Toggle(
                                        Settings.bCopier_prefabs_adjustScale, Strings.Copier.adjustScale,
                                        Styles.CopierToggle);
                                    Settings.bCopier_prefabs_fixReferences = GUILayout.Toggle(
                                        Settings.bCopier_prefabs_fixReferences, Strings.Copier.fixReferences,
                                        Styles.CopierToggle);
                                    Settings.bCopier_prefabs_copyPropertyOverrides =
                                        GUILayout.Toggle(Settings.bCopier_prefabs_copyPropertyOverrides,
                                            Strings.Copier.prefabs_copyPropertyOverrides, Styles.CopierToggle);
                                    Settings.bCopier_prefabs_ignorePrefabByOtherCopiers =
                                        GUILayout.Toggle(Settings.bCopier_prefabs_ignorePrefabByOtherCopiers,
                                            Strings.Copier.prefabs_ignorePrefabByOtherCopiers, Styles.CopierToggle);
                                    Settings.bCopier_prefabs_createObjects = GUILayout.Toggle(
                                        Settings.bCopier_prefabs_createObjects, Strings.Copier.copyGameObjects,
                                        Styles.CopierToggle);
                                }
                            }

                            EditorGUILayout.Space();
                            EditorGUI.EndDisabledGroup();
                        }

                        Helpers.DrawGUILine(1, false);
                    }

                    if(PhysBonesExist)
                    {
                        if(CopierTabs.ComponentIsInSelectedTab("physbone", Settings._copier_selectedTab))
                        {
                            //PhysBones menu
                            Helpers.DrawDropdownWithToggle(ref Settings._copier_expand_physBones, ref Settings.bCopier_physBones_copy, Strings.Copier.physBones, Icons.PhysBone);
                            if(Settings._copier_expand_physBones)
                            {

                                EditorGUI.BeginDisabledGroup(!Settings.bCopier_physBones_copy);
                                EditorGUILayout.Space();

                                using(var cHorizontalScope = new GUILayout.HorizontalScope())
                                {
                                    GUILayout.Space(COPIER_SETTINGS_INDENT_SIZE); // horizontal indent size

                                    using(var cVerticalScope = new GUILayout.VerticalScope())
                                    {
                                        Settings.bCopier_physBones_createObjects = GUILayout.Toggle(
                                            Settings.bCopier_physBones_createObjects, Strings.Copier.copyGameObjects,
                                            Styles.CopierToggle);
                                        Settings.bCopier_physBones_removeOldBones = GUILayout.Toggle(
                                            Settings.bCopier_physBones_removeOldBones,
                                            Strings.Copier.physBones_removeOldBones, Styles.CopierToggle);
                                        Settings.bCopier_physBones_adjustScale = GUILayout.Toggle(
                                            Settings.bCopier_physBones_adjustScale, Strings.Copier.adjustScale,
                                            Styles.CopierToggle);
                                    }
                                }

                                EditorGUILayout.Space();
                                EditorGUI.EndDisabledGroup();
                            }

                            Helpers.DrawGUILine(1, false);
                        }

                        if(CopierTabs.ComponentIsInSelectedTab("physbonecollider", Settings._copier_selectedTab))
                        {
                            //Phys Bone Colliders menu
                            Helpers.DrawDropdownWithToggle(ref Settings._copier_expand_physBoneColliders, ref Settings.bCopier_physBones_copyColliders, Strings.Copier.physBones_colliders, Icons.PhysBoneCollider);

                            if(Settings._copier_expand_physBoneColliders)
                            {
                                EditorGUI.BeginDisabledGroup(!Settings.bCopier_physBones_copyColliders);
                                EditorGUILayout.Space();

                                using(var cHorizontalScope = new GUILayout.HorizontalScope())
                                {
                                    GUILayout.Space(COPIER_SETTINGS_INDENT_SIZE); // horizontal indent size

                                    using(var cVerticalScope = new GUILayout.VerticalScope())
                                    {
                                        Settings.bCopier_physBones_removeOldColliders =
                                            GUILayout.Toggle(Settings.bCopier_physBones_removeOldColliders,
                                                Strings.Copier.physBones_removeOldColliders, Styles.CopierToggle);
                                        Settings.bCopier_physBones_createObjectsColliders =
                                            GUILayout.Toggle(Settings.bCopier_physBones_createObjectsColliders,
                                                Strings.Copier.copyGameObjects, Styles.CopierToggle);
                                        Settings.bCopier_physBones_adjustScaleColliders =
                                            GUILayout.Toggle(Settings.bCopier_physBones_adjustScaleColliders,
                                                Strings.Copier.adjustScale, Styles.CopierToggle);
                                    }
                                }

                                EditorGUILayout.Space();
                                EditorGUI.EndDisabledGroup();
                            }

                            Helpers.DrawGUILine(1, false);
                        }
                    }

                    if(DynamicBonesExist)
                    {
                        if(CopierTabs.ComponentIsInSelectedTab("dynamicbone", Settings._copier_selectedTab))
                        {
                            //DynamicBones menu
                            Helpers.DrawDropdownWithToggle(ref Settings._copier_expand_dynamicBones, ref Settings.bCopier_dynamicBones_copy, Strings.Copier.dynamicBones, Icons.BoneIcon);

                            if(Settings._copier_expand_dynamicBones)
                            {
                                EditorGUI.BeginDisabledGroup(!Settings.bCopier_dynamicBones_copy);
                                EditorGUILayout.Space();

                                using(var cHorizontalScope = new GUILayout.HorizontalScope())
                                {
                                    GUILayout.Space(COPIER_SETTINGS_INDENT_SIZE); // horizontal indent size

                                    using(var cVerticalScope = new GUILayout.VerticalScope())
                                    {
                                        Settings.bCopier_dynamicBones_removeOldBones = GUILayout.Toggle(Settings.bCopier_dynamicBones_removeOldBones, Strings.Copier.dynamicBones_removeOldBones, Styles.CopierToggle);
                                        Settings.bCopier_dynamicBones_createObjects = GUILayout.Toggle(Settings.bCopier_dynamicBones_createObjects, Strings.Copier.copyGameObjects, Styles.CopierToggle);
                                        Settings.bCopier_dynamicBones_adjustScale = GUILayout.Toggle(Settings.bCopier_dynamicBones_adjustScale, Strings.Copier.adjustScale, Styles.CopierToggle);
                                    }
                                }

                                EditorGUILayout.Space();
                                EditorGUI.EndDisabledGroup();
                            }

                            Helpers.DrawGUILine(1, false);
                        }

                        if(CopierTabs.ComponentIsInSelectedTab("dynamicbonecollider", Settings._copier_selectedTab))
                        {
                            //Dynamic Bone Colliders menu
                            Helpers.DrawDropdownWithToggle(ref Settings._copier_expand_dynamicBoneColliders,
                                ref Settings.bCopier_dynamicBones_copyColliders, Strings.Copier.dynamicBones_colliders,
                                Icons.BoneColliderIcon);
                            if(Settings._copier_expand_dynamicBoneColliders)
                            {
                                EditorGUI.BeginDisabledGroup(!Settings.bCopier_dynamicBones_copyColliders);
                                EditorGUILayout.Space();

                                using(var cHorizontalScope = new GUILayout.HorizontalScope())
                                {
                                    GUILayout.Space(COPIER_SETTINGS_INDENT_SIZE); // horizontal indent size

                                    using(var cVerticalScope = new GUILayout.VerticalScope())
                                    {
                                        Settings.bCopier_dynamicBones_removeOldColliders =
                                            GUILayout.Toggle(Settings.bCopier_dynamicBones_removeOldColliders,
                                                Strings.Copier.dynamicBones_removeOldColliders,
                                                Styles.CopierToggle);
                                        Settings.bCopier_dynamicBones_createObjectsColliders =
                                            GUILayout.Toggle(Settings.bCopier_dynamicBones_createObjectsColliders,
                                                Strings.Copier.copyGameObjects, Styles.CopierToggle);
                                        Settings.bCopier_dynamicBones_adjustScaleColliders =
                                            GUILayout.Toggle(Settings.bCopier_dynamicBones_adjustScaleColliders,
                                                Strings.Copier.adjustScale, Styles.CopierToggle);
                                    }
                                }

                                EditorGUILayout.Space();
                                EditorGUI.EndDisabledGroup();
                            }

                            Helpers.DrawGUILine(1, false);
                        }
                    }

                    if(CopierTabs.ComponentIsInSelectedTab<SkinnedMeshRenderer>(Settings._copier_selectedTab))
                    {
                        //SkinnedMeshRenderer menu
                        Helpers.DrawDropdownWithToggle(ref Settings._copier_expand_skinnedMeshRenderer,
                            ref Settings.bCopier_skinMeshRender_copy, Strings.Copier.skinMeshRender,
                            Icons.SkinnedMeshRenderer);
                        if(Settings._copier_expand_skinnedMeshRenderer)
                        {
                            EditorGUI.BeginDisabledGroup(!Settings.bCopier_skinMeshRender_copy);
                            EditorGUILayout.Space();

                            using(var cHorizontalScope = new GUILayout.HorizontalScope())
                            {
                                GUILayout.Space(COPIER_SETTINGS_INDENT_SIZE); // horizontal indent size

                                using(var cVerticalScope = new GUILayout.VerticalScope())
                                {
                                    Settings.bCopier_skinMeshRender_copySettings =
                                        GUILayout.Toggle(Settings.bCopier_skinMeshRender_copySettings,
                                            Strings.Copier.copySettings, Styles.CopierToggle);
                                    Settings.bCopier_skinMeshRender_copyMaterials =
                                        GUILayout.Toggle(Settings.bCopier_skinMeshRender_copyMaterials,
                                            Strings.Copier.skinMeshRender_materials, Styles.CopierToggle);
                                    Settings.bCopier_skinMeshRender_copyBlendShapeValues =
                                        GUILayout.Toggle(Settings.bCopier_skinMeshRender_copyBlendShapeValues,
                                            Strings.Copier.skinMeshRender_blendShapeValues, Styles.CopierToggle);
                                    Settings.bCopier_skinMeshRender_copyBounds =
                                        GUILayout.Toggle(Settings.bCopier_skinMeshRender_copyBounds,
                                            Strings.Copier.skinMeshRender_bounds, Styles.CopierToggle);
                                    Settings.bCopier_skinMeshRender_createObjects =
                                        GUILayout.Toggle(Settings.bCopier_skinMeshRender_createObjects,
                                            Strings.Copier.copyGameObjects, Styles.CopierToggle);
                                }
                            }

                            EditorGUILayout.Space();
                            EditorGUI.EndDisabledGroup();
                        }

                        Helpers.DrawGUILine(1, false);
                    }

                    if(CopierTabs.ComponentIsInSelectedTab<MeshRenderer>(Settings._copier_selectedTab))
                    {
                        //MeshRenderers menu
                        Helpers.DrawDropdownWithToggle(ref Settings._copier_expand_meshRenderers, ref Settings.bCopier_meshRenderers_copy, Strings.Copier.meshRenderers, Icons.MeshRenderer);
                        if(Settings._copier_expand_meshRenderers)
                        {
                            EditorGUI.BeginDisabledGroup(!Settings.bCopier_meshRenderers_copy);
                            EditorGUILayout.Space();

                            using(var cHorizontalScope = new GUILayout.HorizontalScope())
                            {
                                GUILayout.Space(COPIER_SETTINGS_INDENT_SIZE); // horizontal indent size

                                using(var cVerticalScope = new GUILayout.VerticalScope())
                                {
                                    Settings.bCopier_meshRenderers_copySettings = GUILayout.Toggle(Settings.bCopier_meshRenderers_copySettings, Strings.Copier.copySettings, Styles.CopierToggle);
                                    Settings.bCopier_meshRenderers_createMissing = GUILayout.Toggle(Settings.bCopier_meshRenderers_createMissing, Strings.Copier.createMissing, Styles.CopierToggle);
                                    Settings.bCopier_meshRenderers_createObjects = GUILayout.Toggle(Settings.bCopier_meshRenderers_createObjects, Strings.Copier.copyGameObjects, Styles.CopierToggle);
                                }
                            }

                            EditorGUILayout.Space();
                            EditorGUI.EndDisabledGroup();
                        }

                        Helpers.DrawGUILine(1, false);
                    }

                    if(PhysBonesExist)
                    {
                        if(CopierTabs.ComponentIsInSelectedTab("contactreceiver", Settings._copier_selectedTab))
                        {
                            //Contact Receivers menu
                            Helpers.DrawDropdownWithToggle(ref Settings._copier_expand_contactReceiver, ref Settings.bCopier_contactReceiver_copy, Strings.Copier.contactReceiver, Icons.ContactReceiver);
                            if(Settings._copier_expand_contactReceiver)
                            {
                                EditorGUI.BeginDisabledGroup(!Settings.bCopier_contactReceiver_copy);
                                EditorGUILayout.Space();

                                using (var cHorizontalScope = new GUILayout.HorizontalScope())
                                {
                                    GUILayout.Space(COPIER_SETTINGS_INDENT_SIZE); // horizontal indent size

                                    using (var cVerticalScope = new GUILayout.VerticalScope())
                                    {
                                        Settings.bCopier_contactReceiver_createObjects = GUILayout.Toggle(Settings.bCopier_contactReceiver_createObjects, Strings.Copier.copyGameObjects, Styles.CopierToggle);
                                        Settings.bCopier_contactReceiver_removeOld = GUILayout.Toggle(Settings.bCopier_contactReceiver_removeOld, Strings.Copier.contactReceiver_removeOld, Styles.CopierToggle);
                                        Settings.bCopier_contactReceiver_adjustScale = GUILayout.Toggle(Settings.bCopier_contactReceiver_adjustScale, Strings.Copier.adjustScale, Styles.CopierToggle);
                                    }
                                }

                                EditorGUILayout.Space();
                                EditorGUI.EndDisabledGroup();
                            }

                            Helpers.DrawGUILine(1, false);
                        }

                        if(CopierTabs.ComponentIsInSelectedTab("contactsender", Settings._copier_selectedTab))
                        {
                            //Contact Senders menu
                            Helpers.DrawDropdownWithToggle(ref Settings._copier_expand_contactSender, ref Settings.bCopier_contactSender_copy, Strings.Copier.contactSender, Icons.ContactSender);
                            if(Settings._copier_expand_contactSender)
                            {

                                EditorGUI.BeginDisabledGroup(!Settings.bCopier_contactSender_copy);
                                EditorGUILayout.Space();

                                using (var cHorizontalScope = new GUILayout.HorizontalScope())
                                {
                                    GUILayout.Space(COPIER_SETTINGS_INDENT_SIZE); // horizontal indent size

                                    using (var cVerticalScope = new GUILayout.VerticalScope())
                                    {
                                        Settings.bCopier_contactSender_createObjects = GUILayout.Toggle(Settings.bCopier_contactSender_createObjects, Strings.Copier.copyGameObjects, Styles.CopierToggle);
                                        Settings.bCopier_contactSender_removeOld = GUILayout.Toggle(Settings.bCopier_contactSender_removeOld, Strings.Copier.contactSender_removeOld, Styles.CopierToggle);
                                        Settings.bCopier_contactSender_adjustScale = GUILayout.Toggle(Settings.bCopier_contactSender_adjustScale, Strings.Copier.adjustScale, Styles.CopierToggle);
                                    }
                                }

                                EditorGUILayout.Space();
                                EditorGUI.EndDisabledGroup();
                            }

                            Helpers.DrawGUILine(1, false);
                        }
                    }

                    if(CopierTabs.ComponentIsInSelectedTab<ParticleSystem>(Settings._copier_selectedTab))
                    {
                        //Particles menu
                        Helpers.DrawDropdownWithToggle(ref Settings._copier_expand_particleSystems, ref Settings.bCopier_particleSystems_copy, Strings.Copier.particleSystems, Icons.ParticleSystem);
                        if(Settings._copier_expand_particleSystems)
                        {
                            EditorGUI.BeginDisabledGroup(!Settings.bCopier_particleSystems_copy);
                            EditorGUILayout.Space();

                            using(var cHorizontalScope = new GUILayout.HorizontalScope())
                            {
                                GUILayout.Space(COPIER_SETTINGS_INDENT_SIZE); // horizontal indent size

                                using(var cVerticalScope = new GUILayout.VerticalScope())
                                {
                                    Settings.bCopier_particleSystems_replace = GUILayout.Toggle(Settings.bCopier_particleSystems_replace, Strings.Copier.replaceOld, Styles.CopierToggle);
                                    Settings.bCopier_particleSystems_createObjects = GUILayout.Toggle(Settings.bCopier_particleSystems_createObjects, Strings.Copier.copyGameObjects, Styles.CopierToggle);
                                }
                            }

                            EditorGUILayout.Space();
                            EditorGUI.EndDisabledGroup();
                        }
                        Helpers.DrawGUILine(1, false);
                    }

                    if(CopierTabs.ComponentIsInSelectedTab<TrailRenderer>(Settings._copier_selectedTab))
                    {
                        //TrailRenderers menu
                        Helpers.DrawDropdownWithToggle(ref Settings._copier_expand_trailRenderers, ref Settings.bCopier_trailRenderers_copy, Strings.Copier.trailRenderers, Icons.TrailRenderer);
                        if(Settings._copier_expand_trailRenderers)
                        {
                            EditorGUI.BeginDisabledGroup(!Settings.bCopier_trailRenderers_copy);
                            EditorGUILayout.Space();

                            using(var cHorizontalScope = new GUILayout.HorizontalScope())
                            {
                                GUILayout.Space(COPIER_SETTINGS_INDENT_SIZE); // horizontal indent size

                                using(var cVerticalScope = new GUILayout.VerticalScope())
                                {
                                    Settings.bCopier_trailRenderers_copySettings = GUILayout.Toggle(Settings.bCopier_trailRenderers_copySettings, Strings.Copier.copySettings, Styles.CopierToggle);
                                    Settings.bCopier_trailRenderers_createMissing = GUILayout.Toggle(Settings.bCopier_trailRenderers_createMissing, Strings.Copier.createMissing, Styles.CopierToggle);
                                    Settings.bCopier_trailRenderers_createObjects = GUILayout.Toggle(Settings.bCopier_trailRenderers_createObjects, Strings.Copier.copyGameObjects, Styles.CopierToggle);
                                }
                            }

                            EditorGUILayout.Space();
                            EditorGUI.EndDisabledGroup();
                        }
                        Helpers.DrawGUILine(1, false);
                    }

                    if(CopierTabs.ComponentIsInSelectedTab<AudioSource>(Settings._copier_selectedTab))
                    {
                        //AudioSources menu
                        Helpers.DrawDropdownWithToggle(ref Settings._copier_expand_audioSources, ref Settings.bCopier_audioSources_copy, Strings.Copier.audioSources, Icons.AudioSource);
                        if(Settings._copier_expand_audioSources)
                        {
                            EditorGUI.BeginDisabledGroup(!Settings.bCopier_audioSources_copy);
                            EditorGUILayout.Space();

                            using(var cHorizontalScope = new GUILayout.HorizontalScope())
                            {
                                GUILayout.Space(COPIER_SETTINGS_INDENT_SIZE); // horizontal indent size

                                using(var cVerticalScope = new GUILayout.VerticalScope())
                                {
                                    Settings.bCopier_audioSources_copySettings = GUILayout.Toggle(Settings.bCopier_audioSources_copySettings, Strings.Copier.copySettings, Styles.CopierToggle);
                                    Settings.bCopier_audioSources_createMissing = GUILayout.Toggle(Settings.bCopier_audioSources_createMissing, Strings.Copier.createMissing, Styles.CopierToggle);
                                    Settings.bCopier_audioSources_createObjects = GUILayout.Toggle(Settings.bCopier_audioSources_createObjects, Strings.Copier.copyGameObjects, Styles.CopierToggle);
                                }
                            }

                            EditorGUILayout.Space();
                            EditorGUI.EndDisabledGroup();
                        }
                        Helpers.DrawGUILine(1, false);
                    }

                    if(CopierTabs.ComponentIsInSelectedTab<Light>(Settings._copier_selectedTab))
                    {
                        //Lights menu
                        Helpers.DrawDropdownWithToggle(ref Settings._copier_expand_lights, ref Settings.bCopier_lights_copy, Strings.Copier.lights, Icons.Light);
                        if(Settings._copier_expand_lights)
                        {
                            EditorGUI.BeginDisabledGroup(!Settings.bCopier_lights_copy);
                            EditorGUILayout.Space();
                            using(var cHorizontalScope = new GUILayout.HorizontalScope())
                            {
                                GUILayout.Space(COPIER_SETTINGS_INDENT_SIZE); // horizontal indent size

                                using(var cVerticalScope = new GUILayout.VerticalScope())
                                {
                                    Settings.bCopier_lights_copySettings = GUILayout.Toggle(Settings.bCopier_lights_copySettings, Strings.Copier.copySettings, Styles.CopierToggle);
                                    Settings.bCopier_lights_createMissing = GUILayout.Toggle(Settings.bCopier_lights_createMissing, Strings.Copier.createMissing, Styles.CopierToggle);
                                    Settings.bCopier_lights_createObjects = GUILayout.Toggle(Settings.bCopier_lights_createObjects, Strings.Copier.copyGameObjects, Styles.CopierToggle);
                                }
                            }
                            EditorGUILayout.Space();
                            EditorGUI.EndDisabledGroup();
                        }

                        Helpers.DrawGUILine(1, false);
                    }

                    if(CopierTabs.ComponentIsInSelectedTab<Rigidbody>(Settings._copier_selectedTab))
                    {
                        //RidigBodies menu
                        Helpers.DrawDropdownWithToggle(ref Settings._copier_expand_rigidBodies, ref Settings.bCopier_rigidBodies_copy, Strings.Copier.rigidBodies, Icons.RigidBody);
                        if(Settings._copier_expand_rigidBodies)
                        {
                            EditorGUI.BeginDisabledGroup(!Settings.bCopier_rigidBodies_copy);
                            EditorGUILayout.Space();

                            using(var cHorizontalScope = new GUILayout.HorizontalScope())
                            {
                                GUILayout.Space(COPIER_SETTINGS_INDENT_SIZE); // horizontal indent size

                                using(var cVerticalScope = new GUILayout.VerticalScope())
                                {
                                    Settings.bCopier_rigidBodies_copySettings = GUILayout.Toggle(Settings.bCopier_rigidBodies_copySettings, Strings.Copier.copySettings, Styles.CopierToggle);
                                    Settings.bCopier_rigidBodies_createMissing = GUILayout.Toggle(Settings.bCopier_rigidBodies_createMissing, Strings.Copier.createMissing, Styles.CopierToggle);
                                    Settings.bCopier_rigidBodies_createObjects = GUILayout.Toggle(Settings.bCopier_rigidBodies_createObjects, Strings.Copier.copyGameObjects, Styles.CopierToggle);
                                }
                            }

                            EditorGUILayout.Space();
                            EditorGUI.EndDisabledGroup();
                        }
                        Helpers.DrawGUILine(1, false);
                    }

                    if(CopierTabs.ComponentIsInSelectedTab<Joint>(Settings._copier_selectedTab))
                    {
                        //Joints menu
                        Helpers.DrawDropdownWithToggle(ref Settings._copier_expand_joints, ref Settings.bCopier_joints_copy, Strings.Copier.joints, Icons.Joint);
                        if(Settings._copier_expand_joints)
                        {
                            EditorGUI.BeginDisabledGroup(!Settings.bCopier_joints_copy);
                            EditorGUILayout.Space();

                            using(var cHorizontalScope = new GUILayout.HorizontalScope())
                            {
                                GUILayout.Space(COPIER_SETTINGS_INDENT_SIZE); // horizontal indent size

                                using(var cVerticalScope = new GUILayout.VerticalScope())
                                {
                                    Settings.bCopier_joints_fixed = GUILayout.Toggle(Settings.bCopier_joints_fixed, Strings.Copier.joints_fixed, Styles.CopierToggle);
                                    Settings.bCopier_joints_hinge = GUILayout.Toggle(Settings.bCopier_joints_hinge, Strings.Copier.joints_hinge, Styles.CopierToggle);
                                    Settings.bCopier_joints_spring = GUILayout.Toggle(Settings.bCopier_joints_spring, Strings.Copier.joints_spring, Styles.CopierToggle);
                                    Settings.bCopier_joints_character = GUILayout.Toggle(Settings.bCopier_joints_character, Strings.Copier.joints_character, Styles.CopierToggle);
                                    Settings.bCopier_joints_configurable = GUILayout.Toggle(Settings.bCopier_joints_configurable, Strings.Copier.joints_configurable, Styles.CopierToggle);

                                    EditorGUILayout.Space();

                                    Settings.bCopier_joints_removeOld = GUILayout.Toggle(Settings.bCopier_joints_removeOld, Strings.Copier.joints_removeOld, Styles.CopierToggle);
                                    Settings.bCopier_joints_createObjects = GUILayout.Toggle(Settings.bCopier_joints_createObjects, Strings.Copier.copyGameObjects, Styles.CopierToggle);
                                }
                            }

                            EditorGUILayout.Space();
                            EditorGUI.EndDisabledGroup();
                        }

                        Helpers.DrawGUILine(1, false);
                    }

                    if(CopierTabs.ComponentIsInSelectedTab<Collider>(Settings._copier_selectedTab))
                    {
                        //Colliders menu
                        Helpers.DrawDropdownWithToggle(ref Settings._copier_expand_colliders, ref Settings.bCopier_colliders_copy, Strings.Copier.colliders, Icons.ColliderBox);
                        if(Settings._copier_expand_colliders)
                        {
                            EditorGUI.BeginDisabledGroup(!Settings.bCopier_colliders_copy);
                            EditorGUILayout.Space();

                            using(var cHorizontalScope = new GUILayout.HorizontalScope())
                            {
                                GUILayout.Space(COPIER_SETTINGS_INDENT_SIZE); // horizontal indent size

                                using(var cVerticalScope = new GUILayout.VerticalScope())
                                {
                                    Settings.bCopier_colliders_copyBox = GUILayout.Toggle(Settings.bCopier_colliders_copyBox, Strings.Copier.colliders_box, Styles.CopierToggle);
                                    Settings.bCopier_colliders_copyCapsule = GUILayout.Toggle(Settings.bCopier_colliders_copyCapsule, Strings.Copier.colliders_capsule, Styles.CopierToggle);
                                    Settings.bCopier_colliders_copySphere = GUILayout.Toggle(Settings.bCopier_colliders_copySphere, Strings.Copier.colliders_sphere, Styles.CopierToggle);
                                    Settings.bCopier_colliders_copyMesh = GUILayout.Toggle(Settings.bCopier_colliders_copyMesh, Strings.Copier.colliders_mesh, Styles.CopierToggle);

                                    EditorGUILayout.Space();

                                    Settings.bCopier_colliders_adjustScale = GUILayout.Toggle(Settings.bCopier_colliders_adjustScale, Strings.Copier.adjustScale, Styles.CopierToggle);
                                    Settings.bCopier_colliders_removeOld = GUILayout.Toggle(Settings.bCopier_colliders_removeOld, Strings.Copier.colliders_removeOld, Styles.CopierToggle);
                                    Settings.bCopier_colliders_createObjects = GUILayout.Toggle(Settings.bCopier_colliders_createObjects, Strings.Copier.copyGameObjects, Styles.CopierToggle);
                                }
                            }

                            EditorGUILayout.Space();
                            EditorGUI.EndDisabledGroup();
                        }

                        Helpers.DrawGUILine(1, false);
                    }

                    if(CopierTabs.ComponentIsInSelectedTab<Animator>(Settings._copier_selectedTab))
                    {
                        //Animators menu
                        Helpers.DrawDropdownWithToggle(ref Settings._copier_expand_animators, ref Settings.bCopier_animators_copy, Strings.Copier.animators, Icons.Animator);
                        if(Settings._copier_expand_animators)
                        {
                            EditorGUI.BeginDisabledGroup(!Settings.bCopier_animators_copy);
                            EditorGUILayout.Space();

                            using(var cHorizontalScope = new GUILayout.HorizontalScope())
                            {
                                GUILayout.Space(COPIER_SETTINGS_INDENT_SIZE); // horizontal indent size

                                using(var cVerticalScope = new GUILayout.VerticalScope())
                                {
                                    Settings.bCopier_animators_copySettings = GUILayout.Toggle(Settings.bCopier_animators_copySettings, Strings.Copier.copySettings, Styles.CopierToggle);
                                    Settings.bCopier_animators_createMissing = GUILayout.Toggle(Settings.bCopier_animators_createMissing, Strings.Copier.createMissing, Styles.CopierToggle);
                                    Settings.bCopier_animators_createObjects = GUILayout.Toggle(Settings.bCopier_animators_createObjects, Strings.Copier.copyGameObjects, Styles.CopierToggle);
                                    Settings.bCopier_animators_copyMainAnimator = GUILayout.Toggle(Settings.bCopier_animators_copyMainAnimator, Strings.Copier.copyMainAnimator, Styles.CopierToggle);
                                }
                            }

                            EditorGUILayout.Space();
                            EditorGUI.EndDisabledGroup();
                        }

                        Helpers.DrawGUILine(1, false);
                    }

                    if(CopierTabs.ComponentIsInSelectedTab<Transform>(Settings._copier_selectedTab))
                    {
                        //Transforms menu
                        Helpers.DrawDropdownWithToggle(ref Settings._copier_expand_transforms, ref Settings.bCopier_transforms_copy, Strings.Copier.transforms, Icons.Transform);
                        if(Settings._copier_expand_transforms)
                        {
                            EditorGUI.BeginDisabledGroup(!Settings.bCopier_transforms_copy);
                            EditorGUILayout.Space();

                            using(var cHorizontalScope = new GUILayout.HorizontalScope())
                            {
                                GUILayout.Space(COPIER_SETTINGS_INDENT_SIZE); // horizontal indent size

                                using(var cVerticalScope = new GUILayout.VerticalScope())
                                {
                                    Settings.bCopier_transforms_copyPosition = GUILayout.Toggle(Settings.bCopier_transforms_copyPosition, Strings.Copier.transforms_position, Styles.CopierToggle);
                                    Settings.bCopier_transforms_copyRotation = GUILayout.Toggle(Settings.bCopier_transforms_copyRotation, Strings.Copier.transforms_rotation, Styles.CopierToggle);
                                    Settings.bCopier_transforms_copyScale = GUILayout.Toggle(Settings.bCopier_transforms_copyScale, Strings.Copier.transforms_scale, Styles.CopierToggle);
                                    Settings.bCopier_transforms_createMissing = GUILayout.Toggle(Settings.bCopier_transforms_createMissing, Strings.Copier.transforms_createMissing, Styles.CopierToggle);
                                    EditorGUILayout.Space();
                                    Settings.bCopier_transforms_copyActiveState = GUILayout.Toggle(Settings.bCopier_transforms_copyActiveState, Strings.Copier.transforms_copyActiveState, Styles.CopierToggle);
                                    Settings.bCopier_transforms_copyLayerAndTag = GUILayout.Toggle(Settings.bCopier_transforms_copyLayerAndTag, Strings.Copier.transforms_copyLayerAndTag, Styles.CopierToggle);
                                }
                            }

                            EditorGUILayout.Space();
                            EditorGUI.EndDisabledGroup();
                        }

                        Helpers.DrawGUILine(1, false);
                    }

                    if(CopierTabs.ComponentIsInSelectedTab<AimConstraint>(Settings._copier_selectedTab))
                    {
                        //Aim Constraints menu
                        Helpers.DrawDropdownWithToggle(ref Settings._copier_expand_aimConstraints, ref Settings.bCopier_aimConstraint_copy, Strings.Copier.aimConstraints, Icons.AimConstraint);
                        if(Settings._copier_expand_aimConstraints)
                        {
                            EditorGUI.BeginDisabledGroup(!Settings.bCopier_aimConstraint_copy);
                            EditorGUILayout.Space();

                            using(var cHorizontalScope = new GUILayout.HorizontalScope())
                            {
                                GUILayout.Space(COPIER_SETTINGS_INDENT_SIZE); // horizontal indent size

                                using(var cVerticalScope = new GUILayout.VerticalScope())
                                {
                                    Settings.bCopier_aimConstraint_replaceOld = GUILayout.Toggle(Settings.bCopier_aimConstraint_replaceOld, Strings.Copier.replaceOld, Styles.CopierToggle);
                                    Settings.bCopier_aimConstraint_createObjects = GUILayout.Toggle(Settings.bCopier_aimConstraint_createObjects, Strings.Copier.copyGameObjects, Styles.CopierToggle);
                                    Settings.bCopier_aimConstraint_onlyIfHasValidSources = GUILayout.Toggle(Settings.bCopier_aimConstraint_onlyIfHasValidSources, Strings.Copier.onlyIfHasValidSources, Styles.CopierToggle);
                                }
                            }

                            EditorGUILayout.Space();
                            EditorGUI.EndDisabledGroup();
                        }

                        Helpers.DrawGUILine(1, false);
                    }

                    if(CopierTabs.ComponentIsInSelectedTab<LookAtConstraint>(Settings._copier_selectedTab))
                    {
                        //LookAt Constraints menu
                        Helpers.DrawDropdownWithToggle(ref Settings._copier_expand_lookAtConstraints, ref Settings.bCopier_lookAtConstraint_copy, Strings.Copier.lookAtConstraints, Icons.LookAtConstraint);
                        if(Settings._copier_expand_lookAtConstraints)
                        {
                            EditorGUI.BeginDisabledGroup(!Settings.bCopier_lookAtConstraint_copy);
                            EditorGUILayout.Space();

                            using(var cHorizontalScope = new GUILayout.HorizontalScope())
                            {
                                GUILayout.Space(COPIER_SETTINGS_INDENT_SIZE); // horizontal indent size

                                using(var cVerticalScope = new GUILayout.VerticalScope())
                                {
                                    Settings.bCopier_lookAtConstraint_replaceOld = GUILayout.Toggle(Settings.bCopier_lookAtConstraint_replaceOld, Strings.Copier.replaceOld, Styles.CopierToggle);
                                    Settings.bCopier_lookAtConstraint_createObjects = GUILayout.Toggle(Settings.bCopier_lookAtConstraint_createObjects, Strings.Copier.copyGameObjects, Styles.CopierToggle);
                                    Settings.bCopier_lookAtConstraint_onlyIfHasValidSources = GUILayout.Toggle(Settings.bCopier_lookAtConstraint_onlyIfHasValidSources, Strings.Copier.onlyIfHasValidSources, Styles.CopierToggle);
                                }
                            }

                            EditorGUILayout.Space();
                            EditorGUI.EndDisabledGroup();
                        }

                        Helpers.DrawGUILine(1, false);
                    }

                    if(CopierTabs.ComponentIsInSelectedTab<ParentConstraint>(Settings._copier_selectedTab))
                    {
                        //Parent Constraints menu
                        Helpers.DrawDropdownWithToggle(ref Settings._copier_expand_parentConstraints, ref Settings.bCopier_parentConstraint_copy, Strings.Copier.parentConstraints, Icons.ParentConstraint);
                        if(Settings._copier_expand_parentConstraints)
                        {
                            EditorGUI.BeginDisabledGroup(!Settings.bCopier_parentConstraint_copy);
                            EditorGUILayout.Space();

                            using(var cHorizontalScope = new GUILayout.HorizontalScope())
                            {
                                GUILayout.Space(COPIER_SETTINGS_INDENT_SIZE); // horizontal indent size

                                using(var cVerticalScope = new GUILayout.VerticalScope())
                                {
                                    Settings.bCopier_parentConstraint_replaceOld = GUILayout.Toggle(Settings.bCopier_parentConstraint_replaceOld, Strings.Copier.replaceOld, Styles.CopierToggle);
                                    Settings.bCopier_parentConstraint_createObjects = GUILayout.Toggle(Settings.bCopier_parentConstraint_createObjects, Strings.Copier.copyGameObjects, Styles.CopierToggle);
                                    Settings.bCopier_parentConstraint_onlyIfHasValidSources = GUILayout.Toggle(Settings.bCopier_parentConstraint_onlyIfHasValidSources, Strings.Copier.onlyIfHasValidSources, Styles.CopierToggle);
                                }
                            }

                            EditorGUILayout.Space();
                            EditorGUI.EndDisabledGroup();
                        }

                        Helpers.DrawGUILine(1, false);
                    }

                    if(CopierTabs.ComponentIsInSelectedTab<PositionConstraint>(Settings._copier_selectedTab))
                    {
                        //Position Constraints menu
                        Helpers.DrawDropdownWithToggle(ref Settings._copier_expand_positionConstraints, ref Settings.bCopier_positionConstraint_copy, Strings.Copier.positionConstraints, Icons.PositionConstraint);
                        if(Settings._copier_expand_positionConstraints)
                        {
                            EditorGUI.BeginDisabledGroup(!Settings.bCopier_positionConstraint_copy);
                            EditorGUILayout.Space();

                            using(var cHorizontalScope = new GUILayout.HorizontalScope())
                            {
                                GUILayout.Space(COPIER_SETTINGS_INDENT_SIZE); // horizontal indent size

                                using(var cVerticalScope = new GUILayout.VerticalScope())
                                {
                                    Settings.bCopier_positionConstraint_replaceOld = GUILayout.Toggle(Settings.bCopier_positionConstraint_replaceOld, Strings.Copier.replaceOld, Styles.CopierToggle);
                                    Settings.bCopier_positionConstraint_createObjects = GUILayout.Toggle(Settings.bCopier_positionConstraint_createObjects, Strings.Copier.copyGameObjects, Styles.CopierToggle);
                                    Settings.bCopier_positionConstraint_onlyIfHasValidSources = GUILayout.Toggle(Settings.bCopier_positionConstraint_onlyIfHasValidSources, Strings.Copier.onlyIfHasValidSources, Styles.CopierToggle);
                                }
                            }

                            EditorGUILayout.Space();
                            EditorGUI.EndDisabledGroup();
                        }

                        Helpers.DrawGUILine(1, false);
                    }

                    if(CopierTabs.ComponentIsInSelectedTab<RotationConstraint>(Settings._copier_selectedTab))
                    {
                        //Rotation Constraints menu
                        Helpers.DrawDropdownWithToggle(ref Settings._copier_expand_rotationConstraints, ref Settings.bCopier_rotationConstraint_copy, Strings.Copier.rotationConstraints, Icons.RotationConstraint);
                        if(Settings._copier_expand_rotationConstraints)
                        {
                            EditorGUI.BeginDisabledGroup(!Settings.bCopier_rotationConstraint_copy);
                            EditorGUILayout.Space();

                            using(var cHorizontalScope = new GUILayout.HorizontalScope())
                            {
                                GUILayout.Space(COPIER_SETTINGS_INDENT_SIZE); // horizontal indent size

                                using(var cVerticalScope = new GUILayout.VerticalScope())
                                {
                                    Settings.bCopier_rotationConstraint_replaceOld = GUILayout.Toggle(Settings.bCopier_rotationConstraint_replaceOld, Strings.Copier.replaceOld, Styles.CopierToggle);
                                    Settings.bCopier_rotationConstraint_createObjects = GUILayout.Toggle(Settings.bCopier_rotationConstraint_createObjects, Strings.Copier.copyGameObjects, Styles.CopierToggle);
                                    Settings.bCopier_rotationConstraint_onlyIfHasValidSources = GUILayout.Toggle(Settings.bCopier_rotationConstraint_onlyIfHasValidSources, Strings.Copier.onlyIfHasValidSources, Styles.CopierToggle);
                                }
                            }

                            EditorGUILayout.Space();
                            EditorGUI.EndDisabledGroup();
                        }

                        Helpers.DrawGUILine(1, false);
                    }

                    if(CopierTabs.ComponentIsInSelectedTab<ScaleConstraint>(Settings._copier_selectedTab))
                    {
                        //Scale Constraints menu
                        Helpers.DrawDropdownWithToggle(ref Settings._copier_expand_scaleConstraints, ref Settings.bCopier_scaleConstraint_copy, Strings.Copier.scaleConstraints, Icons.ScaleConstraint);
                        if(Settings._copier_expand_scaleConstraints)
                        {
                            EditorGUI.BeginDisabledGroup(!Settings.bCopier_scaleConstraint_copy);
                            EditorGUILayout.Space();

                            using(var cHorizontalScope = new GUILayout.HorizontalScope())
                            {
                                GUILayout.Space(COPIER_SETTINGS_INDENT_SIZE); // horizontal indent size

                                using(var cVerticalScope = new GUILayout.VerticalScope())
                                {
                                    Settings.bCopier_scaleConstraint_replaceOld = GUILayout.Toggle(Settings.bCopier_scaleConstraint_replaceOld, Strings.Copier.replaceOld, Styles.CopierToggle);
                                    Settings.bCopier_scaleConstraint_createObjects = GUILayout.Toggle(Settings.bCopier_scaleConstraint_createObjects, Strings.Copier.copyGameObjects, Styles.CopierToggle);
                                    Settings.bCopier_scaleConstraint_onlyIfHasValidSources = GUILayout.Toggle(Settings.bCopier_scaleConstraint_onlyIfHasValidSources, Strings.Copier.onlyIfHasValidSources, Styles.CopierToggle);
                                }
                            }

                            EditorGUILayout.Space();
                            EditorGUI.EndDisabledGroup();
                        }

                        Helpers.DrawGUILine(1, false);
                    }

                    if(CopierTabs.ComponentIsInSelectedTab<Camera>(Settings._copier_selectedTab))
                    {
                        //Camera menu
                        Helpers.DrawDropdownWithToggle(ref Settings._copier_expand_cameras, ref Settings.bCopier_cameras_copy, Strings.Copier.cameras, Icons.Camera);
                        if(Settings._copier_expand_cameras)
                        {
                            EditorGUI.BeginDisabledGroup(!Settings.bCopier_cameras_copy);
                            EditorGUILayout.Space();

                            using(var cHorizontalScope = new GUILayout.HorizontalScope())
                            {
                                GUILayout.Space(COPIER_SETTINGS_INDENT_SIZE); // horizontal indent size

                                using(var cVerticalScope = new GUILayout.VerticalScope())
                                {
                                    Settings.bCopier_cameras_createObjects = GUILayout.Toggle(Settings.bCopier_cameras_createObjects, Strings.Copier.copyGameObjects, Styles.CopierToggle);
                                }
                            }

                            EditorGUILayout.Space();
                            EditorGUI.EndDisabledGroup();
                        }
                        Helpers.DrawGUILine(1, false);
                    }

                    if(CopierTabs.ComponentIsInSelectedTab("finalik", Settings._copier_selectedTab))
                    {
						bool exists = FinalIKExists;
						//FinalIK menu
						if(exists)
						{
                        	Helpers.DrawDropdownWithToggle(ref Settings._copier_expand_finalIK, ref Settings.bCopier_finalIK_copy, Strings.Copier.finalIK, Icons.Avatar);
							if (Settings._copier_expand_finalIK)
							{
								EditorGUI.BeginDisabledGroup(!Settings.bCopier_finalIK_copy);
								EditorGUILayout.Space();

								using (var cHorizontalScope = new GUILayout.HorizontalScope())
								{
									GUILayout.Space(COPIER_SETTINGS_INDENT_SIZE); // horizontal indent size

									using (var cVerticalScope = new GUILayout.VerticalScope())
									{
										Settings.bCopier_finalIK_copyFabrik = GUILayout.Toggle(Settings.bCopier_finalIK_copyFabrik, Strings.Copier.finalIK_fabrIK, Styles.CopierToggle);
										Settings.bCopier_finalIK_copyAimIK = GUILayout.Toggle(Settings.bCopier_finalIK_copyAimIK, Strings.Copier.finalIK_aimIK, Styles.CopierToggle);
										Settings.bCopier_finalIK_copyCCDIK = GUILayout.Toggle(Settings.bCopier_finalIK_copyCCDIK, Strings.Copier.finalIK_ccdIK, Styles.CopierToggle);
										Settings.bCopier_finalIK_copyRotationLimits = GUILayout.Toggle(Settings.bCopier_finalIK_copyRotationLimits, Strings.Copier.finalIK_rotationLimits, Styles.CopierToggle);
										Settings.bCopier_finalIK_copyLimbIK = GUILayout.Toggle(Settings.bCopier_finalIK_copyLimbIK, Strings.Copier.finalIK_limbIK, Styles.CopierToggle);
										Settings.bCopier_finalIK_copyFBTBipedIK = GUILayout.Toggle(Settings.bCopier_finalIK_copyFBTBipedIK, Strings.Copier.finalIK_fbtBipedIK, Styles.CopierToggle);
										Settings.bCopier_finalIK_copyVRIK = GUILayout.Toggle(Settings.bCopier_finalIK_copyVRIK, Strings.Copier.finalIK_VRIK, Styles.CopierToggle);
										Settings.bCopier_finalIK_copyGrounders = GUILayout.Toggle(Settings.bCopier_finalIK_copyGrounders, Strings.Copier.finalIK_Grounders, Styles.CopierToggle);

										EditorGUILayout.Space();

										Settings.bCopier_finalIK_createObjects = GUILayout.Toggle(
											Settings.bCopier_finalIK_createObjects, Strings.Copier.copyGameObjects,
											Styles.CopierToggle);
									}
								}

								EditorGUILayout.Space();
								EditorGUI.EndDisabledGroup();
							}
						}
						else
						{
							EditorGUI.BeginDisabledGroup(true);
							Helpers.DrawDropdownWithToggle(ref exists, ref exists, Strings.Copier.finalIK + $" ({Strings.Warning.notFound})", Icons.Avatar);
							EditorGUI.EndDisabledGroup();
						}

                        Helpers.DrawGUILine(1, false);
                    }

                    if(CopierTabs.ComponentIsInSelectedTab("vrcstation", Settings._copier_selectedTab))
                    {
                        Helpers.DrawDropdownWithToggle(ref Settings._copier_expand_vrcStations, ref Settings.bCopier_vrcStations_copy, Strings.Copier.vrc_station, Icons.CsScript);
                        if(Settings._copier_expand_vrcStations)
                        {
                            EditorGUI.BeginDisabledGroup(!Settings.bCopier_vrcStations_copy);
                            EditorGUILayout.Space();

                            using(var cHorizontalScope = new GUILayout.HorizontalScope())
                            {
                                GUILayout.Space(COPIER_SETTINGS_INDENT_SIZE); // horizontal indent size

                                using(var cVerticalScope = new GUILayout.VerticalScope())
                                {
                                    Settings.bCopier_vrcStations_fixReferences = GUILayout.Toggle(Settings.bCopier_vrcStations_fixReferences, Strings.Copier.fixReferences, Styles.CopierToggle);
                                    Settings.bCopier_vrcStations_createObjects = GUILayout.Toggle(Settings.bCopier_vrcStations_createObjects, Strings.Copier.copyGameObjects, Styles.CopierToggle);
                                }
                            }

                            EditorGUILayout.Space();
                            EditorGUI.EndDisabledGroup();
                        }

                        Helpers.DrawGUILine(1, false);
                    }

                    if(CopierTabs.ComponentIsInSelectedTab("other", Settings._copier_selectedTab))
                    {
                        //External menu
                        Helpers.DrawDropdownWithToggle(ref Settings._copier_expand_other, ref Settings.bCopier_other_copy, Strings.Copier.other, Icons.CsScript);
                        if(Settings._copier_expand_other)
                        {
                            EditorGUILayout.Space();

                            using(var cHorizontalScope = new GUILayout.HorizontalScope())
                            {
                                GUILayout.Space(COPIER_SETTINGS_INDENT_SIZE); // horizontal indent size

                                using(var cVerticalScope = new GUILayout.VerticalScope())
                                {
                                    using(new EditorGUILayout.HorizontalScope())
                                    {
                                        EditorGUILayout.HelpBox("You can add your own types here by editing the extra copier types json file.", MessageType.Info);
                                        using(new EditorGUILayout.VerticalScope())
                                        {
                                            if(GUILayout.Button("Open json"))
                                                PumkinsTypeCache.OpenExtraTypesJson();
                                            if(GUILayout.Button("Refresh Types"))
                                                PumkinsTypeCache.LoadExtraTypes();
                                        }
                                    }

                                    Helpers.DrawGUILine();

                                    // Draw dynamic types
                                    EditorGUI.BeginDisabledGroup(!Settings.bCopier_other_copy);
                                    foreach(var typeWrapper in PumkinsTypeCache.ExtraTypes)
                                    {
                                        using(new EditorGUILayout.HorizontalScope())
                                        {
                                            GUILayout.Label(typeWrapper.categoryName, EditorStyles.boldLabel, GUILayout.MaxWidth(160));
                                            if(GUILayout.Button("All", GUILayout.MaxWidth(40)))
                                            {
                                                for(int i = 0; i < typeWrapper.enableStates.Count; i++)
                                                    typeWrapper.enableStates[i] = true;
                                            }

                                            if(GUILayout.Button("None", GUILayout.MaxWidth(40)))
                                            {
                                                for(int i = 0; i < typeWrapper.enableStates.Count; i++)
                                                    typeWrapper.enableStates[i] = false;
                                            }
                                        }

                                        for(int i = 0; i < typeWrapper.types.Count; i++)
                                            typeWrapper.enableStates[i] = GUILayout.Toggle(typeWrapper.enableStates[i], typeWrapper.names[i], Styles.CopierToggle);

                                        Helpers.DrawGUILine();
                                    }

                                    EditorGUILayout.Space();
                                    Settings.bCopier_other_fixReferences = GUILayout.Toggle(Settings.bCopier_other_fixReferences, Strings.Copier.fixReferences, Styles.CopierToggle);
                                    Settings.bCopier_other_createGameObjects = GUILayout.Toggle(Settings.bCopier_other_createGameObjects, Strings.Copier.copyGameObjects, Styles.CopierToggle);
                                }
                            }

                            EditorGUILayout.Space();
                            EditorGUI.EndDisabledGroup();
                        }

                        Helpers.DrawGUILine(1, false);
                    }

                    EditorGUILayout.Space();

                    //=======================================================

                    //Ignore Array
                    EditorGUI.BeginChangeCheck();
                    {
                        Helpers.DrawPropertyArrayScrolling(SerializedIgnoreArray, Strings.Copier.exclusions, ref Settings._copierIgnoreArray_expand,
                            ref Settings._copierIgnoreArrayScroll, 0, 100, 0, Icons.Hidden);
                    }
                    if(EditorGUI.EndChangeCheck())
                    {
                        RefreshIgnoreArray();
                    }

                    if(Settings._copierIgnoreArray_expand && SerializedIgnoreArray.arraySize > 0)
                    {
                        using(var cHorizontalScope = new GUILayout.HorizontalScope())
                        {
                            GUILayout.Space(COPIER_SETTINGS_INDENT_SIZE * 4); // horizontal indent size
                            using(var cVerticalScope = new GUILayout.VerticalScope())
                            {
                                Settings.bCopier_ignoreArray_includeChildren = GUILayout.Toggle(Settings.bCopier_ignoreArray_includeChildren, Strings.Copier.includeChildren);
                            }
                        }
                    }

                    Helpers.DrawGUILine();

                    EditorGUILayout.BeginHorizontal();
                    {
                        //TODO: Refactor these 2 buttons
                        if(GUILayout.Button(Strings.Buttons.selectNone, Styles.BigButton, GUILayout.MinWidth(100)))
                        {
                            if(Settings._copier_selectedTab == CopierTabs.Tab.All)
                            {
                                Settings.bCopier_colliders_copy = false;
                                Settings.bCopier_rigidBodies_copy = false;
                                Settings.bCopier_transforms_copy = false;
                                Settings.bCopier_animators_copy = false;
                                Settings.bCopier_joints_copy = false;
                                Settings.bCopier_cameras_copy = false;
                                Settings.bCopier_finalIK_copy = false;
#if VRC_SDK_VRCSDK3 || VRC_SDK_VRCSDK2
                                Settings.bCopier_vrcStations_copy = false;
#endif
                            }
#if VRC_SDK_VRCSDK3 || VRC_SDK_VRCSDK2
                            Settings.bCopier_descriptor_copy = false;
#endif
                            Settings.bCopier_other_copy = false;
                            Settings.bCopier_aimConstraint_copy = false;
                            Settings.bCopier_lookAtConstraint_copy = false;
                            Settings.bCopier_parentConstraint_copy = false;
                            Settings.bCopier_positionConstraint_copy = false;
                            Settings.bCopier_rotationConstraint_copy = false;
                            Settings.bCopier_scaleConstraint_copy = false;
                            Settings.bCopier_trailRenderers_copy = false;
                            Settings.bCopier_lights_copy = false;
                            Settings.bCopier_skinMeshRender_copy = false;
                            Settings.bCopier_audioSources_copy = false;
                            Settings.bCopier_meshRenderers_copy = false;
                            Settings.bCopier_particleSystems_copy = false;
                            Settings.bCopier_physBones_copy = false;
                            Settings.bCopier_physBones_copyColliders = false;
                            Settings.bCopier_contactReceiver_copy = false;
                            Settings.bCopier_contactSender_copy = false;
                            Settings.bCopier_prefabs_copy = false;

                            if(DynamicBonesExist)
                            {
                                Settings.bCopier_dynamicBones_copy = false;
                                Settings.bCopier_dynamicBones_copyColliders = false;
                            }
                        }
                        if(GUILayout.Button(Strings.Buttons.selectAll, Styles.BigButton, GUILayout.MinWidth(100)))
                        {
                            if(Settings._copier_selectedTab == CopierTabs.Tab.All)
                            {
                                Settings.bCopier_colliders_copy = true;
                                Settings.bCopier_rigidBodies_copy = true;
                                Settings.bCopier_transforms_copy = true;
                                Settings.bCopier_animators_copy = true;
                                Settings.bCopier_joints_copy = true;
                                Settings.bCopier_cameras_copy = true;
                                Settings.bCopier_finalIK_copy = true && FinalIKExists;
#if VRC_SDK_VRCSDK3 || VRC_SDK_VRCSDK2
                                Settings.bCopier_vrcStations_copy = true;
#endif
                            }

#if VRC_SDK_VRCSDK3 || VRC_SDK_VRCSDK2
                            Settings.bCopier_descriptor_copy = true;
#endif
                            Settings.bCopier_other_copy = true;
                            Settings.bCopier_aimConstraint_copy = true;
                            Settings.bCopier_lookAtConstraint_copy = true;
                            Settings.bCopier_parentConstraint_copy = true;
                            Settings.bCopier_positionConstraint_copy = true;
                            Settings.bCopier_rotationConstraint_copy = true;
                            Settings.bCopier_scaleConstraint_copy = true;
                            Settings.bCopier_trailRenderers_copy = true;
                            Settings.bCopier_lights_copy = true;
                            Settings.bCopier_skinMeshRender_copy = true;
                            Settings.bCopier_audioSources_copy = true;
                            Settings.bCopier_meshRenderers_copy = true;
                            Settings.bCopier_particleSystems_copy = true;
                            Settings.bCopier_physBones_copy = true;
                            Settings.bCopier_physBones_copyColliders = true;
                            Settings.bCopier_contactReceiver_copy = true;
                            Settings.bCopier_contactSender_copy = true;
                            Settings.bCopier_prefabs_copy = true;

                            if(DynamicBonesExist)
                            {
                                Settings.bCopier_dynamicBones_copy = true;
                                Settings.bCopier_dynamicBones_copyColliders = true;
                            }
                            else
                            {
                                Settings.bCopier_dynamicBones_copy = false;
                                Settings.bCopier_dynamicBones_copyColliders = false;
                            }
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    Helpers.DrawGUILine();

                    EditorGUI.BeginDisabledGroup(!CopierHasSelections);
                    {
                        if(GUILayout.Button(Strings.Buttons.copySelected, Styles.BigButton))
                        {
                            string log = "";
                            if(!CopierSelectedFrom)
                            {
                                log += Strings.Log.copyFromInvalid;
                                Log(log, LogType.Warning);
                            }
                            else
                            {
                                //Cancel Checks
                                if(CopierSelectedFrom == SelectedAvatar)
                                {
                                    Log(log + Strings.Log.cantCopyToSelf, LogType.Warning);
                                    return;
                                }

                                RefreshIgnoreArray();

                                CopyComponentsWithoutParents(CopierSelectedFrom, SelectedAvatar);

                                EditorUtility.SetDirty(SelectedAvatar);
                                if(!EditorApplication.isPlaying)
                                    EditorSceneManager.MarkSceneDirty(SelectedAvatar.scene);

                                avatarInfo = PumkinsAvatarInfo.GetInfo(SelectedAvatar, out _avatarInfoString);

                                log += Strings.Log.done;
                                Log(log, LogType.Log);
                            }
                        }
                    }
                    EditorGUI.EndDisabledGroup();
                }
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.Space();
            }
        }

        void DrawInfoMenuGUI()
        {
            if(Settings._info_expand = GUILayout.Toggle(Settings._info_expand, Strings.Main.info, Styles.Foldout_title))
            {
                EditorGUILayout.Space();

                GUILayout.BeginHorizontal();
                {
                    if(GUILayout.Button(new GUIContent(Strings.Buttons.openGithubPage, Icons.GithubIcon)))
                    {
                        Application.OpenURL(Strings.LINK_GITHUB);
                    }
                }
                GUILayout.EndHorizontal();

                if(GUILayout.Button(new GUIContent(Strings.Buttons.joinDiscordServer, Icons.DiscordIcon)))
                {
                    Application.OpenURL(Strings.LINK_DISCORD);
                }
                if(GUILayout.Button(new GUIContent(Strings.Buttons.openDonationPage, Icons.KofiIcon), Styles.BigButton))
                {
                    Application.OpenURL(Strings.LINK_DONATION);
                }
            }
        }

        void DrawThumbnailsMenuGUI()
        {
            if(Settings._thumbnails_expand = GUILayout.Toggle(Settings._thumbnails_expand, Strings.Main.thumbnails, Styles.Foldout_title))
            {
                Helpers.DrawGUILine();

                EditorGUI.BeginChangeCheck();
                {
                    Settings._presetToolbarSelectedIndex = GUILayout.Toolbar(Settings._presetToolbarSelectedIndex, new string[] { Strings.Thumbnails.cameras, Strings.Thumbnails.poses, Strings.Thumbnails.blendshapes }, Styles.ToolbarBigButtons);
                }
                if(EditorGUI.EndChangeCheck())
                {
                    if(Settings._presetToolbarSelectedIndex == (int)PresetToolbarOptions.Blendshape)
                        SetupBlendeshapeRendererHolders(SelectedAvatar);
                }

                EditorGUILayout.Space();
                Helpers.DrawGUILine();

                switch(Settings._presetToolbarSelectedIndex)
                {
                    case 0:
                        DrawThumbnailCameraGUI();
                        DrawPresetGUI<PumkinsCameraPreset>();
                        break;
                    case 1:
                        DrawThumbnailPoseGUI();
                        DrawPresetGUI<PumkinsPosePreset>();
                        break;
                    case 2:
                        DrawThumbanailBlendshapeGUI();
                        DrawPresetGUI<PumkinsBlendshapePreset>();
                        break;
                    default:
                        break;
                }
            }
        }

        void DrawThumbanailBlendshapeGUI()
        {
            EditorGUILayout.LabelField(new GUIContent(Strings.Thumbnails.blendshapes));
            if(SelectedAvatar)
                Helpers.DrawBlendshapeSlidersWithLabels(ref _selectedAvatarRendererHolders, SelectedAvatar);
            else
                EditorGUILayout.LabelField(new GUIContent(Strings.PoseEditor.selectHumanoidAvatar), Styles.HelpBox_OneLine);
            EditorGUILayout.Space();
        }

        void DrawThumbnailPoseGUI()
        {
            if(GUILayout.Button(Strings.Buttons.openPoseEditor, Styles.BigButton))
                PumkinsPoseEditor.ShowWindow();

            Helpers.DrawGUILine();

            Settings.posePresetApplyBodyPosition = GUILayout.Toggle(Settings.posePresetApplyBodyPosition, Strings.Thumbnails.applyBodyPosition);
            Settings.posePresetApplyBodyRotation = GUILayout.Toggle(Settings.posePresetApplyBodyRotation, Strings.Thumbnails.applyBodyRotation);

            EditorGUILayout.Space();

            Settings.posePresetTryFixSinking = GUILayout.Toggle(Settings.posePresetTryFixSinking, Strings.Thumbnails.tryFixPoseSinking);
        }

        void DrawThumbnailCameraGUI()
        {
            //TODO: Make it so camera isn't being searched for every frame in the property
            SelectedCamera = EditorGUILayout.ObjectField(Strings.Thumbnails.selectedCamera, SelectedCamera, typeof(Camera), true) as Camera;

            Helpers.DrawGUILine();

            DrawOverlayGUI();

            Helpers.DrawGUILine();

            DrawBackgroundGUI();

            Helpers.DrawGUILine();

            DrawCameraControlButtons();
        }

        public void DrawCameraControlButtons()
        {
            //Camera to scene view button
            if(GUILayout.Button(Strings.Buttons.alignCameraToView, Styles.BigButton))
            {
                SelectedCamera.transform.position = SceneView.lastActiveSceneView.camera.transform.position;
                SelectedCamera.transform.rotation = SceneView.lastActiveSceneView.camera.transform.rotation;
            }

            EditorGUILayout.Space();

            Settings.lockSelectedCameraToSceneView = GUILayout.Toggle(Settings.lockSelectedCameraToSceneView, Strings.Thumbnails.lockSelectedCameraToSceneView);

            Helpers.DrawGUILine();

            EditorGUI.BeginDisabledGroup(!SelectedCamera || !SelectedAvatar);
            {
                //Center Camera on Viewpoint button
                GUILayout.BeginHorizontal();
                {
                    string centerOnWhat = "?";
                    switch(Settings.centerCameraMode)
                    {
                        case PumkinsCameraPreset.CameraOffsetMode.AvatarRoot:
                            centerOnWhat = Strings.Main.avatar;
                            break;
                        case PumkinsCameraPreset.CameraOffsetMode.Transform:
                            if(SelectedAvatar && !Settings.centerCameraTransform)
                                Settings.centerCameraTransform = SelectedAvatar.transform.Find(Settings.centerCameraTransformPath);
                            if(Settings.centerCameraTransform)
                                centerOnWhat = Settings.centerCameraTransform.name;
                            break;
                        default:
                            centerOnWhat = Strings.Thumbnails.viewpoint;
                            break;
                    }

                    string centerCameraString = string.Format(Strings.Thumbnails.centerCameraOn, centerOnWhat);
                    if(GUILayout.Button(centerCameraString, Styles.BigButton))
                    {
                        if(SelectedCamera)
                        {
                            switch(Settings.centerCameraMode)
                            {
                                case PumkinsCameraPreset.CameraOffsetMode.AvatarRoot:
                                    CenterCameraOnTransform(SelectedAvatar.transform, Settings.centerCameraPositionOffsetAvatar, Settings.centerCameraRotationOffsetAvatar, Settings.centerCameraFixClippingPlanes);
                                    break;
                                case PumkinsCameraPreset.CameraOffsetMode.Transform:
                                    CenterCameraOnTransform(Settings.centerCameraTransform, Settings.centerCameraPositionOffsetTransform, Settings.centerCameraRotationOffsetTransform, Settings.centerCameraFixClippingPlanes);
                                    break;
#if VRC_SDK_VRCSDK3
                                default:
                                    CenterCameraOnViewpoint(SelectedAvatar, Settings.centerCameraPositionOffsetViewpoint, Settings.centerCameraRotationOffsetViewpoint, Settings.centerCameraFixClippingPlanes);
                                    break;
#endif
                            }
                        }
                        else
                            Log(Strings.Warning.cameraNotFound, LogType.Warning);
                    }
                    if(GUILayout.Button(Icons.Settings, Styles.BigIconButton))
                    {
                        Settings._centerCameraOffsets_expand = !Settings._centerCameraOffsets_expand;
                    }
                }
                GUILayout.EndHorizontal();
                if(Settings._centerCameraOffsets_expand)
                {
                    EditorGUILayout.Space();

                    Settings.centerCameraFixClippingPlanes = GUILayout.Toggle(Settings.centerCameraFixClippingPlanes, Strings.Thumbnails.centerCameraFixClippingPlanes);

                    EditorGUILayout.Space();

                    Settings.centerCameraMode = (PumkinsCameraPreset.CameraOffsetMode)EditorGUILayout.EnumPopup(Strings.Presets.mode, Settings.centerCameraMode);

                    if(Settings.centerCameraMode == PumkinsCameraPreset.CameraOffsetMode.Transform)
                    {
                        EditorGUI.BeginChangeCheck();
                        {
                            Settings.centerCameraTransformPath = EditorGUILayout.TextField(Strings.Presets.transform, Settings.centerCameraTransformPath);
                        }
                        if(EditorGUI.EndChangeCheck())
                        {
                            Settings.centerCameraTransform = SelectedAvatar.transform.Find(Settings.centerCameraTransformPath);
                        }
                    }
                    else
                        GUILayout.Space(18);

                    EditorGUILayout.Space();

                    switch(Settings.centerCameraMode)
                    {
                        case PumkinsCameraPreset.CameraOffsetMode.Transform:
                            Settings.centerCameraPositionOffsetTransform = EditorGUILayout.Vector3Field(Strings.Thumbnails.positionOffset, Settings.centerCameraPositionOffsetTransform);
                            Settings.centerCameraRotationOffsetTransform = EditorGUILayout.Vector3Field(Strings.Thumbnails.rotationOffset, Settings.centerCameraRotationOffsetTransform);
                            break;
#if VRC_SDK_VRCSDK3
                        case PumkinsCameraPreset.CameraOffsetMode.Viewpoint:
                            Settings.centerCameraPositionOffsetViewpoint = EditorGUILayout.Vector3Field(Strings.Thumbnails.positionOffset, Settings.centerCameraPositionOffsetViewpoint);
                            Settings.centerCameraRotationOffsetViewpoint = EditorGUILayout.Vector3Field(Strings.Thumbnails.rotationOffset, Settings.centerCameraRotationOffsetViewpoint);
                            break;
#endif
                        case PumkinsCameraPreset.CameraOffsetMode.AvatarRoot:
                            Settings.centerCameraPositionOffsetAvatar = EditorGUILayout.Vector3Field(Strings.Thumbnails.positionOffset, Settings.centerCameraPositionOffsetAvatar);
                            Settings.centerCameraRotationOffsetAvatar = EditorGUILayout.Vector3Field(Strings.Thumbnails.rotationOffset, Settings.centerCameraRotationOffsetAvatar);
                            break;
                    }

                    EditorGUILayout.Space();

                    EditorGUILayout.BeginHorizontal();
                    {
                        if(GUILayout.Button(Strings.Buttons.reset, GUILayout.MaxWidth(90f)))
                        {
                            switch(Settings.centerCameraMode)
                            {
#if VRC_SDK_VRCSDK3
                                case PumkinsCameraPreset.CameraOffsetMode.Viewpoint:
                                    Settings.centerCameraPositionOffsetViewpoint = DEFAULT_CAMERA_POSITION_OFFSET_VIEWPOINT;
                                    Settings.centerCameraRotationOffsetViewpoint = DEFAULT_CAMERA_ROTATION_OFFSET_VIEWPOINT;
                                    break;
#endif
                                case PumkinsCameraPreset.CameraOffsetMode.AvatarRoot:
                                    Settings.centerCameraPositionOffsetAvatar = DEFAULT_CAMERA_POSITION_OFFSET_AVATAR;
                                    Settings.centerCameraRotationOffsetAvatar = DEFAULT_CAMERA_ROTATION_OFFSET_AVATAR;
                                    break;
                                case PumkinsCameraPreset.CameraOffsetMode.Transform:
                                    Settings.centerCameraPositionOffsetTransform = DEFAULT_CAMERA_POSITION_OFFSET_TRANSFORM;
                                    Settings.centerCameraRotationOffsetTransform = DEFAULT_CAMERA_ROTATION_OFFSET_TRANSFORM;
                                    break;
                            }
                        }
                        if(GUILayout.Button(Strings.Buttons.setFromCamera))
                        {
                            SerialTransform st = null;
                            switch(Settings.centerCameraMode)
                            {
#if VRC_SDK_VRCSDK3
                                case PumkinsCameraPreset.CameraOffsetMode.Viewpoint:
                                    st = PumkinsCameraPreset.GetCameraOffsetFromViewpoint(SelectedAvatar, SelectedCamera);
                                    if(st)
                                    {
                                        Settings.centerCameraPositionOffsetViewpoint = Helpers.RoundVectorValues(st.localPosition, 3);
                                        Settings.centerCameraRotationOffsetViewpoint = Helpers.RoundVectorValues(st.localEulerAngles, 3);
                                    }
                                    break;
#endif
                                case PumkinsCameraPreset.CameraOffsetMode.AvatarRoot:
                                    st = PumkinsCameraPreset.GetOffsetsFromTransform(SelectedAvatar.transform, SelectedCamera);
                                    if(st)
                                    {
                                        Settings.centerCameraPositionOffsetAvatar = Helpers.RoundVectorValues(st.localPosition, 3);
                                        Settings.centerCameraRotationOffsetAvatar = Helpers.RoundVectorValues(st.localEulerAngles, 3);
                                    }
                                    break;
                                case PumkinsCameraPreset.CameraOffsetMode.Transform:
                                    st = PumkinsCameraPreset.GetOffsetsFromTransform(Settings.centerCameraTransform, SelectedCamera);
                                    if(st)
                                    {
                                        Settings.centerCameraPositionOffsetTransform = Helpers.RoundVectorValues(st.localPosition, 3);
                                        Settings.centerCameraRotationOffsetTransform = Helpers.RoundVectorValues(st.localEulerAngles, 3);
                                    }
                                    break;
                            }
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUI.EndDisabledGroup();
        }

        void DrawAvatarInfoMenuGUI()
        {
            if(Settings._avatarInfo_expand = GUILayout.Toggle(Settings._avatarInfo_expand, Strings.Main.avatarInfo, Styles.Foldout_title))
            {
                if(SelectedAvatar == null)
                {
                    if(avatarInfo != null)
                    {
                        avatarInfo = null;
                        _avatarInfoString = Strings.AvatarInfo.selectAvatarFirst;
                    }
                }
                else
                {
                    if(avatarInfo == null)
                    {
                        avatarInfo = PumkinsAvatarInfo.GetInfo(SelectedAvatar, out _avatarInfoString);
                    }
                }

                EditorGUILayout.SelectableLabel(_avatarInfoString, Styles.HelpBox, GUILayout.MinHeight(260));

                EditorGUI.BeginDisabledGroup(SelectedAvatar == null);
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        if(GUILayout.Button(Strings.Buttons.copy))
                        {
                            EditorGUIUtility.systemCopyBuffer = _avatarInfoString;
                        }
                        if(GUILayout.Button(Strings.Buttons.refresh))
                        {
                            avatarInfo = PumkinsAvatarInfo.GetInfo(SelectedAvatar, out _avatarInfoString);
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUI.EndDisabledGroup();
            }
        }

        void DrawToolsMenuGUI()
        {
            if(Settings._tools_expand = GUILayout.Toggle(Settings._tools_expand, Strings.Main.tools, Styles.Foldout_title))
            {
                EditorGUI.BeginDisabledGroup(SelectedAvatar == null);
                {
                    Helpers.DrawGUILine();

                    //Quick setup
                    GUILayout.BeginHorizontal();
                    {
                        if(GUILayout.Button(Strings.Buttons.quickSetupAvatar, Styles.BigButton))
                        {
                            //if(settings._tools_quickSetup_autoRig)
                            //    SetupRig(SelectedAvatar);
#if VRC_SDK_VRCSDK3
                            if(Settings._tools_quickSetup_fillVisemes)
                                DoAction(SelectedAvatar, ToolMenuActions.FillVisemes);
                            if(Settings._tools_quickSetup_setViewpoint)
                                QuickSetViewpoint(SelectedAvatar, Settings._tools_quickSetup_viewpointZDepth);
                            if(Settings._tools_quicksetup_fillEyeBones)
                                DoAction(SelectedAvatar, ToolMenuActions.FillEyeBones);
#endif
                            if(Settings._tools_quickSetup_forceTPose)
                                DoAction(SelectedAvatar, ToolMenuActions.SetTPose);

                            //Set renderer anchors
                            Type[] anchorTypesToSet = new List<Type>
                            {
                                Settings._tools_quickSetup_setMeshRendererAnchor ? typeof(MeshRenderer) : null,
                                Settings._tools_quickSetup_setSkinnedMeshRendererAnchor ? typeof(SkinnedMeshRenderer) : null,
                                Settings._tools_quickSetup_setParticleSystemRendererAnchor ? typeof(ParticleSystemRenderer) : null,
                                Settings._tools_quickSetup_setTrailRendererAnchor ? typeof(TrailRenderer) : null
                            }.Where(t => t != null).ToArray();

                            if(Settings._tools_quicksetup_setMeshRendererAnchor_usePath)
                                SetRendererAnchor(SelectedAvatar, Settings._tools_quickSetup_setRenderAnchor_path, anchorTypesToSet);
                            else
                                SetRendererAnchor(SelectedAvatar, Settings._tools_quickSetup_setRenderAnchor_bone, anchorTypesToSet);

                            if(Settings.showExperimental)
                            {
                                foreach(var tool in newTools)
                                    if(tool.quickSetupActive)
                                        tool.TryExecute(SelectedAvatar);
                            }
                        }

                        if(GUILayout.Button(Icons.Settings, Styles.BigIconButton))
                            Settings._tools_quickSetup_settings_expand = !Settings._tools_quickSetup_settings_expand;
                    }
                    GUILayout.EndHorizontal();

                    if(Settings._tools_quickSetup_settings_expand)
                    {
                        EditorGUILayout.Space();

#if VRC_SDK_VRCSDK3
                        GUILayout.BeginHorizontal();
                        {
                            Vector2 size = EditorStyles.toggle.CalcSize(new GUIContent(Strings.Tools.autoViewpoint));
                            Settings._tools_quickSetup_setViewpoint =
                                GUILayout.Toggle(Settings._tools_quickSetup_setViewpoint, Strings.Tools.autoViewpoint, GUILayout.MaxWidth(size.x));

                            size = EditorStyles.numberField.CalcSize(new GUIContent(Strings.Tools.viewpointZDepth));
                            EditorGUI.BeginDisabledGroup(!Settings._tools_quickSetup_setViewpoint);
                            {
                                float old = EditorGUIUtility.labelWidth;
                                EditorGUIUtility.labelWidth = size.x * 1.1f;
                                Settings._tools_quickSetup_viewpointZDepth = EditorGUILayout.FloatField(Strings.Tools.viewpointZDepth, Settings._tools_quickSetup_viewpointZDepth);
                                EditorGUIUtility.labelWidth = old;
                            }
                            EditorGUI.EndDisabledGroup();
                        }
                        GUILayout.EndHorizontal();

                        Settings._tools_quickSetup_fillVisemes = GUILayout.Toggle(Settings._tools_quickSetup_fillVisemes, Strings.Tools.fillVisemes);
                        Settings._tools_quicksetup_fillEyeBones = GUILayout.Toggle(Settings._tools_quicksetup_fillEyeBones, Strings.Tools.fillEyeBones);
#endif
                        Settings._tools_quickSetup_forceTPose = GUILayout.Toggle(Settings._tools_quickSetup_forceTPose, Strings.Tools.setTPose);

                        Helpers.DrawGUILine();

                        bool anchorUsePath = Settings._tools_quicksetup_setMeshRendererAnchor_usePath;

                        Settings._tools_quicksetup_setMeshRendererAnchor_usePath =
                            EditorGUILayout.ToggleLeft(Strings.Tools.anchorUsePath, anchorUsePath);

                        if(anchorUsePath)
                        {
                            GUILayout.BeginHorizontal();
                            {
                                GUILayout.Label(Strings.Tools.anchorPath);
                                Settings._tools_quickSetup_setRenderAnchor_path =
                                    EditorGUILayout.TextField(Settings._tools_quickSetup_setRenderAnchor_path);
                            }
                            GUILayout.EndHorizontal();
                        }
                        else
                        {
                            Settings._tools_quickSetup_setRenderAnchor_bone =
                                (HumanBodyBones)EditorGUILayout.EnumPopup(Strings.Tools.humanoidBone, Settings._tools_quickSetup_setRenderAnchor_bone);
                        }

                        EditorGUILayout.Space();

                        bool disabled =
                            anchorUsePath && Helpers.StringIsNullOrWhiteSpace(Settings._tools_quickSetup_setRenderAnchor_path);
                        EditorGUI.BeginDisabledGroup(disabled);
                        {
                            Settings._tools_quickSetup_setSkinnedMeshRendererAnchor = GUILayout.Toggle(Settings._tools_quickSetup_setSkinnedMeshRendererAnchor,
                                Strings.Tools.setSkinnedMeshRendererAnchors);
                            Settings._tools_quickSetup_setMeshRendererAnchor =
                                GUILayout.Toggle(Settings._tools_quickSetup_setMeshRendererAnchor, Strings.Tools.setMeshRendererAnchors);
                        }
                        EditorGUI.EndDisabledGroup();

                        EditorGUILayout.Space();
                        if(Settings.showExperimental)
                        {
                            foreach(var tool in newTools)
                                tool.DrawQuickSetupGUI();
                        }
                    }

                    Helpers.DrawGUILine();

                    //Tools
                    if(Settings._tools_avatar_expand = GUILayout.Toggle(Settings._tools_avatar_expand, Strings.Main.avatar, Styles.Foldout))
                    {
                        GUILayout.BeginHorizontal(); //Row
                        {
                            GUILayout.BeginVertical(GUILayout.ExpandWidth(true)); //Left Column
                            {
#if VRC_SDK_VRCSDK3
                                if(GUILayout.Button(Strings.Tools.fillVisemes))
                                    DoAction(SelectedAvatar, ToolMenuActions.FillVisemes);
#endif
                                if(GUILayout.Button(Strings.Tools.revertBlendshapes))
                                    DoAction(SelectedAvatar, ToolMenuActions.RevertBlendshapes);
                                EditorGUI.BeginDisabledGroup(DrawingHandlesGUI);
                                {
                                    if(GUILayout.Button(Strings.Tools.revertScale))
                                        DoAction(SelectedAvatar, ToolMenuActions.RevertScale);
                                }
                                EditorGUI.EndDisabledGroup();
                            }
                            GUILayout.EndVertical();

                            GUILayout.BeginVertical(GUILayout.ExpandWidth(true)); //Right Column
                            {
#if VRC_SDK_VRCSDK3
                                EditorGUI.BeginDisabledGroup(DrawingHandlesGUI);
                                {
                                    if(GUILayout.Button(Strings.Tools.editViewpoint))
                                        DoAction(SelectedAvatar, ToolMenuActions.EditViewpoint);
                                }
                                EditorGUI.EndDisabledGroup();

                                if(GUILayout.Button(Strings.Tools.fillEyeBones))
                                    DoAction(SelectedAvatar, ToolMenuActions.FillEyeBones);
#endif
                                if(GUILayout.Button(Strings.Tools.zeroBlendshapes))
                                    DoAction(SelectedAvatar, ToolMenuActions.ZeroBlendshapes);
                            }
                            GUILayout.EndVertical();
                        }
                        GUILayout.EndHorizontal();

                        EditorGUI.BeginDisabledGroup(DrawingHandlesGUI);
                        {
                            if(GUILayout.Button(Strings.Tools.editScale))
                                DoAction(SelectedAvatar, ToolMenuActions.EditScale);
                        }
                        EditorGUI.EndDisabledGroup();

                        using(new GUILayout.HorizontalScope())
                        {
                            if(GUILayout.Button(Strings.Tools.resetPose))
                                DoAction(SelectedAvatar, ToolMenuActions.ResetPose);
                            Settings._tools_avatar_resetpose_expand = GUILayout.Toggle(Settings._tools_avatar_resetpose_expand,
                                EditorGUIUtility.IconContent("align_vertically_center"), "button", GUILayout.Width(20));
                        }

                        if(Settings._tools_avatar_resetpose_expand)
                        {
                            using(new GUILayout.VerticalScope("helpbox"))
                            {
                                Settings._tools_avatar_resetPose_type =
                                    (SettingsContainer.ResetPoseType)EditorGUILayout.EnumPopup("Reset To", Settings._tools_avatar_resetPose_type);

                                using(new EditorGUI.DisabledScope(Settings._tools_avatar_resetPose_type == SettingsContainer.ResetPoseType.TPose))
                                {
                                    Settings._tools_avatar_resetPose_position = EditorGUILayout.Toggle("Position", Settings._tools_avatar_resetPose_position);
                                    Settings._tools_avatar_resetPose_rotation = EditorGUILayout.Toggle("Rotation", Settings._tools_avatar_resetPose_rotation);
                                    Settings._tools_avatar_resetPose_scale = EditorGUILayout.Toggle("Scale", Settings._tools_avatar_resetPose_scale);
                                }

                                using(new EditorGUI.DisabledScope(Settings._tools_avatar_resetPose_type != SettingsContainer.ResetPoseType.AvatarDefinition))
                                    Settings._tools_avatar_resetPose_fullreset =
                                        EditorGUILayout.Toggle(new GUIContent("Full Reset", "Reset all the objects included in the Avatar definition."),
                                            Settings._tools_avatar_resetPose_fullreset);

                            }
                        }


                        Helpers.DrawGUILine();

                        bool anchorUsePath = Settings._tools_quicksetup_setMeshRendererAnchor_usePath;
                        Settings._tools_quicksetup_setMeshRendererAnchor_usePath = EditorGUILayout.ToggleLeft(
                            Strings.Tools.anchorUsePath, anchorUsePath);

                        if(anchorUsePath)
                        {
                            Settings._tools_quickSetup_setRenderAnchor_path = EditorGUILayout.TextField(Strings.Tools.anchorPath,
                                Settings._tools_quickSetup_setRenderAnchor_path);

                            bool disabled = anchorUsePath && string.IsNullOrWhiteSpace(Settings._tools_quickSetup_setRenderAnchor_path);
                            EditorGUI.BeginDisabledGroup(disabled);
                            {
                                if(GUILayout.Button(Strings.Tools.setSkinnedMeshRendererAnchors))
                                    SetRendererAnchor(SelectedAvatar, Settings._tools_quickSetup_setRenderAnchor_path, typeof(SkinnedMeshRenderer));
                                if(GUILayout.Button(Strings.Tools.setMeshRendererAnchors))
                                    SetRendererAnchor(SelectedAvatar, Settings._tools_quickSetup_setRenderAnchor_path, typeof(MeshRenderer));
                                if(GUILayout.Button(Strings.Tools.setParticleSystemAnchors))
                                    SetRendererAnchor(SelectedAvatar, Settings._tools_quickSetup_setRenderAnchor_path, typeof(ParticleSystemRenderer));
                                if(GUILayout.Button(Strings.Tools.setTrailRendererAnchors))
                                    SetRendererAnchor(SelectedAvatar, Settings._tools_quickSetup_setRenderAnchor_path, typeof(TrailRenderer));
                            }
                            EditorGUI.EndDisabledGroup();
                        }
                        else
                        {
                            Settings._tools_quickSetup_setRenderAnchor_bone =
                                (HumanBodyBones)EditorGUILayout.EnumPopup(Strings.Tools.humanoidBone, Settings._tools_quickSetup_setRenderAnchor_bone);

                            if(GUILayout.Button(Strings.Tools.setSkinnedMeshRendererAnchors))
                                SetRendererAnchor(SelectedAvatar, Settings._tools_quickSetup_setRenderAnchor_bone, typeof(SkinnedMeshRenderer));
                            if(GUILayout.Button(Strings.Tools.setMeshRendererAnchors))
                                SetRendererAnchor(SelectedAvatar, Settings._tools_quickSetup_setRenderAnchor_bone, typeof(MeshRenderer));
                            if(GUILayout.Button(Strings.Tools.setParticleSystemAnchors))
                                SetRendererAnchor(SelectedAvatar, Settings._tools_quickSetup_setRenderAnchor_bone, typeof(ParticleSystemRenderer));
                            if(GUILayout.Button(Strings.Tools.setTrailRendererAnchors))
                                SetRendererAnchor(SelectedAvatar, Settings._tools_quickSetup_setRenderAnchor_bone, typeof(TrailRenderer));
                        }

                        EditorGUILayout.Space();

                        if(Settings.showExperimental)
                            foreach(var tool in newTools)
                                tool.DrawGUI();
                    }

                    Helpers.DrawGUILine();

#if VRC_SDK_VRCSDK3
                    //Setup pbone gui stuff
                    string pboneStateString = Strings.Copier.physBones;
                    if(!PhysBonesExist)
                        pboneStateString += " | Avatar SDK required";

                    if(Settings._tools_physBones_expand = GUILayout.Toggle(Settings._tools_physBones_expand, pboneStateString, Styles.Foldout))
                    {
                        EditorGUI.BeginDisabledGroup(!PhysBonesExist);
                        {
                            EditorGUILayout.BeginHorizontal();
                            {
                                if(GUILayout.Button(Strings.Tools.disablePhysBones))
                                    SetPhysBonesEnabledState(SelectedAvatar, false);
                                if(GUILayout.Button(Strings.Tools.enablePhysBones))
                                    SetPhysBonesEnabledState(SelectedAvatar, true);
                            }
                            EditorGUILayout.EndHorizontal();

                            if(DrawToggleButtonGUI(Strings.Tools.togglePhysBones, _nextTogglePBoneState))
                                TogglePhysBonesEnabledState(SelectedAvatar, ref _nextTogglePBoneState, ref _pBonesThatWereAlreadyDisabled);

                            EditorGUILayout.Space();
                        }
                        EditorGUI.EndDisabledGroup();
                    }

                    EditorGUI.EndDisabledGroup();
                    EditorGUILayout.Space();
#endif
                    if(DynamicBonesExist)
                    {
                        //Setup dbone gui stuff
                        if(Settings._tools_dynamicBones_expand = GUILayout.Toggle(Settings._tools_dynamicBones_expand, Strings.Copier.dynamicBones, Styles.Foldout))
                        {
                            EditorGUI.BeginDisabledGroup(!DynamicBonesExist);
                            {
                                EditorGUILayout.BeginHorizontal();
                                {
                                    if(GUILayout.Button(Strings.Tools.disableDynamicBones))
                                        SetDynamicBonesEnabledState(SelectedAvatar, false);
                                    if(GUILayout.Button(Strings.Tools.enableDynamicBones))
                                        SetDynamicBonesEnabledState(SelectedAvatar, true);
                                }
                                EditorGUILayout.EndHorizontal();

                                if(DrawToggleButtonGUI(Strings.Tools.toggleDynamicBones, _nextToggleDBoneState))
                                    ToggleDynamicBonesEnabledState(SelectedAvatar, ref _nextToggleDBoneState, ref _dBonesThatWereAlreadyDisabled);

                                EditorGUILayout.Space();
                            }
                            EditorGUI.EndDisabledGroup();
                        }

                        EditorGUI.EndDisabledGroup();
                        EditorGUILayout.Space();
                    }
                }
            }
        }

        void DrawRemoveComponentsMenuGUI()
        {

            if(Settings._tools_removeAll_expand = GUILayout.Toggle(Settings._tools_removeAll_expand, Strings.Main.removeAll, Styles.Foldout_title))
            {
                EditorGUILayout.Space();
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true)); //Left Column
                    {
                        EditorGUI.BeginDisabledGroup(!PhysBonesExist);
                        {
                            if(GUILayout.Button(new GUIContent(Strings.Copier.physBones, Icons.PhysBone)))
                                DoAction(SelectedAvatar, ToolMenuActions.RemovePhysBones);
                        }
                        EditorGUI.EndDisabledGroup();
                        EditorGUI.BeginDisabledGroup(!DynamicBonesExist);
                        {
                            if(GUILayout.Button(new GUIContent(Strings.Copier.dynamicBones, Icons.BoneIcon)))
                                DoAction(SelectedAvatar, ToolMenuActions.RemoveDynamicBones);
                        }
                        EditorGUI.EndDisabledGroup();
                        EditorGUI.BeginDisabledGroup(!PhysBonesExist);
                        {
                            if(GUILayout.Button(new GUIContent(Strings.Copier.contactReceiver, Icons.ContactReceiver)))
                                DoAction(SelectedAvatar, ToolMenuActions.RemoveContactReceiver);
                            if(GUILayout.Button(new GUIContent(Strings.Copier.vrc_station, Icons.CsScript)))
                                DoAction(SelectedAvatar, ToolMenuActions.RemoveVRCStation);
                        }
                        EditorGUI.EndDisabledGroup();
                        if(GUILayout.Button(new GUIContent(Strings.Copier.particleSystems, Icons.ParticleSystem)))
                            DoAction(SelectedAvatar, ToolMenuActions.RemoveParticleSystems);
                        if(GUILayout.Button(new GUIContent(Strings.Copier.lights, Icons.Light)))
                            DoAction(SelectedAvatar, ToolMenuActions.RemoveLights);
                        if(GUILayout.Button(new GUIContent(Strings.Copier.joints, Icons.Joint)))
                            DoAction(SelectedAvatar, ToolMenuActions.RemoveJoints);
                        if(GUILayout.Button(new GUIContent(Strings.Copier.animators_inChildren, Icons.Animator)))
                            DoAction(SelectedAvatar, ToolMenuActions.RemoveAnimatorsInChildren);
                        if(GUILayout.Button(new GUIContent(Strings.Copier.colliders, Icons.ColliderBox)))
                            DoAction(SelectedAvatar, ToolMenuActions.RemoveColliders);
                        if(GUILayout.Button(new GUIContent(Strings.Copier.cameras, Icons.Camera)))
                            DoAction(SelectedAvatar, ToolMenuActions.RemoveCameras);

                        EditorGUILayout.Space();

                        if(GUILayout.Button(new GUIContent(Strings.Copier.aimConstraints, Icons.AimConstraint)))
                            DoAction(SelectedAvatar, ToolMenuActions.RemoveAimConstraint);
                        if(GUILayout.Button(new GUIContent(Strings.Copier.lookAtConstraints, Icons.LookAtConstraint)))
                            DoAction(SelectedAvatar, ToolMenuActions.RemoveLookAtConstraint);
                        if(GUILayout.Button(new GUIContent(Strings.Copier.parentConstraints, Icons.ParentConstraint)))
                            DoAction(SelectedAvatar, ToolMenuActions.RemoveParentConstraint);

                        EditorGUILayout.Space();
                        if(GUILayout.Button(new GUIContent(Strings.Copier.finalIK_fabrIK, Icons.FinalIK_FabrIK)))
                            DoAction(SelectedAvatar, ToolMenuActions.RemoveFinalIK_FabrIK);
                        if(GUILayout.Button(new GUIContent(Strings.Copier.finalIK_aimIK, Icons.FINALIK_AimIK)))
                            DoAction(SelectedAvatar, ToolMenuActions.RemoveFinalIK_AimIK);
                        if(GUILayout.Button(new GUIContent(Strings.Copier.finalIK_fbtBipedIK, Icons.FinalIK_FbtBipedIK)))
                            DoAction(SelectedAvatar, ToolMenuActions.RemoveFinalIK_FbtBipedIK);
                        if(GUILayout.Button(new GUIContent(Strings.Copier.finalIK_VRIK, Icons.FinalIK_VRIK)))
                            DoAction(SelectedAvatar, ToolMenuActions.RemoveFinalIK_VRIK);
                    }
                    EditorGUILayout.EndVertical();

                    EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true)); //Right Column
                    {
                        EditorGUI.BeginDisabledGroup(!PhysBonesExist);
                        {
                            if(GUILayout.Button(new GUIContent(Strings.Copier.physBones_colliders, Icons.PhysBoneCollider)))
                                DoAction(SelectedAvatar, ToolMenuActions.RemovePhysBoneColliders);
                        }
                        EditorGUI.EndDisabledGroup();
                        EditorGUI.BeginDisabledGroup(!DynamicBonesExist);
                        {
                            if(GUILayout.Button(new GUIContent(Strings.Copier.dynamicBones_colliders, Icons.BoneColliderIcon)))
                                DoAction(SelectedAvatar, ToolMenuActions.RemoveDynamicBoneColliders);
                        }
                        EditorGUI.BeginDisabledGroup(!PhysBonesExist);
                        {
                            if(GUILayout.Button(new GUIContent(Strings.Copier.contactSender, Icons.ContactSender)))
                                DoAction(SelectedAvatar, ToolMenuActions.RemoveContactSender);
                        }
                        EditorGUI.EndDisabledGroup();
                        EditorGUI.EndDisabledGroup();
                        if(GUILayout.Button(new GUIContent(Strings.Copier.trailRenderers, Icons.TrailRenderer)))
                            DoAction(SelectedAvatar, ToolMenuActions.RemoveTrailRenderers);
                        if(GUILayout.Button(new GUIContent(Strings.Copier.audioSources, Icons.AudioSource)))
                            DoAction(SelectedAvatar, ToolMenuActions.RemoveAudioSources);
                        if(GUILayout.Button(new GUIContent(Strings.Copier.rigidBodies, Icons.RigidBody)))
                            DoAction(SelectedAvatar, ToolMenuActions.RemoveRigidBodies);
                        if(GUILayout.Button(new GUIContent(Strings.Copier.meshRenderers, Icons.MeshRenderer)))
                            DoAction(SelectedAvatar, ToolMenuActions.RemoveMeshRenderers);
                        if(GUILayout.Button(new GUIContent(Strings.Copier.emptyGameObjects, Icons.Prefab)))
                            DoAction(SelectedAvatar, ToolMenuActions.RemoveEmptyGameObjects);
                        if(GUILayout.Button(new GUIContent(Strings.Copier.other_emptyScripts, Icons.SerializableAsset)))
                            DoAction(SelectedAvatar, ToolMenuActions.RemoveMissingScripts);

                        EditorGUILayout.Space(27 * EditorGUIUtility.pixelsPerPoint);

                        if(GUILayout.Button(
                            new GUIContent(Strings.Copier.positionConstraints, Icons.PositionConstraint)))
                            DoAction(SelectedAvatar, ToolMenuActions.RemovePositionConstraint);
                        if(GUILayout.Button(
                            new GUIContent(Strings.Copier.rotationConstraints, Icons.RotationConstraint)))
                            DoAction(SelectedAvatar, ToolMenuActions.RemoveRotationConstraint);
                        if(GUILayout.Button(new GUIContent(Strings.Copier.scaleConstraints, Icons.ScaleConstraint)))
                            DoAction(SelectedAvatar, ToolMenuActions.RemoveScaleConstraint);


                        if(FinalIKExists)
                        {
                            EditorGUILayout.Space();
                            if(GUILayout.Button(new GUIContent(Strings.Copier.finalIK_ccdIK, Icons.FinalIK_CCDIK)))
                                DoAction(SelectedAvatar, ToolMenuActions.RemoveFinalIK_CCDIK);
                            if(GUILayout.Button(new GUIContent(Strings.Copier.finalIK_limbIK, Icons.FINALIK_LimbIK)))
                                DoAction(SelectedAvatar, ToolMenuActions.RemoveFinalIK_LimbIK);
                            if(GUILayout.Button(new GUIContent(Strings.Copier.finalIK_rotationLimits, Icons.FinalIK_RotationLimits)))
                                DoAction(SelectedAvatar, ToolMenuActions.RemoveFinalIK_RotationLimits);
                            if(GUILayout.Button(new GUIContent(Strings.Copier.finalIK_Grounders, Icons.FinalIK_Grounder)))
                                DoAction(SelectedAvatar, ToolMenuActions.RemoveFinalIK_Grounder);
                        }
                    }
                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space();

                Settings.bCopier_ignorePrefabComponents = EditorGUILayout.ToggleLeft(Strings.Copier.ignorePrefabComponents, Settings.bCopier_ignorePrefabComponents);
            }
        }

        bool DrawToggleButtonGUI(string text, bool toggleBool)
        {
            bool b = GUILayout.Button(new GUIContent(text, toggleBool ? Icons.ToggleOff : Icons.ToggleOn), Styles.ButtonWithToggle);
            return b;
        }

        public void DrawPresetGUI<T>() where T : PumkinPreset
        {
            List<PumkinPreset> pr = new List<PumkinPreset>();
            string labelString = "Preset";
            IEnumerable<string> dropdownOptions = new List<string>();

            SerializedProperty pSelectedPresetString = null;
            SerializedProperty pSelectedPresetIndex = null;

            if(typeof(T) == typeof(PumkinsCameraPreset))
            {
                pSelectedPresetString = Settings.SerializedSettings.FindProperty("_selectedCameraPresetString");
                pSelectedPresetIndex = Settings.SerializedSettings.FindProperty("_selectedCameraPresetIndex");

                pr = PumkinsPresetManager.CameraPresets.Cast<PumkinPreset>().ToList();

                labelString = Strings.Thumbnails.cameras;
                dropdownOptions = PumkinsPresetManager.CameraPresets.Select(o => o.name);
            }
            else if(typeof(T) == typeof(PumkinsPosePreset))
            {
                pSelectedPresetString = Settings.SerializedSettings.FindProperty("_selectedPosePresetString");
                pSelectedPresetIndex = Settings.SerializedSettings.FindProperty("_selectedPosePresetIndex");

                pr = PumkinsPresetManager.PosePresets.Cast<PumkinPreset>().ToList();

                labelString = Strings.Thumbnails.poses;
                dropdownOptions = PumkinsPresetManager.PosePresets.Select(o => o.name);
            }
            else if(typeof(T) == typeof(PumkinsBlendshapePreset))
            {
                pSelectedPresetString = Settings.SerializedSettings.FindProperty("_selectedBlendshapePresetString");
                pSelectedPresetIndex = Settings.SerializedSettings.FindProperty("_selectedBlendshapePresetIndex");

                pr = PumkinsPresetManager.BlendshapePresets.Cast<PumkinPreset>().ToList();

                labelString = Strings.Thumbnails.blendshapes;
                dropdownOptions = PumkinsPresetManager.BlendshapePresets.Select(o => o.name);
            }

            if(pSelectedPresetIndex.intValue == -1)
                RefreshPresetIndex<T>();

            bool shouldDisable = !SelectedAvatar || (pr.Count > 0 && pSelectedPresetIndex.intValue >= pr.Count && pr[pSelectedPresetIndex.intValue] == null);

            Helpers.DrawGUILine();

            GUILayout.BeginHorizontal();
            {
                EditorGUI.BeginChangeCheck();
                {
                    pSelectedPresetIndex.intValue = EditorGUILayout.Popup(labelString, pSelectedPresetIndex.intValue, dropdownOptions.ToArray(), Styles.Popup);
                }
                if(EditorGUI.EndChangeCheck())
                {
                    pSelectedPresetString.stringValue = pr[pSelectedPresetIndex.intValue].ToString() ?? "";
                }

                if(GUILayout.Button(Icons.Refresh, Styles.IconButton))
                {
                    PumkinsPresetManager.LoadPresets<T>();
                }
            }
            GUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUI.BeginDisabledGroup(pr.Count == 0 || shouldDisable);
            {
                GUILayout.BeginHorizontal();
                {
                    if(GUILayout.Button(Strings.Buttons.edit))
                    {
                        int newIndex = PumkinsPresetManager.GetPresetIndex<T>(pSelectedPresetString.stringValue);
                        if(newIndex == -1)
                            RefreshPresetStringByIndex<T>(pSelectedPresetIndex.intValue);
                        else
                            pSelectedPresetIndex.intValue = newIndex;

                        pr[pSelectedPresetIndex.intValue].ApplyPreset(SelectedAvatar);

                        if(typeof(T) == typeof(PumkinsCameraPreset))
                            CreateCameraPresetPopup.ShowWindow(pr[pSelectedPresetIndex.intValue] as PumkinsCameraPreset);
                        else if(typeof(T) == typeof(PumkinsPosePreset))
                            CreatePosePresetPopup.ShowWindow(pr[pSelectedPresetIndex.intValue] as PumkinsPosePreset);
                        else if(typeof(T) == typeof(PumkinsBlendshapePreset))
                        {
                            CreateBlendshapePopup.ShowWindow(pr[pSelectedPresetIndex.intValue] as PumkinsBlendshapePreset);
                            PumkinsAvatarTools.SetupBlendeshapeRendererHolders(SelectedAvatar);
                        }
                    }
                    if(GUILayout.Button(Strings.Buttons.load))
                    {
                        if(typeof(T) == typeof(PumkinsBlendshapePreset))
                            Instance.DoAction(SelectedAvatar, ToolMenuActions.RevertBlendshapes);

                        int newIndex = PumkinsPresetManager.GetPresetIndex<T>(pSelectedPresetString.stringValue);
                        if(newIndex == -1)
                            RefreshPresetStringByIndex<T>(pSelectedPresetIndex.intValue);
                        else
                            pSelectedPresetIndex.intValue = newIndex;

                        pr[pSelectedPresetIndex.intValue].ApplyPreset(SelectedAvatar);

                        if(typeof(T) == typeof(PumkinsBlendshapePreset))
                            SetupBlendeshapeRendererHolders(SelectedAvatar);
                    }
                }
                GUILayout.EndHorizontal();
            }
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(pr.Count == 0);
            {
                if(GUILayout.Button(Strings.Buttons.selectInAssets))
                {
                    var asset = pr[pSelectedPresetIndex.intValue];
                    if(asset)
                    {
                        Helpers.SelectAndPing(asset);
                    }
                }
            }
            EditorGUI.EndDisabledGroup();

            if(GUILayout.Button(Strings.Buttons.selectFolder))
            {
                string path = null;
                if(typeof(T) == typeof(PumkinsCameraPreset))
                    path = PumkinsPresetManager.localCamerasPath;
                else if(typeof(T) == typeof(PumkinsPosePreset))
                    path = PumkinsPresetManager.localPosesPath;
                else if(typeof(T) == typeof(PumkinsBlendshapePreset))
                    path = PumkinsPresetManager.localBlendshapesPath;
                Helpers.SelectAndPing(path);
            }

            Helpers.DrawGUILine();

            EditorGUI.BeginDisabledGroup(!SelectedAvatar);
            {
                if(GUILayout.Button(Strings.Buttons.createNewPreset, Styles.BigButton))
                {
                    if(typeof(T) == typeof(PumkinsCameraPreset))
                        CreateCameraPresetPopup.ShowWindow();
                    else if(typeof(T) == typeof(PumkinsPosePreset))
                        CreatePosePresetPopup.ShowWindow();
                    else if(typeof(T) == typeof(PumkinsBlendshapePreset))
                        CreateBlendshapePopup.ShowWindow();
                }

                if(GUILayout.Button(Strings.Buttons.reset))
                {
#if VRC_SDK_VRCSDK3
                    if(typeof(T) == typeof(PumkinsCameraPreset))
                        CenterCameraOnViewpoint(SelectedAvatar, DEFAULT_CAMERA_POSITION_OFFSET_VIEWPOINT,
                            DEFAULT_CAMERA_ROTATION_OFFSET_VIEWPOINT, Settings.centerCameraFixClippingPlanes);
                    else
#endif
                    if(typeof(T) == typeof(PumkinsPosePreset))
                        DoAction(SelectedAvatar, ToolMenuActions.ResetPose);
                    else if(typeof(T) == typeof(PumkinsBlendshapePreset))
                    {
                        DoAction(SelectedAvatar, ToolMenuActions.RevertBlendshapes);
                        PumkinsAvatarTools.SetupBlendeshapeRendererHolders(SelectedAvatar);
                    }
                }
            }
            EditorGUI.EndDisabledGroup();

            Helpers.DrawGUILine();

            Settings.SerializedSettings.ApplyModifiedProperties();
        }

        /// <summary>
        /// Draws the "Use Background" part of the thumbnail menu
        /// </summary>
        public void DrawBackgroundGUI()
        {
            bool needsRefresh = false;
            RawImage raw = _cameraBackgroundImage; //GetCameraBackgroundRawImage(false);
            GameObject background = _cameraBackground; //GetCameraBackground();

            EditorGUI.BeginDisabledGroup(!_selectedCamera);
            if(Helpers.DrawDropdownWithToggle(ref Settings._thumbnails_useCameraBackground_expand,
                ref Settings.bThumbnails_use_camera_background, Strings.Thumbnails.useCameraBackground))
            {
                RefreshBackgroundOverrideType();
                needsRefresh = true;

                if(Settings.bThumbnails_use_camera_background && _selectedCamera)
                    Settings._thumbsCameraBgClearFlagsOld = SelectedCamera.clearFlags;
                else
                    RestoreCameraClearFlags();
            }
            EditorGUI.EndDisabledGroup();

            if(Settings._thumbnails_useCameraBackground_expand || needsRefresh)
            {
                EditorGUILayout.Space();
                EditorGUI.BeginDisabledGroup(!_selectedCamera);
                {
                    EditorGUI.BeginChangeCheck();
                    {
                        cameraBackgroundType = (PumkinsCameraPreset.CameraBackgroundOverrideType)EditorGUILayout.EnumPopup(Strings.Thumbnails.backgroundType, cameraBackgroundType);
                    }
                    if(EditorGUI.EndChangeCheck())
                    {
                        RefreshBackgroundOverrideType();
                    }
                    EditorGUILayout.Space();

                    switch(cameraBackgroundType)
                    {
                        case PumkinsCameraPreset.CameraBackgroundOverrideType.Color:
                            {
                                EditorGUI.BeginChangeCheck();
                                {
                                    Settings._thumbsCamBgColor = EditorGUILayout.ColorField(Strings.Thumbnails.backgroundType_Color, _selectedCamera ? SelectedCamera.backgroundColor : Color.grey);
                                }
                                if(EditorGUI.EndChangeCheck())
                                {
                                    SetCameraBackgroundToColor(Settings._thumbsCamBgColor);
                                }
                            }
                            break;
                        case PumkinsCameraPreset.CameraBackgroundOverrideType.Skybox:
                            {
                                if(Settings.bThumbnails_use_camera_background && _selectedCamera)
                                {
                                    SelectedCamera.clearFlags = CameraClearFlags.Skybox;
                                }

                                Material mat = RenderSettings.skybox;
                                EditorGUI.BeginChangeCheck();
                                {
                                    mat = EditorGUILayout.ObjectField(Strings.Thumbnails.backgroundType_Material, mat, typeof(Material), true) as Material;
                                }
                                if(EditorGUI.EndChangeCheck())
                                {
                                    SetCameraBackgroundToSkybox(mat);
                                }
                            }
                            break;
                        case PumkinsCameraPreset.CameraBackgroundOverrideType.Image:
                            {
                                if(Settings.bThumbnails_use_camera_background && _selectedCamera)
                                    SelectedCamera.clearFlags = Settings._thumbsCameraBgClearFlagsOld;

                                EditorGUILayout.Space();
                                GUILayout.BeginHorizontal();
                                {
                                    EditorGUILayout.SelectableLabel(Settings._backgroundPath, Styles.TextField);
                                    if(GUILayout.Button(Strings.Buttons.browse, GUILayout.MaxWidth(60)) && SelectedCamera)
                                    {
                                        string newPath = Helpers.OpenImageGetPath(Settings._lastOpenFilePath);
                                        if(!string.IsNullOrEmpty(newPath))
                                        {
                                            Settings._lastOpenFilePath = newPath;
                                            SetBackgroundToImageFromPath(Settings._lastOpenFilePath);
                                        }
                                    }
                                    if(GUILayout.Button("X", GUILayout.MaxWidth(25)))
                                    {
                                        Settings._backgroundPath = null;
                                        SetBackgroundToImageFromTexture((Texture2D)null);
                                    }
                                }
                                EditorGUILayout.EndHorizontal();

                                EditorGUI.BeginDisabledGroup(!cameraBackgroundTexture);
                                {
                                    EditorGUI.BeginChangeCheck();
                                    {
                                        Settings.cameraBackgroundImageTint = EditorGUILayout.ColorField(Strings.Thumbnails.tint, Settings.cameraBackgroundImageTint);
                                    }
                                    if(EditorGUI.EndChangeCheck())
                                    {
                                        if(raw)
                                            raw.color = Settings.cameraBackgroundImageTint;
                                    }
                                }
                                EditorGUI.EndDisabledGroup();
                            }
                            break;
                        default:
                            break;
                    }
                }
                EditorGUI.EndDisabledGroup();
            }
        }

        /// <summary>
        /// Draws the "Use Overlay" section in the thumbnails menu
        /// </summary>
        public void DrawOverlayGUI()
        {
            bool needsRefresh = false;
            RawImage raw = _cameraOverlayImage; //GetCameraOverlayRawImage(false);
            GameObject overlay = _cameraOverlay; //GetCameraOverlay(false);

            EditorGUI.BeginDisabledGroup(!_selectedCamera);
            if(Helpers.DrawDropdownWithToggle(ref Settings._thumbnails_useCameraOverlay_expand,
                ref Settings.bThumbnails_use_camera_overlay, Strings.Thumbnails.useCameraOverlay))
            {
                if(cameraOverlayTexture == null && !string.IsNullOrEmpty(Settings._overlayPath))
                    SetOverlayToImageFromPath(Settings._overlayPath);

                needsRefresh = true;
            }
            EditorGUI.EndDisabledGroup();

            if(Settings._thumbnails_useCameraOverlay_expand || needsRefresh)
            {
                EditorGUI.BeginDisabledGroup(!Settings.bThumbnails_use_camera_overlay);
                {
                    EditorGUILayout.Space();
                    GUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.SelectableLabel(Settings._overlayPath, Styles.TextField);
                        if(GUILayout.Button(Strings.Buttons.browse, GUILayout.MaxWidth(60)) && SelectedCamera)
                        {
                            string newPath = Helpers.OpenImageGetPath(Settings._lastOpenFilePath);
                            if(!string.IsNullOrEmpty(newPath))
                            {
                                Settings._lastOpenFilePath = newPath;
                                SetOverlayToImageFromPath(Settings._lastOpenFilePath);
                            }
                        }
                        if(GUILayout.Button("X", GUILayout.MaxWidth(25)))
                        {
                            Settings._overlayPath = null;
                            SetOverlayToImageFromTexture(null);
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUI.BeginDisabledGroup(!cameraOverlayTexture);
                    {
                        EditorGUI.BeginChangeCheck();
                        {
                            Settings.cameraOverlayImageTint = EditorGUILayout.ColorField(Strings.Thumbnails.tint, Settings.cameraOverlayImageTint);
                        }
                        if(EditorGUI.EndChangeCheck())
                        {
                            if(raw)
                                raw.color = Settings.cameraOverlayImageTint;
                        }
                    }
                    EditorGUI.EndDisabledGroup();
                }
                EditorGUI.EndDisabledGroup();
            }
        }
    }
}