// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

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
        public void Log(LogType logType, LogCode code, params string[] messages)
        {
            if (m_Items == null)
            {
                m_Items = new List<LogItem>();
            }
            m_Items.Add(new LogItem(logType, code, messages));
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

        /// <inheritdoc cref="System.Object.GetHashCode"/>
        public override int GetHashCode()
        {
#if NET_STANDARD
            var hash = new HashCode();
            hash.Add(Type);
            hash.Add(Code);
            if (Messages != null) {
                foreach (var id in Messages) {
                    hash.Add(id);
                }
            }
            return hash.ToHashCode();
#else
            var hash = 17;
            hash = hash * 31 + Type.GetHashCode();
            hash = hash * 31 + Code.GetHashCode();

            if (Messages != null)
            {
                hash = hash * 31 + Messages.Length;
                foreach (var message in Messages)
                {
                    hash = hash * 31 + message.GetHashCode();
                }
            }
            return hash;
#endif
        }

        /// <inheritdoc cref="System.Object.Equals(object)"/>
        public override bool Equals(object obj)
        {
            //Check for null and compare run-time types.
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            return Equals((LogItem)obj);
        }

        bool Equals(LogItem other)
        {
            if (Type != other.Type || Code != other.Code) return false;
            if (Messages == null ^ other.Messages == null) return false;
            return Messages == null || AreEqual(Messages, other.Messages);
        }

        static bool AreEqual(IReadOnlyList<string> a, IReadOnlyList<string> b)
        {
            if (a.Count == b.Count)
            {
                for (var i = 0; i < a.Count; i++)
                {
                    if (!a[i].Equals(b[i])) return false;
                }

                return true;
            }
            return false;
        }
    }
}
