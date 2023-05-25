// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

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
