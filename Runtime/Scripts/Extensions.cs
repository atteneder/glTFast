// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using Unity.Cloud.Gltfast.Objects;
using Unity.Cloud.Gltfast.Text.Json.Serialization;
using UnityEngine.Scripting.APIUpdating;

namespace Unity.Cloud.Gltfast
{

    /// <summary>
    /// <a href="https://www.khronos.org/registry/glTF/specs/2.0/glTF-2.0.html#specifying-extensions">glTF Extensions</a>
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter<Extension>))]
    [MovedFrom(true, sourceNamespace: "GLTFast", sourceAssembly: "glTFast")]
    public enum Extension
    {
        /// <summary>
        /// <a href="https://github.com/KhronosGroup/glTF/blob/main/extensions/2.0/Khronos/KHR_draco_mesh_compression/README.md">KHR_draco_mesh_compression</a> glTF extension
        /// </summary>
        [JsonStringEnumMemberName(ExtensionName.DracoMeshCompression)]
        DracoMeshCompression,
        /// <summary>
        /// <a href="https://github.com/KhronosGroup/glTF/blob/main/extensions/2.0/Khronos/KHR_lights_punctual/README.md">KHR_lights_punctual</a> glTF extension
        /// </summary>
        [JsonStringEnumMemberName(ExtensionName.LightsPunctual)]
        LightsPunctual,
        /// <summary>
        /// <a href="https://github.com/KhronosGroup/glTF/tree/main/extensions/2.0/Archived/KHR_materials_pbrSpecularGlossiness">KHR_materials_pbrSpecularGlossiness</a> glTF extension
        /// </summary>
        [JsonStringEnumMemberName(ExtensionName.MaterialsPbrSpecularGlossiness)]
        MaterialsPbrSpecularGlossiness,
        /// <summary>
        /// <a href="https://github.com/KhronosGroup/glTF/blob/main/extensions/2.0/Khronos/KHR_materials_transmission/README.md">KHR_materials_transmission</a> glTF extension
        /// </summary>
        [JsonStringEnumMemberName(ExtensionName.MaterialsTransmission)]
        MaterialsTransmission,
        /// <summary>
        /// <a href="https://github.com/KhronosGroup/glTF/blob/main/extensions/2.0/Khronos/KHR_materials_unlit/README.md">KHR_materials_unlit</a> glTF extension
        /// </summary>
        [JsonStringEnumMemberName(ExtensionName.MaterialsUnlit)]
        MaterialsUnlit,
        /// <summary>
        /// <a href="https://github.com/KhronosGroup/glTF/blob/main/extensions/2.0/Vendor/EXT_mesh_gpu_instancing/README.md">EXT_mesh_gpu_instancing</a> glTF extension
        /// </summary>
        [JsonStringEnumMemberName(ExtensionName.MeshGPUInstancing)]
        MeshGPUInstancing,
        /// <summary>
        /// <a href="https://github.com/KhronosGroup/glTF/blob/main/extensions/2.0/Khronos/KHR_mesh_quantization/README.md">KHR_mesh_quantization</a> glTF extension
        /// </summary>
        [JsonStringEnumMemberName(ExtensionName.MeshQuantization)]
        MeshQuantization,
        /// <summary>
        /// <a href="https://github.com/KhronosGroup/glTF/blob/main/extensions/2.0/Khronos/KHR_texture_basisu/README.md">KHR_texture_basisu</a> glTF extension
        /// </summary>
        [JsonStringEnumMemberName(ExtensionName.TextureBasisUniversal)]
        TextureBasisUniversal,
        /// <summary>
        /// <a href="https://github.com/KhronosGroup/glTF/blob/main/extensions/2.0/Khronos/KHR_texture_transform/README.md">KHR_texture_transform</a> glTF extension
        /// </summary>
        [JsonStringEnumMemberName(ExtensionName.TextureTransform)]
        TextureTransform,
        /// <summary>
        /// <a href="https://github.com/KhronosGroup/glTF/tree/main/extensions/2.0/Khronos/KHR_materials_clearcoat">KHR_materials_clearcoat</a> glTF extension
        /// </summary>
        [JsonStringEnumMemberName(ExtensionName.MaterialsClearcoat)]
        MaterialsClearcoat,
        /// <summary>
        /// <a href="https://github.com/KhronosGroup/glTF/tree/main/extensions/2.0/Khronos/KHR_materials_variants">KHR_materials_variants</a> glTF extension
        /// </summary>
        [JsonStringEnumMemberName(ExtensionName.MaterialsVariants)]
        MaterialsVariants,
        /// <summary>
        /// <a href="https://github.com/KhronosGroup/glTF/tree/main/extensions/2.0/Vendor/EXT_meshopt_compression">EXT_meshopt_compression</a> glTF extension
        /// </summary>
        [JsonStringEnumMemberName(ExtensionName.MeshoptCompression)]
        MeshoptCompression,
        /// <summary>
        /// <a href="https://github.com/KhronosGroup/glTF/tree/main/extensions/2.0/Khronos/KHR_materials_ior">KHR_materials_ior</a> glTF extension
        /// </summary>
        [JsonStringEnumMemberName(ExtensionName.MaterialsIor)]
        MaterialsIor,
        /// <summary>
        /// <a href="https://github.com/KhronosGroup/glTF/tree/main/extensions/2.0/Khronos/KHR_materials_sheen">KHR_materials_sheen</a> glTF extension
        /// </summary>
        [JsonStringEnumMemberName(ExtensionName.MaterialsSheen)]
        MaterialsSheen,
        /// <summary>
        /// <a href="https://github.com/KhronosGroup/glTF/tree/main/extensions/2.0/Khronos/KHR_materials_specular">KHR_materials_specular</a> glTF extension
        /// </summary>
        [JsonStringEnumMemberName(ExtensionName.MaterialsSpecular)]
        MaterialsSpecular,
        /// <summary>
        /// <a href="https://github.com/KhronosGroup/glTF/blob/main/extensions/2.0/Vendor/EXT_texture_webp/README.md">EXT_texture_webp</a> glTF extension
        /// </summary>
        [JsonStringEnumMemberName(ExtensionName.TextureWebP)]
        TextureWebP,
    }

    /// <summary>
    /// Collection of glTF extension names
    /// </summary>
    [MovedFrom(true, sourceNamespace: "GLTFast", sourceAssembly: "glTFast")]
    public static class ExtensionName
    {
        /// <inheritdoc cref="Extension.DracoMeshCompression"/>
        public const string DracoMeshCompression = "KHR_draco_mesh_compression";
        /// <inheritdoc cref="Extension.MaterialsPbrSpecularGlossiness"/>
        public const string MaterialsPbrSpecularGlossiness = "KHR_materials_pbrSpecularGlossiness";
        /// <inheritdoc cref="Extension.MaterialsTransmission"/>
        public const string MaterialsTransmission = "KHR_materials_transmission";
        /// <inheritdoc cref="Extension.MaterialsUnlit"/>
        public const string MaterialsUnlit = "KHR_materials_unlit";
        /// <inheritdoc cref="Extension.MeshGPUInstancing"/>
        public const string MeshGPUInstancing = "EXT_mesh_gpu_instancing";
        /// <inheritdoc cref="Extension.MeshoptCompression"/>
        public const string MeshoptCompression = "EXT_meshopt_compression";
        /// <inheritdoc cref="Extension.MeshQuantization"/>
        public const string MeshQuantization = "KHR_mesh_quantization";
        /// <inheritdoc cref="Extension.TextureBasisUniversal"/>
        public const string TextureBasisUniversal = "KHR_texture_basisu";
        /// <inheritdoc cref="Extension.TextureTransform"/>
        public const string TextureTransform = "KHR_texture_transform";
        /// <inheritdoc cref="Extension.TextureWebP"/>
        public const string TextureWebP = "EXT_texture_webp";
        /// <inheritdoc cref="Extension.LightsPunctual"/>
        public const string LightsPunctual = "KHR_lights_punctual";
        /// <inheritdoc cref="Extension.MaterialsClearcoat"/>
        public const string MaterialsClearcoat = "KHR_materials_clearcoat";
        /// <inheritdoc cref="Extension.MaterialsIor"/>
        public const string MaterialsIor = "KHR_materials_ior";
        /// <inheritdoc cref="Extension.MaterialsSheen"/>
        public const string MaterialsSheen = "KHR_materials_sheen";
        /// <inheritdoc cref="Extension.MaterialsSpecular"/>
        public const string MaterialsSpecular = "KHR_materials_specular";
        /// <inheritdoc cref="Extension.MaterialsVariants"/>
        public const string MaterialsVariants = "KHR_materials_variants";

        /// <summary>
        /// Returns the official name of the glTF extension
        /// </summary>
        /// <param name="extension">Extension enum value</param>
        /// <returns>Name of the glTF extension</returns>
        public static string GetName(this Extension extension)
        {
            switch (extension)
            {
                case Extension.DracoMeshCompression:
                    return DracoMeshCompression;
                case Extension.LightsPunctual:
                    return LightsPunctual;
                case Extension.MaterialsPbrSpecularGlossiness:
                    return MaterialsPbrSpecularGlossiness;
                case Extension.MaterialsTransmission:
                    return MaterialsTransmission;
                case Extension.MaterialsUnlit:
                    return MaterialsUnlit;
                case Extension.MeshGPUInstancing:
                    return MeshGPUInstancing;
                case Extension.MeshQuantization:
                    return MeshQuantization;
                case Extension.TextureBasisUniversal:
                    return TextureBasisUniversal;
                case Extension.TextureTransform:
                    return TextureTransform;
                case Extension.TextureWebP:
                    return TextureWebP;
                case Extension.MaterialsClearcoat:
                    return MaterialsClearcoat;
                case Extension.MaterialsVariants:
                    return MaterialsVariants;
                case Extension.MeshoptCompression:
                    return MeshoptCompression;
                case Extension.MaterialsIor:
                    return MaterialsIor;
                case Extension.MaterialsSpecular:
                    return MaterialsSpecular;
                case Extension.MaterialsSheen:
                    return MaterialsSheen;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Returns the official name of the glTF extension
        /// </summary>
        /// <param name="extension">Extension value</param>
        /// <returns>Name of the glTF extension</returns>
        public static string GetName(this EnumOrRawValue<Extension> extension)
        {
            var name = extension.RawValue != null
                ? System.Text.Encoding.UTF8.GetString(extension.RawValue)
                : extension.Value.GetName();
            return name;
        }
    }
}
