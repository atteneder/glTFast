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

namespace GLTFast {
    struct SamplerKey : IEquatable<SamplerKey> {
        private Sampler m_s;
        public Sampler Sampler {
            get { return m_s; }
        }

        public SamplerKey(Sampler sampler) {
            m_s = sampler;
        }

        public override int GetHashCode() {
            return (m_s.magFilter, m_s.minFilter, m_s.wrapS, m_s.wrapT).GetHashCode();
        }

        public bool Equals(SamplerKey other) {
            return m_s.magFilter == other.m_s.magFilter &&
                m_s.minFilter == other.m_s.minFilter &&
                m_s.wrapS == other.m_s.wrapS &&
                m_s.wrapT == other.m_s.wrapT;
        }

        public override bool Equals(object obj) => obj is SamplerKey other && Equals(other);
    }
}
