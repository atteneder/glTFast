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

namespace GLTFast
{

    enum ImageFormat
    {
        Unknown,
        PNG,
        Jpeg,
        Ktx
    }

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
    }
}
