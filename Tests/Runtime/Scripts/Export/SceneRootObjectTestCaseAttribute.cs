// SPDX-FileCopyrightText: 2024 Unity Technologies and the Draco for Unity authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Builders;
using UnityEngine;

namespace GLTFast.Tests.Export
{

    public class SceneRootObjectTestCaseAttribute : UnityEngine.TestTools.UnityTestAttribute, ITestBuilder
    {

        string m_Suffix;
        string[] m_ObjectNames;

        NUnitTestCaseBuilder m_Builder = new NUnitTestCaseBuilder();

        public SceneRootObjectTestCaseAttribute(string sceneName, string suffix = null)
        {
            m_Suffix = suffix;

#if UNITY_EDITOR
            try
#endif
            {
                m_ObjectNames = ExportTests.GetRootObjectNamesFromStreamingAssets(sceneName);
            }
#if UNITY_EDITOR
            catch (FileNotFoundException)
            {
                m_ObjectNames = ExportTests.GetRootObjectNamesFromObjectList(sceneName);
            }
#endif
        }

        IEnumerable<TestMethod> ITestBuilder.BuildFrom(IMethodInfo method, Test suite)
        {
            var results = new List<TestMethod>();

            try
            {
                for (var i = 0; i < m_ObjectNames.Length; i++)
                {
                    var objectName = m_ObjectNames[i];
                    if (string.IsNullOrEmpty(objectName))
                    {
                        continue;
                    }
                    var data = new TestCaseData(new object[] { i, objectName });

                    var testName = string.IsNullOrEmpty(m_Suffix) ? objectName : $"{objectName}-{m_Suffix}";
                    data.SetName(testName);
                    data.ExpectedResult = new UnityEngine.Object();
                    data.HasExpectedResult = true;

                    var test = m_Builder.BuildTestMethod(method, suite, data);
                    if (test.parms != null)
                        test.parms.HasExpectedResult = false;

                    test.Name = testName;

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
