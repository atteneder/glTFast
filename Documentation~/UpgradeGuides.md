# Upgrade Guides

## Upgrade to 4.5

New shader graphs are used with certain Universal and High Definition render pipeline versions, so projects that included *glTFast*'s shaders have to check and update their included shaders or shader variant collections (see [Materials and Shader Variants](ProjectSetup.md#materials-and-shader-variants) for details).

## Upgrade to 4.x

### Coordinate system conversion change

When upgrading from an older version to 4.x or newer the most notable difference is the imported models' orentation. They will appear 180° rotated around the up-axis (Y).

![GltfAsset component][gltfast3to4]

To counter-act this in applications that used older versions of *glTFast* before, make sure you rotate the parent `Transform` by 180° around the Y-axis, which brings the model back to where it should be.

This change was implemented to conform more closely to the [glTF specification][gltf-spec-coords], which says:

> The front of a glTF asset faces +Z.

In Unity, the positive Z axis is also defined as forward, so it makes sense to align those and so the coordinate space conversion from glTF's right-handed to Unity's left-handed system is performed by inverting the X-axis (before the Z-axis was inverted).

### New Logging

During loading and instantiation, glTFast used to log messages (infos, warnings and errors) directly to Unity's console. The new logging solution allows you to:

- Omit glTFast logging completely to avoid clogging the message log
- Retrieve the logs to process them (e.g. reporting analytics or inform the user properly)

See [Logging](ImportRuntime.md#logging) above.

### Scene based instantiation

glTFast 4.0 introduces scene-based instantiation. While most glTF assets contain only one scene they could consist of multiple scenes and optionally have one of declared the default scene.

The old behaviour was, that all of the glTF's content was loaded. The new interface allows you to load the default scene or any scene of choice. If none of the scenes was declared the default scene (by setting the `scene` property), no objects are instantiated (as defined in the glTF specification).

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

## Legacy documentation

Users of glTFast 1.x can read [the documentation for it](./gltfast-1.md).

[gltf-spec-coords]: https://www.khronos.org/registry/glTF/specs/2.0/glTF-2.0.html#coordinate-system-and-units
[GltfAsset]: xref:GLTFast.GltfAsset
[gltfast3to4]: Images/gltfast3to4.png  "3D scene view showing BoomBoxWithAxes model twice. One with the legacy axis conversion and one with the new orientation"
[GltfImport]: xref:GLTFast.GltfImport
[IGltfReadable]: xref:GLTFast.IGltfReadable
