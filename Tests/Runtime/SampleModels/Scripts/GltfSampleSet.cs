// Copyright 2020 Andreas Atteneder
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

#if !(UNITY_ANDROID || UNITY_WEBGL) || UNITY_EDITOR
#define LOCAL_LOADING
#endif

using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace GLTFast.Tests {

    [CreateAssetMenu(fileName = "glTF-SampleSet", menuName = "ScriptableObjects/GltfSampleSet", order = 1)]
    public class GltfSampleSet : ScriptableObject {

        public string baseLocalPath = "";
        public string streamingAssetsPath = "glTF-Sample-Models/2.0";
        public string baseUrlWeb = "https://raw.githubusercontent.com/KhronosGroup/glTF-Sample-Models/master/2.0/";
        public string baseUrlLocal = "http://localhost:8080/glTF-Sample-Models/2.0/";

        [SerializeField]
        private GltfSampleSetItemEntry[] items;

        public int itemCount => items.Length;

        public string localFilePath {
            get {
                string path;
                if (string.IsNullOrEmpty(streamingAssetsPath)) {
                    path = baseLocalPath;
                }
                else {
                    path = Path.Combine(Application.streamingAssetsPath, streamingAssetsPath);
                }

                return path;
            }
        }

        private string remoteUri {
            get {
                string uriPrefix = string.IsNullOrEmpty(baseUrlWeb) ? "<baseUrlWeb not set!>" : baseUrlWeb;
#if UNITY_EDITOR
                if (!string.IsNullOrEmpty(baseUrlLocal)) {
                    uriPrefix = baseUrlLocal;
                }
#endif
                return uriPrefix;
            }
        }

        public IEnumerable<GltfSampleSetItem> GetItems() {
            foreach (var x in items) {
                if (!x.active) continue;
                yield return new GltfSampleSetItem(
                    string.Format("{0}-{1:X}", x.item.name, x.item.path.GetHashCode()),
                    x.item.path
                );
            }
        }

        public IEnumerable<GltfSampleSetItem> GetItemsPrefixed(bool local = true) {
            var prefix = local ? localFilePath : remoteUri;
            foreach (var entry in items.Where(x => x.active)) {
                if (!string.IsNullOrEmpty(prefix)) {
                    var p = string.Format(
                        "{0}/{1}"
                        , prefix
                        , entry.item.path
                    );
                    yield return new GltfSampleSetItem(entry.item.name, p);
                }
            }
        }

        public IEnumerable<GltfSampleSetItem> GetTestItems(bool local = true) {
            var prefix = local ? localFilePath : remoteUri;
            foreach (var entry in items.Where(x => x.active)) {
                if (!string.IsNullOrEmpty(prefix)) {
                    var p = string.Format(
                        "{0}/{1}"
                        , prefix
                        , entry.item.path
                    );
                    yield return new GltfSampleSetItem(entry.item.name, p);
                }
            }
        }

        public void LoadItemsFromPath(string searchPattern) {
            var basePath = string.IsNullOrEmpty(streamingAssetsPath)
                ? baseLocalPath
                : Path.Combine(Application.streamingAssetsPath, streamingAssetsPath);

            var dir = new DirectoryInfo(basePath);
            var dirLength = dir.FullName.Length + 1;

            var newItems = new List<GltfSampleSetItemEntry>();

            foreach (var file in dir.GetFiles(searchPattern, SearchOption.AllDirectories)) {
                var ext = file.Extension;
                if (ext != ".gltf" && ext != ".glb") continue;
                var i = new GltfSampleSetItemEntry();
                i.active = true;
                i.item.name = file.Name;
                i.item.path = file.FullName.Substring(dirLength);
                newItems.Add(i);
            }

            items = newItems.ToArray();
        }

        public void SetAllActive(bool active = true) {
            for (int i = 0; i < items.Length; i++) {
                items[i].active = active;
            }
        }
    }
}
