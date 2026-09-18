// SPDX-FileCopyrightText: 2024 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

#if DEBUG && (!UNITY_WEBGL || UNITY_EDITOR)
#define LOG_CANCELLATION_SOURCE
#endif

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

#if LOG_CANCELLATION_SOURCE
using System.Diagnostics;
using System.Reflection;
#endif

namespace Unity.Cloud.Gltfast
{
    static class CancellationTokenExtension
    {
#if DEBUG
        internal static Action s_OnCancellationCheck;
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void ThrowIfCancellationRequestedWithTracking(this CancellationToken token)
        {
#if DEBUG
            s_OnCancellationCheck?.Invoke();

            if (token.IsCancellationRequested)
            {
#if LOG_CANCELLATION_SOURCE
                var stackTrace = new StackTrace();
                // frame 0 is this cancellation method, frame 1 is the target caller
                if (stackTrace.FrameCount > 1 && stackTrace.GetFrame(1).GetMethod() is MethodInfo methodInfo)
                    throw new OperationCanceledException($"{methodInfo.DeclaringType}.{methodInfo.Name}", token);
#endif
                throw new OperationCanceledException(token);
            }
#else // DEBUG
            token.ThrowIfCancellationRequested();
#endif // DEBUG
        }
    }
}
