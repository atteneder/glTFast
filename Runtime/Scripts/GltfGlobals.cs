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

namespace GLTFast {
    
    enum ImageFormat {
        Unknown,
        PNG,
        Jpeg,
        KTX
    }
    
    enum ChunkFormat : uint {
        JSON = 0x4e4f534a,
        BIN = 0x004e4942
    }

    public static class GltfGlobals {
        
        /// <summary>
        /// glTF-Binary file extension
        /// </summary>
        internal const string glbExt = ".glb";
        
        /// <summary>
        /// glTF file extension
        /// </summary>
        internal const string gltfExt = ".gltf";
        
        /// <summary>
        /// First four bytes of a glTF-Binary file are made up of this signature
        /// Represents glTF in ASCII
        /// </summary>
        internal const uint GLB_MAGIC = 0x46546c67; 
        
        /// <summary>
        /// Figures out if a byte array contains data of a glTF-Binary
        /// </summary>
        /// <param name="data">data buffer</param>
        /// <returns>True if the data is a glTF-Binary, false otherwise</returns>
        public static bool IsGltfBinary(byte[] data) {
            var magic = BitConverter.ToUInt32( data, 0 );
            return magic == GLB_MAGIC;
        }
    }
}
