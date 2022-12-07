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

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Profiling;

namespace GLTFast
{

    using Schema;

    static class JsonParser
    {
        internal static Root ParseJson(string json)
        {
            // JsonUtility sometimes creates non-null default instances of objects-type members
            // even though there are none in the original JSON.
            // This work-around makes sure not existent JSON nodes will be null in the result.
#if MEASURE_TIMINGS
            var stopWatch = new Stopwatch();
            stopWatch.Start();
#endif
            Root root;

            // Step one: main JSON parsing
            Profiler.BeginSample("JSON main");
            try
            {
                root = JsonUtility.FromJson<Root>(json);
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

            // Step two:
            // detect, if a secondary null-check is necessary.
            Profiler.BeginSample("JSON extension check");
            bool check = false;
            if (root.materials != null)
            {
                for (int i = 0; i < root.materials.Length; i++)
                {
                    var mat = root.materials[i];
                    // mat.extension is always set (not null), because JsonUtility constructs a default
                    // if any of mat.extension's members is not null, it is because there was
                    // a legit extensions node in JSON => we have to check which ones
                    if (mat.extensions.KHR_materials_unlit != null)
                    {
                        check = true;
                    }
                    else
                    {
                        // otherwise dump the wrongfully constructed MaterialExtension
                        mat.extensions = null;
                    }
                }
            }
            if (root.accessors != null)
            {
                for (int i = 0; i < root.accessors.Length; i++)
                {
                    var accessor = root.accessors[i];
                    if (accessor.sparse.indices == null || accessor.sparse.values == null)
                    {
                        // If indices and values members are null, `sparse` is likely
                        // an auto-instance by the JsonUtility and not present in JSON.
                        // Therefore we remove it:
                        accessor.sparse = null;
                    }
#if GLTFAST_SAFE
                    else {
                        // This is very likely a valid sparse accessor.
                        // However, an empty sparse property ( "sparse": {} ) would break
                        // glTFast, so better do a thorough follow-up check
                        check = true;
                    }
#endif // GLTFAST_SAFE
                }
            }
#if DRACO_UNITY
            if(!check && root.meshes!=null) {
                foreach (var mesh in root.meshes) {
                    if (mesh.primitives != null) {
                        foreach (var primitive in mesh.primitives) {
                            if (primitive.extensions?.KHR_draco_mesh_compression != null) {
                                check = true;
                                break;
                            }
                        }
                    }
                }
            }
#endif
            Profiler.EndSample();

            // Step three:
            // If we have to make an explicit check, parse the JSON again with a
            // different, minimal Root class, where class members are serialized to
            // the type string. In case the string is null, there's no JSON node.
            // Otherwise the string would be empty ("").
            if (check)
            {
                Profiler.BeginSample("JSON secondary");
                var fakeRoot = JsonUtility.FromJson<FakeSchema.Root>(json);

                if (root.materials != null)
                {
                    for (var i = 0; i < root.materials.Length; i++)
                    {
                        var mat = root.materials[i];
                        if (mat.extensions == null) continue;
                        Assert.AreEqual(mat.name, fakeRoot.materials[i].name);
                        var fake = fakeRoot.materials[i].extensions;
                        if (fake.KHR_materials_unlit == null)
                        {
                            mat.extensions.KHR_materials_unlit = null;
                        }

                        if (fake.KHR_materials_pbrSpecularGlossiness == null)
                        {
                            mat.extensions.KHR_materials_pbrSpecularGlossiness = null;
                        }

                        if (fake.KHR_materials_transmission == null)
                        {
                            mat.extensions.KHR_materials_transmission = null;
                        }

                        if (fake.KHR_materials_clearcoat == null)
                        {
                            mat.extensions.KHR_materials_clearcoat = null;
                        }

                        if (fake.KHR_materials_sheen == null)
                        {
                            mat.extensions.KHR_materials_sheen = null;
                        }
                    }
                }

#if GLTFAST_SAFE
                if (root.accessors != null) {
                    for (var i = 0; i < root.accessors.Length; i++) {
                        var sparse = fakeRoot.accessors[i].sparse;
                        if (sparse?.indices == null || sparse.values == null) {
                            root.accessors[i].sparse = null;
                        }
                    }
                }
#endif

#if DRACO_UNITY
                if (root.meshes != null) {
                    for (var i = 0; i < root.meshes.Length; i++) {
                        var mesh = root.meshes[i];
                        Assert.AreEqual(mesh.name, fakeRoot.meshes[i].name);
                        for (var j = 0; j < mesh.primitives.Length; j++) {
                            var primitive = mesh.primitives[j];
                            if (primitive.extensions == null ) continue;
                            var fake = fakeRoot.meshes[i].primitives[j];
                            if (fake.extensions.KHR_draco_mesh_compression == null) {
                                // TODO: Differentiate Primitive extensions here
                                // since Draco is the only primitive extension, we
                                // remove the whole extensions property.
                                // primitive.extensions.KHR_draco_mesh_compression = null;
                                primitive.extensions = null;
                            }
                        }
                    }
                }
#endif
                Profiler.EndSample();
            }

            // Step four:
            // Further null checks on nodes' extensions
            if (root.nodes != null)
            {
                for (int i = 0; i < root.nodes.Length; i++)
                {
                    var e = root.nodes[i].extensions;
                    if (e != null)
                    {
                        // Check if GPU instancing extension is valid
                        if (e.EXT_mesh_gpu_instancing?.attributes == null)
                        {
                            e.EXT_mesh_gpu_instancing = null;
                        }
                        // Check if Lights extension is valid
                        if ((e.KHR_lights_punctual?.light ?? -1) < 0)
                        {
                            e.KHR_lights_punctual = null;
                        }
                        // Unset `extension` if none of them was valid
                        if (e.EXT_mesh_gpu_instancing == null &&
                            e.KHR_lights_punctual == null)
                        {
                            root.nodes[i].extensions = null;
                        }
                    }
                }
            }
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
