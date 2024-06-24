// SPDX-FileCopyrightText: 2024 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using UnityEngine;

namespace GLTFast.Export
{
    using Logging;
    using Schema;

    class MetaMaterialExportBuiltIn : IMaterialExport
    {
        static IMaterialExport s_LitMaterialExport;
        static IMaterialExport s_GltfBuiltInMaterialExport;
        static IMaterialExport s_GltfUnlitMaterialExport;

        MetaMaterialExportBuiltIn() { }
        public static IMaterialExport Instance { get; } = new MetaMaterialExportBuiltIn();

        /// <inheritdoc />
        public bool ConvertMaterial(
            UnityEngine.Material uMaterial,
            out Material material,
            IGltfWritable gltf,
            ICodeLogger logger
            )
        {
            var materialExport = FindMatchingMaterialExport(uMaterial.shader.name);
            return materialExport.ConvertMaterial(uMaterial, out material, gltf, logger);
        }

        static IMaterialExport FindMatchingMaterialExport(string shaderName)
        {
            if (!TryFindMatchingGltfMaterialExport(shaderName, out var materialExport))
            {
                s_LitMaterialExport ??= new StandardMaterialExport();
                materialExport = s_LitMaterialExport;
            }
            return materialExport;
        }

        internal static bool TryFindMatchingGltfMaterialExport(string shaderName, out IMaterialExport materialExport)
        {
            if (shaderName.StartsWith("glTF/"))
            {
                if (TryFindMatchingGltfUnlitMaterialExport(shaderName, out materialExport))
                {
                    return true;
                }

                s_GltfBuiltInMaterialExport ??= new GltfBuiltInShaderMaterialExporter();
                materialExport = s_GltfBuiltInMaterialExport;
                return true;
            }

            materialExport = null;
            return false;
        }

        internal static bool TryFindMatchingGltfUnlitMaterialExport(
            string shaderName,
            out IMaterialExport materialExport
            )
        {
            if (shaderName.LastIndexOf("nlit") >= 0)
            {
                // Unlit shader
                s_GltfUnlitMaterialExport ??= new GltfUnlitMaterialExporter();
                materialExport = s_GltfUnlitMaterialExport;
                return true;
            }

            materialExport = null;
            return false;
        }
    }
}
