// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Unity.Cloud.Gltfast.Logging
{

    /// <summary>
    /// Logs messages directly to the console
    /// </summary>
    [MovedFrom(true, sourceNamespace: "GLTFast.Logging", sourceAssembly: "glTFast")]
    public class ConsoleLogger : ICodeLogger
    {
        /// <summary>
        /// Shared no-allocation instance. Used as the default for the public entry
        /// points when no logger is passed. Prefer this over
        /// <c>new ConsoleLogger()</c>.
        /// </summary>
        public static readonly ConsoleLogger Instance = new();

        /// <inheritdoc />
        public void Error(LogCode code, params string[] messages)
        {
            Debug.LogError(LogMessages.GetFullMessage(code, messages));
        }

        /// <inheritdoc />
        public void Warning(LogCode code, params string[] messages)
        {
            Debug.LogWarning(LogMessages.GetFullMessage(code, messages));
        }

        /// <inheritdoc />
        public void Info(LogCode code, params string[] messages)
        {
            Debug.Log(LogMessages.GetFullMessage(code, messages));
        }

        /// <inheritdoc />
        public void Log(LogType logType, LogCode code, params string[] messages)
        {
            Debug.unityLogger.Log(logType, LogMessages.GetFullMessage(code, messages));
        }

        /// <inheritdoc />
        public void Error(string message)
        {
            Debug.LogError(message);
        }

        /// <inheritdoc />
        public void Warning(string message)
        {
            Debug.LogWarning(message);
        }

        /// <inheritdoc />
        public void Info(string message)
        {
            Debug.Log(message);
        }
    }
}
