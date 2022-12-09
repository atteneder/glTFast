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
using System.Collections.Generic;

using UnityEngine;

namespace GLTFast.Logging
{

    /// <summary>
    /// Logger that stores/collects all messages.
    /// </summary>
    [Serializable]
    public class CollectingLogger : ICodeLogger
    {

        List<LogItem> m_Items;

        /// <inheritdoc />
        public void Error(LogCode code, params string[] messages)
        {
            if (m_Items == null)
            {
                m_Items = new List<LogItem>();
            }
            m_Items.Add(new LogItem(LogType.Error, code, messages));
        }

        /// <inheritdoc />
        public void Warning(LogCode code, params string[] messages)
        {
            if (m_Items == null)
            {
                m_Items = new List<LogItem>();
            }
            m_Items.Add(new LogItem(LogType.Warning, code, messages));
        }

        /// <inheritdoc />
        public void Info(LogCode code, params string[] messages)
        {
            if (m_Items == null)
            {
                m_Items = new List<LogItem>();
            }
            m_Items.Add(new LogItem(LogType.Log, code, messages));
        }

        /// <inheritdoc />
        public void Error(string message)
        {
            if (m_Items == null)
            {
                m_Items = new List<LogItem>();
            }
            m_Items.Add(new LogItem(LogType.Error, LogCode.None, message));
        }

        /// <inheritdoc />
        public void Warning(string message)
        {
            if (m_Items == null)
            {
                m_Items = new List<LogItem>();
            }
            m_Items.Add(new LogItem(LogType.Warning, LogCode.None, message));
        }

        /// <inheritdoc />
        public void Info(string message)
        {
            if (m_Items == null)
            {
                m_Items = new List<LogItem>();
            }
            m_Items.Add(new LogItem(LogType.Log, LogCode.None, message));
        }

        /// <summary>
        /// Logs all collected messages to the console.
        /// </summary>
        public void LogAll()
        {
            if (m_Items != null)
            {
                foreach (var item in m_Items)
                {
                    item.Log();
                }
            }
        }

        /// <summary>
        /// Number of log items in <see cref="Items"/>
        /// </summary>
        public int Count => m_Items?.Count ?? 0;

        /// <summary>
        /// Items that were logged
        /// </summary>
        public IEnumerable<LogItem> Items => m_Items?.AsReadOnly();
    }

    /// <summary>
    /// Encapsulates a single log message.
    /// </summary>
    [Serializable]
    public class LogItem
    {

        /// <summary>
        /// The severeness type of the log message.
        /// </summary>
        public LogType Type => type;

        /// <summary>
        /// Message code
        /// </summary>
        public LogCode Code => code;

        /// <summary>
        /// Additional, optional message parts
        /// </summary>
        public string[] Messages => messages;

        [SerializeField]
        LogType type;

        [SerializeField]
        LogCode code;

        [SerializeField]
        string[] messages;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="type">The severeness type of the log message</param>
        /// <param name="code">Message code</param>
        /// <param name="messages">Additional, optional message parts</param>
        public LogItem(LogType type, LogCode code, params string[] messages)
        {
            this.type = type;
            this.code = code;
            this.messages = messages;
        }

        /// <summary>
        /// Logs the message to the console
        /// </summary>
        public void Log()
        {
            Debug.LogFormat(Type, LogOption.NoStacktrace, null, LogMessages.GetFullMessage(Code, Messages));
        }

        /// <summary>
        /// Returns the full log message
        /// </summary>
        /// <returns>Log message</returns>
        public override string ToString()
        {
            return LogMessages.GetFullMessage(Code, Messages);
        }
    }
}
