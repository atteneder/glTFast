// SPDX-FileCopyrightText: 2026 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

#if UNITY_ANIMATION

using System;
using Unity.Cloud.Gltfast.Addons;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Unity.Cloud.Gltfast.Animations
{
    class AnimationModuleDataInstanceApplierFactory : DataInstanceApplierFactory<AnimationClip[]>
    {
        public AnimationModuleDataInstanceApplierFactory(AnimationClip[] data)
            : base(data) { }

        public override IInstanceApplier CreateInstanceApplier(IInstantiator instantiator)
        {
            return new AnimationInstanceApplier(instantiator, Data);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DestroyUtils.SafeDestroy(Data);
            }
        }
    }
}
#endif // UNITY_ANIMATION
