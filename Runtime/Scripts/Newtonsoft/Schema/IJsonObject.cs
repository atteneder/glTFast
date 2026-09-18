// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using UnityEngine.Scripting.APIUpdating;

namespace Unity.Cloud.Gltfast.Newtonsoft.Schema
{
    /// <summary>
    /// Represents a JSON object, containing key-value properties of arbitrary type.
    /// </summary>
    [Obsolete("Use the AdditionalProperties property (on glTF JSON objects) or IPropertyContainer.TryGetValue (on extensions/extras) instead.")]
    [MovedFrom(true, sourceNamespace: "GLTFast.Newtonsoft.Schema", sourceAssembly: "glTFast.Newtonsoft")]
    public interface IJsonObject
    {
        /// <summary>
        /// Tries to find a property of a <paramref name="key"/> and cast its <paramref name="value"/> to type <c>T</c>.
        /// </summary>
        /// <param name="key">Property key</param>
        /// <param name="value">Resulting value</param>
        /// <typeparam name="T">Desired target type</typeparam>
        /// <returns>True if the property was found and successfully cast to type T. False otherwise.</returns>
        bool TryGetValue<T>(string key, out T value);
    }
}
