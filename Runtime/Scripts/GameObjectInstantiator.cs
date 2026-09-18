// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Unity.Cloud.Gltfast.Objects;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Scripting.APIUpdating;
#if UNITY_ANIMATION
using Animation = UnityEngine.Animation;
#endif
using Camera = UnityEngine.Camera;
using CameraType = Unity.Cloud.Gltfast.Objects.CameraType;
using LightType = Unity.Cloud.Gltfast.Objects.LightType;
using Material = UnityEngine.Material;
using Mesh = UnityEngine.Mesh;

// #if UNITY_EDITOR && UNITY_ANIMATION
// using UnityEditor.Animations;
// #endif

namespace Unity.Cloud.Gltfast
{

    using Logging;

    /// <summary>
    /// Generates a GameObject hierarchy from a glTF scene
    /// </summary>
    [MovedFrom(true, sourceNamespace: "GLTFast", sourceAssembly: "glTFast")]
    public class GameObjectInstantiator : IInstantiator
    {
        // Developers might want to customize this class by deriving from it.
        // Hence some members need to stay protected (not private)
        // ReSharper disable MemberCanBePrivate.Global

        /// <summary>
        /// Instantiation settings
        /// </summary>
        protected InstantiationSettings m_Settings;

        /// <summary>
        /// Logger used by this instantiator. May be <c>null</c> when the caller passed
        /// <see cref="Unity.Cloud.Gltfast.Logging.NullLogger.Instance"/>; subclasses MUST use
        /// null-conditional access (<c>m_Logger?.Error(...)</c>).
        /// </summary>
        protected ICodeLogger m_Logger;

        /// <summary>
        /// glTF to instantiate from
        /// </summary>
        protected IGltfReadable m_Gltf;

        /// <summary>
        /// Generated GameObjects will get parented to this Transform
        /// </summary>
        protected Transform m_Parent;

        /// <summary>
        /// glTF node index to instantiated GameObject dictionary
        /// </summary>
        protected Dictionary<uint, GameObject> m_Nodes;

        NodeNameFallback m_NameFallback;

        List<IMaterialsVariantsSlotInstance> m_InstanceSlots;

        /// <summary>
        /// Transform representing the scene.
        /// Root nodes will get parented to it.
        /// </summary>
        public Transform SceneTransform { get; protected set; }

        /// <summary>
        /// Contains information about the latest instance of a glTF scene
        /// </summary>
        public GameObjectSceneInstance SceneInstance { get; protected set; }

        // ReSharper restore MemberCanBePrivate.Global

        /// <summary>
        /// Constructs a GameObjectInstantiator
        /// </summary>
        /// <param name="gltf">glTF to instantiate from</param>
        /// <param name="parent">Generated GameObjects will get parented to this Transform</param>
        /// <param name="logger">Custom logger for reporting messages. Defaults to the shared <see cref="Unity.Cloud.Gltfast.Logging.ConsoleLogger.Instance"/> (writes to Unity's Console) when <c>null</c> is passed. Pass <see cref="Unity.Cloud.Gltfast.Logging.NullLogger.Instance"/> (or <c>new NullLogger()</c>) to suppress all output.</param>
        /// <param name="settings">Instantiation settings</param>
        public GameObjectInstantiator(
            IGltfReadable gltf,
            Transform parent,
            ICodeLogger logger = null,
            InstantiationSettings settings = null
            )
        {
            m_Gltf = gltf;
            m_Parent = parent;
            m_Logger = logger is NullLogger ? null : (logger ?? ConsoleLogger.Instance);
            m_Settings = settings ?? new InstantiationSettings();
        }

        /// <inheritdoc />
        public virtual void BeginScene(
            string name,
            IReadOnlyList<uint> rootNodeIndices
            )
        {
            Profiler.BeginSample("BeginScene");

            m_Nodes = new Dictionary<uint, GameObject>();
            m_NameFallback = new NodeNameFallback();
            SceneInstance = new GameObjectSceneInstance();

            GameObject sceneGameObject;
            if (m_Settings.SceneObjectCreation == SceneObjectCreation.Never
                || (m_Settings.SceneObjectCreation == SceneObjectCreation.WhenMultipleRootNodes
                    && (rootNodeIndices == null || rootNodeIndices.Count == 1))
                )
            {
                sceneGameObject = m_Parent.gameObject;
            }
            else
            {
                sceneGameObject = new GameObject(name ?? "Scene");
                sceneGameObject.transform.SetParent(m_Parent, false);
                sceneGameObject.layer = m_Settings.Layer;
            }
            SceneTransform = sceneGameObject.transform;
            Profiler.EndSample();
        }

#if UNITY_ANIMATION
        /// <inheritdoc />
        public virtual void AddAnimation(AnimationClip[] animationClips)
        {
            if ((m_Settings.Mask & ComponentType.Animation) != 0 && animationClips != null)
            {
                // we want to create an Animator for non-legacy clips, and an Animation component for legacy clips.
                var isLegacyAnimation = animationClips.Length > 0 && animationClips[0].legacy;
                if (isLegacyAnimation)
                {
                    var go = SceneTransform.gameObject;
                    var animation = go.AddComponent<Animation>();

                    if (animation == null)
                    {
                        m_Logger?.Error(LogCode.AnimationComponentFail);
                        return;
                    }

                    for (var index = 0; index < animationClips.Length; index++)
                    {
                        var clip = animationClips[index];
                        animation.AddClip(clip, clip.name);
                        if (index < 1)
                        {
                            animation.clip = clip;
                        }
                    }

                    SceneInstance.SetLegacyAnimation(animation);
                }
                else
                {
                    SceneTransform.gameObject.AddComponent<Animator>();
                }
            }
        }
#endif // UNITY_ANIMATION

        /// <inheritdoc />
        public virtual void CreateNode(
            uint nodeIndex,
            uint? parentIndex,
            double3 position,
            double4 rotation,
            double3 scale,
            string name
        )
        {
            var go = new GameObject(name ?? NodeNameFallback.DefaultName(nodeIndex));
            // Deactivate root-level nodes, so half-loaded scenes won't render.
            go.SetActive(parentIndex.HasValue);
            go.transform.localScale = scale.ToVector3();
            go.transform.localPosition = position.ToVector3();
            go.transform.localRotation = rotation.ToUnityEngineQuaternion();
            go.layer = m_Settings.Layer;
            m_Nodes[nodeIndex] = go;

            if (name == null)
            {
                m_NameFallback.MarkUnnamed(nodeIndex);
            }

            go.transform.SetParent(
                parentIndex.HasValue ? m_Nodes[parentIndex.Value].transform : SceneTransform,
                false);

            NodeCreated?.Invoke(nodeIndex, go);
        }

        /// <summary>Applies the mesh-name fallback to a node, if it is still unnamed and the mesh carries a name.</summary>
        /// <param name="nodeIndex">Index of the node.</param>
        /// <param name="meshResult">The mesh being assigned to it.</param>
        protected void ApplyMeshNameFallback(uint nodeIndex, MeshResult meshResult)
        {
            if (m_NameFallback.TryTake(nodeIndex, meshResult, out var meshName))
            {
                SetFallbackNodeName(nodeIndex, meshName);
            }
        }

        /// <summary>Names a node the glTF left unnamed, once, with the first non-empty name among its meshes.</summary>
        /// <remarks>Not called when no mesh supplies a name; the <c>Node-{index}</c> placeholder stands instead.</remarks>
        /// <param name="nodeIndex">Index of the node to name.</param>
        /// <param name="meshName">The resolved fallback name.</param>
        protected virtual void SetFallbackNodeName(uint nodeIndex, string meshName)
        {
            m_Nodes[nodeIndex].name = meshName;
        }

        [Obsolete("AddPrimitive has been renamed to AddMesh. (UnityUpgradable) -> AddMesh(*)", true)]
        public void AddPrimitive(
            uint nodeIndex,
            string meshName,
            MeshResult meshResult,
            IReadOnlyList<uint> joints = null,
            uint? rootJoint = null,
            IReadOnlyList<float> morphTargetWeights = null,
            int meshNumeration = 0
        ) => AddMesh(nodeIndex, meshName, meshResult, joints, rootJoint, morphTargetWeights, meshNumeration);

        /// <inheritdoc />
        public virtual void AddMesh(
            uint nodeIndex,
            string meshName,
            MeshResult meshResult,
            IReadOnlyList<uint> joints = null,
            uint? rootJoint = null,
            IReadOnlyList<float> morphTargetWeights = null,
            int meshNumeration = 0
        )
        {
            ApplyMeshNameFallback(nodeIndex, meshResult);

            if ((m_Settings.Mask & ComponentType.Mesh) == 0)
            {
                return;
            }

            GameObject meshGo;
            if (meshNumeration == 0)
            {
                // Use Node GameObject for first Primitive
                meshGo = m_Nodes[nodeIndex];
            }
            else
            {
                meshGo = new GameObject(meshName);
                meshGo.transform.SetParent(m_Nodes[nodeIndex].transform, false);
                meshGo.layer = m_Settings.Layer;
            }

            Renderer renderer;

            var hasMorphTargets = meshResult.mesh.blendShapeCount > 0;
            if (joints == null && !hasMorphTargets)
            {
                var mf = meshGo.AddComponent<MeshFilter>();
                mf.mesh = meshResult.mesh;
                var mr = meshGo.AddComponent<MeshRenderer>();
                renderer = mr;
            }
            else
            {
                var smr = meshGo.AddComponent<SkinnedMeshRenderer>();
                smr.updateWhenOffscreen = m_Settings.SkinUpdateWhenOffscreen;
                if (joints != null)
                {
                    var bones = new Transform[joints.Count];
                    for (var j = 0; j < bones.Length; j++)
                    {
                        var jointIndex = joints[j];
                        bones[j] = m_Nodes[jointIndex].transform;
                    }
                    smr.bones = bones;
                    if (rootJoint.HasValue)
                    {
                        smr.rootBone = m_Nodes[rootJoint.Value].transform;
                    }
                }
                smr.sharedMesh = meshResult.mesh;
                if (morphTargetWeights != null)
                {
                    for (var i = 0; i < morphTargetWeights.Count; i++)
                    {
                        var weight = morphTargetWeights[i];
                        smr.SetBlendShapeWeight(i, weight);
                    }
                }
                renderer = smr;
            }

            var materials = new Material[meshResult.materialIndices.Length];
            for (var index = 0; index < materials.Length; index++)
            {
                var materialIndex = meshResult.materialIndices[index];
                var material = (materialIndex.HasValue ? m_Gltf.GetMaterial(materialIndex.Value) : null)
                    ?? m_Gltf.GetDefaultMaterial();
                materials[index] = material;
            }

            renderer.sharedMaterials = materials;

            var slots = m_Gltf.GetMaterialsVariantsSlots(meshResult.meshIndex, meshNumeration);
            if (slots != null && slots.Length > 0)
            {
                m_InstanceSlots ??= new List<IMaterialsVariantsSlotInstance>();
                var instanceSlot = new MaterialsVariantsSlotInstances(renderer, slots);
                m_InstanceSlots.Add(instanceSlot);
            }

            MeshAdded?.Invoke(
                meshGo,
                nodeIndex,
                meshName,
                meshResult,
                joints,
                rootJoint,
                morphTargetWeights,
                meshNumeration
                );
        }

        [Obsolete("AddPrimitiveInstanced has been renamed to AddMeshInstanced. (UnityUpgradable) -> AddMeshInstanced(*)", true)]
        public void AddPrimitiveInstanced(
            uint nodeIndex,
            string meshName,
            MeshResult meshResult,
            uint instanceCount,
            NativeArray<Vector3>? positions,
            NativeArray<Quaternion>? rotations,
            NativeArray<Vector3>? scales,
            int meshNumeration = 0
        ) => AddMeshInstanced(nodeIndex, meshName, meshResult, instanceCount, positions, rotations, scales, meshNumeration);

        /// <inheritdoc />
        public virtual void AddMeshInstanced(
            uint nodeIndex,
            string meshName,
            MeshResult meshResult,
            uint instanceCount,
            NativeArray<Vector3>? positions,
            NativeArray<Quaternion>? rotations,
            NativeArray<Vector3>? scales,
            int meshNumeration = 0
        )
        {
            ApplyMeshNameFallback(nodeIndex, meshResult);

            if ((m_Settings.Mask & ComponentType.Mesh) == 0)
            {
                return;
            }

            var materials = new Material[meshResult.materialIndices.Length];
            for (var index = 0; index < materials.Length; index++)
            {
                var materialIndex = meshResult.materialIndices[index];
                var material = (materialIndex.HasValue ? m_Gltf.GetMaterial(materialIndex.Value) : null)
                    ?? m_Gltf.GetDefaultMaterial();
                material.enableInstancing = true;
                materials[index] = material;
            }

            var slots = m_Gltf.GetMaterialsVariantsSlots(meshResult.meshIndex, meshNumeration);
            var hasMaterialsVariants = slots != null && slots.Length > 0;
            var renderers = hasMaterialsVariants
                ? new Renderer[instanceCount]
                : null;

            for (var i = 0; i < instanceCount; i++)
            {
                var meshGo = new GameObject($"{meshName}_i{i}");
                meshGo.layer = m_Settings.Layer;
                var t = meshGo.transform;
                t.SetParent(m_Nodes[nodeIndex].transform, false);
                t.localPosition = positions?[i] ?? Vector3.zero;
                t.localRotation = rotations?[i] ?? Quaternion.identity;
                t.localScale = scales?[i] ?? Vector3.one;

                var mf = meshGo.AddComponent<MeshFilter>();
                mf.mesh = meshResult.mesh;
                Renderer renderer = meshGo.AddComponent<MeshRenderer>();
                renderer.sharedMaterials = materials;

                if (hasMaterialsVariants)
                {
                    renderers[i] = renderer;
                }
            }

            if (hasMaterialsVariants)
            {
                m_InstanceSlots ??= new List<IMaterialsVariantsSlotInstance>();
                var instanceSlot = new MultiMaterialsVariantsSlotInstances(renderers, slots);
                m_InstanceSlots.Add(instanceSlot);
            }
        }

        /// <inheritdoc />
        public virtual void AddCamera(uint nodeIndex, uint cameraIndex)
        {
            if ((m_Settings.Mask & ComponentType.Camera) == 0)
            {
                return;
            }
            var camera = m_Gltf.GetSourceCamera(cameraIndex);
            if (camera.Type.Value == CameraType.Undefined)
            {
                m_Logger?.Error($"Camera {cameraIndex} has unknown type {camera.Type.ToString()}");
                return;
            }
            switch (camera.Type.Value)
            {
                case CameraType.Orthographic:
                    var o = camera.Orthographic;
                    AddCameraOrthographic(
                        nodeIndex,
                        o.Znear,
                        o.Zfar,
                        o.Xmag,
                        o.Ymag,
                        camera.Name
                    );
                    break;
                case CameraType.Perspective:
                    var p = camera.Perspective;
                    AddCameraPerspective(
                        nodeIndex,
                        p.Yfov,
                        p.Znear,
                        p.Zfar,
                        p.AspectRatio > 0 ? p.AspectRatio : (float?)null,
                        camera.Name
                    );
                    break;
            }
        }

        void AddCameraPerspective(
            uint nodeIndex,
            float verticalFieldOfView,
            float nearClipPlane,
            float? farClipPlane,
            // ReSharper disable once UnusedParameter.Local
            float? aspectRatio,
            string cameraName
        )
        {
            var cam = CreateCamera(nodeIndex, cameraName, out var localScale);
            var farValue = farClipPlane ?? float.MaxValue;

            cam.orthographic = false;

            cam.fieldOfView = verticalFieldOfView * Mathf.Rad2Deg;
            cam.nearClipPlane = nearClipPlane * localScale;
            cam.farClipPlane = farValue * localScale;

            // // If the aspect ratio is given and does not match the
            // // screen's aspect ratio, the viewport rect is reduced
            // // to match the glTFs aspect ratio (box fit)
            // if (aspectRatio.HasValue) {
            //     cam.rect = GetLimitedViewPort(aspectRatio.Value);
            // }
        }

        void AddCameraOrthographic(
            uint nodeIndex,
            float nearClipPlane,
            float? farClipPlane,
            float horizontal,
            float vertical,
            string cameraName
        )
        {
            var cam = CreateCamera(nodeIndex, cameraName, out var localScale);

            var farValue = farClipPlane ?? float.MaxValue;

            cam.orthographic = true;
            cam.nearClipPlane = nearClipPlane * localScale;
            cam.farClipPlane = farValue * localScale;
            cam.orthographicSize = vertical; // Note: Ignores `horizontal`

            // Custom projection matrix
            // Ignores screen's aspect ratio
            cam.projectionMatrix = Matrix4x4.Ortho(
                -horizontal,
                horizontal,
                -vertical,
                vertical,
                nearClipPlane,
                farValue
            );

            // // If the aspect ratio does not match the
            // // screen's aspect ratio, the viewport rect is reduced
            // // to match the glTFs aspect ratio (box fit)
            // var aspectRatio = horizontal / vertical;
            // cam.rect = GetLimitedViewPort(aspectRatio);
        }

        /// <summary>
        /// Creates a camera component on the given node and returns an approximated
        /// local-to-world scale factor, required to counteract that Unity scales
        /// near- and far-clipping-planes via Transform.
        /// </summary>
        /// <param name="nodeIndex">Node's index</param>
        /// <param name="cameraName">Camera's name</param>
        /// <param name="localScale">Approximated local-to-world scale factor</param>
        /// <returns>The newly created Camera component</returns>
        Camera CreateCamera(uint nodeIndex, string cameraName, out float localScale)
        {
            var cameraParent = m_Nodes[nodeIndex];
            var camGo = new GameObject(
                string.IsNullOrEmpty(cameraName)
                    ? $"Camera-{nodeIndex}"
                    : $"{cameraParent.name}-Camera"
                );
            camGo.layer = m_Settings.Layer;
            var camTrans = camGo.transform;
            var parentTransform = cameraParent.transform;
            camTrans.SetParent(parentTransform, false);
            var tmp = Quaternion.Euler(0, 180, 0);
            camTrans.localRotation = tmp;
            var cam = camGo.AddComponent<Camera>();

            // By default, imported cameras are not enabled by default
            cam.enabled = false;

            SceneInstance.AddCamera(cam);

            var parentScale = parentTransform.localToWorldMatrix.lossyScale;
            localScale = (parentScale.x + parentScale.y + parentScale.y) / 3;

            return cam;
        }

        // static Rect GetLimitedViewPort(float aspectRatio) {
        //     var screenAspect = Screen.width / (float)Screen.height;
        //     if (Mathf.Abs(1 - (screenAspect / aspectRatio)) <= math.EPSILON) {
        //         // Identical aspect ratios
        //         return new Rect(0,0,1,1);
        //     }
        //     if (aspectRatio < screenAspect) {
        //         var w = aspectRatio / screenAspect;
        //         return new Rect((1 - w) / 2, 0, w, 1f);
        //     } else {
        //         var h = screenAspect / aspectRatio;
        //         return new Rect(0, (1 - h) / 2, 1f, h);
        //     }
        // }

        /// <inheritdoc />
        public void AddLightPunctual(
            uint nodeIndex,
            uint lightIndex
        )
        {
            if ((m_Settings.Mask & ComponentType.Light) == 0)
            {
                return;
            }
            var lightGameObject = m_Nodes[nodeIndex];
            var lightSource = m_Gltf.GetSourceLightPunctual(lightIndex);

            if (lightSource.Type != LightType.Point)
            {
                // glTF lights' direction is flipped, compared with Unity's, so
                // we're adding a rotated child GameObject to counteract.
                var tmp = new GameObject($"{lightGameObject.name}_Orientation");
                tmp.transform.SetParent(lightGameObject.transform, false);
                tmp.transform.localEulerAngles = new Vector3(0, 180, 0);
                lightGameObject = tmp;
            }
            var light = lightGameObject.AddComponent<Light>();
            lightSource.ToUnityLight(light, m_Settings.LightIntensityFactor);
            SceneInstance.AddLight(light);
        }

        /// <inheritdoc />
        public virtual void EndScene(IReadOnlyList<uint> rootNodeIndices)
        {
            Profiler.BeginSample("EndScene");

            m_NameFallback?.Release();

            if (m_InstanceSlots != null)
            {
                var materialsVariantsControl = new MaterialsVariantsControl(m_Gltf, m_InstanceSlots);
                SceneInstance.SetMaterialsVariantsControl(materialsVariantsControl);
            }

            if (rootNodeIndices != null)
            {
                foreach (var nodeIndex in rootNodeIndices)
                {
                    m_Nodes[nodeIndex].SetActive(true);
                }
            }

            EndSceneCompleted?.Invoke();

            Profiler.EndSample();
        }

        /// <summary>
        /// Information for when a node's GameObject has been created.
        /// </summary>
        /// <param name="nodeIndex">Index of the corresponding glTF node.</param>
        /// <param name="gameObject">GameObject that was created.</param>
        public delegate void NodeCreatedDelegate(
            uint nodeIndex,
            GameObject gameObject
        );

        /// <summary>
        /// Provides information for when a mesh was added to a node GameObject
        /// </summary>
        /// <param name="gameObject">GameObject that holds the Mesh.</param>
        /// <param name="nodeIndex">Index of the node</param>
        /// <param name="meshName">Mesh's name</param>
        /// <param name="meshResult">The converted Mesh</param>
        /// <param name="joints">If a skin was attached, the joint indices. Null otherwise</param>
        /// <param name="rootJoint">Root joint node index, if present</param>
        /// <param name="morphTargetWeights">Morph target weights, if present</param>
        /// <param name="meshNumeration">Per glTF mesh <see cref="MeshResult"/> numeration. A glTF mesh is converted
        /// into one or more MeshResults which are numbered consecutively.</param>
        public delegate void MeshAddedDelegate(
            GameObject gameObject,
            uint nodeIndex,
            string meshName,
            MeshResult meshResult,
            IReadOnlyList<uint> joints = null,
            uint? rootJoint = null,
            IReadOnlyList<float> morphTargetWeights = null,
            int meshNumeration = 0
        );

        /// <summary>Invoked when a node's GameObject has been created.</summary>
        public event NodeCreatedDelegate NodeCreated;
        /// <summary>Invoked after a mesh was added to a node GameObject</summary>
        public event MeshAddedDelegate MeshAdded;
        /// <summary>Invoked after a scene has been instantiated.</summary>
        public event Action EndSceneCompleted;
    }
}
