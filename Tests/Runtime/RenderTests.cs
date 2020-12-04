// Copyright 2020 Andreas Atteneder
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

#if GLTFAST_RENDER_TEST
        
using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Graphics;

// using UnityEngine.Rendering.Universal;
// using UnityEngine.Experimental.Rendering.Universal;

namespace GLTFast.Tests
{
    /// <summary>
    /// This class is based on UniversalGraphicsTests from the Graphics Test Framework package
    /// </summary>
    public class RenderTests
    {
#if UNITY_ANDROID
    static bool wasFirstSceneRan = false;
    const int firstSceneAdditionalFrames = 3;
#endif
        public const string universalPackagePath = "Assets/ReferenceImages";

        [UnityTest, Category("UniversalRP")]
        [PrebuildSetup("SetupGraphicsTestCases")]
        [UseGraphicsTestCases(universalPackagePath)]


        public IEnumerator Run(GraphicsTestCase testCase)
        {
// #if ENABLE_VR
//         // XRTODO: Fix XR tests on macOS or disable them from Yamato directly
//         if (XRGraphicsAutomatedTests.enabled && (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer))
//             Assert.Ignore("Universal XR tests do not run on macOS.");
// #endif
            SceneManager.LoadScene(testCase.ScenePath);

            // Always wait one frame for scene load
            yield return null;

            var cameras = GameObject.FindGameObjectsWithTag("MainCamera").Select(x=>x.GetComponent<Camera>());
            var settings = Object.FindObjectOfType<UniversalGraphicsTestSettings>();
            Assert.IsNotNull(settings, "Invalid test scene, couldn't find UniversalGraphicsTestSettings");

            var gltf = Object.FindObjectOfType<GltfBoundsAsset>();
            Assert.IsNotNull(gltf, "Invalid test scene, couldn't find GltfAsset");

            while (!gltf.isDone) {
                yield return null;
            }

            
            // position camera based on AABB
            var cam = cameras.First();
            FrameBoundsCamera.FrameBounds(cam,gltf.transform,gltf.bounds);

// #if ENABLE_VR
//         if (XRGraphicsAutomatedTests.enabled)
//         {
//             if (settings.XRCompatible)
//             {
//                 XRGraphicsAutomatedTests.running = true;
//             }
//             else
//             {
//                 Assert.Ignore("Test scene is not compatible with XR and will be skipped.");
//             }
//         }
// #endif

            Scene scene = SceneManager.GetActiveScene();

            yield return null;

            int waitFrames = settings.WaitFrames;

            if (settings.ImageComparisonSettings.UseBackBuffer && settings.WaitFrames < 1)
            {
                waitFrames = 1;
            }
            for (int i = 0; i < waitFrames; i++)
                yield return new WaitForEndOfFrame();

#if UNITY_ANDROID
        // On Android first scene often needs a bit more frames to load all the assets
        // otherwise the screenshot is just a black screen
        if (!wasFirstSceneRan)
        {
            for(int i = 0; i < firstSceneAdditionalFrames; i++)
            {
                yield return null;
            }
            wasFirstSceneRan = true;
        }
#endif

            ImageAssert.AreEqual(testCase.ReferenceImage, cameras.Where(x => x != null), settings.ImageComparisonSettings);

            // Does it allocate memory when it renders what's on the main camera?
            bool allocatesMemory = false;
            var mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();

            try
            {
                ImageAssert.AllocatesMemory(mainCamera, settings?.ImageComparisonSettings);
            }
            catch (AssertionException)
            {
                allocatesMemory = true;
            }

            if (allocatesMemory)
                Assert.Fail("Allocated memory when rendering what is on main camera");
        }

#if UNITY_EDITOR
        [TearDown]
        public void DumpImagesInEditor()
        {
            UnityEditor.TestTools.Graphics.ResultsUtility.ExtractImagesFromTestProperties(TestContext.CurrentContext.Test);
        }

// #if ENABLE_VR
//     [TearDown]
//     public void ResetSystemState()
//     {
//         XRGraphicsAutomatedTests.running = false;
//     }
// #endif
#endif
    }
}

#endif
