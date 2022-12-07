// Copyright 2020-2022 Andreas Atteneder
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

using GLTFast.Schema;
using NUnit.Framework;
using UnityEngine;

namespace GLTFast.Tests
{
    class MeshPrimitiveTest
    {

        [Test]
        public void MeshPrimitiveEqualTest()
        {

            var a = new MeshPrimitive { attributes = new Attributes { POSITION = 42 } };
            var b = new MeshPrimitive { attributes = new Attributes { POSITION = 42 } };

            Assert.IsTrue(a.Equals(b));

            a.targets = new[] { new MorphTarget { POSITION = 0 } };
            b.targets = null;
            Assert.IsFalse(a.Equals(b));

            a.targets = new[] { new MorphTarget { POSITION = 0 } };
            b.targets = new[] { new MorphTarget { POSITION = 0 } };
            Assert.IsTrue(a.Equals(b));

            a.targets = new[] { new MorphTarget { POSITION = 0 } };
            b.targets = new[] { new MorphTarget { POSITION = 0, NORMAL = 1 } };
            Assert.IsFalse(a.Equals(b));

            a.targets = null;
            b.targets = new[] { new MorphTarget { POSITION = 0 } };
            Assert.IsFalse(a.Equals(b));

            a.targets = new[] { new MorphTarget { POSITION = 0 } };
            b.targets = new[] { new MorphTarget { POSITION = 0 }, new MorphTarget { POSITION = 0 } };
            Assert.IsFalse(a.Equals(b));

            a = new MeshPrimitive { attributes = new Attributes { POSITION = 41 } };
            b = new MeshPrimitive { attributes = new Attributes { POSITION = 42 } };
            Assert.IsFalse(a.Equals(b));

            a.targets = new[] { new MorphTarget { POSITION = 0 } };
            b.targets = new[] { new MorphTarget { POSITION = 0 } };
            Assert.IsFalse(a.Equals(b));
        }
    }
}
