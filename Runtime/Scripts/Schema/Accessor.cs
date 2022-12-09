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
using UnityEngine.Assertions;

// GLTF_EXPORT
using UnityEngine.Rendering;

namespace GLTFast.Schema
{

    /// <summary>
    /// The datatype of an accessor's components
    /// <seealso href="https://www.khronos.org/registry/glTF/specs/2.0/glTF-2.0.html#accessor-data-types"/>
    /// </summary>
    public enum GltfComponentType
    {
        /// <summary>
        /// Signed byte (8-bit integer)
        /// </summary>
        Byte = 5120,
        /// <summary>
        /// Unsigned byte (8-bit integer)
        /// </summary>
        UnsignedByte = 5121,
        /// <summary>
        /// Signed short (16-bit integer)
        /// </summary>
        Short = 5122,
        /// <summary>
        /// Unsigned short (16-bit integer)
        /// </summary>
        UnsignedShort = 5123,
        /// <summary>
        /// Unsigned int (32-bit integer)
        /// </summary>
        UnsignedInt = 5125,
        /// <summary>
        /// 32-bit floating point number
        /// </summary>
        Float = 5126
    }

    /// <summary>
    /// Specifier for an accessor’s type
    /// <seealso href="https://www.khronos.org/registry/glTF/specs/2.0/glTF-2.0.html#accessor-data-types"/>
    /// </summary>
    public enum GltfAccessorAttributeType : byte
    {
        // Names are identical to glTF specified strings, that's why
        // inconsistent names are ignored.
        // ReSharper disable InconsistentNaming

        /// <summary>
        /// Unknown/undefined type
        /// </summary>
        Undefined,

        /// <summary>
        /// Scalar. single value.
        /// </summary>
        SCALAR,
        /// <summary>
        /// Two component vector
        /// </summary>
        VEC2,
        /// <summary>
        /// Three component vector
        /// </summary>
        VEC3,
        /// <summary>
        /// Four component vector
        /// </summary>
        VEC4,
        /// <summary>
        /// 2x2 matrix (4 values)
        /// </summary>
        MAT2,
        /// <summary>
        /// 3x3 matrix (9 values)
        /// </summary>
        MAT3,
        /// <summary>
        /// 4x4 matrix (16 values)
        /// </summary>
        MAT4
        // ReSharper restore InconsistentNaming
    }

    /// <summary>
    /// An accessor defines a method for retrieving data as typed arrays from
    /// within a buffer view.
    /// See <see href="https://www.khronos.org/registry/glTF/specs/2.0/glTF-2.0.html#accessors">
    /// accessor in the glTF 2.0 specification</see>
    /// </summary>
    [Serializable]
    public class Accessor
    {
        /// <summary>
        /// The index of the bufferView.
        /// If this is undefined, look in the sparse object for the index and value buffer views.
        /// </summary>
        public int bufferView = -1;

        /// <summary>
        /// The offset relative to the start of the bufferView in bytes.
        /// This must be a multiple of the size of the component datatype.
        /// <minimum>0</minimum>
        /// </summary>
        public int byteOffset;

        /// <summary>
        /// The datatype of components in the attribute.
        /// All valid values correspond to WebGL enums.
        /// The corresponding typed arrays are: `Int8Array`, `Uint8Array`, `Int16Array`,
        /// `Uint16Array`, `Uint32Array`, and `Float32Array`, respectively.
        /// 5125 (UNSIGNED_INT) is only allowed when the accessor contains indices
        /// i.e., the accessor is only referenced by `primitive.indices`.
        /// </summary>
        public GltfComponentType componentType;

        /// <summary>
        /// Specifies whether integer data values should be normalized
        /// (`true`) to [0, 1] (for unsigned types) or [-1, 1] (for signed types),
        /// or converted directly (`false`) when they are accessed.
        /// Must be `false` when accessor is used for animation data.
        /// </summary>
        public bool normalized;

        /// <summary>
        /// The number of attributes referenced by this accessor, not to be confused
        /// with the number of bytes or number of components.
        /// <minimum>1</minimum>
        /// </summary>
        public int count;

        /// <summary>
        /// Specifies if the attribute is a scalar, vector, or matrix,
        /// and the number of elements in the vector or matrix.
        /// </summary>
        [SerializeField]
        string type;

        [NonSerialized]
        GltfAccessorAttributeType m_TypeEnum = GltfAccessorAttributeType.Undefined;

        /// <summary>
        /// <see cref="GltfAccessorAttributeType"/> typed/cached getter from the <see cref="type"/> string.
        /// </summary>
        /// <returns>The Accessor's attribute type, if it could be retrieved correctly. <see cref="GltfAccessorAttributeType.Undefined"/> otherwise</returns>
        public GltfAccessorAttributeType GetAttributeType()
        {
            if (m_TypeEnum != GltfAccessorAttributeType.Undefined)
                return m_TypeEnum;

            if (!string.IsNullOrEmpty(type))
            {
                m_TypeEnum = (GltfAccessorAttributeType)Enum.Parse(typeof(GltfAccessorAttributeType), type, true);
                type = null;
                return m_TypeEnum;
            }

            return GltfAccessorAttributeType.Undefined;
        }

        /// <summary>
        /// <see cref="GltfAccessorAttributeType"/> typed setter for the <see cref="type"/> string.
        /// </summary>
        /// <param name="type">Attribute type</param>
        public void SetAttributeType(GltfAccessorAttributeType type)
        {
            m_TypeEnum = type;
            this.type = type.ToString();
        }

        /// <summary>
        /// Maximum value of each component in this attribute.
        /// Both min and max arrays have the same length.
        /// The length is determined by the value of the type property;
        /// it can be 1, 2, 3, 4, 9, or 16.
        ///
        /// When `componentType` is `5126` (FLOAT) each array value must be stored as
        /// double-precision JSON number with numerical value which is equal to
        /// buffer-stored single-precision value to avoid extra runtime conversions.
        ///
        /// `normalized` property has no effect on array values: they always correspond
        /// to the actual values stored in the buffer. When accessor is sparse, this
        /// property must contain max values of accessor data with sparse substitution
        /// applied.
        /// <minItems>1</minItems>
        /// <maxItems>16</maxItems>
        /// </summary>
        public float[] max;

        /// <summary>
        /// Minimum value of each component in this attribute.
        /// Both min and max arrays have the same length.  The length is determined by
        /// the value of the type property; it can be 1, 2, 3, 4, 9, or 16.
        ///
        /// When `componentType` is `5126` (FLOAT) each array value must be stored as
        /// double-precision JSON number with numerical value which is equal to
        /// buffer-stored single-precision value to avoid extra runtime conversions.
        ///
        /// `normalized` property has no effect on array values: they always correspond
        /// to the actual values stored in the buffer. When accessor is sparse, this
        /// property must contain min values of accessor data with sparse substitution
        /// applied.
        /// <minItems>1</minItems>
        /// <maxItems>16</maxItems>
        /// </summary>
        public float[] min;

        /// <summary>
        /// Sparse storage of attributes that deviate from their initialization value.
        /// </summary>
        public AccessorSparse sparse;

        /// <summary>
        /// Provides size of components by type
        /// </summary>
        /// <param name="componentType">glTF component type</param>
        /// <returns>Component size in bytes</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static int GetComponentTypeSize(GltfComponentType componentType)
        {
            switch (componentType)
            {
                case GltfComponentType.Byte:
                case GltfComponentType.UnsignedByte:
                    return 1;
                case GltfComponentType.Short:
                case GltfComponentType.UnsignedShort:
                    return 2;
                case GltfComponentType.Float:
                case GltfComponentType.UnsignedInt:
                    return 4;
                default:
                    throw new ArgumentOutOfRangeException(nameof(componentType), componentType, null);
            }
        }

        /// <summary>
        /// Converts Unity vertex attribute format to glTF component type.
        /// </summary>
        /// <param name="format">vertex attribute format</param>
        /// <returns>glTF component type</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static GltfComponentType GetComponentType(VertexAttributeFormat format)
        {
            switch (format)
            {
                case VertexAttributeFormat.Float32:
                case VertexAttributeFormat.Float16:
                    return GltfComponentType.Float;
                case VertexAttributeFormat.UNorm8:
                case VertexAttributeFormat.UInt8:
                    return GltfComponentType.UnsignedByte;
                case VertexAttributeFormat.SNorm8:
                case VertexAttributeFormat.SInt8:
                    return GltfComponentType.Byte;
                case VertexAttributeFormat.UNorm16:
                case VertexAttributeFormat.UInt16:
                    return GltfComponentType.UnsignedShort;
                case VertexAttributeFormat.SNorm16:
                case VertexAttributeFormat.SInt16:
                    return GltfComponentType.Short;
                case VertexAttributeFormat.UInt32:
                case VertexAttributeFormat.SInt32:
                    return GltfComponentType.UnsignedInt;
                default:
                    throw new ArgumentOutOfRangeException(nameof(format), format, null);
            }
        }

        /// <summary>
        /// Get one-dimensional glTF attribute type by number of components per elements.
        /// Note that this does not support matrix types.
        /// </summary>
        /// <param name="dimension">Number of components per element</param>
        /// <returns>Corresponding one-dimensional glTF attribute type</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static GltfAccessorAttributeType GetAccessorAttributeType(int dimension)
        {
            switch (dimension)
            {
                case 1:
                    return GltfAccessorAttributeType.SCALAR;
                case 2:
                    return GltfAccessorAttributeType.VEC2;
                case 3:
                    return GltfAccessorAttributeType.VEC3;
                case 4:
                    return GltfAccessorAttributeType.VEC4;
                default:
                    throw new ArgumentOutOfRangeException(nameof(dimension), dimension, null);
            }
        }

        /// <summary>
        /// Get number of components of glTF attribute type.
        /// </summary>
        /// <param name="type">glTF attribute type</param>
        /// <returns>Number of components</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static int GetAccessorAttributeTypeLength(GltfAccessorAttributeType type)
        {
            switch (type)
            {
                case GltfAccessorAttributeType.SCALAR:
                    return 1;
                case GltfAccessorAttributeType.VEC2:
                    return 2;
                case GltfAccessorAttributeType.VEC3:
                    return 3;
                case GltfAccessorAttributeType.VEC4:
                case GltfAccessorAttributeType.MAT2:
                    return 4;
                case GltfAccessorAttributeType.MAT3:
                    return 9;
                case GltfAccessorAttributeType.MAT4:
                    return 16;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        /// <summary>
        /// For 3D positional data, returns accessor's bounding box. Applies coordinate system transform (glTF to Unity)
        /// </summary>
        /// <returns>Bounding box enclosing the minimum and maximum values</returns>
        public Bounds? TryGetBounds()
        {
            Assert.AreEqual(GltfAccessorAttributeType.VEC3, GetAttributeType());
            if (min != null && min.Length > 2 && max != null && max.Length > 2)
            {
                var maxBounds = new float3(-min[0], max[1], max[2]);
                var minBounds = new float3(-max[0], min[1], min[2]);
                if (normalized)
                {
                    switch (componentType)
                    {
                        case GltfComponentType.Byte:
                            maxBounds = math.max(maxBounds / sbyte.MaxValue, -1);
                            minBounds = math.max(minBounds / sbyte.MaxValue, -1);
                            break;
                        case GltfComponentType.UnsignedByte:
                            maxBounds /= byte.MaxValue;
                            minBounds /= byte.MaxValue;
                            break;
                        case GltfComponentType.Short:
                            maxBounds = math.max(maxBounds / short.MaxValue, -1);
                            minBounds = math.max(minBounds / short.MaxValue, -1);
                            break;
                        case GltfComponentType.UnsignedShort:
                            maxBounds /= ushort.MaxValue;
                            minBounds /= ushort.MaxValue;
                            break;
                        case GltfComponentType.UnsignedInt:
                            maxBounds /= uint.MaxValue;
                            minBounds /= uint.MaxValue;
                            break;
                    }
                }
                return new Bounds
                {
                    max = maxBounds,
                    min = minBounds
                };
            }
            return null;
        }

        /// <summary>
        /// True if the accessor is <see href="https://www.khronos.org/registry/glTF/specs/2.0/glTF-2.0.html#sparse-accessors">sparse</see>
        /// </summary>
        public bool IsSparse => sparse != null;

        internal void GltfSerialize(JsonWriter writer)
        {
            writer.AddObject();
            if (bufferView >= 0)
            {
                writer.AddProperty("bufferView", bufferView);
            }
            writer.AddProperty("componentType", (int)componentType);
            writer.AddProperty("count", count);
            writer.AddProperty("type", type);
            if (byteOffset > 0)
            {
                writer.AddProperty("byteOffset", byteOffset);
            }
            if (normalized)
            {
                writer.AddProperty("normalized", normalized);
            }
            if (max != null)
            {
                writer.AddArrayProperty("max", max);
            }
            if (min != null)
            {
                writer.AddArrayProperty("min", min);
            }

            if (sparse != null)
            {
                writer.AddProperty("sparse");
                sparse.GltfSerialize(writer);
                writer.Close();
            }
            writer.Close();
        }
    }
}
