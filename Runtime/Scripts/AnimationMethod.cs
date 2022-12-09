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
