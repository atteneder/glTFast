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

#if UNITY_ANIMATION

using System;

namespace GLTFast.Schema {

    [Serializable]
    public class GltfAnimation : NamedObject
    {
        /// <summary>
        /// An array of channels, each of which targets an animation's sampler at a
        /// node's property. Different channels of the same animation can't have equal
        /// targets.
        /// </summary>
        public AnimationChannel[] channels;

        /// <summary>
        /// An array of samplers that combines input and output accessors with an
        /// interpolation algorithm to define a keyframe graph (but not its target).
        /// </summary>
        public AnimationSampler[] samplers;

        internal void GltfSerialize(JsonWriter writer) {
            writer.AddObject();
            GltfSerializeRoot(writer);
            writer.Close();
            throw new NotImplementedException($"GltfSerialize missing on {GetType()}");
        }
    }

    [Serializable]
    public class AnimationChannel
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
        public AnimationChannelTarget target;

        internal void GltfSerialize(JsonWriter writer) {
            throw new NotImplementedException($"GltfSerialize missing on {GetType()}");
        }
    }

    [Serializable]
    public class AnimationChannelTarget {
        /// <summary>
        /// The index of the node to target.
        /// </summary>
        public int node;

        /// <summary>
        /// The name of the node's TRS property to modify.
        /// </summary>
        public string path;

        AnimationChannel.Path m_Path;

        public AnimationChannel.Path GetPath() {
            if (m_Path == AnimationChannel.Path.Unknown) {
                if (!string.IsNullOrEmpty(path)) {
                    try {
                        m_Path = (AnimationChannel.Path)Enum.Parse(typeof(AnimationChannel.Path), path, true);
                    }
                    catch (ArgumentException) {
                        m_Path = AnimationChannel.Path.Invalid;
                    }

                    path = null;
                    return m_Path;
                }

                return AnimationChannel.Path.Invalid;
            }

            return m_Path;
        }

        internal void GltfSerialize(JsonWriter writer) {
            throw new NotImplementedException($"GltfSerialize missing on {GetType()}");
        }
    }

    public enum InterpolationType
    {
        Unknown,
        Linear,
        Step,
        CubicSpline
    }

    [Serializable]
    public class AnimationSampler
    {
        /// <summary>
        /// The index of an accessor containing keyframe input values, e.G., time.
        /// That accessor must have componentType `FLOAT`. The values represent time in
        /// seconds with `time[0] >= 0.0`, and strictly increasing values,
        /// i.e., `time[n + 1] > time[n]`
        /// </summary>
        public int input;

        /// <summary>
        /// Interpolation algorithm. When an animation targets a node's rotation,
        /// and the animation's interpolation is `\"LINEAR\"`, spherical linear
        /// interpolation (slerp) should be used to interpolate quaternions. When
        /// interpolation is `\"STEP\"`, animated value remains constant to the value
        /// of the first point of the timeframe, until the next timeframe.
        /// </summary>
        public string interpolation;

        InterpolationType m_Interpolation;

        public InterpolationType GetInterpolationType() {
            if (m_Interpolation == InterpolationType.Unknown) {
                if (!string.IsNullOrEmpty(interpolation)) {
                    m_Interpolation = (InterpolationType)Enum.Parse(typeof(InterpolationType), interpolation, true);
                    interpolation = null;
                    return m_Interpolation;
                }

                m_Interpolation = InterpolationType.Linear;
            }

            return m_Interpolation;
        }

        /// <summary>
        /// The index of an accessor, containing keyframe output values. Output and input
        /// accessors must have the same `count`. When sampler is used with TRS target,
        /// output accessor's componentType must be `FLOAT`.
        /// </summary>
        public int output;
    }
}

#endif // UNITY_ANIMATION
