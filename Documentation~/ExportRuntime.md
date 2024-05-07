# Runtime Export

You can export individual GameObjects or entire scenes to glTF&trade; files at runtime.

## Include Required Shaders

To be able to export certain textures correctly, a couple of shaders are required. They are located at `Runtime/Shader/Export`. Make sure to include them all in your build.

The easiest way to include them is to add `glTFExport.shadervariants` to the list of *Preloaded Shaders* under *Project Settings* > *Graphics* > *Shader Loading*.

## Export via Script

> **NOTE:** The `GLTFast.Export` namespace can only be used if you reference both `glTFast` and `glTFast.Export` Assemblies in your [Assembly Definition][asmdef].

Here's a step-by-step guide to export a GameObject hierarchy/scene from script

- Create an instance of [GameObjectExport](xref:GLTFast.Export.GameObjectExport)
- Add content via [AddScene](xref:GLTFast.Export.GameObjectExport.AddScene*)
- Two options for the final export
  - Call [SaveToFileAndDispose](xref:GLTFast.Export.GameObjectExport.SaveToFileAndDispose*) to export a glTF to a file(s)
  - Call [SaveToStreamAndDispose](xref:GLTFast.Export.GameObjectExport.SaveToStreamAndDispose*) to export to a [Stream][Stream]

glTF export might create more than one file. For example the binary buffer is usually a separate `.bin` file and textures might be separate files as well.

[!code-cs [simple-export](../Samples/Documentation/Manual/SimpleExport.cs#SimpleExport)]

After calling [SaveToFileAndDispose](xref:GLTFast.Export.GameObjectExport.SaveToFileAndDispose*) the GameObjectExport instance becomes invalid. Do not re-use it.

Further, the export can be customized by passing [ExportSettings](xref:GLTFast.Export.ExportSettings), [GameObjectExportSettings](xref:GLTFast.Export.GameObjectExportSettings) and injectables to [GameObjectExport](xref:GLTFast.Export.GameObjectExport)'s constructor:

[!code-cs [advanced-export](../Samples/Documentation/Manual/ExportSamples.cs#AdvancedExport)]

> **NOTE:** Exporting to a [Stream][Stream] currently only works for self-contained glTF-Binary files (where the binary buffer and all textures are included in the `.glb` file). Trying other export settings will fail.

### Draco Compression

*Unity glTFast* supports applying [Google Draco&trade; 3D Data compression][Draco] to meshes. This requires the [Draco for Unity][DracoForUnity] package to be installed.

[!code-cs [draco-export](../Samples/Documentation/Manual/ExportSamples.cs#ExportSettingsDraco)]

## Trademarks

*Unity&reg;* is a registered trademark of [Unity Technologies][unity].

*Khronos&reg;* is a registered trademark and *glTF&trade;* is a trademark of [The Khronos Group Inc][khronos].

*Draco&trade;* is a trademark of [*Google LLC*][GoogleLLC].

[asmdef]: https://docs.unity3d.com/Manual/ScriptCompilationAssemblyDefinitionFiles.html
[Draco]: https://google.github.io/draco/
[DracoForUnity]: https://docs.unity3d.com/Packages/com.unity.cloud.draco@latest
[GoogleLLC]: https://about.google/
[khronos]: https://www.khronos.org
[unity]: https://unity.com
[Stream]: https://learn.microsoft.com/en-us/dotnet/api/system.io.stream
