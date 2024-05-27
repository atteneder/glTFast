// SPDX-FileCopyrightText: 2024 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Text.RegularExpressions;

namespace GLTFast.Tests.Import
{
    class GltfTestCaseFilter
    {
        Regex m_IncludeFilter;

        public GltfTestCaseFilter(Regex includeFilter)
        {
            m_IncludeFilter = includeFilter;
        }

        public bool Matches(GltfTestCase testCase)
        {
            return m_IncludeFilter.IsMatch(testCase.relativeUri);
        }
    }
}
