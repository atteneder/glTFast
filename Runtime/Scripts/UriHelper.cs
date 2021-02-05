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

namespace GLTFast {

    public static class UriHelper
    {
        const string GLB_EXT = ".glb";
        const string GLTF_EXT = ".gltf";

        public static Uri GetBaseUri( Uri uri ) {
            if(uri==null) return null;
            return new Uri(uri, ".");
        }

        /// <summary>
        /// Get URI that is potentially relative to another URI
        /// </summary>
        /// <param name="uri">Absolute or relative URI</param>
        /// <param name="baseUri">Base URI</param>
        /// <returns>Absolute URI that is potentially relative to baseUri</returns>
        public static Uri GetUriString( string uri, Uri baseUri ) {
            uri = Uri.UnescapeDataString(uri);
            if(Uri.TryCreate(uri, UriKind.Absolute, out var result)){
                return result;
            }
            if(baseUri!=null) return new Uri(baseUri,uri);
            return new Uri(uri,UriKind.RelativeOrAbsolute);
        }

        /// <summary>
        /// Detect glTF type from URI
        /// </summary>
        /// <param name="uri">Input URI</param>
        /// <returns>True if glTF-binary, False if glTF (JSON), null if not sure.</returns>
        public static bool? IsGltfBinary( Uri uri ) {
            string path = uri.IsAbsoluteUri ? uri.LocalPath : uri.OriginalString;
            var index = path.LastIndexOf('.',path.Length-1, Mathf.Min(5,path.Length) );
            if(index<0) return null;
            if(path.EndsWith(GLB_EXT, StringComparison.OrdinalIgnoreCase)) {
                return true;
            }
            if(path.EndsWith(GLTF_EXT, StringComparison.OrdinalIgnoreCase)) {
                return false;
            }
            return null;
        }

        /// string-based IsGltfBinary alternative
        /// Profiling result: Faster/less memory, but for .glb/.gltf just barely better (uknown ~2x)
        /// Downside: less convenient
        // public static bool? IsGltfBinary( string uri ) {
        //     // quick glTF-binary check
        //     if (uri.EndsWith(GLB_EXT, StringComparison.OrdinalIgnoreCase)) return true;
        //     if (uri.EndsWith(GLTF_EXT, StringComparison.OrdinalIgnoreCase)) return false;

        //     // thourough glTF-binary extension check that strips HTTP GET parameters
        //     int getIndex = uri.LastIndexOf('?');
        //     if (getIndex >= 0) {
        //         var ext = uri.Substring(getIndex - GLTF_EXT.Length, GLTF_EXT.Length);
        //         if(ext.EndsWith(GLB_EXT, StringComparison.OrdinalIgnoreCase)) return true;
        //         if(ext.EndsWith(GLTF_EXT, StringComparison.OrdinalIgnoreCase)) return false;
        //     }
        //     return null;
        // }
    }
}
