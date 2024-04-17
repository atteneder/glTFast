# Unity glTFast

[![codecov](https://codecov.unity3d.com/ghe/unity/com.unity.cloud.gltfast.src/graph/badge.svg?token=QZYEFH0SDC)](https://codecov.unity3d.com/ghe/unity/com.unity.cloud.gltfast.src)

<p align="center">
<img src="./Documentation~/Images/unity-gltf-logos.png" alt="Unity and glTF logos side by side" />
</p>

*Unity glTFast* enables use of [glTF&trade; (GL Transmission Format)][gltf] asset files in [Unity&reg;][unity].

It focuses on speed, memory efficiency and a small build footprint while also providing:

- 100% [glTF 2.0 specification][gltf-spec] compliance
- Ease of use
- Robustness and Stability
- Customization and extensibility for advanced users

## Features

*Unity glTFast* supports the full [glTF 2.0 specification][gltf-spec] and many extensions. It works with Universal, High Definition and the Built-In Render Pipelines on all platforms.

See the [comprehensive list of supported features and extensions](./Documentation~/features.md).

### Workflows

There are four use-cases for glTF within Unity

- Import
  - [Runtime Import/Loading](./Documentation~/ImportRuntime.md) in games/applications
  - [Editor Import](./Documentation~/ImportEditor.md) (i.e. import assets at design-time)
- Export
  - [Runtime Export](./Documentation~/ExportRuntime.md) (save and share dynamic, user-generated 3D content)
  - [Editor Export](./Documentation~/ExportEditor.md) (Unity as glTF authoring tool)

[![Schematic diagram of the four glTF workflows](./Documentation~/Images/Unity-glTF-workflows.png "The four glTF workflows")][workflows]

Read more about the workflows in the [documentation][workflows].

## Installing

[Installation instructions](./Documentation~/installation.md)

> **NOTE:** This package originally had the identifier `com.atteneder.gltfast`. Consult the [upgrade guide](./Documentation~/UpgradeGuides.md#unity-fork) to learn how to switch to the Unity version (`com.unity.cloud.gltfast`) or [install the original package](./Documentation~/Original.md).

## Usage

You can load a glTF asset from an URL or a file path.

### Runtime Loading via Component

Add a `GltfAsset` component to a GameObject.

![GltfAsset component][gltfasset_component]

### Runtime Loading via Script

```C#
var gltf = gameObject.AddComponent<GLTFast.GltfAsset>();
gltf.url = "https://raw.githubusercontent.com/KhronosGroup/glTF-Sample-Models/master/2.0/Duck/glTF/Duck.gltf";
```

See [Runtime Loading via Script](./Documentation~/ImportRuntime.md#runtime-loading-via-script) in the documentation for more details and instructions how to [customize the loading behavior](./Documentation~/ImportRuntime.md#customize-loading-behavior) via script.

### Editor Import

Move or copy glTF files into your project's *Assets* folder, similar to other 3D formats:

![Editor Import][import-gif]

*Unity glTFast* will import them to native Unity prefabs and add them to the asset database.

See [Editor Import](./Documentation~/ImportEditor.md) in the documentation for details.

### Editor Export

The main menu has a couple of [entries for glTF export](./Documentation~/ExportEditor.md#export-from-the-main-menu) under `File > Export` and glTFs can also be
created [via script](./Documentation~/ExportEditor.md#export-via-script).

## Project Setup

### Materials and Shader Variants

❗ IMPORTANT ❗

*Unity glTFast* uses custom shader graphs that you **have** to include in builds in order to make materials work. If materials are fine in the Unity Editor but not in builds, chances are some shaders (or variants) are missing.

Read the section *Materials and Shader Variants* in the [Documentation](./Documentation~/ProjectSetup.md#materials-and-shader-variants) for details.

## Contribution

See [CONTRIBUTING.md](CONTRIBUTING.md).

## License

Copyright 2023 Unity Technologies and the Unity glTFast authors

Licensed under the Apache License, Version 2.0 (the "License");
you may not use files in this repository except in compliance with the License.
You may obtain a copy of the License at

   <http://www.apache.org/licenses/LICENSE-2.0>

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.

## Trademarks

*Unity&reg;* is a registered trademark of [Unity Technologies][unity].

*Khronos&reg;* is a registered trademark and [glTF&trade;][gltf] is a trademark of [The Khronos Group Inc][khronos].

[gltf-spec]: https://www.khronos.org/registry/glTF/specs/2.0/glTF-2.0.html
[gltf]: https://www.khronos.org/gltf
[gltfasset_component]: ./Documentation~/Images/gltfasset_component.png  "Inspector showing a GltfAsset component added to a GameObject"
[import-gif]: ./Documentation~/Images/import.gif  "Video showing glTF files being copied into the Assets folder and imported"
[khronos]: https://www.khronos.org
[unity]: https://unity.com
[workflows]: ./Documentation~/index.md#workflows
