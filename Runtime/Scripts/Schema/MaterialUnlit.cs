// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

namespace GLTFast.Schema
{

    /// <summary>
    /// This extension defines an unlit shading model for use in glTF 2.0
    /// materials, as an alternative to the Physically Based Rendering (PBR)
    /// shading models provided by the core specification.
    /// </summary>
    [System.Serializable]
    public class MaterialUnlit
    {
        internal void GltfSerialize(JsonWriter writer)
        {
            writer.AddObject();
            writer.Close();
        }
    }
}
