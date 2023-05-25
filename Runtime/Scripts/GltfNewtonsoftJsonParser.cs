// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

#if NEWTONSOFT_JSON && (DEBUG || GLTFAST_USE_NEWTONSOFT_JSON)
using GLTFast.Schema;
using Newtonsoft.Json;

namespace GLTFast
{
    public class GltfNewtonsoftJsonParser : IGltfJsonParser
    {
        public Root ParseJson<T>(string json) where T : Root
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
#endif
