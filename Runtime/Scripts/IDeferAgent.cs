// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

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
