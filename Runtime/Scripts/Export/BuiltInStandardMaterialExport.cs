// SPDX-FileCopyrightText: 2026 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using GLTFast.Logging;
using GLTFast.Schema;
using UnityEngine;

namespace GLTFast.Export
{
    /// <summary>
    /// Converts Built-In Standard shader based materials to glTF materials
    /// </summary>
    public sealed class BuiltInStandardMaterialExport : StandardMaterialExportBase
    {
        const string k_KeywordMetallicGlossMap = "_METALLICGLOSSMAP";

        static readonly int k_GlossMapScaleProperty = Shader.PropertyToID("_GlossMapScale");

        /// <summary>
        /// _Glossiness shader property identifier
        /// </summary>
        public static readonly int GlossinessProperty = Shader.PropertyToID("_Glossiness");

        /// <inheritdoc/>
        protected override bool HasMetallicGlossMap(UnityEngine.Material uMaterial)
        {
            return uMaterial.IsKeywordEnabled(k_KeywordMetallicGlossMap);
        }

        /// <inheritdoc/>
        protected override bool IsPbrMetallicRoughness(UnityEngine.Material material)
        {
            return material.HasProperty(MetallicProperty)
                && (
                    HasMetallicGlossMap(material)
                    || material.HasProperty(GlossinessProperty)
                );
        }

        /// <inheritdoc/>
        protected override int GetSmoothnessProperty(bool sourceAlbedoAlpha, bool hasMetallicGlossinessMap)
        {
            return sourceAlbedoAlpha || hasMetallicGlossinessMap ? k_GlossMapScaleProperty : GlossinessProperty;
        }
    }
}
