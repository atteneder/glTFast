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

using UnityEngine;

namespace GLTFast.Utils {

    public class StopWatch : MonoBehaviour {
        
        [Tooltip("Maximum time a frame can take. Frame times will not exceed this value even if the actual frame time was longer.")]
        [SerializeField]
        float maxDeltaTime = 10;
        
        float m_StartTime = -1;
        float m_MinFrameTime = float.MaxValue;
        float m_MaxFrameTime = float.MinValue;
        float m_Duration = -1;
        float m_FPSDuration;
        int m_StartFrameCount;
        int m_EndFrameCount;
        bool m_Running;
        bool m_ConsiderLastFrame;

        public float now => (Time.realtimeSinceStartup-m_StartTime)*1000;

        public bool active => m_Running || m_Duration>=0;
        public float lastDuration => m_Duration;
        public int frameCount => m_EndFrameCount-m_StartFrameCount;
        public float averageFrameTime => m_FPSDuration / frameCount;
        public float minFrameTime => m_MinFrameTime;
        public float maxFrameTime => m_MaxFrameTime;

        void Awake() {
            Time.maximumDeltaTime = maxDeltaTime;
        }

        public void StartTime() {
            m_StartTime = Time.realtimeSinceStartup;
            m_Duration = 0;
            m_FPSDuration = 0;
            m_MinFrameTime = float.MaxValue;
            m_MaxFrameTime = float.MinValue;
            m_StartFrameCount = Time.frameCount;
            m_EndFrameCount = m_StartFrameCount;
            m_Running = true;
        }

        public void StopTime() {
            m_Running = false;
            m_Duration = now;
            m_ConsiderLastFrame = true;
        }

        public string GetTextReport() {
            return string.Format(
                "{3} ms (fps avg: {0} min: {1} ms max: {2} ms)"
                , averageFrameTime.ToString("0.00")
                , minFrameTime < float.MaxValue ? minFrameTime.ToString("0.00") : "-"
                , maxFrameTime > float.MinValue ? maxFrameTime.ToString("0.00") : "-"
                , lastDuration >= 0 ? lastDuration.ToString("0.00") : "-"
            );
        }

        void Update() {
            if(m_Running || m_ConsiderLastFrame) {
                m_Duration = now;
                if (m_StartFrameCount < Time.frameCount) {
                    var delta = Time.deltaTime * 1000;
                    m_FPSDuration += delta;
                    UpdateFrameTimes(delta);
                }
                m_ConsiderLastFrame = false;
            }
        }

        void UpdateFrameTimes(float delta) {
            var currentFrame = Time.frameCount;
            // Skip handling frame twice (happened when StopTime was called)
            if (m_EndFrameCount >= currentFrame) return;
            m_EndFrameCount = currentFrame;
            if(m_EndFrameCount > m_StartFrameCount) {
                m_MinFrameTime = Mathf.Min(m_MinFrameTime, delta );
                m_MaxFrameTime = Mathf.Max(m_MaxFrameTime, delta );
            }
        }
    }
}
