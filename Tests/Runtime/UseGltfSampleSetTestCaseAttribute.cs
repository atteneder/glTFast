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

using System;
using System.Collections.Generic;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Builders;
using UnityEditor;
using UnityEngine;

public class UseGltfSampleSetTestCaseAttribute : UnityEngine.TestTools.UnityTestAttribute, ITestBuilder
{
    GltfSampleSet m_sampleSet = null;

    NUnitTestCaseBuilder _builder = new NUnitTestCaseBuilder();

    public UseGltfSampleSetTestCaseAttribute(string sampleSetPath) {
        m_sampleSet = AssetDatabase.LoadAssetAtPath<GltfSampleSet>(sampleSetPath);
    }

    IEnumerable<TestMethod> ITestBuilder.BuildFrom(IMethodInfo method, Test suite) {
        List<TestMethod> results = new List<TestMethod>();
        var nameCounts = new Dictionary<string, int>();
        
        try {
            foreach (var gltfSample in m_sampleSet.GetItems()) {
                var data = new TestCaseData( new object[]{ gltfSample } );

                string name;
                if (nameCounts.TryGetValue(gltfSample.name, out int count)) {
                    name = string.Format("{0}-{1}", gltfSample.name, count);
                    nameCounts[gltfSample.name] = count + 1;
                } else {
                    name = gltfSample.name;
                    nameCounts[gltfSample.name] = 1;
                }

                data.SetName(name);
                data.ExpectedResult = new UnityEngine.Object();
                data.HasExpectedResult = true;

                var test = this._builder.BuildTestMethod(method, suite, data);
                if (test.parms != null)
                    test.parms.HasExpectedResult = false;

                test.Name = name;

                results.Add(test);
            }
        }
        catch (Exception ex) {
            Console.WriteLine("Failed to generate glTF testcases!");
            Debug.LogException(ex);
            throw;
        }

        Console.WriteLine("Generated {0} glTF test cases.", results.Count);
        return results;
    }
}
