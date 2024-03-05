// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

namespace GLTFast.Schema
{

    /// <summary>
    /// Base class for anything with a name property
    /// </summary>
    [System.Serializable]
    public abstract class NamedObject
    {

        /// <summary>
        /// Object's name
        /// </summary>
        public string name;

        internal void GltfSerializeName(JsonWriter writer)
        {
            if (!string.IsNullOrEmpty(name))
            {
                writer.AddPropertySafe("name", name);
            }
        }
    }
}
