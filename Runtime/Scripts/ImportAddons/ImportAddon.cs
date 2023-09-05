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
        public abstract void CreateImportInstance(GltfImport gltfImport);
    }

    /// <summary>
    /// Extension base class.
    /// </summary>
    /// <typeparam name="TImport"><see cref="ImportAddonInstance"/> based type, that is constructed per <see cref="GltfImport"/></typeparam>
    public abstract class ImportAddon<TImport> : ImportAddon
        where TImport : ImportAddonInstance, new()
    {
        /// <inheritdoc />
        public override void CreateImportInstance(GltfImport gltfImport)
        {
            var instance = new TImport();
            instance.Inject(gltfImport);
        }
    }
}
