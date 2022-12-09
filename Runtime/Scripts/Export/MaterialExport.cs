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
        /// <exception cref="Exception"></exception>
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
                        throw new Exception($"Could not determine default MaterialExport (render pipeline {renderPipeline})");
                }
            }
            return s_MaterialExport;
        }
    }
}
