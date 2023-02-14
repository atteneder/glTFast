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

using System;

namespace GLTFast
{
    using Schema;

    static class JsonParser
    {
        static IJsonImplementation k_DefaultJsonImplementation;

        internal static Root ParseJson<T>(string json) where T : Root
        {
            if (k_DefaultJsonImplementation is null)
                SetDefaultImplementation<JsonUtilityImplementation>();

            if (k_DefaultJsonImplementation is null)
                throw new InvalidOperationException(
                    $"{nameof(JsonParser)}.{nameof(k_DefaultJsonImplementation)} is null!");

            return k_DefaultJsonImplementation.ParseJson<T>(json);
        }

        public static void SetDefaultImplementation<T>() where T : IJsonImplementation, new()
        {
            k_DefaultJsonImplementation = new T();
        }
    }
}
