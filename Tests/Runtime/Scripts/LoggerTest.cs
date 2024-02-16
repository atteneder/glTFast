// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace GLTFast.Tests
{
    using Logging;

    static class LoggerTest
    {
        [Test]
        public static void CollectingLoggerTest()
        {
            var r = new CollectingLogger();
            r.Error(LogCode.Download, "404", "https://something.com/nowherfound.glb");

            Assert.AreEqual(1, r.Count);
            var items = r.Items.ToArray();
            Assert.AreEqual("Download URL https://something.com/nowherfound.glb failed: 404", items[0].ToString());
        }

        [Test]
        public static void ConsoleLoggerTest()
        {
            var r = new ConsoleLogger();
            r.Error(LogCode.Download, "404", "https://something.com/nowherfound.glb");
            LogAssert.Expect(LogType.Error, "Download URL https://something.com/nowherfound.glb failed: 404");
        }
    }
}
