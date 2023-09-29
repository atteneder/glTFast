// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

#if NEWTONSOFT_JSON

using GLTFast.Schema;
using Newtonsoft.Json;

namespace GLTFast.Newtonsoft
{
    public class GltfNewtonsoftJsonParser : IGltfJsonParser
    {
        public T ParseJson<T>(string json) where T : RootBase
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
#endif
