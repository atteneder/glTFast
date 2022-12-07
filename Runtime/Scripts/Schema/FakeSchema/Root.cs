// Copyright 2020-2022 Andreas Atteneder
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

using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("glTFast")]

namespace GLTFast.FakeSchema
{

    [System.Serializable]
    class Root
    {
        /// <summary>
        /// An array of materials. A material defines the appearance of a primitive.
        /// </summary>
        public Material[] materials;

#if GLTFAST_SAFE
        public Accessor[] accessors;
#endif

#if DRACO_UNITY
        public Mesh[] meshes;
#endif
    }
}
