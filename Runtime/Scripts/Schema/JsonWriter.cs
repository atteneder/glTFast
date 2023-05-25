// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

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
