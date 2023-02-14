using GLTFast.Schema;

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Profiling;

namespace GLTFast
{
    public class JsonUtilityImplementation : IJsonImplementation
    {
        public Root ParseJson<T>(string json) where T : Root
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
    }
}
