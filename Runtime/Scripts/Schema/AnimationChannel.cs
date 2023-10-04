// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

#if UNITY_ANIMATION

using System;

namespace GLTFast.Schema
{

    [Serializable]
    public class AnimationChannel : AnimationChannelBase<AnimationChannelTarget> { }

    [Serializable]
    public abstract class AnimationChannelBase<TTarget> : AnimationChannelBase
    where TTarget : AnimationChannelTarget
    {
        public TTarget target;

        public override AnimationChannelTarget Target => target;
    }

    [Serializable]
    public abstract class AnimationChannelBase
    {
        public enum Path {
            Unknown,
            Invalid,
            Translation,
            Rotation,
            Scale,
            Weights,
            Pointer
        }

        /// <summary>
        /// The index of a sampler in this animation used to compute the value for the
        /// target, e.g., a node's translation, rotation, or scale (TRS).
        /// </summary>
        public int sampler;

        /// <summary>
        /// The index of the node and TRS property to target.
        /// </summary>
        public abstract AnimationChannelTarget Target { get; }

        internal void GltfSerialize(JsonWriter writer) {
            throw new NotImplementedException($"GltfSerialize missing on {GetType()}");
        }
    }
}
#endif // UNITY_ANIMATION
