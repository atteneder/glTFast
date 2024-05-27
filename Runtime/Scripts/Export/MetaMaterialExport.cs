// SPDX-FileCopyrightText: 2024 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

#if UNITY_SHADER_GRAPH

using System;
using UnityEngine;

namespace GLTFast.Export
{
    using Logging;
    using Schema;

    public class MetaMaterialExport<TLitExport, TGltfShaderGraphExport> :
        IMaterialExport
        where TLitExport : IMaterialExport, new()
        where TGltfShaderGraphExport : IMaterialExport, new()
    {
        static IMaterialExport s_LitMaterialExport;
        static IMaterialExport s_GltfBuiltInMaterialExport;
        static IMaterialExport s_GltfShaderGraphMaterialExport;
        static IMaterialExport s_GltfUnlitMaterialExport;

        /// <inheritdoc />
        public bool ConvertMaterial(UnityEngine.Material uMaterial, out Material material, IGltfWritable gltf, ICodeLogger logger)
        {
            IMaterialExport materialExport;

            var name = uMaterial.shader.name;
#if UNITY_SHADER_GRAPH
            if (name.StartsWith("Shader Graphs/glTF-"))
            {
                if (name.LastIndexOf("nlit") >= 0)
                {
                    // Unlit shader
                    s_GltfUnlitMaterialExport ??= new GltfUnlitMaterialExporter();
                    materialExport = s_GltfUnlitMaterialExport;
                }
                else
                {
                    s_GltfShaderGraphMaterialExport ??= new TGltfShaderGraphExport();
                    materialExport = s_GltfShaderGraphMaterialExport;
                }
            }
            else
#endif
            if (name.StartsWith("glTF/"))
            {
                if (name.LastIndexOf("nlit") >= 0)
                {
                    // Unlit shader
                    s_GltfUnlitMaterialExport ??= new GltfUnlitMaterialExporter();
                    materialExport = s_GltfUnlitMaterialExport;
                }
                else
                {
                    s_GltfBuiltInMaterialExport ??= new GltfBuiltInShaderMaterialExporter();
                    materialExport = s_GltfBuiltInMaterialExport;
                }
            }
            else
            {
                s_LitMaterialExport ??= new TLitExport();
                materialExport = s_LitMaterialExport;
            }

            return materialExport.ConvertMaterial(uMaterial, out material, gltf, logger);
        }
    }
}
#endif // UNITY_SHADER_GRAPH
