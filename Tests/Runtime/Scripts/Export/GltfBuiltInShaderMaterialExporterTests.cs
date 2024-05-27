// SPDX-FileCopyrightText: 2024 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using GLTFast.Export;
using NUnit.Framework;
using UnityEngine;

namespace GLTFast.Tests.Export
{
    [TestFixture, Category("Export")]
    class GltfBuiltInShaderMaterialExporterTests : MaterialExportTests
    {
        [Test]
        public void BaseColorTextureCutout()
        {
            BaseColorTextureCutoutTest(RenderPipeline.BuiltIn);
        }

        [Test]
        public void BaseColorTextureTransparent()
        {
            BaseColorTextureTransparentTest(RenderPipeline.BuiltIn);
        }

        [Test]
        public void BaseColor()
        {
            BaseColorTest(RenderPipeline.BuiltIn);
        }

        [Test]
        public void BaseColorTexture()
        {
            BaseColorTextureTest(RenderPipeline.BuiltIn);
        }

        [Test]
        public void BaseColorTextureTranslated()
        {
            BaseColorTextureTranslatedTest(RenderPipeline.BuiltIn);
        }

        [Test]
        public void BaseColorTextureScaled()
        {
            BaseColorTextureScaledTest(RenderPipeline.BuiltIn);
        }

        [Test]
        public void BaseColorTextureRotated()
        {
            BaseColorTextureRotatedTest(RenderPipeline.BuiltIn);
        }

        [Test]
        public void RoughnessTexture()
        {
            RoughnessTextureTest(RenderPipeline.BuiltIn);
        }

        [Test]
        public void Metallic()
        {
            MetallicTest(RenderPipeline.BuiltIn);
        }

        [Test]
        public void MetallicTexture()
        {
            MetallicTextureTest(RenderPipeline.BuiltIn);
        }

        [Test]
        public void MetallicRoughnessTexture()
        {
            MetallicRoughnessTextureTest(RenderPipeline.BuiltIn);
        }

        [Test]
        public void MetallicRoughnessOcclusionTexture()
        {
            MetallicRoughnessOcclusionTextureTest(RenderPipeline.BuiltIn);
        }

        [Test]
        public void OcclusionTexture()
        {
            OcclusionTextureTest(RenderPipeline.BuiltIn);
        }

        [Test]
        public void EmissiveFactor()
        {
            EmissiveFactorTest(RenderPipeline.BuiltIn);
        }

        [Test]
        public void EmissiveTexture()
        {
            EmissiveTextureTest(RenderPipeline.BuiltIn);
        }

        [Test]
        public void EmissiveTextureFactor()
        {
            EmissiveTextureFactorTest(RenderPipeline.BuiltIn);
        }

        [Test]
        public void NormalTexture()
        {
            NormalTextureTest(RenderPipeline.BuiltIn);
        }

        [Test]
        public void NotGltf()
        {
            NotGltfTest(RenderPipeline.BuiltIn);
        }

        [Test]
        public void Omni()
        {
            OmniTest(RenderPipeline.BuiltIn);
        }

        [Test]
        public void AddImageFail()
        {
            AddImageFailTest(RenderPipeline.BuiltIn);
        }

        [Test]
        public void DoubleSided()
        {
            DoubleSidedTest(RenderPipeline.BuiltIn);
        }

        protected override void SetUpExporter()
        {
            m_Exporter = new GltfBuiltInShaderMaterialExporter();
        }
    }
}
