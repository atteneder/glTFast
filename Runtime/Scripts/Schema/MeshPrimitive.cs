// Copyright 2020-2021 Andreas Atteneder
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

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace GLTFast.Schema {

    public enum DrawMode
    {
        Points = 0,
        Lines = 1,
        LineLoop = 2,
        LineStrip = 3,
        Triangles = 4,
        TriangleStrip = 5,
        TriangleFan = 6
    }

    [System.Serializable]
    public class MeshPrimitive {

        /// <summary>
        /// A dictionary object, where each key corresponds to mesh attribute semantic
        /// and each value is the index of the accessor containing attribute's data.
        /// </summary>
        public Attributes attributes;

        /// <summary>
        /// The index of the accessor that contains mesh indices.
        /// When this is not defined, the primitives should be rendered without indices
        /// using `drawArrays()`. When defined, the accessor must contain indices:
        /// the `bufferView` referenced by the accessor must have a `target` equal
        /// to 34963 (ELEMENT_ARRAY_BUFFER); a `byteStride` that is tightly packed,
        /// i.e., 0 or the byte size of `componentType` in bytes;
        /// `componentType` must be 5121 (UNSIGNED_BYTE), 5123 (UNSIGNED_SHORT)
        /// or 5125 (UNSIGNED_INT), the latter is only allowed
        /// when `OES_element_index_uint` extension is used; `type` must be `\"SCALAR\"`.
        /// </summary>
        public int indices = -1;

        /// <summary>
        /// The index of the material to apply to this primitive when rendering.
        /// </summary>
        public int material = -1;

        /// <summary>
        /// The type of primitives to render. All valid values correspond to WebGL enums.
        /// </summary>
        public DrawMode mode = DrawMode.Triangles;

        /// <summary>
        /// An array of Morph Targets, each  Morph Target is a dictionary mapping
        /// attributes (only "POSITION" and "NORMAL" supported) to their deviations
        /// in the Morph Target (index of the accessor containing the attribute
        /// displacements' data).
        /// </summary>
        /// TODO: Make dictionary key enums?
        //public List<Dictionary<string, AccessorId>> Targets;

        public MeshPrimitiveExtensions extensions;

#if DRACO_UNITY
        public bool isDracoCompressed {
            get {
                return extensions!=null && extensions.KHR_draco_mesh_compression != null;
            }
        }
#endif
    }

    [System.Serializable]
    public class Attributes : ReadOnlyDictionary<string, int> {
        private const string POSITION_KEY = "POSITION";
        private const string NORMAL_KEY = "NORMAL";
        private const string TANGENT_KEY = "TANGENT";
        private const string TEXCOORD_0_KEY = "TEXCOORD_0";
        private const string TEXCOORD_1_KEY = "TEXCOORD_1";
        private const string TEXCOORD_2_KEY = "TEXCOORD_2";
        private const string TEXCOORD_3_KEY = "TEXCOORD_3";
        private const string COLOR_0_KEY = "COLOR_0";
        private const string JOINTS_0_KEY = "JOINTS_0";
        private const string WEIGHTS_0_KEY = "WEIGHTS_0";
        
        public Attributes(IDictionary<string, int> dictionary) : base(dictionary)
        {
        }
        
        private int TryGetDefault(string key, int defaultVal)
        {
            int index;
            return TryGetValue(key, out index) ? index : defaultVal;
        }

        public int POSITION
        {
            get
            {
                return TryGetDefault(POSITION_KEY, -1);
            }
        }

        public int NORMAL
        {
            get
            {
                return TryGetDefault(NORMAL_KEY, -1);
            }
        }

        public int TEXCOORD_0
        {
            get
            {
                return TryGetDefault(TEXCOORD_0_KEY, -1);
            }
        }

        public int TEXCOORD_1
        {
            get
            {
                return TryGetDefault(TEXCOORD_1_KEY, -1);
            }
        }

        public int TEXCOORD_2
        {
            get
            {
                return TryGetDefault(TEXCOORD_2_KEY, -1);
            }
        }

        public int TEXCOORD_3
        {
            get
            {
                return TryGetDefault(TEXCOORD_3_KEY, -1);
            }
        }

        public int COLOR_0
        {
            get
            {
                return TryGetDefault(COLOR_0_KEY, -1);
            }
        }

        public int TANGENT
        {
            get
            {
                return TryGetDefault(TANGENT_KEY, -1);
            }
        }
        public int WEIGHTS_0
        {
            get
            {
                return TryGetDefault(WEIGHTS_0_KEY, -1);
            }
        }

        public int JOINTS_0
        {
            get
            {
                return TryGetDefault(JOINTS_0_KEY, -1);
            }
        }
    }

    [System.Serializable]
    public class MeshPrimitiveExtensions {
#if DRACO_UNITY
        public MeshPrimitiveDracoExtension KHR_draco_mesh_compression;
#endif
    }

#if DRACO_UNITY
    [System.Serializable]
    public class MeshPrimitiveDracoExtension {
        public int bufferView;
        public Attributes attributes;
    }
#endif
}
