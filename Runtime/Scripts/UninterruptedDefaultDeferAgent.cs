// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using UnityEngine;

namespace GLTFast
{

    /// <summary>
    /// Will (un)register an UninterruptedDeferAgent as GltfImport's default when it's enabled or disabled.
    /// </summary>
    [DefaultExecutionOrder(-1)]
    class UninterruptedDefaultDeferAgent : MonoBehaviour
    {

        UninterruptedDeferAgent m_DeferAgent;

        void OnEnable()
        {
            m_DeferAgent = new UninterruptedDeferAgent();
            GltfImport.SetDefaultDeferAgent(m_DeferAgent);
        }

        void OnDisable()
        {
            GltfImport.UnsetDefaultDeferAgent(m_DeferAgent);
            m_DeferAgent = null;
        }
    }
}
