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

using UnityEditor;
using UnityEngine;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;

namespace GLTFast
{
    static class OnScriptsReloadHandler
    {

        // Only run this check if glTFast is in Packages/manifest.json testables
        // (which indicates you're developing it)
#if UNITY_INCLUDE_TESTS

        static ListRequest s_Request;

        [UnityEditor.Callbacks.DidReloadScripts]
        static void OnScriptsReloaded() {
            s_Request = Client.List();
            EditorApplication.update += Progress;
        }

        static void Progress()
        {
            if (s_Request.IsCompleted)
            {
                if (s_Request.Status == StatusCode.Success) {
                    foreach (var package in s_Request.Result) {
                        if (package.name == "com.atteneder.gltfast") {
                            var version = package.version;
                            if (Export.Constants.version != version) {
                                Debug.LogWarning($"Version mismatch in Constants.cs (is {Export.Constants.version}, should be {version}). Please update!");
                            }
                        }
                    }
                }
                else if (s_Request.Status >= StatusCode.Failure) {
                    Debug.Log(s_Request.Error.message);
                }

                EditorApplication.update -= Progress;
            }
        }
#endif
    }
}
