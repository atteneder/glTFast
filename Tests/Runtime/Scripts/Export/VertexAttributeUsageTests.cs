// SPDX-FileCopyrightText: 2024 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using GLTFast.Export;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;

namespace GLTFast.Tests.Export
{
    class VertexAttributeUsageTests
    {
        [Test]
        public void ToVertexAttributeUsage()
        {
            Assert.AreEqual(VertexAttributeUsage.Position, VertexAttribute.Position.ToVertexAttributeUsage());
            Assert.AreEqual(VertexAttributeUsage.Normal, VertexAttribute.Normal.ToVertexAttributeUsage());
            Assert.AreEqual(VertexAttributeUsage.Tangent, VertexAttribute.Tangent.ToVertexAttributeUsage());
            Assert.AreEqual(VertexAttributeUsage.Color, VertexAttribute.Color.ToVertexAttributeUsage());
            Assert.AreEqual(VertexAttributeUsage.TexCoord0, VertexAttribute.TexCoord0.ToVertexAttributeUsage());
            Assert.AreEqual(VertexAttributeUsage.TexCoord1, VertexAttribute.TexCoord1.ToVertexAttributeUsage());
            Assert.AreEqual(VertexAttributeUsage.TexCoord2, VertexAttribute.TexCoord2.ToVertexAttributeUsage());
            Assert.AreEqual(VertexAttributeUsage.TexCoord3, VertexAttribute.TexCoord3.ToVertexAttributeUsage());
            Assert.AreEqual(VertexAttributeUsage.TexCoord4, VertexAttribute.TexCoord4.ToVertexAttributeUsage());
            Assert.AreEqual(VertexAttributeUsage.TexCoord5, VertexAttribute.TexCoord5.ToVertexAttributeUsage());
            Assert.AreEqual(VertexAttributeUsage.TexCoord6, VertexAttribute.TexCoord6.ToVertexAttributeUsage());
            Assert.AreEqual(VertexAttributeUsage.TexCoord7, VertexAttribute.TexCoord7.ToVertexAttributeUsage());
            Assert.AreEqual(VertexAttributeUsage.BlendWeight, VertexAttribute.BlendWeight.ToVertexAttributeUsage());
            Assert.AreEqual(VertexAttributeUsage.BlendIndices, VertexAttribute.BlendIndices.ToVertexAttributeUsage());
        }
    }
}
