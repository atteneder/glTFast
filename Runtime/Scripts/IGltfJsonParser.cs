// Copyright 2020-2023 Andreas Atteneder
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

using GLTFast.Schema;

namespace GLTFast
{
    /// <summary>
    /// Provides a mechanism for deserializing glTF JSON.
    /// </summary>
    public interface IGltfJsonParser
    {
        /// <summary>
        /// Deserializes glTF JSON.
        /// </summary>
        /// <param name="json">Source glTF JSON.</param>
        /// <typeparam name="T">Custom schema type that the glTF JSON is deserialization to.</typeparam>
        /// <returns>Deserialized glTF schema object of Type <see cref="T"/></returns>
        Root ParseJson<T>(string json) where T : Root;
    }
}
