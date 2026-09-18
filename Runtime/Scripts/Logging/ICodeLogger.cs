// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Unity.Cloud.Gltfast.Logging
{

    /// <summary>
    /// A logger that can receive log messages of severeness levels
    /// </summary>
    [MovedFrom(true, sourceNamespace: "GLTFast.Logging", sourceAssembly: "glTFast")]
    public interface ICodeLogger
    {

        /// <summary>
        /// Dispatches a critical error message.
        /// </summary>
        /// <param name="code">Message's log code</param>
        /// <param name="messages">Additional, optional message parts</param>
        void Error(LogCode code, params string[] messages);

        /// <summary>
        /// Dispatches a warning message.
        /// </summary>
        /// <param name="code">Message's log code</param>
        /// <param name="messages">Additional, optional message parts</param>
        void Warning(LogCode code, params string[] messages);

        /// <summary>
        /// Dispatches an informational message.
        /// </summary>
        /// <param name="code">Message's log code</param>
        /// <param name="messages">Additional, optional message parts</param>
        void Info(LogCode code, params string[] messages);

        /// <summary>
        /// Dispatches a message, routing it to Info/Warning/Error based on the log type.
        /// </summary>
        /// <param name="logType">Type of message e.g. warn or error etc.</param>
        /// <param name="code">Message's log code</param>
        /// <param name="messages">Additional, optional message parts</param>
        void Log(LogType logType, LogCode code, params string[] messages);

        /// <summary>
        /// Dispatches a critical error message.
        /// </summary>
        /// <param name="message">Message to send</param>
        void Error(string message);

        /// <summary>
        /// Dispatches a warning message.
        /// </summary>
        /// <param name="message">Message to send</param>
        void Warning(string message);

        /// <summary>
        /// Dispatches an informational message.
        /// </summary>
        /// <param name="message">Message to send</param>
        void Info(string message);
    }
}
