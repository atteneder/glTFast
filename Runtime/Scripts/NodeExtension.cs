// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using Unity.Mathematics;
using UnityEngine.Scripting.APIUpdating;

namespace Unity.Cloud.Gltfast
{

    using Objects;

    /// <summary>
    /// Extension methods for <see cref="Node"/>
    /// </summary>
    [MovedFrom(true, sourceNamespace: "GLTFast", sourceAssembly: "glTFast")]
    public static class NodeExtension
    {
        /// <summary>
        /// Get translation, rotation and scale of a node, regardless of source
        /// properties
        /// </summary>
        /// <param name="node">Input node</param>
        /// <param name="position">Node's translation</param>
        /// <param name="rotation">Node's rotation</param>
        /// <param name="scale">Node's scale</param>
        public static void GetTransform(
            this Node node,
            out double3 position,
            out double4 rotation,
            out double3 scale
            )
        {

            position = double3.zero;
            rotation = Mathematics.k_QuaternionIdentity;
            scale = Mathematics.k_Double3One;

            if (node.Matrix.HasValue)
            {
                var src = node.Matrix.Value;
                var m = new double4x4(
                    src.c0.x,
                    -src.c1.x,
                    -src.c2.x,
                    -src.c3.x,
                    -src.c0.y,
                    src.c1.y,
                    src.c2.y,
                    src.c3.y,
                    -src.c0.z,
                    src.c1.z,
                    src.c2.z,
                    src.c3.z,
                    src.c0.w,
                    src.c1.w,
                    src.c2.w,
                    src.c3.w
                );

                m.Decompose(out var t, out var r, out var s);
                position = t;
                rotation = r;
                scale = s;

            }
            else
            {
                if (node.Translation.HasValue)
                {
                    var t = node.Translation.Value;
                    position = new double3(
                        -t.x,
                        t.y,
                        t.z
                    );
                }
                if (node.Rotation.HasValue)
                {
                    var r = node.Rotation.Value;
                    rotation = new double4(
                        r.x,
                        -r.y,
                        -r.z,
                        r.w
                    );
                }
                if (node.Scale.HasValue)
                {
                    scale = node.Scale.Value;
                }
            }
        }
    }
}
