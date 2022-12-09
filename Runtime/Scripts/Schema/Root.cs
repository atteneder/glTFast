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

using System.IO;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("glTFast.Editor.Tests")]

namespace GLTFast.Schema
{

    /// <summary>
    /// The root object for a glTF asset.
    /// <seealso href="https://www.khronos.org/registry/glTF/specs/2.0/glTF-2.0.html#reference-gltf"/>
    /// </summary>
    [System.Serializable]
    public class Root
    {
        /// <summary>
        /// Names of glTF extensions used somewhere in this asset.
        /// </summary>
        public string[] extensionsUsed;

        /// <summary>
        /// Names of glTF extensions required to properly load this asset.
        /// </summary>
        public string[] extensionsRequired;

        /// <summary>
        /// An array of accessors. An accessor is a typed view into a bufferView.
        /// </summary>
        public Accessor[] accessors;

#if UNITY_ANIMATION
        /// <summary>
        /// An array of keyframe animations.
        /// </summary>
        public GltfAnimation[] animations;
#endif

        /// <summary>
        /// Metadata about the glTF asset.
        /// </summary>
        public Asset asset;

        /// <summary>
        /// An array of buffers. A buffer points to binary geometry, animation, or skins.
        /// </summary>
        public Buffer[] buffers;

        /// <summary>
        /// An array of bufferViews.
        /// A bufferView is a view into a buffer generally representing a subset of the buffer.
        /// </summary>
        public BufferView[] bufferViews;

        /// <summary>
        /// An array of cameras. A camera defines a projection matrix.
        /// </summary>
        public Camera[] cameras;

        /// <summary>
        /// An array of images. An image defines data used to create a texture.
        /// </summary>
        public Image[] images;

        /// <summary>
        /// An array of materials. A material defines the appearance of a primitive.
        /// </summary>
        public Material[] materials;

        /// <summary>
        /// An array of meshes. A mesh is a set of primitives to be rendered.
        /// </summary>
        public Mesh[] meshes;

        /// <summary>
        /// An array of nodes.
        /// </summary>
        public Node[] nodes;

        /// <summary>
        /// An array of samplers. A sampler contains properties for texture filtering and wrapping modes.
        /// </summary>
        public Sampler[] samplers;

        /// <summary>
        /// The index of the default scene.
        /// </summary>
        public int scene = -1;

        /// <summary>
        /// An array of scenes.
        /// </summary>
        public Scene[] scenes;

        /// <summary>
        /// An array of skins. A skin is defined by joints and matrices.
        /// </summary>
        public Skin[] skins;

        /// <summary>
        /// An array of textures.
        /// </summary>
        public Texture[] textures;

        /// <inheritdoc cref="RootExtension"/>
        public RootExtension extensions;

#if UNITY_ANIMATION
        public bool HasAnimation => animations != null && animations.Length > 0;
#endif // UNITY_ANIMATION

        /// <summary>
        /// Looks up if a certain accessor points to interleaved data.
        /// </summary>
        /// <param name="accessorIndex">Accessor index</param>
        /// <returns>True if accessor is interleaved, false if its data is
        /// continuous.</returns>
        public bool IsAccessorInterleaved(int accessorIndex)
        {
            var accessor = accessors[accessorIndex];
            var bufferView = bufferViews[accessor.bufferView];
            if (bufferView.byteStride < 0) return false;
            var elementSize = Accessor.GetAccessorAttributeTypeLength(accessor.GetAttributeType()) * Accessor.GetComponentTypeSize(accessor.componentType);
            return bufferView.byteStride > elementSize;
        }

        /// <summary>
        /// Serialization to JSON
        /// </summary>
        /// <param name="stream">Stream the JSON string is being written to.</param>
        public void GltfSerialize(StreamWriter stream)
        {
            var writer = new JsonWriter(stream);

            if (asset != null)
            {
                writer.AddProperty("asset");
                asset.GltfSerialize(writer);
            }
            if (nodes != null)
            {
                writer.AddArray("nodes");
                foreach (var node in nodes)
                {
                    node.GltfSerialize(writer);
                }
                writer.CloseArray();
            }

            if (extensionsRequired != null)
            {
                writer.AddArrayProperty("extensionsRequired", extensionsRequired);
            }

            if (extensionsUsed != null)
            {
                writer.AddArrayProperty("extensionsUsed", extensionsUsed);
            }

#if UNITY_ANIMATION
            if (animations!=null) {
                writer.AddArray("animations");
                foreach( var animation in animations) {
                    animation.GltfSerialize(writer);
                }
                writer.CloseArray();
            }
#endif

            if (buffers != null)
            {
                writer.AddArray("buffers");
                foreach (var buffer in buffers)
                {
                    buffer.GltfSerialize(writer);
                }
                writer.CloseArray();
            }

            if (bufferViews != null)
            {
                writer.AddArray("bufferViews");
                foreach (var bufferView in bufferViews)
                {
                    bufferView.GltfSerialize(writer);
                }
                writer.CloseArray();
            }

            if (accessors != null)
            {
                writer.AddArray("accessors");
                foreach (var accessor in accessors)
                {
                    accessor.GltfSerialize(writer);
                }
                writer.CloseArray();
            }

            if (cameras != null)
            {
                writer.AddArray("cameras");
                foreach (var camera in cameras)
                {
                    camera.GltfSerialize(writer);
                }
                writer.CloseArray();
            }

            if (images != null)
            {
                writer.AddArray("images");
                foreach (var image in images)
                {
                    image?.GltfSerialize(writer);
                }
                writer.CloseArray();
            }
            if (materials != null)
            {
                writer.AddArray("materials");
                foreach (var material in materials)
                {
                    material.GltfSerialize(writer);
                }
                writer.CloseArray();
            }
            if (meshes != null)
            {
                writer.AddArray("meshes");
                foreach (var mesh in meshes)
                {
                    mesh.GltfSerialize(writer);
                }
                writer.CloseArray();
            }
            if (samplers != null)
            {
                writer.AddArray("samplers");
                foreach (var sampler in samplers)
                {
                    sampler.GltfSerialize(writer);
                }
                writer.CloseArray();
            }
            if (scene >= 0)
            {
                writer.AddProperty("scene", scene);
            }
            if (scenes != null)
            {
                writer.AddArray("scenes");
                foreach (var sceneToSerialize in scenes)
                {
                    sceneToSerialize.GltfSerialize(writer);
                }
                writer.CloseArray();
            }
            if (skins != null)
            {
                writer.AddArray("skins");
                foreach (var skin in skins)
                {
                    skin.GltfSerialize(writer);
                }
                writer.CloseArray();
            }
            if (textures != null)
            {
                writer.AddArray("textures");
                foreach (var texture in textures)
                {
                    texture.GltfSerialize(writer);
                }
                writer.CloseArray();
            }

            if (extensions != null)
            {
                writer.AddProperty("extensions");
                extensions.GltfSerialize(writer);
            }

            writer.Close();
        }
    }
}
