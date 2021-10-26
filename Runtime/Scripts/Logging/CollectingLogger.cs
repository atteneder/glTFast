// Copyright 2020-2021 Andreas Atteneder
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

namespace GLTFast {

    [Serializable]
    public class CollectingLogger : ICodeLogger {

        public List<LogItem> items;

        public void Error(LogCode code, params string[] messages) {
            if (items == null) {
                items = new List<LogItem>();
            }
            items.Add(new LogItem(LogType.Error, code, messages));
        }
        
        public void Warning(LogCode code, params string[] messages) {
            if (items == null) {
                items = new List<LogItem>();
            }
            items.Add(new LogItem(LogType.Warning, code, messages));
        }
        
        public void Info(LogCode code, params string[] messages) {
            if (items == null) {
                items = new List<LogItem>();
            }
            items.Add(new LogItem(LogType.Log, code, messages));
        }
        
        public void Error(string message) {
            if (items == null) {
                items = new List<LogItem>();
            }
            items.Add(new LogItem(LogType.Error, LogCode.None, message ));
        }
        
        public void Warning(string message) {
            if (items == null) {
                items = new List<LogItem>();
            }
            items.Add(new LogItem(LogType.Warning, LogCode.None, message ));
        }
        
        public void Info(string message) {
            if (items == null) {
                items = new List<LogItem>();
            }
            items.Add(new LogItem(LogType.Log, LogCode.None, message ));
        }
        
        public void LogAll() {
            if (items != null) {
                foreach (var item in items) {
                    item.Log();
                }
            }
        }
    }
    
    [Serializable]
    public class LogItem {

        public LogType type = LogType.Error;
        public LogCode code;
        public string[] messages;

        public LogItem(LogType type, LogCode code, params string[] messages) {
            this.type = type;
            this.code = code;
            this.messages = messages;
        }

        public void Log() {
            Debug.LogFormat(type, LogOption.NoStacktrace,null,LogMessages.GetFullMessage(code,messages));
        }

        public override string ToString() {
            return LogMessages.GetFullMessage(code, messages);
        }
    }
}

