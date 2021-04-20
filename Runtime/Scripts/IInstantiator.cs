// Copyright 2020-2021 Andreas Atteneder
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

using UnityEngine;

namespace GLTFast {
    public interface IInstantiator {

        /// <summary>
        /// Used to initialize Instantiators. Always called first.
        /// </summary>
        /// <param name="nodeCount">Quantity of nodes in the glTF file</param>
        void Init(int nodeCount);

        /// <summary>
        /// Called for every Node in the glTF file
        /// </summary>
        /// <param name="nodeIndex">Index of node. Serves as identifier.</param>
        /// <param name="position">Node's local position in hierarchy</param>
        /// <param name="rotation">Node's local rotation in hierarchy</param>
        /// <param name="scale">Node's local scale in hierarchy</param>
        void CreateNode(
            uint nodeIndex,
            Vector3 position,
            Quaternion rotation,
            Vector3 scale
            );

        /// <summary>
        /// Is called to set up hierarchical parent-child relations between nodes.
        /// Always called after both nodes have been created via CreateNode before.
        /// </summary>
        /// <param name="nodeIndex">Index of child node.</param>
        /// <param name="parentIndex">Index of parent node.</param>
        void SetParent(uint nodeIndex, uint parentIndex);

        /// <summary>
        /// Sets the name of a node.
        /// If a node has no name it falls back to the first valid mesh name.
        /// Null otherwise.
        /// </summary>
        /// <param name="nodeIndex">Index of the node to be named.</param>
        /// <param name="name">Valid name or null</param>
        void SetNodeName(uint nodeIndex, string name);

        /// <summary>
        /// Called for adding a Primitive/Mesh to a Node.
        /// </summary>
        /// <param name="nodeIndex">Index of the node</param>
        /// <param name="meshName">Mesh's name</param>
        /// <param name="mesh">The actual Mesh</param>
        /// <param name="materials">The materials</param>
        /// <param name="joints">If a skin was attached, the joint indices. Null otherwise</param>
        /// <param name="primitiveNumeration">Primitves are numerated per Node, starting with 0</param>
        void AddPrimitive(
            uint nodeIndex,
            string meshName,
            Mesh mesh,
            int[] materialIndices,
            int[] joints = null,
            int primitiveNumeration = 0
        );

        /// <summary>
        /// Called for adding a glTF scene.
        /// </summary>
        /// <param name="name">Name of the scene</param>
        /// <param name="nodeIndices">Indices of root level nodes in scene</param>
        void AddScene(
            string name
            ,uint[] nodeIndices
#if UNITY_ANIMATION
            ,AnimationClip[] animationClips
#endif
            );
    }
}
