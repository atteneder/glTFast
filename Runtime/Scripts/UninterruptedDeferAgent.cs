// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System.Threading.Tasks;

namespace GLTFast
{

    /// <summary>
    /// Defer agent that always decides to continue
    /// processing
    /// </summary>
    /// <seealso cref="IDeferAgent"/>
    public class UninterruptedDeferAgent : IDeferAgent
    {
        /// <inheritdoc />
        public bool ShouldDefer()
        {
            return false;
        }

        /// <inheritdoc />
        public bool ShouldDefer(float duration)
        {
            return false;
        }

#pragma warning disable 1998
        /// <inheritdoc />
        public async Task BreakPoint() { }
        /// <inheritdoc />
        public async Task BreakPoint(float duration) { }
#pragma warning restore 1998
    }
}
