// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using UnityEngine.Scripting.APIUpdating;

namespace Unity.Cloud.Gltfast
{
    /// <summary>
    /// Target animation system
    /// </summary>
    [MovedFrom(true, sourceNamespace: "GLTFast", sourceAssembly: "glTFast")]
    public enum AnimationMethod
    {
        /// <summary>
        /// Don't target or import animation
        /// </summary>
        None,
        /// <summary>
        /// <a href="https://docs.unity3d.com/Manual/Animations.html">Legacy Animation System</a>
        /// </summary>
        Legacy,
        /// <summary>
        /// <a href="https://docs.unity3d.com/Manual/AnimationOverview.html">Default Animation System (Mecanim)</a>
        /// </summary>
        Mecanim,
        /// <summary>
        /// <a href="https://docs.unity3d.com/Manual/Playables.html">Playables</a> support has been removed since
        /// it was not usable in builds. Use LegacyAnimation instead.
        /// See: <a href="https://docs.unity3d.com/Packages/com.unity.cloud.gltfast@6.13/manual/UseCaseCustomPlayablesAnimation.html">UseCaseCustomPlayablesAnimation</a>
        /// </summary>
        [Obsolete("Playables support has been removed since it was not usable in builds. Use LegacyAnimation instead. " +
            "See: <a href=\"https://docs.unity3d.com/Packages/com.unity.cloud.gltfast@6.13/manual/UseCaseCustomPlayablesAnimation.html\">UseCaseCustomPlayablesAnimation</a>")]
        Playables
    }
}
