// SPDX-FileCopyrightText: 2024 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

#if UNITY_SHADER_GRAPH

using System;
using UnityEngine;

namespace GLTFast.Export
{
    using Logging;
    using Schema;

    /// <inheritdoc cref="MetaMaterialExportShaderGraphs{TLitExport,TGltfShaderGraphExport}"/>
    [Obsolete("Use MaterialExport.GetDefaultMaterialExport instead.")]
    // TODO: Make private in next major release
    public class MetaMaterialExport<TLitExport, TGltfShaderGraphExport> :
        IMaterialExport
        where TLitExport : IMaterialExport, new()
        where TGltfShaderGraphExport : IMaterialExport, new()
    {
        /// <inheritdoc />
        public bool ConvertMaterial(
            UnityEngine.Material uMaterial,
            out Material material,
            IGltfWritable gltf,
            ICodeLogger logger
            )
        {
            return MetaMaterialExportShaderGraphs<TLitExport, TGltfShaderGraphExport>
                .Instance
                .ConvertMaterial(uMaterial, out material, gltf, logger);
        }
    }

    /// <summary>
    /// Picks a fitting material exporter, based on the used shader.
    /// </summary>
    /// <typeparam name="TLitExport">Fallback material exporter for Unity Standard/Lit shaders.</typeparam>
    /// <typeparam name="TGltfShaderGraphExport">Material exporter for glTFast shader graphs.</typeparam>
    class MetaMaterialExportShaderGraphs<TLitExport, TGltfShaderGraphExport> :
        IMaterialExport
        where TLitExport : IMaterialExport, new()
        where TGltfShaderGraphExport : IMaterialExport, new()
    {
        static TLitExport s_LitMaterialExport;
        static TGltfShaderGraphExport s_GltfShaderGraphMaterialExport;

        MetaMaterialExportShaderGraphs() { }
        public static MetaMaterialExportShaderGraphs<TLitExport, TGltfShaderGraphExport> Instance { get; } =
            new MetaMaterialExportShaderGraphs<TLitExport, TGltfShaderGraphExport>();

        /// <inheritdoc />
        public bool ConvertMaterial(
            UnityEngine.Material uMaterial,
            out Material material,
            IGltfWritable gltf,
            ICodeLogger logger
            )
        {
            IMaterialExport materialExport;

            var name = uMaterial.shader.name;
#if UNITY_SHADER_GRAPH
            if (name.StartsWith("Shader Graphs/glTF-"))
            {
                if (!MetaMaterialExportBuiltIn.TryFindMatchingGltfUnlitMaterialExport(name, out materialExport))
                {
                    s_GltfShaderGraphMaterialExport ??= new TGltfShaderGraphExport();
                    materialExport = s_GltfShaderGraphMaterialExport;
                }
            }
            else
#endif
            if (!MetaMaterialExportBuiltIn.TryFindMatchingGltfMaterialExport(name, out materialExport))
            {
                s_LitMaterialExport ??= new TLitExport();
                materialExport = s_LitMaterialExport;
            }

            return materialExport.ConvertMaterial(uMaterial, out material, gltf, logger);
        }
    }
}
#endif // UNITY_SHADER_GRAPH
