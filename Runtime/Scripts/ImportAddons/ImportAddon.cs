// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using UnityEngine.Scripting.APIUpdating;

namespace Unity.Cloud.Gltfast.Addons
{

    /// <summary>
    /// Import add-on base class.
    /// </summary>
    [MovedFrom(true, sourceNamespace: "GLTFast.Addons", sourceAssembly: "glTFast")]
    public abstract class ImportAddon
    {
        /// <summary>
        /// Creates an import instance that is assigned to a <see cref="GltfImport"/>
        /// </summary>
        /// <param name="gltfImport">GltfImport the import instance is assigned to.</param>
        public abstract void CreateImportInstance(GltfImport gltfImport);
    }

    /// <summary>
    /// Import add-on base class that creates a typed import add-on instance.
    /// </summary>
    /// <typeparam name="TInstance">Type of the add-on instance that is constructed per <see cref="GltfImport"/>.</typeparam>
    [MovedFrom(true, sourceNamespace: "GLTFast.Addons", sourceAssembly: "glTFast")]
    public abstract class ImportAddon<TInstance> : ImportAddon
        where TInstance : ImportAddonInstance, new()
    {
        /// <inheritdoc />
        public override void CreateImportInstance(GltfImport gltfImport)
        {
            var instance = new TInstance();
            instance.Inject(gltfImport);
        }
    }
}
