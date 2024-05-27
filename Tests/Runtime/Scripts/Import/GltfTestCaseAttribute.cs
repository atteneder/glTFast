// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Builders;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace GLTFast.Tests.Import
{
    class GltfTestCaseAttribute : UnityEngine.TestTools.UnityTestAttribute, ITestBuilder
    {
        readonly GltfTestCaseSet m_TestCases;
        readonly GltfTestCaseFilter m_Filter;

        readonly NUnitTestCaseBuilder m_Builder = new NUnitTestCaseBuilder();

        public GltfTestCaseAttribute(string testSetName, int testCaseCount, string includeFilter = null)
        {
#if UNITY_EDITOR
            var path = $"Packages/{GltfGlobals.GltfPackageName}/Tests/Runtime/TestCaseSets/{testSetName}.asset";
            m_TestCases = AssetDatabase.LoadAssetAtPath<GltfTestCaseSet>(path);
            if (m_TestCases == null)
            {
                path = $"Packages/{GltfGlobals.GltfPackageName}.tests/Tests/Runtime/TestCaseSets/{testSetName}.asset";
                m_TestCases = AssetDatabase.LoadAssetAtPath<GltfTestCaseSet>(path);
            }
#else
            var path = $"{testSetName}.json";
            m_TestCases = GltfTestCaseSet.DeserializeFromStreamingAssets(path);
#endif
            if (m_TestCases == null)
            {
                throw new InvalidDataException($"Test case collection not found at {path}");
            }

            m_Filter = includeFilter == null ? null : new GltfTestCaseFilter(new Regex(includeFilter));
            var actualTestCaseCount = m_TestCases.GetTestCaseCount(m_Filter);
            if (testCaseCount != actualTestCaseCount)
            {
                throw new InvalidDataException(
                    $"Incorrect number of test cases in {testSetName}. " +
                    $"Expected {testCaseCount}, but found {actualTestCaseCount} " +
                    (includeFilter == null ? "" : $"(includeFilter: \"{includeFilter}\")")
                    );
            }
        }

        IEnumerable<TestMethod> ITestBuilder.BuildFrom(IMethodInfo method, Test suite)
        {
            var results = new List<TestMethod>();
            var nameCounts = new Dictionary<string, int>();

            try
            {
                foreach (var testCase in m_TestCases.IterateTestCases(m_Filter))
                {
                    var data = new TestCaseData(new object[] { m_TestCases, testCase });

                    var origName = testCase.Filename;
                    string name;
                    if (nameCounts.TryGetValue(origName, out var count))
                    {
                        name = $"{origName}-{count}";
                        nameCounts[origName] = count + 1;
                    }
                    else
                    {
                        name = $"{origName}";
                        nameCounts[origName] = 1;
                    }

                    data.SetName($"{method.Name}.{name}");
                    data.ExpectedResult = new UnityEngine.Object();
                    data.HasExpectedResult = true;

                    var test = m_Builder.BuildTestMethod(method, suite, data);
                    test.Name = name;

                    results.Add(test);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to generate glTF testcases!");
                Debug.LogException(ex);
                throw;
            }

            Console.WriteLine("Generated {0} glTF test cases.", results.Count);
            return results;
        }
    }
}
