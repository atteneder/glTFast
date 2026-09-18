// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Threading.Tasks;
using UnityEngine.Scripting.APIUpdating;

namespace Unity.Cloud.Gltfast
{

    /// <summary>
    /// Defer agent that always decides to continue
    /// processing
    /// </summary>
    /// <seealso cref="IDeferAgent"/>
    [MovedFrom(true, sourceNamespace: "GLTFast", sourceAssembly: "glTFast")]
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

        [Obsolete("BreakPoint has been renamed to BreakPointAsync. (UnityUpgradable) -> BreakPointAsync(*)", true)]
        public Task BreakPoint() => BreakPointAsync();

        [Obsolete("BreakPoint has been renamed to BreakPointAsync. (UnityUpgradable) -> BreakPointAsync(*)", true)]
        public Task BreakPoint(float duration) => BreakPointAsync(duration);

#pragma warning disable 1998
        /// <inheritdoc />
        public async Task BreakPointAsync() { }
        /// <inheritdoc />
        public async Task BreakPointAsync(float duration) { }
#pragma warning restore 1998
    }
}
