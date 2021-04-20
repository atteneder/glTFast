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

using UnityEngine;

namespace GLTFast {
    
    public interface IGltfReadable {
        
        int materialCount { get; }
        int imageCount { get; }
        int textureCount { get; }

        Material GetMaterial(int index = 0);
        Material GetDefaultMaterial();
        
        Texture2D GetImage(int index = 0);
        Texture2D GetTexture(int index = 0);

        Schema.Material GetSourceMaterial(int index = 0);
        Schema.Texture GetSourceTexture(int index = 0);
        Schema.Image GetSourceImage(int index = 0);
    }
}
