// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

namespace GLTFast.Extensions
{

    /// <summary>
    /// Extension base class.
    /// </summary>
    public abstract class ExtensionBase
    {
        /// <summary>
        /// Creates an import instance that is assigned to a <see cref="GltfImport"/>
        /// </summary>
        /// <param name="gltfImport">GltfImport the import instance is assigned to.</param>
        public abstract void CreateImportInstance(GltfImport gltfImport);
    }
}
