// SPDX-FileCopyrightText: 2026 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Unity.Cloud.Gltfast.Addons
{
    interface IPostBeginSceneInstanceApplier : IInstanceApplier
    {
        void PostBeginScene();
    }
}
