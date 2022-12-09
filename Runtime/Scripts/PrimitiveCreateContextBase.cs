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

using UnityEngine.Rendering;
using System.Threading.Tasks;

namespace GLTFast
{

    abstract class PrimitiveCreateContextBase
    {

        public const MeshUpdateFlags defaultMeshUpdateFlags =
            MeshUpdateFlags.DontNotifyMeshUsers
            | MeshUpdateFlags.DontRecalculateBounds
            | MeshUpdateFlags.DontResetBoneBounds
            | MeshUpdateFlags.DontValidateIndices;

        protected string m_MeshName;
        protected int[] m_Materials;

        public int PrimitiveIndex { get; }

        public abstract bool IsCompleted { get; }

        protected PrimitiveCreateContextBase(int primitiveIndex, int materialCount, string meshName)
        {
            this.PrimitiveIndex = primitiveIndex;
            m_Materials = new int[materialCount];
            m_MeshName = meshName;
        }

        public void SetMaterial(int subMesh, int materialIndex)
        {
            m_Materials[subMesh] = materialIndex;
        }

        public MorphTargetsContext morphTargetsContext;

        public abstract Task<Primitive?> CreatePrimitive();
    }
}
