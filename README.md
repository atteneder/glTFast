# glTFast 🚀

<p align="center">
<img src="./Documentation~/img/gltf-unity-logos.png" />
</p>

[![openupm](https://img.shields.io/npm/v/com.atteneder.gltfast?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.atteneder.gltfast/)
[![GitHub issues](https://img.shields.io/github/issues/atteneder/glTFast)](https://github.com/atteneder/glTFast/issues)
[![GitHub license](https://img.shields.io/github/license/atteneder/glTFast)](https://github.com/atteneder/glTFast/blob/main/LICENSE.md)

*glTFast* enables loading [glTF™ (GL Transmission Format)][gltf] asset files in [Unity][unity].

It focuses on speed, memory efficiency and a small build footprint.

Try the [WebGL Demo][gltfast-web-demo] and check out the [demo project](https://github.com/atteneder/glTFastDemo).

## Features

*glTFast* supports runtime loading of glTF 2.0 files.

It supports large parts of the glTF 2.0 specification plus many extensions and runs on following platforms:

- WebGL
- iOS
- Android
- Windows
- macOS
- Linux
- Universal Windows Platform

It is [planned](#goals) to become feature complete. Most notable missing features are:

- No animations
- No morph targets

See the [list of features/extensions](./Documentation~/features.md) for details and limitations.

## Installing

The easiest way to install is to download and open the [Installer Package](https://package-installer.glitch.me/v1/installer/OpenUPM/com.atteneder.gltfast?registry=https%3A%2F%2Fpackage.openupm.com&scope=com.atteneder)

It runs a script that installs *glTFast* via a [scoped registry](https://docs.unity3d.com/Manual/upm-scoped.html).

Afterwards *glTFast* and further, optional packages are listed in the *Package Manager* (under *My Registries*) and can be installed and updated from there.

### Optional dependencies

- [Draco 3D Data Compression Unity Package](https://github.com/atteneder/DracoUnity) (provides support for [KHR_draco_mesh_compression](https://github.com/KhronosGroup/glTF/tree/master/extensions/2.0/Khronos/KHR_draco_mesh_compression))
- [KTX/Basis Texture Unity Package](https://github.com/atteneder/KtxUnity) (in Beta; provides support for [KHR_texture_basisu](https://github.com/KhronosGroup/glTF/tree/master/extensions/2.0/Khronos/KHR_texture_basisu))

<details><summary>Alternative: Install via GIT URL</summary>

Add *glTFast* via Unity's Package Manager ( Window -> Package Manager ). Click the ➕ on the top left and choose *Add package from GIT URL*.

![Package Manager -> + -> Add Package from git URL][upm_install]

Enter the following URL:

`https://github.com/atteneder/glTFast.git`

To add support for Draco mesh compression, repeat the last step and also add the DracoUnity packages using this URL:

`https://gitlab.com/atteneder/DracoUnity.git`

> Note: You have to have a GIT LFS client (large file support) installed on your system. Otherwise you will get an error that the native library file (dll on Windows) is corrupt!

</details>

*glTFast* 2.x requires Unity 2019.3 or newer. For older Unity versions see [Legacy Installation](./Documentation~/gltfast-1.md).

## Usage

You can load a glTF asset from an URL or a file path.

### Load via Component

Add a `GltfAsset` component to a GameObject.

![GltfAsset component][gltfasset_component]

### Load via Script

```C#
var gltf = gameObject.AddComponent<GLTFast.GltfAsset>();
gltf.url = "https://raw.githubusercontent.com/KhronosGroup/glTF-Sample-Models/master/2.0/Duck/glTF/Duck.gltf";
```

See [Load via Script](./Documentation~/glTFast.md#load-via-script) in the detailed documentation for instructions how to customize the loading behaviour via script.

### Materials and Shader Variants

❗ IMPORTANT ❗

*glTFast* uses custom shaders that you **have** to include in builds in order to make materials work. If materials are fine in the Unity Editor but not in builds, chances are some shaders (or variants) are missing.

Read the section *Materials and Shader Variants* in the [Documentation](./Documentation~/glTFast.md#materials-and-shader-variants) for details.

### Advanced

The loading behavior can be highly customized:

- React to loading events by [adding event listeners](./Documentation~/glTFast.md#custom-loading-event-listeners)
- Customize [instantiation](./Documentation~/glTFast.md#instantiation)
- Load glTF once and instantiate it many times (see [example](./Documentation~/glTFast.md#custom-loading-event-listeners))
- Access data of glTF scene (for example get material; see [example](./Documentation~/glTFast.md#custom-loading-event-listeners))
- Tweak and optimize loading performance

See the [Documentation](./Documentation~/glTFast.md) for details.

## Roadmap

Find plans for upcoming changes at the [milestones](https://github.com/atteneder/glTFast/milestones).

## Motivation

### Goals

- Stay fast, memory efficient and small
- Become feature complete
  - Support 100% of the glTF 2.0 specification
  - Support all official Khronos extensions
  - Support selected vendor extension
- Universally usable…
  - …across all popular Unity versions
  - …across all platforms and devices
  - …across different project setups (all important render pipelines, GameObject or entity component system based, DOTS, Tiny, etc.)
- Allow customization

### Extended goals

- glTF Import (create prefab from glTF in the Editor)
- glTF Authoring (create optimized glTFs from prefabs)
- glTF Runtime Export

### Non-goals

- glTF 1.0 backwards compatibility

### Out of scope

Ideas worth pursuing, but not within this package:

- Asset lifetime management
- Download management with asset caching

## Get involved

Contributions like ideas, comments, critique, bug reports, pull requests are highly appreciated. Feel free to get in contact if you consider using or improving *glTFast*.

## Supporters

[Unity Technologies][unity]

Thanks to [Embibe][embibe] for sponsoring the development of skin support! ❤️

## License

Copyright (c) 2020 Andreas Atteneder, All Rights Reserved.

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

[unity]: https://unity.com
[gltf]: https://www.khronos.org/gltf
[gltfast-web-demo]: https://gltf.pixel.engineer
[khronos]: https://www.khronos.org
[embibe]: https://www.embibe.com
[gltfasset_component]: ./Documentation~/img/gltfasset_component.png  "Inspector showing a GltfAsset component added to a GameObject"
[upm_install]: ./Documentation~/img/upm_install.png  "Unity Package Manager add menu"
