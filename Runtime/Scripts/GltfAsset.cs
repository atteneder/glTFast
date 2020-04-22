// Copyright 2020 Andreas Atteneder
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
using UnityEngine.Events;
using UnityEngine.Networking;

namespace GLTFast
{
    public class GltfAsset : MonoBehaviour
    {
        [Serializable]
        public class HttpHeaders
        {
            public string Key;
            public string Value;
        }
        [Header("Http headers")]
        [SerializeField]
        private HttpHeaders[] _headers;

        [SerializeField]
        private bool _forceBinary = false;
        public string url;
        public bool loadOnStartup = true;

        protected GLTFast gLTFastInstance;

        public UnityAction<GltfAsset, bool> onLoadComplete;


        /// <summary>
        /// Method for manual loading with custom <see cref="IDeferAgent"/>.
        /// </summary>
        /// <param name="url">URL of the glTF file.</param>
        /// <param name="deferAgent">Defer Agent takes care of interrupting the
        /// loading procedure in order to keep the frame rate responsive.</param>
        public void Load(string url, IDeferAgent deferAgent = null)
        {
            this.url = url;
            Load(deferAgent);
        }

        /// <summary>
        /// Allow to set http header with scripts. Call this method before loading the asset
        /// </summary>
        /// <param name="key">Http header key</param>
        /// <param name="value">Http header value</param>
        public void AddHttpHeader(string key, string value)
        {
            var initialSize = _headers == null ? 0 : _headers.Length;
            var httpHeaders = new HttpHeaders[initialSize +1];
            if(_headers != null)
            {
                Array.Copy(_headers, httpHeaders, initialSize);
            }
            httpHeaders[initialSize] = new HttpHeaders() {
                Key = key,
                Value = value
            };
            _headers = httpHeaders;
        }

        /// <summary>
        /// If you need to force loading binary in script
        /// </summary>
        /// <param name="force"></param>
        public void SetForceBinary(bool force)
        {
            _forceBinary = force;   
        }

        void Start()
        {
            if (loadOnStartup && !string.IsNullOrEmpty(url))
            {
                // Automatic load on startup
                Load();
            }
        }

        void Load(IDeferAgent deferAgent = null)
        {
            gLTFastInstance = new GLTFast(this);
            gLTFastInstance.onSettingRequest += OnSettingHeaders;
            gLTFastInstance.onLoadComplete += OnLoadComplete;
            gLTFastInstance.Load(url, deferAgent, _forceBinary);
        }

        protected virtual void OnLoadComplete(bool success)
        {
            gLTFastInstance.onLoadComplete -= OnLoadComplete;
            if (success)
            {
                gLTFastInstance.InstantiateGltf(transform);
            }
            if (onLoadComplete != null)
            {
                onLoadComplete(this, success);
            }
        }

        protected virtual void OnSettingHeaders(UnityWebRequest request)
        {
            foreach(var header in _headers)
            {
                request.SetRequestHeader(header.Key, header.Value);
            }
        }

        private void OnDestroy()
        {
            if (gLTFastInstance != null)
            {
                gLTFastInstance.Destroy();
                gLTFastInstance = null;
            }
        }
    }
}
