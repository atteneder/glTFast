// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Unity.Cloud.Gltfast
{
    /// <summary>
    /// Descriptor of a glTF scene instance
    /// </summary>
    [MovedFrom(true, sourceNamespace: "GLTFast", sourceAssembly: "glTFast")]
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

        /// <summary>
        /// Enables controlling and applying materials variants.
        /// </summary>
        public MaterialsVariantsControl MaterialsVariantsControl { get; private set; }

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
        public void AddCamera(Camera camera)
        {
            m_Cameras ??= new List<Camera>();
            m_Cameras.Add(camera);
        }

        /// <summary>
        /// Adds a light.
        /// </summary>
        /// <param name="light">Light to be added.</param>
        public void AddLight(Light light)
        {
            m_Lights ??= new List<Light>();
            m_Lights.Add(light);
        }

        internal void SetMaterialsVariantsControl(MaterialsVariantsControl control)
        {
            MaterialsVariantsControl = control;
        }

#if UNITY_ANIMATION
        /// <summary>
        /// Sets the <see cref="LegacyAnimation"/>.
        /// Use this from your custom <see cref="IInstantiator"/> implementation.
        /// </summary>
        /// <param name="animation">Animation component.</param>
        /// <seealso cref="GameObjectInstantiator"/>
        public void SetLegacyAnimation(Animation animation)
        {
            LegacyAnimation = animation;
        }
#endif
    }
}
