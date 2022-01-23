using System;
using GLTFast.Schema;

namespace GLTFast {
    internal struct SamplerKey : IEquatable<SamplerKey> {
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
