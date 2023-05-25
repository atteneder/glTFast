// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using GLTFast.Schema;

namespace GLTFast
{
    /// <summary>
    /// Provides a mechanism for deserializing glTF JSON.
    /// </summary>
    public interface IGltfJsonParser
    {
        /// <summary>
        /// Deserializes glTF JSON.
        /// </summary>
        /// <param name="json">Source glTF JSON.</param>
        /// <typeparam name="T">Custom schema type that the glTF JSON is deserialization to.</typeparam>
        /// <returns>Deserialized glTF schema object of Type <see cref="T"/></returns>
        Root ParseJson<T>(string json) where T : Root;
    }
}
