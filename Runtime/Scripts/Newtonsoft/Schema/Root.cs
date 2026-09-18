// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using UnityEngine.Scripting.APIUpdating;

namespace Unity.Cloud.Gltfast.Newtonsoft.Schema
{
    [Obsolete("Use Unity.Cloud.Gltfast.Objects.Root instead.")]
    [MovedFrom(true, sourceNamespace: "GLTFast.Newtonsoft.Schema", sourceAssembly: "glTFast.Newtonsoft")]
    public class Root : Unity.Cloud.Gltfast.Objects.Root, IJsonObject
    {
        /// <inheritdoc/>
        public bool TryGetValue<T>(string key, out T value)
        {
            return AdditionalProperties.TryGetValue(key, out value);
        }
    }
}
