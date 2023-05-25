// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using UnityEngine;

namespace GLTFast
{

    /// <summary>
    /// To be added to a GameObject along a default <see cref="IDeferAgent"/>.
    /// Will (un)register it as GltfImport's default when it's enabled or disabled.
    /// </summary>
    [RequireComponent(typeof(IDeferAgent))]
    [DefaultExecutionOrder(-1)]
    class DefaultDeferAgent : MonoBehaviour
    {

        void OnEnable()
        {
            var deferAgent = GetComponent<IDeferAgent>();
            if (deferAgent != null)
            {
                GltfImport.SetDefaultDeferAgent(deferAgent);
            }
        }

        void OnDisable()
        {
            var deferAgent = GetComponent<IDeferAgent>();
            if (deferAgent != null)
            {
                GltfImport.UnsetDefaultDeferAgent(deferAgent);
            }
        }
    }
}
