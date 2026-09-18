// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using Unity.Collections;

namespace GLTFast
{
    enum ChunkFormat : uint
    {
        Json = 0x4e4f534a,
        Binary = 0x004e4942
    }

    /// <summary>
    /// Generic glTF constants and utility methods.
    /// </summary>
    public static class GltfGlobals
    {

        /// <summary>
        /// glTF-Binary file extension
        /// </summary>
        public const string GlbExt = ".glb";

        /// <summary>
        /// glTF file extension
        /// </summary>
        public const string GltfExt = ".gltf";

        /// <summary>
        /// glTF package name
        /// </summary>
        public const string GltfPackageName = "com.atteneder.gltfast";

        /// <summary>
        /// First four bytes of a glTF-Binary file are made up of this signature
        /// Represents glTF in ASCII
        /// </summary>
        public const uint GltfBinaryMagic = 0x46546c67;

        /// <summary>
        /// Figures out if a byte array contains data of a glTF-Binary
        /// </summary>
        /// <param name="data">data buffer</param>
        /// <returns>True if the data is a glTF-Binary, false otherwise</returns>
        public static bool IsGltfBinary(byte[] data)
        {
            var magic = BitConverter.ToUInt32(data, 0);
            return magic == GltfBinaryMagic;
        }

        /// <summary>
        /// Figures out if a byte array contains data of a glTF-Binary
        /// </summary>
        /// <param name="data">data buffer</param>
        /// <returns>True if the data is a glTF-Binary, false otherwise</returns>
        public static bool IsGltfBinary(NativeArray<byte>.ReadOnly data)
        {
            if (data.Length < 4)
            {
                return false;
            }
            var magic = data[0] | (uint)data[1] << 8 | (uint)data[2] << 16 | (uint)data[3] << 24;
            return magic == GltfBinaryMagic;
        }
    }
}
