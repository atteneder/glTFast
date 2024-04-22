// SPDX-FileCopyrightText: 2024 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections;
using System.Threading.Tasks;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;

namespace GLTFast.Tests
{
    static class Utils
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

            void CheckExceptionAndTimeout()
            {
                if (task.Exception != null)
                    throw task.Exception;
                if (timeout > 0 && Time.realtimeSinceStartup - startTime > timeout)
                {
                    throw new System.TimeoutException();
                }
            }
            while (!task.IsCompleted)
            {
                CheckExceptionAndTimeout();
                yield return null;
            }

            CheckExceptionAndTimeout();
        }

        public static void AssertNearOrEqual(float4 reference, float4 value, float epsilon = float.Epsilon)
        {
            var delta = math.abs(reference - value);
            var maxDelta = math.max(delta.x, math.max(delta.y, math.max(delta.z, delta.w)));
            if (maxDelta > epsilon)
            {
                throw new AssertionException($"float4 not equal. expected {reference} got {value} (delta {maxDelta})");
            }
        }

        public static void AssertNearOrEqual(float3 reference, float3 value, float epsilon = float.Epsilon)
        {
            var delta = math.abs(reference - value);
            var maxDelta = math.max(delta.x, math.max(delta.y, delta.z));
            if (maxDelta > epsilon)
            {
                throw new AssertionException($"float3 not equal. expected {reference} got {value} (delta {maxDelta})");
            }
        }

        public static void AssertNearOrEqual(float2 reference, float2 value, float epsilon = float.Epsilon)
        {
            var delta = math.abs(reference - value);
            var maxDelta = math.max(delta.x, delta.y);
            if (maxDelta > epsilon)
            {
                throw new AssertionException($"float2 not equal. expected {reference} got {value} (delta {maxDelta})");
            }
        }

        public static void AssertNearOrEqual(float reference, float value, float epsilon = float.Epsilon)
        {
            var delta = math.abs(reference - value);
            if (delta > epsilon)
            {
                throw new AssertionException($"float not equal. expected {reference} got {value} (delta {delta})");
            }
        }

        public static void AssertNearOrEqual(Color reference, Color value, float epsilon = float.Epsilon)
        {
            AssertNearOrEqual(
                new float4(reference.r, reference.g, reference.b, reference.a),
                new float4(value.r, value.g, value.b, value.a),
                epsilon
                );
        }

        public static void AssertNearOrEqual(Color reference, float4 value, float epsilon = float.Epsilon)
        {
            AssertNearOrEqual(
                new float4(reference.r, reference.g, reference.b, reference.a),
                value,
                epsilon
            );
        }

        public static void AssertNearOrEqual(uint4 reference, uint4 value)
        {
            var b = reference != value;
            if (b.x || b.y || b.z || b.w)
            {
                throw new AssertionException($"float4 not equal. expected {reference} got {value}");
            }
        }
    }
}
