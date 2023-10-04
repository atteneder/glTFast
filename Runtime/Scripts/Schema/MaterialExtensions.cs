// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

namespace GLTFast.Schema
{

    /// <summary>
    /// Material extensions.
    /// </summary>
    [System.Serializable]
    public class MaterialExtensions
    {

        // Names are identical to glTF specified property names, that's why
        // inconsistent names are ignored.
        // ReSharper disable InconsistentNaming

        /// <inheritdoc cref="PbrSpecularGlossiness"/>
        public PbrSpecularGlossiness KHR_materials_pbrSpecularGlossiness;

        /// <inheritdoc cref="PbrSpecularGlossiness"/>
        public MaterialUnlit KHR_materials_unlit;

        /// <inheritdoc cref="PbrSpecularGlossiness"/>
        public Transmission KHR_materials_transmission;

        /// <inheritdoc cref="PbrSpecularGlossiness"/>
        public ClearCoat KHR_materials_clearcoat;

        /// <inheritdoc cref="PbrSpecularGlossiness"/>
        public Sheen KHR_materials_sheen;

        // ReSharper restore InconsistentNaming

        internal void GltfSerialize(JsonWriter writer)
        {
            writer.AddObject();
            if (KHR_materials_pbrSpecularGlossiness != null)
            {
                writer.AddProperty("KHR_materials_pbrSpecularGlossiness");
                KHR_materials_pbrSpecularGlossiness.GltfSerialize(writer);
            }
            if (KHR_materials_unlit != null)
            {
                writer.AddProperty("KHR_materials_unlit");
                KHR_materials_unlit.GltfSerialize(writer);
            }
            if (KHR_materials_transmission != null)
            {
                writer.AddProperty("KHR_materials_transmission");
                KHR_materials_transmission.GltfSerialize(writer);
            }
            if (KHR_materials_clearcoat != null)
            {
                writer.AddProperty("KHR_materials_clearcoat");
                KHR_materials_clearcoat.GltfSerialize(writer);
            }
            if (KHR_materials_sheen != null)
            {
                writer.AddProperty("KHR_materials_sheen");
                KHR_materials_sheen.GltfSerialize(writer);
            }
            writer.Close();
        }
    }
}
