# Runtime Export

You can export individual GameObjects or entire scenes to glTF files at runtime.

## Include Required Shaders

To be able to export certain textures correctly, a couple of shaders are required. They are located at `Runtime/Shader/Export`. Make sure to include them all in your build.

The easiest way to include them is to add `glTFExport.shadervariants` to the list of *Preloaded Shaders* under *Project Settings* > *Graphics* > *Shader Loading*.

## Export via Script

> Note: The `GLTFast.Export` namespace can only be used if you reference both `glTFast` and `glTFast.Export` Assemblies in your [Assembly Definition][asmdef].

Here's a step-by-step guilde to export a GameObject hierarchy/scene from script

- Create an instance of `GLTFast.Export.GameObjectExport`
- Add content via `AddScene`
- Two options for the final export
  - Call `SaveToFileAndDispose` to export a glTF to a file(s)
  - Call `SaveToStreamAndDispose` to export to a `System.IO.Stream`

glTF export might create more than one file. For example the binary buffer is usually a separate `.bin` file and textures might be separate files as well.


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

> Exporting to a `Stream` currently only works for self-contained glTF-Binary files (where the binary buffer and all textures are included in the `.glb` file). Trying other export settings will fail.

[asmdef]: https://docs.unity3d.com/Manual/ScriptCompilationAssemblyDefinitionFiles.html
