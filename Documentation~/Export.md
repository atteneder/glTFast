# glTF Export

## Editor Export

### Export from the Main Menu

The main menu has a couple of entries for export under `File > Export`:

- `Scene (glTF)` exports the entire active scene to glTF (`.gltf` plus external buffer and texture files)
- `Scene (glTF-Binary)` exports the entire active scene to glTF-Binary (`.glb`)
- `Selection (glTF)` exports the currently selected GameObject (with its hierarchy) to glTF (`.gltf` plus external buffer and texture files)
- `Selection (glTF-Binary)` exports the currently selected GameObject (with its hierarchy) to glTF-Binary (`.glb`)

Clicking any of these will open a file selection dialog. If additional files are to be generated (e.g. a buffer or image files) and there's a conflict (i.e. an existing file in that location), a follow-up dialog will as for permission to overwrite.

### Export via Script

> Note: The `GLTFast.Export` namespace can only be used if you reference both `glTFast` and `glTFast.Export` Assemblies in your [Assembly Definition][asmdef].

To export a GameObject hierarchy/scene from script, create an instance of `GLTFast.Export.GameObjectExport`,
add content via `AddScene` and finally call `SaveToFileAndDispose` to export to a file.

```c#
using UnityEngine;
using GLTFast.Export;

public class TestExport : MonoBehaviour {

    [SerializeField]
    string path;

    async void SimpleExport() {

        // Example of gathering GameObjects to be exported (recursively)
        var rootLevelNodes = GameObject.FindGameObjectsWithTag("ExportMe");
        
        // GameObjectExport lets you create glTFs from GameObject hierarchies
        var export = new GameObjectExport();

        // Add a scene
        export.AddScene(rootLevelNodes);

        // Async glTF export
        bool success = await export.SaveToFileAndDispose(path);

        if(!success) {
            Debug.LogError("Something went wrong exporting a glTF");
        }
    }
}
```

After calling `SaveToFileAndDispose` the GameObjectExport instance becomes invalid. Do not re-use it.

Further, the export can be customized by passing settings and injectables to the `GameObjectExport`'s
constructor:

```c#
using UnityEngine;
using GLTFast;
using GLTFast.Export;

public class TestExport : MonoBehaviour {

    [SerializeField]
    string path;

    async void AdvancedExport() {

        // CollectingLogger lets you programatically go through
        // errors and warnings the export raised
        var logger = new CollectingLogger();

        // ExportSettings allow you to configure the export
        // Check its source for details
        var exportSettings = new ExportSettings {
            format = GltfFormat.Binary,
            fileConflictResolution = FileConflictResolution.Overwrite
        };

        // GameObjectExport lets you create glTFs from GameObject hierarchies
        var export = new GameObjectExport( exportSettings, logger: logger);

        // Example of gathering GameObjects to be exported (recursively)
        var rootLevelNodes = GameObject.FindGameObjectsWithTag("ExportMe");

        // Add a scene
        export.AddScene(rootLevelNodes, "My new glTF scene");

        // Async glTF export
        bool success = await export.SaveToFileAndDispose(path);

        if(!success) {
            Debug.LogError("Something went wrong exporting a glTF");
            // Log all exporter messages
            logger.LogAll();
        }
    }
}
```

## Runtime Export

> Note: This feature is coming soon (see [issue](https://github.com/atteneder/glTFast/issues/259))

[asmdef]: https://docs.unity3d.com/Manual/ScriptCompilationAssemblyDefinitionFiles.html
