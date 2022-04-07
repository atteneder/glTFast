﻿// Copyright 2020-2022 Andreas Atteneder
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
    public class Skin : RootChild {
        public int inverseBindMatrices;
        public int skeleton = -1;
        public uint[] joints;
        
        public void GltfSerialize(JsonWriter writer) {
            writer.AddObject();
            if (inverseBindMatrices >= 0)
            {
                writer.AddProperty("inverseBindMatrices", inverseBindMatrices);
            }
            if (skeleton >= 0)
            {
                writer.AddProperty("skeleton", skeleton);
            }

            if (joints != null)
            {
                writer.AddArrayProperty("joints", joints);
            }

            //GltfSerializeRoot(writer);
            writer.Close();
            //throw new System.NotImplementedException($"GltfSerialize missing on {GetType()}");
        }
    }
}
