// Copyright 2020-2021 Andreas Atteneder
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

namespace GLTFast {

    /// <summary>
    /// An IDeferAgent can be used to interrupting the glTF loading procedure
    /// at certain points. This decision is always a trade-off between minimum
    /// loading time and a stable frame rate.
    /// </summary>
    public interface IDeferAgent {
        /// <summary>
        /// This will be called by GltfImport at various points in the loading procedure.
        /// </summary>
        /// <returns>True if the remaining work of the loading procedure should
        /// be deferred to the next frame/Update loop invocation. False if
        /// work can continue.</returns>
        bool ShouldDefer();

        /// <summary>
        /// Indicates if upcoming work should be deferred to the next frame.
        /// </summary>
        /// <param name="duration">Predicted duration of upcoming processing in seconds</param>
        /// <returns>True if the remaining work of the loading procedure should
        /// be deferred to the next frame/Update loop invocation. False if
        /// work can continue.</returns>
        bool ShouldDefer( float duration );

        /// <summary>
        /// Conditional yield. May continue right away or yield once, based on time.
        /// </summary>
        /// <returns>If <see cref="ShoudDefer"/> returns true, returns Task.Yield(). Otherwise returns sync</returns>
        Task BreakPoint();
        
        /// <summary>
        /// Conditional yield. May continue right away or yield once, based on time and duration.
        /// </summary>
        /// <param name="duration">Predicted duration of upcoming processing in seconds</param>
        /// <returns>If <see cref="ShoudDefer"/> returns true, returns Task.Yield(). Otherwise returns sync</returns>
        Task BreakPoint( float duration );
    }
}