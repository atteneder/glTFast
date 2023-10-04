// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

#if UNITY_ANIMATION

using System;

namespace GLTFast.Schema
{
    public enum InterpolationType
    {
        Unknown,
        Linear,
        Step,
        CubicSpline
    }
}
#endif // UNITY_ANIMATION
