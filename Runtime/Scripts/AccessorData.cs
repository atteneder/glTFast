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

using System;
using System.Runtime.InteropServices;
using UnityEngine;
using Unity.Collections;

namespace GLTFast
{
    [Flags]
    enum AccessorUsage {
        Unknown = 0,
        Ignore = 0x1,
        Index = 0x2,
        IndexFlipped = 0x4,
        Position = 0x8,
        Normal = 0x10,
        Tangent = 0x20,
        UV = 0x40,
        Color = 0x80,
        InverseBindMatrix = 0x100,
        AnimationTimes = 0x200,
        Translation = 0x400,
        Rotation = 0x800,
        Scale = 0x1000,
        Weight = 0x2000,
        RequiredForInstantiation = 0x4000
    }

    abstract class AccessorDataBase {
        public abstract void Unpin();
        public abstract void Dispose();
    }

    class AccessorData<T> : AccessorDataBase {
        public T[] data;
        public GCHandle gcHandle;

        public override void Unpin() {
            gcHandle.Free();
        }
        public override void Dispose() {}
    }

    class AccessorNativeData<T> : AccessorDataBase where T: struct
    {
        public NativeArray<T> data;
        public override void Unpin() {}
        public override void Dispose() {
            data.Dispose();
        }
    }
}
