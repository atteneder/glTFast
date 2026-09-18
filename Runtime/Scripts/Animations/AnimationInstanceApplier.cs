// SPDX-FileCopyrightText: 2026 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

#if UNITY_ANIMATION

using System;
using Unity.Cloud.Gltfast.Addons;
using UnityEngine;

namespace Unity.Cloud.Gltfast.Animations
{
    sealed class AnimationInstanceApplier : IPostBeginSceneInstanceApplier
    {
        AnimationClip[] Clips { get; }
        readonly IInstantiator m_Instantiator;

        public AnimationInstanceApplier(IInstantiator instantiator, AnimationClip[] clips)
        {
            m_Instantiator = instantiator;
            Clips = clips;
        }

        public void PostBeginScene()
        {
            m_Instantiator.AddAnimation(Clips);
        }
    }
}
#endif // UNITY_ANIMATION
