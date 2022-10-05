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

using System;
using UnityEngine;

namespace GLTFast {

    /// <summary>
    /// Instantiation settings
    /// </summary>
    [Serializable]
    public class InstantiationSettings {
        
        /// <summary>
        /// Scene object creation method. Determines whether or when a
        /// GameObject/Entity representing the scene should get created.
        /// </summary>
        public enum SceneObjectCreation {
            /// <summary>
            /// Never create a scene object.
            /// </summary>
            Never,
            /// <summary>
            /// Always create a scene object.
            /// </summary>
            Always,
            /// <summary>
            /// Create a scene object if there is more than one root level node.
            /// Otherwise omit creating a scene object.
            /// </summary>
            WhenMultipleRootNodes
        }
        
        /// <summary>
        /// Can be used to exclude component instantiation based on type. 
        /// </summary>
        [Tooltip("Filter component instantiation based on type")]
        public ComponentType mask = ComponentType.All;
        
        /// <summary>
        /// Instantiated objects will be assigned to this layer.
        /// </summary>
        [Tooltip("Instantiated objects will be assigned to this layer")]
        public int layer;
        
        /// <summary>
        /// Corresponds to <see cref="SkinnedMeshRenderer.updateWhenOffscreen"/>
        /// When true, calculate the mesh bounds on every frame, even when
        /// the mesh is not visible.
        /// </summary>
        [Tooltip("When checked, calculate the mesh bounds on every frame, even when the mesh is not visible")]
        public bool skinUpdateWhenOffscreen = true;
        
        /// <summary>
        /// Light intensity values are multiplied by this factor.
        /// </summary>
        [Tooltip("Light intensity values are multiplied by this factor")]
        public float lightIntensityFactor = 1.0f;

        /// <inheritdoc cref="SceneObjectCreation"/>
        [Tooltip("Scene object creation method. Determines whether or when a GameObject/Entity representing the scene should get created.")]
        public SceneObjectCreation sceneObjectCreation = SceneObjectCreation.WhenMultipleRootNodes;
    }
}
