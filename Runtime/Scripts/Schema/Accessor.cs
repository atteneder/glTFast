// Copyright 2020-2021 Andreas Atteneder
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
using UnityEngine;
using UnityEngine.Assertions;

// GLTF_EXPORT
using UnityEngine.Rendering;

namespace GLTFast.Schema {

    public enum GLTFComponentType
    {
        Byte = 5120,
        UnsignedByte = 5121,
        Short = 5122,
        UnsignedShort = 5123,
        UnsignedInt = 5125,
        Float = 5126
    }

    public enum GLTFAccessorAttributeType : byte
    {
        Undefined,
        SCALAR,
        VEC2,
        VEC3,
        VEC4,
        MAT2,
        MAT3,
        MAT4
    }

    [System.Serializable]
    public class Accessor {
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
        public GLTFComponentType componentType;

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
        [UnityEngine.SerializeField]
        string type;

        [NonSerialized]
        GLTFAccessorAttributeType _typeEnum = GLTFAccessorAttributeType.Undefined;
        
        public GLTFAccessorAttributeType typeEnum {
            get {
                if (_typeEnum != GLTFAccessorAttributeType.Undefined) {
                    return _typeEnum;
                } else if (!string.IsNullOrEmpty (type)) {
                    _typeEnum = (GLTFAccessorAttributeType)System.Enum.Parse (typeof(GLTFAccessorAttributeType), type, true);
                    type = null;
                    return _typeEnum;
                } else {
                    return GLTFAccessorAttributeType.Undefined;
                }
            }
            set {
                _typeEnum = value;
                type = value.ToString();
            }
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

        public static int GetComponentTypeSize( GLTFComponentType componentType ) {
            switch (componentType) {
                case GLTFComponentType.Byte:
                case GLTFComponentType.UnsignedByte:
                    return 1;
                case GLTFComponentType.Short:
                case GLTFComponentType.UnsignedShort:
                    return 2;
                case GLTFComponentType.Float:
                case GLTFComponentType.UnsignedInt:
                    return 4;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), componentType, null);
            }
        }
        
        public static GLTFComponentType GetComponentType(VertexAttributeFormat format) {
            switch (format) {
                case VertexAttributeFormat.Float32:
                case VertexAttributeFormat.Float16:
                    return GLTFComponentType.Float;
                case VertexAttributeFormat.UNorm8:
                case VertexAttributeFormat.UInt8:
                    return GLTFComponentType.UnsignedByte;
                case VertexAttributeFormat.SNorm8:
                case VertexAttributeFormat.SInt8:
                    return GLTFComponentType.Byte;
                case VertexAttributeFormat.UNorm16:
                case VertexAttributeFormat.UInt16:
                    return GLTFComponentType.UnsignedShort;
                case VertexAttributeFormat.SNorm16:
                case VertexAttributeFormat.SInt16:
                    return GLTFComponentType.Short;
                case VertexAttributeFormat.UInt32:
                case VertexAttributeFormat.SInt32:
                    return GLTFComponentType.UnsignedInt;
                default:
                    throw new ArgumentOutOfRangeException(nameof(format), format, null);
            }
        }

        public static GLTFAccessorAttributeType GetAccessorAttributeType(int dimension) {
            switch (dimension) {
                case 1:
                    return GLTFAccessorAttributeType.SCALAR;
                case 2:
                    return GLTFAccessorAttributeType.VEC2;
                case 3:
                    return GLTFAccessorAttributeType.VEC3;
                case 4:
                    return GLTFAccessorAttributeType.VEC4;
                default:
                    throw new ArgumentOutOfRangeException(nameof(dimension), dimension, null);
            }
        }
        
        public static int GetAccessorAttributeTypeLength( GLTFAccessorAttributeType type ) {
            switch (type)
            {
                case GLTFAccessorAttributeType.SCALAR:
                    return 1;
                case GLTFAccessorAttributeType.VEC2:
                    return 2;
                case GLTFAccessorAttributeType.VEC3:
                    return 3;
                case GLTFAccessorAttributeType.VEC4:
                case GLTFAccessorAttributeType.MAT2:
                    return 4;
                case GLTFAccessorAttributeType.MAT3:
                    return 9;
                case GLTFAccessorAttributeType.MAT4:
                    return 16;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        /// <summary>
        /// For 3D positional data, returns accessor's bounding box. Applies coordinate system transform (glTF to Unity)
        /// </summary>
        /// <returns>Bounding box enclosing the minimum and maximum values</returns>
        public Bounds? TryGetBounds() {
            Assert.AreEqual(GLTFAccessorAttributeType.VEC3 ,typeEnum);
            if (min != null && min.Length > 2 && max != null && max.Length > 2) {
                return new Bounds {
                    max = new Vector3(-min[0], max[1], max[2]),
                    min = new Vector3(-max[0], min[1], min[2])
                };
            }
            return null;
        }

        public bool isSparse => sparse != null;

        public void GltfSerialize(JsonWriter writer) {
            writer.AddObject();
            if (bufferView >= 0) {
                writer.AddProperty("bufferView", bufferView);
            }
            writer.AddProperty("componentType", (int)componentType);
            writer.AddProperty("count", count);
            writer.AddProperty("type", type);
            if (byteOffset > 0) {
                writer.AddProperty("byteOffset", byteOffset);
            }
            if (normalized) {
                writer.AddProperty("normalized", normalized);
            }
            if (max!=null) {
                writer.AddArrayProperty("max", max);
            }
            if (min!=null) {
                writer.AddArrayProperty("min", min);
            }

            if (sparse != null) {
                writer.AddProperty("sparse");
                sparse.GltfSerialize(writer);
                writer.Close();
            }
            writer.Close();
        }
    }
}
