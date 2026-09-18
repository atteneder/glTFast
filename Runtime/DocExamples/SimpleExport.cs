// SPDX-FileCopyrightText: 2024 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

namespace GLTFast.Documentation.Examples
{
    #region SimpleExport
    using Export;
    using UnityEngine;

    class SimpleExport : MonoBehaviour
    {

        [SerializeField]
        string destinationFilePath;

        async void Start()
        {

            // Example of gathering GameObjects to be exported (recursively)
            var rootLevelNodes = GameObject.FindGameObjectsWithTag("ExportMe");

            // GameObjectExport lets you create glTF files from GameObject hierarchies
            var export = new GameObjectExport();

            // Add a scene
            export.AddScene(rootLevelNodes);

            // Async glTF export
            var success = await export.SaveToFileAndDispose(destinationFilePath);

            if (!success)
            {
                Debug.LogError("Something went wrong exporting a glTF");
            }
        }
    }
    #endregion
}
