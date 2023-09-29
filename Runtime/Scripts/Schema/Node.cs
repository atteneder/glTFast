// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

namespace GLTFast.Schema
{
    /// <inheritdoc />
    [System.Serializable]
    public class Node : NodeBase<NodeExtensions> { }

    /// <inheritdoc />
    /// <typeparam name="TExtensions">Node extensions type</typeparam>
    [System.Serializable]
    public abstract class NodeBase<TExtensions> : NodeBase
        where TExtensions : NodeExtensions
    {
        /// <inheritdoc cref="Extensions"/>
        public TExtensions extensions;

        /// <inheritdoc />
        public override NodeExtensions Extensions => extensions;

        /// <inheritdoc />
        internal override void UnsetExtensions()
        {
            extensions = null;
        }
    }

    /// <summary>
    /// An object defining the hierarchy relations and the local transform of
    /// its content.
    /// </summary>
    [System.Serializable]
    public abstract class NodeBase : NamedObject
    {

        /// <summary>
        /// The indices of this node's children.
        /// </summary>
        public uint[] children;

        /// <summary>
        /// The index of the mesh in this node.
        /// </summary>
        public int mesh = -1;

        /// <summary>
        /// A floating-point 4x4 transformation matrix stored in column-major order.
        /// </summary>
        public float[] matrix;

        /// <summary>
        /// The node's unit quaternion rotation in the order (x, y, z, w),
        /// where w is the scalar.
        /// </summary>
        public float[] rotation;

        /// <summary>
        /// The node's non-uniform scale.
        /// </summary>
        public float[] scale;

        /// <summary>
        /// The node's translation.
        /// </summary>
        public float[] translation;

        // /// <summary>
        // /// The weights of the instantiated Morph Target.
        // /// Number of elements must match number of Morph Targets of used mesh.
        // /// </summary>
        // public double[] weights;

        /// <summary>
        /// The index of the skin (in <see cref="RootBase.Skins"/> referenced by this node.
        /// </summary>
        public int skin = -1;

        /// <summary>
        /// Camera index
        /// </summary>
        public int camera = -1;

        /// <inheritdoc cref="NodeExtensions"/>
        public abstract NodeExtensions Extensions { get; }

        /// <summary>
        /// Sets <see cref="Extensions"/> to null.
        /// </summary>
        internal abstract void UnsetExtensions();

        internal void GltfSerialize(JsonWriter writer)
        {
            writer.AddObject();
            GltfSerializeName(writer);

            if (children != null)
            {
                writer.AddArrayProperty("children", children);
            }

            if (mesh >= 0)
            {
                writer.AddProperty("mesh", mesh);
            }

            if (translation != null)
            {
                writer.AddArrayProperty("translation", translation);
            }

            if (rotation != null)
            {
                writer.AddArrayProperty("rotation", rotation);
            }

            if (scale != null)
            {
                writer.AddArrayProperty("scale", scale);
            }

            if (matrix != null)
            {
                writer.AddArrayProperty("matrix", matrix);
            }

            if (skin >= 0)
            {
                writer.AddProperty("skin", skin);
            }

            if (camera >= 0)
            {
                writer.AddProperty("camera", camera);
            }

            if (Extensions != null)
            {
                writer.AddProperty("extensions");
                Extensions.GltfSerialize(writer);
            }
            writer.Close();
        }

        /// <summary>
        /// Cleans up invalid parsing artifacts created by <see cref="GltfJsonUtilityParser"/>.
        /// If you inherit a custom Node class (for use with
        /// <see cref="GltfImport.LoadWithCustomSchema&lt;T&gt;(string,ImportSettings,System.Threading.CancellationToken)"/>
        /// ) you can override this method to perform sanity checks on the deserialized, custom properties.
        /// </summary>
        public virtual void JsonUtilityCleanup()
        {
            var e = Extensions;
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
                    UnsetExtensions();
                }
            }
        }
    }

    /// <summary>
    /// Node extensions
    /// </summary>
    [System.Serializable]
    public class NodeExtensions
    {
        // Names are identical to glTF specified properties, that's why
        // inconsistent names are ignored.
        // ReSharper disable InconsistentNaming

        /// <inheritdoc cref="MeshGpuInstancing"/>
        public MeshGpuInstancing EXT_mesh_gpu_instancing;
        /// <inheritdoc cref="LightsPunctual"/>
        public NodeLightsPunctual KHR_lights_punctual;

        // Whenever an extension is added, the JsonParser
        // (specifically step four of JsonParser.ParseJson)
        // needs to be updated!

        // ReSharper restore InconsistentNaming
        internal void GltfSerialize(JsonWriter writer)
        {
            writer.AddObject();
            if (EXT_mesh_gpu_instancing != null)
            {
                writer.AddProperty("EXT_mesh_gpu_instancing");
                EXT_mesh_gpu_instancing.GltfSerialize(writer);
            }
            if (KHR_lights_punctual != null)
            {
                writer.AddProperty("KHR_lights_punctual");
                KHR_lights_punctual.GltfSerialize(writer);
            }
            writer.Close();
        }
    }
}
