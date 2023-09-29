// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

namespace GLTFast.Addons
{

    /// <summary>
    /// Import add-on base class.
    /// </summary>
    public abstract class ImportAddon
    {
        /// <summary>
        /// Creates an import instance that is assigned to a <see cref="GltfImport"/>
        /// </summary>
        /// <param name="gltfImport">GltfImport the import instance is assigned to.</param>
        public abstract void CreateImportInstance(GltfImportBase gltfImport);
    }

    /// <summary>
    /// Extension base class.
    /// </summary>
    /// <typeparam name="TInstance">Type of the addon instance, that that is constructed per <see cref="GltfImport"/>.</typeparam>
    public abstract class ImportAddon<TInstance> : ImportAddon
        where TInstance : ImportAddonInstance, new()
    {
        /// <inheritdoc />
        public override void CreateImportInstance(GltfImportBase gltfImport)
        {
            var instance = new TInstance();
            instance.Inject(gltfImport);
        }
    }
}
