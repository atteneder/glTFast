// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.InteropServices;
using Unity.Jobs;

namespace GLTFast
{

    struct ImageCreateContext
    {
        public int imageIndex;
        public byte[] buffer;
        public GCHandle gcHandle;
        public JobHandle jobHandle;
    }
}
