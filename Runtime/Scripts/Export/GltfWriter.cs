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

using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace GLTFast.Export {
    
    using Schema;

    public class GltfWriter {
        
        const int k_MAXStreamCount = 4;
        
        Root m_Gltf;

        List<Scene> m_Scenes;
        List<Node> m_Nodes;
        List<Mesh> m_Meshes;
        
        List<Accessor> m_Accessors;
        List<BufferView> m_BufferViews;

        List<UnityEngine.Mesh> m_UnityMeshes;

        MemoryStream m_BufferStream;
        BinaryWriter m_BufferWriter;

        BinaryWriter bufferWriter {
            get {
                if (m_BufferWriter == null) {
                    m_BufferStream = new MemoryStream();
                    m_BufferWriter = new BinaryWriter(m_BufferStream);
                }
                return m_BufferWriter;
            }
        }
        
        public GltfWriter() {
            m_Gltf = new Root();
        }

        public uint AddScene(string name, uint[] nodes) {
            m_Scenes ??= new List<Scene>();
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
            Vector3 translation,
            Quaternion rotation,
            Vector3 scale,
            uint[] children
            ) {
            var node = new Node {
                name = name,
                children = children,
                translation = new []{translation.x,translation.y,translation.z},
                rotation = new []{rotation.x,rotation.y,rotation.z,rotation.w},
                scale = new []{scale.x,scale.y,scale.z},
            };
            m_Nodes ??= new List<Node>();
            m_Nodes.Add(node);
            return (uint) m_Nodes.Count - 1;
        }
        
        public void AddMeshToNode(uint nodeId, [NotNull] UnityEngine.Mesh uMesh) {
            var node = m_Nodes[(int)nodeId];
            node.mesh = AddMesh(uMesh);
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
            File.WriteAllText(path,json);
            if (m_BufferStream != null) {
                using var file = new FileStream(bufferPath, FileMode.Create, FileAccess.Write);
                m_BufferStream.WriteTo(file);
            }
        }

        void Bake(string bufferPath) {
            if (m_Meshes != null) {
                BakeMeshes();    
            }

            if (m_BufferWriter != null) {
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

            m_Gltf.asset = new Asset {
                version = "2.0",
                generator = "glTFast 4.4.0-exp"
            };

            m_Scenes = null;
            m_Nodes = null;
            m_Meshes = null;
            m_Accessors = null;
            m_BufferViews = null;
        }

        void BakeMeshes() {
            var byteOffset = m_BufferStream?.Length ?? 0;
            for (var meshId = 0; meshId < m_Meshes.Count; meshId++) {
                byteOffset = BakeMesh(meshId, byteOffset);
            }
        }

        long BakeMesh(int meshId, long byteOffset) {
            var bufferViewBaseIndex = (m_BufferViews?.Count ?? 0) + 1; // +1 to offset index bufferView
            var mesh = m_Meshes[meshId];
            var uMesh = m_UnityMeshes[meshId];

            var vertexAttributes = uMesh.GetVertexAttributes();
            var strides = new int[k_MAXStreamCount];

            var attributes = new Attributes();
            var vertexCount = uMesh.vertexCount;

            foreach (var attribute in vertexAttributes) {
                var attrData = new AttributeData { offset = strides[attribute.stream], stream = attribute.stream };
                var size = attribute.dimension * GetAttributeSize(attribute.format);
                strides[attribute.stream] += size;

                m_Accessors ??= new List<Accessor>();
                var accessorId = m_Accessors.Count;

                var accessor = new Accessor {
                    bufferView = bufferViewBaseIndex + attribute.stream,
                    byteOffset = attrData.offset,
                    componentType = Accessor.GetComponentType(attribute.format),
                    count = vertexCount,
                    typeEnum = Accessor.GetAccessorAttributeType(attribute.dimension),

                    // min = new []{}, // TODO
                    // max = new []{}, // TODO
                };
                m_Accessors.Add(accessor);

                switch (attribute.attribute) {
                    case VertexAttribute.Position:
                        attributes.POSITION = accessorId;
                        break;
                    case VertexAttribute.Normal:
                        attributes.NORMAL = accessorId;
                        break;
                    case VertexAttribute.Tangent:
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

            var meshDataArray = UnityEngine.Mesh.AcquireReadOnlyMeshData(uMesh);
            var meshData = meshDataArray[0];

            var buffer = bufferWriter;
            var indexData = meshData.GetIndexData<byte>();
            buffer.BaseStream.Write(indexData);
            var indexBufferView = new BufferView {
                buffer = 0,
                byteOffset = (int)byteOffset,
                byteLength = indexData.Length,
            };
            m_BufferViews ??= new List<BufferView>();
            var indexBufferViewId = m_BufferViews.Count;
            m_BufferViews.Add(indexBufferView);
            byteOffset += indexData.Length;
            indexData.Dispose();

            var indexComponentType = uMesh.indexFormat == IndexFormat.UInt16 ? GLTFComponentType.UnsignedShort : GLTFComponentType.UnsignedInt;
            mesh.primitives = new MeshPrimitive[meshData.subMeshCount];
            var indexOffset = 0;
            for (var subMeshIndex = 0; subMeshIndex < meshData.subMeshCount; subMeshIndex++) {
                var subMesh = meshData.GetSubMesh(subMeshIndex);
                var mode = GetDrawMode(subMesh.topology);
                if (!mode.HasValue) {
                    // TODO: Support some quad to triangle conversion
                    Debug.LogError($"Unsupported topology {subMesh.topology}");
                    mode = DrawMode.Points;
                }

                var indexAccessor = new Accessor {
                    bufferView = indexBufferViewId,
                    typeEnum = GLTFAccessorAttributeType.SCALAR,
                    byteOffset = indexOffset,
                    componentType = indexComponentType,
                    count = subMesh.indexCount,

                    // min = new []{}, // TODO
                    // max = new []{}, // TODO
                };

                indexOffset += subMesh.indexCount * Accessor.GetComponentTypeSize(indexComponentType);

                var accessorId = m_Accessors.Count;
                m_Accessors.Add(indexAccessor);

                mesh.primitives[subMeshIndex] = new MeshPrimitive {
                    mode = mode.Value,
                    attributes = attributes,
                    indices = accessorId,

                    // material = 0
                };
            }

            for (var stream = 0; stream < streamCount; stream++) {
                var vData = meshData.GetVertexData<byte>(stream);
                var bufferView = new BufferView {
                    buffer = 0,
                    byteOffset = (int)byteOffset,
                    byteLength = vData.Length,
                    byteStride = strides[stream]
                };
                byteOffset += vData.Length;
                m_BufferViews.Add(bufferView);
                buffer.BaseStream.Write(vData);
                vData.Dispose();
            }

            meshDataArray.Dispose();
            return byteOffset;
        }

        static DrawMode? GetDrawMode(MeshTopology topology) {
            switch (topology) {
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
            // return JsonUtility.ToJson(m_Gltf,true);
            var settings = new JsonSerializerSettings {
                DefaultValueHandling = DefaultValueHandling.Ignore
            };
            return JsonConvert.SerializeObject(m_Gltf,Formatting.Indented,settings);
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
            m_Meshes ??= new List<Mesh>();
            m_UnityMeshes ??= new List<UnityEngine.Mesh>();
            m_Meshes.Add(mesh);
            m_UnityMeshes.Add(uMesh);
            meshId = m_Meshes.Count - 1;
            return meshId;
        }
        
        static unsafe int GetAttributeSize(VertexAttributeFormat format) {
            return format switch {
                VertexAttributeFormat.Float32 => sizeof(float),
                VertexAttributeFormat.Float16 => sizeof(half),
                VertexAttributeFormat.UNorm8 => sizeof(byte),
                VertexAttributeFormat.SNorm8 => sizeof(sbyte),
                VertexAttributeFormat.UNorm16 => sizeof(ushort),
                VertexAttributeFormat.SNorm16 => sizeof(short),
                VertexAttributeFormat.UInt8 => sizeof(byte),
                VertexAttributeFormat.SInt8 => sizeof(sbyte),
                VertexAttributeFormat.UInt16 => sizeof(ushort),
                VertexAttributeFormat.SInt16 => sizeof(short),
                VertexAttributeFormat.UInt32 => sizeof(uint),
                VertexAttributeFormat.SInt32 => sizeof(int),
                _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
            };
        }
    }
    
    struct AttributeData {
        public int stream;
        public int offset;
    }
}
