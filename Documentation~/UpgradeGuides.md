---
uid: doc-upgrade-guides
---

# Upgrade Guides

These guides will help you upgrade your project to use the latest version of *Unity glTFast*. If you still encounter problems, help us improving this guide and *Unity glTFast* in general by reaching out by raising an issue.

## Unity Fork

With the release of version 5.2.0 the package name and identifier were changed to *Unity glTFast* (`com.unity.cloud.gltfast`) for the following reasons:

- Better integration into Unity internal development processes (including quality assurance and support)
- Distribution via the Unity Package Manager (no scoped libraries required anymore)

For now, both the Unity variant and the original version will receive updates.

### Transition to *Unity glTFast*

The C# namespaces are identical between the variants, so all you need to do is:

- Removed original *glTFast* (with package identifier `com.atteneder.gltfast`).
- [Install *Unity glTFast*](installation.html) (`com.unity.cloud.gltfast`).
- Update assembly definition references (if your project had any).
- Update any dependencies in your packages manifest (if your package had any)

#### Transition Depending Packages

Unity forks have been created for *KtxUnity* and *DracoUnity* as well. If you've used them in conjunction with *glTFast*, you need to transition them to the Unity variants as well.

See their respective upgrade guides

- Upgrade to [*KTX for Unity*](https://docs.unity3d.com/Packages/com.unity.cloud.ktx@3.2/manual/upgrade-guide.html)
- Upgrade to [*Draco for Unity*](https://docs.unity3d.com/Packages/com.unity.cloud.draco@5.0/manual/upgrade-guide.html)

### Keep using the original glTFast

The original *glTFast* (`com.atteneder.gltfast`) as well as *KtxUnity* and *DracoUnity* will still receive identical updates for now. You may choose to continue using them.

If you've installed the packages via the installer script (i.e. via [OpenUPM][OpenUPM] scoped registry - the recommended way), you don't need to change anything. You'll receive updates as usual.

If you've cloned the package via GIT, make sure to switch to the `openupm` branch to make sure the package identifier and name remain the original.

See [Original *glTFast*](./Original.md) for instructions to install the original version from scratch.

## Upgrade to 5.0

### General

The API in general was changed considerably to conform closer to Unity's coding standard and the Microsoft's Framework Design Guidelines. If you have custom code, you likely need to update parts of it to the new API. Some notable items:

- PascalCase on properties (first char is upper-case)
- Removed direct access to fields (replaced by getter-property, where required)
- More consistent naming of assemblies, namespaces, classes, constants, static members, etc.
  - Renamed and moved classes/structs to different files.
- Auto-formatted code for consistent line-endings and code look (a necessary, one-time evil; might be troublesome if you forked *Unity glTFast*)

If you have issues, please also go through the 5.0.0 changelog entry and feel free to reach out for support.

### Moved or Renamed Types

Some assemblies, classes, structs and enum types have been renamed or moved. Make sure you adopt your code appropriately. All entries are within the `GLTFast` namespace.

- Refactored Assembly Definitions
  - `glTFastSchema` was merged into `glTFast` and thus removed
  - `glTFastEditor` was renamed to `glTFast.Editor`
  - `glTFastEditorTests` was renamed to `glTFast.Editor.Tests`
- Moved logging related code into `GLTFast.Logging` namespace
- Replaced `CollectingLogger.item` with `.Count` and `.Items` iterator
- `GameObjectInstantiator.SceneInstance` is now `GameObjectSceneInstance`
- `ImportSettings.NameImportMethod` is now `NameImportMethod`
- Converted  `GameObjectInstantiator.Settings` to `InstantiationSettings`
- `InstantiationSettings.SceneObjectCreation` is now `SceneObjectCreation`
- Converted properties that were hiding conversion logic or caching into methods
  - `Accessor`: `typeEnum` to `GetAttributeType`/`SetAttributeType`
  - `BufferView`: `modeEnum` to `GetMode`
  - `BufferView`: `filterEnum` to `GetFilter`
  - `AnimationChannelTarget`: `pathEnum` to `GetPath`
  - `AnimationSampler`: `interpolationEnum` to `GetInterpolationType`
  - `Camera`: `typeEnum` to `GetCameraType`/`SetCameraType`
  - `LightPunctual`: `typeEnum` to `GetLightType`/`SetLightType`
  - `Material`: `alphaModeEnum` to `GetAlphaMode`/`SetAlphaMode`
- `HttpHeader`'s properties are readonly now. A constructor was added as compensation.
- Obsolete code that was finally removed
  - `GltfImport.Destroy` (was renamed to `GltfImport.Dispose`)
  - `GLTFast.GltFast` (was renamed to `GltfImport`)
  - `GltfImport.InstantiateGltf` (was replaced by `InstantiateMainScene` and `InstantiateScene`)

### Async Scene Instantiation

The addition of `GltfImport.InstantiateSceneAsync` and `GltfImport.InstantiateMainSceneAsync` now provides an asynchronous way of instantiating glTF&trade; scenes. For large scenes this means that the instantiation can be spread over multiple frames, resulting in a smoother frame rate.

The existing, synchronous instantiation methods `GltfImport.InstantiateScene` and `GltfImport.InstantiateMainScene` (including overloads) have been marked obsolete and will be removed eventually. Though they still work, it's recommended to update your code to use the async variants.

Since loading a glTF (the step before instantiation) has been async before, chances are high your enclosing method is already async, as it should be.

```csharp
async void Start() {
    var gltf = new GltfImport();
    var success = await gltf.Load("file:///path/to/file.gltf");
    if(!success) return;

    // Old, sync instantiation
    success = gltf.InstantiateMainScene(transform);
    if(success) Debug.Log("glTF instantiated successfully!");
}
```

All you now have to do is switch to the async method and await it.

```csharp
async void Start() {
    var gltf = new GltfImport();
    var success = await gltf.Load("file:///path/to/file.gltf");
    if(!success) return;

    // New, async instantiation
    success = await gltf.InstantiateMainSceneAsync(transform);
    if(success) Debug.Log("glTF instantiated successfully!");
}
```

### `IInstantiator` Changes

`IInstantiator.BeginScene` signature dropped third parameter `AnimationClip[] animationClips`. As replacement `IInstantiator.AddAnimation` was added. It's only available when built-in Animation module is enabled.

### Texture Support

The built-in packages [*Unity Web Request Texture*][uwrt] and [*Image Conversion*][ImgConv] provide support for PNG/Jpeg texture import and export. They are not a hard requirement anymore, so you…

- …**can** disable them if you don't require PNG/Jpeg texture support
- …**need to** enable them in the Package Manager if you require PNG/Jpeg texture support

See [*Texture Support* in Project Setup](ProjectSetup.md#materials-and-shader-variants) for details.

### Play Animation

Previously the first animation clip would start playing by default, which is not the case anymore. There is a way to restore animation auto-play, depending on how you load glTFs.

#### Play Automatically with the `GltfAsset` component

There's a new property `Play Automatically`, which is checked by default. You shouldn't experience change in behavior, unless you disable this setting.

#### Play Automatically when loading from script

You have to explicitly use a [`GameObjectInstantiator`][GameObjectInstantiator]. It provides a [`SceneInstance`][GameObjectSceneInstance] object which has a `legacyAnimation` property, referencing the `Animation` component. Use it to start or stop playback of any of the animation clips it holds.

```csharp
async void Start() {

    var gltfImport = new GltfImport();
    await gltfImport.Load("test.gltf");
    var instantiator = new GameObjectInstantiator(gltfImport,transform);
    var success = gltfImport.InstantiateMainScene(instantiator);
    if (success) {

        // Get the SceneInstance to access the instance's properties
        var sceneInstance = instantiator.SceneInstance;

        // Play the default (i.e. the first) animation clip
        var legacyAnimation = instantiator.SceneInstance.LegacyAnimation;
        if (legacyAnimation != null) {
            legacyAnimation.Play();
        }
    }
}
```

### `IMaterialGenerator` API change

Rendering meshes with points topology/draw mode (Point clouds) requires special shaders (with a `PSIZE` vertex output). For that reason the `pointsSupport` parameter (`bool`; optional) was added to

- `IMaterialGenerator.GetDefaultMaterial`
- `IMaterialGenerator.GenerateMaterial`

If `pointsSupport` is true, the generated material has to support meshes with points topology.

The bundled default material generators don't support point cloud rendering yet (with the exception of the built-in unlit shader), but this change will allow implementing that in the future (or in custom implementations).

If a material is used on mesh primitives with different draw modes (e.g. on triangles as well as points), still just one Unity material with points support will be created and used for all of them.

### Misc. API Changes

`RenderPipelineUtils.DetectRenderPipeline()` turned to `RenderPipelineUtils.RenderPipeline`

## Upgrade to 4.5

New shader graphs are used with certain Universal and High Definition render pipeline versions, so projects that included *Unity glTFast*'s shaders have to check and update their included shaders or shader variant collections (see [Materials and Shader Variants](ProjectSetup.md#materials-and-shader-variants) for details).

## Upgrade to 4.x

### Coordinate system conversion change

When upgrading from an older version to 4.x or newer the most notable difference is the imported models' orientation. They will appear 180° rotated around the up-axis (Y).

![GltfAsset component][gltfast3to4]

To counter-act this in applications that used older versions of *Unity glTFast* before, make sure you rotate the parent `Transform` by 180° around the Y-axis, which brings the model back to where it should be.

This change was implemented to conform more closely to the [glTF specification][gltf-spec-coords], which says:

> The front of a glTF asset faces +Z.

In Unity, the positive Z axis is also defined as forward, so it makes sense to align those and so the coordinate space conversion from glTF's right-handed to Unity's left-handed system is performed by inverting the X-axis (before the Z-axis was inverted).

### New Logging

During loading and instantiation, *Unity glTFast* used to log messages (infos, warnings and errors) directly to Unity's console. The new logging solution allows you to:

- Omit *Unity glTFast* logging completely to avoid clogging the message log
- Retrieve the logs to process them (e.g. reporting analytics or inform the user properly)

See [Logging](ImportRuntime.md#logging) above.

### Scene based instantiation

*Unity glTFast* 4.0 introduces scene-based instantiation. While most glTF assets contain only one scene they could consist of multiple scenes and optionally have one of declared the default scene.

The old behavior was, that all of the glTF's content was loaded. The new interface allows you to load the default scene or any scene of choice. If none of the scenes was declared the default scene (by setting the `scene` property), no objects are instantiated (as defined in the glTF specification).

[`GltfImport`][GltfImport] (formerly named `GLTFast`) provides the following properties and methods for scene instantiation:

```csharp
// To get the number of scenes
public int sceneCount;
// Returns the default scene's index
public int? defaultSceneIndex;
// Methods for instantiation
public bool InstantiateMainScene( Transform parent );
public bool InstantiateMainScene(IInstantiator instantiator);
public bool InstantiateScene( Transform parent, int sceneIndex = 0);
public bool InstantiateScene( IInstantiator instantiator, int sceneIndex = 0 );
```

Please look at [`GltfAsset`][GltfAsset] for a reference implementation and look at the properties'/methods' XML documentation comments in the source code for details.

### Custom material generation

Creating a custom `IMaterialGenerator` was mainly about implementing the following method:

```csharp
Material GenerateMaterial(Schema.Material gltfMaterial, ref Schema.Texture[] textures, ref Schema.Image[] schemaImages, ref Dictionary<int, Texture2D>[] imageVariants);
```

You'd receive all textures/images/image variants to pick from. This was changed to:

```csharp
Material GenerateMaterial(Schema.Material gltfMaterial, IGltfReadable gltf);
```

[`IGltfReadable`][IGltfReadable] is an interface that allows you to query all loaded textures and much more, allowing more flexible implementations. Please look at the source code.

In the future materials can be created before textures are available/downloaded to speed up the loading.

## Trademarks

*Unity&reg;* is a registered trademark of [Unity Technologies][unity].

*Khronos&reg;* is a registered trademark and *glTF&trade;* is a trademark of [The Khronos Group Inc][khronos].

[GameObjectInstantiator]: xref:GLTFast.GameObjectInstantiator
[GameObjectSceneInstance]: xref:GLTFast.GameObjectSceneInstance
[gltf-spec-coords]: https://www.khronos.org/registry/glTF/specs/2.0/glTF-2.0.html#coordinate-system-and-units
[GltfAsset]: xref:GLTFast.GltfAsset
[gltfast3to4]: Images/gltfast3to4.png  "3D scene view showing BoomBoxWithAxes model twice. One with the legacy axis conversion and one with the new orientation"
[GltfImport]: xref:GLTFast.GltfImport
[IGltfReadable]: xref:GLTFast.IGltfReadable
[ImgConv]: https://docs.unity3d.com/2021.3/Documentation/ScriptReference/UnityEngine.ImageConversionModule.html
[OpenUPM]: https://openupm.com/
[khronos]: https://www.khronos.org
[unity]: https://unity.com
[uwrt]: https://docs.unity3d.com/2021.3/Documentation/ScriptReference/UnityEngine.UnityWebRequestTextureModule.html
