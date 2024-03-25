// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

#if UNITY_2020_2_OR_NEWER
#define GLTFAST_MESH_DATA
#endif

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

#if DRACO_UNITY
using Draco.Encode;
#endif
using GLTFast.Schema;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
#if GLTFAST_MESH_DATA
using Unity.Jobs;
#endif
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

using Buffer = GLTFast.Schema.Buffer;
using Camera = GLTFast.Schema.Camera;
using Debug = UnityEngine.Debug;
using Material = GLTFast.Schema.Material;
using Mesh = GLTFast.Schema.Mesh;
using Sampler = GLTFast.Schema.Sampler;
using Texture = GLTFast.Schema.Texture;

#if DEBUG
using System.Text;
#endif
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GLTFast.Export
{

    using Logging;

    /// <summary>
    /// Provides glTF export independent of workflow (GameObjects/Entities)
    /// </summary>
    public class GltfWriter : IGltfWritable
    {

        enum State
        {
            Initialized,
            ContentAdded,
            Disposed
        }

        struct AttributeData
        {
#if GLTFAST_MESH_DATA
            public int stream;
#endif
            public int offset;
            public int accessorId;
        }

#if GLTFAST_MESH_DATA
        const int k_MAXStreamCount = 4;
        const int k_DefaultInnerLoopBatchCount = 512;
#endif

        State m_State;

        ExportSettings m_Settings;
        IDeferAgent m_DeferAgent;
        ICodeLogger m_Logger;

        Root m_Gltf;

        HashSet<Extension> m_ExtensionsUsedOnly;
        HashSet<Extension> m_ExtensionsRequired;

        List<Scene> m_Scenes;
        List<Node> m_Nodes;
        List<Mesh> m_Meshes;
        List<Material> m_Materials;
        List<Texture> m_Textures;
        List<Image> m_Images;
        List<Camera> m_Cameras;
        List<LightPunctual> m_Lights;
        List<Sampler> m_Samplers;
        List<Accessor> m_Accessors;
        List<BufferView> m_BufferViews;

        List<ImageExportBase> m_ImageExports;
        List<SamplerKey> m_SamplerKeys;
        List<UnityEngine.Material> m_UnityMaterials;
        List<UnityEngine.Mesh> m_UnityMeshes;
        Dictionary<int, int[]> m_NodeMaterials;

        Stream m_BufferStream;
        string m_BufferPath;

        /// <summary>
        /// Provides glTF export independent of workflow (GameObjects/Entities)
        /// </summary>
        /// <param name="exportSettings">Export settings</param>
        /// <param name="deferAgent">Defer agent (<see cref="IDeferAgent"/>); decides when/if to preempt
        /// export to preserve a stable frame rate.</param>
        /// <param name="logger">Interface for logging (error) messages.</param>
        public GltfWriter(
            ExportSettings exportSettings = null,
            IDeferAgent deferAgent = null,
            ICodeLogger logger = null
            )
        {
            m_Gltf = new Root();
            m_Settings = exportSettings ?? new ExportSettings();
            m_Logger = logger;
            m_State = State.Initialized;
            m_DeferAgent = deferAgent ?? new UninterruptedDeferAgent();
        }

        /// <inheritdoc />
        public uint AddNode(
            float3? translation = null,
            quaternion? rotation = null,
            float3? scale = null,
            uint[] children = null,
            string name = null
        )
        {
            CertifyNotDisposed();
            m_State = State.ContentAdded;
            var node = CreateNode(translation, rotation, scale, name);
            node.children = children;
            m_Nodes = m_Nodes ?? new List<Node>();
            m_Nodes.Add(node);
            return (uint)m_Nodes.Count - 1;
        }

        /// <inheritdoc />
        public void AddMeshToNode(int nodeId, UnityEngine.Mesh uMesh, int[] materialIds)
        {
            if ((m_Settings.ComponentMask & ComponentType.Mesh) == 0) return;
            CertifyNotDisposed();
            var node = m_Nodes[nodeId];

            if (materialIds != null && materialIds.Length > 0)
            {
                m_NodeMaterials = m_NodeMaterials ?? new Dictionary<int, int[]>();
                m_NodeMaterials[nodeId] = materialIds;
            }

            node.mesh = AddMesh(uMesh);
        }

        /// <inheritdoc />
        public bool AddCamera(UnityEngine.Camera uCamera, out int cameraId)
        {
            if ((m_Settings.ComponentMask & ComponentType.Camera) == 0)
            {
                cameraId = -1;
                return false;
            }
            CertifyNotDisposed();

            var camera = new Camera();

            if (uCamera.orthographic)
            {
                camera.SetCameraType(Camera.Type.Orthographic);
                var oSize = uCamera.orthographicSize;
                float aspectRatio;
                var targetTexture = uCamera.targetTexture;
                if (targetTexture == null)
                {
                    aspectRatio = Screen.width / (float)Screen.height;
                }
                else
                {
                    aspectRatio = targetTexture.width / (float)targetTexture.height;
                }
                camera.orthographic = new CameraOrthographic
                {
                    ymag = oSize,
                    xmag = oSize * aspectRatio,
                    // TODO: Check if local scale should be applied to near/far
                    znear = uCamera.nearClipPlane,
                    zfar = uCamera.farClipPlane
                };
            }
            else
            {
                camera.SetCameraType(Camera.Type.Perspective);
                camera.perspective = new CameraPerspective
                {
                    yfov = uCamera.fieldOfView * Mathf.Deg2Rad,
                    // TODO: Check if local scale should be applied to near/far
                    znear = uCamera.nearClipPlane,
                    zfar = uCamera.farClipPlane
                };
            }

            if (m_Cameras == null)
            {
                m_Cameras = new List<Camera>();
            }
            cameraId = m_Cameras.Count;
            m_Cameras.Add(camera);
            return true;
        }

        /// <inheritdoc />
        public bool AddLight(Light uLight, out int lightId)
        {
            if ((m_Settings.ComponentMask & ComponentType.Light) == 0)
            {
                lightId = -1;
                return false;
            }
            CertifyNotDisposed();
            var light = KhrLightsPunctual.ConvertToLight(uLight);
            light.intensity *= m_Settings.LightIntensityFactor;

            if (m_Lights == null)
            {
                m_Lights = new List<LightPunctual>();
            }
            lightId = m_Lights.Count;
            m_Lights.Add(light);
            return true;
        }

        /// <inheritdoc />
        public void AddCameraToNode(int nodeId, int cameraId)
        {
            CertifyNotDisposed();
            // glTF cameras face in the opposite direction, so we create a
            // helper node that applies the correct rotation.
            // TODO: Detect if this is node is already a helper node
            //       (from glTF import) and discard it (if possible) to enable
            //       lossless round-trips
            var parent = m_Nodes[nodeId];
            var node = AddChildNode(nodeId, rotation: quaternion.RotateY(math.PI), name: $"{parent.name}_Orientation");
            node.camera = cameraId;
        }

        /// <inheritdoc />
        public void AddLightToNode(int nodeId, int lightId)
        {
            CertifyNotDisposed();
            var node = m_Nodes[nodeId];
            var light = m_Lights[lightId];
            if (light.GetLightType() != LightPunctual.Type.Point)
            {
                // glTF lights face in the opposite direction, so we create a
                // helper node that applies the correct rotation.
                // TODO: Detect if this is node is already a helper node
                //       (from glTF import) and discard it (if possible) to enable
                //       lossless round-trips
                node = AddChildNode(nodeId, rotation: quaternion.RotateY(math.PI), name: $"{node.name}_Orientation");
            }
            node.extensions = node.extensions ?? new NodeExtensions();
            node.Extensions.KHR_lights_punctual = new NodeLightsPunctual
            {
                light = lightId
            };
        }

        /// <inheritdoc />
        public uint AddScene(uint[] nodes, string name = null)
        {
            CertifyNotDisposed();
            m_Scenes = m_Scenes ?? new List<Scene>();
            var scene = new Scene
            {
                name = name,
                nodes = nodes
            };
            m_Scenes.Add(scene);
            if (m_Scenes.Count == 1)
            {
                m_Gltf.scene = 0;
            }
            return (uint)m_Scenes.Count - 1;
        }

        /// <inheritdoc />
        public bool AddMaterial(UnityEngine.Material uMaterial, out int materialId, IMaterialExport materialExport)
        {

            if (m_Materials != null)
            {
                materialId = m_UnityMaterials.IndexOf(uMaterial);
                if (materialId >= 0)
                {
                    return true;
                }
            }
            else
            {
                m_Materials = new List<Material>();
                m_UnityMaterials = new List<UnityEngine.Material>();
            }

            var success = materialExport.ConvertMaterial(uMaterial, out var material, this, m_Logger);

            materialId = m_Materials.Count;
            m_Materials.Add(material);
            m_UnityMaterials.Add(uMaterial);
            return success;
        }

        /// <inheritdoc />
        public int AddImage(ImageExportBase imageExport)
        {
#if UNITY_IMAGECONVERSION
            CertifyNotDisposed();
            int imageId;
            if (m_ImageExports != null) {
                imageId = m_ImageExports.IndexOf(imageExport);
                if (imageId >= 0) {
                    return imageId;
                }
            } else {
                m_ImageExports = new List<ImageExportBase>();
                m_Images = new List<Image>();
            }

            imageId = m_ImageExports.Count;

            // TODO: Create sampler, if required
            // TODO: KTX encoding

            var image = new Image {
                name = imageExport.FileName,
                mimeType = imageExport.MimeType
            };

            m_ImageExports.Add(imageExport);
            m_Images.Add(image);

            return imageId;
#else
            m_Logger?.Warning(LogCode.ImageConversionNotEnabled);
            return -1;
#endif
        }

        /// <inheritdoc />
        public int AddTexture(int imageId, int samplerId)
        {
#if UNITY_IMAGECONVERSION
            CertifyNotDisposed();
            m_Textures = m_Textures ?? new List<Texture>();

            var texture = new Texture {
                source = imageId,
                sampler = samplerId
            };

            var index = m_Textures.IndexOf(texture);
            if (index >= 0) {
                return index;
            }

            m_Textures.Add(texture);
            return m_Textures.Count - 1;
#else
            return -1;
#endif
        }

        /// <inheritdoc />
        public int AddSampler(FilterMode filterMode, TextureWrapMode wrapModeU, TextureWrapMode wrapModeV)
        {
            if (filterMode == FilterMode.Bilinear && wrapModeU == TextureWrapMode.Repeat && wrapModeV == TextureWrapMode.Repeat)
            {
                // This is the default, so no sampler needed
                return -1;
            }
            CertifyNotDisposed();
            m_Samplers = m_Samplers ?? new List<Sampler>();
            m_SamplerKeys = m_SamplerKeys ?? new List<SamplerKey>();

            var samplerKey = new SamplerKey(filterMode, wrapModeU, wrapModeV);

            var index = m_SamplerKeys.IndexOf(samplerKey);
            if (index >= 0)
            {
                return index;
            }

            m_Samplers.Add(new Sampler(filterMode, wrapModeU, wrapModeV));
            m_SamplerKeys.Add(samplerKey);
            return m_Samplers.Count - 1;
        }

        /// <inheritdoc />
        public void RegisterExtensionUsage(Extension extension, bool required = true)
        {
            CertifyNotDisposed();
            if (required)
            {
                m_ExtensionsRequired = m_ExtensionsRequired ?? new HashSet<Extension>();
                m_ExtensionsRequired.Add(extension);
            }
            else
            {
                if (m_ExtensionsRequired == null || !m_ExtensionsRequired.Contains(extension))
                {
                    m_ExtensionsUsedOnly = m_ExtensionsUsedOnly ?? new HashSet<Extension>();
                    m_ExtensionsUsedOnly.Add(extension);
                }
            }
        }

        /// <inheritdoc />
        public async Task<bool> SaveToFileAndDispose(string path)
        {

            CertifyNotDisposed();

            var ext = Path.GetExtension(path);
            var binary = m_Settings.Format == GltfFormat.Binary;
            string bufferPath = null;
            if (!binary)
            {
                if (string.IsNullOrEmpty(ext))
                {
                    bufferPath = path + ".bin";
                }
                else
                {
                    bufferPath = path.Substring(0, path.Length - ext.Length) + ".bin";
                }
            }

            var outStream = new FileStream(path, FileMode.Create);
            var success = await SaveAndDispose(outStream, bufferPath, Path.GetDirectoryName(path));
            outStream.Close();
            return success;
        }

        /// <inheritdoc />
        public async Task<bool> SaveToStreamAndDispose(Stream stream)
        {

            CertifyNotDisposed();

            if (m_Settings.Format != GltfFormat.Binary || GetFinalImageDestination() == ImageDestination.SeparateFile)
            {
                m_Logger?.Error(LogCode.None, "Save to Stream currently only works for self-contained glTF-Binary");
                return false;
            }

            return await SaveAndDispose(stream);
        }

        async Task<bool> SaveAndDispose(Stream outStream, string bufferPath = null, string directory = null)
        {

#if DEBUG
            if (m_State != State.ContentAdded) {
                Debug.LogWarning("Exporting empty glTF");
            }
#endif
            m_BufferPath = bufferPath;

            var success = await Bake(Path.GetFileName(m_BufferPath), directory);

            if (!success)
            {
                m_BufferStream?.Close();
                Dispose();
                return false;
            }

            var isBinary = m_Settings.Format == GltfFormat.Binary;

            const uint headerSize = 12; // 4 bytes magic + 4 bytes version + 4 bytes length (uint each)
            const uint chunkOverhead = 8; // 4 bytes chunk length + 4 bytes chunk type (uint each)
            if (isBinary)
            {
                outStream.Write(BitConverter.GetBytes(GltfGlobals.GltfBinaryMagic));
                outStream.Write(BitConverter.GetBytes((uint)2));

                MemoryStream jsonStream = null;
                uint jsonLength;
                var outStreamCanSeek = outStream.CanSeek;
                if (outStreamCanSeek)
                {
                    // Write empty 3 place-holder uints for:
                    // - total length
                    // - JSON chunk length
                    // - JSON chunk format identifier
                    // They'll get filled in later
                    for (var i = 0; i < 12; i++)
                    {
                        outStream.WriteByte(0);
                    }
                    await WriteJsonToStream(outStream);
                    jsonLength = (uint)(outStream.Length - headerSize - chunkOverhead);
                }
                else
                {
                    jsonStream = new MemoryStream();
                    await WriteJsonToStream(jsonStream);
                    jsonLength = (uint)jsonStream.Length;
                }
                LogSummary(jsonLength, m_BufferStream?.Length ?? 0);
                var jsonPad = GetPadByteCount(jsonLength);
                var binPad = 0;
                var totalLength = (uint)(headerSize + chunkOverhead + jsonLength + jsonPad);
                var hasBufferContent = (m_BufferStream?.Length ?? 0) > 0;
                if (hasBufferContent)
                {
                    binPad = GetPadByteCount((uint)m_BufferStream.Length);
                    totalLength += (uint)(chunkOverhead + m_BufferStream.Length + binPad);
                }

                if (outStreamCanSeek)
                {
                    outStream.Seek(8, SeekOrigin.Begin);
                }

                outStream.Write(BitConverter.GetBytes(totalLength));

                outStream.Write(BitConverter.GetBytes((uint)(jsonLength + jsonPad)));
                outStream.Write(BitConverter.GetBytes((uint)ChunkFormat.Json));

                if (outStreamCanSeek)
                {
                    outStream.Seek(0, SeekOrigin.End);
                }
                else
                {
                    jsonStream.WriteTo(outStream);
                    jsonStream.Close();
                }

                for (var i = 0; i < jsonPad; i++)
                {
                    outStream.WriteByte(0x20);
                }

                if (hasBufferContent)
                {
                    outStream.Write(BitConverter.GetBytes((uint)(m_BufferStream.Length + binPad)));
                    outStream.Write(BitConverter.GetBytes((uint)ChunkFormat.Binary));
                    var ms = (MemoryStream)m_BufferStream;
                    ms.WriteTo(outStream);
                    await ms.FlushAsync();
                    for (var i = 0; i < binPad; i++)
                    {
                        outStream.WriteByte(0);
                    }
                }
            }
            else
            {
                await WriteJsonToStream(outStream);
                var jsonLength = 0u;
                if (outStream.CanSeek)
                {
                    jsonLength = (uint)(outStream.Length - headerSize - chunkOverhead);
                }
                LogSummary(jsonLength, m_BufferStream?.Length ?? 0);
            }

            Dispose();
            return true;
        }

        async Task WriteJsonToStream(Stream outStream)
        {
            var writer = new StreamWriter(outStream);
            m_Gltf.GltfSerialize(writer);
            await writer.FlushAsync();
        }

        void CertifyNotDisposed()
        {
            if (m_State == State.Disposed)
            {
                throw new InvalidOperationException("GltfWriter was already disposed");
            }
        }

        ImageDestination GetFinalImageDestination()
        {
            var imageDest = m_Settings.ImageDestination;
            if (imageDest == ImageDestination.Automatic)
            {
                imageDest = m_Settings.Format == GltfFormat.Binary
                    ? ImageDestination.MainBuffer
                    : ImageDestination.SeparateFile;
            }

            return imageDest;
        }

        static int GetPadByteCount(uint length)
        {
            return (4 - (int)(length & 3)) & 3;
        }

        [Conditional("DEBUG")]
        void LogSummary(long jsonLength, long bufferLength)
        {
#if DEBUG
            var sb = new StringBuilder("glTF summary: ");
            sb.AppendFormat("{0} bytes JSON + {1} bytes buffer", jsonLength, bufferLength);
            if (m_Gltf != null) {
                sb.AppendFormat(", {0} nodes", m_Gltf.Nodes?.Count ?? 0);
                sb.AppendFormat(" ,{0} meshes", m_Gltf.meshes?.Length ?? 0);
                sb.AppendFormat(" ,{0} materials", m_Gltf.Materials?.Count ?? 0);
                sb.AppendFormat(" ,{0} images", m_Gltf.Images?.Count ?? 0);
            }
            m_Logger?.Info(sb.ToString());
#endif
        }

        async Task<bool> Bake(string bufferPath, string directory)
        {
            var success = true;

            if (m_Meshes != null && m_Meshes.Count > 0)
            {
#if GLTFAST_MESH_DATA
                success = await BakeMeshes();
                if (!success) return false;
#else
                await BakeMeshesLegacy();
#endif
            }

            AssignMaterialsToMeshes();

            success = await BakeImages(directory);

            if (!success) return false;

            if (m_BufferStream != null && m_BufferStream.Length > 0)
            {
                m_Gltf.buffers = new[] {
                    new Buffer {
                        uri = bufferPath,
                        byteLength = (uint) m_BufferStream.Length
                    }
                };
            }

            m_Gltf.scenes = m_Scenes?.ToArray();
            m_Gltf.nodes = m_Nodes?.ToArray();
            m_Gltf.meshes = m_Meshes?.ToArray();
            m_Gltf.accessors = m_Accessors?.ToArray();
            m_Gltf.bufferViews = m_BufferViews?.ToArray();
            m_Gltf.materials = m_Materials?.ToArray();
            m_Gltf.images = m_Images?.ToArray();
            m_Gltf.textures = m_Textures?.ToArray();
            m_Gltf.samplers = m_Samplers?.ToArray();
            m_Gltf.cameras = m_Cameras?.ToArray();

            if (m_Lights != null && m_Lights.Count > 0)
            {
                RegisterExtensionUsage(Extension.LightsPunctual);
                m_Gltf.extensions = m_Gltf.extensions ?? new Schema.RootExtensions();
                m_Gltf.extensions.KHR_lights_punctual = m_Gltf.extensions.KHR_lights_punctual ?? new LightsPunctual();
                m_Gltf.extensions.KHR_lights_punctual.lights = m_Lights.ToArray();
            }

            m_Gltf.asset = new Asset
            {
                version = "2.0",
                generator = $"Unity {Application.unityVersion} glTFast {Constants.version}"
            };

            BakeExtensions();
            return true;
        }

        void BakeExtensions()
        {
            if (m_ExtensionsRequired != null)
            {
                var usedOnlyCount = m_ExtensionsUsedOnly?.Count ?? 0;
                m_Gltf.extensionsRequired = new string[m_ExtensionsRequired.Count];
                m_Gltf.extensionsUsed = new string[m_ExtensionsRequired.Count + usedOnlyCount];
                var i = 0;
                foreach (var extension in m_ExtensionsRequired)
                {
                    var name = extension.GetName();
                    Assert.IsFalse(string.IsNullOrEmpty(name));
                    m_Gltf.extensionsRequired[i] = name;
                    m_Gltf.extensionsUsed[i] = name;
                    i++;
                }
            }

            if (m_ExtensionsUsedOnly != null)
            {
                var i = 0;
                if (m_Gltf.extensionsUsed == null)
                {
                    m_Gltf.extensionsUsed = new string[m_ExtensionsUsedOnly.Count];
                }
                else
                {
                    i = m_Gltf.extensionsUsed.Length - m_ExtensionsUsedOnly.Count;
                }

                foreach (var extension in m_ExtensionsUsedOnly)
                {
                    m_Gltf.extensionsUsed[i++] = extension.GetName();
                }
            }
        }

        void AssignMaterialsToMeshes()
        {
            if (m_NodeMaterials != null && m_Meshes != null)
            {
                var meshMaterialCombos = new Dictionary<MeshMaterialCombination, int>(m_Meshes.Count);
                var originalCombos = new Dictionary<int, MeshMaterialCombination>(m_Meshes.Count);
                foreach (var nodeMaterial in m_NodeMaterials)
                {
                    var nodeId = nodeMaterial.Key;
                    var materialIds = nodeMaterial.Value;
                    var node = m_Nodes[nodeId];
                    var originalMeshId = node.mesh;
                    var mesh = m_Meshes[originalMeshId];

                    var meshMaterialCombo = new MeshMaterialCombination(originalMeshId, materialIds);

                    if (!originalCombos.ContainsKey(originalMeshId))
                    {
                        // First usage of the original -> assign materials to original
                        AssignMaterialsToMesh(materialIds, mesh);
                        originalCombos[originalMeshId] = meshMaterialCombo;
                        meshMaterialCombos[meshMaterialCombo] = originalMeshId;
                    }
                    else
                    {
                        // Mesh is re-used -> check if this exact materials set was used before
                        if (meshMaterialCombos.TryGetValue(meshMaterialCombo, out var meshId))
                        {
                            // Materials are identical -> re-use Mesh object
                            node.mesh = meshId;
                        }
                        else
                        {
                            // Materials differ -> clone Mesh object and assign materials to clone
                            var clonedMeshId = DuplicateMesh(originalMeshId);
                            mesh = m_Meshes[clonedMeshId];
                            AssignMaterialsToMesh(materialIds, mesh);
                            node.mesh = clonedMeshId;
                            meshMaterialCombos[meshMaterialCombo] = clonedMeshId;
                        }
                    }
                }
            }
            m_NodeMaterials = null;
        }

        static void AssignMaterialsToMesh(int[] materialIds, Mesh mesh)
        {
            for (var i = 0; i < materialIds.Length && i < mesh.primitives.Length; i++)
            {
                mesh.primitives[i].material = materialIds[i] >= 0 ? materialIds[i] : -1;
            }
        }

        int DuplicateMesh(int meshId)
        {
            var src = m_Meshes[meshId];
            var copy = (Mesh)src.Clone();
            m_Meshes.Add(copy);
            return m_Meshes.Count - 1;
        }

#if GLTFAST_MESH_DATA

        async Task<bool> BakeMeshes() {
            Profiler.BeginSample("AcquireReadOnlyMeshData");

            if ((m_Settings.Compression & Compression.Draco) != 0)
            {
#if DRACO_UNITY
                RegisterExtensionUsage(Extension.DracoMeshCompression);
                if (m_Settings.DracoSettings == null)
                {
                    //Ensure fallback to default settings
                    m_Settings.DracoSettings = new DracoExportSettings();
                }
                if ((m_Settings.Compression & Compression.Uncompressed) != 0)
                {
                    m_Logger?.Warning(LogCode.UncompressedFallbackNotSupported);
                }
#else
                m_Logger?.Error(LogCode.PackageMissing, "Draco For Unity", ExtensionName.DracoMeshCompression);
                return false;
#endif
            }
            var tasks = m_Settings.Deterministic ? null : new List<Task>(m_Meshes.Count);

            var meshDataArray = UnityEngine.Mesh.AcquireReadOnlyMeshData(m_UnityMeshes);
            Profiler.EndSample();
            for (var meshId = 0; meshId < m_Meshes.Count; meshId++)
            {
                Task task;
#if DRACO_UNITY
                if ((m_Settings.Compression & Compression.Draco) != 0)
                {
                    task = BakeMeshDraco(meshId, meshDataArray[meshId]);
                }
                else
#endif
                {
                    task = BakeMesh(meshId, meshDataArray[meshId]);
                }

                if (m_Settings.Deterministic)
                {
                    await task;
                }
                else
                {
                    tasks.Add(task);
                }
                await m_DeferAgent.BreakPoint();
            }

            if (!m_Settings.Deterministic)
            {
                await Task.WhenAll(tasks);
            }
            meshDataArray.Dispose();
            return true;
        }

        async Task BakeMesh(int meshId, UnityEngine.Mesh.MeshData meshData) {

            Profiler.BeginSample("BakeMesh 1");

            var mesh = m_Meshes[meshId];
            var uMesh = m_UnityMeshes[meshId];

            var vertexAttributes = uMesh.GetVertexAttributes();
            var strides = new int[k_MAXStreamCount];
            var alignments = new int[k_MAXStreamCount];

            var attributes = new Attributes();
            var vertexCount = uMesh.vertexCount;
            var attrDataDict = new Dictionary<VertexAttribute, AttributeData>();

            foreach (var attribute in vertexAttributes) {
                if (attribute.attribute == VertexAttribute.BlendWeight
                    || attribute.attribute == VertexAttribute.BlendIndices)
                {
                    Debug.LogWarning($"Vertex attribute {attribute.attribute} is not supported yet...skipping");
                    continue;
                }

                var attrData = new AttributeData {
                    offset = strides[attribute.stream],
                    stream = attribute.stream
                };

                var attributeSize = GetAttributeSize(attribute.format);
                var size = attribute.dimension * attributeSize;
                strides[attribute.stream] += size;
                alignments[attribute.stream] = math.max(alignments[attribute.stream], attributeSize);

                // Adhere data alignment rules
                Assert.IsTrue(attrData.offset % 4 == 0);

                var accessor = new Accessor {
                    byteOffset = attrData.offset,
                    componentType = Accessor.GetComponentType(attribute.format),
                    count = vertexCount,
                };
                accessor.SetAttributeType(Accessor.GetAccessorAttributeType(attribute.dimension));

                var accessorId = AddAccessor(accessor);

                attrData.accessorId = accessorId;
                attrDataDict[attribute.attribute] = attrData;

                switch (attribute.attribute) {
                    case VertexAttribute.Position:
                        Assert.AreEqual(VertexAttributeFormat.Float32,attribute.format);
                        Assert.AreEqual(3,attribute.dimension);
                        var bounds = uMesh.bounds;
                        var max = bounds.max;
                        var min = bounds.min;
                        accessor.min = new[] { -max.x, min.y, min.z };
                        accessor.max = new[] { -min.x, max.y, max.z };
                        attributes.POSITION = accessorId;
                        break;
                    case VertexAttribute.Normal:
                        Assert.AreEqual(VertexAttributeFormat.Float32,attribute.format);
                        Assert.AreEqual(3,attribute.dimension);
                        attributes.NORMAL = accessorId;
                        break;
                    case VertexAttribute.Tangent:
                        Assert.AreEqual(VertexAttributeFormat.Float32,attribute.format);
                        Assert.AreEqual(4,attribute.dimension);
                        attributes.TANGENT = accessorId;
                        break;
                    case VertexAttribute.Color:
                        attributes.COLOR_0 = accessorId;
                        break;
                    case VertexAttribute.TexCoord0:
                        attributes.TEXCOORD_0 = accessorId;
                        break;
                    case VertexAttribute.TexCoord1:
                        attributes.TEXCOORD_1 = accessorId;
                        break;
                    case VertexAttribute.TexCoord2:
                        attributes.TEXCOORD_2 = accessorId;
                        break;
                    case VertexAttribute.TexCoord3:
                        attributes.TEXCOORD_3 = accessorId;
                        break;
                    case VertexAttribute.TexCoord4:
                        attributes.TEXCOORD_4 = accessorId;
                        break;
                    case VertexAttribute.TexCoord5:
                        attributes.TEXCOORD_5 = accessorId;
                        break;
                    case VertexAttribute.TexCoord6:
                        attributes.TEXCOORD_6 = accessorId;
                        break;
                    case VertexAttribute.TexCoord7:
                        attributes.TEXCOORD_7 = accessorId;
                        break;
                    case VertexAttribute.BlendWeight:
                        attributes.WEIGHTS_0 = accessorId;
                        break;
                    case VertexAttribute.BlendIndices:
                        attributes.JOINTS_0 = accessorId;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            var streamCount = 1;
            for (var stream = 0; stream < strides.Length; stream++) {
                var stride = strides[stream];
                if (stride <= 0) continue;
                streamCount = stream + 1;
            }

            var indexComponentType = uMesh.indexFormat == IndexFormat.UInt16 ? GltfComponentType.UnsignedShort : GltfComponentType.UnsignedInt;
            mesh.primitives = new MeshPrimitive[meshData.subMeshCount];
            var indexAccessors = new Accessor[meshData.subMeshCount];
            var indexOffset = 0;
            MeshTopology? topology = null;
            for (var subMeshIndex = 0; subMeshIndex < meshData.subMeshCount; subMeshIndex++) {
                var subMesh = meshData.GetSubMesh(subMeshIndex);
                if (!topology.HasValue) {
                    topology = subMesh.topology;
                } else {
                    Assert.AreEqual(topology.Value, subMesh.topology, "Mixed topologies are not supported!");
                }
                var mode = GetDrawMode(subMesh.topology);
                if (!mode.HasValue) {
                    m_Logger?.Error(LogCode.TopologyUnsupported, subMesh.topology.ToString());
                    mode = DrawMode.Points;
                }

                var indexAccessor = new Accessor {
                    byteOffset = indexOffset,
                    componentType = indexComponentType,
                    count = subMesh.indexCount,

                    // min = new []{}, // TODO
                    // max = new []{}, // TODO
                };
                indexAccessor.SetAttributeType(GltfAccessorAttributeType.SCALAR);

                if (subMesh.topology == MeshTopology.Quads) {
                    indexAccessor.count = indexAccessor.count / 2 * 3;
                }

                var indexAccessorId = AddAccessor(indexAccessor);
                indexAccessors[subMeshIndex] = indexAccessor;

                indexOffset += indexAccessor.count * Accessor.GetComponentTypeSize(indexComponentType);

                mesh.primitives[subMeshIndex] = new MeshPrimitive {
                    mode = mode.Value,
                    attributes = attributes,
                    indices = indexAccessorId,
                };
            }
            Assert.IsTrue(topology.HasValue);
            Profiler.EndSample(); // "BakeMesh 1"

            int indexBufferViewId;
            if (uMesh.indexFormat == IndexFormat.UInt16) {
                var indexData16 = meshData.GetIndexData<ushort>();
                if (topology.Value == MeshTopology.Quads) {
                    Profiler.BeginSample("IndexJobUInt16QuadsSchedule");
                    var quadCount = indexData16.Length / 4;
                    var destIndices = new NativeArray<ushort>(quadCount*6,Allocator.TempJob);
                    var job = new ExportJobs.ConvertIndicesQuadFlippedJob<ushort> {
                        input = indexData16,
                        result = destIndices
                    }.Schedule(quadCount, k_DefaultInnerLoopBatchCount);
                    Profiler.EndSample();
                    while (!job.IsCompleted) {
                        await Task.Yield();
                    }
                    Profiler.BeginSample("IndexJobUInt16QuadsPostWork");
                    job.Complete();
                    indexBufferViewId = WriteBufferViewToBuffer(
                        destIndices.Reinterpret<byte>(sizeof(ushort)),
                        byteAlignment:sizeof(ushort)
                        );
                    destIndices.Dispose();
                    Profiler.EndSample();
                } else {
                    Profiler.BeginSample("IndexJobUInt16TrisSchedule");
                    var triangleCount = indexData16.Length / 3;
                    var destIndices = new NativeArray<ushort>(indexData16.Length,Allocator.TempJob);
                    var job = new ExportJobs.ConvertIndicesFlippedJob<ushort> {
                        input = indexData16,
                        result = destIndices
                    }.Schedule(triangleCount, k_DefaultInnerLoopBatchCount);
                    Profiler.EndSample();
                    while (!job.IsCompleted) {
                        await Task.Yield();
                    }
                    Profiler.BeginSample("IndexJobUInt16TrisPostWork");
                    job.Complete();
                    indexBufferViewId = WriteBufferViewToBuffer(
                        destIndices.Reinterpret<byte>(sizeof(ushort)),
                        byteAlignment:sizeof(ushort)
                        );
                    destIndices.Dispose();
                    Profiler.EndSample();
                }
            } else {
                var indexData32 = meshData.GetIndexData<uint>();
                if (topology.Value == MeshTopology.Quads) {
                    Profiler.BeginSample("IndexJobUInt32QuadsSchedule");
                    var quadCount = indexData32.Length / 4;
                    var destIndices = new NativeArray<uint>(quadCount*6,Allocator.TempJob);
                    var job = new ExportJobs.ConvertIndicesQuadFlippedJob<uint> {
                        input = indexData32,
                        result = destIndices
                    }.Schedule(quadCount, k_DefaultInnerLoopBatchCount);
                    Profiler.EndSample();
                    while (!job.IsCompleted) {
                        await Task.Yield();
                    }
                    Profiler.BeginSample("IndexJobUInt32QuadsPostWork");
                    job.Complete();
                    indexBufferViewId = WriteBufferViewToBuffer(
                        destIndices.Reinterpret<byte>(sizeof(uint)),
                        byteAlignment:sizeof(uint)
                        );
                    destIndices.Dispose();
                    Profiler.EndSample();
                } else {
                    Profiler.BeginSample("IndexJobUInt32TrisSchedule");
                    var triangleCount = indexData32.Length / 3;
                    var destIndices = new NativeArray<uint>(indexData32.Length, Allocator.TempJob);
                    var job = new ExportJobs.ConvertIndicesFlippedJob<uint> {
                        input = indexData32,
                        result = destIndices
                    }.Schedule(triangleCount, k_DefaultInnerLoopBatchCount);
                    Profiler.EndSample();
                    while (!job.IsCompleted) {
                        await Task.Yield();
                    }
                    Profiler.BeginSample("IndexJobUInt32TrisPostWork");
                    job.Complete();
                    indexBufferViewId = WriteBufferViewToBuffer(
                        destIndices.Reinterpret<byte>(sizeof(uint)),
                        byteAlignment:sizeof(uint)
                        );
                    destIndices.Dispose();
                    Profiler.EndSample();
                }
            }

            foreach (var accessor in indexAccessors) {
                accessor.bufferView = indexBufferViewId;
            }

            var inputStreams = new NativeArray<byte>[streamCount];
            var outputStreams = new NativeArray<byte>[streamCount];

            for (var stream = 0; stream < streamCount; stream++) {
                inputStreams[stream] = meshData.GetVertexData<byte>(stream);
                outputStreams[stream] = new NativeArray<byte>(inputStreams[stream], Allocator.TempJob);
            }

            foreach (var pair in attrDataDict) {
                var vertexAttribute = pair.Key;
                var attrData = pair.Value;
                switch (vertexAttribute) {
                    case VertexAttribute.Position:
                    case VertexAttribute.Normal:
                        await ConvertPositionAttribute(
                            attrData,
                            (uint)strides[attrData.stream],
                            vertexCount,
                            inputStreams[attrData.stream],
                            outputStreams[attrData.stream]
                            );
                        break;
                    case VertexAttribute.Tangent:
                        await ConvertTangentAttribute(
                            attrData,
                            (uint)strides[attrData.stream],
                            vertexCount,
                            inputStreams[attrData.stream],
                            outputStreams[attrData.stream]
                            );
                        break;
                    case VertexAttribute.TexCoord0:
                    case VertexAttribute.TexCoord1:
                    case VertexAttribute.TexCoord2:
                    case VertexAttribute.TexCoord3:
                    case VertexAttribute.TexCoord4:
                    case VertexAttribute.TexCoord5:
                    case VertexAttribute.TexCoord6:
                    case VertexAttribute.TexCoord7:
                        await ConvertTexCoordAttribute(
                            attrData,
                            (uint)strides[attrData.stream],
                            vertexCount,
                            inputStreams[attrData.stream],
                            outputStreams[attrData.stream]
                            );
                        break;
                }
            }

            var bufferViewIds = new int[streamCount];
            for (var stream = 0; stream < streamCount; stream++) {
                bufferViewIds[stream] = WriteBufferViewToBuffer(
                    outputStreams[stream],
                    strides[stream],
                    alignments[stream]
                    );
                inputStreams[stream].Dispose();
                outputStreams[stream].Dispose();
            }

            foreach (var pair in attrDataDict) {
                var attrData = pair.Value;
                m_Accessors[attrData.accessorId].bufferView = bufferViewIds[attrData.stream];
            }
        }

#if DRACO_UNITY
        async Task BakeMeshDraco(int meshId, UnityEngine.Mesh.MeshData meshData)
        {
            var mesh = m_Meshes[meshId];
            var unityMesh = m_UnityMeshes[meshId];

            var results = await DracoEncoder.EncodeMesh(
                unityMesh,
                meshData,
                (QuantizationSettings) m_Settings.DracoSettings,
                (SpeedSettings) m_Settings.DracoSettings
            );

            if (results == null) return;

            mesh.primitives = new MeshPrimitive[results.Length];
            for (var submesh = 0; submesh < results.Length; submesh++) {
                var encodeResult = results[submesh];
                var bufferViewId = WriteBufferViewToBuffer(encodeResult.data);

                var attributes = new Attributes();
                var dracoAttributes = new Attributes();

                foreach ( var vertexAttributeTuple in encodeResult.vertexAttributes)
                {
                    var vertexAttribute = vertexAttributeTuple.Key;
                    var attribute = vertexAttributeTuple.Value;
                    var accessor = new Accessor {
                        componentType = GltfComponentType.Float,
                        count = (int)encodeResult.vertexCount
                    };
                    var attributeType = Accessor.GetAccessorAttributeType(attribute.dimensions);
                    accessor.SetAttributeType(attributeType);

                    var accessorId = AddAccessor(accessor);

                    if (vertexAttribute == VertexAttribute.Position)
                    {
                        var submeshDesc = unityMesh.GetSubMesh(submesh);
                        var bounds = submeshDesc.bounds;
                        var center = bounds.center;
                        var extents = bounds.extents;
                        accessor.min = new[]
                        {
                            center.x-extents.x,
                            center.y-extents.y,
                            center.z-extents.z
                        };
                        accessor.max = new[]
                        {
                            center.x+extents.x,
                            center.y+extents.y,
                            center.z+extents.z
                        };
                    }
                    SetAttributesByType(
                        vertexAttribute,
                        attributes,
                        dracoAttributes,
                        accessorId,
                        (int)attribute.identifier
                        );
                }

                var indexAccessor = new Accessor
                {
                    componentType = GltfComponentType.UnsignedInt,
                    count = (int)encodeResult.indexCount
                };
                indexAccessor.SetAttributeType(GltfAccessorAttributeType.SCALAR);

                var indicesId = AddAccessor(indexAccessor);

                mesh.primitives[submesh] = new MeshPrimitive {
                    extensions = new MeshPrimitiveExtensions {
                        KHR_draco_mesh_compression = new MeshPrimitiveDracoExtension {
                            bufferView = bufferViewId,
                            attributes = dracoAttributes
                        }
                    },
                    attributes = attributes,
                    indices = indicesId
                };
            }
        }

        static void SetAttributesByType(
            VertexAttribute type,
            Attributes attributes,
            Attributes dracoAttributes,
            int accessorId,
            int dracoId
            )
        {
            switch (type)
            {
                case VertexAttribute.Position:
                    attributes.POSITION = accessorId;
                    dracoAttributes.POSITION = dracoId;
                    break;
                case VertexAttribute.Normal:
                    attributes.NORMAL = accessorId;
                    dracoAttributes.NORMAL = dracoId;
                    break;
                case VertexAttribute.Tangent:
                    attributes.TANGENT = accessorId;
                    dracoAttributes.TANGENT = dracoId;
                    break;
                case VertexAttribute.Color:
                    attributes.COLOR_0 = accessorId;
                    dracoAttributes.COLOR_0 = dracoId;
                    break;
                case VertexAttribute.TexCoord0:
                    attributes.TEXCOORD_0 = accessorId;
                    dracoAttributes.TEXCOORD_0 = dracoId;
                    break;
                case VertexAttribute.TexCoord1:
                    attributes.TEXCOORD_1 = accessorId;
                    dracoAttributes.TEXCOORD_1 = dracoId;
                    break;
                case VertexAttribute.TexCoord2:
                    attributes.TEXCOORD_2 = accessorId;
                    dracoAttributes.TEXCOORD_2 = dracoId;
                    break;
                case VertexAttribute.TexCoord3:
                    attributes.TEXCOORD_3 = accessorId;
                    dracoAttributes.TEXCOORD_3 = dracoId;
                    break;
                case VertexAttribute.TexCoord4:
                    attributes.TEXCOORD_4 = accessorId;
                    dracoAttributes.TEXCOORD_4 = dracoId;
                    break;
                case VertexAttribute.TexCoord5:
                    attributes.TEXCOORD_5 = accessorId;
                    dracoAttributes.TEXCOORD_5 = dracoId;
                    break;
                case VertexAttribute.TexCoord6:
                    attributes.TEXCOORD_6 = accessorId;
                    dracoAttributes.TEXCOORD_6 = dracoId;
                    break;
                case VertexAttribute.TexCoord7:
                    attributes.TEXCOORD_7 = accessorId;
                    dracoAttributes.TEXCOORD_7 = dracoId;
                    break;
            }
        }
#endif // DRACO_UNITY

        int AddAccessor(Accessor accessor) {
            m_Accessors = m_Accessors ?? new List<Accessor>();
            var accessorId = m_Accessors.Count;
            m_Accessors.Add(accessor);
            return accessorId;
        }
#else

        async Task BakeMeshesLegacy()
        {
            for (var meshId = 0; meshId < m_Meshes.Count; meshId++)
            {
                BakeMeshLegacy(meshId);
                await m_DeferAgent.BreakPoint();
            }
        }

        void BakeMeshLegacy(int meshId)
        {

            Profiler.BeginSample("BakeMeshLegacy");

            var mesh = m_Meshes[meshId];
            var uMesh = m_UnityMeshes[meshId];

            var attributes = new Attributes();
            var vertexAttributes = uMesh.GetVertexAttributes();
            var attrDataDict = new Dictionary<VertexAttribute, AttributeData>();

            for (var streamId = 0; streamId < vertexAttributes.Length; streamId++)
            {

                var attribute = vertexAttributes[streamId];

                switch (attribute.attribute)
                {
                    case VertexAttribute.BlendWeight:
                    case VertexAttribute.BlendIndices:
                        Debug.LogWarning($"Vertex attribute {attribute.attribute} is not supported yet");
                        continue;
                }

                var attrData = new AttributeData
                {
                    offset = 0,
#if GLTFAST_MESH_DATA
                    stream = streamId
#endif
                };

                var accessor = new Accessor
                {
                    byteOffset = attrData.offset,
                    componentType = Accessor.GetComponentType(attribute.format),
                    count = uMesh.vertexCount,
                };
                accessor.SetAttributeType(Accessor.GetAccessorAttributeType(attribute.dimension));

                var accessorId = AddAccessor(accessor);

                attrData.accessorId = accessorId;
                attrDataDict[attribute.attribute] = attrData;

                switch (attribute.attribute)
                {
                    case VertexAttribute.Position:
                        Assert.AreEqual(VertexAttributeFormat.Float32, attribute.format);
                        Assert.AreEqual(3, attribute.dimension);
                        var bounds = uMesh.bounds;
                        var max = bounds.max;
                        var min = bounds.min;
                        accessor.min = new[] { -max.x, min.y, min.z };
                        accessor.max = new[] { -min.x, max.y, max.z };
                        attributes.POSITION = accessorId;
                        break;
                    case VertexAttribute.Normal:
                        Assert.AreEqual(VertexAttributeFormat.Float32, attribute.format);
                        Assert.AreEqual(3, attribute.dimension);
                        attributes.NORMAL = accessorId;
                        break;
                    case VertexAttribute.Tangent:
                        Assert.AreEqual(VertexAttributeFormat.Float32, attribute.format);
                        Assert.AreEqual(4, attribute.dimension);
                        attributes.TANGENT = accessorId;
                        break;
                    case VertexAttribute.Color:
                        accessor.componentType = GltfComponentType.UnsignedByte;
                        accessor.normalized = true;
                        attributes.COLOR_0 = accessorId;
                        break;
                    case VertexAttribute.TexCoord0:
                        attributes.TEXCOORD_0 = accessorId;
                        break;
                    case VertexAttribute.TexCoord1:
                        attributes.TEXCOORD_1 = accessorId;
                        break;
                    case VertexAttribute.TexCoord2:
                        attributes.TEXCOORD_2 = accessorId;
                        break;
                    case VertexAttribute.TexCoord3:
                        attributes.TEXCOORD_3 = accessorId;
                        break;
                    case VertexAttribute.TexCoord4:
                        attributes.TEXCOORD_4 = accessorId;
                        break;
                    case VertexAttribute.TexCoord5:
                        attributes.TEXCOORD_5 = accessorId;
                        break;
                    case VertexAttribute.TexCoord6:
                        attributes.TEXCOORD_6 = accessorId;
                        break;
                    case VertexAttribute.TexCoord7:
                        attributes.TEXCOORD_7 = accessorId;
                        break;
                    case VertexAttribute.BlendWeight:
                        attributes.WEIGHTS_0 = accessorId;
                        break;
                    case VertexAttribute.BlendIndices:
                        attributes.JOINTS_0 = accessorId;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            var indexComponentType = uMesh.indexFormat == IndexFormat.UInt16 ? GltfComponentType.UnsignedShort : GltfComponentType.UnsignedInt;
            mesh.primitives = new MeshPrimitive[uMesh.subMeshCount];
            var indexAccessors = new Accessor[uMesh.subMeshCount];
            var indexOffset = 0;
            MeshTopology? topology = null;
            var totalIndexCount = 0u;
            for (var subMeshIndex = 0; subMeshIndex < uMesh.subMeshCount; subMeshIndex++)
            {
                var subMesh = uMesh.GetSubMesh(subMeshIndex);
                if (!topology.HasValue)
                {
                    topology = subMesh.topology;
                }
                else
                {
                    Assert.AreEqual(topology.Value, subMesh.topology, "Mixed topologies are not supported!");
                }
                var mode = GetDrawMode(subMesh.topology);
                if (!mode.HasValue)
                {
                    m_Logger?.Error(LogCode.TopologyUnsupported, subMesh.topology.ToString());
                    mode = DrawMode.Points;
                }

                var indexAccessor = new Accessor
                {
                    byteOffset = indexOffset,
                    componentType = indexComponentType,
                    count = subMesh.indexCount,

                    // min = new []{}, // TODO
                    // max = new []{}, // TODO
                };
                indexAccessor.SetAttributeType(GltfAccessorAttributeType.SCALAR);

                if (subMesh.topology == MeshTopology.Quads)
                {
                    indexAccessor.count = indexAccessor.count / 2 * 3;
                }

                var indexAccessorId = AddAccessor(indexAccessor);
                indexAccessors[subMeshIndex] = indexAccessor;

                indexOffset += indexAccessor.count * Accessor.GetComponentTypeSize(indexComponentType);

                mesh.primitives[subMeshIndex] = new MeshPrimitive
                {
                    mode = mode.Value,
                    attributes = attributes,
                    indices = indexAccessorId,
                };

                totalIndexCount += uMesh.GetIndexCount(subMeshIndex);
            }
            Assert.IsTrue(topology.HasValue);

            Profiler.BeginSample("ExportIndices");
            int indexBufferViewId;
            var totalFaceCount = topology == MeshTopology.Quads ? (uint)(totalIndexCount * 1.5) : totalIndexCount;
            if (uMesh.indexFormat == IndexFormat.UInt16)
            {
                var destIndices = new NativeArray<ushort>((int)totalFaceCount, Allocator.TempJob);
                var offset = 0;
                for (var subMeshIndex = 0; subMeshIndex < uMesh.subMeshCount; subMeshIndex++)
                {
                    var indexData16 = uMesh.GetIndices(subMeshIndex);
                    switch (topology)
                    {
                        case MeshTopology.Triangles:
                            {
                                var triCount = indexData16.Length / 3;
                                for (var i = 0; i < triCount; i++)
                                {
                                    destIndices[offset + i * 3] = (ushort)indexData16[i * 3];
                                    destIndices[offset + i * 3 + 1] = (ushort)indexData16[i * 3 + 2];
                                    destIndices[offset + i * 3 + 2] = (ushort)indexData16[i * 3 + 1];
                                }
                                offset += indexData16.Length;
                                break;
                            }
                        case MeshTopology.Quads:
                            {
                                var quadCount = indexData16.Length / 4;
                                for (var i = 0; i < quadCount; i++)
                                {
                                    destIndices[offset + i * 6 + 0] = (ushort)indexData16[i * 4 + 0];
                                    destIndices[offset + i * 6 + 1] = (ushort)indexData16[i * 4 + 2];
                                    destIndices[offset + i * 6 + 2] = (ushort)indexData16[i * 4 + 1];
                                    destIndices[offset + i * 6 + 3] = (ushort)indexData16[i * 4 + 2];
                                    destIndices[offset + i * 6 + 4] = (ushort)indexData16[i * 4 + 0];
                                    destIndices[offset + i * 6 + 5] = (ushort)indexData16[i * 4 + 3];
                                }
                                offset += quadCount * 6;
                                break;
                            }
                        default:
                            {
                                for (var i = 0; i < indexData16.Length; i++)
                                {
                                    destIndices[offset + i] = (ushort)indexData16[i];
                                }
                                offset += indexData16.Length;
                                break;
                            }
                    }
                }
                indexBufferViewId = WriteBufferViewToBuffer(
                    destIndices.Reinterpret<byte>(sizeof(ushort)),
                    byteAlignment: sizeof(ushort)
                );
                destIndices.Dispose();
            }
            else
            {
                var destIndices = new NativeArray<uint>((int)totalFaceCount, Allocator.TempJob);
                var offset = 0;
                for (var subMeshIndex = 0; subMeshIndex < uMesh.subMeshCount; subMeshIndex++)
                {
                    var indexData16 = uMesh.GetIndices(subMeshIndex);
                    switch (topology)
                    {
                        case MeshTopology.Triangles:
                            {
                                var triCount = indexData16.Length / 3;
                                for (var i = 0; i < triCount; i++)
                                {
                                    destIndices[offset + i * 3] = (uint)indexData16[i * 3];
                                    destIndices[offset + i * 3 + 1] = (uint)indexData16[i * 3 + 2];
                                    destIndices[offset + i * 3 + 2] = (uint)indexData16[i * 3 + 1];
                                }
                                offset += indexData16.Length;
                                break;
                            }
                        case MeshTopology.Quads:
                            {
                                var quadCount = indexData16.Length / 4;
                                for (var i = 0; i < quadCount; i++)
                                {
                                    destIndices[offset + i * 6 + 0] = (uint)indexData16[i * 4 + 0];
                                    destIndices[offset + i * 6 + 1] = (uint)indexData16[i * 4 + 2];
                                    destIndices[offset + i * 6 + 2] = (uint)indexData16[i * 4 + 1];
                                    destIndices[offset + i * 6 + 3] = (uint)indexData16[i * 4 + 2];
                                    destIndices[offset + i * 6 + 4] = (uint)indexData16[i * 4 + 0];
                                    destIndices[offset + i * 6 + 5] = (uint)indexData16[i * 4 + 3];
                                }
                                offset += quadCount * 6;
                                break;
                            }
                        default:
                            {
                                for (var i = 0; i < indexData16.Length; i++)
                                {
                                    destIndices[offset + i] = (uint)indexData16[i];
                                }
                                offset += indexData16.Length;
                                break;
                            }
                    }
                }
                indexBufferViewId = WriteBufferViewToBuffer(
                    destIndices.Reinterpret<byte>(sizeof(uint)),
                    byteAlignment: sizeof(uint)
                );
                destIndices.Dispose();
            }
            Profiler.EndSample();

            foreach (var accessor in indexAccessors)
            {
                accessor.bufferView = indexBufferViewId;
            }

            Profiler.BeginSample("ExportVertexAttributes");
            foreach (var pair in attrDataDict)
            {
                var vertexAttribute = pair.Key;
                var attrData = pair.Value;
                var bufferViewId = -1;
                switch (vertexAttribute)
                {
                    case VertexAttribute.Position:
                        {
                            var vertices = new List<Vector3>();
                            uMesh.GetVertices(vertices);
                            var outStream = new NativeArray<Vector3>(vertices.Count, Allocator.TempJob);
                            for (var i = 0; i < vertices.Count; i++)
                            {
                                outStream[i] = new Vector3(-vertices[i].x, vertices[i].y, vertices[i].z);
                            }
                            bufferViewId = WriteBufferViewToBuffer(
                                outStream.Reinterpret<byte>(12),
                                12
                            );
                            outStream.Dispose();
                            break;
                        }
                    case VertexAttribute.Normal:
                        {
                            var normals = new List<Vector3>();
                            uMesh.GetNormals(normals);
                            var outStream = new NativeArray<Vector3>(normals.Count, Allocator.TempJob);
                            for (var i = 0; i < normals.Count; i++)
                            {
                                outStream[i] = new Vector3(-normals[i].x, normals[i].y, normals[i].z);
                            }
                            bufferViewId = WriteBufferViewToBuffer(
                                outStream.Reinterpret<byte>(12),
                                12
                            );
                            outStream.Dispose();
                            break;
                        }
                    case VertexAttribute.Tangent:
                        {
                            var tangents = new List<Vector4>();
                            uMesh.GetTangents(tangents);
                            var outStream = new NativeArray<Vector4>(tangents.Count, Allocator.TempJob);
                            for (var i = 0; i < tangents.Count; i++)
                            {
                                outStream[i] = new Vector4(tangents[i].x, tangents[i].y, -tangents[i].z, tangents[i].w);
                            }
                            bufferViewId = WriteBufferViewToBuffer(
                                outStream.Reinterpret<byte>(16),
                                16
                            );
                            outStream.Dispose();
                            break;
                        }
                    case VertexAttribute.Color:
                        {
                            var colors = new List<Color32>();
                            uMesh.GetColors(colors);
                            var outStream = new NativeArray<Color32>(colors.Count, Allocator.TempJob);
                            for (var i = 0; i < colors.Count; i++)
                            {
                                outStream[i] = colors[i];
                            }
                            bufferViewId = WriteBufferViewToBuffer(
                                outStream.Reinterpret<byte>(4),
                                4
                            );
                            outStream.Dispose();
                            break;
                        }
                    case VertexAttribute.TexCoord0:
                    case VertexAttribute.TexCoord1:
                    case VertexAttribute.TexCoord2:
                    case VertexAttribute.TexCoord3:
                    case VertexAttribute.TexCoord4:
                    case VertexAttribute.TexCoord5:
                    case VertexAttribute.TexCoord6:
                    case VertexAttribute.TexCoord7:
                        {
                            var uvs = new List<Vector2>();
                            var channel = (int)vertexAttribute - (int)VertexAttribute.TexCoord0;
                            uMesh.GetUVs(channel, uvs);
                            var outStream = new NativeArray<Vector2>(uvs.Count, Allocator.TempJob);
                            for (var i = 0; i < uvs.Count; i++)
                            {
                                outStream[i] = new Vector2(uvs[i].x, 1 - uvs[i].y);
                            }
                            bufferViewId = WriteBufferViewToBuffer(
                                outStream.Reinterpret<byte>(8),
                                8
                            );
                            outStream.Dispose();
                            break;
                        }
                    case VertexAttribute.BlendWeight:
                        break;
                    case VertexAttribute.BlendIndices:
                        break;
                }
                m_Accessors[attrData.accessorId].bufferView = bufferViewId;
            }
            Profiler.EndSample();
            Profiler.EndSample();
        }

        int AddAccessor(Accessor accessor)
        {
            m_Accessors = m_Accessors ?? new List<Accessor>();
            var accessorId = m_Accessors.Count;
            m_Accessors.Add(accessor);
            return accessorId;
        }
#endif // #if GLTFAST_MESH_DATA

        async Task<bool> BakeImages(string directory)
        {
            if (m_ImageExports != null)
            {
                Dictionary<int, string> fileNameOverrides = null;
                var imageDest = GetFinalImageDestination();
                var overwrite = m_Settings.FileConflictResolution == FileConflictResolution.Overwrite;
                if (!overwrite && imageDest == ImageDestination.SeparateFile)
                {
                    var fileExists = false;
                    var fileNames = new HashSet<string>(
#if NET_STANDARD
                        m_ImageExports.Count
#endif
                        );

                    bool GetUniqueFileName(ref string filename)
                    {
                        if (fileNames.Contains(filename))
                        {
                            var i = 0;
                            var extension = Path.GetExtension(filename);
                            var baseName = Path.GetFileNameWithoutExtension(filename);
                            string newName;
                            do
                            {
                                newName = $"{baseName}_{i++}{extension}";
                            } while (fileNames.Contains(newName));

                            filename = newName;
                            return true;
                        }
                        return false;
                    }

                    for (var imageId = 0; imageId < m_ImageExports.Count; imageId++)
                    {
                        var imageExport = m_ImageExports[imageId];
                        var fileName = Path.GetFileName(imageExport.FileName);
                        if (GetUniqueFileName(ref fileName))
                        {
                            fileNameOverrides = fileNameOverrides ?? new Dictionary<int, string>();
                            fileNameOverrides[imageId] = fileName;
                        }
                        fileNames.Add(fileName);
                        var destPath = Path.Combine(directory, fileName);
                        if (File.Exists(destPath))
                        {
                            fileExists = true;
                        }
                    }

                    if (fileExists)
                    {
#if UNITY_EDITOR
                        overwrite = EditorUtility.DisplayDialog(
                            "Image file conflicts",
                            "Some image files at the destination will be overwritten",
                            "Overwrite", "Cancel");
                        if (!overwrite) {
                            return false;
                        }
#else
                        if (m_Settings.FileConflictResolution == FileConflictResolution.Abort)
                        {
                            return false;
                        }
#endif
                    }
                }

                for (var imageId = 0; imageId < m_ImageExports.Count; imageId++)
                {
                    var imageExport = m_ImageExports[imageId];
                    if (imageDest == ImageDestination.MainBuffer)
                    {
                        // TODO: Write from file to buffer stream directly
                        var imageBytes = imageExport.GetData();
                        if (imageBytes != null)
                        {
                            m_Images[imageId].bufferView = WriteBufferViewToBuffer(imageBytes);
                        }
                    }
                    else if (imageDest == ImageDestination.SeparateFile)
                    {
                        if (!(fileNameOverrides != null && fileNameOverrides.TryGetValue(imageId, out var fileName)))
                        {
                            fileName = imageExport.FileName;
                        }
                        if (imageExport.Write(Path.Combine(directory, fileName), overwrite))
                        {
                            m_Images[imageId].uri = fileName;
                        }
                        else
                        {
                            m_Images[imageId] = null;
                        }
                    }
                    await m_DeferAgent.BreakPoint();
                }
            }

            m_ImageExports = null;
            return true;
        }

#if GLTFAST_MESH_DATA

        static async Task ConvertPositionAttribute(
            AttributeData attrData,
            uint byteStride,
            int vertexCount,
            NativeArray<byte> inputStream,
            NativeArray<byte> outputStream
            )
        {
            var job = CreateConvertPositionAttributeJob(attrData, byteStride, vertexCount, inputStream, outputStream);
            while (!job.IsCompleted) {
                await Task.Yield();
            }
            job.Complete();
        }

        static unsafe JobHandle CreateConvertPositionAttributeJob(
            AttributeData attrData,
            uint byteStride,
            int vertexCount,
            NativeArray<byte> inputStream,
            NativeArray<byte> outputStream
            )
        {
            var job = new ExportJobs.ConvertPositionFloatJob {
                input = (byte*)inputStream.GetUnsafeReadOnlyPtr() + attrData.offset,
                byteStride = byteStride,
                output = (byte*)outputStream.GetUnsafePtr() + attrData.offset
            }.Schedule(vertexCount, k_DefaultInnerLoopBatchCount);
            return job;
        }

        static async Task ConvertTangentAttribute(
            AttributeData attrData,
            uint byteStride,
            int vertexCount,
            NativeArray<byte> inputStream,
            NativeArray<byte> outputStream
            )
        {
            var job = CreateConvertTangentAttributeJob(attrData, byteStride, vertexCount, inputStream, outputStream);
            while (!job.IsCompleted) {
                await Task.Yield();
            }
            job.Complete();
        }

        static unsafe JobHandle CreateConvertTangentAttributeJob(
            AttributeData attrData,
            uint byteStride,
            int vertexCount,
            NativeArray<byte> inputStream,
            NativeArray<byte> outputStream
        ) {
            var job = new ExportJobs.ConvertTangentFloatJob {
                input = (byte*)inputStream.GetUnsafeReadOnlyPtr() + attrData.offset,
                byteStride = byteStride,
                output = (byte*)outputStream.GetUnsafePtr() + attrData.offset
            }.Schedule(vertexCount, k_DefaultInnerLoopBatchCount);
            return job;
        }

        static async Task ConvertTexCoordAttribute(
            AttributeData attrData,
            uint byteStride,
            int vertexCount,
            NativeArray<byte> inputStream,
            NativeArray<byte> outputStream
        ) {
            var job = CreateConvertTexCoordAttributeJob(attrData, byteStride, vertexCount, inputStream, outputStream);
            while (!job.IsCompleted) {
                await Task.Yield();
            }
            job.Complete();
        }

        static unsafe JobHandle CreateConvertTexCoordAttributeJob(
            AttributeData attrData,
            uint byteStride,
            int vertexCount,
            NativeArray<byte> inputStream,
            NativeArray<byte> outputStream
        ) {
            var job = new ExportJobs.ConvertTexCoordFloatJob {
                input = (byte*)inputStream.GetUnsafeReadOnlyPtr() + attrData.offset,
                byteStride = byteStride,
                output = (byte*)outputStream.GetUnsafePtr() + attrData.offset
            }.Schedule(vertexCount, k_DefaultInnerLoopBatchCount);
            return job;
        }


#endif // GLTFAST_MESH_DATA

        static DrawMode? GetDrawMode(MeshTopology topology)
        {
            switch (topology)
            {
                case MeshTopology.Quads:
                    return DrawMode.Triangles;
                case MeshTopology.Triangles:
                    return DrawMode.Triangles;
                case MeshTopology.Lines:
                    return DrawMode.Lines;
                case MeshTopology.LineStrip:
                    return DrawMode.LineStrip;
                case MeshTopology.Points:
                    return DrawMode.Points;
                default:
                    return null;
            }
        }

        Node AddChildNode(
            int parentId,
            float3? translation = null,
            quaternion? rotation = null,
            float3? scale = null,
            string name = null
        )
        {
            var parent = m_Nodes[parentId];
            var node = CreateNode(translation, rotation, scale, name);
            m_Nodes.Add(node);
            var nodeId = (uint)m_Nodes.Count - 1;
            if (parent.children == null)
            {
                parent.children = new[] { nodeId };
            }
            else
            {
                var newChildren = new uint[parent.children.Length + 1];
                newChildren[0] = nodeId;
                parent.children.CopyTo(newChildren, 1);
                parent.children = newChildren;
            }
            return node;
        }

        static Node CreateNode(
            float3? translation = null,
            quaternion? rotation = null,
            float3? scale = null,
            string name = null
            )
        {
            var node = new Node
            {
                name = name,
            };
            if (translation.HasValue && !translation.Equals(float3.zero))
            {
                node.translation = new[] { -translation.Value.x, translation.Value.y, translation.Value.z };
            }
            if (rotation.HasValue && !rotation.Equals(quaternion.identity))
            {
                node.rotation = new[] { rotation.Value.value.x, -rotation.Value.value.y, -rotation.Value.value.z, rotation.Value.value.w };
            }
            if (scale.HasValue && !scale.Equals(new float3(1f)))
            {
                node.scale = new[] { scale.Value.x, scale.Value.y, scale.Value.z };
            }

            return node;
        }

        int AddMesh(UnityEngine.Mesh uMesh)
        {
            int meshId;

#if !UNITY_EDITOR
            if (!uMesh.isReadable)
            {
                m_Logger?.Error(LogCode.MeshNotReadable, uMesh.name);
                return -1;
            }
#endif

            if (m_UnityMeshes != null)
            {
                meshId = m_UnityMeshes.IndexOf(uMesh);
                if (meshId >= 0)
                {
                    return meshId;
                }
            }

            var mesh = new Mesh
            {
                name = uMesh.name
            };
            m_Meshes = m_Meshes ?? new List<Mesh>();
            m_UnityMeshes = m_UnityMeshes ?? new List<UnityEngine.Mesh>();
            m_Meshes.Add(mesh);
            m_UnityMeshes.Add(uMesh);
            meshId = m_Meshes.Count - 1;
            return meshId;
        }

        unsafe int WriteBufferViewToBuffer(byte[] bufferViewData, int? byteStride = null)
        {
            var bufferHandle = GCHandle.Alloc(bufferViewData, GCHandleType.Pinned);
            fixed (void* bufferAddress = &bufferViewData[0])
            {
                var nativeData = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<byte>(bufferAddress, bufferViewData.Length, Allocator.None);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                var safetyHandle = AtomicSafetyHandle.Create();
                NativeArrayUnsafeUtility.SetAtomicSafetyHandle(array: ref nativeData, safetyHandle);
#endif
                var bufferViewId = WriteBufferViewToBuffer(nativeData, byteStride);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                AtomicSafetyHandle.Release(safetyHandle);
#endif
                bufferHandle.Free();
                return bufferViewId;
            }
        }

        Stream CertifyBuffer()
        {
            if (m_BufferStream == null)
            {
                // Delayed, implicit stream generation.
                // if `m_BufferPath` was set, we need a FileStream
                if (m_BufferPath != null)
                {
                    m_BufferStream = new FileStream(m_BufferPath, FileMode.Create);
                }
                else
                {
                    m_BufferStream = new MemoryStream();
                }
            }
            return m_BufferStream;
        }

        /// <summary>
        /// Writes the given data to the main buffer, creates a bufferView and returns its index
        /// </summary>
        /// <param name="bufferViewData">Content to write to buffer</param>
        /// <param name="byteStride">The byte size of an element. Provide it,
        /// if it cannot be inferred from the accessor</param>
        /// <param name="byteAlignment">If not zero, the offsets of the bufferView
        /// will be multiple of it to please alignment rules (padding bytes will be added,
        /// if required; see https://www.khronos.org/registry/glTF/specs/2.0/glTF-2.0.html#data-alignment )
        /// </param>
        /// <returns>Buffer view index</returns>
        int WriteBufferViewToBuffer(NativeArray<byte> bufferViewData, int? byteStride = null, int byteAlignment = 0)
        {
            Profiler.BeginSample("WriteBufferViewToBuffer");
            var buffer = CertifyBuffer();
            var byteOffset = buffer.Length;

            if (byteAlignment > 0)
            {
                Assert.IsTrue(byteAlignment < 5); // There is no componentType that requires more than 4 bytes
                var alignmentByteCount = (byteAlignment - (byteOffset % byteAlignment)) % byteAlignment;
                for (var i = 0; i < alignmentByteCount; i++)
                {
                    buffer.WriteByte(0);
                }
                // Update byteOffset
                byteOffset = buffer.Length;
            }

            buffer.Write(bufferViewData);

            var bufferView = new BufferView
            {
                buffer = 0,
                byteOffset = (int)byteOffset,
                byteLength = bufferViewData.Length,
            };
            if (byteStride.HasValue)
            {
                // Adhere data alignment rules
                Assert.IsTrue(byteStride.Value % 4 == 0);
                bufferView.byteStride = byteStride.Value;
            }
            m_BufferViews = m_BufferViews ?? new List<BufferView>();
            var bufferViewId = m_BufferViews.Count;
            m_BufferViews.Add(bufferView);
            Profiler.EndSample();
            return bufferViewId;
        }

        void Dispose()
        {
            m_Settings = null;

            m_Logger = null;
            m_Gltf = null;
            m_ExtensionsUsedOnly = null;
            m_ExtensionsRequired = null;
            m_ImageExports = null;
            m_SamplerKeys = null;
            m_UnityMaterials = null;
            m_UnityMeshes = null;
            m_NodeMaterials = null;
            m_BufferStream?.Close();
            m_BufferStream = null;
            m_BufferPath = null;

            m_Scenes = null;
            m_Nodes = null;
            m_Meshes = null;
            m_Accessors = null;
            m_BufferViews = null;
            m_Materials = null;
            m_Images = null;
            m_Textures = null;
            m_Samplers = null;

            m_State = State.Disposed;
        }

#if GLTFAST_MESH_DATA
        static unsafe int GetAttributeSize(VertexAttributeFormat format) {
            switch (format) {
                case VertexAttributeFormat.Float32:
                    return sizeof(float);
                case VertexAttributeFormat.Float16:
                    return sizeof(half);
                case VertexAttributeFormat.UNorm8:
                    return sizeof(byte);
                case VertexAttributeFormat.SNorm8:
                    return sizeof(sbyte);
                case VertexAttributeFormat.UNorm16:
                    return sizeof(ushort);
                case VertexAttributeFormat.SNorm16:
                    return sizeof(short);
                case VertexAttributeFormat.UInt8:
                    return sizeof(byte);
                case VertexAttributeFormat.SInt8:
                    return sizeof(sbyte);
                case VertexAttributeFormat.UInt16:
                    return sizeof(ushort);
                case VertexAttributeFormat.SInt16:
                    return sizeof(short);
                case VertexAttributeFormat.UInt32:
                    return sizeof(uint);
                case VertexAttributeFormat.SInt32:
                    return sizeof(int);
                default:
                    throw new ArgumentOutOfRangeException(nameof(format), format, null);
            }
        }
#endif
    }
}
