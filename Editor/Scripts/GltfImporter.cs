// Copyright 2020-2022 Andreas Atteneder
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

#if !GLTFAST_EDITOR_IMPORT_OFF

// glTFast is on the path to being official, so it should have highest priority as importer by default
// This define is included for completeness.
// Other glTF importers should specify this via AsmDef dependency, for example
// `com.atteneder.gltfast@3.0.0: HAVE_GLTFAST` and then checking here `#if HAVE_GLTFAST`
#if false
#define ANOTHER_IMPORTER_HAS_HIGHER_PRIORITY
#endif

#if !ANOTHER_IMPORTER_HAS_HIGHER_PRIORITY && !GLTFAST_FORCE_DEFAULT_IMPORTER_OFF
#define ENABLE_DEFAULT_GLB_IMPORTER
#endif
#if GLTFAST_FORCE_DEFAULT_IMPORTER_ON
#define ENABLE_DEFAULT_GLB_IMPORTER
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using GLTFast.Logging;
using GLTFast.Utils;
using UnityEditor;
#if UNITY_2020_2_OR_NEWER
using UnityEditor.AssetImporters;
#else
using UnityEditor.Experimental.AssetImporters;
#endif
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace GLTFast.Editor
{

#if ENABLE_DEFAULT_GLB_IMPORTER
    [ScriptedImporter(1, new[] { "gltf", "glb" })]
#else
    [ScriptedImporter(1, null, overrideExts: new[] { "gltf","glb" })]
#endif
    class GltfImporter : ScriptedImporter
    {

        [SerializeField]
        EditorImportSettings editorImportSettings;

        [SerializeField]
        ImportSettings importSettings;

        [SerializeField]
        InstantiationSettings instantiationSettings;

        // These are used/read in the GltfImporterEditor
        // ReSharper disable NotAccessedField.Local
        [SerializeField]
        GltfAssetDependency[] assetDependencies;

        [SerializeField]
        LogItem[] reportItems;
        // ReSharper restore NotAccessedField.Local

        GltfImport m_Gltf;

        HashSet<string> m_ImportedNames;
        HashSet<Object> m_ImportedObjects;

        // static string[] GatherDependenciesFromSourceFile(string path) {
        //     // Called before actual import for each changed asset that is imported by this importer type
        //     // Extract the dependencies for the asset specified in path.
        //     // For asset dependencies that are discovered, return them in the string array, where the string is the path to asset
        //
        //     // TODO: Texture files with relative URIs should be included here
        //     return null;
        // }

        public override void OnImportAsset(AssetImportContext ctx)
        {

            reportItems = null;

            var downloadProvider = new EditorDownloadProvider();
            var logger = new CollectingLogger();

            m_Gltf = new GltfImport(
                downloadProvider,
                new UninterruptedDeferAgent(),
                null,
                logger
                );

            var gltfIcon = AssetDatabase.LoadAssetAtPath<Texture2D>($"Packages/{GltfGlobals.GltfPackageName}/Editor/UI/gltf-icon-bug.png");

            if (editorImportSettings == null)
            {
                // Design-time import specific settings
                editorImportSettings = new EditorImportSettings();
            }

            if (importSettings == null)
            {
                // Design-time import specific changes to default settings
                importSettings = new ImportSettings
                {
                    // Avoid naming conflicts by default
                    NodeNameMethod = NameImportMethod.OriginalUnique,
                    GenerateMipMaps = true,
                    AnimationMethod = AnimationMethod.Mecanim,
                };
            }

            if (instantiationSettings == null)
            {
                instantiationSettings = new InstantiationSettings();
            }

            var success = AsyncHelpers.RunSync(() => m_Gltf.Load(ctx.assetPath, importSettings));

            CollectingLogger instantiationLogger = null;
            if (success)
            {
                m_ImportedNames = new HashSet<string>();
                m_ImportedObjects = new HashSet<Object>();

                if (instantiationSettings.SceneObjectCreation == SceneObjectCreation.Never)
                {

                    // There *has* to be a common parent GameObject that gets
                    // added to the ScriptedImporter, so we overrule this
                    // setting.

                    instantiationSettings.SceneObjectCreation = SceneObjectCreation.WhenMultipleRootNodes;
                    Debug.LogWarning("SceneObjectCreation setting \"Never\" is not available for Editor (design-time) imports. Falling back to WhenMultipleRootNodes.", this);
                }

                instantiationLogger = new CollectingLogger();
                for (var sceneIndex = 0; sceneIndex < m_Gltf.SceneCount; sceneIndex++)
                {
                    var scene = m_Gltf.GetSourceScene(sceneIndex);
                    var sceneName = m_Gltf.GetSceneName(sceneIndex);
                    var go = new GameObject(sceneName);
                    var instantiator = new GameObjectInstantiator(m_Gltf, go.transform, instantiationLogger, instantiationSettings);
                    var index = sceneIndex;
                    success = AsyncHelpers.RunSync(() => m_Gltf.InstantiateSceneAsync(instantiator, index));
                    if (!success) break;
                    var useFirstChild = true;
                    var multipleNodes = scene.nodes.Length > 1;
                    var hasAnimation = false;
#if UNITY_ANIMATION
                    if (importSettings.AnimationMethod != AnimationMethod.None
                        && (instantiationSettings.Mask & ComponentType.Animation) != 0) {
                        var animationClips = m_Gltf.GetAnimationClips();
                        if (animationClips != null && animationClips.Length > 0) {
                            hasAnimation = true;
                        }
                    }
#endif

                    if (instantiationSettings.SceneObjectCreation == SceneObjectCreation.Never
                        || instantiationSettings.SceneObjectCreation == SceneObjectCreation.WhenMultipleRootNodes && !multipleNodes)
                    {
                        // No scene GameObject was created, so the first
                        // child is the first (and in this case only) node.

                        // If there's animation, its clips' paths are relative
                        // to the root GameObject (which will also carry the
                        // `Animation` component. If not, we can import the the
                        // first and only node as root directly.

                        useFirstChild = !hasAnimation;
                    }

                    var sceneTransform = useFirstChild
                        ? go.transform.GetChild(0)
                        : go.transform;
                    var sceneGo = sceneTransform.gameObject;
                    AddObjectToAsset(ctx, $"scenes/{sceneName}", sceneGo, gltfIcon);
                    if (sceneIndex == m_Gltf.DefaultSceneIndex)
                    {
                        ctx.SetMainObject(sceneGo);
                    }
                }

                for (var i = 0; i < m_Gltf.TextureCount; i++)
                {
                    var texture = m_Gltf.GetTexture(i);
                    if (texture != null)
                    {
                        var textureAssetPath = AssetDatabase.GetAssetPath(texture);
                        if (string.IsNullOrEmpty(textureAssetPath))
                        {
                            AddObjectToAsset(ctx, $"textures/{texture.name}", texture);
                        }
                    }
                }

                for (var i = 0; i < m_Gltf.MaterialCount; i++)
                {
                    var mat = m_Gltf.GetMaterial(i);

                    // Overriding double-sided for GI baking
                    // Resolves problems with meshes that are not a closed
                    // volume at a potential minor cost of baking speed.
                    mat.doubleSidedGI = true;

                    if (mat != null)
                    {
                        AddObjectToAsset(ctx, $"materials/{mat.name}", mat);
                    }
                }

                if (m_Gltf.defaultMaterial != null)
                {
                    // If a default/fallback material was created, import it as well'
                    // to avoid (pink) objects without materials
                    var mat = m_Gltf.defaultMaterial;
                    AddObjectToAsset(ctx, $"materials/{mat.name}", mat);
                }

                var meshes = m_Gltf.GetMeshes();
                if (meshes != null)
                {
                    foreach (var mesh in meshes)
                    {
                        if (mesh == null)
                        {
                            continue;
                        }
                        if (editorImportSettings.generateSecondaryUVSet && !HasSecondaryUVs(mesh))
                        {
                            Unwrapping.GenerateSecondaryUVSet(mesh);
                        }
                        AddObjectToAsset(ctx, $"meshes/{mesh.name}", mesh);
                    }
                }

#if UNITY_ANIMATION
                var clips = m_Gltf.GetAnimationClips();
                if (clips != null) {
                    foreach (var animationClip in clips) {
                        if (animationClip == null) {
                            continue;
                        }
                        if (importSettings.AnimationMethod == AnimationMethod.Mecanim) {
                            var settings = AnimationUtility.GetAnimationClipSettings(animationClip);
                            settings.loopTime = true;
                            AnimationUtility.SetAnimationClipSettings (animationClip, settings);
                        }
                        AddObjectToAsset(ctx, $"animations/{animationClip.name}", animationClip);
                    }
                }

                // TODO seems the states don't properly connect to the Animator here
                // (would need to be saved as SubAssets of the AnimatorController)
                // var animators = go.GetComponentsInChildren<Animator>();
                // foreach (var animator in animators)
                // {
                //     var controller = animator.runtimeAnimatorController as UnityEditor.Animations.AnimatorController;
                //     if (controller != null) {
                //         AddObjectToAsset(ctx, $"animatorControllers/{animator.name}", controller);
                //         foreach (var layer in controller.layers)
                //         {
                //             var stateMachine = layer.stateMachine;
                //             stateMachine.hideFlags = HideFlags.HideInHierarchy;
                //             if(stateMachine)
                //                 AddObjectToAsset(ctx, $"animatorControllers/{animator.name}/{stateMachine.name}", stateMachine);
                //         }
                //     }
                // }
#endif

                m_ImportedNames = null;
                m_ImportedObjects = null;
            }

            var deps = new List<GltfAssetDependency>();
            for (var index = 0; index < downloadProvider.assetDependencies.Count; index++)
            {
                var dependency = downloadProvider.assetDependencies[index];
                if (ctx.assetPath == dependency.originalUri)
                {
                    // Skip original gltf/glb file
                    continue;
                }

                var guid = AssetDatabase.AssetPathToGUID(dependency.originalUri);
                if (!string.IsNullOrEmpty(guid))
                {
                    dependency.assetPath = dependency.originalUri;
                    ctx.DependsOnSourceAsset(dependency.assetPath);
                }

                deps.Add(dependency);
            }

            assetDependencies = deps.ToArray();

            var reportItemList = new List<LogItem>();
            if (logger.Count > 0)
            {
                reportItemList.AddRange(logger.Items);
            }
            if (instantiationLogger?.Items != null)
            {
                reportItemList.AddRange(instantiationLogger.Items);
            }

            if (reportItemList.Any(x => x.Type == LogType.Error || x.Type == LogType.Exception))
            {
                Debug.LogError($"Failed to import {assetPath} (see inspector for details)", this);
            }
            reportItems = reportItemList.ToArray();
        }

        void AddObjectToAsset(AssetImportContext ctx, string originalName, Object obj, Texture2D thumbnail = null)
        {
            if (m_ImportedObjects.Contains(obj))
            {
                return;
            }
            var uniqueAssetName = GetUniqueAssetName(originalName);
            ctx.AddObjectToAsset(uniqueAssetName, obj, thumbnail);
            m_ImportedNames.Add(uniqueAssetName);
            m_ImportedObjects.Add(obj);
        }

        string GetUniqueAssetName(string originalName)
        {
            if (string.IsNullOrWhiteSpace(originalName))
            {
                originalName = "Asset";
            }
            if (m_ImportedNames.Contains(originalName))
            {
                var i = 0;
                string extName;
                do
                {
                    extName = $"{originalName}_{i++}";
                } while (m_ImportedNames.Contains(extName));
                return extName;
            }
            return originalName;
        }

        static bool HasSecondaryUVs(Mesh mesh)
        {
            var attributes = mesh.GetVertexAttributes();
            return attributes.Any(attribute => attribute.attribute == VertexAttribute.TexCoord1);
        }
    }
}

#endif // !GLTFAST_EDITOR_IMPORT_OFF
