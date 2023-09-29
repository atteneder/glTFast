// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using Unity.Collections;
using UnityEngine;

namespace GLTFast
{

    /// <summary>
    /// After parsing and loading a glTF's content and converting its content
    /// into Unity resources,the second step is instantiation.
    /// Implementors of this interface can convert glTF resources into scene
    /// objects.
    /// </summary>
    public interface IInstantiator
    {

        /// <summary>
        /// Starts creating a scene instance.
        /// Has to be called at first and concluded by calling
        /// <see cref="EndScene"/>.
        /// </summary>
        /// <param name="name">Name of the scene</param>
        /// <param name="rootNodeIndices">Indices of root level nodes in scene</param>
        void BeginScene(
            string name
            , uint[] rootNodeIndices
        );

#if UNITY_ANIMATION
        /// <summary>
        /// Adds animation clips to the current scene.
        /// Only available if the built-in Animation module is enabled.
        /// </summary>
        /// <param name="animationClips">Animation clips</param>
        void AddAnimation(AnimationClip[] animationClips);
#endif

        /// <summary>
        /// Called for every Node in the glTF file
        /// </summary>
        /// <param name="nodeIndex">Index of node. Serves as identifier.</param>
        /// <param name="parentIndex">Index of the parent's node. If it's null,
        /// the node's a root-level node</param>
        /// <param name="position">Node's local position in hierarchy</param>
        /// <param name="rotation">Node's local rotation in hierarchy</param>
        /// <param name="scale">Node's local scale in hierarchy</param>
        void CreateNode(
            uint nodeIndex,
            uint? parentIndex,
            Vector3 position,
            Quaternion rotation,
            Vector3 scale
            );

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
        /// <param name="meshResult">The converted Mesh</param>
        /// <param name="joints">If a skin was attached, the joint indices. Null otherwise</param>
        /// <param name="rootJoint">Root joint node index, if present</param>
        /// <param name="morphTargetWeights">Morph target weights, if present</param>
        /// <param name="primitiveNumeration">Primitives are numerated per Node, starting with 0</param>
        void AddPrimitive(
            uint nodeIndex,
            string meshName,
            MeshResult meshResult,
            uint[] joints = null,
            uint? rootJoint = null,
            float[] morphTargetWeights = null,
            int primitiveNumeration = 0
        );

        /// <summary>
        /// Called for adding a Primitive/Mesh to a Node that uses
        /// GPU instancing (EXT_mesh_gpu_instancing) to render the same mesh/material combination many times.
        /// Similar to/called instead of <see cref="AddPrimitive"/>, without joints/skin support.
        /// </summary>
        /// <param name="nodeIndex">Index of the node</param>
        /// <param name="meshName">Mesh's name</param>
        /// <param name="meshResult">The converted Mesh</param>
        /// <param name="instanceCount">Number of instances</param>
        /// <param name="positions">Instance positions</param>
        /// <param name="rotations">Instance rotations</param>
        /// <param name="scales">Instance scales</param>
        /// <param name="primitiveNumeration">Primitives are numerated per Node, starting with 0</param>
        void AddPrimitiveInstanced(
            uint nodeIndex,
            string meshName,
            MeshResult meshResult,
            uint instanceCount,
            NativeArray<Vector3>? positions,
            NativeArray<Quaternion>? rotations,
            NativeArray<Vector3>? scales,
            int primitiveNumeration = 0
        );

        /// <summary>
        /// Called when a node has a camera assigned
        /// </summary>
        /// <param name="nodeIndex">Index of the node</param>
        /// <param name="cameraIndex">Index of the assigned camera</param>
        void AddCamera(
            uint nodeIndex,
            uint cameraIndex
        );

        /// <summary>
        /// Called when a node has a punctual light assigned (KHR_lights_punctual)
        /// </summary>
        /// <param name="nodeIndex">Index of the node</param>
        /// <param name="lightIndex">Index of the punctual light</param>
        void AddLightPunctual(
            uint nodeIndex,
            uint lightIndex
        );

        /// <summary>
        /// Is called at last, after all scene content has been created.
        /// Immediately afterwards the scene will be rendered, so use it to
        /// finally prepare the instance.
        /// </summary>
        /// <param name="rootNodeIndices">Indices of root level nodes in scene</param>
        void EndScene(
            uint[] rootNodeIndices
        );
    }
}
