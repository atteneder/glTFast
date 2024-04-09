// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

namespace GLTFast.Schema
{

    /// <summary>
    /// Metadata about the glTF asset.
    /// </summary>
    [System.Serializable]
    public class Asset : NamedObject
    {
        /// <summary>
        /// A copyright message suitable for display to credit the content creator.
        /// </summary>
        public string copyright;

        /// <summary>
        /// Tool that generated this glTF model. Useful for debugging.
        /// </summary>
        public string generator;

        /// <summary>
        /// The glTF version.
        /// </summary>
        public string version;

        /// <summary>
        /// The minimum glTF version that this asset targets.
        /// </summary>
        public string minVersion;

        internal void GltfSerialize(JsonWriter writer)
        {
            writer.OpenBrackets();
            if (!string.IsNullOrEmpty(version))
            {
                writer.AddProperty("version", version);
            }
            if (!string.IsNullOrEmpty(generator))
            {
                writer.AddPropertySafe("generator", generator);
            }
            if (!string.IsNullOrEmpty(copyright))
            {
                writer.AddPropertySafe("copyright", copyright);
            }
            if (!string.IsNullOrEmpty(minVersion))
            {
                writer.AddProperty("minVersion", minVersion);
            }
            writer.Close();
        }
    }
}
