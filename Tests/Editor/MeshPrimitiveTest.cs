// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using GLTFast.Schema;
using NUnit.Framework;
using UnityEngine;

namespace GLTFast.Tests
{
    class MeshPrimitiveTest
    {

        [Test]
        public void MeshPrimitiveEqualTest()
        {

            var a = new MeshPrimitive { attributes = new Attributes { POSITION = 42 } };
            var b = new MeshPrimitive { attributes = new Attributes { POSITION = 42 } };

            Assert.IsTrue(a.Equals(b));

            a.targets = new[] { new MorphTarget { POSITION = 0 } };
            b.targets = null;
            Assert.IsFalse(a.Equals(b));

            a.targets = new[] { new MorphTarget { POSITION = 0 } };
            b.targets = new[] { new MorphTarget { POSITION = 0 } };
            Assert.IsTrue(a.Equals(b));

            a.targets = new[] { new MorphTarget { POSITION = 0 } };
            b.targets = new[] { new MorphTarget { POSITION = 0, NORMAL = 1 } };
            Assert.IsFalse(a.Equals(b));

            a.targets = null;
            b.targets = new[] { new MorphTarget { POSITION = 0 } };
            Assert.IsFalse(a.Equals(b));

            a.targets = new[] { new MorphTarget { POSITION = 0 } };
            b.targets = new[] { new MorphTarget { POSITION = 0 }, new MorphTarget { POSITION = 0 } };
            Assert.IsFalse(a.Equals(b));

            a = new MeshPrimitive { attributes = new Attributes { POSITION = 41 } };
            b = new MeshPrimitive { attributes = new Attributes { POSITION = 42 } };
            Assert.IsFalse(a.Equals(b));

            a.targets = new[] { new MorphTarget { POSITION = 0 } };
            b.targets = new[] { new MorphTarget { POSITION = 0 } };
            Assert.IsFalse(a.Equals(b));
        }
    }
}
