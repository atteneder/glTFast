// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

#if NEWTONSOFT_JSON

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using GLTFast.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine.Scripting;

namespace GLTFast.Newtonsoft.Schema
{
    public class Root : RootBase<
        Accessor,
        Animation,
        Asset,
        Buffer,
        BufferView,
        Camera,
        RootExtensions,
        Image,
        Material,
        Mesh,
        Node,
        Sampler,
        Scene,
        Skin,
        Texture
    >, IJsonObject
    {
        public UnclassifiedData extras;

        [JsonExtensionData]
        IDictionary<string, JToken> m_JsonExtensionData;

        [Preserve]
        public Root() {}

        public bool TryGetValue<T>(string key, out T value)
        {
            if (m_JsonExtensionData != null
                && m_JsonExtensionData.TryGetValue(key, out var token))
            {
                value = token.ToObject<T>();
                return true;
            }

            value = default;
            return false;
        }
    }
}

#endif
