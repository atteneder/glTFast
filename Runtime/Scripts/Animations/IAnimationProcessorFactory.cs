// SPDX-FileCopyrightText: 2026 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using UnityEngine.Scripting.APIUpdating;

namespace Unity.Cloud.Gltfast.Animations
{
    /// <summary>
    /// Creates per-import <see cref="IAnimationProcessor"/> instances.
    /// </summary>
    /// <remarks>
    /// Add-ons that want to handle animation import implement this interface (typically on the
    /// same type that derives from <see cref="Unity.Cloud.Gltfast.Addons.ImportAddonInstance"/>) and a fresh
    /// <see cref="IAnimationProcessor"/> is requested for every import call.
    /// </remarks>
    [MovedFrom(true, sourceNamespace: "GLTFast.Animations", sourceAssembly: "glTFast")]
    public interface IAnimationProcessorFactory
    {
        /// <summary>
        /// Creates a new <see cref="IAnimationProcessor"/> for the upcoming animation conversion phase.
        /// </summary>
        /// <param name="clipCount">Total number of animation clips to expect.</param>
        /// <returns>
        /// A new <see cref="IAnimationProcessor"/> instance owned by the caller. The caller disposes
        /// it at the end of the conversion phase.
        /// </returns>
        IAnimationProcessor CreateAnimationProcessor(int clipCount);
    }
}
