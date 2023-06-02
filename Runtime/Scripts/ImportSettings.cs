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

    using Schema;

    /// <summary>
    /// glTF import settings
    /// </summary>
    [Serializable]
    public class ImportSettings
    {
        /// <inheritdoc cref="NameImportMethod"/>
        public NameImportMethod NodeNameMethod
        {
            get => nodeNameMethod;
            set => nodeNameMethod = value;
        }

        /// <inheritdoc cref="GLTFast.AnimationMethod"/>
        public AnimationMethod AnimationMethod
        {
            get => animationMethod;
            set => animationMethod = value;
        }

        /// <summary>
        /// Set this property to true to enable mip map generation.
        /// Note: Creating mipmaps from Jpeg/PNG textures is very slow (at the moment).
        /// See https://github.com/atteneder/glTFast/issues/220 for details
        /// </summary>
        public bool GenerateMipMaps
        {
            get => generateMipMaps;
            set => generateMipMaps = value;
        }

        /// <summary>
        /// Defines the default minification filter mode for textures that have no such specification in data
        /// </summary>
        public Sampler.MinFilterMode DefaultMinFilterMode
        {
            get => defaultMinFilterMode;
            set => defaultMinFilterMode = value;
        }

        /// <summary>
        /// Define the default magnification filter mode for textures that have no such specification in data
        /// </summary>
        public Sampler.MagFilterMode DefaultMagFilterMode
        {
            get => defaultMagFilterMode;
            set => defaultMagFilterMode = value;
        }

        /// <summary>
        /// This property defines the anisotropic filtering level for imported textures
        /// </summary>
        public int AnisotropicFilterLevel
        {
            get => anisotropicFilterLevel;
            set => anisotropicFilterLevel = value;
        }

        [SerializeField]
        [Tooltip("Controls how node names are created.")]
        NameImportMethod nodeNameMethod = NameImportMethod.Original;

        [SerializeField]
        [Tooltip("Target animation system.")]
        AnimationMethod animationMethod = AnimationMethod.Legacy;

        [SerializeField]
        [Tooltip("Controls if mipmaps are created for imported textures.")]
        bool generateMipMaps;

        [SerializeField]
        [Tooltip("Minification filter mode fallback if no mode was provided.")]
        Sampler.MinFilterMode defaultMinFilterMode = Sampler.MinFilterMode.Linear;

        [SerializeField]
        [Tooltip("Magnification filter mode fallback if no mode was provided.")]
        Sampler.MagFilterMode defaultMagFilterMode = Sampler.MagFilterMode.Linear;

        [SerializeField]
        [Tooltip("Anisotropic filtering level for imported textures.")]
        int anisotropicFilterLevel = 1;
    }
}
