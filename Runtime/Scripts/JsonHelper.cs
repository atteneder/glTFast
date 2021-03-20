using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace GLTFast
{
    public static class JsonHelper
    {
        /// <summary>
        /// A utility function to work around the JsonUtility inability to deserialize to a dictionary.
        /// </summary>
        /// <param name="json">JSON string</param>
        /// <returns>A dictionary with the key value pairs found in the json</returns>
        private static Dictionary<string, int> StringIntDictionaryFromJson(string json)
        {
            string reformatted = JsonDictionaryToArray(json);
            StringIntKeyValueArray loadedData = JsonUtility.FromJson<StringIntKeyValueArray>(reformatted);
            Dictionary<string, int> dictionary = new Dictionary<string, int>();
            for (int i = 0; i < loadedData.items.Length; i++)
            {
                dictionary.Add(loadedData.items[i].key, loadedData.items[i].value);
            }
            return dictionary;
        }
        
        /// <summary>
        /// Takes a json object string with key value pairs, and returns a json string
        /// in the format of `{"items": [{"key": $key_name, "value": $value}]}`.
        /// This format can be handled by JsonUtility and support an aribtrary number
        /// of key/value pairs
        /// </summary>
        /// <param name="json">JSON string in the format `{"key": $value}`</param>
        /// <returns>Returns a reformatted JSON string</returns>
        private static string JsonDictionaryToArray(string json)
        {
            string reformatted = "{\"items\": [";
            string pattern = @"""(\w+)"":\s?(""?\w+""?)";
            RegexOptions options = RegexOptions.Multiline;

            foreach (Match m in Regex.Matches(json, pattern, options))
            {
                string key = m.Groups[1].Value;
                string value = m.Groups[2].Value;

                reformatted += $"{{\"key\":\"{key}\", \"value\":{value}}},";
            }
            reformatted = reformatted.TrimEnd(',');
            reformatted += "]}";
            return reformatted;
        }

        [Serializable]
        private class StringKeyValue
        {
            public string key = string.Empty;
            public int value = 0;
        }

        [Serializable]
        private class StringIntKeyValueArray
        {
            public StringKeyValue[] items = Array.Empty<StringKeyValue>();
        }

    }
}
