#if UNITY_ANIMATION

using System;
using GLTFast.Schema;
using UnityEngine;

namespace GLTFast {

    public enum PointerType {
        Unknown = -1,
        Camera,
        Light,
        Material,
        Mesh,
        Node,
    }

    public class AnimationPointerData {
        public PointerType PointerType = PointerType.Unknown;
        public Type AnimationTargetType;
        public GltfAccessorAttributeType accessorType;
        public string Target;
        public int TargetId = -1;
        public string[] AnimationProperties;

        public AnimationPointerData(string pointerPath) {
            switch (pointerPath) {
                case string p when p.StartsWith("/cameras/"):
                    PointerType = PointerType.Camera;
                    AnimationTargetType = typeof(UnityEngine.Camera);
                    TargetId = ParseTargetId(pointerPath["/cameras/".Length..]);
                    Target = pointerPath[$"/cameras/{TargetId}/".Length..];
                    break;
                case string p when p.StartsWith("/extensions/KHR_lights_punctual/lights/"):
                    PointerType = PointerType.Light;
                    AnimationTargetType = typeof(UnityEngine.Light);
                    TargetId = ParseTargetId(pointerPath["/extensions/KHR_lights_punctual/lights/".Length..]);
                    Target = pointerPath[$"/extensions/KHR_lights_punctual/lights/{TargetId}/".Length..];
                    break;
                case string p when p.StartsWith("/materials/"):
                    PointerType = PointerType.Material;
                    AnimationTargetType = typeof(Renderer);
                    TargetId = ParseTargetId(pointerPath["/materials/".Length..]);
                    Target = pointerPath[$"/materials/{TargetId}/".Length..];
                    break;
                case string p when p.StartsWith("/meshes/"):
                    PointerType = PointerType.Mesh;
                    AnimationTargetType = typeof(UnityEngine.MeshRenderer);
                    TargetId = ParseTargetId(pointerPath["/meshes/".Length..]);
                    Target = pointerPath[$"/meshes/{TargetId}/".Length..];
                    break;
                case string p when p.StartsWith("/nodes/"):
                    PointerType = PointerType.Node;
                    if (pointerPath[^7..].Equals("weights")) {
                        AnimationTargetType = typeof(UnityEngine.MeshRenderer);
                    } else {
                        AnimationTargetType = typeof(Transform);
                    }
                    TargetId = ParseTargetId(pointerPath["/nodes/".Length..]);
                    Target = pointerPath[$"/nodes/{TargetId}/".Length..];
                    break;
            }

            bool success = SetTargetPropertiesAndAccessors(Target);
#if DEBUG
            if(!success) {
                Debug.LogWarning($"glTF animation pointer {pointerPath} is not supported.");
            }
#endif
        }

        public static int ParseTargetId(string name) {
            var split = name[..name.IndexOf("/")];
            if(int.TryParse(split, out var targetId)) {
                return targetId;
            }
            return -1;
        }

        public bool SetTargetPropertiesAndAccessors(string target) {
            string template;
            switch(target) {
                // Core
                case "rotation":
                    template = "localRotation.";
                    AnimationProperties = new[] {$"{template}x", $"{template}y", $"{template}z", $"{template}w"};
                    accessorType = GltfAccessorAttributeType.VEC4;
                    break;
                case "scale":
                    template = "localScale.";
                    AnimationProperties = new [] {$"{template}x", $"{template}y", $"{template}z"};
                    accessorType =GltfAccessorAttributeType.VEC3;
                    break;
                case "translation":
                    template = "localPosition.";
                    AnimationProperties = new [] {$"{template}x", $"{template}y", $"{template}z"};
                    accessorType =GltfAccessorAttributeType.VEC3;
                    break;
                case "pbrMetallicRoughness/baseColorFactor":
                    template = "material.baseColorFactor.";
                    AnimationProperties = new[] {$"{template}r", $"{template}g", $"{template}b", $"{template}a"};
                    accessorType = GltfAccessorAttributeType.VEC4;
                    break;
                case "pbrMetallicRoughness/metallicFactor":
                    AnimationProperties = new[] {"material.metallicFactor"};
                    accessorType = GltfAccessorAttributeType.SCALAR;
                    break;
                case "pbrMetallicRoughness/roughnessFactor":
                    AnimationProperties = new[] {"material.roughnessFactor"};
                    accessorType = GltfAccessorAttributeType.SCALAR;
                    break;
                case "alphaCutoff":
                    AnimationProperties = new[] {"material.alphaCutoff"};
                    accessorType = GltfAccessorAttributeType.SCALAR;
                    break;
                case "emissiveFactor":
                    template = "material.emissiveFactor.";
                    AnimationProperties = new[] {$"{template}r", $"{template}g", $"{template}b"};
                    accessorType = GltfAccessorAttributeType.VEC3;
                    break;
                case "normalTexture/scale":
                    AnimationProperties = new[] {"material.normalTexture_scale"};
                    accessorType = GltfAccessorAttributeType.SCALAR;
                    break;
                case "occlusionTexture/strength":
                    AnimationProperties = new[] {"material.occlusionTexture_strength"};
                    accessorType = GltfAccessorAttributeType.SCALAR;
                    break;

                // KHR_materials_transmission
                case "extensions/KHR_materials_transmission/transmissionFactor":
                    AnimationProperties = new[] {"material.transmissionFactor"};
                    accessorType = GltfAccessorAttributeType.SCALAR;
                    break;

                // KHR_texture_transform
                case "pbrMetallicRoughness/baseColorTexture/extensions/KHR_texture_transform/scale":
                    template = "material.baseColorTexture_ST.";
                    AnimationProperties = new[] {$"{template}x", $"{template}y"};
                    accessorType = GltfAccessorAttributeType.VEC2;
                    break;
                case "pbrMetallicRoughness/baseColorTexture/extensions/KHR_texture_transform/offset":
                    template = "material.baseColorTexture_ST.";
                    AnimationProperties = new[] {$"{template}z", $"{template}w"};
                    accessorType = GltfAccessorAttributeType.VEC2;
                    break;
                case "pbrMetallicRoughness/baseColorTexture/extensions/KHR_texture_transform/rotation":
                    AnimationProperties = new[] {"material.baseColorTexture_Rotation"};
                    accessorType = GltfAccessorAttributeType.SCALAR;
                    break;

                // KHR_texture_transform
                case "pbrMetallicRoughness/normalTexture/extensions/KHR_texture_transform/scale":
                    template = "material.normalTexture_ST.";
                    AnimationProperties = new[] {$"{template}x", $"{template}y"};
                    accessorType = GltfAccessorAttributeType.VEC2;
                    break;
                case "pbrMetallicRoughness/normalTexture/extensions/KHR_texture_transform/offset":
                    template = "material.normalTexture_ST.";
                    AnimationProperties = new[] {$"{template}z", $"{template}w"};
                    accessorType = GltfAccessorAttributeType.VEC2;
                    break;
                case "pbrMetallicRoughness/normalTexture/extensions/KHR_texture_transform/rotation":
                    AnimationProperties = new[] {"material.normalTexture_Rotation"};
                    accessorType = GltfAccessorAttributeType.SCALAR;
                    break;
                default:
                    return false;
            }
            return true;
        }
    }
}
#endif

