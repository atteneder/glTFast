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
using System.Threading.Tasks;

using UnityEngine;

namespace GLTFast.Loading
{

    /// <summary>
    /// Provides a mechanism for loading external resources from a URI
    /// </summary>
    public interface IDownloadProvider
    {
        /// <summary>
        /// Sends a URI request
        /// </summary>
        /// <param name="url">URI to request</param>
        /// <returns>Object representing the request</returns>
        Task<IDownload> Request(Uri url);

        /// <summary>
        /// Sends a URI request to load a texture
        /// </summary>
        /// <param name="url">URI to request</param>
        /// <param name="nonReadable">If true, resulting texture is not CPU readable (uses less memory)</param>
        /// <returns>Object representing the request</returns>
        Task<ITextureDownload> RequestTexture(Uri url, bool nonReadable);
    }

    /// <summary>
    /// Provides a mechanism to inspect the progress and result of a download
    /// or file access request
    /// </summary>
    public interface IDownload : IDisposable
    {
        /// <summary>
        /// True, if the request was successful
        /// </summary>
        bool Success { get; }

        /// <summary>
        /// Error message in case the request failed. Null otherwise.
        /// </summary>
        string Error { get; }

        /// <summary>
        /// Resulting data
        /// </summary>
        byte[] Data { get; }

        /// <summary>
        /// Resulting data as text
        /// </summary>
        string Text { get; }

        /// <summary>
        /// True if the result is a glTF-binary, false if it is not.
        /// No value if determining the glTF type was not possible or failed.
        /// </summary>
        bool? IsBinary { get; }
    }

    /// <summary>
    /// Provides a mechanism to inspect the progress and result of a texture download
    /// or texture file access request
    /// </summary>
    public interface ITextureDownload : IDownload
    {
        /// <summary>
        /// Resulting texture
        /// </summary>
        Texture2D Texture { get; }
    }
}
