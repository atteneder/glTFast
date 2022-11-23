﻿// Copyright 2020-2022 Andreas Atteneder
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
using UnityEngine.Serialization;

namespace GLTFast.Editor {

    /// <summary>
    /// Editor Import specific settings (not relevant at runtime)
    /// </summary>
    [Serializable]
    class EditorImportSettings {
        
        /// <summary>
        /// Creates a secondary UV set on all meshes, if there is none present already.
        /// Often used for lightmaps. 
        /// </summary>
        [Tooltip("Generate Lightmap UVs")]
        public bool generateSecondaryUVSet;
    }
}