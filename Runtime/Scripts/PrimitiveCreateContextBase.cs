// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

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

        public int MeshIndex { get; }
        public int PrimitiveIndex { get; }

        public abstract bool IsCompleted { get; }

        protected PrimitiveCreateContextBase(
            int meshIndex,
            int primitiveIndex,
            int subMeshCount,
            string meshName
            )
        {
            MeshIndex = meshIndex;
            PrimitiveIndex = primitiveIndex;
            m_Materials = new int[subMeshCount];
            m_MeshName = meshName;
        }

        public void SetMaterial(int subMesh, int materialIndex)
        {
            m_Materials[subMesh] = materialIndex;
        }

        public MorphTargetsContext morphTargetsContext;

        public abstract Task<MeshResult?> CreatePrimitive();
    }
}
