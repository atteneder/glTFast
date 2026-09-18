// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using UnityEngine.Scripting.APIUpdating;

namespace Unity.Cloud.Gltfast.Newtonsoft.Schema
{
    [Obsolete("Use Unity.Cloud.Gltfast.Objects.NormalTextureInfo instead.")]
    [MovedFrom(true, sourceNamespace: "GLTFast.Newtonsoft.Schema", sourceAssembly: "glTFast.Newtonsoft")]
    public class NormalTextureInfo : Unity.Cloud.Gltfast.Objects.NormalTextureInfo, IJsonObject
    {
        /// <inheritdoc/>
        public bool TryGetValue<T>(string key, out T value)
        {
            return AdditionalProperties.TryGetValue(key, out value);
        }
    }
}
