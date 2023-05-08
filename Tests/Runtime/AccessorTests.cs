// Copyright 2020-2023 Andreas Atteneder
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
using UnityEngine;

using GLTFast.Schema;

namespace GLTFast.Tests
{
    public class AccessorTests
    {
        [Test]
        public void GetAccessorAttributeType()
        {
            Assert.AreEqual(GltfAccessorAttributeType.SCALAR, Accessor.GetAccessorAttributeType(1));
            Assert.AreEqual(GltfAccessorAttributeType.VEC2, Accessor.GetAccessorAttributeType(2));
            Assert.AreEqual(GltfAccessorAttributeType.VEC3, Accessor.GetAccessorAttributeType(3));
            Assert.AreEqual(GltfAccessorAttributeType.VEC4, Accessor.GetAccessorAttributeType(4));
            
            Assert.That(() => Accessor.GetAccessorAttributeType(0), 
                Throws.TypeOf<ArgumentOutOfRangeException>());
            
            Assert.That(() => Accessor.GetAccessorAttributeType(5), 
                Throws.TypeOf<ArgumentOutOfRangeException>());
        }
        
        [Test]
        public void GetAccessorAttributeTypeLength()
        {
            Assert.AreEqual(1, Accessor.GetAccessorAttributeTypeLength(GltfAccessorAttributeType.SCALAR));
            Assert.AreEqual(2, Accessor.GetAccessorAttributeTypeLength(GltfAccessorAttributeType.VEC2));
            Assert.AreEqual(3, Accessor.GetAccessorAttributeTypeLength(GltfAccessorAttributeType.VEC3));
            Assert.AreEqual(4, Accessor.GetAccessorAttributeTypeLength(GltfAccessorAttributeType.VEC4));
            Assert.AreEqual(4, Accessor.GetAccessorAttributeTypeLength(GltfAccessorAttributeType.MAT2));
            Assert.AreEqual(9, Accessor.GetAccessorAttributeTypeLength(GltfAccessorAttributeType.MAT3));
            Assert.AreEqual(16, Accessor.GetAccessorAttributeTypeLength(GltfAccessorAttributeType.MAT4));
            
            Assert.That(() => Accessor.GetAccessorAttributeTypeLength(GltfAccessorAttributeType.Undefined), 
                Throws.TypeOf<ArgumentOutOfRangeException>());
        }
    }
}
