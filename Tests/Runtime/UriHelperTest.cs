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
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using GLTFast;
using UnityEngine.Profiling;

public class UriHelperTest
{
    static Uri[] glb = new []{
        new Uri("file.glb",UriKind.RelativeOrAbsolute),
        new Uri("file:///dir/sub/file.glb",UriKind.RelativeOrAbsolute),
        new Uri("http://www.server.com/dir/sub/file.glb",UriKind.RelativeOrAbsolute),
        new Uri("http://www.server.com/dir/sub/file.glb",UriKind.RelativeOrAbsolute),
        new Uri("http://www.server.com/dir/sub/file.glb?a=123&b=234",UriKind.RelativeOrAbsolute),
    };
    static Uri[] gltf = new []{
        new Uri("file.gltf",UriKind.RelativeOrAbsolute),
        new Uri("file:///dir/sub/file.gltf",UriKind.RelativeOrAbsolute),
        new Uri("http://www.server.com/dir/sub/file.gltf",UriKind.RelativeOrAbsolute),
        new Uri("http://www.server.com/dir/sub/file.gltf",UriKind.RelativeOrAbsolute),
        new Uri("http://www.server.com/dir/sub/file.gltf?a=123&b=234",UriKind.RelativeOrAbsolute),
    };
    static Uri[] unknown = new []{
        new Uri("f",UriKind.RelativeOrAbsolute),
        new Uri("file:///dir/sub/f",UriKind.RelativeOrAbsolute),
        new Uri("http://www.server.com/dir/sub/f",UriKind.RelativeOrAbsolute),
        new Uri("http://www.server.com/dir/sub/f",UriKind.RelativeOrAbsolute),
        new Uri("http://www.server.com/dir/sub/f?a=123&b=234)",UriKind.RelativeOrAbsolute),
    };

    [Test]
    public void GetBaseUriTest()
    {
        // HTTP(s) gltf
        Assert.AreEqual(new Uri("http://www.server.com/dir/sub/"),UriHelper.GetBaseUri("http://www.server.com/dir/sub/file.gltf"));
        Assert.AreEqual(new Uri("https://www.server.com/dir/sub/"),UriHelper.GetBaseUri("https://www.server.com/dir/sub/file.gltf"));
        Assert.AreEqual(new Uri("http://www.server.com/dir/sub/"),UriHelper.GetBaseUri("http://www.server.com/dir/sub/file.gltf?a=123&b=456"));
        Assert.AreEqual(new Uri("https://www.server.com/dir/sub/"),UriHelper.GetBaseUri("https://www.server.com/dir/sub/file.gltf?a=123&b=456"));
        // HTTP(s) glb
        Assert.AreEqual(new Uri("http://www.server.com/dir/sub/"),UriHelper.GetBaseUri("http://www.server.com/dir/sub/file.glb"));
        Assert.AreEqual(new Uri("https://www.server.com/dir/sub/"),UriHelper.GetBaseUri("https://www.server.com/dir/sub/file.glb"));
        Assert.AreEqual(new Uri("http://www.server.com/dir/sub/"),UriHelper.GetBaseUri("http://www.server.com/dir/sub/file.glb?a=123&b=456"));
        Assert.AreEqual(new Uri("https://www.server.com/dir/sub/"),UriHelper.GetBaseUri("https://www.server.com/dir/sub/file.glb?a=123&b=456"));
        // HTTP(s) none
        Assert.AreEqual(new Uri("http://www.server.com/dir/sub/"),UriHelper.GetBaseUri("http://www.server.com/dir/sub/file"));
        Assert.AreEqual(new Uri("https://www.server.com/dir/sub/"),UriHelper.GetBaseUri("https://www.server.com/dir/sub/file"));
        Assert.AreEqual(new Uri("http://www.server.com/dir/sub/"),UriHelper.GetBaseUri("http://www.server.com/dir/sub/file?a=123&b=456"));
        Assert.AreEqual(new Uri("https://www.server.com/dir/sub/"),UriHelper.GetBaseUri("https://www.server.com/dir/sub/file?a=123&b=456"));

        // file paths
        Assert.AreEqual(new Uri("file:///dir/sub/"),UriHelper.GetBaseUri("file:///dir/sub/file.gltf"));
        Assert.AreEqual(new Uri("file:///dir/sub/"),UriHelper.GetBaseUri("/dir/sub/file.gltf"));

        // special char `+`
        Assert.AreEqual(new Uri("https://www.server.com/dir/sub/"),UriHelper.GetBaseUri("https://www.server.com/dir/sub/file+test.gltf"));
        Assert.AreEqual(new Uri("file:///dir/sub/"),UriHelper.GetBaseUri("file:///dir/sub/file+test.gltf"));
    }

    [Test]
    public void GetUriStringTest()
    {
        var baseUri = new Uri("http://www.server.com/dir/sub/");

        Assert.AreEqual( new Uri("file+test.gltf",UriKind.RelativeOrAbsolute), UriHelper.GetUriString("file+test.gltf",null));
        Assert.AreEqual( new Uri("http://www.server.com/dir/sub/file+test.gltf",UriKind.RelativeOrAbsolute), UriHelper.GetUriString("file+test.gltf",baseUri));
        Assert.AreEqual( new Uri("http://www.server.com/dir/sub/sub2/sub3/file+test.gltf",UriKind.RelativeOrAbsolute), UriHelper.GetUriString("sub2/sub3/file+test.gltf",baseUri));
    }

    [Test]
    public void IsGltfBinaryTest()
    {
        Profiler.BeginSample("IsGltfBinaryProfile");
        for (int i = 0; i < 1000; i++)
        {
            for (int j = 0; j < glb.Length; j++) {
                Profiler.BeginSample("IsGltfBinaryUriTrue");
                Assert.IsTrue(UriHelper.IsGltfBinary(glb[j]));
                Profiler.EndSample();
            }
            for (int j = 0; j < gltf.Length; j++) {
                Profiler.BeginSample("IsGltfBinaryUriFalse");
                Assert.IsFalse(UriHelper.IsGltfBinary(gltf[j]));
                Profiler.EndSample();
            }
            for (int j = 0; j < unknown.Length; j++) {
                Profiler.BeginSample("IsGltfBinaryUriUnknown");
                Assert.IsNull(UriHelper.IsGltfBinary(unknown[j]));
                Profiler.EndSample();
            }
        }
        Profiler.EndSample();
    }
}
