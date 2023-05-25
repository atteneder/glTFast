// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;

namespace GLTFast
{
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
}
