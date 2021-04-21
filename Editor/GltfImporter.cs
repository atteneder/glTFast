using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GLTFast.Editor {

    [ScriptedImporter(1,new [] {"gltf","glb"})] 
    public class GltfImporter : ScriptedImporter {

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
        
        public override void OnImportAsset(AssetImportContext ctx) {

            m_Gltf = new GltfImport(
                new EditorDownloadProvider(),
                new UninterruptedDeferAgent()
                );

            var settings = new ImportSettings {
                nodeNameMethod = ImportSettings.NameImportMethod.OriginalUnique
            };

            var success = AsyncHelpers.RunSync<bool>(() => m_Gltf.Load(ctx.assetPath,settings));
            
            if (success) {
                m_ImportedNames = new HashSet<string>();
                m_ImportedObjects = new HashSet<Object>();
                
                var go = new GameObject("root");
                m_Gltf.InstantiateGltf(go.transform);

                var sceneIndex = 0;
                foreach (Transform sceneTransform in go.transform) {
                    var sceneGo = sceneTransform.gameObject;
                    AddObjectToAsset(ctx,$"scenes/{sceneGo.name}", sceneGo);
                    if (sceneIndex == 0) {
                        ctx.SetMainObject(sceneGo);
                    }
                    sceneIndex++;
                }
                
                for (var i = 0; i < m_Gltf.textureCount; i++) {
                    var texture = m_Gltf.GetTexture(i);
                    if (texture != null) {
                        AddObjectToAsset(ctx, $"textures/{texture.name}", texture);
                    }
                }
                
                for (var i = 0; i < m_Gltf.materialCount; i++) {
                    var mat = m_Gltf.GetMaterial(i);
                    AddObjectToAsset(ctx, $"materials/{mat.name}", mat);
                }
                
                var meshes = m_Gltf.GetMeshes();
                if (meshes != null) {
                    foreach (var mesh in meshes) {
                        AddObjectToAsset(ctx, $"meshes/{mesh.name}", mesh);
                    }
                }
                
                var clips = m_Gltf.GetAnimationClips();
                if (clips != null) {
                    foreach (var animationClip in clips) {
                        AddObjectToAsset(ctx, $"animations/{animationClip.name}", animationClip);
                    }
                }
                
                m_ImportedNames = null;
                m_ImportedObjects = null;
            }
        }

        string AddObjectToAsset(AssetImportContext ctx, string originalName, Object obj) {
            if (m_ImportedObjects.Contains(obj)) {
                return null;
            }
            var uniqueAssetName = GetUniqueAssetName(originalName);
            ctx.AddObjectToAsset(uniqueAssetName, obj);
            m_ImportedNames.Add(uniqueAssetName);
            m_ImportedObjects.Add(obj);
            return uniqueAssetName;
        }

        string GetUniqueAssetName(string originalName) {
            if (string.IsNullOrWhiteSpace(originalName)) {
                originalName = "Asset";
            }
            if(m_ImportedNames.Contains(originalName)) {
                var i = 0;
                string extName;
                do {
                    extName = $"{originalName}_{i++}";
                } while (m_ImportedNames.Contains(extName));
                return extName;
            }
            return originalName;
        }
    }

    [ScriptedImporter(1, "testy")]
    public class TestImporter : ScriptedImporter {

        public override void OnImportAsset(AssetImportContext ctx) {
            var root = new GameObject("root");

            var n1 = new GameObject("node");
            var n2 = new GameObject("node2");
            var n3 = new GameObject("node");
            
            n1.transform.SetParent(root.transform,false);
            n2.transform.SetParent(root.transform,false);
            n3.transform.SetParent(n2.transform,false);
            
            ctx.AddObjectToAsset("root/n1", n1);
            ctx.AddObjectToAsset("root/n2", n2);
            ctx.AddObjectToAsset("root/n3", n3);
            ctx.AddObjectToAsset("root", root);
            
            ctx.SetMainObject(root);
        }
    }
}