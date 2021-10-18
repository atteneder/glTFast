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
using GLTFast.Schema;
using JetBrains.Annotations;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;
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

[assembly: InternalsVisibleTo("glTFast.Tests")]

namespace GLTFast.Export {
    public class GltfWriter : IGltfWritable {
        
        const int k_MAXStreamCount = 4;
        const int k_DefaultInnerLoopBatchCount = 512;
        
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

        MemoryStream m_BufferStream;

        Stream bufferWriter => m_BufferStream = m_BufferStream ?? new MemoryStream();

        public GltfWriter() {
            m_Gltf = new Root();
        }

        public void RegisterExtensionUsage(Extension extension, bool required = true) {
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
        
        public uint AddScene(string name, uint[] nodes) {
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
        
        public uint AddNode(
            string name,
            float3 translation,
            quaternion rotation,
            float3 scale,
            uint[] children
            )
        {
            var node = new Node {
                name = name,
                children = children,
            };
            if( !translation.Equals(float3.zero) ) {
                node.translation = new[] { -translation.x, translation.y, translation.z };
            }
            if( !rotation.Equals(quaternion.identity) ) {
                node.rotation = new[] { rotation.value.x, -rotation.value.y, -rotation.value.z, rotation.value.w };
            }
            if( !scale.Equals(new float3(1f)) ) {
                node.scale = new[] { scale.x, scale.y, scale.z };
            }
            m_Nodes = m_Nodes ?? new List<Node>();
            m_Nodes.Add(node);
            return (uint) m_Nodes.Count - 1;
        }
        
        public void AddMeshToNode(uint nodeId, [NotNull] UnityEngine.Mesh uMesh, List<UnityEngine.Material> uMaterials) {
            var node = m_Nodes[(int)nodeId];

            if (uMaterials != null && uMaterials.Count > 0) {
                var materialIds = new int[uMaterials.Count];
                for (var i = 0; i < uMaterials.Count; i++) {
                    var uMaterial = uMaterials[i];
                    materialIds[i] = uMaterial==null ? -1 : AddMaterial(uMaterial);
                }
                m_NodeMaterials = m_NodeMaterials ?? new Dictionary<int, int[]>();
                m_NodeMaterials[(int)nodeId] = materialIds;
            }

            node.mesh = AddMesh(uMesh);
        }

        int AddMaterial(UnityEngine.Material uMaterial) {

            var materialId = -1;
            if (m_Materials!=null) {
                materialId = m_UnityMaterials.IndexOf(uMaterial);
                if (materialId >= 0) {
                    return materialId;
                }
            } else {
                m_Materials = new List<Material>();    
                m_UnityMaterials = new List<UnityEngine.Material>();    
            }
            
            var material = StandardMaterialExport.ConvertMaterial(uMaterial, this);

            materialId = m_Materials.Count;
            m_Materials.Add(material);
            m_UnityMaterials.Add(uMaterial);
            return materialId;
        }

        
        public int AddImage( UnityEngine.Texture uTexture ) {
            var imageId = -1;
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

            // TODO: Create sampler
            // TODO: Save as external file
            // TODO: KTX encoding

#if UNITY_EDITOR
            var assetPath = AssetDatabase.GetAssetPath(uTexture);
            if (File.Exists(assetPath)) {
                var mimeType = GetMimeType(assetPath);
                if (!string.IsNullOrEmpty(mimeType)) {
                    var image = new Image {
                        name = uTexture.name
                    };
                    var imageData = File.ReadAllBytes(assetPath);
                    image.bufferView = WriteBufferViewToBuffer(imageData);
                    image.mimeType = mimeType;
                    m_UnityTextures.Add(uTexture);
                    m_Images.Add(image);
                } else {
                    Debug.LogError($"Could not determine type of image {assetPath}");
                }
            }
#else
            throw new NotImplementedException("Exporting textures at runtime is not yet implemented");
#endif

            return imageId;
        }

        public int AddTexture(int imageId) {
            m_Textures = m_Textures ?? new List<Texture>();
            
            var texture = new Texture {
                source = imageId
            };
            m_Textures.Add(texture);
            return m_Textures.Count - 1;
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

        public void SaveToFile(string path) {
            
            var ext = Path.GetExtension(path);
            string bufferPath;
            if (string.IsNullOrEmpty(ext)) {
                bufferPath = path + ".bin";
            } else {
                bufferPath = path.Substring(0, path.Length - ext.Length) + ".bin";
            }
            Bake(Path.GetFileName(bufferPath));
            var json = GetJson();
            LogSummary(json.Length, m_BufferStream?.Length ?? 0);
            File.WriteAllText(path,json);
            if (m_BufferStream != null) {
                using (var file = new FileStream(bufferPath, FileMode.Create, FileAccess.Write)) {
                    m_BufferStream.WriteTo(file);
                }
            }
        }

#if DEBUG
        [Conditional("DEBUG")]
        void LogSummary(int jsonLength, long bufferLength) {
            var sb = new StringBuilder("glTF summary: ");
            sb.AppendFormat("{0} bytes JSON + {1} bytes buffer", jsonLength, bufferLength);
            if (m_Gltf != null) {
                sb.AppendFormat(", {0} nodes", m_Gltf.nodes?.Length ?? 0);
                sb.AppendFormat(" ,{0} meshes", m_Gltf.meshes?.Length ?? 0);
                sb.AppendFormat(" ,{0} materials", m_Gltf.materials?.Length ?? 0);
                sb.AppendFormat(" ,{0} images", m_Gltf.images?.Length ?? 0);
            }
            Debug.Log(sb.ToString());
        }
#endif

        void Bake(string bufferPath) {
            if (m_Meshes != null) {
#if GLTFAST_MESH_DATA
                BakeMeshes();
#else
                throw new NotImplementedException("glTF export (containing meshes) is currently not supported on Unity 2020.1 and older");
#endif
            }

            AssignMaterialsToMeshes();

            if (m_BufferStream != null) {
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
                generator = "glTFast 4.4.0-exp"
            };
            
            BakeExtensions();

            m_Scenes = null;
            m_Nodes = null;
            m_Meshes = null;
            m_Accessors = null;
            m_BufferViews = null;
            m_Materials = null;
            m_Images = null;
            m_Textures = null;
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

                    var meshMaterialCombo = new MeshMaterialCombination {
                        meshId = originalMeshId,
                        materialIds = materialIds,
                    };

                    if (!originalCombos.TryGetValue(originalMeshId, out var originalCombo)) {
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

        void BakeMeshes() {
            var meshDataArray = UnityEngine.Mesh.AcquireReadOnlyMeshData(m_UnityMeshes);
            for (var meshId = 0; meshId < m_Meshes.Count; meshId++) {
                BakeMesh(meshId, meshDataArray[meshId]);
            }
            meshDataArray.Dispose();
        }

        void BakeMesh(int meshId, UnityEngine.Mesh.MeshData meshData) {

            var mesh = m_Meshes[meshId];
            var uMesh = m_UnityMeshes[meshId];

            var vertexAttributes = uMesh.GetVertexAttributes();
            var strides = new int[k_MAXStreamCount];

            var attributes = new Attributes();
            var vertexCount = uMesh.vertexCount;
            var attrDataDict = new Dictionary<VertexAttribute, AttributeData>();
            
            foreach (var attribute in vertexAttributes) {
                var attrData = new AttributeData {
                    offset = strides[attribute.stream],
                    stream = attribute.stream
                };
                
                var size = attribute.dimension * GetAttributeSize(attribute.format);
                strides[attribute.stream] += size;

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
                    Debug.LogError($"Unsupported topology {subMesh.topology}");
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
                    job.Complete(); // TODO: Wait until thread is finished
                    indexBufferViewId = WriteBufferViewToBuffer(
                        destIndices.Reinterpret<byte>(sizeof(uint)),
                        byteAlignment:sizeof(uint)
                        );
                    destIndices.Dispose();
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
                        ConvertPositionAttribute(
                            attrData,
                            (uint)strides[attrData.stream],
                            vertexCount,
                            inputStreams[attrData.stream],
                            outputStreams[attrData.stream]
                            );
                        break;
                    case VertexAttribute.Tangent:
                        ConvertTangentAttribute(
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
                bufferViewIds[stream] = WriteBufferViewToBuffer(outputStreams[stream],strides[stream]);
                inputStreams[stream].Dispose();
                outputStreams[stream].Dispose();
            }

            foreach (var pair in attrDataDict) {
                var attrData = pair.Value;
                m_Accessors[attrData.accessorId].bufferView = bufferViewIds[attrData.stream];
            }
        }

        int AddAccessor(Accessor accessor) {
            m_Accessors = m_Accessors ?? new List<Accessor>();
            var accessorId = m_Accessors.Count;
            m_Accessors.Add(accessor);
            return accessorId;
        }

#endif // #if GLTFAST_MESH_DATA

        static unsafe void ConvertPositionAttribute(
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
            }.Schedule(vertexCount,k_DefaultInnerLoopBatchCount);
            job.Complete(); // TODO: Wait until thread is finished
        }

        static unsafe void ConvertTangentAttribute(
            AttributeData attrData,
            uint byteStride,
            int vertexCount,
            NativeArray<byte> inputStream,
            NativeArray<byte> outputStream
            )
        {
            var job = new ExportJobs.ConvertTangentFloatJob {
                input = (byte*)inputStream.GetUnsafeReadOnlyPtr() + attrData.offset,
                byteStride = byteStride,
                output = (byte*)outputStream.GetUnsafePtr() + attrData.offset
            }.Schedule(vertexCount,k_DefaultInnerLoopBatchCount);
            job.Complete(); // TODO: Wait until thread is finished
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
            var buffer = bufferWriter;
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
            return bufferViewId;
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
            public int meshId;
            public int[] materialIds;

            public override bool Equals(object obj) {
                //Check for null and compare run-time types.
                if (obj == null || ! GetType().Equals(obj.GetType())) {
                    return false;
                }
                return Equals((MeshMaterialCombination)obj);
            }

            bool Equals(MeshMaterialCombination other) {
                return meshId == other.meshId && Equals(materialIds, other.materialIds);
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
#if NET_4_6
                return HashCode.Combine(meshId, materialIds);
#else
                var hash = 17;
                hash = hash * 31 + meshId.GetHashCode();
                hash = hash * 31 + materialIds.GetHashCode();
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
