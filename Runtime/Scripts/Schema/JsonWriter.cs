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

using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace GLTFast.Schema
{

    class JsonWriter
    {

        StreamWriter m_Stream;

        bool m_Separation;

        public JsonWriter(StreamWriter stream)
        {
            m_Stream = stream;
            OpenBrackets();
        }

        public void OpenBrackets()
        {
            m_Stream.Write('{');
            m_Separation = false;
        }

        public void AddProperty(string name)
        {
            Separate();
            m_Stream.Write('"');
            m_Stream.Write(name);
            m_Stream.Write("\":");
            m_Separation = false;
        }

        public void AddObject()
        {
            Separate();
            m_Stream.Write('{');
            m_Separation = false;
        }

        public void AddArray(string name)
        {
            Separate();
            m_Stream.Write('"');
            m_Stream.Write(name);
            m_Stream.Write("\":[");
            m_Separation = false;
        }

        public void CloseArray()
        {
            m_Stream.Write(']');
            m_Separation = true;
        }

        public void AddArrayProperty<T>(string name, IEnumerable<T> values)
        {
            AddArray(name);
            foreach (var value in values)
            {
                Separate();
                m_Stream.Write(value.ToString());
            }
            CloseArray();
        }

        public void AddArrayProperty(string name, IEnumerable<float> values)
        {
            AddArray(name);
            foreach (var value in values)
            {
                Separate();
                m_Stream.Write(value.ToString("R", CultureInfo.InvariantCulture));
            }
            CloseArray();
        }

        public void AddArrayProperty(string name, IEnumerable<string> values)
        {
            AddArray(name);
            foreach (var value in values)
            {
                Separate();
                m_Stream.Write('"');
                m_Stream.Write(value);
                m_Stream.Write('"');
            }
            CloseArray();
        }

        public void AddProperty<T>(string name, T value)
        {
            Separate();
            m_Stream.Write('"');
            m_Stream.Write(name);
            m_Stream.Write("\":");
            m_Stream.Write(value.ToString());
        }

        public void AddProperty(string name, float value)
        {
            Separate();
            m_Stream.Write('"');
            m_Stream.Write(name);
            m_Stream.Write("\":");
            m_Stream.Write(value.ToString("R", CultureInfo.InvariantCulture));
        }

        public void AddProperty(string name, string value)
        {
            Separate();
            m_Stream.Write('"');
            m_Stream.Write(name);
            m_Stream.Write("\":\"");
            m_Stream.Write(value);
            m_Stream.Write('"');
        }

        public void AddProperty(string name, bool value)
        {
            Separate();
            m_Stream.Write('"');
            m_Stream.Write(name);
            m_Stream.Write("\":");
            m_Stream.Write(value ? "true" : "false");
        }

        void Separate()
        {
            if (m_Separation)
            {
                m_Stream.Write(',');
            }
            m_Separation = true;
        }

        public void Close()
        {
            m_Stream.Write('}');
            m_Separation = true;
        }
    }
}
