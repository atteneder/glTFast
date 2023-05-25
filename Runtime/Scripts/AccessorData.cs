// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Runtime.InteropServices;
using UnityEngine;
using Unity.Collections;

namespace GLTFast
{
    [Flags]
    enum AccessorUsage
    {
        Unknown = 0,
        Ignore = 1 << 0,
        Index = 1 << 1,
        IndexFlipped = 1 << 2,
        Position = 1 << 3,
        Normal = 1 << 4,
        Tangent = 1 << 5,
        UV = 1 << 6,
        Color = 1 << 7,
        InverseBindMatrix = 1 << 8,
        AnimationTimes = 1 << 9,
        Translation = 1 << 10,
        Rotation = 1 << 11,
        Scale = 1 << 12,
        Weight = 1 << 13,
        RequiredForInstantiation = 1 << 14
    }

    abstract class AccessorDataBase
    {
        public abstract void Unpin();
        public abstract void Dispose();
    }

    class AccessorData<T> : AccessorDataBase
    {
        public T[] data;
        public GCHandle gcHandle;

        public override void Unpin()
        {
            gcHandle.Free();
        }
        public override void Dispose() { }
    }

    class AccessorNativeData<T> : AccessorDataBase where T : struct
    {
        public NativeArray<T> data;
        public override void Unpin() { }
        public override void Dispose()
        {
            data.Dispose();
        }
    }
}
