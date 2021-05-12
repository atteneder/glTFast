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

using System;
using System.Threading.Tasks;
using UnityEngine;

namespace GLTFast {
    
    [DefaultExecutionOrder(-10)]
    public class TimeBudgetPerFrameDeferAgent : MonoBehaviour, IDeferAgent {
        
        float lastTime;
        float timeBudget = .5f/30;

        /// <summary>
        /// Defers work to the next frame if a fix time budget is
        /// used up.
        /// </summary>
        /// <param name="frameBudget">Time budget as part of the target frame rate.</param>
        public void SetFrameBudget( float frameBudget = 0.5f )
        {
            float targetFrameRate = Application.targetFrameRate;
            if(targetFrameRate<0) targetFrameRate = 30;
            timeBudget = frameBudget/targetFrameRate;
            ResetLastTime();
        }

        void Awake() {
            SetFrameBudget();
        }

        void Update() {
            ResetLastTime();
        }

        void ResetLastTime()
        {
            lastTime = Time.realtimeSinceStartup;
        }

        public bool ShouldDefer() {
            return !FitsInCurrentFrame(0);
        }
        
        public bool ShouldDefer( float duration ) {
            return !FitsInCurrentFrame(duration);
        }
        
        bool FitsInCurrentFrame(float duration) {
            return duration <= timeBudget - (Time.realtimeSinceStartup - lastTime);
        }

        public async Task BreakPoint() {
            if (ShouldDefer()) {
                await Task.Yield();
            }
        }
        
        public async Task BreakPoint( float duration ) {
            if (ShouldDefer(duration)) {
                await Task.Yield();
            }
        }
    }
}