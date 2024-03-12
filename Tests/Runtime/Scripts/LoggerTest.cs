// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
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

        [Test]
        public static void TestLogItemEquals()
        {
            var a = new LogItem(LogType.Error, LogCode.None);
            var b = new LogItem(LogType.Error, LogCode.None);
            Assert.AreEqual(a, b);
            Assert.AreEqual(a.GetHashCode(), b.GetHashCode());

            a = new LogItem(LogType.Log, LogCode.None, "MyMessage");
            b = new LogItem(LogType.Log, LogCode.None, "MyMessage");
            Assert.AreEqual(a, b);
            Assert.AreEqual(a.GetHashCode(), b.GetHashCode());

            a = new LogItem(LogType.Log, LogCode.None, "FirstMessage", "SecondMessage");
            b = new LogItem(LogType.Log, LogCode.None, "FirstMessage", "SecondMessage");
            Assert.AreEqual(a, b);
            Assert.AreEqual(a.GetHashCode(), b.GetHashCode());

            a = new LogItem(LogType.Error, LogCode.None);
            b = new LogItem(LogType.Assert, LogCode.None);
            Assert.AreNotEqual(a, b);
            Assert.AreNotEqual(a.GetHashCode(), b.GetHashCode());

            a = new LogItem(LogType.Error, LogCode.None);
            b = new LogItem(LogType.Error, LogCode.EmbedSlow);
            Assert.AreNotEqual(a, b);
            Assert.AreNotEqual(a.GetHashCode(), b.GetHashCode());

            a = new LogItem(LogType.Log, LogCode.None, "MyMessage");
            b = new LogItem(LogType.Log, LogCode.None);
            Assert.AreNotEqual(a, b);
            Assert.AreNotEqual(a.GetHashCode(), b.GetHashCode());

            a = new LogItem(LogType.Log, LogCode.None, "MyMessage");
            b = new LogItem(LogType.Log, LogCode.None, "DifferentMessage");
            Assert.AreNotEqual(a, b);
            Assert.AreNotEqual(a.GetHashCode(), b.GetHashCode());

            a = new LogItem(LogType.Log, LogCode.None, "FirstMessage", "SecondMessage");
            b = new LogItem(LogType.Log, LogCode.None, "FirstMessage", "DifferentSecondMessage");
            Assert.AreNotEqual(a, b);
            Assert.AreNotEqual(a.GetHashCode(), b.GetHashCode());

            a = new LogItem(LogType.Log, LogCode.None, "FirstMessage", "SecondMessage");
            b = new LogItem(LogType.Log, LogCode.None, "FirstMessage");
            Assert.AreNotEqual(a, b);
            Assert.AreNotEqual(a.GetHashCode(), b.GetHashCode());
        }

        internal static void AssertLogger(CollectingLogger logger, IEnumerable<LogCode> expectedLogCodes)
        {
            AssertLogCodes(logger.Items, expectedLogCodes);
        }

        internal static void AssertLogCodes(IEnumerable<LogItem> logItems, IEnumerable<LogCode> expectedLogCodes)
        {
            var expectedLogCodeFound = new Dictionary<LogCode, bool>();
            foreach (var logCode in expectedLogCodes)
            {
                expectedLogCodeFound[logCode] = false;
            }

            foreach (var item in logItems)
            {
                switch (item.Type)
                {
                    case LogType.Assert:
                    case LogType.Error:
                    case LogType.Exception:
                        if (expectedLogCodeFound.Keys.Contains(item.Code))
                        {
                            expectedLogCodeFound[item.Code] = true;
                            // Informal log
                            Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, null, item.ToString());
                        }
                        else
                        {
                            item.Log();
                            throw new AssertionException($"Unhandled {item.Type} message {item} ({item.Code}).");
                        }
                        break;
                    case LogType.Warning:
                    case LogType.Log:
                    default:
                        item.Log();
                        break;
                }
            }

            foreach (var b in expectedLogCodeFound.Where(b => !b.Value))
            {
                throw new AssertionException($"Missing expected log message {b.Key}.");
            }
        }

        internal static void AssertLogger(CollectingLogger logger, IEnumerable<LogItem> expectedLogItems)
        {
            AssertLogItems(logger.Items, expectedLogItems);
        }

        static void AssertLogItems(IEnumerable<LogItem> logItems, IEnumerable<LogItem> expectedLogItems)
        {
            var items = expectedLogItems.ToList();

            foreach (var item in logItems)
            {
                var index = items.IndexOf(item);
                if (index >= 0)
                {
                    items.RemoveAt(index);
                    // Informal log
                    Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, null, item.ToString());
                    continue;
                }

                item.Log();
            }

            foreach (var b in items)
            {
                throw new AssertionException($"Missing expected log message \"{b}\".");
            }
        }
    }
}
