// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

#if UNITY_ANIMATION || GLTFAST_ANIMATION

using System;
using UnityEngine.Scripting.APIUpdating;

namespace Unity.Cloud.Gltfast.Newtonsoft.Schema
{
    [Obsolete("Use Unity.Cloud.Gltfast.Objects.AnimationChannelTarget instead.")]
    [MovedFrom(true, sourceNamespace: "GLTFast.Newtonsoft.Schema", sourceAssembly: "glTFast.Newtonsoft")]
    public class AnimationChannelTarget : Unity.Cloud.Gltfast.Objects.AnimationChannelTarget, IJsonObject
    {
        /// <inheritdoc/>
        public bool TryGetValue<T>(string key, out T value)
        {
            return AdditionalProperties.TryGetValue(key, out value);
        }
    }
}

#endif
