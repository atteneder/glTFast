// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

#if UNITY_ANIMATION

using System;

namespace GLTFast.Schema
{
    [Serializable]
    public class AnimationChannelTarget {
        /// <summary>
        /// The index of the node to target.
        /// </summary>
        public int node;

        /// <summary>
        /// The name of the node's TRS property to modify.
        /// </summary>
        // Field is public for unified serialization only. Warn via Obsolete attribute.
        [Obsolete("Use GetPath for access.")]
        public string path;

        AnimationChannel.Path m_Path;

        public AnimationChannel.Path GetPath() {
            if (m_Path != AnimationChannel.Path.Unknown)
            {
                return m_Path;
            }

#pragma warning disable CS0618 // Type or member is obsolete
            if (!Enum.TryParse(path, true, out m_Path))
            {
                m_Path = AnimationChannel.Path.Invalid;
            }
            path = null;
#pragma warning restore CS0618 // Type or member is obsolete
            return m_Path;
        }

        internal void GltfSerialize(JsonWriter writer) {
            throw new NotImplementedException($"GltfSerialize missing on {GetType()}");
        }
    }
}
#endif // UNITY_ANIMATION
