// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using UnityEngine;

namespace GLTFast.Schema
{

    /// <summary>
    /// Extension for adding punctual lights.
    /// </summary>
    /// <seealso href="https://github.com/KhronosGroup/glTF/tree/main/extensions/2.0/Khronos/KHR_lights_punctual"/>
    [Serializable]
    public class LightsPunctual
    {

        /// <summary>
        /// Collection of lights
        /// </summary>
        public LightPunctual[] lights;

        internal void GltfSerialize(JsonWriter writer)
        {
            writer.AddObject();
            writer.AddArray("lights");
            foreach (var light in lights)
            {
                light.GltfSerialize(writer);
            }
            writer.CloseArray();
            writer.Close();
        }

        /// <inheritdoc cref="RootExtensions.JsonUtilityCleanup"/>
        public bool JsonUtilityCleanup()
        {
            return lights != null;
        }
    }
}
