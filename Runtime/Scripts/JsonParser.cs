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

#if NEWTONSOFT_JSON && GLTFAST_USE_NEWTONSOFT_JSON
#define JSON_NEWTONSOFT
#else
#define JSON_UTILITY
#endif

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Profiling;
#if JSON_NEWTONSOFT || (DEBUG && NEWTONSOFT_JSON)
using Newtonsoft.Json;
#endif

namespace GLTFast
{

    using Schema;

    static class JsonParser
    {
        internal static Root ParseJson<T>(string json) where T : Root
        {
#if JSON_UTILITY
            return ParseJsonJsonUtility<T>(json);
#else
            return ParseJsonNewtonsoftJson<T>(json);
#endif
        }

#if JSON_UTILITY || DEBUG
        internal static T ParseJsonJsonUtility<T>(string json) where T : Root
        {
            // JsonUtility sometimes creates non-null default instances of objects-type members
            // even though there are none in the original JSON.
            // This work-around makes sure not existent JSON nodes will be null in the result.
#if MEASURE_TIMINGS
            var stopWatch = new Stopwatch();
            stopWatch.Start();
#endif
            T root;

            // Main JSON parsing
            Profiler.BeginSample("JsonUtility main");
            try
            {
                root = JsonUtility.FromJson<T>(json);
            }
            catch (System.ArgumentException)
            {
                return null;
            }

            if (root == null)
            {
                return null;
            }
            Profiler.EndSample();

            // Detect, if a secondary null-check is necessary.
            if (root.JsonUtilitySecondParseRequired())
            {
                // If we have to make an explicit check, parse the JSON again with a
                // different, minimal Root class, where class members are serialized to
                // the type string. In case the string is null, there's no JSON node.
                // Otherwise the string would be empty ("").
                Profiler.BeginSample("JsonUtility secondary");
                var fakeRoot = JsonUtility.FromJson<FakeSchema.Root>(json);
                root.JsonUtilityCleanupAgainstSecondParse(fakeRoot);
                Profiler.EndSample();
            }

            // Further, generic checks and cleanups
            root.JsonUtilityCleanup();

#if MEASURE_TIMINGS
            stopWatch.Stop();
            var elapsedSeconds = stopWatch.ElapsedMilliseconds / 1000f;
            var throughput = json.Length / elapsedSeconds;
            Debug.Log($"JSON throughput: {throughput} bytes/sec ({json.Length} bytes in {elapsedSeconds} seconds)");
#endif
            return root;
        }
#endif // JSON_UTILITY || DEBUG

#if JSON_NEWTONSOFT || (DEBUG && NEWTONSOFT_JSON)
        internal static T ParseJsonNewtonsoftJson<T>(string json) where T : Root {
            return JsonConvert.DeserializeObject<T>(json);
        }
#endif // JSON_NEWTONSOFT
    }
}
