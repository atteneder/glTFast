# Editor Import

You can move/copy *glTF&trade;* files into your project's *Assets* folder, similar to other 3D formats. *Unity glTFast* will import them to native Unity prefabs and add them to the asset database.

![Editor Import][import-gif]

Don't forget to also copy over companion buffer (`.bin`) and image files! The file names and relative paths cannot be changed, otherwise references may break.

Select a glTF in the Project view to see its import settings and eventual warnings/errors in the Inspector. Expand it in the Project View to see the imported components (Scenes, Meshes, Materials, AnimationClips and Textures).

## Default Importer Selection

*Unity glTFast* uses Unity's `ScriptedImporter` interface. For any given file format (file extension) there has to be one default importer and there can be additional, alternative importers. *Unity glTFast* will register itself as the default importer for the `.gltf` and `.glb` extensions.

You can install any number of alternative importers, but if any of those registers itself as default importer as well, this will result in an error like this:

> Multiple scripted importers are targeting the extension 'glb' and have all been rejected: …

The recommended solution is to move other importers from default to alternative (consult their respective documentations how to do that)

If that's not possible or wanted, you can de-prioritize *Unity glTFast* from default to alternative importer by adding the `GLTFAST_FORCE_DEFAULT_IMPORTER_OFF` to your project's scripting defines.

[import-gif]: Images/import.gif  "Video showing glTF files being copied into the Assets folder and imported"

## Trademarks

*Unity&reg;* is a registered trademark of [Unity Technologies][unity].

*Khronos&reg;* is a registered trademark and *glTF&trade;* is a trademark of [The Khronos Group Inc][khronos].

[khronos]: https://www.khronos.org
[unity]: https://unity.com
