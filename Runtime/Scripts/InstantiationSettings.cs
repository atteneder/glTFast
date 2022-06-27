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

namespace GLTFast {

    /// <summary>
    /// Instantiation settings
    /// </summary>
    [Serializable]
    public class InstantiationSettings {
        
        public enum SceneObjectCreation {
            Never,
            Always,
            WhenSingleRootNode
        }
        
        /// <summary>
        /// Can be used to exclude component instantiation based on type. 
        /// </summary>
        public ComponentType mask = ComponentType.All;
        
        /// <summary>
        /// Instantiated objects will be assigned to this layer.
        /// </summary>
        public int layer;
        
        /// <summary>
        /// Corresponds to <see cref="SkinnedMeshRenderer.updateWhenOffscreen"/>
        /// When true, calculate the mesh bounds at all times, even when
        /// the mesh is not visible.
        /// </summary>
        public bool skinUpdateWhenOffscreen = true;
        
        /// <summary>
        /// Light intensity values are multiplied by this factor.
        /// </summary>
        public float lightIntensityFactor = 1.0f;

        public SceneObjectCreation sceneObjectCreation = SceneObjectCreation.WhenSingleRootNode;
    }
}
