// SPDX-FileCopyrightText: 2026 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

#if UNITY_ANIMATION

using System;

namespace Unity.Cloud.Gltfast.Animations
{
    sealed class AnimationModuleProcessorFactory : IAnimationProcessorFactory
    {
        readonly bool m_Legacy;

        public AnimationModuleProcessorFactory(bool legacy)
        {
            m_Legacy = legacy;
        }

        public IAnimationProcessor CreateAnimationProcessor(int clipCount)
        {
            return new AnimationModuleProcessor(clipCount, m_Legacy);
        }
    }
}
#endif // UNITY_ANIMATION
