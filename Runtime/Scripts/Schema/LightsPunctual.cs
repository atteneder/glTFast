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
using Unity.Mathematics;
using UnityEngine;

namespace GLTFast.Schema
{

    /// <summary>
    /// Extension for adding punctual lights.
    /// <seealso href="https://github.com/KhronosGroup/glTF/tree/main/extensions/2.0/Khronos/KHR_lights_punctual"/>
    /// </summary>
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
    }

    /// <summary>
    /// glTF light
    /// </summary>
    [Serializable]
    public class LightPunctual
    {

        /// <summary>
        /// glTF light type
        /// </summary>
        public enum Type
        {
            /// <summary>Unknown light type</summary>
            Unknown,
            /// <summary>Spot light</summary>
            Spot,
            /// <summary>Directional light</summary>
            Directional,
            /// <summary>Point light</summary>
            Point,
        }

        /// <summary>
        /// Name of the light
        /// </summary>
        public string name;

        /// <summary>
        /// RGB value for light's color in linear space
        /// </summary>
        [SerializeField]
        float[] color = { 1, 1, 1 };

        /// <summary>
        /// Light's color in linear space
        /// </summary>
        public Color LightColor
        {
            get =>
                new Color(
                    color[0],
                    color[1],
                    color[2]
                );
            set
            {
                color = new[] { value.r, value.g, value.b };
            }
        }

        /// <summary>
        /// Brightness of light in. The units that this is defined in depend on
        /// the type of light. point and spot lights use luminous intensity in
        /// candela (lm/sr) while directional lights use illuminance
        /// in lux (lm/m2)
        /// </summary>
        public float intensity = 1;

        /// <summary>
        /// Hint defining a distance cutoff at which the light's intensity may
        /// be considered to have reached zero. Supported only for point and
        /// spot lights. Must be > 0. When undefined, range is assumed to be
        /// infinite.
        /// </summary>
        public float range = -1;

        /// <summary>
        /// Spot light properties (only set on spot lights).
        /// </summary>
        public SpotLight spot;

        [SerializeField]
        string type;

        [NonSerialized]
        Type m_TypeEnum = Type.Unknown;

        /// <summary>
        /// Returns the type of the light
        /// It converts the <see cref="type"/> string and caches it.
        /// </summary>
        /// <returns>Light type, if it was retrieved correctly. <see cref="Type.Unknown"/> otherwise</returns>
        public Type GetLightType()
        {
            if (m_TypeEnum != Type.Unknown)
            {
                return m_TypeEnum;
            }

            if (!string.IsNullOrEmpty(type))
            {
                m_TypeEnum = (Type)Enum.Parse(typeof(Type), type, true);
                type = null;
                return m_TypeEnum;
            }

            return Type.Unknown;
        }

        /// <summary>
        /// Sets the type of the light
        /// </summary>
        /// <param name="type">Light type</param>
        public void SetLightType(Type type)
        {
            m_TypeEnum = type;
            this.type = type.ToString().ToLowerInvariant();
        }

        internal void GltfSerialize(JsonWriter writer)
        {
            writer.AddObject();
            writer.AddProperty("type", type);
            if (!string.IsNullOrEmpty(name))
            {
                writer.AddProperty("name", name);
            }
            if (LightColor != Color.white)
            {
                writer.AddArrayProperty("color", color);
            }
            if (Math.Abs(intensity - 1.0) > Constants.epsilon)
            {
                writer.AddProperty("intensity", intensity);
            }
            if (range > 0 && GetLightType() != Type.Directional)
            {
                writer.AddProperty("range", range);
            }
            if (spot != null)
            {
                writer.AddProperty("spot");
                spot.GltfSerialize(writer);
            }
            writer.Close();
        }

    }

    /// <summary>
    /// glTF spot light properties
    /// </summary>
    [Serializable]
    public class SpotLight
    {

        /// <summary>
        /// Angle, in radians, from centre of spotlight where falloff begins
        /// Must be greater than or equal to 0 and less than outerConeAngle
        /// </summary>
        public float innerConeAngle;

        /// <summary>
        /// Angle, in radians, from centre of spotlight where falloff ends.
        /// Must be greater than innerConeAngle and less than or equal to
        /// PI / 2.0.
        /// </summary>
        public float outerConeAngle = math.PI / 4f;

        internal void GltfSerialize(JsonWriter writer)
        {
            writer.AddObject();
            writer.AddProperty("innerConeAngle", innerConeAngle);
            writer.AddProperty("outerConeAngle", outerConeAngle);
            writer.Close();
        }
    }
}
