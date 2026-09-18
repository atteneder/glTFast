# Installation

## Install the *Unity glTFast* package using the Unity Package Manager

> [!TIP]
> Click the following link to fast-track the installation and skip to step 5: [com.unity.cloud.gltfast](com.unity3d.kharma:upmpackage/com.unity.cloud.gltfast)

To install the *Unity glTFast* package, follow these steps:

1. With your project opened in the Unity Editor, open the [Package Manager window](xref:um-upm-ui-access).
2. Click the install button (➕) in the toolbar. The options for installing packages appear.
3. Select *Install package by technical name…* from the install menu (*Install package by name…* on older versions). Two text boxes and an *Install* button appear.
4. Enter `com.unity.cloud.gltfast` into the *Name* field.
5. Click *Install*.

The Editor installs the latest available version of the package and any dependent packages.

## Optional Packages

There are some related package that improve *Unity glTFast* by extending its feature set.

- [Built-in Packages](https://docs.unity3d.com/Manual/pack-build.html)
  - [Image Conversion](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/UnityEngine.ImageConversionModule.html) for import/export of Jpeg and PNG textures.
  - [Unity Web Request Texture](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/UnityEngine.UnityWebRequestTextureModule.html) for import of Jpeg/PNG images from URI.
  - [Animation](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/UnityEngine.AnimationModule.html) for animation playback.
  - [Physics](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/UnityEngine.PhysicsModule.html) to create a glTF scene's [bounding box collider](xref:UnityEngine.BoxCollider) via [GltfBoundsAsset](xref:Unity.Cloud.Gltfast.GltfBoundsAsset).
- [Draco&trade; for Unity][DracoForUnity] (provides support for [KHR_draco_mesh_compression][ExtDraco])
- [KTX&trade; for Unity][KtxUnity] (provides support for [KHR_texture_basisu][ExtBasisU])
- [*meshoptimizer decompression for Unity*][Meshopt] (provides support for [EXT_meshopt_compression][ExtMeshopt])

## Project Setup

With the package(s) installed you're ready to get started, but have a look at [Project Setup](ProjectSetup.md) to certify your builds work as expected as well and tweak the package's behavior overall.

## Install from Source Repository

If you intend to modify or develop *glTFast* you need to install it from the source repository. See the [development documentation](development.md) for instructions on how the [get the sources](sources.md) and [setup your project](test-project-setup.md#setup-a-custom-project).

## Trademarks

*Unity&reg;* is a registered trademark of [Unity Technologies][unity].

*Khronos&reg;* is a registered trademark and [glTF&trade;][gltf] is a trademark of [The Khronos Group Inc][khronos].

*KTX&trade;* and the KTX logo are trademarks of the [The Khronos Group Inc][khronos].

*Draco&trade;* is a trademark of [*Google LLC*][GoogleLLC].

[DracoForUnity]: https://docs.unity3d.com/Packages/com.unity.cloud.draco@latest
[ExtBasisU]: https://github.com/KhronosGroup/glTF/tree/master/extensions/2.0/Khronos/KHR_texture_basisu
[ExtDraco]: https://github.com/KhronosGroup/glTF/tree/master/extensions/2.0/Khronos/KHR_draco_mesh_compression
[ExtMeshopt]: https://github.com/KhronosGroup/glTF/tree/main/extensions/2.0/Vendor/EXT_meshopt_compression
[gltf]: https://www.khronos.org/gltf
[GoogleLLC]: https://about.google/
[khronos]: https://www.khronos.org
[KtxUnity]: https://docs.unity3d.com/Packages/com.unity.cloud.ktx@latest/
[Meshopt]: https://docs.unity3d.com/Packages/com.unity.meshopt.decompress@latest/
[unity]: https://unity.com
