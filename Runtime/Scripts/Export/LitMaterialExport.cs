// SPDX-FileCopyrightText: 2026 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

#if USING_URP || USING_HDRP

using System;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Unity.Cloud.Gltfast.Export
{
    /// <summary>
    /// Converts URP/HDRP Lit shader based materials to glTF materials
    /// </summary>
    [MovedFrom(true, sourceNamespace: "GLTFast.Export", sourceAssembly: "glTFast.Export")]
    public sealed class LitMaterialExport : StandardMaterialExportBase
    {
        const string k_KeywordMetallicSpecGlossMap = "_METALLICSPECGLOSSMAP"; // URP Lit

        protected override bool IsPbrMetallicRoughness(UnityEngine.Material material)
        {
            return material.HasProperty(MetallicProperty)
                && (
                    HasMetallicGlossMap(material)
                    || material.HasProperty(SmoothnessProperty)
                );
        }

        protected override bool HasMetallicGlossMap(UnityEngine.Material uMaterial)
        {
            return uMaterial.IsKeywordEnabled(k_KeywordMetallicSpecGlossMap);
        }

        protected override int GetSmoothnessProperty(bool sourceAlbedoAlpha, bool hasMetallicGlossinessMap)
        {
            return SmoothnessProperty;
        }
    }
}
#endif // USING_URP || USING_HDRP
