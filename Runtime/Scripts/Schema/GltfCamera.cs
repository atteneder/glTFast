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
    
    /// <summary>
    /// A camera’s projection
    /// </summary>
    [Serializable]
    public class Camera : NamedObject {

        /// <summary>
        /// Camera projection type
        /// </summary>
        public enum Type {
            /// <summary>
            /// Orthogonal projection
            /// </summary>
            Orthographic,
            /// <summary>
            ///  Perspective projection
            /// </summary>
            Perspective
        }
        
        [SerializeField]
        string type;

        Type? _typeEnum;
        
        /// <summary>
        /// <see cref="Type"/> typed view onto <see cref="type"/> string. 
        /// </summary>
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
            set {
                type = null;
                _typeEnum = value;
            }
        }
        
        /// <inheritdoc cref="CameraOrthographic"/>
        public CameraOrthographic orthographic;
        
        /// <inheritdoc cref="CameraOrthographic"/>
        public CameraPerspective perspective;
        
        internal void GltfSerialize(JsonWriter writer) {
            writer.AddObject();
            GltfSerializeRoot(writer);
            writer.AddProperty("type", _typeEnum.ToString().ToLower());
            if(perspective!=null) {
                writer.AddProperty("perspective");
                perspective.GltfSerialize(writer);
            }
            if(orthographic!=null) {
                writer.AddProperty("orthographic");
                orthographic.GltfSerialize(writer);
            }
            writer.Close();
        }
    }

    /// <summary>
    /// An orthographic camera containing properties to create an orthographic projection matrix.
    /// </summary>
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
        
        internal void GltfSerialize(JsonWriter writer) {
            writer.AddObject();
            writer.AddProperty("xmag", xmag);
            writer.AddProperty("ymag", ymag);
            writer.AddProperty("zfar", zfar);
            writer.AddProperty("znear", znear);
            writer.Close();
        }
    }
    
    /// <summary>
    /// A perspective camera containing properties to create a perspective projection matrix.
    /// </summary>
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
        
        internal void GltfSerialize(JsonWriter writer) {
            writer.AddObject();
            if (aspectRatio > 0) {
                writer.AddProperty("aspectRatio", aspectRatio);
            }
            writer.AddProperty("yfov", yfov);
            if (zfar < float.MaxValue) {
                writer.AddProperty("zfar", zfar);
            }
            writer.AddProperty("znear", znear);
            writer.Close();
        }
    }
}
