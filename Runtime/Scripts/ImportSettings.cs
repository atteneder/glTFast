// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Unity.Cloud.Gltfast
{

    using Objects;

    /// <summary>
    /// glTF import settings
    /// </summary>
    [Serializable]
    [MovedFrom(true, sourceNamespace: "GLTFast", sourceAssembly: "glTFast")]
    public class ImportSettings
    {
        /// <inheritdoc cref="NameImportMethod"/>
        public NameImportMethod NodeNameMethod
        {
            get => nodeNameMethod;
            set => nodeNameMethod = value;
        }

        /// <inheritdoc cref="Unity.Cloud.Gltfast.AnimationMethod"/>
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
        /// Create textures readable. Increases memory consumption.
        /// </summary>
        public bool TexturesReadable
        {
            get => texturesReadable;
            set => texturesReadable = value;
        }

        /// <summary>
        /// Defines the default minification filter mode for textures that have no such specification in data
        /// </summary>
        public MinFilterMode DefaultMinFilterMode
        {
            get => defaultMinFilterMode;
            set => defaultMinFilterMode = value;
        }

        /// <summary>
        /// Define the default magnification filter mode for textures that have no such specification in data
        /// </summary>
        public MagFilterMode DefaultMagFilterMode
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
        [Tooltip("Create textures readable. Increases memory consumption.")]
        bool texturesReadable;

        [SerializeField]
        [Tooltip("Minification filter mode fallback if no mode was provided.")]
        MinFilterMode defaultMinFilterMode = MinFilterMode.Linear;

        [SerializeField]
        [Tooltip("Magnification filter mode fallback if no mode was provided.")]
        MagFilterMode defaultMagFilterMode = MagFilterMode.Linear;

        [SerializeField]
        [Tooltip("Anisotropic filtering level for imported textures.")]
        int anisotropicFilterLevel = 1;
    }
}
