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

namespace GLTFast
{

    /// <summary>
    /// Instantiation settings
    /// </summary>
    [Serializable]
    public class InstantiationSettings
    {
        /// <summary>
        /// Can be used to exclude component instantiation based on type.
        /// </summary>
        public ComponentType Mask
        {
            get => mask;
            set => mask = value;
        }

        /// <summary>
        /// Instantiated objects will be assigned to this layer.
        /// </summary>
        public int Layer
        {
            get => layer;
            set => layer = value;
        }

        /// <summary>
        /// Corresponds to <see cref="SkinnedMeshRenderer.updateWhenOffscreen"/>
        /// When true, calculate the mesh bounds on every frame, even when
        /// the mesh is not visible.
        /// </summary>
        public bool SkinUpdateWhenOffscreen
        {
            get => skinUpdateWhenOffscreen;
            set => skinUpdateWhenOffscreen = value;
        }

        /// <summary>
        /// Light intensity values are multiplied by this factor.
        /// </summary>
        public float LightIntensityFactor
        {
            get => lightIntensityFactor;
            set => lightIntensityFactor = value;
        }

        /// <inheritdoc cref="GLTFast.SceneObjectCreation"/>
        public SceneObjectCreation SceneObjectCreation
        {
            get => sceneObjectCreation;
            set => sceneObjectCreation = value;
        }

        [SerializeField]
        [Tooltip("Filter component instantiation based on type")]
        ComponentType mask = ComponentType.All;

        [SerializeField]
        [Tooltip("Instantiated objects will be assigned to this layer")]
        int layer;

        [SerializeField]
        [Tooltip("When checked, calculate the mesh bounds on every frame, even when the mesh is not visible")]
        bool skinUpdateWhenOffscreen = true;

        [SerializeField]
        [Tooltip("Light intensity values are multiplied by this factor")]
        float lightIntensityFactor = 1.0f;

        [SerializeField]
        [Tooltip("Scene object creation method. Determines whether or when a GameObject/Entity representing the scene should get created.")]
        SceneObjectCreation sceneObjectCreation = SceneObjectCreation.WhenMultipleRootNodes;
    }
}
