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

    public abstract class ExtensionBase
    {
        public abstract void CreateImportInstance(GltfImport gltfImport);
    }

    public abstract class Extension<TImport> : ExtensionBase
        where TImport : ImportInstance, new()
    {
        public override void CreateImportInstance(GltfImport gltfImport)
        {
            var instance = new TImport();
            instance.Inject(gltfImport);
        }
    }

    public abstract class ImportInstance : IDisposable
    {
        public abstract bool SupportsExtension(string extensionName);
        public abstract void Inject(GltfImport gltfImport);
        public abstract void Inject(IInstantiator instantiator);
        public abstract void Dispose();
    }

    public static class ExtensionRegistry
    {
        static List<ExtensionBase> s_Extensions;

        public static void RegisterExtension(ExtensionBase extension)
        {
            CertifyDefaultExtensionsRegistered();
            s_Extensions.Add(extension);
        }

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
