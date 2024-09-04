// SPDX-FileCopyrightText: 2024 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GLTFast.Export;
using GLTFast.Schema;
using Unity.Mathematics;
using UnityEngine;
using Camera = UnityEngine.Camera;
using Material = UnityEngine.Material;
using Mesh = UnityEngine.Mesh;
using Texture = GLTFast.Schema.Texture;

namespace GLTFast.Tests.Export
{
    class GltfWritableMock : IGltfWritable
    {
        public List<ImageExportBase> imageExports = new List<ImageExportBase>();
        public List<Sampler> samplers = new List<Sampler>();
        public List<Texture> textures = new List<Texture>();

        public Dictionary<Extension, bool> extensions = new Dictionary<Extension, bool>();

        List<SamplerKey> m_SamplerKeys;

        bool m_ImageConversion;

        public GltfWritableMock(bool imageConversion = true)
        {
            m_ImageConversion = imageConversion;
        }

        public uint AddNode(float3? translation = null, quaternion? rotation = null, float3? scale = null, uint[] children = null, string name = null)
        {
            throw new NotImplementedException();
        }

        public void AddMeshToNode(int nodeId, Mesh uMesh, int[] materialIds)
        {
            AddMeshToNode(nodeId, uMesh, materialIds, true);
        }

        public void AddMeshToNode(int nodeId, Mesh uMesh, int[] materialIds, bool skinning)
        {
            throw new NotImplementedException();
        }

        public void AddMeshToNode(int nodeId, Mesh uMesh, int[] materialIds, uint[] joints)
        {
            throw new NotImplementedException();
        }

        public void AddCameraToNode(int nodeId, int cameraId)
        {
            throw new NotImplementedException();
        }

        public void AddLightToNode(int nodeId, int lightId)
        {
            throw new NotImplementedException();
        }

        public bool AddMaterial(Material uMaterial, out int materialId, IMaterialExport materialExport)
        {
            throw new NotImplementedException();
        }

        public int AddImage(ImageExportBase imageExport)
        {
            if (!m_ImageConversion) return -1;

            var imageId = imageExports.IndexOf(imageExport);
            if (imageId >= 0)
            {
                return imageId;
            }

            imageId = imageExports.Count;
            imageExports.Add(imageExport);
            return imageId;
        }

        public int AddTexture(int imageId, int samplerId)
        {
            if (!m_ImageConversion) return -1;

            textures ??= new List<Texture>();

            var texture = new Texture
            {
                source = imageId,
                sampler = samplerId
            };

            var index = textures.IndexOf(texture);
            if (index >= 0)
            {
                return index;
            }

            textures.Add(texture);
            return textures.Count - 1;
        }

        public int AddSampler(FilterMode filterMode, TextureWrapMode wrapModeU, TextureWrapMode wrapModeV)
        {
            if (filterMode == FilterMode.Bilinear && wrapModeU == TextureWrapMode.Repeat && wrapModeV == TextureWrapMode.Repeat)
            {
                // This is the default, so no sampler needed
                return -1;
            }
            samplers ??= new List<Sampler>();
            m_SamplerKeys ??= new List<SamplerKey>();

            var samplerKey = new SamplerKey(filterMode, wrapModeU, wrapModeV);

            var index = m_SamplerKeys.IndexOf(samplerKey);
            if (index >= 0)
            {
                return index;
            }

            samplers.Add(new Sampler(filterMode, wrapModeU, wrapModeV));
            m_SamplerKeys.Add(samplerKey);
            return samplers.Count - 1;
        }

        public bool AddCamera(Camera uCamera, out int cameraId)
        {
            throw new NotImplementedException();
        }

        public bool AddLight(Light uLight, out int lightId)
        {
            throw new NotImplementedException();
        }

        public uint AddScene(uint[] nodes, string name = null)
        {
            throw new NotImplementedException();
        }

        public void RegisterExtensionUsage(Extension extension, bool required = true)
        {
            if (required || (extensions.TryGetValue(extension, out var oldRequired) && !oldRequired))
            {
                extensions[extension] = required;
            }
        }

        public Task<bool> SaveToFileAndDispose(string path)
        {
            throw new NotImplementedException();
        }

        public Task<bool> SaveToStreamAndDispose(Stream stream)
        {
            throw new NotImplementedException();
        }
    }
}
