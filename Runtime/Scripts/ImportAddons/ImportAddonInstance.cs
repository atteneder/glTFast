// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;

namespace GLTFast.Addons
{

    /// <summary>
    /// Base class for customizing load- and instantiation behavior.
    /// Injects itself into <see cref="GltfImport"/> and <see cref="IInstantiator"/>.
    /// </summary>
    public abstract class ImportAddonInstance : IDisposable
    {
        /// <summary>
        /// Obtains whether a certain glTF extension
        /// is supported by this <see cref="ImportAddonInstance"/>.
        /// </summary>
        /// <param name="extensionName">Name of the glTF extension</param>
        /// <seealso href="https://registry.khronos.org/glTF/specs/2.0/glTF-2.0.html#specifying-extensions"/>
        /// <returns>True if this ImportInstance offers support for the given glTF extension. False otherwise.</returns>
        public abstract bool SupportsGltfExtension(string extensionName);

        /// <summary>
        /// Injects this import instance into a <see cref="GltfImport"/>
        /// </summary>
        /// <param name="gltfImport"><see cref="GltfImport"/> to be injected into.</param>
        public abstract void Inject(GltfImportBase gltfImport);

        /// <summary>
        /// Injects this import instance into an instantiator.
        /// </summary>
        /// <param name="instantiator">Instantiator to be injected into.</param>
        public abstract void Inject(IInstantiator instantiator);

        /// <summary>
        /// Releases previously allocated resources.
        /// </summary>
        public abstract void Dispose();
    }
}
