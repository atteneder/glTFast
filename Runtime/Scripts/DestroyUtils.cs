// SPDX-FileCopyrightText: 2026 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using UnityEngine;

namespace Unity.Cloud.Gltfast
{
    static class DestroyUtils
    {
        public static void SafeDestroy(Object obj)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Object.DestroyImmediate(obj);
            }
            else
#endif
            {
                Object.Destroy(obj);
            }
        }

        public static void SafeDestroy(IEnumerable<Object> objects)
        {
            if (objects != null)
            {
                foreach (var obj in objects)
                {
                    SafeDestroy(obj);
                }
            }
        }
    }
}
