// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Scripting.APIUpdating;

namespace Unity.Cloud.Gltfast.Loading
{

    /// <summary>
    /// Default <see cref="IDownloadProvider"/> implementation
    /// </summary>
    [MovedFrom(true, sourceNamespace: "GLTFast.Loading", sourceAssembly: "glTFast")]
    public class DefaultDownloadProvider : IDownloadProvider
    {

        [Obsolete("Request has been renamed to RequestAsync. (UnityUpgradable) -> RequestAsync(*)", true)]
        public Task<IDownload> Request(Uri url) => RequestAsync(url);

        /// <summary>
        /// Sends a URI request and waits for its completion.
        /// </summary>
        /// <param name="url">URI to request</param>
        /// <returns>Object representing the request</returns>
        public async Task<IDownload> RequestAsync(Uri url)
        {
            var req = new AwaitableDownload(url);
            await req.WaitAsync();
            return req;
        }

        [Obsolete("RequestTexture has been renamed to RequestTextureAsync. (UnityUpgradable) -> RequestTextureAsync(*)", true)]
        public Task<ITextureDownload> RequestTexture(Uri url, bool nonReadable) => RequestTextureAsync(url, nonReadable);

        /// <summary>
        /// Sends a URI request to load a texture
        /// </summary>
        /// <param name="url">URI to request</param>
        /// <param name="nonReadable">If true, resulting texture is not CPU readable (uses less memory)</param>
        /// <returns>Object representing the request</returns>
#pragma warning disable CS1998
        public async Task<ITextureDownload> RequestTextureAsync(Uri url, bool nonReadable)
        {
#pragma warning restore CS1998
#if UNITY_WEBREQUEST_TEXTURE
            var req = new AwaitableTextureDownload(url, nonReadable);
            await req.WaitAsync();
            return req;
#else
            return null;
#endif
        }
    }

    /// <summary>
    /// Default <see cref="IDownload"/> implementation that loads URIs via <see cref="UnityWebRequest"/>
    /// </summary>
    [MovedFrom(true, sourceNamespace: "GLTFast.Loading", sourceAssembly: "glTFast")]
    public class AwaitableDownload : IDownload
    {
        /// <summary>
        /// <see cref="UnityWebRequest"/> that is used for the download
        /// </summary>
        protected UnityWebRequest m_Request;

        /// <summary>
        /// The download's <see cref="UnityWebRequestAsyncOperation"/>
        /// </summary>
        protected UnityWebRequestAsyncOperation m_AsyncOperation;

        /// <summary>
        /// Empty constructor
        /// </summary>
        protected AwaitableDownload() { }

        /// <summary>
        /// Creates a download of a URI
        /// </summary>
        /// <param name="url">URI to request</param>
        public AwaitableDownload(Uri url)
        {
            Init(url);
        }

        void Init(Uri url)
        {
            m_Request = UnityWebRequest.Get(url);
            m_AsyncOperation = m_Request.SendWebRequest();
        }

        /// <summary>
        /// Waits until the URI request is completed.
        /// </summary>
        /// <returns>A task that represents the completion of the download</returns>
        public async Task WaitAsync()
        {
            while (!m_AsyncOperation.isDone)
            {
                await Task.Yield();
            }
        }

        /// <summary>
        /// True if the download finished and was successful
        /// </summary>
        public bool Success => m_Request != null && m_Request.isDone && m_Request.result == UnityWebRequest.Result.Success;

        /// <summary>
        /// If the download failed, error description
        /// </summary>
        public string Error => m_Request == null ? "Request disposed" : m_Request.error;

        /// <inheritdoc />
        public NativeArray<byte>.ReadOnly Data => m_Request?.downloadHandler.nativeData ?? default;

        /// <summary>
        /// Releases previously allocated resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases previously allocated resources.
        /// </summary>
        /// <param name="disposing">Indicates whether the method call comes from a Dispose method (its value is true)
        /// or from a finalizer (its value is false).</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_Request?.Dispose();
                m_Request = null;
            }
        }
    }

#if UNITY_WEBREQUEST_TEXTURE
    /// <summary>
    /// Default <see cref="ITextureDownload"/> implementation that loads
    /// texture URIs via <seealso cref="UnityWebRequest"/>.
    /// </summary>
    [MovedFrom(true, sourceNamespace: "GLTFast.Loading", sourceAssembly: "glTFast")]
    public class AwaitableTextureDownload : AwaitableDownload, ITextureDownload
    {

        /// <summary>
        /// Parameter-less constructor, required for inheritance.
        /// </summary>
        protected AwaitableTextureDownload() { }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="url">Texture URI to request</param>
        /// <param name="nonReadable">If true, resulting texture is not CPU readable (uses less memory)</param>
        public AwaitableTextureDownload(Uri url, bool nonReadable)
        {
            Init(url, nonReadable);
        }

        /// <summary>
        /// Generates the UnityWebRequest used for sending the request.
        /// </summary>
        /// <param name="url">Texture URI to request</param>
        /// <param name="nonReadable">If true, resulting texture is not CPU readable (uses less memory)</param>
        /// <returns>UnityWebRequest used for sending the request</returns>
        protected static UnityWebRequest CreateRequest(Uri url, bool nonReadable)
        {
            return UnityWebRequestTexture.GetTexture(url, nonReadable);
        }

        void Init(Uri url, bool nonReadable)
        {
            m_Request = CreateRequest(url, nonReadable);
            m_AsyncOperation = m_Request.SendWebRequest();
        }

        /// <inheritdoc />
        public Texture2D Texture => (m_Request?.downloadHandler as DownloadHandlerTexture)?.texture;
    }
#endif
}
