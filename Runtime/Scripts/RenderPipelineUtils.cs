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

using UnityEngine;
#if USING_URP || USING_HDRP
using UnityEngine.Rendering;
#endif
#if USING_URP
using UnityEngine.Rendering.Universal;
#endif
#if USING_HDRP
using UnityEngine.Rendering.HighDefinition;
#endif

namespace GLTFast
{

    /// <summary>
    /// Render Pipeline
    /// </summary>
    public enum RenderPipeline
    {
        /// <summary>
        /// Unknown Render Pipeline
        /// </summary>
        Unknown,
        /// <summary>
        /// <see href="https://docs.unity3d.com/Manual/built-in-render-pipeline.html">Built-in Render Pipeline</see>
        /// Unity's built-in render pipeline
        /// </summary>
        BuiltIn,
        /// <summary>
        /// <see href="https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@latest">Universal Render Pipeline</see>
        /// </summary>
        Universal,
        /// <summary>
        /// <see href="https://docs.unity3d.com/Packages/com.unity.render-pipelines.high-definition@latest">High Definition Render Pipeline</see>
        /// </summary>
        HighDefinition
    }

    /// <summary>
    /// Render Pipeline Utilities
    /// </summary>
    public static class RenderPipelineUtils
    {

        static RenderPipeline s_RenderPipeline = RenderPipeline.Unknown;

        /// <summary>
        /// Currently used render pipeline
        /// </summary>
        public static RenderPipeline RenderPipeline
        {
            get
            {
                if (s_RenderPipeline == RenderPipeline.Unknown)
                {
                    s_RenderPipeline = DetectRenderPipeline();
                }
                return s_RenderPipeline;
            }
        }

        static RenderPipeline DetectRenderPipeline()
        {
#if USING_URP || USING_HDRP
            // ReSharper disable once Unity.PerformanceCriticalCodeNullComparison
            var rpAsset = QualitySettings.renderPipeline ? QualitySettings.renderPipeline : GraphicsSettings.defaultRenderPipeline;
            if (rpAsset != null) {
#if USING_URP
                if (rpAsset is UniversalRenderPipelineAsset) {
                    return RenderPipeline.Universal;
                }
#endif
#if USING_HDRP
                if (rpAsset is HDRenderPipelineAsset) {
                    return RenderPipeline.HighDefinition;
                }
#endif
                throw new System.Exception("glTFast: Unknown Render Pipeline");
            }
#endif
            return RenderPipeline.BuiltIn;
        }
    }
}
