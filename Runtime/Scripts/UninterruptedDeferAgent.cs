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

using System.Threading.Tasks;

namespace GLTFast
{

    /// <summary>
    /// <seealso cref="IDeferAgent"/> that always decides to continue
    /// processing
    /// </summary>
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
