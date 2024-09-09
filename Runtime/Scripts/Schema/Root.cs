// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine.Assertions;
using UnityEngine.Profiling;

[assembly: InternalsVisibleTo("glTFast.Editor.Tests")]

namespace GLTFast.Schema
{

    /// <inheritdoc />
    [Serializable]
    public class Root : RootBase<
        Accessor,
        Animation,
        Asset,
        Buffer,
        BufferView,
        Camera,
        RootExtensions,
        Image,
        Material,
        Mesh,
        Node,
        Sampler,
        Scene,
        Skin,
        Texture
    >
    { }

    /// <inheritdoc />
    /// <typeparam name="TAccessor">Accessor type</typeparam>
    /// <typeparam name="TAnimation">Animation type</typeparam>
    /// <typeparam name="TAsset">Asset type</typeparam>
    /// <typeparam name="TBuffer">Buffer type</typeparam>
    /// <typeparam name="TBufferView">BufferView type</typeparam>
    /// <typeparam name="TCamera">Camera type</typeparam>
    /// <typeparam name="TExtensions">Extensions type</typeparam>
    /// <typeparam name="TImage">Image type</typeparam>
    /// <typeparam name="TMaterial">Material type</typeparam>
    /// <typeparam name="TMesh">Mesh type</typeparam>
    /// <typeparam name="TNode">Node type</typeparam>
    /// <typeparam name="TSampler">Sampler type</typeparam>
    /// <typeparam name="TScene">Scene type</typeparam>
    /// <typeparam name="TSkin">Skin type</typeparam>
    /// <typeparam name="TTexture">Texture type</typeparam>
    [Serializable]
    public abstract class RootBase<
        TAccessor,
        TAnimation,
        TAsset,
        TBuffer,
        TBufferView,
        TCamera,
        TExtensions,
        TImage,
        TMaterial,
        TMesh,
        TNode,
        TSampler,
        TScene,
        TSkin,
        TTexture
    > : RootBase
        where TAccessor : AccessorBase
        where TAnimation : AnimationBase
        where TAsset : Asset
        where TBuffer : Buffer
        where TBufferView : BufferViewBase
        where TCamera : CameraBase
        where TExtensions : RootExtensions
        where TImage : Image
        where TMaterial : MaterialBase
        where TMesh : MeshBase
        where TNode : NodeBase
        where TSampler : Sampler
        where TScene : Scene
        where TSkin : Skin
        where TTexture : TextureBase
    {
        /// <inheritdoc cref="Accessors"/>
        public TAccessor[] accessors;

#if UNITY_ANIMATION
        /// <inheritdoc cref="Animations"/>
        public TAnimation[] animations;
#endif

        /// <inheritdoc cref="Asset"/>
        public TAsset asset;

        /// <inheritdoc cref="Buffer"/>
        public TBuffer[] buffers;

        /// <inheritdoc cref="BufferView"/>
        public TBufferView[] bufferViews;

        /// <inheritdoc cref="Camera"/>
        public TCamera[] cameras;

        /// <inheritdoc cref="Image"/>
        public TImage[] images;

        /// <inheritdoc cref="Material"/>
        public TMaterial[] materials;

        /// <inheritdoc cref="Node"/>
        public TNode[] nodes;

        /// <inheritdoc cref="Sampler"/>
        public TSampler[] samplers;

        /// <inheritdoc cref="Scene"/>
        public TScene[] scenes;

        /// <inheritdoc cref="Skin"/>
        public TSkin[] skins;

        /// <inheritdoc cref="Texture"/>
        public TTexture[] textures;

        /// <inheritdoc cref="RootExtensions"/>
        public TExtensions extensions;

        /// <inheritdoc cref="Meshes"/>
        public TMesh[] meshes;

        /// <inheritdoc />
        public override IReadOnlyList<AccessorBase> Accessors => accessors;

#if UNITY_ANIMATION
        /// <inheritdoc />
        public override IReadOnlyList<AnimationBase> Animations => animations;
#endif

        /// <inheritdoc />
        public override Asset Asset => asset;

        /// <inheritdoc />
        public override IReadOnlyList<Buffer> Buffers => buffers;

        /// <inheritdoc />
        public override IReadOnlyList<BufferViewBase> BufferViews => bufferViews;

        /// <inheritdoc />
        public override IReadOnlyList<CameraBase> Cameras => cameras;

        /// <inheritdoc />
        public override IReadOnlyList<Image> Images => images;

        /// <inheritdoc />
        public override IReadOnlyList<MaterialBase> Materials => materials;

        /// <inheritdoc />
        public override IReadOnlyList<NodeBase> Nodes => nodes;

        /// <inheritdoc />
        public override IReadOnlyList<Sampler> Samplers => samplers;

        /// <inheritdoc />
        public override IReadOnlyList<Scene> Scenes => scenes;

        /// <inheritdoc />
        public override IReadOnlyList<Skin> Skins => skins;

        /// <inheritdoc />
        public override IReadOnlyList<TextureBase> Textures => textures;

        /// <inheritdoc />
        public override RootExtensions Extensions => extensions;

        /// <inheritdoc />
        internal override void UnsetExtensions()
        {
            extensions = null;
        }

        /// <inheritdoc />
        public override IReadOnlyList<MeshBase> Meshes => meshes;
    }

    /// <summary>
    /// The root object for a glTF asset.
    /// </summary>
    /// <seealso href="https://www.khronos.org/registry/glTF/specs/2.0/glTF-2.0.html#reference-gltf"/>
    [Serializable]
    public abstract class RootBase
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
        public abstract IReadOnlyList<AccessorBase> Accessors { get; }

#if UNITY_ANIMATION
        /// <summary>
        /// An array of keyframe animations.
        /// </summary>
        public abstract IReadOnlyList<AnimationBase> Animations { get; }
#endif

        /// <summary>
        /// Metadata about the glTF asset.
        /// </summary>
        public abstract Asset Asset { get; }

        /// <summary>
        /// An array of buffers. A buffer points to binary geometry, animation, or skins.
        /// </summary>
        public abstract IReadOnlyList<Buffer> Buffers { get; }

        /// <summary>
        /// An array of bufferViews.
        /// A bufferView is a view into a buffer generally representing a subset of the buffer.
        /// </summary>
        public abstract IReadOnlyList<BufferViewBase> BufferViews { get; }

        /// <summary>
        /// An array of cameras. A camera defines a projection matrix.
        /// </summary>
        public abstract IReadOnlyList<CameraBase> Cameras { get; }

        /// <summary>
        /// An array of images. An image defines data used to create a texture.
        /// </summary>
        public abstract IReadOnlyList<Image> Images { get; }

        /// <summary>
        /// An array of materials. A material defines the appearance of a primitive.
        /// </summary>
        public abstract IReadOnlyList<MaterialBase> Materials { get; }

        /// <summary>
        /// An array of meshes. A mesh is a set of primitives to be rendered.
        /// </summary>
        public abstract IReadOnlyList<MeshBase> Meshes { get; }

        /// <summary>
        /// An array of nodes.
        /// </summary>
        public abstract IReadOnlyList<NodeBase> Nodes { get; }

        /// <summary>
        /// An array of samplers. A sampler contains properties for texture filtering and wrapping modes.
        /// </summary>
        public abstract IReadOnlyList<Sampler> Samplers { get; }

        /// <summary>
        /// The index of the default scene.
        /// </summary>
        public int scene = -1;

        /// <summary>
        /// An array of scenes.
        /// </summary>
        public abstract IReadOnlyList<Scene> Scenes { get; }

        /// <summary>
        /// An array of skins. A skin is defined by joints and matrices.
        /// </summary>
        public abstract IReadOnlyList<Skin> Skins { get; }

        /// <summary>
        /// An array of textures.
        /// </summary>
        public abstract IReadOnlyList<TextureBase> Textures { get; }

        /// <inheritdoc cref="RootExtensions"/>
        public abstract RootExtensions Extensions { get; }

        /// <summary>
        /// Sets <see cref="Extensions"/> to null.
        /// </summary>
        internal abstract void UnsetExtensions();

#if UNITY_ANIMATION
        public bool HasAnimation => Animations != null && Animations.Count > 0;
#endif // UNITY_ANIMATION

        /// <summary>
        /// Looks up if a certain accessor points to interleaved data.
        /// </summary>
        /// <param name="accessorIndex">Accessor index</param>
        /// <returns>True if accessor is interleaved, false if its data is
        /// continuous.</returns>
        public bool IsAccessorInterleaved(int accessorIndex)
        {
            var accessor = Accessors[accessorIndex];
            var bufferView = BufferViews[accessor.bufferView];
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

            if (Asset != null)
            {
                writer.AddProperty("asset");
                Asset.GltfSerialize(writer);
            }
            if (Nodes != null)
            {
                writer.AddArray("nodes");
                foreach (var node in Nodes)
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
            if (Animations!=null) {
                writer.AddArray("animations");
                foreach( var animation in Animations) {
                    animation.GltfSerialize(writer);
                }
                writer.CloseArray();
            }
#endif

            if (Buffers != null)
            {
                writer.AddArray("buffers");
                foreach (var buffer in Buffers)
                {
                    buffer.GltfSerialize(writer);
                }
                writer.CloseArray();
            }

            if (BufferViews != null)
            {
                writer.AddArray("bufferViews");
                foreach (var bufferView in BufferViews)
                {
                    bufferView.GltfSerialize(writer);
                }
                writer.CloseArray();
            }

            if (Accessors != null)
            {
                writer.AddArray("accessors");
                foreach (var accessor in Accessors)
                {
                    accessor.GltfSerialize(writer);
                }
                writer.CloseArray();
            }

            if (Cameras != null)
            {
                writer.AddArray("cameras");
                foreach (var camera in Cameras)
                {
                    camera.GltfSerialize(writer);
                }
                writer.CloseArray();
            }

            if (Images != null)
            {
                writer.AddArray("images");
                foreach (var image in Images)
                {
                    image?.GltfSerialize(writer);
                }
                writer.CloseArray();
            }
            if (Materials != null)
            {
                writer.AddArray("materials");
                foreach (var material in Materials)
                {
                    material.GltfSerialize(writer);
                }
                writer.CloseArray();
            }
            if (Meshes != null)
            {
                writer.AddArray("meshes");
                foreach (var mesh in Meshes)
                {
                    mesh.GltfSerialize(writer);
                }
                writer.CloseArray();
            }
            if (Samplers != null)
            {
                writer.AddArray("samplers");
                foreach (var sampler in Samplers)
                {
                    sampler.GltfSerialize(writer);
                }
                writer.CloseArray();
            }
            if (scene >= 0)
            {
                writer.AddProperty("scene", scene);
            }
            if (Scenes != null)
            {
                writer.AddArray("scenes");
                foreach (var sceneToSerialize in Scenes)
                {
                    sceneToSerialize.GltfSerialize(writer);
                }
                writer.CloseArray();
            }
            if (Skins != null)
            {
                writer.AddArray("skins");
                foreach (var skin in Skins)
                {
                    skin.GltfSerialize(writer);
                }
                writer.CloseArray();
            }
            if (Textures != null)
            {
                writer.AddArray("textures");
                foreach (var texture in Textures)
                {
                    texture.GltfSerialize(writer);
                }
                writer.CloseArray();
            }

            if (Extensions != null)
            {
                writer.AddProperty("extensions");
                Extensions.GltfSerialize(writer);
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
            if (Materials != null)
            {
                foreach (var mat in Materials)
                {
                    // mat.extension is always set (not null), because JsonUtility constructs a default
                    // if any of mat.extension's members is not null, it is because there was
                    // a legit extensions node in JSON => we have to check which ones
                    if (mat.Extensions.KHR_materials_unlit != null)
                    {
                        check = true;
                    }
                    else
                    {
                        // otherwise dump the wrongfully constructed MaterialExtension
                        mat.UnsetExtensions();
                    }
                }
            }
            if (Accessors != null)
            {
                foreach (var accessor in Accessors)
                {
                    if (accessor.Sparse.Indices == null || accessor.Sparse.Values == null)
                    {
                        // If indices and values members are null, `sparse` is likely
                        // an auto-instance by the JsonUtility and not present in JSON.
                        // Therefore we remove it:
                        accessor.UnsetSparse();
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
            if(!check && Meshes!=null) {
                foreach (var mesh in Meshes) {
                    if (mesh.Primitives != null) {
                        foreach (var primitive in mesh.Primitives) {
                            if (primitive.Extensions?.KHR_draco_mesh_compression != null) {
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

            if (Materials != null)
            {
                for (var i = 0; i < Materials.Count; i++)
                {
                    var mat = Materials[i];
                    if (mat.Extensions == null) continue;
                    Assert.AreEqual(mat.name, fakeRoot.materials[i].name);
                    var fake = fakeRoot.materials[i].extensions;
                    if (fake.KHR_materials_unlit == null)
                    {
                        mat.Extensions.KHR_materials_unlit = null;
                    }

                    if (fake.KHR_materials_pbrSpecularGlossiness == null)
                    {
                        mat.Extensions.KHR_materials_pbrSpecularGlossiness = null;
                    }

                    if (fake.KHR_materials_transmission == null)
                    {
                        mat.Extensions.KHR_materials_transmission = null;
                    }

                    if (fake.KHR_materials_clearcoat == null)
                    {
                        mat.Extensions.KHR_materials_clearcoat = null;
                    }

                    if (fake.KHR_materials_sheen == null)
                    {
                        mat.Extensions.KHR_materials_sheen = null;
                    }

                    if (fake.KHR_materials_ior == null)
                    {
                        mat.Extensions.KHR_materials_ior = null;
                    }

                    if (fake.KHR_materials_specular == null)
                    {
                        mat.Extensions.KHR_materials_specular = null;
                    }
                }
            }

#if GLTFAST_SAFE
            if (Accessors != null) {
                for (var i = 0; i < Accessors.Count; i++) {
                    var sparse = fakeRoot.accessors[i].sparse;
                    if (sparse?.indices == null || sparse.values == null) {
                        Accessors[i].UnsetSparse();
                    }
                }
            }
#endif

            if (Meshes != null)
            {
                for (var i = 0; i < Meshes.Count; i++)
                {
                    var mesh = Meshes[i];
                    Assert.AreEqual(mesh.name, fakeRoot.meshes[i].name);
                    for (var j = 0; j < mesh.Primitives.Count; j++)
                    {
                        var primitive = mesh.Primitives[j];
                        if (primitive.Extensions == null) continue;
                        var fake = fakeRoot.meshes[i].primitives[j];
#if DRACO_UNITY
                        if (fake.extensions.KHR_draco_mesh_compression == null) {
                            primitive.Extensions.KHR_draco_mesh_compression = null;
                        }
#endif
                        if (fake.extensions.KHR_materials_variants == null)
                        {
                            primitive.Extensions.KHR_materials_variants = null;
                        }
                    }
                }
            }
            Profiler.EndSample();
        }

        /// <summary>
        /// Cleans up invalid parsing artifacts created by <see cref="GltfJsonUtilityParser"/>.
        /// If you inherit a custom Root class (for use with
        /// <see cref="GltfImport.LoadWithCustomSchema&lt;T&gt;(string,ImportSettings,System.Threading.CancellationToken)"/>
        /// ) you can override this method to perform sanity checks on the deserialized, custom properties.
        /// </summary>
        public virtual void JsonUtilityCleanup()
        {
            if (Nodes != null)
            {
                foreach (var t in Nodes)
                {
                    t.JsonUtilityCleanup();
                }
            }

            if (Extensions != null && !Extensions.JsonUtilityCleanup())
            {
                UnsetExtensions();
            }

            if (Textures != null)
            {
                foreach (var texture in Textures)
                {
                    texture.JsonUtilityCleanup();
                }
            }
        }

        /// <summary>
        /// Number of materials variants.
        /// </summary>
        /// <seealso href="https://github.com/KhronosGroup/glTF/tree/main/extensions/2.0/Khronos/KHR_materials_variants"/>
        public int MaterialsVariantsCount => Extensions?.KHR_materials_variants?.variants?.Count ?? 0;

        /// <summary>
        /// Gets the name of a specific materials variant.
        /// </summary>
        /// <param name="index">Materials variant index.</param>
        /// <returns>Name of a materials variant.</returns>
        /// <seealso href="https://github.com/KhronosGroup/glTF/tree/main/extensions/2.0/Khronos/KHR_materials_variants"/>
        public string GetMaterialsVariantName(int index)
        {
            var variants = Extensions?.KHR_materials_variants?.variants;
            if (variants != null && index >= 0 && index < variants.Count)
            {
                return variants[index].name;
            }

            return null;
        }
    }
}
