// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;

namespace GLTFast.Extensions
{

    /// <summary>
    /// Central point to register glTFast extensions.
    /// All registered extension will be injected into all <see cref="GltfImport"/>
    /// and their <see cref="IInstantiator"/> 
    /// </summary>
    public static class ExtensionRegistry
    {
        static List<ExtensionBase> s_Extensions;

        /// <summary>
        /// Registers an extension.
        /// </summary>
        /// <param name="extension">Extension to register.</param>
        public static void RegisterExtension(ExtensionBase extension)
        {
            CertifyDefaultExtensionsRegistered();
            s_Extensions.Add(extension);
        }

        /// <summary>
        /// Injects all registered extensions into a <see cref="GltfImport"/>.
        /// </summary>
        /// <param name="gltfImport">Target <see cref="GltfImport"/></param>
        internal static void InjectAllExtensions(GltfImport gltfImport)
        {
            CertifyDefaultExtensionsRegistered();
            foreach (var extension in s_Extensions)
            {
                extension.CreateImportInstance(gltfImport);
            }
        }

        static void CertifyDefaultExtensionsRegistered()
        {
            if (s_Extensions == null)
            {
                s_Extensions = new List<ExtensionBase>();

                // TODO: Register all default extensions
                // TODO: Investigate if extensions can be auto-registered via reflection
            }
        }
    }
}
