// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

namespace GLTFast
{

    /// <summary>
    /// <see href="https://www.khronos.org/registry/glTF/specs/2.0/glTF-2.0.html#specifying-extensions">glTF Extensions</see>
    /// </summary>
    public enum Extension
    {
        /// <summary>
        /// <see href="https://github.com/KhronosGroup/glTF/blob/main/extensions/2.0/Khronos/KHR_draco_mesh_compression/README.md">KHR_draco_mesh_compression</see> glTF extension
        /// </summary>
        DracoMeshCompression,
        /// <summary>
        /// <see href="https://github.com/KhronosGroup/glTF/blob/main/extensions/2.0/Khronos/KHR_lights_punctual/README.md">KHR_lights_punctual</see> glTF extension
        /// </summary>
        LightsPunctual,
        /// <summary>
        /// <see href="https://github.com/KhronosGroup/glTF/tree/main/extensions/2.0/Archived/KHR_materials_pbrSpecularGlossiness">KHR_materials_pbrSpecularGlossiness</see> glTF extension
        /// </summary>
        MaterialsPbrSpecularGlossiness,
        /// <summary>
        /// <see href="https://github.com/KhronosGroup/glTF/blob/main/extensions/2.0/Khronos/KHR_materials_transmission/README.md">KHR_materials_transmission</see> glTF extension
        /// </summary>
        MaterialsTransmission,
        /// <summary>
        /// <see href="https://github.com/KhronosGroup/glTF/blob/main/extensions/2.0/Khronos/KHR_materials_unlit/README.md">KHR_materials_unlit</see> glTF extension
        /// </summary>
        MaterialsUnlit,
        /// <summary>
        /// <see href="https://github.com/KhronosGroup/glTF/blob/main/extensions/2.0/Vendor/EXT_mesh_gpu_instancing/README.md">EXT_mesh_gpu_instancing</see> glTF extension
        /// </summary>
        MeshGPUInstancing,
        /// <summary>
        /// <see href="https://github.com/KhronosGroup/glTF/blob/main/extensions/2.0/Khronos/KHR_mesh_quantization/README.md">KHR_mesh_quantization</see> glTF extension
        /// </summary>
        MeshQuantization,
        /// <summary>
        /// <see href="https://github.com/KhronosGroup/glTF/blob/main/extensions/2.0/Khronos/KHR_texture_basisu/README.md">KHR_texture_basisu</see> glTF extension
        /// </summary>
        TextureBasisUniversal,
        /// <summary>
        /// <see href="https://github.com/KhronosGroup/glTF/blob/main/extensions/2.0/Khronos/KHR_texture_transform/README.md">KHR_texture_transform</see> glTF extension
        /// </summary>
        TextureTransform,
        /// <summary>
        /// <see href="https://github.com/KhronosGroup/glTF/tree/main/extensions/2.0/Khronos/KHR_materials_clearcoat">KHR_materials_clearcoat</see> glTF extension
        /// </summary>
        MaterialsClearcoat,
    }

    /// <summary>
    /// Collection of glTF extension names
    /// </summary>
    public static class ExtensionName
    {
        /// <summary>
        /// <see href="https://github.com/KhronosGroup/glTF/blob/main/extensions/2.0/Khronos/KHR_draco_mesh_compression/README.md">KHR_draco_mesh_compression</see> glTF extension
        /// </summary>
        public const string DracoMeshCompression = "KHR_draco_mesh_compression";
        /// <summary>
        /// <see href="https://github.com/KhronosGroup/glTF/tree/main/extensions/2.0/Archived/KHR_materials_pbrSpecularGlossiness">KHR_materials_pbrSpecularGlossiness</see> glTF extension
        /// </summary>
        public const string MaterialsPbrSpecularGlossiness = "KHR_materials_pbrSpecularGlossiness";
        /// <summary>
        /// <see href="https://github.com/KhronosGroup/glTF/blob/main/extensions/2.0/Khronos/KHR_materials_transmission/README.md">KHR_materials_transmission</see> glTF extension
        /// </summary>
        public const string MaterialsTransmission = "KHR_materials_transmission";
        /// <summary>
        /// <see href="https://github.com/KhronosGroup/glTF/blob/main/extensions/2.0/Khronos/KHR_materials_unlit/README.md">KHR_materials_unlit</see> glTF extension
        /// </summary>
        public const string MaterialsUnlit = "KHR_materials_unlit";
        /// <summary>
        /// <see href="https://github.com/KhronosGroup/glTF/blob/main/extensions/2.0/Vendor/EXT_mesh_gpu_instancing/README.md">EXT_mesh_gpu_instancing</see> glTF extension
        /// </summary>
        public const string MeshGPUInstancing = "EXT_mesh_gpu_instancing";
        /// <summary>
        /// <see href="https://github.com/KhronosGroup/glTF/blob/main/extensions/2.0/Vendor/EXT_meshopt_compression/README.md">EXT_meshopt_compression</see> glTF extension
        /// </summary>
        public const string MeshoptCompression = "EXT_meshopt_compression";
        /// <summary>
        /// <see href="https://github.com/KhronosGroup/glTF/blob/main/extensions/2.0/Khronos/KHR_mesh_quantization/README.md">KHR_mesh_quantization</see> glTF extension
        /// </summary>
        public const string MeshQuantization = "KHR_mesh_quantization";
        /// <summary>
        /// <see href="https://github.com/KhronosGroup/glTF/blob/main/extensions/2.0/Khronos/KHR_texture_basisu/README.md">KHR_texture_basisu</see> glTF extension
        /// </summary>
        public const string TextureBasisUniversal = "KHR_texture_basisu";
        /// <summary>
        /// <see href="https://github.com/KhronosGroup/glTF/blob/main/extensions/2.0/Khronos/KHR_texture_transform/README.md">KHR_texture_transform</see> glTF extension
        /// </summary>
        public const string TextureTransform = "KHR_texture_transform";
        /// <summary>
        /// <see href="https://github.com/KhronosGroup/glTF/tree/main/extensions/2.0/Khronos/KHR_lights_punctual">KHR_lights_punctual</see> glTF extension
        /// </summary>
        public const string LightsPunctual = "KHR_lights_punctual";
        /// <summary>
        /// <see href="https://github.com/KhronosGroup/glTF/tree/main/extensions/2.0/Khronos/KHR_materials_clearcoat">KHR_materials_clearcoat</see> glTF extension
        /// </summary>
        public const string MaterialsClearcoat = "KHR_materials_clearcoat";

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
                case Extension.MaterialsClearcoat:
                    return MaterialsClearcoat;
                default:
                    return null;
            }
        }
    }
}
