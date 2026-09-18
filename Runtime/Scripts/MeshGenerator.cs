// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Cloud.Gltfast.Jobs;
using Unity.Cloud.Gltfast.Logging;
using Unity.Cloud.Gltfast.Objects;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using Mesh = UnityEngine.Mesh;

namespace Unity.Cloud.Gltfast
{
    class MeshGenerator : MeshGeneratorBase
    {
        VertexBufferGeneratorBase m_VertexData;

        IndicesData m_Indices;

        readonly SubMeshAssignment[] m_SubMeshAssignments;
        readonly IReadOnlyList<MeshPrimitive> m_Primitives;

        MeshTopology m_Topology;

        int SubMeshCount => m_SubMeshAssignments?.Length ?? m_Primitives.Count;

        MeshPrimitive GetSubMesh(int index) =>
            m_SubMeshAssignments == null
                ? m_Primitives[index]
                : m_SubMeshAssignments[index].Primitive;

        public MeshGenerator(
            IReadOnlyList<MeshPrimitive> primitives,
            SubMeshAssignment[] subMeshAssignments,
            IReadOnlyList<string> morphTargetNames,
            string meshName,
            IGltfReadable gltf,
            BufferStore buffers,
            IDeferAgent deferAgent,
            ICodeLogger logger
        )
            : base(meshName)
        {
            m_Primitives = primitives;
            m_SubMeshAssignments = subMeshAssignments;
            if (CreateVertexGenerator(gltf, buffers, logger, out var hasNormals, out var hasTangents))
            {
                CreateMorphTargetGenerator(morphTargetNames, hasNormals, hasTangents, buffers, deferAgent, logger);
                m_CreationTask = GenerateMesh(buffers, logger);
            }
        }

        bool CreateVertexGenerator(
            IGltfReadable gltf,
            BufferStore buffers,
            ICodeLogger logger,
            out bool hasNormals,
            out bool hasTangents
            )
        {
            var drawMode = m_Primitives[0].Mode;
            if (!SetTopology(drawMode))
            {
                logger?.Error(LogCode.PrimitiveModeUnsupported, drawMode.ToString());
            }

            var mainBufferType = GetMainBufferType(gltf, out hasNormals, out hasTangents);

            switch (mainBufferType)
            {
                case MainBufferType.Position:
                    m_VertexData = new VertexBufferGenerator<Vertex.VPos>(m_Primitives.Count, buffers, logger);
                    break;
                case MainBufferType.PosNorm:
                    m_VertexData = new VertexBufferGenerator<Vertex.VPosNorm>(m_Primitives.Count, buffers, logger);
                    break;
                case MainBufferType.PosNormTan:
                    m_VertexData = new VertexBufferGenerator<Vertex.VPosNormTan>(m_Primitives.Count, buffers, logger);
                    break;
                default:
                    logger?.Error(LogCode.BufferMainInvalidType, mainBufferType.ToString());
                    return false;
            }
            m_VertexData.calculateNormals = !hasNormals && (mainBufferType & MainBufferType.Normal) > 0;
            m_VertexData.calculateTangents = !hasTangents && (mainBufferType & MainBufferType.Tangent) > 0;

            foreach (var primitive in m_Primitives)
            {
                m_VertexData.AddPrimitive(primitive.Attributes);
            }

            m_VertexData.Initialize();
            return true;
        }

        MainBufferType GetMainBufferType(
            IGltfReadable gltf,
            out bool hasNormals,
            out bool hasTangents
            )
        {
            var mainBufferType = MainBufferType.Position;
            var firstAttributes = m_Primitives[0].Attributes;
            hasNormals = firstAttributes.Normal.HasValue;
            hasTangents = firstAttributes.Tangent.HasValue;

            if (hasTangents)
                mainBufferType = MainBufferType.PosNormTan;
            else if (hasNormals)
                mainBufferType = MainBufferType.PosNorm;

            Profiler.BeginSample("LoadAccessorData.ScheduleVertexJob");

            for (var i = 0; i < SubMeshCount; i++)
            {
                var primitive = GetSubMesh(i);
                if (primitive.Mode == PrimitiveMode.Triangles
                    || primitive.Mode == PrimitiveMode.TriangleFan
                    || primitive.Mode == PrimitiveMode.TriangleStrip)
                {
                    if (!primitive.Material.HasValue)
                    {
                        mainBufferType |= MainBufferType.Normal;
                    }
                    else
                    {
                        var material = gltf.GetSourceMaterial(primitive.Material.Value);
                        if (material.RequiresTangents)
                        {
                            mainBufferType |= MainBufferType.Normal | MainBufferType.Tangent;
                        }
                        else if (material.RequiresNormals)
                        {
                            mainBufferType |= MainBufferType.Normal;
                        }
                    }
                }
            }
            Profiler.EndSample();

            return mainBufferType;
        }

        bool SetTopology(PrimitiveMode primitiveMode)
        {
            switch (primitiveMode)
            {
                case PrimitiveMode.Triangles:
                case PrimitiveMode.TriangleStrip:
                case PrimitiveMode.TriangleFan:
                    m_Topology = MeshTopology.Triangles;
                    break;
                case PrimitiveMode.Points:
                    m_Topology = MeshTopology.Points;
                    break;
                case PrimitiveMode.Lines:
                    m_Topology = MeshTopology.Lines;
                    break;
                case PrimitiveMode.LineLoop:
                case PrimitiveMode.LineStrip:
                    m_Topology = MeshTopology.LineStrip;
                    break;
                default:
                    m_Topology = MeshTopology.Triangles;
                    return false;
            }
            return true;
        }

        void CreateMorphTargetGenerator(
            IReadOnlyList<string> morphTargetNames,
            bool hasNormals,
            bool hasTangents,
            BufferStore buffers,
            IDeferAgent deferAgent,
            ICodeLogger logger
            )
        {
            var morphTargets = m_Primitives[0].Targets;
            if (morphTargets != null)
            {
                m_MorphTargetsGenerator = new MorphTargetsGenerator(
                    m_VertexData.VertexCount,
                    m_Primitives.Count,
                    morphTargets.Count,
                    morphTargetNames,
                    hasNormals,
                    hasTangents,
                    buffers,
                    deferAgent,
                    logger
                );
            }
        }

        async Task<Mesh> GenerateMesh(BufferStore buffers, ICodeLogger logger)
        {
            if (!await m_VertexData.CreateVertexBufferAsync())
                return null;

            var indexFormat = IndexFormat.UInt16;
            for (var i = 0; i < SubMeshCount; i++)
            {
                var primitive = GetSubMesh(i);
                if (primitive.Indices.HasValue)
                {
                    var accessor = buffers.GetAccessor(primitive.Indices.Value);
                    if (accessor.ComponentType == AccessorDataType.UnsignedInt)
                    {
                        indexFormat = IndexFormat.UInt32;
                        break;
                    }
                }
                else
                {
                    var vertexCount = buffers.GetAccessor(primitive.Attributes.Position.Value).Count;
                    if (vertexCount > ushort.MaxValue)
                    {
                        indexFormat = IndexFormat.UInt32;
                        break;
                    }
                }
            }

            m_Indices = new IndicesData(indexFormat, SubMeshCount);

            var tmpList = new List<JobHandle>(SubMeshCount);
            for (var subMeshIndex = 0; subMeshIndex < SubMeshCount; subMeshIndex++)
            {
                var primitive = GetSubMesh(subMeshIndex);
                if (primitive.Indices.HasValue)
                {
                    var flip = primitive.Mode == PrimitiveMode.Triangles;
                    var accessor = buffers.GetAccessor(primitive.Indices.Value);

                    var minIndexCount = 3;
                    var indexCount = accessor.Count;
                    switch (primitive.Mode)
                    {
                        case PrimitiveMode.TriangleStrip or PrimitiveMode.TriangleFan:
                            indexCount = (accessor.Count - 2) * 3;
                            break;
                        case PrimitiveMode.LineLoop:
                            minIndexCount = 2;
                            indexCount = accessor.Count + 1;
                            break;
                        case PrimitiveMode.Lines or PrimitiveMode.LineStrip:
                            minIndexCount = 2;
                            break;
                        case PrimitiveMode.Points:
                            minIndexCount = 1;
                            break;
                    }

                    if (accessor.Count < minIndexCount)
                    {
                        logger?.Error(
                            LogCode.IndexCountInvalid,
                            accessor.Count.ToString()
                        );
                        return null;
                    }

                    JobHandle? getIndicesJob = null;

                    m_Indices.Allocate(subMeshIndex, indexCount);

                    var status = buffers.TryGetBufferView(
                        accessor.BufferView.Value,
                        out var accessorData,
                        out _,
                        accessor.ByteOffset,
                        accessor.ByteSize
                    );

                    if (status != BufferAccessStatus.Success)
                    {
                        logger?.Error(LogCode.AccessorAccessFailed, GltfIndex.Describe(primitive.Indices));
                        continue;
                    }

                    Assert.AreEqual(accessor.Type.Value, AccessorType.Scalar);
                    if (accessor.IsSparse)
                    {
                        logger?.Error(LogCode.SparseAccessor, "indices");
                    }

                    switch (indexFormat)
                    {
                        case IndexFormat.UInt16:
                        {
                            var indices = m_Indices.GetIndices16(subMeshIndex);
                            GetIndicesUInt16Job(accessor, accessorData, indices, out getIndicesJob, flip, logger);
                            break;
                        }
                        case IndexFormat.UInt32:
                        {
                            var indices = m_Indices.GetIndices32(subMeshIndex);
                            GetIndicesUInt32Job(accessor, accessorData, indices, out getIndicesJob, flip, logger);
                            break;
                        }
                    }
                    if (!getIndicesJob.HasValue)
                        return null;

                    switch (primitive.Mode)
                    {
                        case PrimitiveMode.LineLoop:
                        {
                            // Wait for indices to be ready.
                            while (!getIndicesJob.Value.IsCompleted)
                            {
                                await Task.Yield();
                            }
                            getIndicesJob.Value.Complete();

                            if (indexFormat == IndexFormat.UInt16)
                            {
                                var indices = m_Indices.GetIndices16(subMeshIndex);
                                indices[^1] = indices[0];
                            }
                            else
                            {
                                var indices = m_Indices.GetIndices32(subMeshIndex);
                                indices[^1] = indices[0];
                            }

                            break;
                        }
                        case PrimitiveMode.TriangleStrip:
                        {
                            JobHandle job;
                            if (indexFormat == IndexFormat.UInt16)
                            {
                                job = new RecalculateIndicesForTriangleStripInPlaceJob<ushort>
                                {
                                    indices = m_Indices.GetIndices16(subMeshIndex),
                                }.Schedule(getIndicesJob.Value);
                            }
                            else
                            {
                                job = new RecalculateIndicesForTriangleStripInPlaceJob<uint>
                                {
                                    indices = m_Indices.GetIndices32(subMeshIndex),
                                }.Schedule(getIndicesJob.Value);
                            }
                            tmpList.Add(job);
                            break;
                        }
                        case PrimitiveMode.TriangleFan:
                        {
                            JobHandle job;
                            if (indexFormat == IndexFormat.UInt16)
                            {
                                job = new RecalculateIndicesForTriangleFanInPlaceJob<ushort>
                                {
                                    indices = m_Indices.GetIndices16(subMeshIndex),
                                }.Schedule(getIndicesJob.Value);
                            }
                            else
                            {
                                job = new RecalculateIndicesForTriangleFanInPlaceJob<uint>
                                {
                                    indices = m_Indices.GetIndices32(subMeshIndex),
                                }.Schedule(getIndicesJob.Value);
                            }
                            tmpList.Add(job);
                            break;
                        }
                        default:
                            tmpList.Add(getIndicesJob.Value);
                            break;
                    }
                }
                else
                {
                    var vertexCount = buffers.GetAccessor(primitive.Attributes.Position.Value).Count;
                    var indexCount = primitive.Mode switch
                    {
                        PrimitiveMode.TriangleStrip or PrimitiveMode.TriangleFan => (vertexCount - 2) * 3,
                        PrimitiveMode.LineLoop => vertexCount + 1,
                        _ => vertexCount
                    };

                    m_Indices.Allocate(subMeshIndex, indexCount);

                    JobHandle job;
                    if (indexFormat == IndexFormat.UInt16)
                    {
                        CalculateIndicesUInt16Job(primitive, m_Indices.GetIndices16(subMeshIndex), out job);
                    }
                    else
                    {
                        CalculateIndicesUInt32Job(primitive, m_Indices.GetIndices32(subMeshIndex), out job);
                    }
                    tmpList.Add(job);
                }
            }

            if (m_MorphTargetsGenerator != null)
            {
                for (var subMeshIndex = 0; subMeshIndex < m_Primitives.Count; subMeshIndex++)
                {
                    var primitive = m_Primitives[subMeshIndex];
                    AddMorphTargets(subMeshIndex, primitive, logger);
                }
                tmpList.Add(m_MorphTargetsGenerator.GetJobHandle());
            }

            await AwaitJobs(tmpList);

            return await BuildMeshResultAsync(logger);
        }

        void AddMorphTargets(int subMesh, MeshPrimitive primitive, ICodeLogger logger)
        {
            if (m_MorphTargetsGenerator == null)
                return;
            var vertexOffset = m_VertexData.VertexIntervals[subMesh];
            for (var morphTargetIndex = 0; morphTargetIndex < primitive.Targets.Count; morphTargetIndex++)
            {
                var morphTarget = primitive.Targets[morphTargetIndex];
                var success = m_MorphTargetsGenerator.AddMorphTarget(
                    vertexOffset,
                    subMesh,
                    morphTargetIndex,
                    morphTarget,
                    logger
                );
                if (!success)
                {
                    logger?.Error(LogCode.MorphTargetContextFail);
                }
            }
        }

        async Task<Mesh> BuildMeshResultAsync(ICodeLogger logger)
        {
            Profiler.BeginSample("CreateMesh");
            var msh = new Mesh
            {
                name = m_MeshName
            };

            m_VertexData.ApplyOnMesh(msh);

            Profiler.BeginSample("SetIndices");
            var indexCount = m_Indices.GetTotalIndexCount();
            Profiler.BeginSample("SetIndexBufferParams");
            msh.SetIndexBufferParams(indexCount, m_Indices.IndexFormat);
            Profiler.EndSample();
            msh.subMeshCount = m_Indices.SubMeshCount;
            indexCount = 0;
            Bounds bounds = default;
            for (var i = 0; i < m_Indices.SubMeshCount; i++)
            {
                Profiler.BeginSample("SetIndexBufferData");
                int subMeshIndexCount;
                if (m_Indices.IndexFormat == IndexFormat.UInt16)
                {
                    var indices = m_Indices.GetIndices16(i);
                    subMeshIndexCount = indices.Length;
                    msh.SetIndexBufferData(indices, 0, indexCount, indices.Length, defaultMeshUpdateFlags);
                }
                else
                {
                    var indices = m_Indices.GetIndices32(i);
                    subMeshIndexCount = indices.Length;
                    msh.SetIndexBufferData(indices, 0, indexCount, indices.Length, defaultMeshUpdateFlags);
                }

                Profiler.EndSample();

                Profiler.BeginSample("SetSubMesh");
                var vertexBufferIndex = m_SubMeshAssignments != null ? m_SubMeshAssignments[i].VertexBufferIndex : i;
                m_VertexData.GetVertexRange(vertexBufferIndex, out var baseVertex, out var vertexCount);
                var subMeshBoundsValid = m_VertexData.TryGetBounds(vertexBufferIndex, logger, out var subMeshBounds);
                var subMeshDescriptor = new SubMeshDescriptor
                {
                    indexStart = indexCount,
                    indexCount = subMeshIndexCount,
                    topology = m_Topology,
                    baseVertex = baseVertex,
                    firstVertex = baseVertex,
                    vertexCount = vertexCount,
                    bounds = subMeshBounds
                };
                msh.SetSubMesh(
                    i,
                    subMeshDescriptor,
                    subMeshBoundsValid
                        ? defaultMeshUpdateFlags
                        : defaultMeshUpdateFlags & ~MeshUpdateFlags.DontRecalculateBounds
                    );
                if (!subMeshBoundsValid)
                {
                    subMeshDescriptor = msh.GetSubMesh(i);
                    subMeshBounds = subMeshDescriptor.bounds;
                }

                if (i == 0)
                {
                    bounds = subMeshBounds;
                }
                else
                {
                    bounds.Encapsulate(subMeshBounds);
                }
                Profiler.EndSample();
                indexCount += subMeshIndexCount;
            }

            msh.bounds = bounds;

            Profiler.EndSample();

            if (m_Topology == MeshTopology.Triangles || m_Topology == MeshTopology.Quads)
            {
                if (m_VertexData.calculateNormals)
                {
                    Profiler.BeginSample("RecalculateNormals");
                    msh.RecalculateNormals();
                    Profiler.EndSample();
                }
                if (m_VertexData.calculateTangents)
                {
                    Profiler.BeginSample("RecalculateTangents");
                    msh.RecalculateTangents();
                    Profiler.EndSample();
                }
            }

            if (m_MorphTargetsGenerator != null)
            {
                await m_MorphTargetsGenerator.ApplyOnMeshAndDisposeAsync(msh);
            }

#if GLTFAST_KEEP_MESH_DATA
            Profiler.BeginSample("UploadMeshData");
            msh.UploadMeshData(false);
            Profiler.EndSample();
#endif

            Profiler.EndSample();

            return msh;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                m_VertexData?.Dispose();
                m_Indices.Dispose();
            }
        }


        static void GetIndicesUInt16Job(
            Accessor accessor,
            ReadOnlyNativeArray<byte> accessorData,
            NativeArray<ushort> indices,
            out JobHandle? jobHandle,
            bool flip,
            ICodeLogger logger
            )
        {
            Profiler.BeginSample("GetIndicesUInt16Job");
            switch (accessor.ComponentType)
            {
                case AccessorDataType.UnsignedByte:
                {
                    if (flip)
                    {
                        var job8 = new ConvertIndicesUInt8ToUInt16FlippedJob
                        {
                            input = accessorData.Reinterpret<byte3>().AsNativeArrayReadOnly(),
                            result = indices.Reinterpret<ushort3>(UnsafeUtility.SizeOf<ushort>())
                        };
                        jobHandle = job8.Schedule(accessor.Count / 3, GltfImport.DefaultBatchCount);
                    }
                    else
                    {
                        var job8 = new ConvertIndicesUInt8ToUInt16Job
                        {
                            input = accessorData.AsNativeArrayReadOnly(),
                            result = indices
                        };
                        jobHandle = job8.Schedule(accessor.Count, GltfImport.DefaultBatchCount);
                    }
                    break;
                }
                case AccessorDataType.UnsignedShort:
                {
                    if (flip)
                    {
                        var job16 = new ConvertIndicesUInt16ToUInt16FlippedJob
                        {
                            input = accessorData.Reinterpret<ushort3>().AsNativeArrayReadOnly(),
                            result = indices.Reinterpret<ushort3>(UnsafeUtility.SizeOf<ushort>())
                        };
                        jobHandle = job16.Schedule(accessor.Count / 3, GltfImport.DefaultBatchCount);
                    }
                    else
                    {
                        unsafe
                        {
                            var job = new MemCopyJob
                            {
                                bufferSize = accessorData.Length,
                                input = (byte*)accessorData.GetUnsafeReadOnlyPtr(),
                                result = (byte*)indices.GetUnsafePtr()
                            };
                            jobHandle = job.Schedule();
                        }
                    }
                    break;
                }
                default:
                    logger?.Error(LogCode.IndexFormatInvalid, accessor.ComponentType.ToString());
                    jobHandle = null;
                    break;
            }
            Profiler.EndSample();
        }

        static void GetIndicesUInt32Job(
            Accessor accessor,
            ReadOnlyNativeArray<byte> accessorData,
            NativeArray<uint> indices,
            out JobHandle? jobHandle,
            bool flip,
            ICodeLogger logger
            )
        {
            Profiler.BeginSample("GetIndicesUInt32Job");
            switch (accessor.ComponentType)
            {
                case AccessorDataType.UnsignedByte:
                {
                    if (flip)
                    {
                        var job8 = new ConvertIndicesUInt8ToUInt32FlippedJob
                        {
                            input = accessorData.Reinterpret<byte3>().AsNativeArrayReadOnly(),
                            result = indices.Reinterpret<uint3>(UnsafeUtility.SizeOf<uint>())
                        };
                        jobHandle = job8.Schedule(accessor.Count / 3, GltfImport.DefaultBatchCount);
                    }
                    else
                    {
                        var job8 = new ConvertIndicesUInt8ToUInt32Job
                        {
                            input = accessorData.AsNativeArrayReadOnly(),
                            result = indices
                        };
                        jobHandle = job8.Schedule(accessor.Count, GltfImport.DefaultBatchCount);
                    }
                    break;
                }
                case AccessorDataType.UnsignedShort:
                {
                    if (flip)
                    {
                        var job16 = new ConvertIndicesUInt16ToUInt32FlippedJob
                        {
                            input = accessorData.Reinterpret<ushort3>().AsNativeArrayReadOnly(),
                            result = indices.Reinterpret<uint3>(UnsafeUtility.SizeOf<uint>())
                        };
                        jobHandle = job16.Schedule(accessor.Count / 3, GltfImport.DefaultBatchCount);
                    }
                    else
                    {
                        var job16 = new ConvertIndicesUInt16ToUInt32Job
                        {
                            input = accessorData.Reinterpret<ushort>().AsNativeArrayReadOnly(),
                            result = indices
                        };
                        jobHandle = job16.Schedule(accessor.Count, GltfImport.DefaultBatchCount);
                    }
                    break;
                }
                case AccessorDataType.UnsignedInt:
                {
                    if (flip)
                    {
                        var job32 = new ConvertIndicesUInt32ToUInt32FlippedJob
                        {
                            input = accessorData.Reinterpret<uint3>().AsNativeArrayReadOnly(),
                            result = indices.Reinterpret<uint3>(UnsafeUtility.SizeOf<uint>())
                        };
                        jobHandle = job32.Schedule(accessor.Count / 3, GltfImport.DefaultBatchCount);
                    }
                    else
                    {
                        unsafe
                        {
                            Assert.AreEqual(accessor.Count * UnsafeUtility.SizeOf<uint>(), accessorData.Length);
                            var job = new MemCopyJob
                            {
                                bufferSize = accessorData.Length,
                                input = (byte*)accessorData.GetUnsafeReadOnlyPtr(),
                                result = (byte*)indices.GetUnsafePtr()
                            };
                            jobHandle = job.Schedule();
                        }
                    }
                    break;
                }
                default:
                    logger?.Error(LogCode.IndexFormatInvalid, accessor.ComponentType.ToString());
                    jobHandle = null;
                    break;
            }
            Profiler.EndSample();
        }

        static void CalculateIndicesUInt16Job(
            MeshPrimitive primitive,
            NativeArray<ushort> indices,
            out JobHandle jobHandle
            )
        {
            Profiler.BeginSample("CalculateIndicesJob");
            // No indices: calculate them
            switch (primitive.Mode)
            {
                case PrimitiveMode.LineLoop:
                {
                    // Set the last index to the first vertex
                    indices[^1] = 0;
                    var job = new CreateIndicesUInt16Job()
                    {
                        result = indices
                    };
                    jobHandle = job.Schedule(indices.Length - 1, GltfImport.DefaultBatchCount);
                    break;
                }
                case PrimitiveMode.Triangles:
                {
                    var job = new CreateIndicesUInt16FlippedJob
                    {
                        result = indices
                    };
                    jobHandle = job.Schedule(indices.Length, GltfImport.DefaultBatchCount);
                    break;
                }
                case PrimitiveMode.TriangleStrip:
                {
                    var job = new CreateIndicesForTriangleStripUInt16Job
                    {
                        result = indices
                    };
                    jobHandle = job.Schedule(indices.Length, GltfImport.DefaultBatchCount);
                    break;
                }
                case PrimitiveMode.TriangleFan:
                    var triangleFanJob = new CreateIndicesForTriangleFanUInt16Job
                    {
                        result = indices
                    };
                    jobHandle = triangleFanJob.Schedule(indices.Length, GltfImport.DefaultBatchCount);
                    break;
                default:
                {
                    var job = new CreateIndicesUInt16Job()
                    {
                        result = indices
                    };
                    jobHandle = job.Schedule(indices.Length, GltfImport.DefaultBatchCount);
                    break;
                }
            }
            Profiler.EndSample();
        }

        static void CalculateIndicesUInt32Job(
            MeshPrimitive primitive,
            NativeArray<uint> indices,
            out JobHandle jobHandle
            )
        {
            Profiler.BeginSample("CalculateIndicesJob");
            // No indices: calculate them
            switch (primitive.Mode)
            {
                case PrimitiveMode.LineLoop:
                {
                    // Set the last index to the first vertex
                    indices[^1] = 0;
                    var job = new CreateIndicesUInt32Job()
                    {
                        result = indices
                    };
                    jobHandle = job.Schedule(indices.Length - 1, GltfImport.DefaultBatchCount);
                    break;
                }
                case PrimitiveMode.Triangles:
                {
                    var job = new CreateIndicesUInt32FlippedJob
                    {
                        result = indices
                    };
                    jobHandle = job.Schedule(indices.Length, GltfImport.DefaultBatchCount);
                    break;
                }
                case PrimitiveMode.TriangleStrip:
                {
                    var job = new CreateIndicesForTriangleStripUInt32Job
                    {
                        result = indices
                    };
                    jobHandle = job.Schedule(indices.Length, GltfImport.DefaultBatchCount);
                    break;
                }
                case PrimitiveMode.TriangleFan:
                    var triangleFanJob = new CreateIndicesForTriangleFanUInt32Job
                    {
                        result = indices
                    };
                    jobHandle = triangleFanJob.Schedule(indices.Length, GltfImport.DefaultBatchCount);
                    break;
                default:
                {
                    var job = new CreateIndicesUInt32Job()
                    {
                        result = indices
                    };
                    jobHandle = job.Schedule(indices.Length, GltfImport.DefaultBatchCount);
                    break;
                }
            }
            Profiler.EndSample();
        }

        static async Task AwaitJobs(List<JobHandle> tmpList)
        {
            if (tmpList.Count > 0)
            {
                var jobHandles = new NativeArray<JobHandle>(tmpList.ToArray(), Allocator.Persistent);
                var allJobs = JobHandle.CombineDependencies(jobHandles);
                jobHandles.Dispose();
                while (!allJobs.IsCompleted)
                {
                    await Task.Yield();
                }
                allJobs.Complete();
            }
        }
    }
}
