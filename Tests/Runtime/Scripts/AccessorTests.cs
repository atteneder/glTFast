// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using NUnit.Framework;
using UnityEngine;

using GLTFast.Schema;

namespace GLTFast.Tests
{
    class AccessorTests
    {
        [Test]
        public void GetAccessorAttributeType()
        {
            Assert.AreEqual(GltfAccessorAttributeType.SCALAR, Accessor.GetAccessorAttributeType(1));
            Assert.AreEqual(GltfAccessorAttributeType.VEC2, Accessor.GetAccessorAttributeType(2));
            Assert.AreEqual(GltfAccessorAttributeType.VEC3, Accessor.GetAccessorAttributeType(3));
            Assert.AreEqual(GltfAccessorAttributeType.VEC4, Accessor.GetAccessorAttributeType(4));

            Assert.That(() => Accessor.GetAccessorAttributeType(0),
                Throws.TypeOf<ArgumentOutOfRangeException>());

            Assert.That(() => Accessor.GetAccessorAttributeType(5),
                Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void GetAccessorAttributeTypeLength()
        {
            Assert.AreEqual(1, Accessor.GetAccessorAttributeTypeLength(GltfAccessorAttributeType.SCALAR));
            Assert.AreEqual(2, Accessor.GetAccessorAttributeTypeLength(GltfAccessorAttributeType.VEC2));
            Assert.AreEqual(3, Accessor.GetAccessorAttributeTypeLength(GltfAccessorAttributeType.VEC3));
            Assert.AreEqual(4, Accessor.GetAccessorAttributeTypeLength(GltfAccessorAttributeType.VEC4));
            Assert.AreEqual(4, Accessor.GetAccessorAttributeTypeLength(GltfAccessorAttributeType.MAT2));
            Assert.AreEqual(9, Accessor.GetAccessorAttributeTypeLength(GltfAccessorAttributeType.MAT3));
            Assert.AreEqual(16, Accessor.GetAccessorAttributeTypeLength(GltfAccessorAttributeType.MAT4));

            Assert.That(() => Accessor.GetAccessorAttributeTypeLength(GltfAccessorAttributeType.Undefined),
                Throws.TypeOf<ArgumentOutOfRangeException>());
        }
    }
}
