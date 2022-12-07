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

using Unity.Mathematics;

namespace GLTFast.Schema
{

    /// <summary>
    /// Occlusion map specific texture info
    /// </summary>
    [System.Serializable]
    public class OcclusionTextureInfo : TextureInfo
    {

        /// <summary>
        /// A scalar multiplier controlling the amount of occlusion applied.
        /// A value of 0.0 means no occlusion.
        /// A value of 1.0 means full occlusion.
        /// This value is ignored if the corresponding texture is not specified.
        /// This value is linear.
        /// <minimum>0.0</minimum>
        /// <maximum>1.0</maximum>
        /// </summary>
        public float strength = 1.0f;

        internal override void GltfSerialize(JsonWriter writer)
        {
            writer.AddObject();
            GltfSerializeTextureInfo(writer);
            if (math.abs(strength - 1f) > Constants.epsilon)
            {
                writer.AddProperty("strength", strength);
            }
            writer.Close();
        }
    }
}
