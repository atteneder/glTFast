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

### Scene Origin

When adding GameObjects to a glTF scene, the resulting glTF root nodes' positions will be their original GameObjects' world position in the Unity scene. That might be undesirable (e.g. if the scene is far off the origin and thus not centered), so [AddScene](xref:GLTFast.Export.GameObjectExport.AddScene(ICollection{UnityEngine.GameObject},Unity.Mathematics.float4x4,System.String)) allows you to provide an inverse scene origin matrix that'll be applied to all root-level nodes.

Here's an example how to export a GameObject, discarding its transform:

[!code-cs [local-transform](../Samples/Documentation/Manual/ExportSamples.cs#LocalTransform)]

### Vertex Attribute Discarding

In certain cases glTFast discards mesh vertex attributes that are not used or required. This not only reduces the resulting glTF's file size, but in case of vertex colors, is necessary to preserve visual consistency.

This behavior might be undesirable, for example in authoring workflows where the resulting glTF will be further edited. In that case vertex attribute discarding can be disabled on a per-attribute basis by setting [ExportSettings' PreservedVertexAttributes](xref:GLTFast.Export.ExportSettings.PreservedVertexAttributes) mask.

Examples of vertex attribute discarding:

- Vertex colors, when the assigned material(s) do not use them.
- Normals and tangents, when the assigned material is unlit and does not require them for shading.
- When no material was assigned, a default fallback material will be assumed. This does not require tangents nor texture coordinates, hence those are discarded.

> **NOTE:** Not all cases of potential discarding are covered at the moment (e.g. unused texture coordinates when no textures are assigned).

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
