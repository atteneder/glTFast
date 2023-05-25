// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

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
                SetDefaultParser<GltfJsonUtilityParser>();

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
