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
