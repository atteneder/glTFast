// SPDX-FileCopyrightText: 2026 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Buffers;
using Unity.Cloud.Gltfast.Text.Json;
using Unity.Cloud.Gltfast.Text.Json.Serialization;

namespace Unity.Cloud.Gltfast.Objects
{

    abstract class EnumOrRawValueConverter<TEnum> : JsonConverter<EnumOrRawValue<TEnum>> where TEnum : struct, Enum
    {
        const int k_MaxStackByteCount = 256;

        protected abstract bool TryReadEnum(ReadOnlySpan<byte> utf8Value, out TEnum result);
        protected abstract void WriteEnum(ref Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options);

        public override EnumOrRawValue<TEnum> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String)
            {
                throw new JsonException("Expected string token.");
            }

            if (TryReadEnumValue(ref reader, out var result))
            {
                return new EnumOrRawValue<TEnum>(result);
            }

            var maxByteCount = reader.HasValueSequence
                ? checked((int)reader.ValueSequence.Length)
                : reader.ValueSpan.Length;
            var buffer = new byte[maxByteCount];
            var bytesWritten = reader.CopyString(buffer);
            if (bytesWritten != maxByteCount)
            {
                Array.Resize(ref buffer, bytesWritten);
            }
            reader.Skip(); // Correctly advance past the string token
            return new EnumOrRawValue<TEnum>(buffer);
        }

        bool TryReadEnumValue(ref Utf8JsonReader reader, out TEnum result)
        {
            // Fast path: the value is a single contiguous span with no escapes.
            if (!reader.HasValueSequence && !reader.ValueIsEscaped)
            {
                return TryReadEnum(reader.ValueSpan, out result);
            }

            // Fallback: the value is split across multiple buffer segments and/or contains escape
            // sequences. Reassemble the unescaped bytes into a contiguous buffer before matching.
            var length = reader.HasValueSequence
                ? checked((int)reader.ValueSequence.Length)
                : reader.ValueSpan.Length;
            if (length > k_MaxStackByteCount)
            {
                result = default;
                return false;
            }

            Span<byte> buffer = stackalloc byte[length];
            var bytesWritten = reader.CopyString(buffer);
            return TryReadEnum(buffer[..bytesWritten], out result);
        }

        public override void Write(Utf8JsonWriter writer, EnumOrRawValue<TEnum> value, JsonSerializerOptions options)
        {
            if (value.RawValue == null)
            {
                WriteEnum(ref writer, value.Value, options);
            }
            else
            {
                writer.WriteStringValue(value.RawValue);
            }
        }
    }

    class AccessorTypeValueConverter : EnumOrRawValueConverter<AccessorType>
    {
        static ReadOnlySpan<byte> k_Scalar => new byte[] { 0x53, 0x43, 0x41, 0x4C, 0x41, 0x52 };
        static ReadOnlySpan<byte> k_Vector2 => new byte[] { 0x56, 0x45, 0x43, 0x32 };
        static ReadOnlySpan<byte> k_Vector3 => new byte[] { 0x56, 0x45, 0x43, 0x33 };
        static ReadOnlySpan<byte> k_Vector4 => new byte[] { 0x56, 0x45, 0x43, 0x34 };
        static ReadOnlySpan<byte> k_Matrix2x2 => new byte[] { 0x4D, 0x41, 0x54, 0x32 };
        static ReadOnlySpan<byte> k_Matrix3x3 => new byte[] { 0x4D, 0x41, 0x54, 0x33 };
        static ReadOnlySpan<byte> k_Matrix4x4 => new byte[] { 0x4D, 0x41, 0x54, 0x34 };

        protected override bool TryReadEnum(ReadOnlySpan<byte> utf8Value, out AccessorType result)
        {
            if (utf8Value.SequenceEqual(k_Scalar)) { result = AccessorType.Scalar; return true; }
            if (utf8Value.SequenceEqual(k_Vector2)) { result = AccessorType.Vector2; return true; }
            if (utf8Value.SequenceEqual(k_Vector3)) { result = AccessorType.Vector3; return true; }
            if (utf8Value.SequenceEqual(k_Vector4)) { result = AccessorType.Vector4; return true; }
            if (utf8Value.SequenceEqual(k_Matrix2x2)) { result = AccessorType.Matrix2x2; return true; }
            if (utf8Value.SequenceEqual(k_Matrix3x3)) { result = AccessorType.Matrix3x3; return true; }
            if (utf8Value.SequenceEqual(k_Matrix4x4)) { result = AccessorType.Matrix4x4; return true; }

            result = default;
            return false;
        }

        protected override void WriteEnum(ref Utf8JsonWriter writer, AccessorType value, JsonSerializerOptions options)
        {
            switch (value)
            {
                case AccessorType.Scalar: writer.WriteStringValue(k_Scalar); break;
                case AccessorType.Vector2: writer.WriteStringValue(k_Vector2); break;
                case AccessorType.Vector3: writer.WriteStringValue(k_Vector3); break;
                case AccessorType.Vector4: writer.WriteStringValue(k_Vector4); break;
                case AccessorType.Matrix2x2: writer.WriteStringValue(k_Matrix2x2); break;
                case AccessorType.Matrix3x3: writer.WriteStringValue(k_Matrix3x3); break;
                case AccessorType.Matrix4x4: writer.WriteStringValue(k_Matrix4x4); break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
        }
    }

    class AlphaModeValueConverter : EnumOrRawValueConverter<AlphaMode>
    {
        static ReadOnlySpan<byte> k_Opaque => new byte[] { 0x4F, 0x50, 0x41, 0x51, 0x55, 0x45 };
        static ReadOnlySpan<byte> k_Mask => new byte[] { 0x4D, 0x41, 0x53, 0x4B };
        static ReadOnlySpan<byte> k_Blend => new byte[] { 0x42, 0x4C, 0x45, 0x4E, 0x44 };

        protected override bool TryReadEnum(ReadOnlySpan<byte> utf8Value, out AlphaMode result)
        {
            if (utf8Value.SequenceEqual(k_Opaque)) { result = AlphaMode.Opaque; return true; }
            if (utf8Value.SequenceEqual(k_Mask)) { result = AlphaMode.Mask; return true; }
            if (utf8Value.SequenceEqual(k_Blend)) { result = AlphaMode.Blend; return true; }

            result = default;
            return false;
        }

        protected override void WriteEnum(ref Utf8JsonWriter writer, AlphaMode value, JsonSerializerOptions options)
        {
            switch (value)
            {
                case AlphaMode.Opaque: writer.WriteStringValue(k_Opaque); break;
                case AlphaMode.Mask: writer.WriteStringValue(k_Mask); break;
                case AlphaMode.Blend: writer.WriteStringValue(k_Blend); break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
        }
    }

    class CameraTypeValueConverter : EnumOrRawValueConverter<CameraType>
    {
        static ReadOnlySpan<byte> k_Orthographic => new byte[] { 0x6F, 0x72, 0x74, 0x68, 0x6F, 0x67, 0x72, 0x61, 0x70, 0x68, 0x69, 0x63 };
        static ReadOnlySpan<byte> k_Perspective => new byte[] { 0x70, 0x65, 0x72, 0x73, 0x70, 0x65, 0x63, 0x74, 0x69, 0x76, 0x65 };

        protected override bool TryReadEnum(ReadOnlySpan<byte> utf8Value, out CameraType result)
        {
            if (utf8Value.SequenceEqual(k_Perspective)) { result = CameraType.Perspective; return true; }
            if (utf8Value.SequenceEqual(k_Orthographic)) { result = CameraType.Orthographic; return true; }

            result = default;
            return false;
        }

        protected override void WriteEnum(ref Utf8JsonWriter writer, CameraType value, JsonSerializerOptions options)
        {
            switch (value)
            {
                case CameraType.Orthographic: writer.WriteStringValue(k_Orthographic); break;
                case CameraType.Perspective: writer.WriteStringValue(k_Perspective); break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
        }
    }

    class ImageMimeTypeValueConverter : EnumOrRawValueConverter<ImageMimeType>
    {
        static ReadOnlySpan<byte> k_Jpeg => new byte[] { 0x69, 0x6D, 0x61, 0x67, 0x65, 0x2F, 0x6A, 0x70, 0x65, 0x67 };
        static ReadOnlySpan<byte> k_Png => new byte[] { 0x69, 0x6D, 0x61, 0x67, 0x65, 0x2F, 0x70, 0x6E, 0x67 };
        static ReadOnlySpan<byte> k_Ktx2 => new byte[] { 0x69, 0x6D, 0x61, 0x67, 0x65, 0x2F, 0x6B, 0x74, 0x78, 0x32 };
        static ReadOnlySpan<byte> k_WebP => new byte[] { 0x69, 0x6D, 0x61, 0x67, 0x65, 0x2F, 0x77, 0x65, 0x62, 0x70 };

        protected override bool TryReadEnum(ReadOnlySpan<byte> utf8Value, out ImageMimeType result)
        {
            if (utf8Value.SequenceEqual(k_Jpeg)) { result = ImageMimeType.Jpeg; return true; }
            if (utf8Value.SequenceEqual(k_Png)) { result = ImageMimeType.Png; return true; }
            if (utf8Value.SequenceEqual(k_Ktx2)) { result = ImageMimeType.Ktx2; return true; }
            if (utf8Value.SequenceEqual(k_WebP)) { result = ImageMimeType.WebP; return true; }

            result = default;
            return false;
        }

        protected override void WriteEnum(ref Utf8JsonWriter writer, ImageMimeType value, JsonSerializerOptions options)
        {
            switch (value)
            {
                case ImageMimeType.Jpeg: writer.WriteStringValue(k_Jpeg); break;
                case ImageMimeType.Png: writer.WriteStringValue(k_Png); break;
                case ImageMimeType.Ktx2: writer.WriteStringValue(k_Ktx2); break;
                case ImageMimeType.WebP: writer.WriteStringValue(k_WebP); break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
        }
    }

    class ExtensionValueConverter : EnumOrRawValueConverter<Extension>
    {
        static ReadOnlySpan<byte> k_DracoMeshCompression => new byte[] { 0x4B, 0x48, 0x52, 0x5F, 0x64, 0x72, 0x61, 0x63, 0x6F, 0x5F, 0x6D, 0x65, 0x73, 0x68, 0x5F, 0x63, 0x6F, 0x6D, 0x70, 0x72, 0x65, 0x73, 0x73, 0x69, 0x6F, 0x6E };
        static ReadOnlySpan<byte> k_LightsPunctual => new byte[] { 0x4B, 0x48, 0x52, 0x5F, 0x6C, 0x69, 0x67, 0x68, 0x74, 0x73, 0x5F, 0x70, 0x75, 0x6E, 0x63, 0x74, 0x75, 0x61, 0x6C };
        static ReadOnlySpan<byte> k_MaterialsPbrSpecularGlossiness => new byte[] { 0x4B, 0x48, 0x52, 0x5F, 0x6D, 0x61, 0x74, 0x65, 0x72, 0x69, 0x61, 0x6C, 0x73, 0x5F, 0x70, 0x62, 0x72, 0x53, 0x70, 0x65, 0x63, 0x75, 0x6C, 0x61, 0x72, 0x47, 0x6C, 0x6F, 0x73, 0x73, 0x69, 0x6E, 0x65, 0x73, 0x73 };
        static ReadOnlySpan<byte> k_MaterialsTransmission => new byte[] { 0x4B, 0x48, 0x52, 0x5F, 0x6D, 0x61, 0x74, 0x65, 0x72, 0x69, 0x61, 0x6C, 0x73, 0x5F, 0x74, 0x72, 0x61, 0x6E, 0x73, 0x6D, 0x69, 0x73, 0x73, 0x69, 0x6F, 0x6E };
        static ReadOnlySpan<byte> k_MaterialsUnlit => new byte[] { 0x4B, 0x48, 0x52, 0x5F, 0x6D, 0x61, 0x74, 0x65, 0x72, 0x69, 0x61, 0x6C, 0x73, 0x5F, 0x75, 0x6E, 0x6C, 0x69, 0x74 };
        static ReadOnlySpan<byte> k_MeshGPUInstancing => new byte[] { 0x45, 0x58, 0x54, 0x5F, 0x6D, 0x65, 0x73, 0x68, 0x5F, 0x67, 0x70, 0x75, 0x5F, 0x69, 0x6E, 0x73, 0x74, 0x61, 0x6E, 0x63, 0x69, 0x6E, 0x67 };
        static ReadOnlySpan<byte> k_MeshQuantization => new byte[] { 0x4B, 0x48, 0x52, 0x5F, 0x6D, 0x65, 0x73, 0x68, 0x5F, 0x71, 0x75, 0x61, 0x6E, 0x74, 0x69, 0x7A, 0x61, 0x74, 0x69, 0x6F, 0x6E };
        static ReadOnlySpan<byte> k_TextureBasisUniversal => new byte[] { 0x4B, 0x48, 0x52, 0x5F, 0x74, 0x65, 0x78, 0x74, 0x75, 0x72, 0x65, 0x5F, 0x62, 0x61, 0x73, 0x69, 0x73, 0x75 };
        static ReadOnlySpan<byte> k_TextureTransform => new byte[] { 0x4B, 0x48, 0x52, 0x5F, 0x74, 0x65, 0x78, 0x74, 0x75, 0x72, 0x65, 0x5F, 0x74, 0x72, 0x61, 0x6E, 0x73, 0x66, 0x6F, 0x72, 0x6D };
        static ReadOnlySpan<byte> k_MaterialsClearcoat => new byte[] { 0x4B, 0x48, 0x52, 0x5F, 0x6D, 0x61, 0x74, 0x65, 0x72, 0x69, 0x61, 0x6C, 0x73, 0x5F, 0x63, 0x6C, 0x65, 0x61, 0x72, 0x63, 0x6F, 0x61, 0x74 };
        static ReadOnlySpan<byte> k_MaterialsVariants => new byte[] { 0x4B, 0x48, 0x52, 0x5F, 0x6D, 0x61, 0x74, 0x65, 0x72, 0x69, 0x61, 0x6C, 0x73, 0x5F, 0x76, 0x61, 0x72, 0x69, 0x61, 0x6E, 0x74, 0x73 };
        static ReadOnlySpan<byte> k_MeshoptCompression => new byte[] { 0x45, 0x58, 0x54, 0x5F, 0x6D, 0x65, 0x73, 0x68, 0x6F, 0x70, 0x74, 0x5F, 0x63, 0x6F, 0x6D, 0x70, 0x72, 0x65, 0x73, 0x73, 0x69, 0x6F, 0x6E };
        static ReadOnlySpan<byte> k_MaterialsIor => new byte[] { 0x4B, 0x48, 0x52, 0x5F, 0x6D, 0x61, 0x74, 0x65, 0x72, 0x69, 0x61, 0x6C, 0x73, 0x5F, 0x69, 0x6F, 0x72 };
        static ReadOnlySpan<byte> k_MaterialsSheen => new byte[] { 0x4B, 0x48, 0x52, 0x5F, 0x6D, 0x61, 0x74, 0x65, 0x72, 0x69, 0x61, 0x6C, 0x73, 0x5F, 0x73, 0x68, 0x65, 0x65, 0x6E };
        static ReadOnlySpan<byte> k_MaterialsSpecular => new byte[] { 0x4B, 0x48, 0x52, 0x5F, 0x6D, 0x61, 0x74, 0x65, 0x72, 0x69, 0x61, 0x6C, 0x73, 0x5F, 0x73, 0x70, 0x65, 0x63, 0x75, 0x6C, 0x61, 0x72 };
        static ReadOnlySpan<byte> k_TextureWebP => new byte[] { 0x45, 0x58, 0x54, 0x5F, 0x74, 0x65, 0x78, 0x74, 0x75, 0x72, 0x65, 0x5F, 0x77, 0x65, 0x62, 0x70 };

        protected override bool TryReadEnum(ReadOnlySpan<byte> utf8Value, out Extension result)
        {
            if (utf8Value.SequenceEqual(k_DracoMeshCompression)) { result = Extension.DracoMeshCompression; return true; }
            if (utf8Value.SequenceEqual(k_LightsPunctual)) { result = Extension.LightsPunctual; return true; }
            if (utf8Value.SequenceEqual(k_MaterialsPbrSpecularGlossiness)) { result = Extension.MaterialsPbrSpecularGlossiness; return true; }
            if (utf8Value.SequenceEqual(k_MaterialsTransmission)) { result = Extension.MaterialsTransmission; return true; }
            if (utf8Value.SequenceEqual(k_MaterialsUnlit)) { result = Extension.MaterialsUnlit; return true; }
            if (utf8Value.SequenceEqual(k_MeshGPUInstancing)) { result = Extension.MeshGPUInstancing; return true; }
            if (utf8Value.SequenceEqual(k_MeshQuantization)) { result = Extension.MeshQuantization; return true; }
            if (utf8Value.SequenceEqual(k_TextureBasisUniversal)) { result = Extension.TextureBasisUniversal; return true; }
            if (utf8Value.SequenceEqual(k_TextureTransform)) { result = Extension.TextureTransform; return true; }
            if (utf8Value.SequenceEqual(k_MaterialsClearcoat)) { result = Extension.MaterialsClearcoat; return true; }
            if (utf8Value.SequenceEqual(k_MaterialsVariants)) { result = Extension.MaterialsVariants; return true; }
            if (utf8Value.SequenceEqual(k_MeshoptCompression)) { result = Extension.MeshoptCompression; return true; }
            if (utf8Value.SequenceEqual(k_MaterialsIor)) { result = Extension.MaterialsIor; return true; }
            if (utf8Value.SequenceEqual(k_MaterialsSheen)) { result = Extension.MaterialsSheen; return true; }
            if (utf8Value.SequenceEqual(k_MaterialsSpecular)) { result = Extension.MaterialsSpecular; return true; }
            if (utf8Value.SequenceEqual(k_TextureWebP)) { result = Extension.TextureWebP; return true; }

            result = default;
            return false;
        }

        protected override void WriteEnum(ref Utf8JsonWriter writer, Extension value, JsonSerializerOptions options)
        {
            switch (value)
            {
                case Extension.DracoMeshCompression: writer.WriteStringValue(k_DracoMeshCompression); break;
                case Extension.LightsPunctual: writer.WriteStringValue(k_LightsPunctual); break;
                case Extension.MaterialsPbrSpecularGlossiness: writer.WriteStringValue(k_MaterialsPbrSpecularGlossiness); break;
                case Extension.MaterialsTransmission: writer.WriteStringValue(k_MaterialsTransmission); break;
                case Extension.MaterialsUnlit: writer.WriteStringValue(k_MaterialsUnlit); break;
                case Extension.MeshGPUInstancing: writer.WriteStringValue(k_MeshGPUInstancing); break;
                case Extension.MeshQuantization: writer.WriteStringValue(k_MeshQuantization); break;
                case Extension.TextureBasisUniversal: writer.WriteStringValue(k_TextureBasisUniversal); break;
                case Extension.TextureTransform: writer.WriteStringValue(k_TextureTransform); break;
                case Extension.MaterialsClearcoat: writer.WriteStringValue(k_MaterialsClearcoat); break;
                case Extension.MaterialsVariants: writer.WriteStringValue(k_MaterialsVariants); break;
                case Extension.MeshoptCompression: writer.WriteStringValue(k_MeshoptCompression); break;
                case Extension.MaterialsIor: writer.WriteStringValue(k_MaterialsIor); break;
                case Extension.MaterialsSheen: writer.WriteStringValue(k_MaterialsSheen); break;
                case Extension.MaterialsSpecular: writer.WriteStringValue(k_MaterialsSpecular); break;
                case Extension.TextureWebP: writer.WriteStringValue(k_TextureWebP); break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
        }
    }

    class LightTypeValueConverter : EnumOrRawValueConverter<LightType>
    {
        static ReadOnlySpan<byte> k_Spot => new byte[] { 0x73, 0x70, 0x6F, 0x74 };
        static ReadOnlySpan<byte> k_Directional => new byte[] { 0x64, 0x69, 0x72, 0x65, 0x63, 0x74, 0x69, 0x6F, 0x6E, 0x61, 0x6C };
        static ReadOnlySpan<byte> k_Point => new byte[] { 0x70, 0x6F, 0x69, 0x6E, 0x74 };

        protected override bool TryReadEnum(ReadOnlySpan<byte> utf8Value, out LightType result)
        {
            if (utf8Value.SequenceEqual(k_Spot)) { result = LightType.Spot; return true; }
            if (utf8Value.SequenceEqual(k_Directional)) { result = LightType.Directional; return true; }
            if (utf8Value.SequenceEqual(k_Point)) { result = LightType.Point; return true; }

            result = default;
            return false;
        }

        protected override void WriteEnum(ref Utf8JsonWriter writer, LightType value, JsonSerializerOptions options)
        {
            switch (value)
            {
                case LightType.Spot: writer.WriteStringValue(k_Spot); break;
                case LightType.Directional: writer.WriteStringValue(k_Directional); break;
                case LightType.Point: writer.WriteStringValue(k_Point); break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
        }
    }

#if UNITY_ANIMATION || GLTFAST_ANIMATION
    class AnimationPathValueConverter : EnumOrRawValueConverter<AnimationPath>
    {
        static ReadOnlySpan<byte> k_Translation => new byte[] { 0x74, 0x72, 0x61, 0x6E, 0x73, 0x6C, 0x61, 0x74, 0x69, 0x6F, 0x6E };
        static ReadOnlySpan<byte> k_Rotation => new byte[] { 0x72, 0x6F, 0x74, 0x61, 0x74, 0x69, 0x6F, 0x6E };
        static ReadOnlySpan<byte> k_Scale => new byte[] { 0x73, 0x63, 0x61, 0x6C, 0x65 };
        static ReadOnlySpan<byte> k_Weights => new byte[] { 0x77, 0x65, 0x69, 0x67, 0x68, 0x74, 0x73 };
        static ReadOnlySpan<byte> k_Pointer => new byte[] { 0x70, 0x6F, 0x69, 0x6E, 0x74, 0x65, 0x72 };

        protected override bool TryReadEnum(ReadOnlySpan<byte> utf8Value, out AnimationPath result)
        {
            if (utf8Value.SequenceEqual(k_Translation)) { result = AnimationPath.Translation; return true; }
            if (utf8Value.SequenceEqual(k_Rotation)) { result = AnimationPath.Rotation; return true; }
            if (utf8Value.SequenceEqual(k_Scale)) { result = AnimationPath.Scale; return true; }
            if (utf8Value.SequenceEqual(k_Weights)) { result = AnimationPath.Weights; return true; }
            if (utf8Value.SequenceEqual(k_Pointer)) { result = AnimationPath.Pointer; return true; }

            result = default;
            return false;
        }

        protected override void WriteEnum(ref Utf8JsonWriter writer, AnimationPath value, JsonSerializerOptions options)
        {
            switch (value)
            {
                case AnimationPath.Translation: writer.WriteStringValue(k_Translation); break;
                case AnimationPath.Rotation: writer.WriteStringValue(k_Rotation); break;
                case AnimationPath.Scale: writer.WriteStringValue(k_Scale); break;
                case AnimationPath.Weights: writer.WriteStringValue(k_Weights); break;
                case AnimationPath.Pointer: writer.WriteStringValue(k_Pointer); break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
        }
    }

    class InterpolationValueConverter : EnumOrRawValueConverter<Interpolation>
    {
        static ReadOnlySpan<byte> k_Linear => new byte[] { 0x4C, 0x49, 0x4E, 0x45, 0x41, 0x52 };
        static ReadOnlySpan<byte> k_Step => new byte[] { 0x53, 0x54, 0x45, 0x50 };
        static ReadOnlySpan<byte> k_CubicSpline => new byte[] { 0x43, 0x55, 0x42, 0x49, 0x43, 0x53, 0x50, 0x4C, 0x49, 0x4E, 0x45 };

        protected override bool TryReadEnum(ReadOnlySpan<byte> utf8Value, out Interpolation result)
        {
            if (utf8Value.SequenceEqual(k_Linear)) { result = Interpolation.Linear; return true; }
            if (utf8Value.SequenceEqual(k_Step)) { result = Interpolation.Step; return true; }
            if (utf8Value.SequenceEqual(k_CubicSpline)) { result = Interpolation.CubicSpline; return true; }

            result = default;
            return false;
        }

        protected override void WriteEnum(ref Utf8JsonWriter writer, Interpolation value, JsonSerializerOptions options)
        {
            switch (value)
            {
                case Interpolation.Linear: writer.WriteStringValue(k_Linear); break;
                case Interpolation.Step: writer.WriteStringValue(k_Step); break;
                case Interpolation.CubicSpline: writer.WriteStringValue(k_CubicSpline); break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
        }
    }
#endif
}
