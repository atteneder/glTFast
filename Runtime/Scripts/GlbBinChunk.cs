// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using UnityEngine;

namespace GLTFast
{
    readonly struct GlbBinChunk
    {
        public int Start { get; }

        public uint Length { get; }

        public GlbBinChunk(int start, uint length)
        {
            Start = start;
            Length = length;
        }
    }
}
