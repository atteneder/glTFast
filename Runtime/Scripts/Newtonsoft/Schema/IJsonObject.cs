// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

#if NEWTONSOFT_JSON

using System;

namespace GLTFast.Newtonsoft.Schema
{
    /// <summary>
    /// Represents a JSON object, containing key-value properties of arbitrary type.
    /// </summary>
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

#endif
