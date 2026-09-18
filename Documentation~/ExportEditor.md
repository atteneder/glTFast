# Editor Export

## Export from the Main Menu

The top menu has a couple of entries for exporting…

- …the active scene `File > Export Scene > glTF`
- …the current assets `Assets > Export glTF > glTF` (assets selected in project view)
- …the current selection `GameObject > Export glTF > glTF` (GameObjects selected in scene view or hierarchy view)

For each there are two options

- `glTF (.gltf)` exports a `.gltf` (JSON) plus external buffer and texture files
- `glTF-Binary (.glb)` exports a single `.glb` file containing all buffers and textures

Clicking any of these will open a file selection dialog. If additional files are to be generated (e.g. a buffer or image files) and there's a conflict (i.e. an existing file in that location), a follow-up dialog will as for permission to overwrite.

## Export via Script

Exporting via script works exactly the same as [Runtime Export](ExportRuntime.md), with the exception that you don't need to [include the required shaders](ExportRuntime.md#include-required-shaders).

### Editor Scripting: Enforce Synchronous I/O

When triggering an export from Editor scripting &mdash; for example a menu item, a custom inspector, or an asset post-processor &mdash; pass `forceSync: true` to [SaveToFileAndDispose](xref:GLTFast.Export.GameObjectExport.SaveToFileAndDispose*):

```csharp
await export.SaveToFileAndDispose(path, forceSync: true);
```

Unity does not pump the main-thread `SynchronizationContext` outside Play Mode, so awaited asynchronous I/O continuations (e.g. `Stream.WriteAsync`) may never resume and the export can hang silently. The `forceSync` overload routes writes through the synchronous I/O path instead, which completes deterministically in Edit Mode. At runtime (Play Mode or in a built player), keep the default asynchronous path to avoid blocking the main thread.

See [Batch Export Scene Roots to glTF Files](UseCaseBatchExport.md) for a complete Editor scripting example.

## Trademarks

*Unity&reg;* is a registered trademark of [Unity Technologies][unity].

*Khronos&reg;* is a registered trademark and *glTF&trade;* is a trademark of [The Khronos Group Inc][khronos].

[khronos]: https://www.khronos.org
[unity]: https://unity.com
