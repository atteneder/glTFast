// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine.Assertions;
using UnityEngine.Profiling;

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
            return bufferView.byteStride > accessor.ElementByteSize;
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

        /// <summary>
        /// Detects if a secondary null-check is necessary.
        /// </summary>
        /// <returns>True if a secondary parse against the FakeSchema is required. False otherwise</returns>
        internal bool JsonUtilitySecondParseRequired()
        {
            Profiler.BeginSample("JsonUtilitySecondParseRequired");
            var check = false;
            if (materials != null)
            {
                foreach (var mat in materials)
                {
                    // mat.extension is always set (not null), because JsonUtility constructs a default
                    // if any of mat.extension's members is not null, it is because there was
                    // a legit extensions node in JSON => we have to check which ones
                    if (mat.extensions.KHR_materials_unlit != null)
                    {
                        check = true;
                    }
                    else
                    {
                        // otherwise dump the wrongfully constructed MaterialExtension
                        mat.extensions = null;
                    }
                }
            }
            if (accessors != null)
            {
                foreach (var accessor in accessors)
                {
                    if (accessor.sparse.indices == null || accessor.sparse.values == null)
                    {
                        // If indices and values members are null, `sparse` is likely
                        // an auto-instance by the JsonUtility and not present in JSON.
                        // Therefore we remove it:
                        accessor.sparse = null;
                    }
#if GLTFAST_SAFE
                    else {
                        // This is very likely a valid sparse accessor.
                        // However, an empty sparse property ( "sparse": {} ) would break
                        // glTFast, so better do a thorough follow-up check
                        check = true;
                    }
#endif // GLTFAST_SAFE
                }
            }
#if DRACO_UNITY
            if(!check && meshes!=null) {
                foreach (var mesh in meshes) {
                    if (mesh.primitives != null) {
                        foreach (var primitive in mesh.primitives) {
                            if (primitive.extensions?.KHR_draco_mesh_compression != null) {
                                check = true;
                                break;
                            }
                        }
                    }
                }
            }
#endif
            Profiler.EndSample();
            return check;
        }

        internal void JsonUtilityCleanupAgainstSecondParse(FakeSchema.Root fakeRoot)
        {
            Profiler.BeginSample("JsonUtilityCleanup");

            if (materials != null)
            {
                for (var i = 0; i < materials.Length; i++)
                {
                    var mat = materials[i];
                    if (mat.extensions == null) continue;
                    Assert.AreEqual(mat.name, fakeRoot.materials[i].name);
                    var fake = fakeRoot.materials[i].extensions;
                    if (fake.KHR_materials_unlit == null)
                    {
                        mat.extensions.KHR_materials_unlit = null;
                    }

                    if (fake.KHR_materials_pbrSpecularGlossiness == null)
                    {
                        mat.extensions.KHR_materials_pbrSpecularGlossiness = null;
                    }

                    if (fake.KHR_materials_transmission == null)
                    {
                        mat.extensions.KHR_materials_transmission = null;
                    }

                    if (fake.KHR_materials_clearcoat == null)
                    {
                        mat.extensions.KHR_materials_clearcoat = null;
                    }

                    if (fake.KHR_materials_sheen == null)
                    {
                        mat.extensions.KHR_materials_sheen = null;
                    }
                }
            }

#if GLTFAST_SAFE
            if (accessors != null) {
                for (var i = 0; i < accessors.Length; i++) {
                    var sparse = fakeRoot.accessors[i].sparse;
                    if (sparse?.indices == null || sparse.values == null) {
                        accessors[i].sparse = null;
                    }
                }
            }
#endif

#if DRACO_UNITY
            if (meshes != null) {
                for (var i = 0; i < meshes.Length; i++) {
                    var mesh = meshes[i];
                    Assert.AreEqual(mesh.name, fakeRoot.meshes[i].name);
                    for (var j = 0; j < mesh.primitives.Length; j++) {
                        var primitive = mesh.primitives[j];
                        if (primitive.extensions == null ) continue;
                        var fake = fakeRoot.meshes[i].primitives[j];
                        if (fake.extensions.KHR_draco_mesh_compression == null) {
                            // TODO: Differentiate Primitive extensions here
                            // since Draco is the only primitive extension, we
                            // remove the whole extensions property.
                            // primitive.extensions.KHR_draco_mesh_compression = null;
                            primitive.extensions = null;
                        }
                    }
                }
            }
#endif
            Profiler.EndSample();
        }

        /// <summary>
        /// Generic checks and cleanups
        /// </summary>
        public virtual void JsonUtilityCleanup()
        {
            if (nodes != null)
            {
                foreach (var t in nodes)
                {
                    var e = t.extensions;
                    if (e != null)
                    {
                        // Check if GPU instancing extension is valid
                        if (e.EXT_mesh_gpu_instancing?.attributes == null)
                        {
                            e.EXT_mesh_gpu_instancing = null;
                        }
                        // Check if Lights extension is valid
                        if ((e.KHR_lights_punctual?.light ?? -1) < 0)
                        {
                            e.KHR_lights_punctual = null;
                        }
                        // Unset `extension` if none of them was valid
                        if (e.EXT_mesh_gpu_instancing == null &&
                            e.KHR_lights_punctual == null)
                        {
                            t.extensions = null;
                        }
                    }
                }
            }

            if (extensions != null && !extensions.JsonUtilityCleanup())
            {
                extensions = null;
            }
        }
    }
}
