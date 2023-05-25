// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using UnityEngine;

namespace GLTFast.Schema
{

    /// <summary>
    /// Assigns a light to a node
    /// </summary>
    [System.Serializable]
    public class NodeLightsPunctual
    {

        /// <summary>
        /// Light index
        /// </summary>
        public int light = -1;

        internal void GltfSerialize(JsonWriter writer)
        {
            writer.AddObject();
            if (light >= 0)
            {
                writer.AddProperty("light", light);
            }
            writer.Close();
        }
    }
}
