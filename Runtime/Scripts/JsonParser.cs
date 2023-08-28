// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

#if NEWTONSOFT_JSON && GLTFAST_USE_NEWTONSOFT_JSON
#define JSON_NEWTONSOFT
#else
#define JSON_UTILITY
#endif

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
#if JSON_NEWTONSOFT
                SetDefaultParser<GltfNewtonsoftJsonParser>();
#else
                SetDefaultParser<GltfJsonUtilityParser>();
#endif

            if (k_DefaultJsonParser is null)
                throw new InvalidOperationException(
                    $"{nameof(JsonParser)}.{nameof(k_DefaultJsonParser)} is null!");

            return k_DefaultJsonParser.ParseJson<T>(json);
        }

        public static void SetDefaultParser<T>() where T : IGltfJsonParser, new()
        {
            k_DefaultJsonParser = new T();
        }
    }
}
