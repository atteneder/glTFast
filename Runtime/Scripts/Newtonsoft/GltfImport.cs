// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Unity.Cloud.Gltfast.Newtonsoft
{
    /// <summary>
    /// Loads a glTF's content, converts it to Unity resources and is able to
    /// feed it to an <see cref="IInstantiator"/> for instantiation.
    /// Before System.Text.Json was used as JSON deserialization, this class used Newtonsoft JSON and is now obsolete.
    /// </summary>
    [Obsolete("Use Unity.Cloud.Gltfast.GltfImport instead.")]
    [MovedFrom(true, sourceNamespace: "GLTFast.Newtonsoft", sourceAssembly: "glTFast.Newtonsoft")]
    public class GltfImport : Unity.Cloud.Gltfast.GltfImport { }
}
