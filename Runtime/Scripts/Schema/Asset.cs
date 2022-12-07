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

namespace GLTFast.Schema
{

    /// <summary>
    /// Metadata about the glTF asset.
    /// </summary>
    [System.Serializable]
    public class Asset
    {
        /// <summary>
        /// A copyright message suitable for display to credit the content creator.
        /// </summary>
        public string copyright;

        /// <summary>
        /// Tool that generated this glTF model. Useful for debugging.
        /// </summary>
        public string generator;

        /// <summary>
        /// The glTF version.
        /// </summary>
        public string version;

        /// <summary>
        /// The minimum glTF version that this asset targets.
        /// </summary>
        public string minVersion;

        internal void GltfSerialize(JsonWriter writer)
        {
            writer.OpenBrackets();
            if (!string.IsNullOrEmpty(version))
            {
                writer.AddProperty("version", version);
            }
            if (!string.IsNullOrEmpty(generator))
            {
                writer.AddProperty("generator", generator);
            }
            if (!string.IsNullOrEmpty(copyright))
            {
                writer.AddProperty("copyright", copyright);
            }
            if (!string.IsNullOrEmpty(minVersion))
            {
                writer.AddProperty("minVersion", minVersion);
            }
            writer.Close();
        }
    }
}
