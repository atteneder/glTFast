// SPDX-FileCopyrightText: 2026 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using UnityEngine;

namespace Unity.Cloud.Gltfast.Logging
{
    /// <summary>
    /// A no-op <see cref="ICodeLogger"/> that drops every message. Pass
    /// <see cref="Instance"/> to opt out of glTFast logging. See the 7.0 upgrade
    /// guide for the security and privacy implications of silencing output.
    /// </summary>
    /// <seealso cref="ConsoleLogger"/>
    /// <seealso cref="CollectingLogger"/>
    public sealed class NullLogger : ICodeLogger
    {
        /// <summary>
        /// Shared no-allocation instance. Prefer this over <c>new NullLogger()</c>
        /// when opting out of glTFast logging.
        /// </summary>
        public static readonly NullLogger Instance = new NullLogger();

        /// <inheritdoc />
        public void Error(LogCode code, params string[] messages) { }

        /// <inheritdoc />
        public void Warning(LogCode code, params string[] messages) { }

        /// <inheritdoc />
        public void Info(LogCode code, params string[] messages) { }

        /// <inheritdoc />
        public void Log(LogType logType, LogCode code, params string[] messages) { }

        /// <inheritdoc />
        public void Error(string message) { }

        /// <inheritdoc />
        public void Warning(string message) { }

        /// <inheritdoc />
        public void Info(string message) { }
    }
}
