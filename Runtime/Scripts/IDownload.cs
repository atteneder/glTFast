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

using System.Collections;
using UnityEngine;

namespace GLTFast.Loading {

    public interface IDownloadProvider {
        IDownload Request(string url);
        ITextureDownload RequestTexture(string url);
    }

    public interface IDownload : IEnumerator {
        bool success {get;}
        string error {get;}
        byte[] data { get; }
        string text { get; }
    }

    public interface ITextureDownload : IDownload {
        Texture2D texture { get; }
    }
}
