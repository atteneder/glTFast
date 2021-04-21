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
using System.IO;
using UnityEngine;

namespace GLTFast {

    public static class UriHelper
    {
        const string GLB_EXT = ".glb";
        const string GLTF_EXT = ".gltf";

        public static Uri GetBaseUri( Uri uri ) {
            if(uri==null) return null;
            if (!uri.IsAbsoluteUri) {
                return new Uri(Path.GetDirectoryName(uri.OriginalString), UriKind.Relative);
            }
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

            if (baseUri != null) {
                return baseUri.IsAbsoluteUri
                    ? new Uri(baseUri,uri)
                    : new Uri(Path.Combine(baseUri.OriginalString, uri), UriKind.Relative);
            }
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

        /// <summary>
        /// Detect image format from URI string
        /// </summary>
        /// <param name="uri">Input URI string</param>
        /// <returns>ImageFormat if detected correctly, ImageFormat.Unkonwn otherwise</returns>
        internal static ImageFormat GetImageFormatFromUri(string uri) {
            if (string.IsNullOrEmpty(uri)) return ImageFormat.Unknown;
            var queryStartIndex = uri.LastIndexOf('?');
            if (queryStartIndex < 0) queryStartIndex = uri.Length;
            var extStartIndex = uri.LastIndexOf('.', queryStartIndex-1,Mathf.Min(5,queryStartIndex)); // we assume that the first period before the query string is the file format period.
            if (extStartIndex < 0) return ImageFormat.Unknown; // if we can't find a period, we don't know the file format.
            var fileExtension = uri.Substring(extStartIndex+1, queryStartIndex - extStartIndex - 1); // extract the file ending
            if (fileExtension.Equals("png", StringComparison.OrdinalIgnoreCase)) return ImageFormat.PNG;
            if (fileExtension.Equals("jpg", StringComparison.OrdinalIgnoreCase) || fileExtension.Equals("jpeg", StringComparison.OrdinalIgnoreCase)) return ImageFormat.Jpeg;
            if (fileExtension.Equals("ktx", StringComparison.OrdinalIgnoreCase) || fileExtension.Equals("ktx2", StringComparison.OrdinalIgnoreCase)) return ImageFormat.KTX;
            return ImageFormat.Unknown;
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
