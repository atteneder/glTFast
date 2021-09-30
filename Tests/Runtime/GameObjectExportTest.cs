// Copyright 2020-2021 Andreas Atteneder
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

using System.IO;
using GLTFast.Export;
using NUnit.Framework;
using UnityEngine;

namespace GLTFast.Tests {
    public class GameObjectExportTest {

        // [UnityTest]
        // public IEnumerator SimpleTree() {
        [Test]
        public void SimpleTree() {

            var root = new GameObject("root");
            var childA = GameObject.CreatePrimitive(PrimitiveType.Cube);
            childA.name = "child A";
            var childB = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            childB.name = "child B";
            childA.transform.parent = root.transform;
            childB.transform.parent = root.transform;
            childB.transform.localPosition = new Vector3(1, 0, 0);

            var export = new GameObjectExport();
            export.AddScene("UnityScene" ,new []{root});
            export.SaveToFile(Path.Combine(Application.persistentDataPath,"root.gltf"));
        }
    }
}
