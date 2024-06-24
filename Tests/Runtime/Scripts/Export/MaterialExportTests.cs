// SPDX-FileCopyrightText: 2024 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using GLTFast.Export;
using GLTFast.Logging;
using GLTFast.Schema;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools.Utils;
using Material = UnityEngine.Material;

namespace GLTFast.Tests.Export
{
    abstract class MaterialExportTests
    {
        const string k_ResourcePath = "Export/Materials/";

        protected IMaterialExport m_Exporter;

        protected void BaseColorTest(RenderPipeline renderPipeline)
        {
            var material = ConvertMaterial("BaseColor", out _, renderPipeline);

            Assert.IsNotNull(material.pbrMetallicRoughness);
            Assert.AreEqual(new[] { .199999973f, .5, .75f, 1 }, material.pbrMetallicRoughness.baseColorFactor);
        }

        protected void BaseColorTextureTest(RenderPipeline renderPipeline)
        {
#if UNITY_IMAGECONVERSION
            var texture = BaseColorTextureTest(
                "BaseColorTexture",
                out var gltfWriter,
                renderPipeline,
                out _);
            Assert.IsNull(texture.extensions);
            Assert.AreEqual(0, gltfWriter.extensions.Count);
#else
            Assert.Ignore("Texture export is disabled! " + LogMessages.GetFullMessage(LogCode.ImageConversionNotEnabled));
#endif
        }

        protected void BaseColorTextureTranslatedTest(RenderPipeline renderPipeline)
        {
#if UNITY_IMAGECONVERSION
            var texture = BaseColorTextureTest(
                "BaseColorTextureTranslated",
                out var gltfWriter,
                renderPipeline,
                out _);
            Assert.IsTrue(gltfWriter.extensions[Extension.TextureTransform]);
            var transform = texture.extensions?.KHR_texture_transform;
            Assert.IsNotNull(transform);
            Assert.AreEqual(new[] { 0.4f, 0.6f }, transform.offset);
            Assert.AreEqual(new[] { 1f, 1f }, transform.scale);
            Assert.AreEqual(0, transform.rotation);
#else
            Assert.Ignore("Texture export is disabled! " + LogMessages.GetFullMessage(LogCode.ImageConversionNotEnabled));
#endif
        }


        protected void BaseColorTextureScaledTest(RenderPipeline renderPipeline)
        {
#if UNITY_IMAGECONVERSION
            var texture = BaseColorTextureTest(
                "BaseColorTextureScaled",
                out var gltfWriter,
                renderPipeline,
                out _);
            Assert.IsTrue(gltfWriter.extensions[Extension.TextureTransform]);
            var transform = texture.extensions?.KHR_texture_transform;
            Assert.IsNotNull(transform);
            Assert.AreEqual(new[] { 1.2f, 1.3f }, transform.scale);
            Assert.AreEqual(new[] { 0f, 0f }, transform.offset);
            Assert.AreEqual(0, transform.rotation);
#else
            Assert.Ignore("Texture export is disabled! " + LogMessages.GetFullMessage(LogCode.ImageConversionNotEnabled));
#endif
        }

        protected void BaseColorTextureCutoutTest(RenderPipeline renderPipeline)
        {
#if UNITY_IMAGECONVERSION
            BaseColorTextureTest(
                "BaseColorTextureCutout",
                out _,
                renderPipeline,
                out var material);
            Assert.AreEqual(MaterialBase.AlphaMode.Mask, material.GetAlphaMode());
            Assert.AreEqual(.6f, material.alphaCutoff);
#else
            Assert.Ignore("Texture export is disabled! " + LogMessages.GetFullMessage(LogCode.ImageConversionNotEnabled));
#endif
        }

        protected void BaseColorTextureTransparentTest(RenderPipeline renderPipeline)
        {
#if UNITY_IMAGECONVERSION
            BaseColorTextureTest(
                "BaseColorTextureTransparent",
                out _,
                renderPipeline,
                out var material);
            Assert.AreEqual(MaterialBase.AlphaMode.Blend, material.GetAlphaMode());
#else
            Assert.Ignore("Texture export is disabled! " + LogMessages.GetFullMessage(LogCode.ImageConversionNotEnabled));
#endif
        }

        protected void BaseColorTextureRotatedTest(RenderPipeline renderPipeline)
        {
#if UNITY_IMAGECONVERSION
            var texture = BaseColorTextureTest(
                "BaseColorTextureRotated",
                out var gltfWriter,
                renderPipeline,
                out _);
            Assert.IsTrue(gltfWriter.extensions[Extension.TextureTransform]);
            var transform = texture.extensions?.KHR_texture_transform;
            Assert.IsNotNull(transform);
            Assert.AreEqual(45, transform.rotation);
            Assert.AreEqual(new[] { 0f, 0f }, transform.offset);
            var comparer = new FloatEqualityComparer(10e-8f);
            Assert.That(transform.scale, Is.EquivalentTo(new[] { 1f, 1f }).Using(comparer));
#else
            Assert.Ignore("Texture export is disabled! " + LogMessages.GetFullMessage(LogCode.ImageConversionNotEnabled));
#endif
        }

        protected void RoughnessTextureTest(RenderPipeline renderPipeline)
        {
#if UNITY_IMAGECONVERSION
            var material = ConvertMaterial("RoughnessTexture", out var gltfWriter, renderPipeline);

            Assert.AreEqual(1, gltfWriter.imageExports.Count);
            Assert.AreEqual(1, gltfWriter.samplers.Count);
            Assert.AreEqual(1, gltfWriter.textures.Count);
            Assert.IsInstanceOf<ImageExport>(gltfWriter.imageExports[0]);

            var mrTexture = material.pbrMetallicRoughness?.metallicRoughnessTexture;
            Assert.IsNotNull(mrTexture);
            Assert.IsNull(mrTexture.extensions);

            Assert.AreEqual(.89f, material.pbrMetallicRoughness.roughnessFactor);
            Assert.AreEqual(0f, material.pbrMetallicRoughness.metallicFactor);
#else
            Assert.Ignore("Texture export is disabled! " + LogMessages.GetFullMessage(LogCode.ImageConversionNotEnabled));
#endif
        }

        protected void MetallicTest(RenderPipeline renderPipeline)
        {
            var material = ConvertMaterial("Metallic", out _, renderPipeline);
            Assert.IsNotNull(material.pbrMetallicRoughness);
            Assert.AreEqual(.89f, material.pbrMetallicRoughness.metallicFactor);
        }

        protected void MetallicTextureTest(RenderPipeline renderPipeline)
        {
#if UNITY_IMAGECONVERSION
            var material = ConvertMaterial("MetallicTexture", out var gltfWriter, renderPipeline);

            Assert.AreEqual(1, gltfWriter.imageExports.Count);
            Assert.AreEqual(0, gltfWriter.samplers.Count);
            Assert.AreEqual(1, gltfWriter.textures.Count);
            Assert.IsInstanceOf<ImageExport>(gltfWriter.imageExports[0]);

            Assert.IsNotNull(material.pbrMetallicRoughness);
            Assert.AreEqual(1f, material.pbrMetallicRoughness.metallicFactor);

            var mrTexture = material.pbrMetallicRoughness?.metallicRoughnessTexture;
            Assert.NotNull(mrTexture);

            var transform = mrTexture.extensions?.KHR_texture_transform;
            Assert.IsNotNull(transform);
            Assert.AreEqual(new[] { 2f, 2f }, transform.scale);
#else
            Assert.Ignore("Texture export is disabled! " + LogMessages.GetFullMessage(LogCode.ImageConversionNotEnabled));
#endif
        }

        protected void MetallicRoughnessTextureTest(RenderPipeline renderPipeline)
        {
#if UNITY_IMAGECONVERSION
            var material = ConvertMaterial("MetallicRoughnessTexture", out var gltfWriter, renderPipeline);

            Assert.AreEqual(1, gltfWriter.imageExports.Count);
            Assert.AreEqual(0, gltfWriter.samplers.Count);
            Assert.AreEqual(1, gltfWriter.textures.Count);
            Assert.IsInstanceOf<ImageExport>(gltfWriter.imageExports[0]);

            Assert.IsNotNull(material.pbrMetallicRoughness);
            Assert.AreEqual(1f, material.pbrMetallicRoughness.metallicFactor);
            Assert.AreEqual(1f, material.pbrMetallicRoughness.roughnessFactor);

            var mrTexture = material.pbrMetallicRoughness?.metallicRoughnessTexture;
            Assert.NotNull(mrTexture);
#else
            Assert.Ignore("Texture export is disabled! " + LogMessages.GetFullMessage(LogCode.ImageConversionNotEnabled));
#endif
        }

        protected void MetallicRoughnessOcclusionTextureTest(RenderPipeline renderPipeline)
        {
#if UNITY_IMAGECONVERSION
            var material = ConvertMaterial("MetallicRoughnessOcclusionTexture", out var gltfWriter, renderPipeline);

            Assert.AreEqual(1, gltfWriter.imageExports.Count);
            Assert.AreEqual(0, gltfWriter.samplers.Count);
            Assert.AreEqual(1, gltfWriter.textures.Count);
            Assert.IsInstanceOf<ImageExport>(gltfWriter.imageExports[0]);

            var mrTexture = material.pbrMetallicRoughness?.metallicRoughnessTexture;
            Assert.NotNull(mrTexture);

            var oTexture = material.occlusionTexture;
            Assert.NotNull(oTexture);
            Assert.AreEqual(.8f, oTexture.strength);
#else
            Assert.Ignore("Texture export is disabled! " + LogMessages.GetFullMessage(LogCode.ImageConversionNotEnabled));
#endif
        }

        protected void OcclusionTextureTest(RenderPipeline renderPipeline)
        {
#if UNITY_IMAGECONVERSION
            var material = ConvertMaterial("OcclusionTexture", out var gltfWriter, renderPipeline);

            Assert.AreEqual(1, gltfWriter.imageExports.Count);
            Assert.AreEqual(0, gltfWriter.samplers.Count);
            Assert.AreEqual(1, gltfWriter.textures.Count);
            Assert.IsInstanceOf<ImageExport>(gltfWriter.imageExports[0]);

            Assert.IsNotNull(material.pbrMetallicRoughness);
            Assert.AreEqual(0f, material.pbrMetallicRoughness.metallicFactor);

            var oTexture = material.occlusionTexture;
            Assert.NotNull(oTexture);
            Assert.AreEqual(.8f, oTexture.strength);

            var transform = oTexture.extensions?.KHR_texture_transform;
            Assert.IsNotNull(transform);
            Assert.AreEqual(new[] { 2f, 2f }, transform.scale);
#else
            Assert.Ignore("Texture export is disabled! " + LogMessages.GetFullMessage(LogCode.ImageConversionNotEnabled));
#endif
        }

        protected void EmissiveFactorTest(RenderPipeline renderPipeline)
        {
            var material = ConvertMaterial("EmissiveFactor", out _, renderPipeline);

            Assert.AreEqual(new Color(1, 1, 0), material.Emissive);
            Assert.IsNull(material.emissiveTexture);
        }

        protected void EmissiveTextureTest(RenderPipeline renderPipeline)
        {
#if UNITY_IMAGECONVERSION
            var material = ConvertMaterial("EmissiveTexture", out var gltfWriter, renderPipeline);

            Assert.AreEqual(1, gltfWriter.imageExports.Count);
            Assert.AreEqual(0, gltfWriter.samplers.Count);
            Assert.AreEqual(1, gltfWriter.textures.Count);
            Assert.IsInstanceOf<ImageExport>(gltfWriter.imageExports[0]);

            Assert.AreEqual(Color.white, material.Emissive);

            var texture = material.emissiveTexture;
            Assert.NotNull(texture);

            Assert.IsNull(texture.extensions?.KHR_texture_transform);
#else
            Assert.Ignore("Texture export is disabled! " + LogMessages.GetFullMessage(LogCode.ImageConversionNotEnabled));
#endif
        }

        protected void EmissiveTextureFactorTest(RenderPipeline renderPipeline)
        {
#if UNITY_IMAGECONVERSION
            var material = ConvertMaterial("EmissiveTextureFactor", out var gltfWriter, renderPipeline);

            Assert.AreEqual(1, gltfWriter.imageExports.Count);
            Assert.AreEqual(0, gltfWriter.samplers.Count);
            Assert.AreEqual(1, gltfWriter.textures.Count);
            Assert.IsInstanceOf<ImageExport>(gltfWriter.imageExports[0]);

            Assert.AreEqual(new Color(1, .7353569f, 0), material.Emissive);

            var texture = material.emissiveTexture;
            Assert.NotNull(texture);

            var transform = texture.extensions?.KHR_texture_transform;
            Assert.IsNotNull(transform);
            Assert.AreEqual(new[] { 2f, 3f }, transform.scale);
#else
            Assert.Ignore("Texture export is disabled! " + LogMessages.GetFullMessage(LogCode.ImageConversionNotEnabled));
#endif
        }

        protected void NormalTextureTest(RenderPipeline renderPipeline)
        {
#if UNITY_IMAGECONVERSION
            var material = ConvertMaterial("NormalTexture", out var gltfWriter, renderPipeline);

            Assert.AreEqual(1, gltfWriter.imageExports.Count);
            Assert.AreEqual(0, gltfWriter.samplers.Count);
            Assert.AreEqual(1, gltfWriter.textures.Count);
            Assert.IsInstanceOf<ImageExport>(gltfWriter.imageExports[0]);

            Assert.IsNotNull(material.pbrMetallicRoughness);

            var texture = material.normalTexture;
            Assert.NotNull(texture);
            Assert.AreEqual(1.1f, texture.scale);

            var transform = texture.extensions?.KHR_texture_transform;
            Assert.IsNotNull(transform);
            Assert.AreEqual(new[] { 1.5f, 1.2f }, transform.scale);
            Assert.AreEqual(new[] { 0f, 0f }, transform.offset);
            Assert.AreEqual(0, transform.rotation);
#else
            Assert.Ignore("Texture export is disabled! " + LogMessages.GetFullMessage(LogCode.ImageConversionNotEnabled));
#endif
        }

        protected void NotGltfTest(RenderPipeline renderPipeline)
        {
            var material = ConvertMaterial("NotGltf", out _, renderPipeline);
            Assert.IsNotNull(material);
        }

        protected void OmniTest(RenderPipeline renderPipeline)
        {
#if UNITY_IMAGECONVERSION
            var material = ConvertMaterial("Omni", out var gltfWriter, renderPipeline);

            Assert.AreEqual(4, gltfWriter.imageExports.Count);
            Assert.AreEqual(0, gltfWriter.samplers.Count);
            Assert.AreEqual(4, gltfWriter.textures.Count);
            Assert.IsInstanceOf<ImageExport>(gltfWriter.imageExports[0]);

            Assert.IsNotNull(material.pbrMetallicRoughness);
            Assert.AreEqual(new[] { 0.787412345f, 0.603827417f, 0.447988421f, 1 }, material.pbrMetallicRoughness.baseColorFactor);
            var baseColorTexture = material.pbrMetallicRoughness.baseColorTexture;
            Assert.IsNotNull(baseColorTexture);
            Assert.AreEqual(0, baseColorTexture.index);
            Assert.AreEqual(0, baseColorTexture.texCoord);

            Assert.AreEqual(1, gltfWriter.extensions.Count);

            var baseColorTransform = baseColorTexture.extensions?.KHR_texture_transform;
            Assert.IsNotNull(baseColorTransform);
            Assert.AreEqual(new[] { 0.4f, 0.6f }, baseColorTransform.offset);
            Assert.AreEqual(new[] { 1f, 1f }, baseColorTransform.scale);
            Assert.AreEqual(0, baseColorTransform.rotation);

            var mrTexture = material.pbrMetallicRoughness?.metallicRoughnessTexture;
            Assert.NotNull(mrTexture);

            var mrTransform = mrTexture.extensions?.KHR_texture_transform;
            Assert.IsNotNull(mrTransform);
            Assert.AreEqual(new[] { 1.11f, 1.21f }, mrTransform.scale);

            var normalTexture = material.normalTexture;
            Assert.NotNull(normalTexture);
            Assert.AreEqual(0.42f, normalTexture.scale);

            var normalTransform = normalTexture.extensions?.KHR_texture_transform;
            Assert.IsNotNull(normalTransform);
            Assert.AreEqual(new[] { 1.5f, 1.2f }, normalTransform.scale);
            Assert.AreEqual(new[] { 0f, 0f }, normalTransform.offset);
            Assert.AreEqual(0, normalTransform.rotation);

            var oTexture = material.occlusionTexture;
            Assert.NotNull(oTexture);
            Assert.AreEqual(.58f, oTexture.strength);

            var oTransform = oTexture.extensions?.KHR_texture_transform;
            Assert.IsNotNull(oTransform);
            Assert.AreEqual(new[] { 1.1f, 1.2f }, oTransform.scale);
#else
            Assert.Ignore("Texture export is disabled! " + LogMessages.GetFullMessage(LogCode.ImageConversionNotEnabled));
#endif
        }

        protected void AddImageFailTest(RenderPipeline renderPipeline)
        {
            const string name = "Omni";
            var gltfWriter = new GltfWritableMock(false);
            var uMaterial = Resources.Load<Material>($"{GetResourcePath(renderPipeline)}{name}");
            Assert.IsNotNull(uMaterial);
            var logger = new CollectingLogger();
            m_Exporter.ConvertMaterial(uMaterial, out var material, gltfWriter, logger);
            Assert.IsNotNull(material);
            LoggerTest.AssertLogger(logger);
        }

        protected void DoubleSidedTest(RenderPipeline renderPipeline)
        {
            var material = ConvertMaterial("DoubleSided", out _, renderPipeline);
            Assert.AreEqual(true, material.doubleSided);
        }

        TextureInfo BaseColorTextureTest(
            string name,
            out GltfWritableMock gltfWriter,
            RenderPipeline renderPipeline,
            out Schema.Material material
            )
        {
            material = ConvertMaterial(name, out gltfWriter, renderPipeline);

            Assert.AreEqual(1, gltfWriter.imageExports.Count);
            Assert.AreEqual(0, gltfWriter.samplers.Count);
            Assert.AreEqual(1, gltfWriter.textures.Count);
            Assert.IsInstanceOf<ImageExport>(gltfWriter.imageExports[0]);

            Assert.IsNotNull(material.pbrMetallicRoughness);
            Assert.AreEqual(new[] { 0.787412345f, 0.603827417f, 0.447988421f, 1 }, material.pbrMetallicRoughness.baseColorFactor);
            var texture = material.pbrMetallicRoughness.baseColorTexture;
            Assert.IsNotNull(texture);
            Assert.AreEqual(0, texture.index);
            Assert.AreEqual(0, texture.texCoord);
            return texture;
        }

        [OneTimeSetUp]
        public void SetUp()
        {
            SetUpExporter();
        }

        protected abstract void SetUpExporter();

        Schema.Material ConvertMaterial(
            string name,
            out GltfWritableMock gltfWriter,
            RenderPipeline renderPipeline
            )
        {
            gltfWriter = new GltfWritableMock();
            var uMaterial = Resources.Load<Material>($"{GetResourcePath(renderPipeline)}{name}");
            Assert.IsNotNull(uMaterial);
            var logger = new CollectingLogger();
            m_Exporter.ConvertMaterial(uMaterial, out var material, gltfWriter, logger);
            Assert.IsNotNull(material);
            LoggerTest.AssertLogger(logger);
            return material;
        }

        static string GetResourcePath(RenderPipeline renderPipeline)
        {
            switch (renderPipeline)
            {
                case RenderPipeline.BuiltIn:
                    return $"{k_ResourcePath}Built-In/";
                case RenderPipeline.Universal:
                    return $"{k_ResourcePath}URP/";
                case RenderPipeline.HighDefinition:
                    return $"{k_ResourcePath}HDRP/";
                case RenderPipeline.Unknown:
                default:
                    throw new ArgumentOutOfRangeException(nameof(renderPipeline), renderPipeline, null);
            }
        }
    }
}
