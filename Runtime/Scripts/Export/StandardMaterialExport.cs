// SPDX-FileCopyrightText: 2026 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using GLTFast.Logging;
using UnityEngine;
using Material = GLTFast.Schema.Material;

namespace GLTFast.Export
{
    /// <summary>
    /// Converts URP/HDRP Lit and Built-In Standard shader based materials to glTF materials
    /// </summary>
    /// <seealso cref="BuiltInStandardMaterialExport"/>
    /// <seealso cref="LitMaterialExport"/>
    [Obsolete("Use BuiltInStandardMaterialExport or LitMaterialExport instead.")]
    public class StandardMaterialExport : MaterialExportBase
    {
        /// <inheritdoc />
        public override bool ConvertMaterial(
            UnityEngine.Material uMaterial,
            out Material material,
            IGltfWritable gltf,
            ICodeLogger logger
            )
        {
#if USING_URP || USING_HDRP
            if (RenderPipelineUtils.RenderPipeline != RenderPipeline.BuiltIn)
            {
                var litExporter = new LitMaterialExport();
                return litExporter.ConvertMaterial(uMaterial, out material, gltf, logger);
            }
#endif
            var builtInExporter = new BuiltInStandardMaterialExport();
            return builtInExporter.ConvertMaterial(uMaterial, out material, gltf, logger);
        }
    }
}
