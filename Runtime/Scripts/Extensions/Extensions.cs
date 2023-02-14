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

    public abstract class Extension<ImportType> : ExtensionBase
        where ImportType : ImportInstance, new()
    {
        public override void CreateImportInstance(GltfImport gltfImport)
        {
            var instance = new ImportType();
            instance.Inject(gltfImport);
        }
    }

    public abstract class ImportInstance : IDisposable
    {
        public abstract void Inject(GltfImport gltfImport);
        public abstract void Inject(IInstantiator instantiator);
        public abstract void Dispose();
    }

    public static class ExtensionRegistry
    {

        static List<ExtensionBase> extensions;

        public static void RegisterExtension(ExtensionBase extension)
        {
            if (extensions == null)
            {
                extensions = new List<ExtensionBase>();
            }
            extensions.Add(extension);
        }

        public static void InjectAllExtensions(GltfImport gltfImport)
        {
            foreach (var extension in extensions)
            {
                extension.CreateImportInstance(gltfImport);
            }
        }
    }
}
