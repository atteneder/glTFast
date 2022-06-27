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

    using Schema;
    
    /// <summary>
    /// glTF import settings
    /// </summary>
    [Serializable]
    public class ImportSettings {
        
        /// <summary>
        /// Defines how node names are created
        /// </summary>
        public enum NameImportMethod {
            /// <summary>
            /// Use original node names.
            /// Fallback to mesh's name (if present)
            /// Fallback to "Node_&lt;index&gt;" as last resort. 
            /// </summary>
            Original,
            /// <summary>
            /// Identical to <see cref="Original">Original</see>, but
            /// names are made unique (within their hierarchical position)
            /// by supplementing a continuous number.
            /// This is required for correct animation target lookup and import continuity. 
            /// </summary>
            OriginalUnique
        }

        /// <summary>
        /// Target animation system
        /// </summary>
        public enum AnimationMethod
        {
            /// <summary>
            /// Don't target or import animation
            /// </summary>
            None,
            /// <summary>
            /// <see href="https://docs.unity3d.com/Manual/Animations.html">Legacy Animation System</see>
            /// </summary>
            Legacy,
            /// <summary>
            /// <see href="https://docs.unity3d.com/Manual/AnimationOverview.html">Default Animation System (Mecanim)</see>
            /// </summary>
            Mecanim
        }

        /// <inheritdoc cref="NameImportMethod"/>
        [Tooltip("Controls how node names are created.")]
        public NameImportMethod nodeNameMethod = NameImportMethod.Original;
        
        /// <inheritdoc cref="AnimationMethod"/>
        [Tooltip("Target animation system.")]
        public AnimationMethod animationMethod = AnimationMethod.Legacy;

        /// <summary>
        /// Set this property to true to enable mip map generation.
        /// Note: Creating mipmaps from Jpeg/PNG textures is very slow (at the moment).
        /// See https://github.com/atteneder/glTFast/issues/220 for details 
        /// </summary>
        [Tooltip("Controls if mipmaps are created for imported textures.")]
        public bool generateMipMaps;

        /// <summary>
        /// Defines the default minification filter mode for textures that have no such specification in data
        /// </summary>
        [Tooltip("Minification filter mode fallback if no mode was provided.")]
        public Sampler.MinFilterMode defaultMinFilterMode = Sampler.MinFilterMode.Linear;
        
        /// <summary>
        /// Define the default magnification filter mode for textures that have no such specification in data
        /// </summary>
        [Tooltip("Magnification filter mode fallback if no mode was provided.")]
        public Sampler.MagFilterMode defaultMagFilterMode = Sampler.MagFilterMode.Linear;

        /// <summary>
        /// This property defines the anisotropic filtering level for imported textures
        /// </summary>
        [Tooltip("Anisotropic filtering level for imported textures.")]
        public int anisotropicFilterLevel = 1;
    }
}
