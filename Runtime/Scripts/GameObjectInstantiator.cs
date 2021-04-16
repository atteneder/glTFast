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

using System;
using UnityEngine;
// #if UNITY_EDITOR && UNITY_ANIMATION
// using UnityEditor.Animations;
// #endif

namespace GLTFast {
    public class GameObjectInstantiator : IInstantiator {

        protected Transform parent;

        protected GameObject[] nodes;

        public GameObjectInstantiator(Transform parent) {
            this.parent = parent;
        }

        public virtual void Init(int nodeCount) {
            nodes = new GameObject[nodeCount];
        }

        public void CreateNode(
            uint nodeIndex,
            Vector3 position,
            Quaternion rotation,
            Vector3 scale
        ) {
            var go = new GameObject();
            go.transform.localScale = scale;
            go.transform.localPosition = position;
            go.transform.localRotation = rotation;
            nodes[nodeIndex] = go;
        }

        public void SetParent(uint nodeIndex, uint parentIndex) {
            if(nodes[nodeIndex]==null || nodes[parentIndex]==null ) {
                Debug.LogError("Invalid hierarchy");
                return;
            }
            nodes[nodeIndex].transform.SetParent(nodes[parentIndex].transform,false);
        }

        public void SetNodeName(uint nodeIndex, string name) {
            nodes[nodeIndex].name = name ?? $"Node-{nodeIndex}";
        }

        public virtual void AddPrimitive(
            uint nodeIndex,
            string meshName,
            UnityEngine.Mesh mesh,
            UnityEngine.Material[] materials,
            int[] joints = null,
            int primitiveNumeration = 0
        ) {

            GameObject meshGo;
            if(primitiveNumeration==0) {
                // Use Node GameObject for first Primitive
                meshGo = nodes[nodeIndex];
            } else {
                meshGo = new GameObject( $"{meshName ?? "Primitive"}_{primitiveNumeration}" );
                meshGo.transform.SetParent(nodes[nodeIndex].transform,false);
            }

            Renderer renderer;

            if(joints==null) {
                var mf = meshGo.AddComponent<MeshFilter>();
                mf.mesh = mesh;
                var mr = meshGo.AddComponent<MeshRenderer>();
                renderer = mr;
            } else {
                var smr = meshGo.AddComponent<SkinnedMeshRenderer>();
                var bones = new Transform[joints.Length];
                for (var j = 0; j < bones.Length; j++)
                {
                    var jointIndex = joints[j];
                    bones[j] = nodes[jointIndex].transform;
                }
                smr.bones = bones;
                smr.sharedMesh = mesh;
                renderer = smr;
            }

            renderer.sharedMaterials = materials;
        }

        public void AddScene(
            string name,
            uint[] nodeIndices
#if UNITY_ANIMATION
            ,AnimationClip[] animationClips
#endif // UNITY_ANIMATION
            )
        {
            var go = new GameObject(name ?? "Scene");
            go.transform.SetParent( parent, false);

            foreach(var nodeIndex in nodeIndices) {
                if (nodes[nodeIndex] != null) {
                    nodes[nodeIndex].transform.SetParent( go.transform, false );
                }
            }

#if UNITY_ANIMATION
            if (animationClips != null) {
                
// #if UNITY_EDITOR
//                 // This variant creates a Mecanim Animator and AnimationController
//                 // which does not work at runtime. It's kept for potential Editor import usage
//
//                 var animator = go.AddComponent<Animator>();
//                 var controller = new AnimatorController();
//                 
//                 for (var index = 0; index < animationClips.Length; index++) {
//                     var clip = animationClips[index];
//                     controller.AddLayer(clip.name);
//                     // controller.layers[index].defaultWeight = 1;
//                     var stateMachine = controller.layers[index].stateMachine;
//                     AnimatorState entryState = null;
//                     var state = stateMachine.AddState(clip.name);
//                     state.motion = clip;
//                     var loopTransition = state.AddTransition(state);
//                     loopTransition.hasExitTime = true;
//                     loopTransition.duration = 0;
//                     loopTransition.exitTime = 0;
//                     entryState = state;
//                     stateMachine.AddEntryTransition(entryState);
//                 }
//                 
//                 animator.runtimeAnimatorController = controller;
//                 
//                 for (var index = 0; index < animationClips.Length; index++) {
//                     controller.layers[index].blendingMode = AnimatorLayerBlendingMode.Additive;
//                     animator.SetLayerWeight(index,1);
//                 }
// #endif // UNITY_EDITOR

                var animation = go.AddComponent<Animation>();
                
                for (var index = 0; index < animationClips.Length; index++) {
                    var clip = animationClips[index];
                    animation.AddClip(clip,clip.name);
                    if (index < 1) {
                        animation.clip = clip;
                    }
                }
                animation.Play();
            }
#endif // UNITY_ANIMATION
        }
    }
}
