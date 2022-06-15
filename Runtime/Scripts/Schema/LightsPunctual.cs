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

namespace GLTFast.Schema {

    [Serializable]
    public class LightsPunctual {

        public LightPunctual[] lights;
        
        internal void GltfSerialize(JsonWriter writer) {
            writer.AddObject();
            writer.Close();
            throw new NotImplementedException($"GltfSerialize missing on {GetType()}");
        }

    }
    
    [Serializable]
    public class LightPunctual {

        public enum Type {
            Unknown,
            Spot,
            Directional,
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
        float[] color = {1,1,1};

        /// <summary>
        /// Light's color in linear space
        /// </summary>
        public Color lightColor {
            get =>
                new Color(
                    color[0],
                    color[1],
                    color[2]
                );
            set {
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

        public SpotLight spot;
        
        [SerializeField]
        string type;
        
        [NonSerialized]
        Type m_TypeEnum = Type.Unknown;
        
        /// <summary>
        /// Type of the light
        /// </summary>
        public Type typeEnum {
            get {
                if (m_TypeEnum != Type.Unknown) {
                    return m_TypeEnum;
                }
                if (!string.IsNullOrEmpty (type)) {
                    m_TypeEnum = (Type)Enum.Parse (typeof(Type), type, true);
                    type = null;
                    return m_TypeEnum;
                }
                return Type.Unknown;
            }
            set {
                m_TypeEnum = value;
                type = value.ToString();
            }
        }
        
        internal void GltfSerialize(JsonWriter writer) {
            writer.AddObject();
            writer.Close();
            throw new NotImplementedException($"GltfSerialize missing on {GetType()}");
        }

    }
    
    [Serializable]
    public class SpotLight {
        
        /// <summary>
        /// Angle, in radians, from centre of spotlight where falloff begins
        /// Must be greater than or equal to 0 and less than outerConeAngle
        /// </summary>
        public float innerConeAngle = 0;

        /// <summary>
        /// Angle, in radians, from centre of spotlight where falloff ends.
        /// Must be greater than innerConeAngle and less than or equal to
        /// PI / 2.0.
        /// </summary>
        public float outerConeAngle = math.PI/4f;
        
        internal void GltfSerialize(JsonWriter writer) {
            writer.AddObject();
            writer.AddProperty("innerConeAngle", innerConeAngle);
            writer.AddProperty("outerConeAngle", outerConeAngle);
            writer.Close();
        }
    }
}