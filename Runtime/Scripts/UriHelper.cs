// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace GLTFast
{

    static class UriHelper
    {

        public static Uri GetBaseUri(Uri uri)
        {
            if (uri == null) return null;
            if (!uri.IsAbsoluteUri)
            {
                var uriString = Path.GetDirectoryName(uri.OriginalString) ?? "";
                return new Uri(uriString, UriKind.Relative);
            }
            return new Uri(uri, ".");
        }

        /// <summary>
        /// Get URI that is potentially relative to another URI
        /// </summary>
        /// <param name="uri">Absolute or relative URI</param>
        /// <param name="baseUri">Base URI</param>
        /// <returns>Absolute URI that is potentially relative to baseUri</returns>
        public static Uri GetUriString(string uri, Uri baseUri)
        {
            uri = Uri.UnescapeDataString(uri);
            if (Uri.TryCreate(uri, UriKind.Absolute, out var result))
            {
                return result;
            }

            if (baseUri != null)
            {
                uri = RemoveDotSegments(uri, out var parentLevels);
                if (baseUri.IsAbsoluteUri)
                {
                    for (int i = 0; i < parentLevels; i++)
                    {
                        baseUri = new Uri(baseUri, "..");
                    }
                    return new Uri(baseUri, uri);
                }

                var parentPath = baseUri.OriginalString;
                for (int i = 0; i < parentLevels; i++)
                {
                    parentPath = Path.GetDirectoryName(parentPath);
                    if (string.IsNullOrEmpty(parentPath))
                    {
                        baseUri = new Uri("", UriKind.Relative);
                        break;
                    }
                    baseUri = new Uri(parentPath, UriKind.Relative);
                }
                return new Uri(Path.Combine(baseUri.OriginalString, uri), UriKind.Relative);
            }
            return new Uri(uri, UriKind.RelativeOrAbsolute);
        }

        /// <summary>
        /// Removes relative dot segments "." and ".." and resolves them.
        /// See https://datatracker.ietf.org/doc/html/rfc3986#section-5.2.4
        /// </summary>
        /// <param name="uri">Relative input URI</param>
        /// <param name="parentLevels">Number of levels going beyond this paths hierarchy (due to "..")</param>
        /// <returns>Resolved/compressed input URI without dot segments</returns>
        public static string RemoveDotSegments(string uri, out int parentLevels)
        {
            var segments = new List<string>();
            var start = 0;
            parentLevels = 0;
            while (true)
            {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_WSA
                var i = uri.IndexOfAny(new char[]{Path.DirectorySeparatorChar,Path.AltDirectorySeparatorChar},start);
#else
                var i = uri.IndexOf(Path.DirectorySeparatorChar, start);
#endif
                var found = i >= 0;
                var len = found ? (i - start) : uri.Length - start;
                if (len > 0)
                {
                    var segment = uri.Substring(start, len);

                    if (segment == "..")
                    {
                        if (segments.Count > 0)
                        {
                            segments.RemoveAt(segments.Count - 1);
                        }
                        else
                        {
                            parentLevels++;
                        }
                    }
                    else if (segment != ".")
                    {
                        segments.Add(segment);
                    }
                }
                if (!found)
                {
                    break;
                }
                start = i + 1;
            }

            var sb = new StringBuilder();
            var first = true;
            foreach (var segment in segments)
            {
                if (!first)
                {
                    sb.Append(Path.DirectorySeparatorChar);
                }
                sb.Append(segment);
                first = false;
            }
            return sb.ToString();
        }

        /// <summary>
        /// Detect glTF type from URI
        /// </summary>
        /// <param name="uri">Input URI</param>
        /// <returns>True if glTF-binary, False if glTF (JSON), null if not sure.</returns>
        public static bool? IsGltfBinary(Uri uri)
        {
            string path = uri.IsAbsoluteUri ? uri.LocalPath : uri.OriginalString;
            var index = path.LastIndexOf('.', path.Length - 1, Mathf.Min(5, path.Length));
            if (index < 0) return null;
            if (path.EndsWith(GltfGlobals.GlbExt, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            if (path.EndsWith(GltfGlobals.GltfExt, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            return null;
        }

        /// <summary>
        /// Detect image format from URI string
        /// </summary>
        /// <param name="uri">Input URI string</param>
        /// <returns>ImageFormat if detected correctly, <see cref="ImageFormat.Unknown"/> otherwise</returns>
        internal static ImageFormat GetImageFormatFromUri(string uri)
        {
            if (string.IsNullOrEmpty(uri)) return ImageFormat.Unknown;
            var queryStartIndex = uri.LastIndexOf('?');
            if (queryStartIndex < 0) queryStartIndex = uri.Length;
            var extStartIndex = uri.LastIndexOf('.', queryStartIndex - 1, Mathf.Min(5, queryStartIndex)); // we assume that the first period before the query string is the file format period.
            if (extStartIndex < 0) return ImageFormat.Unknown; // if we can't find a period, we don't know the file format.
            var fileExtension = uri.Substring(extStartIndex + 1, queryStartIndex - extStartIndex - 1); // extract the file ending
            if (fileExtension.Equals("png", StringComparison.OrdinalIgnoreCase)) return ImageFormat.PNG;
            if (fileExtension.Equals("jpg", StringComparison.OrdinalIgnoreCase) || fileExtension.Equals("jpeg", StringComparison.OrdinalIgnoreCase)) return ImageFormat.Jpeg;
            if (fileExtension.Equals("ktx", StringComparison.OrdinalIgnoreCase) || fileExtension.Equals("ktx2", StringComparison.OrdinalIgnoreCase)) return ImageFormat.Ktx;
            return ImageFormat.Unknown;
        }

        // // string-based IsGltfBinary alternative
        // // Profiling result: Faster/less memory, but for .glb/.gltf just barely better (unknown ~2x)
        // // Downside: less convenient
        //  public static bool? IsGltfBinary( string uri ) {
        //      // quick glTF-binary check
        //      if (uri.EndsWith(GltfGlobals.glbExt, StringComparison.OrdinalIgnoreCase)) return true;
        //      if (uri.EndsWith(GltfGlobals.gltfExt, StringComparison.OrdinalIgnoreCase)) return false;
        //
        //      // thorough glTF-binary extension check that strips HTTP GET parameters
        //      int getIndex = uri.LastIndexOf('?');
        //      if (getIndex >= 0) {
        //          var ext = uri.Substring(getIndex - GltfGlobals.gltfExt.Length, GltfGlobals.gltfExt.Length);
        //          if(ext.EndsWith(GltfGlobals.glbExt, StringComparison.OrdinalIgnoreCase)) return true;
        //          if(ext.EndsWith(GltfGlobals.gltfExt, StringComparison.OrdinalIgnoreCase)) return false;
        //      }
        //      return null;
        //  }
    }
}
