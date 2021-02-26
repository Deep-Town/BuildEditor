using UnityEngine;
using UnityEditor;
using Unity.XR.Oculus;
using UnityEditor.XR.Management;
using UnityEngine.XR.Management;
using static DeepTown.PlatformManager;
using static DeepTown.GlobalData;
using DeepTown.Utils;

namespace DeepTown.Editor
{
    public class BuildEditor : EditorWindow
    {

        private static OculusLoader oculusSettings
        {
            get
            {
                OculusLoader generalSettings = null;
                EditorBuildSettings.TryGetConfigObject(XRGeneralSettings.k_SettingsKey, out generalSettings);
                if (generalSettings == null)
                {
                    EditorBuildSettings.TryGetConfigObject(XRGeneralSettings.k_SettingsKey, out generalSettings);
                    if (generalSettings == null)
                    {
                        string searchText = "t:OculusLoader";
                        string[] assets = AssetDatabase.FindAssets(searchText);
                        if (assets.Length > 0)
                        {
                            string path = AssetDatabase.GUIDToAssetPath(assets[0]);
                            generalSettings = AssetDatabase.LoadAssetAtPath(path, typeof(OculusLoader)) as OculusLoader;
                        }
                    }
                }
                return generalSettings;
            }
        }

        private static XRGeneralSettingsPerBuildTarget currentSettings
        {
            get
            {
                XRGeneralSettingsPerBuildTarget generalSettings = null;
                EditorBuildSettings.TryGetConfigObject(XRGeneralSettings.k_SettingsKey, out generalSettings);
                if (generalSettings == null)
                {
                    EditorBuildSettings.TryGetConfigObject(XRGeneralSettings.k_SettingsKey, out generalSettings);
                    if (generalSettings == null)
                    {
                        string searchText = "t:XRGeneralSettings";
                        string[] assets = AssetDatabase.FindAssets(searchText);
                        if (assets.Length > 0)
                        {
                            string path = AssetDatabase.GUIDToAssetPath(assets[0]);
                            generalSettings = AssetDatabase.LoadAssetAtPath(path, typeof(XRGeneralSettingsPerBuildTarget)) as XRGeneralSettingsPerBuildTarget;
                        }
                    }

                    EditorBuildSettings.AddConfigObject(XRGeneralSettings.k_SettingsKey, generalSettings, true);
                }
                return generalSettings;
            }
        }

        [MenuItem("Deep Town/Build Editor")]
        public static void ShowWindow()
        {
            //Create new build-data.json if doesn't exist
            if (!FileUtils.FileExists(FileUtils.GetStreammingAssetsPath("build-data.json")))
            {
                FileUtils.CreateFile(FileUtils.GetStreammingAssetsPath("build-data.json"), 
                    new string[] { "{", "\"platform\": \"Windows\"", "}" }); //Default to Windows
                AssetDatabase.Refresh();
            }

            switch (EditorUserBuildSettings.activeBuildTarget)
            {
                case BuildTarget.StandaloneWindows:
                    activePlatform = Platform.Windows;
                    break;
                case BuildTarget.Android:
                    bool isQuest = CheckQuest();
                    if (isQuest)
                        activePlatform = Platform.Quest;
                    else
                        activePlatform = Platform.Android;
                    break;
                case BuildTarget.iOS:
                    activePlatform = Platform.IOS;
                    break;
                default:
                    EditorUtility.DisplayDialog("Error", "New Platform detected! Update as per requirement.", "Continue");
                    break;
            }

            var w = GetWindow<BuildEditor>(false, "Build Editor", true);
            w.minSize = new Vector2(500, 500);
            w.maxSize = new Vector2(750, 5000);
        }

        void OnGUI()
        {
            GUIStyle labelStyle = new GUIStyle
            {
                fontSize = 15,
                alignment = TextAnchor.UpperLeft
            };

            GUIStyle activePlatformStyle = new GUIStyle
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.UpperCenter
            };

            GUIStyle warnStyle = new GUIStyle
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.UpperCenter
            };
            warnStyle.normal.textColor = Color.red;

            GUIStyle headerStyle = new GUIStyle
            {
                fontSize = 30,
                alignment = TextAnchor.UpperCenter
            };

            GUIStyle companyStyle = new GUIStyle
            {
                fontSize = 15,
                alignment = TextAnchor.UpperCenter
            };

            //Adjust with selected editor theme
            if (EditorGUIUtility.isProSkin)
            {
                headerStyle.normal.textColor = Color.white;
                companyStyle.normal.textColor = Color.white;
                labelStyle.normal.textColor = Color.white;
                activePlatformStyle.normal.textColor = Color.white;
            }

            if (EditorApplication.isCompiling || EditorApplication.isPlaying) return;

            //Header
            GUILayout.BeginVertical();
            GUI.Label(new Rect(position.width / 4, 0, (position.width / 2), 50), "Build Editor", headerStyle);
            GUI.Label(new Rect(position.width / 4, 30, (position.width / 2), 50), "Deep Town", companyStyle);
            GUILayout.EndHorizontal();


            //Build Target
            GUILayout.BeginVertical();
            GUI.Label(new Rect(10, 55, (position.width / 2), 50), "Change Build Target", labelStyle);
            if (GUI.Button(new Rect(10, 80, position.width - 25, 50), "Windows Standalone"))
            {
                SwitchWindows();
            }

            if (GUI.Button(new Rect(10, 140, position.width - 25, 50), "Android (Regular/AR)"))
            {
                SwitchAndroidRegular();
            }

            if (GUI.Button(new Rect(10, 200, position.width - 25, 50), "Quest"))
            {
                SwitchQuest();
            }

            if (GUI.Button(new Rect(10, 260, position.width - 25, 50), "IOS"))
            {
                SwitchIos();
            }
            GUILayout.EndVertical();

            //Platform Dependent Setting
            GUILayout.BeginVertical();
            GUI.Label(new Rect(10, 325, (position.width / 2), 50), "Update Platform Dependent Settings", labelStyle);
            if (GUI.Button(new Rect(10, 350, position.width - 25, 50), "Update"))
            {
                UpdateSettings();
            }

            GUI.Label(new Rect(position.width / 4, 410, (position.width / 2), 50), $"Active Platform: {activePlatform}", activePlatformStyle);

            CheckDependency();

            GUILayout.EndVertical();
            this.Repaint();
        }

        void CheckDependency()
        {
            GUIStyle warnStyle = new GUIStyle
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.UpperCenter
            };
            warnStyle.normal.textColor = Color.red;

            if (!FileUtils.FileExists(FileUtils.GetStreammingAssetsPath("build-data.json")))
            {
                GUI.Label(new Rect(position.width / 4, 430, (position.width / 2), 50), "build-data.json is missing from Streaming Assets folder.", warnStyle);
                return;
            }

            if (activePlatform.ToString() != LoadMeta())
                GUI.Label(new Rect(position.width / 4, 430, (position.width / 2), 50), "Build Target was changed traditionally, please Select your Platform again from here.", warnStyle);
            else if (activePlatform == Platform.Undefined)
                GUI.Label(new Rect(position.width / 4, 430, (position.width / 2), 50), "Target is Undefined! To Resolve, Select your Platform again.", warnStyle);
        }

        void SwitchWindows()
        {
            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows64)
            {
                EditorUtility.DisplayDialog("Error", "Your selected Build Target is already active!", "Continue");
                return;
            }
            activePlatform = Platform.Windows;
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
            UpdateSettings();
        }

        void SwitchAndroidRegular()
        {
            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android && activePlatform == Platform.Android)
            {
                EditorUtility.DisplayDialog("Error", "Your selected Build Target is already active!", "Continue");
                return;
            }
            activePlatform = Platform.Android;
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            UpdateSettings();
        }

        void SwitchQuest()
        {
            if (activePlatform == Platform.Quest)
            {
                EditorUtility.DisplayDialog("Error", "Your selected Build Target is already active!", "Continue");
                return;
            }
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            }
            activePlatform = Platform.Quest;
            UpdateSettings();
        }

        void SwitchIos()
        {
            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS)
            {
                EditorUtility.DisplayDialog("Error", "Your selected Build Target is already active!", "Continue");
                return;
            }
            activePlatform = Platform.IOS; 
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS, BuildTarget.iOS);
            UpdateSettings();
        }

        void UpdateSettings()
        {
            SaveMeta(activePlatform.ToString()); //Save Meta

            //Windows
            if (activePlatform == Platform.Windows)
            {
                PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;
                PlayerSettings.visibleInBackground = true;
            }

            //Common Android Settings
            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
            {
                PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
                PlayerSettings.Android.forceSDCardPermission = true;
            }

            //Quest
            if (activePlatform == Platform.Quest)
            {
                //Change Texture Comperession
                EditorUserBuildSettings.androidBuildSubtarget = MobileTextureSubtarget.ASTC;

                //Get XR General Settings
                XRGeneralSettings settings = currentSettings.SettingsForBuildTarget(EditorUserBuildSettings.selectedBuildTargetGroup);
                var serializedSettingsObject = new SerializedObject(settings);
                serializedSettingsObject.Update();

                //Set Initialize XR on Startup
                SerializedProperty initOnStart = serializedSettingsObject.FindProperty("m_InitManagerOnStart");
                initOnStart.intValue = 1;

                //Get loader according to platform
                SerializedProperty loaderProp = serializedSettingsObject.FindProperty("m_LoaderManagerInstance");
                if (loaderProp.objectReferenceValue == null)
                {
                    var xrManagerSettings = ScriptableObject.CreateInstance<XRManagerSettings>() as XRManagerSettings;
                    xrManagerSettings.name = $"{EditorUserBuildSettings.selectedBuildTargetGroup.ToString()} Providers";
                    AssetDatabase.AddObjectToAsset(xrManagerSettings, AssetDatabase.GetAssetOrScenePath(currentSettings));
                    loaderProp.objectReferenceValue = xrManagerSettings;
                    serializedSettingsObject.ApplyModifiedProperties();
                }

                var obj = loaderProp.objectReferenceValue;

                if (obj != null)
                {
                    loaderProp.objectReferenceValue = obj;
                }
                else if (obj == null)
                {
                    settings.AssignedSettings = null;
                    loaderProp.objectReferenceValue = null;
                }

                //Add Oculus to loader
                var oculusProp = new SerializedObject(loaderProp.objectReferenceValue);
                oculusProp.Update();

                //If we already have this, don't add anymore.
                if (oculusProp.FindProperty("m_Loaders").arraySize == 1)
                    if (oculusProp.FindProperty("m_Loaders").GetArrayElementAtIndex(0).objectReferenceValue == oculusSettings)
                        return;

                oculusProp.FindProperty("m_Loaders").arraySize = 0;
                oculusProp.FindProperty("m_Loaders").InsertArrayElementAtIndex(0);
                oculusProp.FindProperty("m_Loaders").GetArrayElementAtIndex(0).objectReferenceValue = oculusSettings;
                oculusProp.ApplyModifiedProperties();
                serializedSettingsObject.ApplyModifiedProperties();
            }
            else if (activePlatform == Platform.Android)
            {
                //Change Texture Comperession
                EditorUserBuildSettings.androidBuildSubtarget = MobileTextureSubtarget.Generic;

                XRGeneralSettings settings = currentSettings.SettingsForBuildTarget(EditorUserBuildSettings.selectedBuildTargetGroup);
                var serializedSettingsObject = new SerializedObject(settings);
                serializedSettingsObject.Update();

                //Set Initialize XR on Startup
                SerializedProperty initOnStart = serializedSettingsObject.FindProperty("m_InitManagerOnStart");
                initOnStart.intValue = 0;

                //Get loader according to platform
                SerializedProperty loaderProp = serializedSettingsObject.FindProperty("m_LoaderManagerInstance");
                if (loaderProp.objectReferenceValue == null)
                {
                    var xrManagerSettings = ScriptableObject.CreateInstance<XRManagerSettings>() as XRManagerSettings;
                    xrManagerSettings.name = $"{EditorUserBuildSettings.selectedBuildTargetGroup.ToString()} Providers";
                    AssetDatabase.AddObjectToAsset(xrManagerSettings, AssetDatabase.GetAssetOrScenePath(currentSettings));
                    loaderProp.objectReferenceValue = xrManagerSettings;
                    serializedSettingsObject.ApplyModifiedProperties();
                }

                var obj = loaderProp.objectReferenceValue;

                if (obj != null)
                {
                    loaderProp.objectReferenceValue = obj;
                }
                else if (obj == null)
                {
                    settings.AssignedSettings = null;
                    loaderProp.objectReferenceValue = null;
                }

                //Add Oculus to loader
                var oculusProp = new SerializedObject(loaderProp.objectReferenceValue);
                oculusProp.Update();

                //If we already have this, don't add anymore.
                if (oculusProp.FindProperty("m_Loaders").arraySize == 0)
                    return;

                oculusProp.FindProperty("m_Loaders").arraySize = 0;
                oculusProp.ApplyModifiedProperties();
                serializedSettingsObject.ApplyModifiedProperties();
            }
            EditorUtility.DisplayDialog("Successfull", "Successfully updated all relevent setting for selected Build Target.", "Continue");
        }

        void SaveMeta(string meta)
        {
            SavePlatform(activePlatform);
        }

        static string LoadMeta()
        {
            return GetPlatform().ToString();
        }


        public static bool CheckQuest()
        {
            EditorUserBuildSettings.androidBuildSubtarget = MobileTextureSubtarget.ASTC;

            XRGeneralSettings settings = currentSettings.SettingsForBuildTarget(EditorUserBuildSettings.selectedBuildTargetGroup);
            var serializedSettingsObject = new SerializedObject(settings);
            serializedSettingsObject.Update();

            SerializedProperty initOnStart = serializedSettingsObject.FindProperty("m_InitManagerOnStart");
            if (initOnStart.intValue == 1) return true;

            //Get loader according to platform
            SerializedProperty loaderProp = serializedSettingsObject.FindProperty("m_LoaderManagerInstance");
            if (loaderProp.objectReferenceValue == null)
            {
                var xrManagerSettings = ScriptableObject.CreateInstance<XRManagerSettings>() as XRManagerSettings;
                xrManagerSettings.name = $"{EditorUserBuildSettings.selectedBuildTargetGroup.ToString()} Providers";
                AssetDatabase.AddObjectToAsset(xrManagerSettings, AssetDatabase.GetAssetOrScenePath(currentSettings));
                loaderProp.objectReferenceValue = xrManagerSettings;
                serializedSettingsObject.ApplyModifiedProperties();
            }

            var obj = loaderProp.objectReferenceValue;

            if (obj != null)
            {
                loaderProp.objectReferenceValue = obj;
            }
            else if (obj == null)
            {
                settings.AssignedSettings = null;
                loaderProp.objectReferenceValue = null;
            }

            //Add Oculus to loader
            var oculusProp = new SerializedObject(loaderProp.objectReferenceValue);
            oculusProp.Update();

            //If we already have this, don't add anymore.
            if (oculusProp.FindProperty("m_Loaders").arraySize > 0) return true;

            return false;
        }
    }
}
