# Editor Import

You can move/copy *glTF* files into your project's *Assets* folder, similar to other 3D formats. *glTFast* will import them to native Unity prefabs and add them to the asset database.

![Editor Import][import-gif]

Don't forget to also copy over companion buffer (`.bin`) and image files! The file names and relative paths cannot be changed, otherwise references may break.

Select a glTF in the Project view to see its import settings and eventual warnings/errors in the Inspector. Expand it in the Project View to see the imported components (Scenes, Meshes, Materials, AnimationClips and Textures).

[import-gif]: Images/import.gif  "Video showing glTF files being copied into the Assets folder and imported"
