// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using UnityEngine;

namespace GLTFast
{
    /// <summary>
    /// Descriptor of a glTF scene instance
    /// </summary>
    public class GameObjectSceneInstance
    {

        /// <summary>
        /// List of instantiated cameras
        /// </summary>
        public IReadOnlyList<Camera> Cameras => m_Cameras;
        /// <summary>
        /// List of instantiated lights
        /// </summary>
        public IReadOnlyList<Light> Lights => m_Lights;

#if UNITY_ANIMATION
        /// <summary>
        /// <see cref="Animation" /> component. Is null if scene has no
        /// animation clips.
        /// Only available if the built-in Animation module is enabled.
        /// </summary>
        public Animation LegacyAnimation { get; private set; }
#endif

        List<Camera> m_Cameras;
        List<Light> m_Lights;

        /// <summary>
        /// Adds a camera
        /// </summary>
        /// <param name="camera">Camera to be added</param>
        internal void AddCamera(Camera camera)
        {
            if (m_Cameras == null)
            {
                m_Cameras = new List<Camera>();
            }
            m_Cameras.Add(camera);
        }

        internal void AddLight(Light light)
        {
            if (m_Lights == null)
            {
                m_Lights = new List<Light>();
            }
            m_Lights.Add(light);
        }

#if UNITY_ANIMATION
        internal void SetLegacyAnimation(Animation animation) {
            LegacyAnimation = animation;
        }
#endif
    }
}
