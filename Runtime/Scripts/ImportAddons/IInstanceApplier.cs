// SPDX-FileCopyrightText: 2026 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Unity.Cloud.Gltfast.Addons
{
    /// <summary>
    /// Marker interface for objects that apply data produced during the conversion phase to a
    /// single instantiated scene.
    /// </summary>
    /// <remarks>
    /// An applier is bound to one <see cref="IInstantiator"/> and is created by an
    /// <see cref="IInstanceApplierFactory"/>. Concrete capabilities are exposed by derived
    /// interfaces such as <see cref="IPostBeginSceneInstanceApplier"/>, which the import pipeline
    /// invokes at well-defined points of the instantiation lifecycle.
    /// </remarks>
    [MovedFrom(true, sourceNamespace: "GLTFast.Addons", sourceAssembly: "glTFast")]
    public interface IInstanceApplier { }
}
