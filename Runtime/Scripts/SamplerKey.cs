// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using GLTFast.Schema;
using UnityEngine;

namespace GLTFast
{
    struct SamplerKey : IEquatable<SamplerKey>
    {

        FilterMode m_FilterMode;
        TextureWrapMode m_WrapModeU;
        TextureWrapMode m_WrapModeV;

        public SamplerKey(Sampler sampler)
        {
            m_FilterMode = sampler.FilterMode;
            m_WrapModeU = sampler.WrapU;
            m_WrapModeV = sampler.WrapV;
        }

        public SamplerKey(FilterMode filterMode, TextureWrapMode wrapModeU, TextureWrapMode wrapModeV)
        {
            m_FilterMode = filterMode;
            m_WrapModeU = wrapModeU;
            m_WrapModeV = wrapModeV;
        }

        public override int GetHashCode()
        {
            return (m_FilterMode, m_WrapModeU, m_WrapModeV).GetHashCode();
        }

        public bool Equals(SamplerKey other)
        {
            return m_FilterMode == other.m_FilterMode &&
                m_WrapModeU == other.m_WrapModeU &&
                m_WrapModeV == other.m_WrapModeV;
        }

        public override bool Equals(object obj) => obj is SamplerKey other && Equals(other);
    }
}
