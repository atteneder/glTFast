#if UNITY_ANIMATION

using System;
using GLTFast.Schema;
using UnityEngine;

namespace GLTFast {
    
    public enum TargetType {
        Unknown = -1,
        Camera,
        Light,
        Material,
        Mesh,
        Node,
    }

    public class AnimationData {
        public TargetType TargetType = TargetType.Unknown;
        public Type AnimationClipType;
        public GltfAccessorAttributeType AccessorType;
        public string TargetProperty;
        public int TargetId = -1;
        public string[] PropertyNames;

        public static AnimationData TranslationData() {
            var template = "localPosition.";
            return new AnimationData
            {
                TargetType = TargetType.Node,
                AnimationClipType = typeof(Transform),
                AccessorType = GltfAccessorAttributeType.VEC3,
                TargetProperty = "translationNative",
                PropertyNames = new [] {$"{template}x", $"{template}y", $"{template}z"}
            };
        }

        public static AnimationData RotationData() {
            var template = "localRotation.";
            return new AnimationData
            {
                TargetType = TargetType.Node,
                AnimationClipType = typeof(Transform),
                AccessorType = GltfAccessorAttributeType.VEC4,
                TargetProperty = "rotationNative",
                PropertyNames = new [] {$"{template}x", $"{template}y", $"{template}z", $"{template}w"}
            };
        }

        public static AnimationData ScaleData() {
            var template = "localScale.";
            return new AnimationData
            {
                TargetType = TargetType.Node,
                AnimationClipType = typeof(Transform),
                AccessorType = GltfAccessorAttributeType.VEC3,
                TargetProperty = "scaleNative",
                PropertyNames = new [] {$"{template}x", $"{template}y", $"{template}z"}
            };
        }

        public static AnimationData WeightData() {
            return new AnimationData
            {
                TargetType = TargetType.Node,
                AnimationClipType = typeof(MeshRenderer),
                AccessorType = GltfAccessorAttributeType.SCALAR,
                TargetProperty = "weights"
            };            
        }

        public static AnimationData GeneratePointerData(string pointerPath) {
            var data = new AnimationData();

            switch (pointerPath) {
                case string p when p.StartsWith("/cameras/"):
                    data.TargetType = TargetType.Camera;
                    data.AnimationClipType = typeof(AnimationCameraGameObject);
                    data.TargetId = ParsePointerTargetId(pointerPath["/cameras/".Length..]);
                    data.TargetProperty = pointerPath[$"/cameras/{data.TargetId}/".Length..];
                    break;
                case string p when p.StartsWith("/extensions/KHR_lights_punctual/lights/"):
                    data.TargetType = TargetType.Light;
                    data.AnimationClipType = typeof(UnityEngine.Light);
                    data.TargetId = ParsePointerTargetId(pointerPath["/extensions/KHR_lights_punctual/lights/".Length..]);
                    data.TargetProperty = pointerPath[$"/extensions/KHR_lights_punctual/lights/{data.TargetId}/".Length..];
                    break;
                case string p when p.StartsWith("/materials/"):
                    data.TargetType = TargetType.Material;
                    data.AnimationClipType = typeof(Renderer);
                    data.TargetId = ParsePointerTargetId(pointerPath["/materials/".Length..]);
                    data.TargetProperty = pointerPath[$"/materials/{data.TargetId}/".Length..];
                    break;
                case string p when p.StartsWith("/meshes/"):
                    data.TargetType = TargetType.Mesh;
                    data.AnimationClipType = typeof(UnityEngine.MeshRenderer);
                    data.TargetId = ParsePointerTargetId(pointerPath["/meshes/".Length..]);
                    data.TargetProperty = pointerPath[$"/meshes/{data.TargetId}/".Length..];
                    break;
                case string p when p.StartsWith("/nodes/"):
                    data.TargetType = TargetType.Node;
                    if (pointerPath[^7..].Equals("weights")) {
                        data.AnimationClipType = typeof(UnityEngine.MeshRenderer);
                    } else {
                        data.AnimationClipType = typeof(Transform);
                    }
                    data.TargetId = ParsePointerTargetId(pointerPath["/nodes/".Length..]);
                    data.TargetProperty = pointerPath[$"/nodes/{data.TargetId}/".Length..];
                    break;
            }

            string template;
            switch(data.TargetProperty) {
                // Core
                case "rotation":
                    template = "localRotation.";
                    data.PropertyNames = new[] {$"{template}x", $"{template}y", $"{template}z", $"{template}w"};
                    data.AccessorType = GltfAccessorAttributeType.VEC4;
                    break;
                case "scale":
                    template = "localScale.";
                    data.PropertyNames = new [] {$"{template}x", $"{template}y", $"{template}z"};
                    data.AccessorType = GltfAccessorAttributeType.VEC3;
                    break;
                case "translation":
                    template = "localPosition.";
                    data.PropertyNames = new [] {$"{template}x", $"{template}y", $"{template}z"};
                    data.AccessorType = GltfAccessorAttributeType.VEC3;
                    break;
                case "weights":
                    data.AccessorType = GltfAccessorAttributeType.SCALAR;
                    break;
                case "orthographic/xmag":
                    data.PropertyNames = new [] {"xMag"};
                    data.AccessorType = GltfAccessorAttributeType.SCALAR;
                    break;
                case "orthographic/ymag":
                    data.PropertyNames = new [] {"yMag"};
                    data.AccessorType = GltfAccessorAttributeType.SCALAR;
                    break;
                case "orthographic/zfar":
                    data.PropertyNames = new [] {"farClipPlane"};
                    data.AccessorType = GltfAccessorAttributeType.SCALAR;
                    break;
                case "orthographic/znear":
                    data.PropertyNames = new [] {"nearClipPlane"};
                    data.AccessorType = GltfAccessorAttributeType.SCALAR;
                    break;
                case "perspective/yfov":
                    data.PropertyNames = new [] {"fov"};
                    data.AccessorType = GltfAccessorAttributeType.SCALAR;
                    break;
                case "perspective/zfar":
                    data.PropertyNames = new [] {"farClipPlane"};
                    data.AccessorType = GltfAccessorAttributeType.SCALAR;
                    break;
                case "perspective/znear":
                    data.PropertyNames = new [] {"nearClipPlane"};
                    data.AccessorType = GltfAccessorAttributeType.SCALAR;
                    break;
                case "pbrMetallicRoughness/baseColorFactor":
                    template = "material.baseColorFactor.";
                    data.PropertyNames = new[] {$"{template}r", $"{template}g", $"{template}b", $"{template}a"};
                    data.AccessorType = GltfAccessorAttributeType.VEC4;
                    break;
                case "pbrMetallicRoughness/metallicFactor":
                    data.PropertyNames = new[] {"material.metallicFactor"};
                    data.AccessorType = GltfAccessorAttributeType.SCALAR;
                    break;
                case "pbrMetallicRoughness/roughnessFactor":
                    data.PropertyNames = new[] {"material.roughnessFactor"};
                    data.AccessorType = GltfAccessorAttributeType.SCALAR;
                    break;
                case "alphaCutoff":
                    data.PropertyNames = new[] {"material.alphaCutoff"};
                    data.AccessorType = GltfAccessorAttributeType.SCALAR;
                    break;
                case "emissiveFactor":
                    template = "material.emissiveFactor.";
                    data.PropertyNames = new[] {$"{template}r", $"{template}g", $"{template}b"};
                    data.AccessorType = GltfAccessorAttributeType.VEC3;
                    break;
                case "normalTexture/scale":
                    data.PropertyNames = new[] {"material.normalTexture_scale"};
                    data.AccessorType = GltfAccessorAttributeType.SCALAR;
                    break;
                case "occlusionTexture/strength":
                    data.PropertyNames = new[] {"material.occlusionTexture_strength"};
                    data.AccessorType = GltfAccessorAttributeType.SCALAR;
                    break;

                // KHR_materials_transmission
                case "extensions/KHR_materials_transmission/transmissionFactor":
                    data.PropertyNames = new[] {"material.transmissionFactor"};
                    data.AccessorType = GltfAccessorAttributeType.SCALAR;
                    break;

                // KHR_texture_transform
                case "pbrMetallicRoughness/baseColorTexture/extensions/KHR_texture_transform/scale":
                    template = "material.baseColorTexture_ST.";
                    data.PropertyNames = new[] {$"{template}x", $"{template}y"};
                    data.AccessorType = GltfAccessorAttributeType.VEC2;
                    break;
                case "pbrMetallicRoughness/baseColorTexture/extensions/KHR_texture_transform/offset":
                    template = "material.baseColorTexture_ST.";
                    data.PropertyNames = new[] {$"{template}z", $"{template}w"};
                    data.AccessorType = GltfAccessorAttributeType.VEC2;
                    break;
                case "pbrMetallicRoughness/baseColorTexture/extensions/KHR_texture_transform/rotation":
                    data.PropertyNames = new[] {"material.baseColorTexture_Rotation"};
                    data.AccessorType = GltfAccessorAttributeType.SCALAR;
                    break;
                case "pbrMetallicRoughness/normalTexture/extensions/KHR_texture_transform/scale":
                    template = "material.normalTexture_ST.";
                    data.PropertyNames = new[] {$"{template}x", $"{template}y"};
                    data.AccessorType = GltfAccessorAttributeType.VEC2;
                    break;
                case "pbrMetallicRoughness/normalTexture/extensions/KHR_texture_transform/offset":
                    template = "material.normalTexture_ST.";
                    data.PropertyNames = new[] {$"{template}z", $"{template}w"};
                    data.AccessorType = GltfAccessorAttributeType.VEC2;
                    break;
                case "pbrMetallicRoughness/normalTexture/extensions/KHR_texture_transform/rotation":
                    data.PropertyNames = new[] {"material.normalTexture_Rotation"};
                    data.AccessorType = GltfAccessorAttributeType.SCALAR;
                    break;
                default:
#if DEBUG
                    Debug.LogWarning($"glTF animation pointer {pointerPath} is not supported.");
#endif
                    break;
            }

            return data;
        }

        public static int ParsePointerTargetId(string name) {
            var split = name[..name.IndexOf("/")];
            if(int.TryParse(split, out var targetId)) {
                return targetId;
            }
            return -1;
        }
    }
}
#endif

