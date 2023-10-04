// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

#if UNITY_ANIMATION

using System;

namespace GLTFast.Schema
{
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
        // Field is public for unified serialization only. Warn via Obsolete attribute.
        [Obsolete("Use GetInterpolationType for access.")]
        public string interpolation;

        InterpolationType m_Interpolation;

        public InterpolationType GetInterpolationType() {
            if (m_Interpolation != InterpolationType.Unknown)
            {
                return m_Interpolation;
            }

#pragma warning disable CS0618 // Type or member is obsolete
            if (!Enum.TryParse(interpolation, true, out m_Interpolation))
            {
                m_Interpolation = InterpolationType.Linear;
            }
            interpolation = null;
#pragma warning restore CS0618 // Type or member is obsolete

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
