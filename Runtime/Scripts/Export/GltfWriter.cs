// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

#if UNITY_2023_3_OR_NEWER
#define ASYNC_MESH_DATA
#endif

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

#if DRACO_UNITY
using Draco.Encode;
#endif
using GLTFast.Schema;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
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

        readonly struct AttributeData
        {
            public readonly VertexAttributeDescriptor descriptor;
            public readonly int inputOffset;
            public readonly int outputOffset;

            public AttributeData(
                VertexAttributeDescriptor descriptor,
                int inputOffset,
                int outputOffset
                )
            {
                this.descriptor = descriptor;
                this.inputOffset = inputOffset;
                this.outputOffset = outputOffset;
            }

            public int Size => GetAttributeSize(descriptor.format) * descriptor.dimension;
        }

        const int k_MAXStreamCount = 4;
        const int k_DefaultInnerLoopBatchCount = 512;

        State m_State;

        ExportSettings m_Settings;
        IDeferAgent m_DeferAgent;
        ICodeLogger m_Logger;

        Root m_Gltf;

        HashSet<Extension> m_ExtensionsUsedOnly;
        HashSet<Extension> m_ExtensionsRequired;

        List<Scene> m_Scenes;
        List<Node> m_Nodes;
        Dictionary<Transform, int> m_transformToNodeId;
        List<Mesh> m_Meshes;
        List<Skin> m_Skins;
        Dictionary<int, int> m_MeshBindPoses;
        List<int> m_SkinMesh;
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
        List<VertexAttributeUsage> m_MeshVertexAttributeUsage;
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
        /// <param name="meshIdToBonesId">Function used to get the bone Ids of a given mesh</param>
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
        [Obsolete("Use overload with skinning parameter.")]
        public void AddMeshToNode(int nodeId, UnityEngine.Mesh uMesh, int[] materialIds)
        {
            AddMeshToNode(nodeId, uMesh, materialIds, true);
        }

        /// <inheritdoc />
        [Obsolete("Use overload with skinning parameter.")]
        public void AddMeshToNode(int nodeId, UnityEngine.Mesh uMesh, int[] materialIds, bool skinning)
        {
            AddMeshToNode(nodeId, uMesh, materialIds, null);
        }

        /// <inheritdoc />
        public void AddMeshToNode(
            int nodeId,
            UnityEngine.Mesh uMesh,
            int[] materialIds,
            uint[] joints
            )
        {
            if ((m_Settings.ComponentMask & ComponentType.Mesh) == 0) return;
            CertifyNotDisposed();
            var node = m_Nodes[nodeId];

            // Always export positions.
            var attributeUsage = VertexAttributeUsage.Position;
            var skinning = joints != null && joints.Length > 0;
            if (skinning)
            {
                attributeUsage |= VertexAttributeUsage.Skinning;
            }
            var noMaterialAssigned = false;

            if (materialIds != null && materialIds.Length > 0)
            {
                m_NodeMaterials ??= new Dictionary<int, int[]>();
                m_NodeMaterials[nodeId] = materialIds;

                foreach (var materialId in materialIds)
                {
                    if (materialId < 0)
                    {
                        noMaterialAssigned = true;
                    }
                    else
                    {
                        attributeUsage |= GetVertexAttributeUsage(m_UnityMaterials[materialId].shader);
                    }
                }
            }
            else
            {
                noMaterialAssigned = true;
            }

            if (noMaterialAssigned)
            {
                // No material.
                // This means the default material will be assigned, which requires positions, normals and colors.
                attributeUsage |= VertexAttributeUsage.Normal | VertexAttributeUsage.Color;
            }

            if (!skinning)
            {
                attributeUsage &= ~VertexAttributeUsage.Skinning;
            }

            node.mesh = AddMesh(uMesh, attributeUsage);
            if (skinning)
            {
                node.skin = AddSkin(node.mesh, joints);
            }
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
#if UNITY_WEBGL && !UNITY_EDITOR
                    // FlushAsync never finishes on the Web, so doing it in sync
                    ms.Flush();
#else
                    await ms.FlushAsync();
#endif
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
#if UNITY_WEBGL && !UNITY_EDITOR
            // FlushAsync never finishes on the Web, so doing it in sync
            writer.Flush();
#else
            await writer.FlushAsync();
#endif
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
                success = await BakeMeshes();
                if (!success) return false;
            }

            AssignBindPosesToSkins();
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
            m_Gltf.skins = m_Skins?.ToArray();
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

        void AssignBindPosesToSkins()
        {
            if (m_SkinMesh == null || m_MeshBindPoses == null) return;
            for (var skinId = 0; skinId < m_SkinMesh.Count; skinId++)
            {
                var meshId = m_SkinMesh[skinId];
                var inverseBindMatricesAccessor = m_MeshBindPoses[meshId];
                m_Skins[skinId].inverseBindMatrices = inverseBindMatricesAccessor;
            }

            m_SkinMesh = null;
            m_MeshBindPoses = null;
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
                    if (originalMeshId < 0) continue;
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

        async Task<bool> BakeMeshes()
        {
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

            var meshData = CollectMeshData(out var meshDataArray);

            Profiler.EndSample();
            for (var meshId = 0; meshId < m_Meshes.Count; meshId++)
            {
                Task task;
#if DRACO_UNITY
                if ((m_Settings.Compression & Compression.Draco) != 0)
                {
                    task = BakeMeshDraco(meshId);
                }
                else
#endif
                {
                    task = BakeMesh(meshId, meshData[meshId]);
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
            meshDataArray?.Dispose();
            return true;
        }

        IMeshData[] CollectMeshData(out UnityEngine.Mesh.MeshDataArray? meshDataArray)
        {
            var meshData = new IMeshData[m_UnityMeshes.Count];
            var nonReadableMesh = false;
            var readableMeshCount = 0;
            List<UnityEngine.Mesh> readableMeshes = null;
            List<int> indexMap = null;

            for (var i = 0; i < m_UnityMeshes.Count; i++)
            {
                var mesh = m_UnityMeshes[i];
                if (mesh.isReadable)
                {
                    if (nonReadableMesh)
                    {
                        // There's been a non-readable mesh before, so put this mesh in the queue.
                        if (readableMeshes == null)
                        {
                            readableMeshes = new List<UnityEngine.Mesh>();
                            indexMap = new List<int>();
                        }
                        readableMeshes.Add(mesh);
                        indexMap.Add(i);
                    }

                    readableMeshCount++;
                }
                else
                {
#if UNITY_2021_3_OR_NEWER
                    meshData[i] = mesh.indexFormat == IndexFormat.UInt16
                        ? new NonReadableMeshData<ushort>(mesh)
                        : new NonReadableMeshData<uint>(mesh);
                    if (readableMeshes == null && readableMeshCount > 0)
                    {
                        // This is the first non-readable mesh, so all potential predecessors are readable and we put
                        // them in the queue.
                        readableMeshes = new List<UnityEngine.Mesh>(i);
                        indexMap = new List<int>(i);
                        for (var a = 0; a < i; a++)
                        {
                            readableMeshes.Add(m_UnityMeshes[a]);
                            indexMap.Add(a);
                        }
                    }
#endif
                    nonReadableMesh = true;
                }
            }

            meshDataArray = null;
            if (readableMeshCount > 0)
            {
                if (readableMeshes == null)
                {
                    // All meshes are readable, bulk acquire data for all of them.
                    meshDataArray = UnityEngine.Mesh.AcquireReadOnlyMeshData(m_UnityMeshes);
                    for (var i = 0; i < m_UnityMeshes.Count; i++)
                    {
                        meshData[i] = m_UnityMeshes[i].indexFormat == IndexFormat.UInt16
                            ? (IMeshData)new MeshDataProxy<ushort>(meshDataArray.Value[i])
                            : new MeshDataProxy<uint>(meshDataArray.Value[i]);
                    }
                }
                else
                {
                    // Only a subset of the meshes are readable.
                    meshDataArray = UnityEngine.Mesh.AcquireReadOnlyMeshData(readableMeshes);
                    for (var i = 0; i < readableMeshes.Count; i++)
                    {
                        var actualIndex = indexMap[i];
                        meshData[actualIndex] = m_UnityMeshes[actualIndex].indexFormat == IndexFormat.UInt16
                            ? (IMeshData)new MeshDataProxy<ushort>(meshDataArray.Value[i])
                            : new MeshDataProxy<uint>(meshDataArray.Value[i]);
                    }
                }
            }

            return meshData;
        }

        async Task BakeMesh(int meshId, IMeshData meshData)
        {

            Profiler.BeginSample("BakeMesh 1");

            var mesh = m_Meshes[meshId];
            var uMesh = m_UnityMeshes[meshId];
            var vertexAttributeUsage = m_Settings.PreservedVertexAttributes | m_MeshVertexAttributeUsage[meshId];

            var vertexAttributes = uMesh.GetVertexAttributes();
            var inputStrides = new int[k_MAXStreamCount];
            var outputStrides = new int[k_MAXStreamCount];
            var alignments = new int[k_MAXStreamCount];
            var streamAccessorIds = new List<int>[k_MAXStreamCount];

            var attributes = new Attributes();
            var vertexCount = uMesh.vertexCount;
            var attrDataDict = new Dictionary<VertexAttribute, AttributeData>();

            foreach (var attribute in vertexAttributes)
            {
                var excludeAttribute = (attribute.attribute.ToVertexAttributeUsage() & vertexAttributeUsage) == VertexAttributeUsage.None;

                var attributeElementSize = GetAttributeSize(attribute.format);
                var attributeSize = attribute.dimension * attributeElementSize;

                var attrData = new AttributeData(
                    attribute,
                    inputStrides[attribute.stream],
                    outputStrides[attribute.stream]
                );

                inputStrides[attribute.stream] += attributeSize;
                alignments[attribute.stream] = math.max(alignments[attribute.stream], attributeElementSize);

                if (excludeAttribute)
                {
                    continue;
                }

                outputStrides[attribute.stream] += attributeSize;
                // Adhere data alignment rules
                Assert.IsTrue(attrData.outputOffset % 4 == 0);

                var accessor = new Accessor
                {
                    byteOffset = attrData.outputOffset,
                    componentType = Accessor.GetComponentType(attribute.format),
                    count = vertexCount,
                };
                accessor.SetAttributeType(Accessor.GetAccessorAttributeType(attribute.dimension));

                var accessorId = AddAccessor(accessor);

                streamAccessorIds[attribute.stream] ??= new List<int>();
                streamAccessorIds[attribute.stream].Add(accessorId);

                attrDataDict[attribute.attribute] = attrData;

                switch (attribute.attribute)
                {
                    case VertexAttribute.Position:
                        var bounds = uMesh.bounds;
                        var max = bounds.max;
                        var min = bounds.min;
                        accessor.min = new[] { -max.x, min.y, min.z };
                        accessor.max = new[] { -min.x, max.y, max.z };
                        attributes.POSITION = accessorId;
                        break;
                    case VertexAttribute.Normal:
                        attributes.NORMAL = accessorId;
                        break;
                    case VertexAttribute.Tangent:
                        Assert.AreEqual(4, attribute.dimension, "Invalid tangent vector dimension");
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
                        accessor.componentType = GltfComponentType.UnsignedShort;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            // Add skin
            var bindposes = uMesh.bindposes;
            if (bindposes != null && bindposes.Length > 0)
            {
                var accessor = new Accessor
                {
                    byteOffset = 0,
                    componentType = GltfComponentType.Float,
                    count = bindposes.Length
                };
                accessor.SetAttributeType(GltfAccessorAttributeType.MAT4);

                var accessorId = AddAccessor(accessor);
                m_MeshBindPoses ??= new Dictionary<int, int>();
                m_MeshBindPoses[meshId] = accessorId;

                var bufferViewId = await WriteBindPosesToBuffer(bindposes);
                accessor.bufferView = bufferViewId;
            }

            var streamCount = 1;
            for (var stream = 0; stream < outputStrides.Length; stream++)
            {
                var stride = outputStrides[stream];
                if (stride <= 0) continue;
                streamCount = stream + 1;
            }

            var indexComponentType = uMesh.indexFormat == IndexFormat.UInt16 ? GltfComponentType.UnsignedShort : GltfComponentType.UnsignedInt;
            mesh.primitives = new MeshPrimitive[meshData.subMeshCount];
            var indexAccessors = new Accessor[meshData.subMeshCount];
            var indexOffset = 0;
            MeshTopology? topology = null;
            for (var subMeshIndex = 0; subMeshIndex < meshData.subMeshCount; subMeshIndex++)
            {
                var subMeshTopology = meshData.GetTopology(subMeshIndex);
                if (!topology.HasValue)
                {
                    topology = subMeshTopology;
                }
                else
                {
                    Assert.AreEqual(topology.Value, subMeshTopology, "Mixed topologies are not supported!");
                }
                var mode = GetDrawMode(subMeshTopology);
                if (!mode.HasValue)
                {
                    m_Logger?.Error(LogCode.TopologyUnsupported, subMeshTopology.ToString());
                    mode = DrawMode.Points;
                }

                var indexAccessor = new Accessor
                {
                    byteOffset = indexOffset,
                    componentType = indexComponentType,
                    count = meshData.GetIndexCount(subMeshIndex),

                    // min = new []{}, // TODO
                    // max = new []{}, // TODO
                };
                indexAccessor.SetAttributeType(GltfAccessorAttributeType.SCALAR);

                if (subMeshTopology == MeshTopology.Quads)
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
            }
            Assert.IsTrue(topology.HasValue);
            Profiler.EndSample(); // "BakeMesh 1"

            int indexBufferViewId;
            if (uMesh.indexFormat == IndexFormat.UInt16)
            {
                var indexData16 =
#if ASYNC_MESH_DATA
                    await
#endif
                        ((IMeshData<ushort>)meshData).GetIndexData();
                if (topology.Value == MeshTopology.Quads)
                {
                    Profiler.BeginSample("IndexJobUInt16QuadsSchedule");
                    var quadCount = indexData16.Length / 4;
                    var destIndices = new NativeArray<ushort>(quadCount * 6, Allocator.TempJob);
                    var job = new ExportJobs.ConvertIndicesQuadFlippedJob<ushort>
                    {
                        input = indexData16,
                        result = destIndices
                    }.Schedule(quadCount, k_DefaultInnerLoopBatchCount);
                    Profiler.EndSample();
                    while (!job.IsCompleted)
                    {
                        await Task.Yield();
                    }
                    Profiler.BeginSample("IndexJobUInt16QuadsPostWork");
                    job.Complete();
                    indexBufferViewId = WriteBufferViewToBuffer(
                        destIndices.Reinterpret<byte>(sizeof(ushort)),
                        BufferViewTarget.ElementArrayBuffer,
                        byteAlignment: sizeof(ushort)
                        );
                    destIndices.Dispose();
                    Profiler.EndSample();
                }
                else
                {
                    Profiler.BeginSample("IndexJobUInt16TrisSchedule");
                    var triangleCount = indexData16.Length / 3;
                    var destIndices = new NativeArray<ushort>(triangleCount * 3, Allocator.TempJob);
                    var job = new ExportJobs.ConvertIndicesFlippedJob<ushort>
                    {
                        input = indexData16,
                        result = destIndices
                    }.Schedule(triangleCount, k_DefaultInnerLoopBatchCount);
                    Profiler.EndSample();
                    while (!job.IsCompleted)
                    {
                        await Task.Yield();
                    }
                    Profiler.BeginSample("IndexJobUInt16TrisPostWork");
                    job.Complete();
                    indexBufferViewId = WriteBufferViewToBuffer(
                        destIndices.Reinterpret<byte>(sizeof(ushort)),
                        BufferViewTarget.ElementArrayBuffer,
                        byteAlignment: sizeof(ushort)
                        );
                    destIndices.Dispose();
                    Profiler.EndSample();
                }
                indexData16.Dispose();
            }
            else
            {
                var indexData32 =
#if ASYNC_MESH_DATA
                    await
#endif
                    ((IMeshData<uint>)meshData).GetIndexData();
                if (topology.Value == MeshTopology.Quads)
                {
                    Profiler.BeginSample("IndexJobUInt32QuadsSchedule");
                    var quadCount = indexData32.Length / 4;
                    var destIndices = new NativeArray<uint>(quadCount * 6, Allocator.TempJob);
                    var job = new ExportJobs.ConvertIndicesQuadFlippedJob<uint>
                    {
                        input = indexData32,
                        result = destIndices
                    }.Schedule(quadCount, k_DefaultInnerLoopBatchCount);
                    Profiler.EndSample();
                    while (!job.IsCompleted)
                    {
                        await Task.Yield();
                    }
                    Profiler.BeginSample("IndexJobUInt32QuadsPostWork");
                    job.Complete();
                    indexBufferViewId = WriteBufferViewToBuffer(
                        destIndices.Reinterpret<byte>(sizeof(uint)),
                        BufferViewTarget.ElementArrayBuffer,
                        byteAlignment: sizeof(uint)
                        );
                    destIndices.Dispose();
                    Profiler.EndSample();
                }
                else
                {
                    Profiler.BeginSample("IndexJobUInt32TrisSchedule");
                    var triangleCount = indexData32.Length / 3;
                    var destIndices = new NativeArray<uint>(indexData32.Length, Allocator.TempJob);
                    var job = new ExportJobs.ConvertIndicesFlippedJob<uint>
                    {
                        input = indexData32,
                        result = destIndices
                    }.Schedule(triangleCount, k_DefaultInnerLoopBatchCount);
                    Profiler.EndSample();
                    while (!job.IsCompleted)
                    {
                        await Task.Yield();
                    }
                    Profiler.BeginSample("IndexJobUInt32TrisPostWork");
                    job.Complete();
                    indexBufferViewId = WriteBufferViewToBuffer(
                        destIndices.Reinterpret<byte>(sizeof(uint)),
                        BufferViewTarget.ElementArrayBuffer,
                        byteAlignment: sizeof(uint)
                        );
                    destIndices.Dispose();
                    Profiler.EndSample();
                }

                indexData32.Dispose();
            }

            foreach (var accessor in indexAccessors)
            {
                accessor.bufferView = indexBufferViewId;
            }

            var inputStreams = new NativeArray<byte>[streamCount];
            var outputStreams = new NativeArray<byte>[streamCount];

            for (var stream = 0; stream < streamCount; stream++)
            {
                inputStreams[stream] =
#if ASYNC_MESH_DATA
                    await
#endif
                    meshData.GetVertexData(stream);

                outputStreams[stream] = new NativeArray<byte>(outputStrides[stream] * vertexCount, Allocator.TempJob);
            }

            foreach (var pair in attrDataDict)
            {
                var vertexAttribute = pair.Key;
                var attrData = pair.Value;
                switch (vertexAttribute)
                {
                    case VertexAttribute.Position:
                    case VertexAttribute.Normal:
                        await ConvertPositionAttribute(
                            attrData,
                            (uint)inputStrides[attrData.descriptor.stream],
                            (uint)outputStrides[attrData.descriptor.stream],
                            vertexCount,
                            inputStreams[attrData.descriptor.stream],
                            outputStreams[attrData.descriptor.stream]
                            );
                        break;
                    case VertexAttribute.Tangent:
                        await ConvertTangentAttribute(
                            attrData,
                            (uint)inputStrides[attrData.descriptor.stream],
                            (uint)outputStrides[attrData.descriptor.stream],
                            vertexCount,
                            inputStreams[attrData.descriptor.stream],
                            outputStreams[attrData.descriptor.stream]
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
                            (uint)inputStrides[attrData.descriptor.stream],
                            (uint)outputStrides[attrData.descriptor.stream],
                            vertexCount,
                            inputStreams[attrData.descriptor.stream],
                            outputStreams[attrData.descriptor.stream]
                            );
                        break;
                    case VertexAttribute.Color:
                    case VertexAttribute.BlendWeight:
                        await ConvertSkinWeightsAttribute(
                            attrData,
                            (uint)inputStrides[attrData.descriptor.stream],
                            (uint)outputStrides[attrData.descriptor.stream],
                            vertexCount,
                            inputStreams[attrData.descriptor.stream],
                            outputStreams[attrData.descriptor.stream]
                        );
                        break;
                    case VertexAttribute.BlendIndices:
                        Profiler.BeginSample("ConvertSkinningAttributesJob");
                        // indices are uint*4 in Unity, and ushort*4 in glTF
                        await ConvertSkinIndicesAttributes(
                            attrData,
                            (uint)inputStrides[attrData.descriptor.stream],
                            (uint)outputStrides[attrData.descriptor.stream],
                            vertexCount,
                            inputStreams[attrData.descriptor.stream],
                            outputStreams[attrData.descriptor.stream]
                        );
                        Profiler.EndSample();
                        break;
                    default:
                        await ConvertGenericAttribute(
                            attrData,
                            (uint)inputStrides[attrData.descriptor.stream],
                            (uint)outputStrides[attrData.descriptor.stream],
                            vertexCount,
                            inputStreams[attrData.descriptor.stream],
                            outputStreams[attrData.descriptor.stream]
                        );
                        break;
                }
            }

            var bufferViewIds = new int[streamCount];
            for (var stream = 0; stream < streamCount; stream++)
            {
                var bufferViewId = WriteBufferViewToBuffer(
                    outputStreams[stream],
                    BufferViewTarget.ArrayBuffer,
                    outputStrides[stream],
                    alignments[stream]
                    );
                bufferViewIds[stream] = bufferViewId;

                inputStreams[stream].Dispose();
                outputStreams[stream].Dispose();

                var accessorIds = streamAccessorIds[stream];
                if (accessorIds != null)
                {
                    foreach (var accessorId in accessorIds)
                    {
                        m_Accessors[accessorId].bufferView = bufferViewId;
                    }
                }
            }
        }

        async Task<int> WriteBindPosesToBuffer(Matrix4x4[] bindposes)
        {
            var bufferViewId = -1;
            var nativeBindPoses = new ManagedNativeArray<Matrix4x4, float4x4>(bindposes);
            var matrices = nativeBindPoses.nativeArray;
            var job = new ExportJobs.ConvertMatrixJob
            {
                matrices = matrices
            }.Schedule(bindposes.Length, k_DefaultInnerLoopBatchCount);

            while (!job.IsCompleted)
            {
                await Task.Yield();
            }
            job.Complete();
            bufferViewId = WriteBufferViewToBuffer(
                matrices.Reinterpret<byte>(sizeof(float) * 4 * 4), BufferViewTarget.None
            );

            nativeBindPoses.Dispose();
            return bufferViewId;
        }

#if DRACO_UNITY
        async Task BakeMeshDraco(int meshId)
        {
            var mesh = m_Meshes[meshId];
            var unityMesh = m_UnityMeshes[meshId];

#if UNITY_EDITOR
            // Non-readable meshes are unsupported during playmode or in builds, but work in Editor exports.
            if (Application.isPlaying)
#endif
            {
                if (!unityMesh.isReadable)
                {
                    return;
                }
            }

            var results = await DracoEncoder.EncodeMesh(
                unityMesh,
                (QuantizationSettings) m_Settings.DracoSettings,
                (SpeedSettings) m_Settings.DracoSettings
            );

            if (results == null) return;

            mesh.primitives = new MeshPrimitive[results.Length];
            for (var submesh = 0; submesh < results.Length; submesh++) {
                var encodeResult = results[submesh];
                var bufferViewId = WriteBufferViewToBuffer(encodeResult.data, BufferViewTarget.None);

                var attributes = new Attributes();
                var dracoAttributes = new Attributes();

                foreach ( var vertexAttributeTuple in encodeResult.vertexAttributes)
                {
                    var vertexAttribute = vertexAttributeTuple.Key;
                    var attribute = vertexAttributeTuple.Value;
                    var accessor = new Accessor {
                        componentType = vertexAttribute == VertexAttribute.BlendIndices
                            ? GltfComponentType.UnsignedShort
                            : GltfComponentType.Float,
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
                            -center.x-extents.x,
                            center.y-extents.y,
                            center.z-extents.z
                        };
                        accessor.max = new[]
                        {
                            -center.x+extents.x,
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
                case VertexAttribute.BlendWeight:
                    attributes.WEIGHTS_0 = accessorId;
                    dracoAttributes.WEIGHTS_0 = dracoId;
                    break;
                case VertexAttribute.BlendIndices:
                    attributes.JOINTS_0 = accessorId;
                    dracoAttributes.JOINTS_0 = dracoId;
                    break;
            }
        }
#endif // DRACO_UNITY

        int AddAccessor(Accessor accessor)
        {
            m_Accessors = m_Accessors ?? new List<Accessor>();
            var accessorId = m_Accessors.Count;
            m_Accessors.Add(accessor);
            return accessorId;
        }

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
                            m_Images[imageId].bufferView = WriteBufferViewToBuffer(imageBytes, BufferViewTarget.None);
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

        static async Task ConvertSkinWeightsAttribute(
            AttributeData attrData,
            uint inputByteStride,
            uint outputByteStride,
            int vertexCount,
            NativeArray<byte> inputStream,
            NativeArray<byte> outputStream
        )
        {
            var job = ConvertSkinWeightsAttributeJob(attrData, inputByteStride, outputByteStride, vertexCount, inputStream, outputStream);
            while (!job.IsCompleted)
            {
                await Task.Yield();
            }
            job.Complete(); // TODO: Wait until thread is finished
        }

        static async Task ConvertPositionAttribute(
            AttributeData attrData,
            uint inputByteStride,
            uint outputByteStride,
            int vertexCount,
            NativeArray<byte> inputStream,
            NativeArray<byte> outputStream
            )
        {
            var job = CreateConvertPositionAttributeJob(
                attrData,
                inputByteStride,
                outputByteStride,
                vertexCount,
                inputStream,
                outputStream
                );
            while (!job.IsCompleted)
            {
                await Task.Yield();
            }
            job.Complete();
        }

        static unsafe JobHandle CreateConvertPositionAttributeJob(
            AttributeData attrData,
            uint inputByteStride,
            uint outputByteStride,
            int vertexCount,
            NativeArray<byte> inputStream,
            NativeArray<byte> outputStream
            )
        {
            if (attrData.descriptor.format == VertexAttributeFormat.Float16)
            {
                return new ExportJobs.ConvertPositionHalfJob
                {
                    input = (byte*)inputStream.GetUnsafeReadOnlyPtr() + attrData.inputOffset,
                    inputByteStride = inputByteStride,
                    outputByteStride = outputByteStride,
                    output = (byte*)outputStream.GetUnsafePtr() + attrData.outputOffset
                }.Schedule(vertexCount, k_DefaultInnerLoopBatchCount);
            }
            Assert.AreEqual(VertexAttributeFormat.Float32, attrData.descriptor.format, "Unsupported positions/normals format");
            return new ExportJobs.ConvertPositionFloatJob
            {
                input = (byte*)inputStream.GetUnsafeReadOnlyPtr() + attrData.inputOffset,
                inputByteStride = inputByteStride,
                outputByteStride = outputByteStride,
                output = (byte*)outputStream.GetUnsafePtr() + attrData.outputOffset
            }.Schedule(vertexCount, k_DefaultInnerLoopBatchCount);
        }

        static async Task ConvertTangentAttribute(
            AttributeData attrData,
            uint inputByteStride,
            uint outputByteStride,
            int vertexCount,
            NativeArray<byte> inputStream,
            NativeArray<byte> outputStream
            )
        {
            var job = CreateConvertTangentAttributeJob(
                attrData,
                inputByteStride,
                outputByteStride,
                vertexCount,
                inputStream,
                outputStream
                );
            while (!job.IsCompleted)
            {
                await Task.Yield();
            }
            job.Complete();
        }

        static unsafe JobHandle CreateConvertTangentAttributeJob(
            AttributeData attrData,
            uint inputByteStride,
            uint outputByteStride,
            int vertexCount,
            NativeArray<byte> inputStream,
            NativeArray<byte> outputStream
        )
        {
            if (attrData.descriptor.format == VertexAttributeFormat.Float16)
            {
                return new ExportJobs.ConvertTangentHalfJob
                {
                    input = (byte*)inputStream.GetUnsafeReadOnlyPtr() + attrData.inputOffset,
                    inputByteStride = inputByteStride,
                    outputByteStride = outputByteStride,
                    output = (byte*)outputStream.GetUnsafePtr() + attrData.outputOffset
                }.Schedule(vertexCount, k_DefaultInnerLoopBatchCount);
            }
            Assert.AreEqual(VertexAttributeFormat.Float32, attrData.descriptor.format, "Unsupported tangents format");
            return new ExportJobs.ConvertTangentFloatJob
            {
                input = (byte*)inputStream.GetUnsafeReadOnlyPtr() + attrData.inputOffset,
                inputByteStride = inputByteStride,
                outputByteStride = outputByteStride,
                output = (byte*)outputStream.GetUnsafePtr() + attrData.outputOffset
            }.Schedule(vertexCount, k_DefaultInnerLoopBatchCount);
        }

        static async Task ConvertTexCoordAttribute(
            AttributeData attrData,
            uint inputByteStride,
            uint outputByteStride,
            int vertexCount,
            NativeArray<byte> inputStream,
            NativeArray<byte> outputStream
        )
        {
            var job = CreateConvertTexCoordAttributeJob(
                attrData,
                inputByteStride,
                outputByteStride,
                vertexCount,
                inputStream,
                outputStream);
            while (!job.IsCompleted)
            {
                await Task.Yield();
            }
            job.Complete();
        }

        static async Task ConvertGenericAttribute(
            AttributeData attrData,
            uint inputByteStride,
            uint outputByteStride,
            int vertexCount,
            NativeArray<byte> inputStream,
            NativeArray<byte> outputStream
        )
        {
            var job = CreateConvertGenericAttributeJob(
                attrData,
                inputByteStride,
                outputByteStride,
                vertexCount,
                inputStream,
                outputStream);
            while (!job.IsCompleted)
            {
                await Task.Yield();
            }
            job.Complete();
        }

        static unsafe JobHandle CreateConvertTexCoordAttributeJob(
            AttributeData attrData,
            uint inputByteStride,
            uint outputByteStride,
            int vertexCount,
            NativeArray<byte> inputStream,
            NativeArray<byte> outputStream
        )
        {
            if (attrData.descriptor.format == VertexAttributeFormat.Float16)
            {
                return new ExportJobs.ConvertTexCoordHalfJob
                {
                    input = (byte*)inputStream.GetUnsafeReadOnlyPtr() + attrData.inputOffset,
                    inputByteStride = inputByteStride,
                    outputByteStride = outputByteStride,
                    output = (byte*)outputStream.GetUnsafePtr() + attrData.outputOffset
                }.Schedule(vertexCount, k_DefaultInnerLoopBatchCount);
            }
            return new ExportJobs.ConvertTexCoordFloatJob
            {
                input = (byte*)inputStream.GetUnsafeReadOnlyPtr() + attrData.inputOffset,
                inputByteStride = inputByteStride,
                outputByteStride = outputByteStride,
                output = (byte*)outputStream.GetUnsafePtr() + attrData.outputOffset
            }.Schedule(vertexCount, k_DefaultInnerLoopBatchCount);
        }

        static unsafe JobHandle ConvertSkinWeightsAttributeJob(
            AttributeData attrData,
            uint inputByteStride,
            uint outputByteStride,
            int vertexCount,
            NativeArray<byte> inputStream,
            NativeArray<byte> outputStream
        )
        {
            var job = new ExportJobs.ConvertSkinWeightsJob
            {
                input = (byte*)inputStream.GetUnsafeReadOnlyPtr() + attrData.inputOffset,
                inputByteStride = inputByteStride,
                outputByteStride = outputByteStride,
                output = (byte*)outputStream.GetUnsafePtr() + attrData.outputOffset,
            }.Schedule(vertexCount, k_DefaultInnerLoopBatchCount);
            return job;
        }

        static async Task ConvertSkinIndicesAttributes(
            AttributeData indicesAttrData,
            uint inputByteStride,
            uint outputByteStride,
            int vertexCount,
            NativeArray<byte> input,
            NativeArray<byte> output
        )
        {
            var job = CreateConvertSkinIndicesAttributesJob(indicesAttrData, inputByteStride, outputByteStride, vertexCount, input, output);
            while (!job.IsCompleted)
            {
                await Task.Yield();
            }
            job.Complete();
        }

        static unsafe JobHandle CreateConvertSkinIndicesAttributesJob(
            AttributeData indicesAttrData,
            uint inputByteStride,
            uint outputByteStride,
            int vertexCount,
            NativeArray<byte> input,
            NativeArray<byte> output
        )
        {
            var job = new ExportJobs.ConvertSkinIndicesJob
            {
                input = (byte*)input.GetUnsafeReadOnlyPtr(),
                inputByteStride = inputByteStride,
                outputByteStride = outputByteStride,
                indicesOffset = indicesAttrData.inputOffset,
                output = (byte*)output.GetUnsafePtr()
            }.Schedule(vertexCount, k_DefaultInnerLoopBatchCount);
            return job;
        }

        static unsafe JobHandle CreateConvertGenericAttributeJob(
            AttributeData attrData,
            uint inputByteStride,
            uint outputByteStride,
            int vertexCount,
            NativeArray<byte> inputStream,
            NativeArray<byte> outputStream
        )
        {
            var job = new ExportJobs.ConvertGenericJob
            {
                inputByteStride = inputByteStride,
                outputByteStride = outputByteStride,
                byteLength = (uint)attrData.Size,
                input = (byte*)inputStream.GetUnsafeReadOnlyPtr() + attrData.inputOffset,
                output = (byte*)outputStream.GetUnsafePtr() + attrData.outputOffset
            }.Schedule(vertexCount, k_DefaultInnerLoopBatchCount);
            return job;
        }

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

        int AddMesh(UnityEngine.Mesh uMesh, VertexAttributeUsage attributeUsage)
        {
            int meshId;
            if (!uMesh.isReadable)
            {
#if DEBUG && !UNITY_6000_0_OR_NEWER
                Debug.LogWarning($"Exporting non-readable meshes is not reliable in builds across platforms and " +
                    $"graphics APIs! Consider making mesh \"{uMesh.name}\" readable.", uMesh);
#endif
                // Unity 2020 and older does not support accessing non-readable meshes via GraphicsBuffer.
#if UNITY_2021_3_OR_NEWER
                // As of now Draco for Unity does not support encoding non-readable meshes.
                if ((m_Settings.Compression & Compression.Draco) != 0)
#endif
                {
#if UNITY_2021_3_OR_NEWER && UNITY_EDITOR
                    // Non-readable meshes are unsupported during playmode or in builds, but work in Editor exports.
                    if (Application.isPlaying)
#endif
                    {
                        m_Logger?.Error(LogCode.MeshNotReadable, uMesh.name);
                        return -1;
                    }
                }
            }

            if (m_UnityMeshes != null)
            {
                meshId = m_UnityMeshes.IndexOf(uMesh);
                if (meshId >= 0)
                {
                    SetVertexAttributeUsage(meshId, attributeUsage);
                    return meshId;
                }
            }

            var mesh = new Mesh
            {
                name = uMesh.name
            };
            m_Meshes = m_Meshes ?? new List<Mesh>();
            m_UnityMeshes = m_UnityMeshes ?? new List<UnityEngine.Mesh>();
            m_MeshVertexAttributeUsage ??= new List<VertexAttributeUsage>();
            m_Meshes.Add(mesh);
            m_UnityMeshes.Add(uMesh);
            m_MeshVertexAttributeUsage.Add(attributeUsage);
            meshId = m_Meshes.Count - 1;

            return meshId;
        }

        int AddSkin(int meshId, uint[] joints)
        {
            m_Skins ??= new List<Skin>();
            m_SkinMesh ??= new List<int>();
            var skinId = m_Skins.Count;
            var newSkin = new Skin
            {
                joints = joints
            };
            m_Skins.Add(newSkin);
            m_SkinMesh.Add(meshId);
            return skinId;
        }

        unsafe int WriteBufferViewToBuffer(byte[] bufferViewData, BufferViewTarget target, int? byteStride = null)
        {
            var bufferHandle = GCHandle.Alloc(bufferViewData, GCHandleType.Pinned);
            fixed (void* bufferAddress = &bufferViewData[0])
            {
                var nativeData = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<byte>(bufferAddress, bufferViewData.Length, Allocator.None);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                var safetyHandle = AtomicSafetyHandle.Create();
                NativeArrayUnsafeUtility.SetAtomicSafetyHandle(array: ref nativeData, safetyHandle);
#endif
                var bufferViewId = WriteBufferViewToBuffer(nativeData, target, byteStride);
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
        /// <param name="bufferViewTarget">Target of the bufferView</param>
        /// <param name="byteStride">The byte size of an element. Provide it,
        /// if it cannot be inferred from the accessor</param>
        /// <param name="byteAlignment">If not zero, the offsets of the bufferView
        /// will be multiple of it to please alignment rules (padding bytes will be added,
        /// if required; see https://www.khronos.org/registry/glTF/specs/2.0/glTF-2.0.html#data-alignment )
        /// </param>
        /// <returns>Buffer view index</returns>
        int WriteBufferViewToBuffer(NativeArray<byte> bufferViewData, BufferViewTarget bufferViewTarget, int? byteStride = null, int byteAlignment = 0)
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
                target = (int)bufferViewTarget,
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

        void SetVertexAttributeUsage(int meshId, VertexAttributeUsage attributeUsage)
        {
            var existingUsage = m_MeshVertexAttributeUsage[meshId];
            if (((existingUsage ^ attributeUsage) & VertexAttributeUsage.Color) == VertexAttributeUsage.Color)
            {
                m_Logger.Warning(LogCode.InconsistentVertexColorUsage, meshId.ToString());
            }
            m_MeshVertexAttributeUsage[meshId] = attributeUsage | existingUsage;
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
            m_MeshVertexAttributeUsage = null;
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

        static unsafe int GetAttributeSize(VertexAttributeFormat format)
        {
            switch (format)
            {
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

        static VertexAttributeUsage GetVertexAttributeUsage(Shader shader)
        {
            var shaderName = shader.name;
            if (shaderName.EndsWith("unlit", StringComparison.InvariantCultureIgnoreCase))
            {
                return VertexAttributeUsage.Position
                    // Only two UV channels
                    | VertexAttributeUsage.TwoTexCoords
                    | VertexAttributeUsage.Color
                    | VertexAttributeUsage.Skinning;
            }
            if (shaderName.StartsWith("Shader Graphs/glTF-", StringComparison.InvariantCulture)
                || shaderName.StartsWith("glTF/", StringComparison.InvariantCulture)
                || shaderName.StartsWith("Particles/Standard", StringComparison.InvariantCulture)
                )
            {
                return VertexAttributeUsage.Position
                    | VertexAttributeUsage.Normal
                    | VertexAttributeUsage.Tangent
                    // Only two UV channels
                    | VertexAttributeUsage.TwoTexCoords
                    | VertexAttributeUsage.Color
                    | VertexAttributeUsage.Skinning;
            }
            // Note: No vertex colors. Most shaders don't make use of them, so discard them by default.
            return VertexAttributeUsage.Position
                | VertexAttributeUsage.Normal
                | VertexAttributeUsage.Tangent
                | VertexAttributeUsage.AllTexCoords
                | VertexAttributeUsage.Skinning;
        }
    }
}
