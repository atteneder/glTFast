// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

namespace GLTFast.Extensions
{

    /// <summary>
    /// Extension base class.
    /// </summary>
    /// <typeparam name="TImport"><see cref="ImportInstance"/> based type, that is constructed per <see cref="GltfImport"/></typeparam>
    public abstract class Extension<TImport> : ExtensionBase
        where TImport : ImportInstance, new()
    {
        /// <inheritdoc />
        public override void CreateImportInstance(GltfImport gltfImport)
        {
            var instance = new TImport();
            instance.Inject(gltfImport);
        }
    }
}
