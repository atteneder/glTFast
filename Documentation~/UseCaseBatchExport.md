# Batch Export Scene Roots to glTF Files

A common Editor scripting use case is exporting GameObjects to individual `.glb` files.

The example below adds a `Tools > glTFast Examples > Batch Export` menu entry.

[!code-cs [batch-export](../Editor/DocExamples/EditorExportSamples.cs#BatchExportAllObjects)]

A few details worth highlighting:

- `forceSync: true` is required because the menu item runs in Edit Mode &mdash; see [Editor Scripting: Enforce Synchronous I/O](ExportEditor.md#editor-scripting-enforce-synchronous-io) for the rationale.

## Trademarks

*Unity&reg;* is a registered trademark of [Unity Technologies][unity].

*Khronos&reg;* is a registered trademark and *glTF&trade;* is a trademark of [The Khronos Group Inc][khronos].

[khronos]: https://www.khronos.org
[unity]: https://unity.com
