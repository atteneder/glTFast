using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

namespace GLTFast
{
    public class EntityInstantiator : IInstantiator
    {
        EntityManager entityManager;

        Entity parent;

        Entity[] nodes;

        public EntityInstantiator(World world)
        {
            entityManager = world.EntityManager;

            var ent = entityManager.CreateEntity(
                typeof(LocalToWorld),
                typeof(Translation),
                typeof(Rotation)
            );
            entityManager.SetComponentData(ent, new Translation { Value = new float3(0, 0, 0) });
            entityManager.SetComponentData(ent, new Rotation { Value = quaternion.identity });

            parent = ent;
        }

        public EntityInstantiator(World world, ComponentType[] additionalComponents)
        {
            entityManager = world.EntityManager;

            ComponentType[] components = new ComponentType[3 + additionalComponents.Length];

            components[0] = typeof(LocalToWorld);
            components[1] = typeof(Translation);
            components[2] = typeof(Rotation);
            for (int i = 3; i < components.Length; i++)
            {
                components[i] = additionalComponents[i];
            }

            var ent = entityManager.CreateEntity(components);
            entityManager.SetComponentData(ent, new Translation { Value = new float3(0, 0, 0) });
            entityManager.SetComponentData(ent, new Rotation { Value = quaternion.identity });

            parent = ent;
        }

        public void Init(int nodeCount)
        {
            nodes = new Entity[nodeCount];
        }

        public void CreateNode(
            uint nodeIndex,
            Vector3 position,
            Quaternion rotation,
            Vector3 scale
        )
        {
            var ent = entityManager.CreateEntity(
                typeof(LocalToWorld),
                typeof(Translation),
                typeof(Rotation),
                typeof(RenderMesh),
                typeof(RenderBounds)
            );
            entityManager.SetComponentData(ent, new Translation { Value = position });
            entityManager.SetComponentData(ent, new Rotation { Value = rotation });
            if (scale != Vector3.one)
                entityManager.AddComponentData(ent, new NonUniformScale { Value = scale });
            nodes[nodeIndex] = ent;
        }

        public void SetParent(uint nodeIndex, uint parentIndex)
        {
            if (nodes[nodeIndex] == null || nodes[parentIndex] == null)
            {
                Debug.LogError("Invalid hierarchy");
                return;
            }
            entityManager.AddComponentData(nodes[nodeIndex], new LocalToParent
            {
                Value = float4x4.identity
            });
            entityManager.AddComponentData(nodes[nodeIndex], new Parent
            {
                Value = nodes[parentIndex]
            });
        }

        public void SetNodeName(uint nodeIndex, string name)
        {
            //nodes[nodeIndex].name = name ?? "Node";
            entityManager.SetName(nodes[nodeIndex], name);
        }

        public void AddPrimitive(
            uint nodeIndex,
            string meshName,
            UnityEngine.Mesh mesh,
            UnityEngine.Material[] materials,
            int[] joints = null,
            bool first = true
        )
        {
            Entity entity;
            if (first)
            {
                // Use Node GameObject for first Primitive
                entity = nodes[nodeIndex];
            }
            else
            {
                entity = entityManager.CreateEntity(
                    typeof(RenderMesh),
                    typeof(RenderBounds),
                    typeof(LocalToWorld),
                    typeof(LocalToParent),
                    typeof(Parent)
                );
                entityManager.SetName(entity, meshName ?? "Primitive");
                entityManager.SetComponentData(entity, new LocalToParent
                {
                    Value = float4x4.identity
                });
                entityManager.SetComponentData(entity, new Parent
                {
                    Value = nodes[nodeIndex]
                });
            }

            Mesh meshInstance = Object.Instantiate<Mesh>(mesh);

            entityManager.SetSharedComponentData(entity, new RenderMesh
            {
                mesh = meshInstance,
                material = new Material(materials[0])
            });


            ///TODO: Create entities when have more than 1 material, with same mesh
            if (materials.Length > 1)
            {
                // Do something
            }
        }

        public void AddScene(string name, uint[] nodeIndices)
        {
            entityManager.SetName(parent, name ?? "Scene");
            foreach (var nodeIndex in nodeIndices)
            {
                if (nodes[nodeIndex] != null)
                {
                    entityManager.AddComponentData(nodes[nodeIndex], new LocalToParent
                    {
                        Value = float4x4.identity
                    });
                    entityManager.AddComponentData(nodes[nodeIndex], new Parent
                    {
                        Value = parent
                    });
                }
            }
        }
    }
}