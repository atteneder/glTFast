// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Unity.Cloud.Gltfast.Text.Json;
using Unity.Cloud.Gltfast.Text.Json.Serialization;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;

// GLTF_EXPORT
using UnityEngine.Rendering;
using UnityEngine.Scripting.APIUpdating;

namespace Unity.Cloud.Gltfast.Objects
{
    /// <summary>
    /// An accessor defines a method for retrieving data as typed arrays from
    /// within a buffer view.
    /// See <a href="https://www.khronos.org/registry/glTF/specs/2.0/glTF-2.0.html#accessors">.
    /// accessor in the glTF 2.0 specification</a>.
    /// </summary>
    [MovedFrom(true, sourceNamespace: "GLTFast.Schema", sourceAssembly: "glTFast")]
    public class Accessor : NamedObject, IAdditionalPropertyContainer
    {
        /// <summary>
        /// Sparse storage of attributes that deviate from their initialization value.
        /// </summary>
        [JsonPropertyName("sparse")]
        public AccessorSparse Sparse { get; set; }

        /// <summary>
        /// The index of the bufferView.
        /// If this is undefined, look in the sparse object for the index and value buffer views.
        /// </summary>
        [JsonPropertyName("bufferView")]
        public int? BufferView { get; set; }

        /// <summary>
        /// The offset relative to the start of the bufferView in bytes.
        /// This must be a multiple of the size of the component datatype.
        /// </summary>
        [JsonPropertyName("byteOffset")]
        public int ByteOffset { get; set; }

        /// <summary>
        /// The datatype of components in the attribute.
        /// All valid values correspond to WebGL enums.
        /// The corresponding typed arrays are: `Int8Array`, `Uint8Array`, `Int16Array`,
        /// `Uint16Array`, `Uint32Array`, and `Float32Array`, respectively.
        /// 5125 (UNSIGNED_INT) is only allowed when the accessor contains indices
        /// i.e., the accessor is only referenced by `primitive.indices`.
        /// </summary>
        [JsonPropertyName("componentType")]
        public AccessorDataType ComponentType { get; set; }

        /// <summary>
        /// Specifies whether integer data values should be normalized
        /// (`true`) to [0, 1] (for unsigned types) or [-1, 1] (for signed types),
        /// or converted directly (`false`) when they are accessed.
        /// Must be `false` when accessor is used for animation data.
        /// </summary>
        [JsonPropertyName("normalized")]
        public bool Normalized { get; set; }

        /// <summary>
        /// The number of attributes referenced by this accessor, not to be confused
        /// with the number of bytes or number of components.
        /// </summary>
        [JsonPropertyName("count")]
        public int Count { get; set; }

        /// <inheritdoc cref="AccessorType"/>
        [JsonPropertyName("type")]
        [JsonConverter(typeof(AccessorTypeValueConverter))]
        public EnumOrRawValue<AccessorType> Type { get; set; }

        /// <inheritdoc cref="Root.Extras"/>
        [JsonPropertyName("extras")]
        [JsonConverter(typeof(ExtrasConverter))]
        public ExtrasContainer Extras { get; set; }

        /// <inheritdoc cref="Asset.Extensions"/>
        [JsonPropertyName("extensions")]
        public AccessorExtensions Extensions { get; set; }

        /// <summary>JSON properties without a matching member.</summary>
        [JsonExtensionData, JsonInclude]
        internal Dictionary<string, JsonElement> ExtensionData { get; set; }

        /// <inheritdoc/>
        [JsonIgnore]
        public ReadOnlyProperties AdditionalProperties => new(ExtensionData ?? ReadOnlyProperties.Empty);

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
        /// </summary>
        [JsonPropertyName("max")]
        [JsonConverter(typeof(DoubleListConverter))]
        public List<double> Max { get; set; }

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
        /// </summary>
        [JsonPropertyName("min")]
        [JsonConverter(typeof(DoubleListConverter))]
        public List<double> Min { get; set; }

        /// <summary>
        /// Provides size of components by type
        /// </summary>
        /// <param name="componentType">glTF component type</param>
        /// <returns>Component size in bytes</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when value of <see cref="ComponentType"/> is unknown</exception>
        public static int GetComponentTypeSize(AccessorDataType componentType)
        {
            switch (componentType)
            {
                case AccessorDataType.Byte:
                case AccessorDataType.UnsignedByte:
                    return 1;
                case AccessorDataType.Short:
                case AccessorDataType.UnsignedShort:
                    return 2;
                case AccessorDataType.Float:
                case AccessorDataType.UnsignedInt:
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
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the value of <paramref name="format"/> is unknown.</exception>
        public static AccessorDataType GetComponentType(VertexAttributeFormat format)
        {
            switch (format)
            {
                case VertexAttributeFormat.Float32:
                case VertexAttributeFormat.Float16:
                    return AccessorDataType.Float;
                case VertexAttributeFormat.UNorm8:
                case VertexAttributeFormat.UInt8:
                    return AccessorDataType.UnsignedByte;
                case VertexAttributeFormat.SNorm8:
                case VertexAttributeFormat.SInt8:
                    return AccessorDataType.Byte;
                case VertexAttributeFormat.UNorm16:
                case VertexAttributeFormat.UInt16:
                    return AccessorDataType.UnsignedShort;
                case VertexAttributeFormat.SNorm16:
                case VertexAttributeFormat.SInt16:
                    return AccessorDataType.Short;
                case VertexAttributeFormat.UInt32:
                case VertexAttributeFormat.SInt32:
                    return AccessorDataType.UnsignedInt;
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
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="dimension"/> is not between 1 and 4.</exception>
        public static AccessorType GetAccessorAttributeType(int dimension)
        {
            if (dimension is < 1 or > 4)
            {
                throw new ArgumentOutOfRangeException(nameof(dimension), dimension, null);
            }
            return (AccessorType)dimension;
        }

        /// <summary>
        /// Get number of components of glTF attribute type.
        /// </summary>
        /// <param name="type">glTF attribute type</param>
        /// <returns>Number of components</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the value of <see cref="type"/> is unknown.</exception>
        public static int GetAccessorAttributeTypeLength(AccessorType type)
        {
            switch (type)
            {
                case AccessorType.Scalar:
                    return 1;
                case AccessorType.Vector2:
                    return 2;
                case AccessorType.Vector3:
                    return 3;
                case AccessorType.Vector4:
                case AccessorType.Matrix2x2:
                    return 4;
                case AccessorType.Matrix3x3:
                    return 9;
                case AccessorType.Matrix4x4:
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
            Assert.AreEqual(AccessorType.Vector3, Type.Value);
            if (Min is { Count: > 2 } && Max is { Count: > 2 })
            {
                var maxBounds = new double3(-Min[0], Max[1], Max[2]);
                var minBounds = new double3(-Max[0], Min[1], Min[2]);
                if (Normalized)
                {
                    switch (ComponentType)
                    {
                        case AccessorDataType.Byte:
                            maxBounds = math.max(maxBounds / sbyte.MaxValue, -1);
                            minBounds = math.max(minBounds / sbyte.MaxValue, -1);
                            break;
                        case AccessorDataType.UnsignedByte:
                            maxBounds /= byte.MaxValue;
                            minBounds /= byte.MaxValue;
                            break;
                        case AccessorDataType.Short:
                            maxBounds = math.max(maxBounds / short.MaxValue, -1);
                            minBounds = math.max(minBounds / short.MaxValue, -1);
                            break;
                        case AccessorDataType.UnsignedShort:
                            maxBounds /= ushort.MaxValue;
                            minBounds /= ushort.MaxValue;
                            break;
                        case AccessorDataType.UnsignedInt:
                            maxBounds /= uint.MaxValue;
                            minBounds /= uint.MaxValue;
                            break;
                    }
                }
                return new Bounds
                {
                    max = maxBounds.ToVector3(),
                    min = minBounds.ToVector3()
                };
            }
            return null;
        }

        /// <summary>
        /// True if the accessor is <a href="https://www.khronos.org/registry/glTF/specs/2.0/glTF-2.0.html#sparse-accessors">sparse</a>
        /// </summary>
        [JsonIgnore]
        public bool IsSparse => Sparse != null;

        /// <summary>
        /// Byte size of one element
        /// </summary>
        [JsonIgnore]
        public int ElementByteSize => GetAccessorAttributeTypeLength(Type.Value) * GetComponentTypeSize(ComponentType);

        /// <summary>
        /// Overall, byte size.
        /// Ignores interleaved or sparse accessors
        /// </summary>
        [JsonIgnore]
        public int ByteSize => ElementByteSize * Count;
    }
}
