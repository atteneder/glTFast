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

using System;

namespace GLTFast
{
    using Schema;

    static class JsonParser
    {
        static IGltfJsonParser k_DefaultJsonParser;

        internal static Root ParseJson<T>(string json) where T : Root
        {
            if (k_DefaultJsonParser is null)
                SetDefaultImplementation<GltfJsonUtilityParser>();

            if (k_DefaultJsonParser is null)
                throw new InvalidOperationException(
                    $"{nameof(JsonParser)}.{nameof(k_DefaultJsonParser)} is null!");

            return k_DefaultJsonParser.ParseJson<T>(json);
        }

        public static void SetDefaultImplementation<T>() where T : IGltfJsonParser, new()
        {
            k_DefaultJsonParser = new T();
        }
    }
}
