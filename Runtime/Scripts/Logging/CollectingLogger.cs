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

namespace GLTFast.Logging {

    /// <summary>
    /// Logger that stores/collects all messages.
    /// </summary>
    [Serializable]
    public class CollectingLogger : ICodeLogger {

        List<LogItem> items;

        /// <inheritdoc />
        public void Error(LogCode code, params string[] messages) {
            if (items == null) {
                items = new List<LogItem>();
            }
            items.Add(new LogItem(LogType.Error, code, messages));
        }
        
        /// <inheritdoc />
        public void Warning(LogCode code, params string[] messages) {
            if (items == null) {
                items = new List<LogItem>();
            }
            items.Add(new LogItem(LogType.Warning, code, messages));
        }
        
        /// <inheritdoc />
        public void Info(LogCode code, params string[] messages) {
            if (items == null) {
                items = new List<LogItem>();
            }
            items.Add(new LogItem(LogType.Log, code, messages));
        }
        
        /// <inheritdoc />
        public void Error(string message) {
            if (items == null) {
                items = new List<LogItem>();
            }
            items.Add(new LogItem(LogType.Error, LogCode.None, message ));
        }
        
        /// <inheritdoc />
        public void Warning(string message) {
            if (items == null) {
                items = new List<LogItem>();
            }
            items.Add(new LogItem(LogType.Warning, LogCode.None, message ));
        }
        
        /// <inheritdoc />
        public void Info(string message) {
            if (items == null) {
                items = new List<LogItem>();
            }
            items.Add(new LogItem(LogType.Log, LogCode.None, message ));
        }
        
        /// <summary>
        /// Logs all collected messages to the console.
        /// </summary>
        public void LogAll() {
            if (items != null) {
                foreach (var item in items) {
                    item.Log();
                }
            }
        }
        
        /// <summary>
        /// Number of log items in <see cref="Items"/>
        /// </summary>
        public int Count {
            get { return items?.Count ?? 0; }
        }

        /// <summary>
        /// Items that were logged
        /// </summary>
        public IEnumerable<LogItem> Items { get { return items?.AsReadOnly(); } }
    }
    
    /// <summary>
    /// Encapsulates a single log message.
    /// </summary>
    [Serializable]
    public class LogItem {

        /// <summary>
        /// The severeness type of the log message.
        /// </summary>
        public LogType type;
        
        /// <summary>
        /// Message code
        /// </summary>
        public LogCode code;
        
        /// <summary>
        /// Additional, optional message parts
        /// </summary>
        public string[] messages;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="type">The severeness type of the log message</param>
        /// <param name="code">Message code</param>
        /// <param name="messages">Additional, optional message parts</param>
        public LogItem(LogType type, LogCode code, params string[] messages) {
            this.type = type;
            this.code = code;
            this.messages = messages;
        }

        /// <summary>
        /// Logs the message to the console
        /// </summary>
        public void Log() {
            Debug.LogFormat(type, LogOption.NoStacktrace,null,LogMessages.GetFullMessage(code,messages));
        }

        /// <summary>
        /// Returns the full log message
        /// </summary>
        /// <returns>Log message</returns>
        public override string ToString() {
            return LogMessages.GetFullMessage(code, messages);
        }
    }
}

