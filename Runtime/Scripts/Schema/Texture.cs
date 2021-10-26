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

namespace GLTFast.Schema {

    [System.Serializable]
    public class Texture : RootChild {
        /// <summary>
        /// The index of the sampler used by this texture.
        /// </summary>
        public int sampler = -1;

        /// <summary>
        /// The index of the image used by this texture.
        /// </summary>
        public int source = -1;

        public TextureExtension extensions;

        public int GetImageIndex() {
            if(extensions!=null) {
                if(extensions.KHR_texture_basisu!=null && extensions.KHR_texture_basisu.source >= 0 ) {
                    return extensions.KHR_texture_basisu.source;
                }
            }
            return source;
        }

        public bool isKtx {
            get {
                return extensions!=null && extensions.KHR_texture_basisu!=null;
            }
        }
        
        public void GltfSerialize(JsonWriter writer) {
            writer.AddObject();
            GltfSerializeRoot(writer);
            if (source >= 0) {
                writer.AddProperty("source", source);
            }
            if (sampler >= 0) {
                writer.AddProperty("sampler", sampler);
            }
            if (extensions!=null) {
                writer.AddProperty("extensions");
                extensions.GltfSerialize(writer);
            }
            writer.Close();
        }
    }
}
