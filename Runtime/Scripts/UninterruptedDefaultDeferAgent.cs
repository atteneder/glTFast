// Copyright 2020-2022 Andreas Atteneder
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

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
