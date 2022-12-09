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
    /// Defines how node names are created
    /// </summary>
    public enum NameImportMethod
    {
        /// <summary>
        /// Use original node names.
        /// Fallback to mesh's name (if present)
        /// Fallback to "Node_&lt;index&gt;" as last resort.
        /// </summary>
        Original,
        /// <summary>
        /// Identical to <see cref="Original">Original</see>, but
        /// names are made unique (within their hierarchical position)
        /// by supplementing a continuous number.
        /// This is required for correct animation target lookup and import continuity.
        /// </summary>
        OriginalUnique
    }
}
