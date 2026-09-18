// SPDX-FileCopyrightText: 2026 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Unity.Cloud.Gltfast.Addons;
using Unity.Cloud.Gltfast.Objects;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Unity.Cloud.Gltfast.Animations
{
    /// <summary>
    /// Interface for processing animation clips.
    /// The animation clip/curve data may be processed towards a specific animation system.
    /// </summary>
    /// <remarks>
    /// A processor instance is created by an <see cref="IAnimationProcessorFactory"/> for a single
    /// import call (the conversion phase). Its lifetime ends when the conversion phase ends and
    /// <see cref="IDisposable.Dispose"/> is invoked. <see cref="IDisposable.Dispose"/> is the
    /// place to release scratch buffers and other temporary resources.
    /// </remarks>
    [MovedFrom(true, sourceNamespace: "GLTFast.Animations", sourceAssembly: "glTFast")]
    public interface IAnimationProcessor : IDisposable
    {
        /// <summary>
        /// Initialize a new animation clip with the given name and index.
        /// Is called before any animation curves are added to the clip.
        /// </summary>
        /// <param name="index">glTF animation clip index.</param>
        /// <param name="name">glTF animation clip name.</param>
        void AddClip(int index, string name);

        /// <summary>
        /// Adds a translation curve to an animation clip.
        /// </summary>
        /// <param name="clipIndex">glTF animation clip index.</param>
        /// <param name="targetNode">glTF index of the targeted node.</param>
        /// <param name="nodeHierarchyInfo">Can be used to query hierarchical information and
        /// build an animation path string.</param>
        /// <param name="times">Time values.</param>
        /// <param name="values">Output translation values.</param>
        /// <param name="interpolation">Interpolation type.</param>
        void AddTranslationCurves(
            int clipIndex,
            int targetNode,
            INodeHierarchyInfo nodeHierarchyInfo,
            NativeArray<float>.ReadOnly times,
            NativeArray<float3>.ReadOnly values,
            Interpolation interpolation
        );

        /// <summary>
        /// Adds a rotation curve to an animation clip.
        /// </summary>
        /// <param name="clipIndex">glTF animation clip index.</param>
        /// <param name="targetNode">glTF index of the targeted node.</param>
        /// <param name="nodeHierarchyInfo">Can be used to query hierarchical information and
        /// build an animation path string.</param>
        /// <param name="times">Time values.</param>
        /// <param name="values">Output rotation values.</param>
        /// <param name="interpolation">Interpolation type.</param>
        void AddRotationCurves(
            int clipIndex,
            int targetNode,
            INodeHierarchyInfo nodeHierarchyInfo,
            NativeArray<float>.ReadOnly times,
            NativeArray<quaternion>.ReadOnly values,
            Interpolation interpolation
        );

        /// <summary>
        /// Adds a local scale curve to an animation clip.
        /// </summary>
        /// <param name="clipIndex">glTF animation clip index.</param>
        /// <param name="targetNode">glTF index of the targeted node.</param>
        /// <param name="nodeHierarchyInfo">Can be used to query hierarchical information and
        /// build an animation path string.</param>
        /// <param name="times">Time values.</param>
        /// <param name="values">Output scale values.</param>
        /// <param name="interpolation">Interpolation type.</param>
        void AddScaleCurves(
            int clipIndex,
            int targetNode,
            INodeHierarchyInfo nodeHierarchyInfo,
            NativeArray<float>.ReadOnly times,
            NativeArray<float3>.ReadOnly values,
            Interpolation interpolation
        );

        /// <summary>
        /// Adds a morph target weight curve to an animation clip.
        /// </summary>
        /// <param name="clipIndex">glTF animation clip index.</param>
        /// <param name="targetNode">glTF index of the targeted node.</param>
        /// <param name="meshNumeration">Target mesh number. A glTF mesh is converted into one or more
        /// <see cref="MeshResult"/> which are numbered consecutively.
        /// <see cref="IInstantiator.AddMesh"/> is called once for each of those MeshResults
        /// and the meshNumeration matches.</param>
        /// <param name="meshName">Name of the targeted Unity mesh.</param>
        /// <param name="nodeHierarchyInfo">Can be used to query hierarchical information and
        /// build an animation path string.</param>
        /// <param name="times">Time values.</param>
        /// <param name="values">Output morph target weight values.</param>
        /// <param name="interpolation">Interpolation type.</param>
        /// <param name="morphTargetNames">Morph targets' names.</param>
        void AddMorphTargetWeightCurves(
            int clipIndex,
            int targetNode,
            int meshNumeration,
            string meshName,
            INodeHierarchyInfo nodeHierarchyInfo,
            NativeArray<float>.ReadOnly times,
            NativeArray<float>.ReadOnly values,
            Interpolation interpolation,
            IReadOnlyList<string> morphTargetNames = null
        );

        /// <summary>
        /// Signals that the conversion phase has finished and all animation curves have been added.
        /// Implementations may finalize their internal state and optionally return a factory that
        /// produces appliers to attach the processed animation data to instantiated scenes.
        /// </summary>
        /// <returns>An <see cref="IDataInstanceApplierFactory"/> that creates instance appliers for
        /// the processed animation data, or <see langword="null"/> if no per-instance application is
        /// required.</returns>
        IDataInstanceApplierFactory Complete() => null;
    }
}
