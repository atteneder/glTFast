// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;

namespace GLTFast.Addons
{

    /// <summary>
    /// Central point to register glTFast import add-ons.
    /// All registered import add-ons will be injected into all <see cref="GltfImport"/>
    /// and their <see cref="IInstantiator"/>
    /// </summary>
    public static class ImportAddonRegistry
    {
        static List<ImportAddon> s_Addons;

        /// <summary>
        /// Registers an add-on.
        /// </summary>
        /// <param name="addon">Import add-on to register.</param>
        public static void RegisterImportAddon(ImportAddon addon)
        {
            CertifyDefaultAddonsRegistered();
            s_Addons.Add(addon);
        }

        /// <summary>
        /// Injects all registered import add-ons into a <see cref="GltfImport"/>.
        /// </summary>
        /// <param name="gltfImport">Target <see cref="GltfImport"/></param>
        internal static void InjectAllAddons(GltfImportBase gltfImport)
        {
            CertifyDefaultAddonsRegistered();
            foreach (var importAddon in s_Addons)
            {
                importAddon.CreateImportInstance(gltfImport);
            }
        }

        static void CertifyDefaultAddonsRegistered()
        {
            if (s_Addons == null)
            {
                s_Addons = new List<ImportAddon>();

                // TODO: Register all default import add-ons
                // TODO: Investigate if add-ons can be auto-registered via reflection
            }
        }
    }
}
