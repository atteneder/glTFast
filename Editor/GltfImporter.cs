using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GLTFast.Editor {

    [ScriptedImporter(1,"glb")] // new [] {"gltf","glb"}
    public class GltfImporter : ScriptedImporter {

        GLTFast m_Gltf;

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

            var absPath = Path.Combine(Path.GetDirectoryName(Application.dataPath) ?? throw new InvalidOperationException(), ctx.assetPath);

            m_Gltf = new GLTFast(null, new UninterruptedDeferAgent() );

            var settings = new ImportSettings {
                nodeNameMethod = ImportSettings.NameImportMethod.OriginalUnique
            };

            var success = AsyncHelpers.RunSync<bool>(() => m_Gltf.Load(absPath,settings));
            
            if (success) {
                m_ImportedNames = new HashSet<string>();
                m_ImportedObjects = new HashSet<Object>();
                
                var go = new GameObject("root");
                m_Gltf.InstantiateGltf(go.transform);

                foreach (Transform sceneTransform in go.transform) {
                    var sceneGo = sceneTransform.gameObject;
                    var identifier = AddObjectToAsset(ctx,$"scenes/{sceneGo.name}", sceneGo);
                    AddHierarchy(sceneTransform,ctx,identifier);
                }

                for (var i = 0; i < m_Gltf.materialCount; i++) {
                    var mat = m_Gltf.GetMaterial(i);
                    AddObjectToAsset(ctx, $"materials/{mat.name}", mat);
                }
                
                for (var i = 0; i < m_Gltf.textureCount; i++) {
                    var texture = m_Gltf.GetTexture(i);
                    if (texture != null) {
                        AddObjectToAsset(ctx, $"textures/{texture.name}", texture);
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
            // obj.name = uniqueAssetName;
            ctx.AddObjectToAsset(uniqueAssetName, obj);
            m_ImportedNames.Add(uniqueAssetName);
            m_ImportedObjects.Add(obj);
            return uniqueAssetName;
        }

        void AddHierarchy(Transform root, AssetImportContext ctx, string prefix) {

            var mfs = root.GetComponents<MeshFilter>();
            foreach (var meshFilter in mfs) {
                var mesh = meshFilter.sharedMesh;
                if (mesh != null) {
                    AddObjectToAsset(ctx,$"meshes/{mesh.name}",mesh);
                }
            }
            
            foreach (Transform child in root) {
                var identifier = $"{prefix}/{child.name}";
                // identifier = AddObjectToAsset(ctx,identifier, child.gameObject);
                // identifier = GetUniqueAssetName(identifier);
                AddHierarchy(child,ctx,identifier);
            }
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