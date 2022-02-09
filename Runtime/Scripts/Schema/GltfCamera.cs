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

namespace GLTFast.Schema {
    
    [Serializable]
    public class Camera : RootChild {

        public enum Type {
            Orthographic,
            Perspective
        }
        
        [SerializeField]
        string type;

        Type? _typeEnum;
        
        public Type typeEnum {
            get {
                if (_typeEnum.HasValue) {
                    return _typeEnum.Value;
                }
                if (!string.IsNullOrEmpty (type)) {
                    _typeEnum = (Type)System.Enum.Parse (typeof(Type), type, true);
                    type = null;
                    return _typeEnum.Value;
                }
                if (orthographic != null) _typeEnum = Type.Orthographic;
                if (perspective != null) _typeEnum = Type.Perspective;
                return _typeEnum.Value;
            }
        }
        
        public CameraOrthographic orthographic;
        public CameraPerspective perspective;
        
        internal void GltfSerialize(JsonWriter writer) {
            writer.AddObject();
            GltfSerializeRoot(writer);
            writer.Close();
            throw new System.NotImplementedException($"GltfSerialize missing on {GetType()}");
        }
    }

    [Serializable]
    public class CameraOrthographic {
        
        /// <summary>
        /// The floating-point horizontal magnification of the view. Must not be zero.
        /// /// </summary>
        public float xmag;
        
        /// <summary>
        /// The floating-point vertical magnification of the view. Must not be zero.
        /// </summary>
        public float ymag;
        
        /// <summary>
        /// The floating-point distance to the far clipping plane. zfar must be greater than znear.
        /// /// </summary>
        public float zfar;
        
        /// <summary>
        /// The floating-point distance to the near clipping plane.
        /// </summary>
        public float znear;
    }
    
    [Serializable]
    public class CameraPerspective {
        
        /// <summary>
        /// The floating-point aspect ratio of the field of view.
        /// </summary>
        public float aspectRatio = -1;
        
        /// <summary>
        /// The floating-point vertical field of view in radians.
        /// </summary>
        public float yfov;
        
        /// <summary>
        /// The floating-point distance to the far clipping plane.
        /// </summary>
        public float zfar = -1f;
        
        /// <summary>
        /// The floating-point distance to the near clipping plane.
        /// </summary>
        public float znear;
    }
}
