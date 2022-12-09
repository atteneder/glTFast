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
    /// Scene object creation method. Determines whether or when a
    /// GameObject/Entity representing the scene should get created.
    /// </summary>
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
