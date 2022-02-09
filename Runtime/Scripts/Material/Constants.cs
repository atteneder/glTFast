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

namespace GLTFast.Materials {
    
    /// <summary>
    /// Contains material related constant variables that are required for both
    /// import (glTF to Unity) and export (Unity to glTF) material conversions.
    /// TODO: Make const var location consistent
    /// </summary>
    public static class Constants {
        
        /// <summary>
        /// Shader keyword for normal mapping
        /// </summary>
        public const string kwNormalMap = "_NORMALMAP";
    }
}
