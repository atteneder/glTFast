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

using System;
using System.Threading.Tasks;
using UnityEngine;

namespace GLTFast
{

    /// <summary>
    /// Claims a certain fraction of the target frame time and keeps track of
    /// whether this time frame was surpassed.
    /// </summary>
    [DefaultExecutionOrder(-10)]
    public class TimeBudgetPerFrameDeferAgent : MonoBehaviour, IDeferAgent
    {

        [SerializeField]
        [Range(.01f, 5f)]
        [Tooltip("Per-frame time budget as fraction of the targeted frame time. Keep it well below 0.5, so there's enough time for other game logic and rendering. A value of 1.0 can lead to dropping a full frame. Even higher values can stall for multiple frames.")]
        float frameBudget = .5f;

        float m_LastTime;
        float m_TimeBudget = .5f / 30;

        /// <summary>
        /// Defers work to the next frame if a fix time budget is
        /// used up.
        /// </summary>
        /// <param name="newFrameBudget">Per-frame time budget as fraction of the targeted frame time</param>
        public void SetFrameBudget(float newFrameBudget = 0.5f)
        {
            frameBudget = newFrameBudget;
            UpdateTimeBudget();
        }

        void UpdateTimeBudget()
        {
            float targetFrameRate = Application.targetFrameRate;
            if (targetFrameRate < 0) targetFrameRate = 30;
            m_TimeBudget = frameBudget / targetFrameRate;
            ResetLastTime();
        }

        void Awake()
        {
            UpdateTimeBudget();
        }

        void Update()
        {
            ResetLastTime();
        }

        void ResetLastTime()
        {
            m_LastTime = Time.realtimeSinceStartup;
        }

        /// <inheritdoc />
        public bool ShouldDefer()
        {
            return !FitsInCurrentFrame(0);
        }

        /// <inheritdoc />
        public bool ShouldDefer(float duration)
        {
            return !FitsInCurrentFrame(duration);
        }

        bool FitsInCurrentFrame(float duration)
        {
            return duration <= m_TimeBudget - (Time.realtimeSinceStartup - m_LastTime);
        }

        /// <inheritdoc />
        public async Task BreakPoint()
        {
            if (ShouldDefer())
            {
                await Task.Yield();
            }
        }

        /// <inheritdoc />
        public async Task BreakPoint(float duration)
        {
            if (ShouldDefer(duration))
            {
                await Task.Yield();
            }
        }
    }
}
