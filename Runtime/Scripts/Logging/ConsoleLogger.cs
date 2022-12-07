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
