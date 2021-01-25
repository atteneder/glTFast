// Copyright 2020-2021 Andreas Atteneder
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

using Unity.Mathematics;
using UnityEngine;

namespace GLTFast.Tests {

    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    public class FrameBoundsCamera : MonoBehaviour {

        public BoxCollider boxCollider;

        private Camera _camera;

        // Start is called before the first frame update
        void Start() {
            _camera = GetComponent<Camera>();
            if (_camera == null) {
                Destroy(this);
            }
        }

        // Update is called once per frame
        void Update() {
            if (boxCollider == null) return;
            Bounds bounds = new Bounds(boxCollider.center, boxCollider.size);
            FrameBounds(_camera, boxCollider.transform, bounds);
        }

        public static void FrameBounds(Camera camera, Transform boundsTransform, Bounds bounds) {
            float3 scale = boundsTransform.localScale;
            float3 boundsSize = bounds.size;
            var distance = math.length(scale * boundsSize);
            var angle = math.radians(20);

            var centerPosition = boundsTransform.TransformPoint(bounds.center);
            var cameraPos = centerPosition +
                new Vector3(0, distance * math.sin(angle), -distance * math.cos(angle));
            camera.transform.position = cameraPos;
            camera.transform.LookAt(centerPosition);

            camera.nearClipPlane = distance * .001f;
            camera.farClipPlane = distance * 3;
        }
    }
}
