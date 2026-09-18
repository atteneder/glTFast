// SPDX-FileCopyrightText: 2024 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Unity.Cloud.Gltfast
{
    [Flags]
    enum MainBufferType
    {
        None = 0x0,
        Position = 0x1,
        Normal = 0x2,
        Tangent = 0x4,

        PosNorm = Position | Normal,
        PosNormTan = Position | Normal | Tangent,
    }
}
