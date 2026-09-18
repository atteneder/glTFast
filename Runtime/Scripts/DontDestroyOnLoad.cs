// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Unity.Cloud.Gltfast
{

    /// <summary>
    /// Makes GameObject survive scene changes
    /// </summary>
    /// <seealso cref="UnityEngine.Object.DontDestroyOnLoad"/>
    [MovedFrom(true, sourceNamespace: "GLTFast", sourceAssembly: "glTFast")]
    public class DontDestroyOnLoad : MonoBehaviour
    {
        void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}
