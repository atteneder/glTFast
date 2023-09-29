// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using GLTFast.Schema;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Profiling;
#if UNITY_ANIMATION
using Animation = UnityEngine.Animation;
#endif
using Camera = UnityEngine.Camera;
using Material = UnityEngine.Material;
using Mesh = UnityEngine.Mesh;

// #if UNITY_EDITOR && UNITY_ANIMATION
// using UnityEditor.Animations;
// #endif

namespace GLTFast
{

    using Logging;

    /// <summary>
    /// Generates a GameObject hierarchy from a glTF scene
    /// </summary>
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
        /// Instantiation logger
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
        /// <param name="logger">Custom logger</param>
        /// <param name="settings">Instantiation settings</param>
        public GameObjectInstantiator(
            IGltfReadable gltf,
            Transform parent,
            ICodeLogger logger = null,
            InstantiationSettings settings = null
            )
        {
            this.m_Gltf = gltf;
            this.m_Parent = parent;
            m_Logger = logger;
            m_Settings = settings ?? new InstantiationSettings();
        }

        /// <inheritdoc />
        public virtual void BeginScene(
            string name,
            uint[] rootNodeIndices
            )
        {
            Profiler.BeginSample("BeginScene");

            m_Nodes = new Dictionary<uint, GameObject>();
            SceneInstance = new GameObjectSceneInstance();

            GameObject sceneGameObject;
            if (m_Settings.SceneObjectCreation == SceneObjectCreation.Never
                || m_Settings.SceneObjectCreation == SceneObjectCreation.WhenMultipleRootNodes && rootNodeIndices.Length == 1)
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
        public void AddAnimation(AnimationClip[] animationClips) {
            if ((m_Settings.Mask & ComponentType.Animation) != 0 && animationClips != null) {
                // we want to create an Animator for non-legacy clips, and an Animation component for legacy clips.
                var isLegacyAnimation = animationClips.Length > 0 && animationClips[0].legacy;
// #if UNITY_EDITOR
//                 // This variant creates a Mecanim Animator and AnimationController
//                 // which does not work at runtime. It's kept for potential Editor import usage
//                 if(!isLegacyAnimation) {
//                     var animator = go.AddComponent<Animator>();
//                     var controller = new UnityEditor.Animations.AnimatorController();
//                     controller.name = animator.name;
//                     controller.AddLayer("Default");
//                     controller.layers[0].defaultWeight = 1;
//                     for (var index = 0; index < animationClips.Length; index++) {
//                         var clip = animationClips[index];
//                         // controller.AddLayer(clip.name);
//                         // controller.layers[index].defaultWeight = 1;
//                         var state = controller.AddMotion(clip, 0);
//                         controller.AddParameter("Test", AnimatorControllerParameterType.Bool);
//                         // var stateMachine = controller.layers[0].stateMachine;
//                         // UnityEditor.Animations.AnimatorState entryState = null;
//                         // var state = stateMachine.AddState(clip.name);
//                         // state.motion = clip;
//                         // var loopTransition = state.AddTransition(state);
//                         // loopTransition.hasExitTime = true;
//                         // loopTransition.duration = 0;
//                         // loopTransition.exitTime = 0;
//                         // entryState = state;
//                         // stateMachine.AddEntryTransition(entryState);
//                         // UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath
//                     }
//
//                     animator.runtimeAnimatorController = controller;
//
//                     // for (var index = 0; index < animationClips.Length; index++) {
//                     //     controller.layers[index].blendingMode = UnityEditor.Animations.AnimatorLayerBlendingMode.Additive;
//                     //     animator.SetLayerWeight(index,1);
//                     // }
//                 }
// #endif // UNITY_EDITOR

                if(isLegacyAnimation) {
                    var animation = SceneTransform.gameObject.AddComponent<Animation>();

                    for (var index = 0; index < animationClips.Length; index++) {
                        var clip = animationClips[index];
                        animation.AddClip(clip,clip.name);
                        if (index < 1) {
                            animation.clip = clip;
                        }
                    }

                    SceneInstance.SetLegacyAnimation(animation);
                }
                else {
                    SceneTransform.gameObject.AddComponent<Animator>();
                }
            }
        }
#endif // UNITY_ANIMATION

        /// <inheritdoc />
        public void CreateNode(
            uint nodeIndex,
            uint? parentIndex,
            Vector3 position,
            Quaternion rotation,
            Vector3 scale
        )
        {
            var go = new GameObject();
            // Deactivate root-level nodes, so half-loaded scenes won't render.
            go.SetActive(parentIndex.HasValue);
            go.transform.localScale = scale;
            go.transform.localPosition = position;
            go.transform.localRotation = rotation;
            go.layer = m_Settings.Layer;
            m_Nodes[nodeIndex] = go;

            go.transform.SetParent(
                parentIndex.HasValue ? m_Nodes[parentIndex.Value].transform : SceneTransform,
                false);

            NodeCreated?.Invoke(nodeIndex, go);
        }

        /// <inheritdoc />
        public virtual void SetNodeName(uint nodeIndex, string name)
        {
            m_Nodes[nodeIndex].name = name ?? $"Node-{nodeIndex}";
        }

        /// <inheritdoc />
        public virtual void AddPrimitive(
            uint nodeIndex,
            string meshName,
            MeshResult meshResult,
            uint[] joints = null,
            uint? rootJoint = null,
            float[] morphTargetWeights = null,
            int primitiveNumeration = 0
        )
        {
            if ((m_Settings.Mask & ComponentType.Mesh) == 0)
            {
                return;
            }

            GameObject meshGo;
            if (primitiveNumeration == 0)
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
                    var bones = new Transform[joints.Length];
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
                    for (var i = 0; i < morphTargetWeights.Length; i++)
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
                var material = m_Gltf.GetMaterial(meshResult.materialIndices[index]) ?? m_Gltf.GetDefaultMaterial();
                materials[index] = material;
            }

            renderer.sharedMaterials = materials;

            MeshAdded?.Invoke(
                meshGo,
                nodeIndex,
                meshName,
                meshResult,
                joints,
                rootJoint,
                morphTargetWeights,
                primitiveNumeration
                );
        }

        /// <inheritdoc />
        public virtual void AddPrimitiveInstanced(
            uint nodeIndex,
            string meshName,
            MeshResult meshResult,
            uint instanceCount,
            NativeArray<Vector3>? positions,
            NativeArray<Quaternion>? rotations,
            NativeArray<Vector3>? scales,
            int primitiveNumeration = 0
        )
        {
            if ((m_Settings.Mask & ComponentType.Mesh) == 0)
            {
                return;
            }

            var materials = new Material[meshResult.materialIndices.Length];
            for (var index = 0; index < materials.Length; index++)
            {
                var material = m_Gltf.GetMaterial(meshResult.materialIndices[index]) ?? m_Gltf.GetDefaultMaterial();
                material.enableInstancing = true;
                materials[index] = material;
            }

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
            switch (camera.GetCameraType())
            {
                case Schema.Camera.Type.Orthographic:
                    var o = camera.Orthographic;
                    AddCameraOrthographic(
                        nodeIndex,
                        o.znear,
                        o.zfar >= 0 ? o.zfar : (float?)null,
                        o.xmag,
                        o.ymag,
                        camera.name
                    );
                    break;
                case Schema.Camera.Type.Perspective:
                    var p = camera.Perspective;
                    AddCameraPerspective(
                        nodeIndex,
                        p.yfov,
                        p.znear,
                        p.zfar,
                        p.aspectRatio > 0 ? p.aspectRatio : (float?)null,
                        camera.name
                    );
                    break;
            }
        }

        void AddCameraPerspective(
            uint nodeIndex,
            float verticalFieldOfView,
            float nearClipPlane,
            float farClipPlane,
            // ReSharper disable once UnusedParameter.Local
            float? aspectRatio,
            string cameraName
        )
        {
            var cam = CreateCamera(nodeIndex, cameraName, out var localScale);

            cam.orthographic = false;

            cam.fieldOfView = verticalFieldOfView * Mathf.Rad2Deg;
            cam.nearClipPlane = nearClipPlane * localScale;
            cam.farClipPlane = farClipPlane * localScale;

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

            if (lightSource.GetLightType() != LightPunctual.Type.Point)
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
        public virtual void EndScene(uint[] rootNodeIndices)
        {
            Profiler.BeginSample("EndScene");
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
        /// <param name="gameObject">GameObject that holds the Msh.</param>
        /// <param name="nodeIndex">Index of the node</param>
        /// <param name="meshName">Mesh's name</param>
        /// <param name="meshResult">The converted Mesh</param>
        /// <param name="joints">If a skin was attached, the joint indices. Null otherwise</param>
        /// <param name="rootJoint">Root joint node index, if present</param>
        /// <param name="morphTargetWeights">Morph target weights, if present</param>
        /// <param name="primitiveNumeration">Primitives are numerated per Node, starting with 0</param>
        public delegate void MeshAddedDelegate(
            GameObject gameObject,
            uint nodeIndex,
            string meshName,
            MeshResult meshResult,
            uint[] joints = null,
            uint? rootJoint = null,
            float[] morphTargetWeights = null,
            int primitiveNumeration = 0
        );

        /// <summary>Invoked when a node's GameObject has been created.</summary>
        public event NodeCreatedDelegate NodeCreated;
        /// <summary>Invoked after a mesh was added to a node GameObject</summary>
        public event MeshAddedDelegate MeshAdded;
        /// <summary>Invoked after a scene has been instantiated.</summary>
        public event Action EndSceneCompleted;
    }
}
