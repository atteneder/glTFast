# Use case: Use glTFast Add-on API

This use case describes the steps to use glTFast Add-on API to import custom data from the `extras` property of a glTF&trade; JSON object. This example uses Newtonsoft JSON parser to deserialize data.

To accomplish this use case, do the following:

1. Add custom data in a glTF asset
2. Create a custom glTF import behavior
3. Add assembly definitions
4. Set up a new scene
5. Import the glTF asset in runtime

## Before you start

Before you start, you must add the following package dependencies to your project.

* In the `manifest.json` file, add the following dependencies:

```json
  {
    "dependencies": {
      // Add these lines:
      // Replace "<x.y.z>" with the version you wish to install
      "com.unity.cloud.gltfast": "<x.y.z>",
      "com.unity.nuget.newtonsoft-json": "<x.y.z>"
      // Other dependencies...
    }
  }
```

## How do I...?

### Add custom data in a glTF asset

Add some custom data in the `extras` property of a glTF JSON object:

```json
  "nodes": [
    {
      // Example of mesh data in a glTF
      "mesh": 0,
      "name": "Cube",

      // Add these lines:
      "extras": {
        "some-extra-key": "some-extra-value"
      }
    }
  ]
```

### Create a custom glTF import behavior

To create a custom glTF import behavior, follow these steps:

1. Open your Unity&reg; Project.
2. Go to the **Assets** folder in the Project window.
3. Select and hold **Create**.
4. Select **C# Script**.
5. Rename the new script as `CustomGltfImport`.
6. Open the `CustomGltfImport` script and replace the content with the following:
   [!code-cs [custom-gltf-import](../Samples/Documentation/Manual/CustomGltfImport.cs#CustomGltfImport)]
7. Repeat step 2-4 to create another new script
8. Rename the new script as `ExtraData`.
9. Open the `ExtraData` script and replace the content with the following:
   [!code-cs [extra-data](../Samples/Documentation/Manual/ExtraData.cs#ExtraData)]

### Add assembly definitions

In your [assembly definition](https://docs.unity3d.com/Manual/ScriptCompilationAssemblyDefinitionFiles.html) file (`.asmdef` file), add the following references:

```json
  "references": [
    // Add these lines:
    "glTFast",
    "glTFast.Newtonsoft"
    // Other references...
  ]
```

### Set up a new scene

To set up a new scene, follow these steps:

1. Create a new scene.
2. Create a GameObject called **GltfImport**.
3. Select **Add Component** in the Inspector window and add the **Custom Gltf Import** component.
4. In the **Uri** field, set the path to point to where the glTF asset is stored.

### Import the glTF asset in runtime

Select **Play**, the glTF asset should be loaded and displayed at runtime.

You can verify that the custom data in the `extras` property of the glTF is imported correctly by inspecting the loaded glTF asset:
![Screen capture that displays the extra data in the imported glTF asset](Images/gltf-extra-data.PNG)

## Trademarks

*Unity&reg;* is a registered trademark of [Unity Technologies][Unity].

*glTF&trade;* is a trademark of [The Khronos Group Inc][Khronos].

[Khronos]: https://www.khronos.org
[Unity]: https://unity.com
