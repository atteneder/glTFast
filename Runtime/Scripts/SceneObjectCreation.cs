// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using UnityEngine.Scripting.APIUpdating;

namespace Unity.Cloud.Gltfast
{
    /// <summary>
    /// Scene object creation method. Determines whether or when a
    /// GameObject/Entity representing the scene should get created.
    /// </summary>
    [MovedFrom(true, sourceNamespace: "GLTFast", sourceAssembly: "glTFast")]
    public enum SceneObjectCreation
    {
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
}
