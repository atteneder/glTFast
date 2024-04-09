// SPDX-FileCopyrightText: 2024 Unity Technologies and the Draco for Unity authors
// SPDX-License-Identifier: Apache-2.0

#if !(UNITY_ANDROID || UNITY_WEBGL) || UNITY_EDITOR
#define LOCAL_LOADING
#endif

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using GLTFast.Tests.Import;
using UnityEngine;

namespace GLTFast.Tests
{
    /// <summary>
    /// Helper component that loads all glTFs of a GltfTestCaseSet.
    /// Typical use-cases during development:
    /// - Extract all shader variants
    /// - Performance testing
    /// </summary>
    // Not actually used by any test at the moment, so exclude from coverage.
    [ExcludeFromCodeCoverage]
    class LoadGltfTestCaseSet : MonoBehaviour
    {
        enum Strategy
        {
            Fast,
            Responsive
        }

        [SerializeField]
        GltfTestCaseSet m_GltfTestCaseSet;

        [SerializeField]
        protected bool m_Local = true;

        [SerializeField]
        Strategy m_Strategy = Strategy.Responsive;

        [SerializeField]
        int m_NumVisibleAssets = 1;

        Queue<GltfAssetBase> m_VisibleAssets = new Queue<GltfAssetBase>();

        async void Start()
        {
            await MassLoadRoutine(m_GltfTestCaseSet);
        }

        async Task MassLoadRoutine(GltfTestCaseSet set)
        {

            // stopWatch.StartTime();

            IDeferAgent deferAgent;
            if (m_Strategy == Strategy.Fast)
            {
                deferAgent = new UninterruptedDeferAgent();
            }
            else
            {
                deferAgent = gameObject.AddComponent<TimeBudgetPerFrameDeferAgent>();
            }

            var loadTasks = new List<Task>(set.TestCaseCount);

            var rootDir = set.RootPath;


            foreach (var testCase in set.IterateTestCases())
            {
                var path = Path.Combine(rootDir, testCase.relativeUri);
                if (m_Local)
                {
                    var loadTask = LoadIt(
#if LOCAL_LOADING
                        $"file://{path}"
#else
                        path
#endif
                        , deferAgent
                    );
                    loadTasks.Add(loadTask);
                    await deferAgent.BreakPoint();
                }
                else
                {
                    var loadTask = LoadIt(path, deferAgent);
                    loadTasks.Add(loadTask);
                    await deferAgent.BreakPoint();
                }
            }

            await Task.WhenAll(loadTasks);

            // stopWatch.StopTime();
            // Debug.LogFormat("Finished loading {1} glTFs in {0} milliseconds!",stopWatch.lastDuration,set.itemCount);
        }

        async Task LoadIt(string n, IDeferAgent deferAgent)
        {
            var go = new GameObject(Path.GetFileNameWithoutExtension(n));
            // Debug.Log(go.name);
            var gltfAsset = go.AddComponent<GltfAsset>();
            gltfAsset.LoadOnStartup = false; // prevent auto-loading
            await gltfAsset.Load(n, null, deferAgent); // load manually with custom defer agent
            if (m_VisibleAssets.Count >= m_NumVisibleAssets)
            {
                var oldAsset = m_VisibleAssets.Dequeue();
                // oldAsset.gameObject.SetActive(false);
                Destroy(oldAsset.gameObject);
            }
            m_VisibleAssets.Enqueue(gltfAsset);
        }
    }
}
