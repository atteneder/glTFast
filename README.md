# glTFast for Unity

<p align="center">
<img src="./Documentation~/Images/unity-gltf-logos.png" alt="Unity and glTF logos side by side" />
</p>

[![openupm](https://img.shields.io/npm/v/com.atteneder.gltfast?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.atteneder.gltfast/)
[![GitHub issues](https://img.shields.io/github/issues/atteneder/glTFast)](https://github.com/atteneder/glTFast/issues)
[![GitHub license](https://img.shields.io/github/license/atteneder/glTFast)](https://github.com/atteneder/glTFast/blob/main/LICENSE.md)
![Code coverage result](./Documentation~/Images/badge_linecoverage.svg "Code coverage result badge")

*glTFast* enables use of [glTF™ (GL Transmission Format)][gltf] asset files in [Unity][unity].

It focuses on speed, memory efficiency and a small build footprint while also providing:

- 100% [glTF 2.0 specification][gltf-spec] compliance
- Ease of use
- Robustness and Stability
- Customization and extensibility for advanced users

Check out the [demo project](https://github.com/atteneder/glTFastDemo) and try the [WebGL Demo][gltfast-web-demo].

## Features

*glTFast* supports the full [glTF 2.0 specification][gltf-spec] and many extensions. It works with Universal, High Definition and the Built-In Render Pipelines on all platforms.

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

The easiest way to install is to download and open the [Installer Package](https://package-installer.glitch.me/v1/installer/OpenUPM/com.atteneder.gltfast?registry=https%3A%2F%2Fpackage.openupm.com&scope=com.atteneder)

It runs a script that installs *glTFast* via a [scoped registry](https://docs.unity3d.com/Manual/upm-scoped.html).

Afterwards *glTFast* and further, optional packages are listed in the *Package Manager* (under *My Registries*) and can be installed and updated from there.

### Optional Packages

There are some related package that improve *glTFast* by extending its feature set.

- [Draco 3D Data Compression Unity Package][DracoUnity] (provides support for [KHR_draco_mesh_compression][ExtDraco])
- [KTX/Basis Texture Unity Package][KtxUnity] (provides support for [KHR_texture_basisu][ExtBasisU])
- [*meshoptimizer decompression for Unity*][Meshopt] (provides support for [EXT_meshopt_compression][ExtMeshopt])

*glTFast* 5.x requires Unity 2019.3 or newer. For older Unity versions see [Legacy Installation](./Documentation~/gltfast-1.md).

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

See [Runtime Loading via Script](./Documentation~/ImportRuntime.md#runtime-loading-via-script) in the documentation for more details and instructions how to [customize the loading behaviour](./Documentation~/ImportRuntime.md#customize-loading-behavior) via script.

### Editor Import

Move or copy *glTF* files into your project's *Assets* folder, similar to other 3D formats:

![Editor Import][import-gif]

*glTFast* will import them to native Unity prefabs and add them to the asset database.

See [Editor Import](./Documentation~/ImportEditor.md) in the documentation for details.

### Editor Export

The main menu has a couple of [entries for glTF export](./Documentation~/ExportEditor.md#export-from-the-main-menu) under `File > Export` and glTFs can also be
created [via script](./Documentation~/ExportEditor.md#export-via-script).

## Project Setup

### Materials and Shader Variants

❗ IMPORTANT ❗

*glTFast* uses custom shader graphs that you **have** to include in builds in order to make materials work. If materials are fine in the Unity Editor but not in builds, chances are some shaders (or variants) are missing.

Read the section *Materials and Shader Variants* in the [Documentation](./Documentation~/ProjectSetup.md#materials-and-shader-variants) for details.

## Get involved

Contributions in the form of ideas, comments, critique, bug reports, pull requests are highly appreciated. Feel free to get in contact if you consider using or improving *glTFast*.

## Supporters

[Unity Technologies][unity]

Thanks to [Embibe][embibe] for sponsoring the development of skin support! ❤️

## License

Copyright (c) 2020-2022 Andreas Atteneder, All Rights Reserved.

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

*Unity* is a registered trademark of [Unity Technologies][unity].

*Khronos®* is a registered trademark and *glTF™* is a trademark of [The Khronos Group Inc][khronos].

[embibe]: https://www.embibe.com
[DracoUnity]: https://github.com/atteneder/DracoUnity
[ExtBasisU]: https://github.com/KhronosGroup/glTF/tree/master/extensions/2.0/Khronos/KHR_texture_basisu
[ExtDraco]: https://github.com/KhronosGroup/glTF/tree/master/extensions/2.0/Khronos/KHR_draco_mesh_compression
[ExtMeshopt]: https://github.com/KhronosGroup/glTF/tree/main/extensions/2.0/Vendor/EXT_meshopt_compression
[gltf-spec]: https://www.khronos.org/registry/glTF/specs/2.0/glTF-2.0.html
[gltf]: https://www.khronos.org/gltf
[gltfasset_component]: ./Documentation~/Images/gltfasset_component.png  "Inspector showing a GltfAsset component added to a GameObject"
[gltfast-web-demo]: https://gltf.pixel.engineer
[import-gif]: ./Documentation~/Images/import.gif  "Video showing glTF files being copied into the Assets folder and imported"
[khronos]: https://www.khronos.org
[KtxUnity]: https://github.com/atteneder/KtxUnity
[Meshopt]: https://docs.unity3d.com/Packages/com.unity.meshopt.decompress@0.1/manual/index.html
[unity]: https://unity.com
[workflows]: ./Documentation~/index.md#workflows