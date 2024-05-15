// SPDX-FileCopyrightText: 2024 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

#if USING_URP || USING_HDRP
#define USING_SRP
#endif

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
#if USING_SRP
using UnityEngine.Rendering;
#endif

namespace GLTFast.Editor.Tests
{
    static class SetupProject
    {
        const string k_RenderPipelineAssetsPath = "Packages/com.unity.cloud.gltfast/Tests/Runtime/RenderPipelineAssets/";

        static readonly Dictionary<string, ProjectSetup> k_ProjectSetups = new Dictionary<string, ProjectSetup>
        {
            ["default"] = new ProjectSetup(
                    new[] {
                        "com.unity.nuget.newtonsoft-json",
                        "com.unity.modules.unitywebrequesttexture",
                        "com.unity.cloud.draco",
                        "com.unity.cloud.ktx",
                        "com.unity.meshopt.decompress",
                        "com.unity.modules.physics",
                        "com.unity.modules.animation",
                        "com.unity.modules.imageconversion"
                    }),
            ["minimalistic"] = new ProjectSetup(
                    new[] {
                        "com.unity.nuget.newtonsoft-json",
                        "com.unity.modules.unitywebrequesttexture"
                    }),
            ["urp"] = new ProjectSetup(
                    new[] {
                        "com.unity.nuget.newtonsoft-json",
                        "com.unity.modules.unitywebrequesttexture",
                        "com.unity.render-pipelines.universal"
                    }),
            ["hdrp"] = new ProjectSetup(
                    new[] {
                        "com.unity.nuget.newtonsoft-json",
                        "com.unity.modules.unitywebrequesttexture",
                        "com.unity.render-pipelines.high-definition"
                    }),
            ["all_defines"] = new ProjectSetup(
                    new[] {
                        "com.unity.nuget.newtonsoft-json",
                        "com.unity.modules.unitywebrequesttexture",
                        "com.unity.modules.physics",
                        "com.unity.modules.animation",
                        "com.unity.modules.imageconversion",
                        "com.unity.render-pipelines.universal"
                    },
                    new[] {
                        "GLTFAST_EDITOR_IMPORT_OFF",
                        "GLTFAST_SAFE",
                        "GLTFAST_KEEP_MESH_DATA"
                    })
        };

        public static async void ApplySetup()
        {
            var args = Environment.GetCommandLineArgs();
            foreach (var arg in args)
            {
                const string prefix = "glTFastSetup:";
                if (arg.StartsWith(prefix))
                {
                    var name = arg.Substring(prefix.Length);
                    if (k_ProjectSetups.TryGetValue(name, out var setup))
                    {
                        await setup.Apply();
                    }
                    else
                    {
                        throw new ArgumentException($"Test Setup {name} not found!");
                    }
                    break;
                }
            }
        }

        public static void SetRenderPipeline()
        {
            var args = Environment.GetCommandLineArgs();
            foreach (var arg in args)
            {
                const string prefix = "SRP:";
                if (arg.StartsWith(prefix))
                {
                    var subPath = arg.Substring(prefix.Length);
                    var assetPath = $"{k_RenderPipelineAssetsPath}{subPath}.asset";
#if USING_SRP
                    PlayerSettings.colorSpace = ColorSpace.Linear;
                    var asset = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(assetPath);
                    if (asset == null)
                    {
                        throw new InvalidDataException($"Could not find render pipeline asset at {subPath}.");
                    }
                    Debug.Log($"Set default render pipeline to {subPath}");
                    GraphicsSettings.defaultRenderPipeline = asset;
                    break;
#else
                    throw new InvalidOperationException(
                        $"Could not set render pipeline asset ({subPath}): No SRP package installed");
#endif
                }
            }


        }
    }

    class ProjectSetup
    {
        public ProjectSetup(string[] dependencies, string[] defines = null)
        {
            Dependencies = dependencies;
            Defines = defines;
        }

        string[] Dependencies { get; }
        string[] Defines { get; }

        public async Task Apply()
        {
            await InstallDependencies();
            if (Defines != null)
            {
                ApplyScriptingDefines(Defines);
            }
        }

        async Task InstallDependencies()
        {
            foreach (var dependency in Dependencies)
            {
                var request = Client.Add(dependency);
                while (!request.IsCompleted)
                {
                    await Task.Yield();
                }
            }
        }

        static void ApplyScriptingDefines(IEnumerable<string> newDefines)
        {
            var target = EditorUserBuildSettings.activeBuildTarget;
            var group = BuildPipeline.GetBuildTargetGroup(target);

#if UNITY_2021_2_OR_NEWER
            var namedBuildTarget = UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(group);
            var scriptingDefineSymbols = PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget);
#else
            var scriptingDefineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
#endif
            var defines = new HashSet<string>(scriptingDefineSymbols.Split(';'));

            foreach (var define in newDefines)
            {
#if UNITY_2021_2_OR_NEWER
                Debug.Log($"Adding scripting define {define} ({namedBuildTarget}).");
#else
                Debug.Log($"Adding scripting define {define} ({group}).");
#endif
                Debug.Log($"Adding scripting define {define} ({group}).");
                defines.Add(define);
            }
#if UNITY_2021_2_OR_NEWER
            PlayerSettings.SetScriptingDefineSymbols(namedBuildTarget, defines.ToArray());
#else
            PlayerSettings.SetScriptingDefineSymbolsForGroup(group, defines.ToArray());
#endif
        }
    }
}
