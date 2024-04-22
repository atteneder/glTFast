// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using UnityEngine;

namespace GLTFast.Logging
{

    /// <summary>
    /// Logs messages directly to the console
    /// </summary>
    public class ConsoleLogger : ICodeLogger
    {

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
