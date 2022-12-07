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

using System;
using UnityEngine;

namespace GLTFast.Logging
{

    /// <summary>
    /// A logger that can receive log messages of severeness levels
    /// </summary>
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
