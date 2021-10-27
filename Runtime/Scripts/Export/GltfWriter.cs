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
using GLTFast.Schema;
using JetBrains.Annotations;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using Buffer = GLTFast.Schema.Buffer;
using Debug = UnityEngine.Debug;
using Material = GLTFast.Schema.Material;
using Mesh = GLTFast.Schema.Mesh;
using Texture = GLTFast.Schema.Texture;

#if DEBUG
using System.Text;
#endif
#if UNITY_EDITOR
using UnityEditor;
#endif

[assembly: InternalsVisibleTo("glTFastEditor")]
[assembly: InternalsVisibleTo("glTF-test-framework.Tests")]

namespace GLTFast.Export {

    public class GltfWriter : IGltfWritable {

        enum State {
            Initialized,
            ContentAdded,
            Disposed
        }
        
#region Constants
        const int k_MAXStreamCount = 4;
        const int k_DefaultInnerLoopBatchCount = 512;
#endregion Constants

#region Private
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
        List<Accessor> m_Accessors;
        List<BufferView> m_BufferViews;

        List<UnityEngine.Material> m_UnityMaterials;
        List<UnityEngine.Mesh> m_UnityMeshes;
        List<UnityEngine.Texture> m_UnityTextures;
        Dictionary<int, int[]> m_NodeMaterials;
        Dictionary<int, string> m_ImagePathsToAdd;

        Stream m_BufferStream;
        string m_BufferPath;
#endregion Private

        /// <summary>
        /// Provides glTF export independent of workflow (GameObjects/Entities)
        /// </summary>
        /// <param name="exportSettings">Export settings</param>
        /// <param name="deferAgent">Defer agent; decides when/if to preempt
        /// export to preserve a stable frame rate <seealso cref="IDeferAgent"/></param>
        /// <param name="logger">Interface for logging (error) messages
        /// <seealso cref="ConsoleLogger"/></param>
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

        /// <summary>
        /// Adds a node to the glTF
        /// </summary>
        /// <param name="translation">Local translation of the node (in Unity-space)</param>
        /// <param name="rotation">Local rotation of the node (in Unity-space)</param>
        /// <param name="scale">Local scale of the node (in Unity-space)</param>
        /// <param name="children">Array of node indices that are parented to
        /// this newly created node</param>
        /// <param name="name">Name of the node</param>
        /// <returns>glTF node index</returns>
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
            var node = new Node {
                name = name,
                children = children,
            };
            if( translation.HasValue && !translation.Equals(float3.zero) ) {
                node.translation = new[] { -translation.Value.x, translation.Value.y, translation.Value.z };
            }
            if( rotation.HasValue && !rotation.Equals(quaternion.identity) ) {
                node.rotation = new[] { rotation.Value.value.x, -rotation.Value.value.y, -rotation.Value.value.z, rotation.Value.value.w };
            }
            if( scale.HasValue && !scale.Equals(new float3(1f)) ) {
                node.scale = new[] { scale.Value.x, scale.Value.y, scale.Value.z };
            }
            m_Nodes = m_Nodes ?? new List<Node>();
            m_Nodes.Add(node);
            return (uint) m_Nodes.Count - 1;
        }
        
        /// <summary>
        /// Assigns a mesh to a previously added node
        /// </summary>
        /// <param name="nodeId">Index of the node to add the mesh to</param>
        /// <param name="uMesh">Unity mesh to be assigned and exported</param>
        /// <param name="uMaterials">Materials to be assigned and exported
        /// (multiple in case of sub-meshes)</param>
        /// <returns>True if the conversion was flawless, false otherwise (use
        /// <see cref="m_Logger"/> for analysing errors)</returns>
        public bool AddMeshToNode(int nodeId, [NotNull] UnityEngine.Mesh uMesh, List<UnityEngine.Material> uMaterials) {
            CertifyNotDisposed();
            var node = m_Nodes[nodeId];

            var success = true;
            if (uMaterials != null && uMaterials.Count > 0) {
                var materialIds = new int[uMaterials.Count];
                for (var i = 0; i < uMaterials.Count; i++) {
                    var uMaterial = uMaterials[i];
                    if (uMaterial == null) {
                        materialIds[i] = -1;
                    } else { 
                        success &= AddMaterial(uMaterial, out materialIds[i]);
                    }
                }
                m_NodeMaterials = m_NodeMaterials ?? new Dictionary<int, int[]>();
                m_NodeMaterials[nodeId] = materialIds;
            }

            node.mesh = AddMesh(uMesh);
            return success;
        }

        /// <summary>
        /// Adds a scene to the glTF
        /// </summary>
        /// <param name="nodes">Root level nodes</param>
        /// <param name="name">Name of the scene</param>
        /// <returns>glTF scene index</returns>
        public uint AddScene(uint[] nodes, string name = null) {
            CertifyNotDisposed();
            m_Scenes = m_Scenes ?? new List<Scene>();
            var scene = new Scene {
                name = name,
                nodes = nodes
            };
            m_Scenes.Add(scene);
            if (m_Scenes.Count == 1) {
                m_Gltf.scene = 0;
            }
            return (uint) m_Scenes.Count - 1;
        }

        public int AddImage( UnityEngine.Texture uTexture ) {
            CertifyNotDisposed();
            int imageId;
            if (m_UnityTextures != null) {
                imageId = m_UnityTextures.IndexOf(uTexture);
                if (imageId >= 0) {
                    return imageId;
                }
            } else {
                m_UnityTextures = new List<UnityEngine.Texture>();
                m_Images = new List<Image>();
            }

            imageId = m_UnityTextures.Count;

            // TODO: Create sampler, if required
            // TODO: KTX encoding

#if UNITY_EDITOR

            var assetPath = AssetDatabase.GetAssetPath(uTexture);
            if (File.Exists(assetPath)) {
                var mimeType = GetMimeType(assetPath);
                if (!string.IsNullOrEmpty(mimeType)) {
                    var image = new Image {
                        name = uTexture.name,
                        mimeType = mimeType
                    };

                    m_ImagePathsToAdd = m_ImagePathsToAdd ?? new Dictionary<int, string>();
                    m_ImagePathsToAdd[imageId] = assetPath;
                    
                    m_UnityTextures.Add(uTexture);
                    m_Images.Add(image);
                } else {
                    m_Logger?.Error(LogCode.ImageFormatUnknown,uTexture.name,assetPath);
                    return -1;
                }
            }
#else
            throw new NotImplementedException("Exporting textures at runtime is not yet implemented");
#endif

            return imageId;
        }

        public int AddTexture(int imageId) {
            CertifyNotDisposed();
            m_Textures = m_Textures ?? new List<Texture>();
            
            var texture = new Texture {
                source = imageId
            };
            m_Textures.Add(texture);
            return m_Textures.Count - 1;
        }

        public void RegisterExtensionUsage(Extension extension, bool required = true) {
            CertifyNotDisposed();
            if (required) {
                m_ExtensionsRequired = m_ExtensionsRequired ?? new HashSet<Extension>();
                m_ExtensionsRequired.Add(extension);
            } else {
                if (m_ExtensionsRequired == null || !m_ExtensionsRequired.Contains(extension)) {
                    m_ExtensionsUsedOnly = m_ExtensionsUsedOnly ?? new HashSet<Extension>();
                    m_ExtensionsUsedOnly.Add(extension);
                }
            }
        }
        
        /// <summary>
        /// Exports the collected scenes/content as glTF and disposes this object.
        /// After the export this instance cannot be re-used!
        /// </summary>
        /// <param name="path">glTF destination file path</param>
        /// <returns>True if the glTF file was created successfully, false otherwise</returns>
        public async Task<bool> SaveToFileAndDispose(string path) {
            CertifyNotDisposed();
#if DEBUG
            if (m_State != State.ContentAdded) {
                Debug.LogWarning("Exporting empty glTF");
            }
#endif
            var ext = Path.GetExtension(path);
            var binary = m_Settings.format == GltfFormat.Binary;
            m_BufferPath = null;
            if (!binary) {
                if (string.IsNullOrEmpty(ext)) {
                    m_BufferPath = path + ".bin";
                } else {
                    m_BufferPath = path.Substring(0, path.Length - ext.Length) + ".bin";
                }
            }

            var success = await Bake(Path.GetFileName(m_BufferPath), Path.GetDirectoryName(path));

            if (!success) {
                m_BufferStream?.Close();
                Dispose();
                return false;
            }
            
            var json = GetJson();
            LogSummary(json.Length, m_BufferStream?.Length ?? 0);

            if (binary) {
                const uint headerSize = 12; // 4 bytes magic + 4 bytes version + 4 bytes length (uint each)
                const uint chunkOverhead = 8; // 4 bytes chunk length + 4 bytes chunk type (uint each)
                var glb = new FileStream(path,FileMode.Create);
                glb.Write(BitConverter.GetBytes(GltfGlobals.GLB_MAGIC));
                glb.Write(BitConverter.GetBytes((uint)2));

                var jsonPad = GetPadByteCount((uint)json.Length);
                var binPad = 0;
                var totalLength = (uint) (headerSize + chunkOverhead + json.Length + jsonPad);
                var hasBufferContent = (m_BufferStream?.Length ?? 0) > 0; 
                if (hasBufferContent) {
                    binPad = GetPadByteCount((uint)m_BufferStream.Length);
                    totalLength += (uint) (chunkOverhead + m_BufferStream.Length + binPad);
                }
                
                glb.Write(BitConverter.GetBytes(totalLength));
                
                glb.Write(BitConverter.GetBytes((uint)(json.Length+jsonPad)));
                glb.Write(BitConverter.GetBytes((uint)ChunkFormat.JSON));
                var sw = new StreamWriter(glb);
                sw.Write(json);
                for (int i = 0; i < jsonPad; i++) {
                    sw.Write(' ');
                }
                sw.Flush();

                if (hasBufferContent) {
                    glb.Write(BitConverter.GetBytes((uint)(m_BufferStream.Length+binPad)));
                    glb.Write(BitConverter.GetBytes((uint)ChunkFormat.BIN));
                    var ms = (MemoryStream)m_BufferStream;
                    ms.WriteTo(glb);
                    ms.Flush();
                    for (int i = 0; i < binPad; i++) {
                        sw.Write('\0');
                    }
                }
                sw.Close();
                glb.Close();
            }
            else {
                File.WriteAllText(path,json);
            }

            Dispose();
            return true;
        }

        void CertifyNotDisposed() {
            if (m_State == State.Disposed) {
                throw new InvalidOperationException("GltfWriter was already disposed");
            }
        }

        static string GetMimeType(string assetPath) {
            string mimeType = null;
            if (assetPath.EndsWith(".png", StringComparison.OrdinalIgnoreCase)) {
                mimeType = "image/png";
            }
            else if (assetPath.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                assetPath.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)) {
                mimeType = "image/jpeg";
            }

            return mimeType;
        }

        ImageDestination GetFinalImageDestination() {
            var imageDest = m_Settings.imageDestination;
            if (imageDest == ImageDestination.Automatic) {
                imageDest = m_Settings.format == GltfFormat.Binary
                    ? ImageDestination.MainBuffer
                    : ImageDestination.SeparateFile;
            }

            return imageDest;
        }

        bool AddMaterial(UnityEngine.Material uMaterial, out int materialId) {

            if (m_Materials!=null) {
                materialId = m_UnityMaterials.IndexOf(uMaterial);
                if (materialId >= 0) {
                    return true;
                }
            } else {
                m_Materials = new List<Material>();    
                m_UnityMaterials = new List<UnityEngine.Material>();    
            }
            
            var success = StandardMaterialExport.ConvertMaterial(uMaterial, out var material, this, m_Logger);

            materialId = m_Materials.Count;
            m_Materials.Add(material);
            m_UnityMaterials.Add(uMaterial);
            return success;
        }
        
        int GetPadByteCount(uint length) {
            return (4 - (int)(length & 3) ) & 3;
        }

        [Conditional("DEBUG")]
        void LogSummary(int jsonLength, long bufferLength) {
#if DEBUG
            var sb = new StringBuilder("glTF summary: ");
            sb.AppendFormat("{0} bytes JSON + {1} bytes buffer", jsonLength, bufferLength);
            if (m_Gltf != null) {
                sb.AppendFormat(", {0} nodes", m_Gltf.nodes?.Length ?? 0);
                sb.AppendFormat(" ,{0} meshes", m_Gltf.meshes?.Length ?? 0);
                sb.AppendFormat(" ,{0} materials", m_Gltf.materials?.Length ?? 0);
                sb.AppendFormat(" ,{0} images", m_Gltf.images?.Length ?? 0);
            }
            m_Logger?.Info(sb.ToString());
#endif
        }

        async Task<bool> Bake(string bufferPath, string directory) {
            if (m_Meshes != null) {
#if GLTFAST_MESH_DATA
                await BakeMeshes();
#else
                throw new NotImplementedException("glTF export (containing meshes) is currently not supported on Unity 2020.1 and older");
#endif
            }

            AssignMaterialsToMeshes();

            var success = await BakeImages(directory);

            if (!success) return false;
            
            if (m_BufferStream != null && m_BufferStream.Length > 0) {
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

            m_Gltf.asset = new Asset {
                version = "2.0",
                generator = $"Unity {Application.unityVersion} glTFast {Constants.version}"
            };
            
            BakeExtensions();
            return true;
        }

        void BakeExtensions() {
            if (m_ExtensionsRequired != null) {
                var usedOnlyCount = m_ExtensionsUsedOnly == null ? 0 : m_ExtensionsUsedOnly.Count;
                m_Gltf.extensionsRequired = new string[m_ExtensionsRequired.Count];
                m_Gltf.extensionsUsed = new string[m_ExtensionsRequired.Count + usedOnlyCount];
                var i = 0;
                foreach (var extension in m_ExtensionsRequired) {
                    var name = extension.GetName();
                    m_Gltf.extensionsRequired[i] = name;
                    m_Gltf.extensionsUsed[i] = name;
                    i++;
                }
            }

            if (m_ExtensionsUsedOnly != null) {
                var i = 0;
                if (m_Gltf.extensionsUsed == null) {
                    m_Gltf.extensionsUsed = new string[m_ExtensionsUsedOnly.Count];
                }
                else {
                    i = m_Gltf.extensionsUsed.Length - m_ExtensionsUsedOnly.Count;
                }

                foreach (var extension in m_ExtensionsUsedOnly) {
                    m_Gltf.extensionsUsed[i++] = extension.GetName();
                }
            }
        }

        void AssignMaterialsToMeshes() {
            if (m_NodeMaterials != null && m_Meshes != null) {
                var meshMaterialCombos = new Dictionary<MeshMaterialCombination, int>(m_Meshes.Count);
                var originalCombos = new Dictionary<int, MeshMaterialCombination>(m_Meshes.Count);
                foreach (var nodeMaterial in m_NodeMaterials) {
                    var nodeId = nodeMaterial.Key;
                    var materialIds = nodeMaterial.Value;
                    var node = m_Nodes[nodeId];
                    var originalMeshId = node.mesh;
                    var mesh = m_Meshes[originalMeshId];

                    var meshMaterialCombo = new MeshMaterialCombination(originalMeshId, materialIds);

                    if (!originalCombos.ContainsKey(originalMeshId)) {
                        // First usage of the original -> assign materials to original
                        AssignMaterialsToMesh(materialIds, mesh);
                        originalCombos[originalMeshId] = meshMaterialCombo;
                        meshMaterialCombos[meshMaterialCombo] = originalMeshId;
                    } else {
                        // Mesh is re-used -> check if this exact materials set was used before
                        if (meshMaterialCombos.TryGetValue(meshMaterialCombo, out var meshId)) {
                            // Materials are identical -> re-use Mesh object
                            node.mesh = meshId;
                        } else {
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

        static void AssignMaterialsToMesh(int[] materialIds, Mesh mesh) {
            for (var i = 0; i < materialIds.Length; i++) {
                mesh.primitives[i].material = materialIds[i] >= 0 ? materialIds[i] : -1;
            }
        }

        int DuplicateMesh(int meshId) {
            var src = m_Meshes[meshId];
            var copy = (Mesh)src.Clone();
            m_Meshes.Add(copy);
            return m_Meshes.Count - 1;
        }

#if GLTFAST_MESH_DATA

        async Task BakeMeshes() {
            Profiler.BeginSample("BakeMeshes");
            Profiler.BeginSample("AcquireReadOnlyMeshData");
            var meshDataArray = UnityEngine.Mesh.AcquireReadOnlyMeshData(m_UnityMeshes);
            Profiler.EndSample();
            for (var meshId = 0; meshId < m_Meshes.Count; meshId++) {
                await BakeMesh(meshId, meshDataArray[meshId]);
                await m_DeferAgent.BreakPoint();
            }
            meshDataArray.Dispose();
            Profiler.EndSample();
        }

        async Task BakeMesh(int meshId, UnityEngine.Mesh.MeshData meshData) {
            
            Profiler.BeginSample("BakeMesh");
            
            var mesh = m_Meshes[meshId];
            var uMesh = m_UnityMeshes[meshId];

            var vertexAttributes = uMesh.GetVertexAttributes();
            var strides = new int[k_MAXStreamCount];
            var alignments = new int[k_MAXStreamCount];

            var attributes = new Attributes();
            var vertexCount = uMesh.vertexCount;
            var attrDataDict = new Dictionary<VertexAttribute, AttributeData>();
            
            foreach (var attribute in vertexAttributes) {
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
                    typeEnum = Accessor.GetAccessorAttributeType(attribute.dimension),
                };
                
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
            
            var indexComponentType = uMesh.indexFormat == IndexFormat.UInt16 ? GLTFComponentType.UnsignedShort : GLTFComponentType.UnsignedInt;
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

                Accessor indexAccessor;
                
                indexAccessor = new Accessor {
                    typeEnum = GLTFAccessorAttributeType.SCALAR,
                    byteOffset = indexOffset,
                    componentType = indexComponentType,
                    count = subMesh.indexCount,

                    // min = new []{}, // TODO
                    // max = new []{}, // TODO
                };
                
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

            Profiler.BeginSample("ScheduleIndexJob");
            int indexBufferViewId;
            if (uMesh.indexFormat == IndexFormat.UInt16) {
                var indexData16 = meshData.GetIndexData<ushort>();
                if (topology.Value == MeshTopology.Quads) {
                    var quadCount = indexData16.Length / 4;
                    var destIndices = new NativeArray<ushort>(quadCount*6,Allocator.TempJob);
                    var job = new ExportJobs.ConvertIndicesQuadFlippedJob<ushort> {
                        input = indexData16,
                        result = destIndices
                    }.Schedule(quadCount, k_DefaultInnerLoopBatchCount);
                    while (!job.IsCompleted) {
                        await Task.Yield();
                    }
                    job.Complete(); // TODO: Wait until thread is finished
                    indexBufferViewId = WriteBufferViewToBuffer(
                        destIndices.Reinterpret<byte>(sizeof(ushort)),
                        byteAlignment:sizeof(ushort)
                        );
                    destIndices.Dispose();
                } else {
                    var triangleCount = indexData16.Length / 3;
                    var destIndices = new NativeArray<ushort>(indexData16.Length,Allocator.TempJob);
                    var job = new ExportJobs.ConvertIndicesFlippedJob<ushort> {
                        input = indexData16,
                        result = destIndices
                    }.Schedule(triangleCount, k_DefaultInnerLoopBatchCount);
                    while (!job.IsCompleted) {
                        await Task.Yield();
                    }
                    job.Complete(); // TODO: Wait until thread is finished
                    indexBufferViewId = WriteBufferViewToBuffer(
                        destIndices.Reinterpret<byte>(sizeof(ushort)),
                        byteAlignment:sizeof(ushort)
                        );
                    destIndices.Dispose();
                }
            } else {
                var indexData32 = meshData.GetIndexData<uint>();
                if (topology.Value == MeshTopology.Quads) {
                    var quadCount = indexData32.Length / 4;
                    var destIndices = new NativeArray<uint>(quadCount*6,Allocator.TempJob);
                    var job = new ExportJobs.ConvertIndicesQuadFlippedJob<uint> {
                        input = indexData32,
                        result = destIndices
                    }.Schedule(quadCount, k_DefaultInnerLoopBatchCount);
                    while (!job.IsCompleted) {
                        await Task.Yield();
                    }
                    job.Complete(); // TODO: Wait until thread is finished
                    indexBufferViewId = WriteBufferViewToBuffer(
                        destIndices.Reinterpret<byte>(sizeof(uint)),
                        byteAlignment:sizeof(uint)
                        );
                    destIndices.Dispose();
                } else {
                    var triangleCount = indexData32.Length / 3;
                    var destIndices = new NativeArray<uint>(indexData32.Length, Allocator.TempJob);
                    var job = new ExportJobs.ConvertIndicesFlippedJob<uint> {
                        input = indexData32,
                        result = destIndices
                    }.Schedule(triangleCount, k_DefaultInnerLoopBatchCount);
                    while (!job.IsCompleted) {
                        await Task.Yield();
                    }
                    job.Complete(); // TODO: Wait until thread is finished
                    indexBufferViewId = WriteBufferViewToBuffer(
                        destIndices.Reinterpret<byte>(sizeof(uint)),
                        byteAlignment:sizeof(uint)
                        );
                    destIndices.Dispose();
                }
            }
            Profiler.EndSample();

            foreach (var accessor in indexAccessors) {
                accessor.bufferView = indexBufferViewId;
            }

            var inputStreams = new NativeArray<byte>[streamCount];
            var outputStreams = new NativeArray<byte>[streamCount];
            
            for (var stream = 0; stream < streamCount; stream++) {
                inputStreams[stream] = meshData.GetVertexData<byte>(stream);
                outputStreams[stream] = new NativeArray<byte>(inputStreams[stream], Allocator.TempJob);
            }

            Profiler.BeginSample("ScheduleVertexJob");
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
                }
            }
            Profiler.EndSample();

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
            
            Profiler.EndSample();
        }

        int AddAccessor(Accessor accessor) {
            m_Accessors = m_Accessors ?? new List<Accessor>();
            var accessorId = m_Accessors.Count;
            m_Accessors.Add(accessor);
            return accessorId;
        }

#endif // #if GLTFAST_MESH_DATA

        async Task<bool> BakeImages(string directory) {
            if (m_ImagePathsToAdd != null) {
                var imageDest = GetFinalImageDestination();
                var overwrite = m_Settings.fileConflictResolution == FileConflictResolution.Overwrite;
                if (!overwrite && imageDest == ImageDestination.SeparateFile) {
                    var fileExists = false;
                    foreach (var pair in m_ImagePathsToAdd) {
                        var assetPath = pair.Value;
                        var fileName = Path.GetFileName(assetPath);
                        var destPath = Path.Combine(directory,fileName);
                        if (File.Exists(destPath)) {
                            fileExists = true;
                            break;
                        }
                    }

                    if (fileExists) {
#if UNITY_EDITOR
                        overwrite = EditorUtility.DisplayDialog(
                            "Image file conflicts",
                            "Some image files at the destination will be overwritten",
                            "Overwrite", "Cancel");
                        if (!overwrite) {
                            return false;
                        }
#else
                        if (m_Settings.fileConflictResolution == FileConflictResolution.Abort) {
                            return false;
                        }
#endif
                    }
                }

                foreach (var pair in m_ImagePathsToAdd) {
                    var imageId = pair.Key;
                    var assetPath = pair.Value;
                    if (imageDest == ImageDestination.MainBuffer) {
                        // TODO: Write from file to buffer stream directly
                        var imageBytes = File.ReadAllBytes(assetPath);
                        m_Images[imageId].bufferView = WriteBufferViewToBuffer(imageBytes); 
                    } else if (imageDest == ImageDestination.SeparateFile) {
                        var fileName = Path.GetFileName(assetPath);
                        File.Copy(assetPath, Path.Combine(directory,fileName), overwrite);
                        m_Images[imageId].uri = fileName;
                    }
                    await m_DeferAgent.BreakPoint();
                }
            }

            m_ImagePathsToAdd = null;
            return true;
        }
        
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
            job.Complete(); // TODO: Wait until thread is finished
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
            job.Complete(); // TODO: Wait until thread is finished
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

        static DrawMode? GetDrawMode(MeshTopology topology) {
            switch (topology) {
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

        string GetJson() {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            m_Gltf.GltfSerialize(writer);
            writer.Flush();
            stream.Seek(0,SeekOrigin.Begin);
            var reader = new StreamReader( stream );
            var json = reader.ReadToEnd();
            reader.Close();
            return json;
        }
        
        int AddMesh([NotNull] UnityEngine.Mesh uMesh) {
            int meshId;
            
#if !UNITY_EDITOR
            if (!uMesh.isReadable) {
                m_Logger?.Error(LogCode.MeshNotReadable, uMesh.name);
                return -1;
            }
#endif
       
            if (m_UnityMeshes!=null) {
                meshId = m_UnityMeshes.IndexOf(uMesh);
                if (meshId >= 0) {
                    return meshId;
                }
            }

            var mesh = new Mesh {
                name = uMesh.name
            };
            m_Meshes = m_Meshes ?? new List<Mesh>();
            m_UnityMeshes = m_UnityMeshes ?? new List<UnityEngine.Mesh>();
            m_Meshes.Add(mesh);
            m_UnityMeshes.Add(uMesh);
            meshId = m_Meshes.Count - 1;
            return meshId;
        }

        unsafe int WriteBufferViewToBuffer( byte[] bufferViewData, int? byteStride = null) {
            var bufferHandle = GCHandle.Alloc(bufferViewData,GCHandleType.Pinned);
            fixed (void* bufferAddress = &bufferViewData[0]) {
                var nativeData = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<byte>(bufferAddress,bufferViewData.Length,Allocator.None);
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

        Stream CertifyBuffer() {
            if (m_BufferStream == null) {
                // Delayed, implicit stream generation.
                // if `m_BufferPath` was set, we need a FileStream 
                if (m_BufferPath != null) {
                    m_BufferStream = new FileStream(m_BufferPath,FileMode.Create);
                } else {
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
        int WriteBufferViewToBuffer(NativeArray<byte> bufferViewData, int? byteStride = null, int byteAlignment = 0) {
            Profiler.BeginSample("WriteBufferViewToBuffer");
            var buffer = CertifyBuffer();
            var byteOffset = buffer.Length;

            if (byteAlignment > 0) {
                Assert.IsTrue(byteAlignment<5); // There is no componentType that requires more than 4 bytes
                var alignmentByteCount = (byteAlignment-(byteOffset % byteAlignment)) % byteAlignment;
                for (var i = 0; i < alignmentByteCount; i++) {
                    buffer.WriteByte(0);
                }
                // Update byteOffset
                byteOffset = buffer.Length;
            }
            
            buffer.Write(bufferViewData);
            
            var bufferView = new BufferView {
                buffer = 0,
                byteOffset = (int)byteOffset,
                byteLength = bufferViewData.Length,
            };
            if (byteStride.HasValue) {
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

        void Dispose() {
            m_Settings = null;

            m_Logger = null;
            m_Gltf = null;
            m_ExtensionsUsedOnly = null;
            m_ExtensionsRequired = null;
            m_UnityMaterials = null;
            m_UnityMeshes = null;
            m_UnityTextures = null;
            m_NodeMaterials = null;
            m_ImagePathsToAdd = null;
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
            
            m_State = State.Disposed;
        }
        
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
        
        internal struct MeshMaterialCombination {
            readonly int m_MeshId;
            readonly int[] m_MaterialIds;
            
            public MeshMaterialCombination(int meshId, int[] materialIds) {
                m_MeshId = meshId;
                m_MaterialIds = materialIds;
            }
            
            public override bool Equals(object obj) {
                //Check for null and compare run-time types.
                if (obj == null || ! GetType().Equals(obj.GetType())) {
                    return false;
                }
                return Equals((MeshMaterialCombination)obj);
            }

            bool Equals(MeshMaterialCombination other) {
                return m_MeshId == other.m_MeshId && Equals(m_MaterialIds, other.m_MaterialIds);
            }

            static bool Equals(int[] a, int[] b) {
                if (a == null && b == null) {
                    return true;
                }
                if (a == null ^ b == null) {
                    return false;
                }
                if (a.Length != b.Length) {
                    return false;
                }
                for (var i = 0; i < a.Length; i++) {
                    if (a[i] != b[i]) {
                        return false;
                    }
                }
                return true;
            }

            public override int GetHashCode() {
#if NET_STANDARD
                return HashCode.Combine(m_MeshId, m_MaterialIds);
#else
                var hash = 17;
                hash = hash * 31 + m_MeshId.GetHashCode();
                hash = hash * 31 + m_MaterialIds.GetHashCode();
                return hash;
#endif
            }
        }
    }
    
    struct AttributeData {
        public int stream;
        public int offset;
        public int accessorId;
    }
}
