using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GLTFast {
    
    public class TimeBudgetPerFrameDeferAgent : MonoBehaviour, IDeferAgent
    {
        float lastTime;
        float timeBudget;

        /// <summary>
        /// Defers work to the next frame if a fix time budget is
        /// used up.
        /// </summary>
        /// <param name="frameBudget">Time budget as part of the target frame rate.</param>
        public TimeBudgetPerFrameDeferAgent( float frameBudget = 0.5f )
        {
            float targetFrameRate = Application.targetFrameRate;
            if(targetFrameRate<0) targetFrameRate = 30;
            timeBudget = frameBudget/targetFrameRate;
            Reset();
        }

        void Update() {
            Reset();
        }

        void Reset()
        {
            lastTime = Time.realtimeSinceStartup;
        }

        public bool ShouldDefer() {
            float now = Time.realtimeSinceStartup;
            if( now-lastTime > timeBudget ) {
                lastTime = now;
                return true;
            }
            return false;
        }
    }
}