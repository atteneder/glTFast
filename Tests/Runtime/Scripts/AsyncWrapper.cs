// SPDX-FileCopyrightText: 2024 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace GLTFast.Tests
{
    static class AsyncWrapper
    {
        /// <summary>
        /// Wraps a <see cref="Task"/> in an <see cref="IEnumerator"/>.
        /// </summary>
        /// <param name="task">The async Task to wait form</param>
        /// <param name="timeout">Optional timeout in seconds</param>
        /// <returns>IEnumerator</returns>
        /// <exception cref="AggregateException"></exception>
        /// <exception cref="TimeoutException">Thrown when a timout was set and the task took too long</exception>
        public static IEnumerator WaitForTask(Task task, float timeout = -1)
        {
            var startTime = Time.realtimeSinceStartup;

            while (!task.IsCompleted)
            {
                CheckExceptionAndTimeout();
                yield return null;
            }

            CheckExceptionAndTimeout();
            yield break;

            void CheckExceptionAndTimeout()
            {
                if (task.Exception != null)
                    throw task.Exception;
                if (timeout > 0 && Time.realtimeSinceStartup - startTime > timeout)
                {
                    throw new TimeoutException();
                }
            }
        }
    }
}
