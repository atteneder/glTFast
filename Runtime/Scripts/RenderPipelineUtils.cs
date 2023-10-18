// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

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
        /// <a href="https://docs.unity3d.com/Manual/built-in-render-pipeline.html">Built-in Render Pipeline</a>
        /// Unity's built-in render pipeline
        /// </summary>
        BuiltIn,
        /// <summary>
        /// <a href="https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@latest">Universal Render Pipeline</a>
        /// </summary>
        Universal,
        /// <summary>
        /// <a href="https://docs.unity3d.com/Packages/com.unity.render-pipelines.high-definition@latest">High Definition Render Pipeline</a>
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
