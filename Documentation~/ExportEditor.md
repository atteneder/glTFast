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

## Trademarks

*Unity&reg;* is a registered trademark of [Unity Technologies][unity].

*Khronos&reg;* is a registered trademark and *glTF&trade;* is a trademark of [The Khronos Group Inc][khronos].

[khronos]: https://www.khronos.org
[unity]: https://unity.com
