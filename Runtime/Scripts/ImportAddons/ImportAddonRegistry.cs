// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Unity.Cloud.Gltfast.Addons
{

    /// <summary>
    /// Central point to register glTFast import add-ons.
    /// All registered import add-ons will be injected into all <see cref="GltfImport"/>
    /// and their <see cref="IInstantiator"/>
    /// </summary>
    [MovedFrom(true, sourceNamespace: "GLTFast.Addons", sourceAssembly: "glTFast")]
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
        internal static void InjectAllAddons(GltfImport gltfImport)
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

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStaticsOnLoad()
        {
            // Reset static state
            s_Addons = null;
        }
#endif
    }
}
