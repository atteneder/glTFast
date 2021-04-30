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

using System;
using NUnit.Framework;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.TestTools;

namespace GLTFast.Tests
{
    public class ReportTest
    {
        [Test]
        public static void LogTest() {
            var r = new Report();
            
            r.Error(ReportCode.Download,"404", "https://something.com/nowherfound.glb");
            
            r.LogAll();
            
            LogAssert.Expect(LogType.Error, "Download URL https://something.com/nowherfound.glb failed: 404");
        }
    }
}