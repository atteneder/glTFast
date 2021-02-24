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
using UnityEngine.Networking;
using System.IO;

namespace GLTFast.Loading {
    public class LocalFileProvider : IDownloadProvider {
        public async Task<IDownload> Request(Uri url) {
            var req = new FileLoad(url);
            while (req.MoveNext())
                await req.WaitForChunkFinished();

            req.Close();
            return req;
        }

        public async Task<ITextureDownload> RequestTexture(Uri url,bool nonReadable) {
            var req = new AwaitableTextureLoad(url,nonReadable);
            while (req.MoveNext()) {
                await Task.Yield();
            }
            return req;
        }
    }

    public class FileLoad : IDownload {

        protected const Int32 bufferSize = 32 * 4096;
        protected FileStream fileStream;

        protected string path;
        protected int length;
        protected int sumLoaded;
        protected byte[] bytes;
        protected string readError = null;
        protected Task<int> readChunkTask;

        public FileLoad() {}

        public FileLoad(Uri url) {
            if (url.Scheme != "file") {
                throw new ArgumentException("FileLoad can only load uris starting with file:");
            }
            path = url.LocalPath;
            if (!File.Exists(path)) {
                throw new ArgumentException("File " + url.LocalPath + " does not exist!");
            }
            Init();
        }

        public void Close() {
            Debug.Log("[FileLoad] Closing " + fileStream);
            fileStream.Close();
        }

        protected void Init() {
            Debug.Log("[FileLoad] Opening " + path);
            fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, useAsync: true);
            length = (int)fileStream.Length;
            sumLoaded = 0;
            bytes = new byte[length];
        }

        public object Current { get { return readChunkTask; } }
        public bool MoveNext() { 
            if (fileStream == null) {
                Debug.LogWarning("[FileLoad] filestream is null, this shouldn't happend!");
                return false;
            } 

            if (success)
                return false;

            try {
                var readSize = Math.Min(length - sumLoaded, bufferSize);
                readChunkTask = fileStream.ReadAsync(bytes, sumLoaded, readSize);
                return true; 
            } catch (Exception e) {
                readError = e.Message;
                throw e;
            }
        }

        public async Task WaitForChunkFinished() {
            var count = await readChunkTask;
            sumLoaded += count;
            Debug.Log("[FileLoad] Read " + sumLoaded + "/" + length + " bytes");
            return;
        }

        public void Reset() {}
        public bool success => sumLoaded >= length;
        public string error => readError;
        public byte[] data => bytes;
        public string text { get { return System.Text.Encoding.UTF8.GetString(bytes); } }
        public bool? isBinary
        {
            get 
            {
                if (success) {
                    return path.EndsWith(".glb");
                } else {
                    return null;
                }
            }
        }
    }

    public class AwaitableTextureLoad : AwaitableDownload, ITextureDownload {

        public AwaitableTextureLoad():base() {}
        public AwaitableTextureLoad(Uri url):base(url) {}

        public AwaitableTextureLoad(Uri url, bool nonReadable) {
            Init(url,nonReadable);
        }

        protected static UnityWebRequest CreateRequest(Uri url, bool nonReadable) {
            return UnityWebRequestTexture.GetTexture(url,nonReadable);
        }

        protected void Init(Uri url, bool nonReadable) {
            request = CreateRequest(url,nonReadable);
            asynOperation = request.SendWebRequest();
        }

        public Texture2D texture {
            get {
                return (request.downloadHandler as  DownloadHandlerTexture ).texture;
            }
        }
    }
}
