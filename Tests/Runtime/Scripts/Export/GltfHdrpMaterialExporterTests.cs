// SPDX-FileCopyrightText: 2024 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using GLTFast.Export;
using NUnit.Framework;
using UnityEngine;

namespace GLTFast.Tests.Export
{
    [TestFixture, Category("Export")]
    class GltfHdrpMaterialExporterTests : MaterialExportTests
    {
        [Test]
        public void BaseColor()
        {
            CertifyRequirements();
            BaseColorTest(RenderPipeline.HighDefinition);
        }

        [Test]
        public void BaseColorTexture()
        {
            CertifyRequirements();
            BaseColorTextureTest(RenderPipeline.HighDefinition);
        }

        [Test]
        public void BaseColorTextureTranslated()
        {
            CertifyRequirements();
            BaseColorTextureTranslatedTest(RenderPipeline.HighDefinition);
        }

        [Test]
        public void BaseColorTextureScaled()
        {
            CertifyRequirements();
            BaseColorTextureScaledTest(RenderPipeline.HighDefinition);
        }

        [Test]
        public void BaseColorTextureRotated()
        {
            CertifyRequirements();
            BaseColorTextureRotatedTest(RenderPipeline.HighDefinition);
        }

        [Test]
        public void BaseColorTextureCutout()
        {
            CertifyRequirements();
#if !USING_URP || USING_URP_12_OR_NEWER
            BaseColorTextureCutoutTest(RenderPipeline.HighDefinition);
#else
            Assert.Ignore("Not testing legacy URP version older than 12.x.");
#endif
        }

        [Test]
        public void BaseColorTextureTransparent()
        {
            CertifyRequirements();
#if !USING_URP || USING_URP_12_OR_NEWER
            BaseColorTextureTransparentTest(RenderPipeline.HighDefinition);
#else
            Assert.Ignore("Not testing legacy URP version older than 12.x.");
#endif
        }

        [Test]
        public void RoughnessTexture()
        {
            CertifyRequirements();
            RoughnessTextureTest(RenderPipeline.HighDefinition);
        }

        [Test]
        public void Metallic()
        {
            CertifyRequirements();
            MetallicTest(RenderPipeline.HighDefinition);
        }

        [Test]
        public void MetallicTexture()
        {
            CertifyRequirements();
            MetallicTextureTest(RenderPipeline.HighDefinition);
        }

        [Test]
        public void MetallicRoughnessTexture()
        {
            CertifyRequirements();
            MetallicRoughnessTextureTest(RenderPipeline.HighDefinition);
        }

        [Test]
        public void MetallicRoughnessOcclusionTexture()
        {
            CertifyRequirements();
            MetallicRoughnessOcclusionTextureTest(RenderPipeline.HighDefinition);
        }

        [Test]
        public void OcclusionTexture()
        {
            CertifyRequirements();
            OcclusionTextureTest(RenderPipeline.HighDefinition);
        }

        [Test]
        public void EmissiveFactor()
        {
            CertifyRequirements();
            EmissiveFactorTest(RenderPipeline.HighDefinition);
        }

        [Test]
        public void EmissiveTexture()
        {
            CertifyRequirements();
            EmissiveTextureTest(RenderPipeline.HighDefinition);
        }

        [Test]
        public void EmissiveTextureFactor()
        {
            CertifyRequirements();
            EmissiveTextureFactorTest(RenderPipeline.HighDefinition);
        }

        [Test]
        public void NormalTexture()
        {
            CertifyRequirements();
            NormalTextureTest(RenderPipeline.HighDefinition);
        }

        [Test]
        public void NotGltf()
        {
            CertifyRequirements();
            NotGltfTest(RenderPipeline.HighDefinition);
        }

        [Test]
        public void Omni()
        {
            CertifyRequirements();
            OmniTest(RenderPipeline.HighDefinition);
        }

        [Test]
        public void AddImageFail()
        {
            CertifyRequirements();
            AddImageFailTest(RenderPipeline.HighDefinition);
        }

        [Test]
        public void DoubleSided()
        {
            CertifyRequirements();
#if !USING_URP || USING_URP_12_OR_NEWER
            DoubleSidedTest(RenderPipeline.HighDefinition);
#else
            Assert.Ignore("Not testing legacy URP version older than 12.x.");
#endif
        }

        static void CertifyRequirements()
        {
#if !UNITY_SHADER_GRAPH
            Assert.Ignore("Shader Graph package is missing...ignoring tests on Shader Graph based materials.");
#endif
            if (RenderPipeline.HighDefinition != RenderPipelineUtils.RenderPipeline)
            {
                Assert.Ignore("Test requires Universal Render Pipeline.");
            }
        }

        protected override void SetUpExporter()
        {
#if USING_HDRP
            m_Exporter = new GltfHdrpMaterialExporter();
#endif
        }
    }
}
