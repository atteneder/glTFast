// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using GLTFast.Logging;
using UnityEngine;

namespace GLTFast.Tests.Import
{
    [Serializable]
    class GltfTestCase
    {
        public string relativeUri;

        public bool expectLoadFail;
        public bool expectInstantiationFail;

        public LogCode[] expectedLogCodes;

        public string Filename
        {
            get
            {
                var lastSeparatorIndex = relativeUri.LastIndexOf('/');
                return lastSeparatorIndex >= 0
                    ? relativeUri.Substring(lastSeparatorIndex + 1)
                    : relativeUri;
            }
        }

        public override string ToString()
        {
            return relativeUri;
        }
    }
}
