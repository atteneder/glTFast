// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;

#if NEWTONSOFT_JSON
#endif
using UnityEngine;
using UnityEngine.Assertions;

namespace GLTFast.Schema
{
    /// <summary>
    /// Light
    /// </summary>
    [Serializable]
    public class LightPunctual : NamedObject
    {

        /// <summary>
        /// Light type
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
        /// RGB values for light's color in linear space
        /// </summary>
        // Field is public for unified serialization only. Warn via Obsolete attribute.
        [Obsolete("Use LightColor for access.")]
        public float[] color = { 1, 1, 1 };

        /// <summary>
        /// Light's color in linear space
        /// </summary>
        public Color LightColor
        {
#pragma warning disable CS0618 // Type or member is obsolete
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
#pragma warning restore CS0618 // Type or member is obsolete
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

        /// <inheritdoc cref="Type"/>
        // Field is public for unified serialization only. Warn via Obsolete attribute.
        [Obsolete("Use GetLightType and SetLightType for access.")]
        public string type;

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

#pragma warning disable CS0618 // Type or member is obsolete
            Enum.TryParse(type, true, out m_TypeEnum);
            type = null;
#pragma warning restore CS0618 // Type or member is obsolete
            return m_TypeEnum;
        }

        /// <summary>
        /// Sets the type of the light
        /// </summary>
        /// <param name="lightType">Light type</param>
        public void SetLightType(Type lightType)
        {
            m_TypeEnum = lightType;
#pragma warning disable CS0618 // Type or member is obsolete
            type = null;
#pragma warning restore CS0618 // Type or member is obsolete
        }

        internal void GltfSerialize(JsonWriter writer)
        {
            writer.AddObject();
            Assert.AreNotEqual(Type.Unknown, m_TypeEnum);
            writer.AddProperty("type", m_TypeEnum.ToString().ToLowerInvariant());
            GltfSerializeName(writer);
            if (LightColor != Color.white)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                writer.AddArrayProperty("color", color);
#pragma warning restore CS0618 // Type or member is obsolete
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
}
