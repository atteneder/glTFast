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

namespace GLTFast.Schema {

    [System.Serializable]
    public class MaterialExtension {
        public PbrSpecularGlossiness KHR_materials_pbrSpecularGlossiness;
        public MaterialUnlit KHR_materials_unlit;
        public Transmission KHR_materials_transmission;
        public ClearCoat KHR_materials_clearcoat;
        public Sheen KHR_materials_sheen;
        
        internal void GltfSerialize(JsonWriter writer) {
            writer.AddObject();
            if(KHR_materials_pbrSpecularGlossiness!=null) {
                writer.AddProperty("KHR_materials_pbrSpecularGlossiness");
                KHR_materials_pbrSpecularGlossiness.GltfSerialize(writer);
            }
            if(KHR_materials_unlit!=null) {
                writer.AddProperty("KHR_materials_unlit");
                KHR_materials_unlit.GltfSerialize(writer);
            }
            if(KHR_materials_transmission!=null) {
                writer.AddProperty("KHR_materials_transmission");
                KHR_materials_transmission.GltfSerialize(writer);
            }
            if(KHR_materials_clearcoat!=null) {
                writer.AddProperty("KHR_materials_clearcoat");
                KHR_materials_clearcoat.GltfSerialize(writer);
            }
            if(KHR_materials_sheen!=null) {
                writer.AddProperty("KHR_materials_sheen");
                KHR_materials_sheen.GltfSerialize(writer);
            }
            writer.Close();
        }
    }
}
