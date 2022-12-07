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
    /// An IDeferAgent decides whether to interrupt a preempt-able procedure
    /// running on the main thread at the current point in time.
    /// This decision manages the trade-off between minimum procedure duration
    /// and a responsive frame rate.
    /// </summary>
    public interface IDeferAgent
    {
        /// <summary>
        /// This will be called at various points in the loading procedure.
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
        bool ShouldDefer(float duration);

        /// <summary>
        /// Conditional yield. May continue right away or yield once, based on time.
        /// </summary>
        /// <returns>If <see cref="ShouldDefer()"/> returns true, returns Task.Yield(). Otherwise returns sync</returns>
        Task BreakPoint();

        /// <summary>
        /// Conditional yield. May continue right away or yield once, based on time and duration.
        /// </summary>
        /// <param name="duration">Predicted duration of upcoming processing in seconds</param>
        /// <returns>If <see cref="ShouldDefer(float)"/> returns true, returns Task.Yield(). Otherwise returns sync</returns>
        Task BreakPoint(float duration);
    }
}
