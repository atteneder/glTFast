// SPDX-FileCopyrightText: 2025 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

#if UNITY_ENTITIES_GRAPHICS

using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Unity.Cloud.Gltfast
{
    [BurstCompile]
    static class EntityUtils
    {
        internal static Entity CreateSceneRootEntity(World world, string name = null)
        {
            var entityManager = world.EntityManager;
            var sceneArchetype = entityManager.CreateArchetype(
                typeof(LocalTransform),
                typeof(LocalToWorld)
            );
            var entity = entityManager.CreateEntity(sceneArchetype);
            entityManager.SetComponentData(
                entity,
                new LocalTransform
                {
                    Position = float3.zero,
                    Rotation = quaternion.identity,
                    Scale = 1,
                });
            entityManager.SetComponentData(entity, new LocalToWorld { Value = float4x4.identity });
#if UNITY_EDITOR
            entityManager.SetName(entity, string.IsNullOrEmpty(name) ? "glTF" : name);
#endif
            return entity;
        }

#if UNITY_ENTITIES_GRAPHICS
        [BurstCompile]
#endif
        internal static void DestroyChildren(ref Entity rootEntity, ref EntityManager entityManager)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            if (entityManager.HasComponent<Child>(rootEntity))
            {
                var children = entityManager.GetBuffer<Child>(rootEntity);
                foreach (var child in children)
                {
                    ecb.DestroyEntity(child.Value);
                }
            }
            ecb.Playback(entityManager);
            ecb.Dispose();
        }
    }
}
#endif // UNITY_ENTITIES_GRAPHICS
