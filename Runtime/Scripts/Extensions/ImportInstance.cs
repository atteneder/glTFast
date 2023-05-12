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

namespace GLTFast.Extensions
{

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
}
