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

using UnityEngine;

namespace GLTFast.Schema {

    [System.Serializable]
    public class PbrSpecularGlossiness {
        public float[] diffuseFactor = { 1, 1, 1, 1 };

        public Color diffuseColor {
            get {
                return new Color(
                    diffuseFactor[0],
                    diffuseFactor[1],
                    diffuseFactor[2],
                    diffuseFactor[3]
                );
            }
        }


        public TextureInfo diffuseTexture = null;

        public float[] specularFactor = { 1, 1, 1 };

        public Color specularColor {
            get {
                return new Color(
                    specularFactor[0],
                    specularFactor[1],
                    specularFactor[2]
                );
            }
        }

        public float glossinessFactor = 1;

        public TextureInfo specularGlossinessTexture = null;
        
        public void GltfSerialize(JsonWriter writer) {
            writer.AddObject();
            writer.Close();
            throw new System.NotImplementedException($"GltfSerialize missing on {GetType()}");
        }
    }
}


