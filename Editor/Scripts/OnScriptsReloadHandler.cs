// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

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
                        if (package.name == GltfGlobals.GltfPackageName) {
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
