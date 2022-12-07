// Copyright 2020-2022 Andreas Atteneder
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

using UnityEngine;
using UnityEngine.Assertions;

namespace GLTFast
{

    using Schema;

    /// <summary>
    /// Extension methods for <seealso cref="Node"/>
    /// </summary>
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
        public static void GetTransform(this Node node, out Vector3 position, out Quaternion rotation, out Vector3 scale)
        {

            position = Vector3.zero;
            rotation = Quaternion.identity;
            scale = Vector3.one;

            if (node.matrix != null)
            {
                Matrix4x4 m = new Matrix4x4();
                m.m00 = node.matrix[0];
                m.m10 = -node.matrix[1];
                m.m20 = -node.matrix[2];
                m.m30 = node.matrix[3];
                m.m01 = -node.matrix[4];
                m.m11 = node.matrix[5];
                m.m21 = node.matrix[6];
                m.m31 = node.matrix[7];
                m.m02 = -node.matrix[8];
                m.m12 = node.matrix[9];
                m.m22 = node.matrix[10];
                m.m32 = node.matrix[11];
                m.m03 = -node.matrix[12];
                m.m13 = node.matrix[13];
                m.m23 = node.matrix[14];
                m.m33 = node.matrix[15];

                m.Decompose(out var t, out var r, out var s);
                position = t;
                rotation = r;
                scale = s;

            }
            else
            {
                if (node.translation != null)
                {
                    Assert.AreEqual(node.translation.Length, 3);
                    position = new Vector3(
                        -node.translation[0],
                        node.translation[1],
                        node.translation[2]
                    );
                }
                if (node.rotation != null)
                {
                    Assert.AreEqual(node.rotation.Length, 4);
                    rotation = new Quaternion(
                        node.rotation[0],
                        -node.rotation[1],
                        -node.rotation[2],
                        node.rotation[3]
                    );
                }
                if (node.scale != null)
                {
                    Assert.AreEqual(node.scale.Length, 3);
                    scale = new Vector3(
                        node.scale[0],
                        node.scale[1],
                        node.scale[2]
                    );
                }
            }
        }
    }
}
