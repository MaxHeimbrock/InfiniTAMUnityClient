#if UNITY_5 || UNITY_5_3_OR_NEWER
using UnityEngine;
using UnityEditor;

using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System;

namespace AnimusUnity
{
    class AnimusImportMenu : MonoBehaviour
    {
        static readonly string SYMBOL_ANIMUS_USE_UNSAFE_CODE = "ANIMUS_USE_UNSAFE_CODE";
        static readonly string SYMBOL_ANIMUS_USE_OPENCV = "ANIMUS_USE_OPENCV";
        [MenuItem("Tools/Animus Client Tools/Use Unsafe Code", validate = true, priority = 10)]
        static bool ValidateUseUnsafeCode()
        {

            Menu.SetChecked("Tools/Animus Client Tools/Use Unsafe Code", PlayerSettings.allowUnsafeCode && EditorUserBuildSettings.activeScriptCompilationDefines.Contains(SYMBOL_ANIMUS_USE_UNSAFE_CODE));
            return true;
        }
        
        [MenuItem("Tools/Animus Client Tools/Apply Recommended Build Settings", validate = false, priority = 10)]
        static bool SetRecommendedSettings()
        {
            PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.Standalone, ApiCompatibilityLevel.NET_4_6);
            PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.Android, ApiCompatibilityLevel.NET_4_6);
            PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.iOS, ApiCompatibilityLevel.NET_4_6);
            PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.tvOS, ApiCompatibilityLevel.NET_4_6);
            PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.WebGL, ApiCompatibilityLevel.NET_4_6);
        
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.Mono2x);
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.Mono2x);
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.iOS, ScriptingImplementation.Mono2x);
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.tvOS, ScriptingImplementation.Mono2x);
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.WebGL, ScriptingImplementation.Mono2x);

            PlayerSettings.Android.forceInternetPermission = true;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel21;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel28;
            Debug.Log("Animus client SDK - Recommended settings applied");
            return true;
        }
        
        [MenuItem("Tools/Animus Client Tools/Use Unsafe Code", validate = false, priority = 10)]
        public static void UseUnsafeCode()
        {
            if (Menu.GetChecked("Tools/Animus Client Tools/Use Unsafe Code"))
            {
                if (EditorUtility.DisplayDialog("Disable Unsafe Code",
                    "Do you want to disable Unsafe Code in Animus Client SDK for Unity?", "Yes", "Cancel"))
                {

                    PlayerSettings.allowUnsafeCode = false;
                    RemoveSymbol(SYMBOL_ANIMUS_USE_UNSAFE_CODE);

                    Debug.Log("PlayerSettings.allowUnsafeCode has been set to " + PlayerSettings.allowUnsafeCode + " and \"" + SYMBOL_ANIMUS_USE_UNSAFE_CODE + "\" has been removed from Scripting Define Symbols.");
                }
            }
            else
            {
                if (EditorUtility.DisplayDialog("Enable Unsafe Code",
                    "Do you want to enable Unsafe Code in Animus Client SDK for Unity?", "Yes", "Cancel"))
                {

                    PlayerSettings.allowUnsafeCode = true;
                    AddSymbol(SYMBOL_ANIMUS_USE_UNSAFE_CODE);

                    Debug.Log("PlayerSettings.allowUnsafeCode has been set to " + PlayerSettings.allowUnsafeCode + " and \"" + SYMBOL_ANIMUS_USE_UNSAFE_CODE + "\" has been added to Scripting Define Symbols.");
                }
            }
        }
        
        [MenuItem("Tools/Animus Client Tools/Use OpenCV", validate = true, priority = 13)]
        static bool ValidateUseOpenCV()
        {
            Menu.SetChecked("Tools/Animus Client Tools/Use OpenCV", EditorUserBuildSettings.activeScriptCompilationDefines.Contains(SYMBOL_ANIMUS_USE_OPENCV));
            return true;
        }
        
        [MenuItem("Tools/Animus Client Tools/Use OpenCV", validate = false, priority = 13)]
        public static void UseOpenCV()
        {
            if (Menu.GetChecked("Tools/Animus Client Tools/Use OpenCV"))
            {
                RemoveSymbol(SYMBOL_ANIMUS_USE_OPENCV);
                Debug.Log($"{SYMBOL_ANIMUS_USE_OPENCV} has been removed to Scripting Define Symbols.");
            }
            else
            {
                AddSymbol(SYMBOL_ANIMUS_USE_OPENCV);
                Debug.Log($"{SYMBOL_ANIMUS_USE_OPENCV} has been added to Scripting Define Symbols.");
            }
        }


        /// <summary>
        /// Sets the plugin import settings.
        /// </summary>
        [MenuItem("Tools/Animus Client Tools/Set Plugin Import Settings", false, 1)]
        public static void SetPluginImportSettings()
        {
            string[] client_guids = UnityEditor.AssetDatabase.FindAssets("AnimusClient");
            if (client_guids.Length == 0)
            {
                Debug.LogWarning("SetPluginImportSettings Failed : AnimusClient.cs is missing.");
                return;
            }

            string animusClientFolderPath = AssetDatabase.GUIDToAssetPath(client_guids[0]);
            string pluginsFolderPath = animusClientFolderPath + "/Plugins";


            string[] common_guids = UnityEditor.AssetDatabase.FindAssets("AnimusImportMenu");
            if (common_guids.Length == 0)
            {
                Debug.LogWarning("SetPluginImportSettings Failed : AnimusImportMenu.cs is missing.");
                return;
            }

            string animusCommonFolderPath = AssetDatabase.GUIDToAssetPath(common_guids[0]).Substring(0,
                AssetDatabase.GUIDToAssetPath(common_guids[0]).LastIndexOf("Editor/AnimusImportMenu.cs"));

            string jsonFolderPath = animusCommonFolderPath + "/JsonDotNet/Assemblies";


            //Android
            SetPlugins(GetPluginFilePaths(pluginsFolderPath + "/Android/libs/armeabi-v7a"), null,
                new Dictionary<BuildTarget, Dictionary<string, string>>()
                {
                    {
                        BuildTarget.Android, new Dictionary<string, string>()
                        {
                            {
                                "CPU",
                                "ARMv7"
                            }
                        }
                    }
                });
            SetPlugins(GetPluginFilePaths(pluginsFolderPath + "/Android/libs/arm64-v8a"), null,
                new Dictionary<BuildTarget, Dictionary<string, string>>()
                {
                    {
                        BuildTarget.Android, new Dictionary<string, string>()
                        {
                            {
                                "CPU",
                                "ARM64"
                            }
                        }
                    }
                });
            SetPlugins(GetPluginFilePaths(pluginsFolderPath + "/Android/libs/x86"), null,
                new Dictionary<BuildTarget, Dictionary<string, string>>()
                {
                    {
                        BuildTarget.Android, new Dictionary<string, string>()
                        {
                            {
                                "CPU",
                                "x86"
                            }
                        }
                    }
                });

            SetPlugins(GetPluginFilePaths(pluginsFolderPath + "/iOS"), null,
                new Dictionary<BuildTarget, Dictionary<string, string>>()
                {
                    {
                        BuildTarget.iOS,
                        new Dictionary<string, string>()
                        {
                            {"CoreFoundation", "true"},
                            {"Security", "true"},
                            {"VideoToolbox", "true"},
                        }
                    }
                });

            //iOS
            SetPlugins(GetPluginFilePaths(pluginsFolderPath + "/iOS/armeabi-v7a"), null,
                new Dictionary<BuildTarget, Dictionary<string, string>>()
                {
                    {
                        BuildTarget.iOS, new Dictionary<string, string>()
                        {
                            {
                                "CPU",
                                "ARMv7"
                            },
                            {
                                "AddToEmbeddedBinaries",
                                "true"
                            }
                        }
                    }
                });
            SetPlugins(GetPluginFilePaths(pluginsFolderPath + "/iOS/arm64-v8a"), null,
                new Dictionary<BuildTarget, Dictionary<string, string>>()
                {
                    {
                        BuildTarget.iOS, new Dictionary<string, string>()
                        {
                            {
                                "CPU",
                                "ARM64"
                            },
                            {
                                "AddToEmbeddedBinaries",
                                "true"
                            }
                        }
                    }
                });
            SetPlugins(GetPluginFilePaths(pluginsFolderPath + "/iOS/amd64"), null,
                new Dictionary<BuildTarget, Dictionary<string, string>>()
                {
                    {
                        BuildTarget.iOS, new Dictionary<string, string>()
                        {
                            {
                                "CPU",
                                "x86_64"
                            },
                            {
                                "AddToEmbeddedBinaries",
                                "true"
                            }
                        }
                    }
                });
            //OSX
            SetPlugins(new string[] {pluginsFolderPath + "/macOS/opencvforunity.bundle"},
                new Dictionary<string, string>()
                {
                    {
                        "CPU",
                        "AnyCPU"
                    },
                    {
                        "OS",
                        "OSX"
                    }
                },
                new Dictionary<BuildTarget, Dictionary<string, string>>()
                {
#if UNITY_2017_3_OR_NEWER
                    {
                        BuildTarget.StandaloneOSX, new Dictionary<string, string>()
                        {
                            {
                                "CPU",
                                "AnyCPU"
                            }
                        }
                    }
#else
                    {
                        BuildTarget.StandaloneOSXIntel,new Dictionary<string, string> () { {
                                "CPU",
                                "x86"
                            }
                        }
                    }, {
                        BuildTarget.StandaloneOSXIntel64,new Dictionary<string, string> () { {
                                "CPU",
                                "x86_64"
                            }
                        }
                    }, {
                        BuildTarget.StandaloneOSXUniversal,new Dictionary<string, string> () { {
                                "CPU",
                                "AnyCPU"
                            }
                        }
                    }
#endif
                });
            //Windows
            SetPlugins(GetPluginFilePaths(pluginsFolderPath + "/Windows/x86"),
                new Dictionary<string, string>()
                {
                    {
                        "CPU",
                        "x86"
                    },
                    {
                        "OS",
                        "Windows"
                    }
                },
                new Dictionary<BuildTarget, Dictionary<string, string>>()
                {
                    {
                        BuildTarget.StandaloneWindows, new Dictionary<string, string>()
                        {
                            {
                                "CPU",
                                "x86"
                            }
                        }
                    }
                });
            SetPlugins(GetPluginFilePaths(pluginsFolderPath + "/Windows/x86_64"),
                new Dictionary<string, string>()
                {
                    {
                        "CPU",
                        "x86_64"
                    },
                    {
                        "OS",
                        "Windows"
                    }
                },
                new Dictionary<BuildTarget, Dictionary<string, string>>()
                {
                    {
                        BuildTarget.StandaloneWindows64, new Dictionary<string, string>()
                        {
                            {
                                "CPU",
                                "x86_64"
                            }
                        }
                    }
                });
            // Linux
#if UNITY_2019_2_OR_NEWER
            SetPlugins(GetPluginFilePaths(pluginsFolderPath + "/Linux/x86"), null, null);
#else
            SetPlugins(GetPluginFilePaths(pluginsFolderPath + "/Linux/x86"), new Dictionary<string, string>()
                {
                    {
                        "CPU",
                        "x86"
                    },
                    {
                        "OS",
                        "Linux"
                    }
                },
                new Dictionary<BuildTarget, Dictionary<string, string>>()
                {
                    {
                        BuildTarget.StandaloneLinux64, new Dictionary<string, string>()
                        {
                            {
                                "CPU",
                                "x86"
                            }
                        }
                    },
                }
            );
#endif

            SetPlugins(GetPluginFilePaths(pluginsFolderPath + "/Linux/x86_64"), new Dictionary<string, string>()
                {
                    {
                        "CPU",
                        "x86_64"
                    },
                    {
                        "OS",
                        "Linux"
                    }
                },
                new Dictionary<BuildTarget, Dictionary<string, string>>()
                {
                    {
                        BuildTarget.StandaloneLinux64, new Dictionary<string, string>()
                        {
                            {
                                "CPU",
                                "x86_64"
                            }
                        }
                    },
                }
            );

#if UNITY_2019_1_OR_NEWER
            //SetPlugins(GetPluginFilePaths(pluginsFolderPath + "/Lumin/libs/arm64-v8a"), null,
            //    new Dictionary<BuildTarget, Dictionary<string, string>>()
            //   {
            //        {
            //           BuildTarget.Lumin, new Dictionary<string, string>()
            //          {
            //               {
            //                   "CPU",
            //                  "ARM64"
            //              }
            //          }
            //      }
            //  });
#else
            //SetPlugins(GetPluginFilePaths(pluginsFolderPath + "/Lumin/libs/arm64-v8a"), null, null);
#endif
        }

        /// <summary>
        /// Gets the plugin file paths.
        /// </summary>
        /// <returns>The plugin file paths.</returns>
        /// <param name="folderPath">Folder path.</param>
        static string[] GetPluginFilePaths (string folderPath)
        {
            Regex reg = new Regex (".meta$|.DS_Store$|.zip");
            try {
                return Directory.GetFiles (folderPath).Where (f => !reg.IsMatch (f)).ToArray ();
            } catch (Exception ex) {
                Debug.LogWarning ("SetPluginImportSettings Failed :" + ex);
                return null;
            }
        }

        /// <summary>
        /// Sets the plugins.
        /// </summary>
        /// <param name="files">Files.</param>
        /// <param name="editorSettings">Editor settings.</param>
        /// <param name="settings">Settings.</param>
        public static void SetPlugins (string[] files, Dictionary<string, string> editorSettings, Dictionary<BuildTarget, Dictionary<string, string>> settings)
        {
            if (files == null)
                return;
            
            foreach (string item in files) {
                
                PluginImporter pluginImporter = PluginImporter.GetAtPath (item) as PluginImporter;
                
                if (pluginImporter != null) {
                    
                    pluginImporter.SetCompatibleWithAnyPlatform (false);
                    pluginImporter.SetCompatibleWithEditor (false);
                    pluginImporter.SetCompatibleWithPlatform (BuildTarget.Android, false);
                    pluginImporter.SetCompatibleWithPlatform (BuildTarget.iOS, false);
                    pluginImporter.SetCompatibleWithPlatform (BuildTarget.StandaloneWindows, false);
                    pluginImporter.SetCompatibleWithPlatform (BuildTarget.StandaloneWindows64, false);
#if UNITY_2017_3_OR_NEWER
                    pluginImporter.SetCompatibleWithPlatform (BuildTarget.StandaloneOSX, false);
#else
                    pluginImporter.SetCompatibleWithPlatform (BuildTarget.StandaloneOSXIntel, false);
                    pluginImporter.SetCompatibleWithPlatform (BuildTarget.StandaloneOSXIntel64, false);
                    pluginImporter.SetCompatibleWithPlatform (BuildTarget.StandaloneOSXUniversal, false);
#endif
                    pluginImporter.SetCompatibleWithPlatform (BuildTarget.StandaloneLinux64, false);
                    pluginImporter.SetCompatibleWithPlatform (BuildTarget.WSAPlayer, false);
                    pluginImporter.SetCompatibleWithPlatform (BuildTarget.WebGL, false);
                    
                    
                    if (editorSettings != null) {
                        pluginImporter.SetCompatibleWithEditor (true);
                        
                        
                        foreach (KeyValuePair<string, string> pair in editorSettings) {
                            if (pluginImporter.GetEditorData (pair.Key) != pair.Value) {
                                pluginImporter.SetEditorData (pair.Key, pair.Value);
                            }
                        }
                    }
                    
                    if (settings != null) {
                        foreach (KeyValuePair<BuildTarget, Dictionary<string, string>> settingPair in settings) {
                            
                            pluginImporter.SetCompatibleWithPlatform (settingPair.Key, true);
                            if (settingPair.Value != null) {
                                foreach (KeyValuePair<string, string> pair in settingPair.Value) {
                                    if (pluginImporter.GetPlatformData (settingPair.Key, pair.Key) != pair.Value) {
                                        pluginImporter.SetPlatformData (settingPair.Key, pair.Key, pair.Value);
                                    }
                                }
                            }
                            
                        }

#if UNITY_2019_1_OR_NEWER
                        pluginImporter.isPreloaded = true;
#endif
                    }
                    else {
                        pluginImporter.SetCompatibleWithPlatform (BuildTarget.Android, false);
                        pluginImporter.SetCompatibleWithPlatform (BuildTarget.iOS, false);
                        pluginImporter.SetCompatibleWithPlatform (BuildTarget.StandaloneWindows, false);
                        pluginImporter.SetCompatibleWithPlatform (BuildTarget.StandaloneWindows64, false);
#if UNITY_2017_3_OR_NEWER
                        pluginImporter.SetCompatibleWithPlatform (BuildTarget.StandaloneOSX, false);
#else
                        pluginImporter.SetCompatibleWithPlatform (BuildTarget.StandaloneOSXIntel, false);
                        pluginImporter.SetCompatibleWithPlatform (BuildTarget.StandaloneOSXIntel64, false);
                        pluginImporter.SetCompatibleWithPlatform (BuildTarget.StandaloneOSXUniversal, false);
#endif
                        pluginImporter.SetCompatibleWithPlatform (BuildTarget.StandaloneLinux64, false);
                        pluginImporter.SetCompatibleWithPlatform (BuildTarget.WSAPlayer, false);
                        pluginImporter.SetCompatibleWithPlatform (BuildTarget.WebGL, false);

#if UNITY_2019_1_OR_NEWER
                        pluginImporter.isPreloaded = false;
#endif
                    }

                    pluginImporter.SaveAndReimport ();
                    
                    Debug.Log ("SetPluginImportSettings Success :" + item);
                } else {
                    Debug.LogWarning ("SetPluginImportSettings Faild :" + item);
                }
            }
        }
        
        public static void AddSymbol(params string[] symbols) {
            PlayerSettings.allowUnsafeCode = true;
            Symbol.Add(BuildTargetGroup.Standalone, Symbol.GetCurrentSymbols(BuildTargetGroup.Standalone), symbols);
            Symbol.Add(BuildTargetGroup.Android, Symbol.GetCurrentSymbols(BuildTargetGroup.Android), symbols);
            Symbol.Add(BuildTargetGroup.iOS, Symbol.GetCurrentSymbols(BuildTargetGroup.iOS), symbols);
            Symbol.Add(BuildTargetGroup.WebGL, Symbol.GetCurrentSymbols(BuildTargetGroup.WebGL), symbols);
            Symbol.Add(BuildTargetGroup.WSA, Symbol.GetCurrentSymbols(BuildTargetGroup.WSA), symbols);
#if UNITY_2019_1_OR_NEWER
            Symbol.Add(BuildTargetGroup.Lumin, Symbol.GetCurrentSymbols(BuildTargetGroup.Lumin), symbols);
#endif
        }
        
        public static void RemoveSymbol(params string[] symbols) {
            PlayerSettings.allowUnsafeCode = true;
            Symbol.Remove(BuildTargetGroup.Standalone, Symbol.GetCurrentSymbols(BuildTargetGroup.Standalone), symbols);
            Symbol.Remove(BuildTargetGroup.Android, Symbol.GetCurrentSymbols(BuildTargetGroup.Android), symbols);
            Symbol.Remove(BuildTargetGroup.iOS, Symbol.GetCurrentSymbols(BuildTargetGroup.iOS), symbols);
            Symbol.Remove(BuildTargetGroup.WebGL, Symbol.GetCurrentSymbols(BuildTargetGroup.WebGL), symbols);
            Symbol.Remove(BuildTargetGroup.WSA, Symbol.GetCurrentSymbols(BuildTargetGroup.WSA), symbols);
#if UNITY_2019_1_OR_NEWER
            Symbol.Add(BuildTargetGroup.Lumin, Symbol.GetCurrentSymbols(BuildTargetGroup.Lumin), symbols);
#endif
        }
    }

    static class Symbol
        {

            public static IEnumerable<string> GetCurrentSymbols(BuildTargetGroup buildTargetGroup)
            {
                return PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup).Split(';');
            }

            private static void SaveSymbol(BuildTargetGroup buildTargetGroup, IEnumerable<string> currentSymbols)
            {

                var symbols = String.Join(";", currentSymbols.ToArray());

                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, symbols);

            }

            public static void Add(BuildTargetGroup buildTargetGroup, IEnumerable<string> currentSymbols, params string[] symbols)
            {
                currentSymbols = currentSymbols.Except(symbols);
                currentSymbols = currentSymbols.Concat(symbols).Distinct();
                SaveSymbol(buildTargetGroup, currentSymbols);
            }

            public static void Remove(BuildTargetGroup buildTargetGroup, IEnumerable<string> currentSymbols, params string[] symbols)
            {
                currentSymbols = currentSymbols.Except(symbols);
                SaveSymbol(buildTargetGroup, currentSymbols);
            }

            public static void Set(BuildTargetGroup buildTargetGroup, IEnumerable<string> currentSymbols, params string[] symbols)
            {
                SaveSymbol(buildTargetGroup, symbols);
            }

        }
}
#endif
