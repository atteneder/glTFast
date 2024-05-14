// SPDX-FileCopyrightText: 2024 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System.Threading.Tasks;
using UnityEngine.Serialization;

namespace Samples.Documentation.Manual
{
    using UnityEngine;
    using GLTFast;
    using GLTFast.Export;
    using GLTFast.Logging;

    class ExportSamples : MonoBehaviour
    {

        [FormerlySerializedAs("path")]
        [SerializeField]
        string destinationFilePath;

        async void AdvancedExport()
        {
            #region AdvancedExport

            // CollectingLogger lets you programmatically go through
            // errors and warnings the export raised
            var logger = new CollectingLogger();

            // ExportSettings and GameObjectExportSettings allow you to configure the export
            // Check their respective source for details

            // ExportSettings provides generic export settings
            var exportSettings = new ExportSettings
            {
                Format = GltfFormat.Binary,
                FileConflictResolution = FileConflictResolution.Overwrite,

                // Export everything except cameras or animation
                ComponentMask = ~(ComponentType.Camera | ComponentType.Animation),

                // Boost light intensities
                LightIntensityFactor = 100f,

                // Ensure mesh vertex attributes colors and texture coordinate (channels 1 through 8) are always
                // exported, even if they are not used/referenced.
                PreservedVertexAttributes = VertexAttributeUsage.AllTexCoords | VertexAttributeUsage.Color,
            };

            // GameObjectExportSettings provides settings specific to a GameObject/Component based hierarchy
            var gameObjectExportSettings = new GameObjectExportSettings
            {
                // Include inactive GameObjects in export
                OnlyActiveInHierarchy = false,

                // Also export disabled components
                DisabledComponents = true,

                // Only export GameObjects on certain layers
                LayerMask = LayerMask.GetMask("Default", "MyCustomLayer"),
            };

            // GameObjectExport lets you create glTFs from GameObject hierarchies
            var export = new GameObjectExport(exportSettings, gameObjectExportSettings, logger: logger);

            // Example of gathering GameObjects to be exported (recursively)
            var rootLevelNodes = GameObject.FindGameObjectsWithTag("ExportMe");

            // Add a scene
            export.AddScene(rootLevelNodes, "My new glTF scene");

            // Async glTF export
            var success = await export.SaveToFileAndDispose(destinationFilePath);

            if (!success)
            {
                Debug.LogError("Something went wrong exporting a glTF");

                // Log all exporter messages
                logger.LogAll();
            }

            #endregion
        }

        void ExportSettingsDraco()
        {
            #region ExportSettingsDraco
            // ExportSettings provides generic export settings
            var exportSettings = new ExportSettings
            {
                // Enable Draco compression
                Compression = Compression.Draco,
                // Optional: Tweak the Draco compression settings
                DracoSettings = new DracoExportSettings
                {
                    positionQuantization = 12
                }
            };
            #endregion
        }

        async void Start()
        {
            await LocalTransform();
        }

        async Task LocalTransform()
        {
            #region LocalTransform
            var export = new GameObjectExport();

            // Add a scene
            export.AddScene(
                // Only the current gameObject as root node.
                new[] { gameObject },
                // The worldToLocalMatrix counter-acts any world-space transform, effectively moving the resulting
                // root node to the origin.
                gameObject.transform.worldToLocalMatrix,
                "Node at origin glTF scene"
                );
            #endregion

            // Async glTF export
            var success = await export.SaveToFileAndDispose(destinationFilePath);

            if (!success)
            {
                Debug.LogError("Something went wrong exporting a glTF");
            }
        }
    }
}
