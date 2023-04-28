// Copyright 2020-2023 Andreas Atteneder
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

using System;
using System.Collections.Generic;
using UnityEngine;

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

    /// <summary>
    /// Base class for customizing load- and instantiation behavior.
    /// Injects itself into <see cref="GltfImport"/> and <see cref="IInstantiator"/>.
    /// </summary>
    public abstract class ImportInstance : IDisposable
    {
        /// <summary>
        /// Obtains whether a certain glTF extension
        /// is supported by this <see cref="ImportInstance"/>.
        /// </summary>
        /// <param name="extensionName">Name of the glTF extension</param>
        /// <seealso href="https://registry.khronos.org/glTF/specs/2.0/glTF-2.0.html#specifying-extensions"/>
        /// <returns>True if this ImportInstance offers support for the given glTF extension. False otherwise.</returns>
        public abstract bool SupportsExtension(string extensionName);
        
        /// <summary>
        /// Injects this import instance into a <see cref="GltfImport"/>
        /// </summary>
        /// <param name="gltfImport"><see cref="GltfImport"/> to be injected into.</param>
        public abstract void Inject(GltfImport gltfImport);
        
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
