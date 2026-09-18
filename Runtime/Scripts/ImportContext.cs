// SPDX-FileCopyrightText: 2026 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using Unity.Cloud.Gltfast.Loading;
using Unity.Cloud.Gltfast.Logging;
using UnityEngine;

namespace Unity.Cloud.Gltfast
{
    /// <summary>
    /// Contains cross-cutting settings and services of a glTF import instance.
    /// </summary>
    readonly struct ImportContext
    {
        public ImportContext(IDownloadProvider downloadProvider, ICodeLogger logger, IDeferAgent deferAgent)
        {
            DownloadProvider = downloadProvider;
            Logger = logger;
            DeferAgent = deferAgent;
        }

        /// <summary>DownloadProvider used by this glTF import instance.</summary>
        public IDownloadProvider DownloadProvider { get; }

        /// <summary>Logger used by this glTF import instance.</summary>
        public ICodeLogger Logger { get; }

        /// <summary>Defer agent used by this glTF import instance.</summary>
        public IDeferAgent DeferAgent { get; }
    }
}
