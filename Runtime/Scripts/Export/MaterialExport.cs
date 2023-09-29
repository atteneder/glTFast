// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using UnityEngine;

namespace GLTFast.Export
{

    /// <summary>
    /// Provides the default material export
    /// </summary>
    public static class MaterialExport
    {

        static IMaterialExport s_MaterialExport;

        /// <summary>
        /// Provides the default material exporter
        /// </summary>
        /// <returns>Default material export</returns>
        /// <exception cref="InvalidOperationException">Is thrown when the default material export couldn't be determined based on the current render pipeline.</exception>
        public static IMaterialExport GetDefaultMaterialExport()
        {
            if (s_MaterialExport == null)
            {

                var renderPipeline = RenderPipelineUtils.RenderPipeline;

                switch (renderPipeline)
                {
                    case RenderPipeline.BuiltIn:
                    case RenderPipeline.Universal:
                        s_MaterialExport = new StandardMaterialExport();
                        break;
#if USING_HDRP
                    case RenderPipeline.HighDefinition:
                        s_MaterialExport = new HighDefinitionMaterialExport();
                        break;
#endif
                    default:
                        throw new InvalidOperationException($"Could not determine default MaterialExport (render pipeline {renderPipeline})");
                }
            }
            return s_MaterialExport;
        }
    }
}
