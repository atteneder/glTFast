# Runtime Loading

You can load a glTF&trade; asset from an URL or a file path.

> Note: glTFs are loaded via [UnityWebRequests](https://docs.unity3d.com/ScriptReference/Networking.UnityWebRequest.html). File paths have to be prefixed with `file://` in the Unity Editor and on certain platforms (e.g. iOS).

## Runtime Loading via Component

Add a `GltfAsset` component to a GameObject. It offers a lot of settings for import and instantiation.

![GltfAsset component][gltfasset_component]

## Runtime Loading via Script

```C#
var gltf = gameObject.AddComponent<GLTFast.GltfAsset>();
gltf.Url = "https://raw.githubusercontent.com/KhronosGroup/glTF-Sample-Models/master/2.0/Duck/glTF/Duck.gltf";
```

### Load from byte array

In case you want to handle download/file loading yourself, you can load glTF binary files directly from C# byte[] like so:

```csharp
async void LoadGltfBinaryFromMemory() {
    var filePath = "/path/to/file.glb";
    byte[] data = File.ReadAllBytes(filePath);
    var gltf = new GltfImport();
    bool success = await gltf.LoadGltfBinary(
        data,
        // The URI of the original data is important for resolving relative URIs within the glTF
        new Uri(filePath)
        );
    if (success) {
        success = await gltf.InstantiateMainSceneAsync(transform);
    }
}
```

> Note: Most users want to load self-contained glTF binary files this way, but `LoadGltfBinary` also takes the original URI of glTF file as second parameter, so it can resolve relative URIs.

## Customize loading behavior

Loading via script allows you to:

- Custom download or file loading behavior (see [`IDownloadProvider`][IDownload])
- Customize loading behavior (like texture settings) via [`ImportSettings`](#import-settings)
- Custom material generation (see [`IMaterialGenerator`][IMaterialGenerator]])
- Customize [instantiation](#instantiation)
- Load glTF once and instantiate its scenes many times (see example [below](#custom-post-loading-behavior))
- Access data of glTF scene (for example get material; see example [below](#custom-post-loading-behavior))
- [Logging](#logging) allow reacting and communicating incidents during loading and instantiation
- Tweak and optimize loading performance

### Import Settings

`GltfImport.Load` accepts an optional instance of [`ImportSettings`][ImportSettings] as parameter. Have a look at this class to see all options available. Here's an example usage:

```C#
async void Start()
{
    var gltf = new GLTFast.GltfImport();

    // Create a settings object and configure it accordingly
    var settings = new ImportSettings {
        GenerateMipMaps = true,
        AnisotropicFilterLevel = 3,
        NodeNameMethod = NameImportMethod.OriginalUnique
    };
    // Load the glTF and pass along the settings
    var success = await gltf.Load("file:///path/to/file.gltf", settings);

    if (success) {
        var gameObject = new GameObject("glTF");
        await gltf.InstantiateMainSceneAsync(gameObject.transform);
    }
    else {
        Debug.LogError("Loading glTF failed!");
    }
}
```

### Custom Post-Loading Behavior

The async `Load` method can be awaited and followed up by custom behavior.

```C#
async void Start() {
    // First step: load glTF
    var gltf = new GLTFast.GltfImport();
    var success = await gltf.Load("file:///path/to/file.gltf");

    if (success) {
        // Here you can customize the post-loading behavior

        // Get the first material
        var material = gltf.GetMaterial();
        Debug.LogFormat("The first material is called {0}", material.name);

        // Instantiate the glTF's main scene
        await gltf.InstantiateMainSceneAsync( new GameObject("Instance 1").transform );
        // Instantiate the glTF's main scene
        await gltf.InstantiateMainSceneAsync( new GameObject("Instance 2").transform );

        // Instantiate each of the glTF's scenes
        for (int sceneId = 0; sceneId < gltf.SceneCount; sceneId++) {
            await gltf.InstantiateSceneAsync(transform, sceneId);
        }
    } else {
        Debug.LogError("Loading glTF failed!");
    }
}
```

### Instantiation

Creating actual GameObjects (or Entities) from the imported data (nodes, meshes, materials) is called instantiation.

You can customize it by providing an implementation of [`IInstantiator`][IInstantiator] (see [source][IInstantiator] and the reference implementation [`GameObjectInstantiator`][GameObjectInstantiator] for details).

Inject your custom instantiation like so

```csharp
public class YourCustomInstantiator : GLTFast.IInstantiator {
  // Your code here
}
…

  // In your custom post-loading script, use it like this
  bool success = await gltfAsset.InstantiateMainSceneAsync( new YourCustomInstantiator() );
```

#### GameObjectInstantiator Setup

The [`GameObjectInstantiator`][GameObjectInstantiator] accepts [InstantiationSettings][InstantiationSettings]) via the constructor's `settings` parameter.

##### `SkinUpdateWhenOffscreen`

Meshes that are skinned or have morph targets and are animated might move way outside their initial bounding box and thus break the culling. To prevent this the `SkinnedMeshRenderer`'s *Update When Offscreen* property is enabled by default. This comes at a runtime performance cost (see [Determining a GameObject’s visibility](https://docs.unity3d.com/2021.2/Documentation/Manual/class-SkinnedMeshRenderer.html) from the documentation).

You can disable this by setting `SkinUpdateWhenOffscreen` to false.

##### `Layer`

Instantiated `GameObject`s will be assigned to this [layer](https://docs.unity3d.com/Manual/Layers.html).

##### `Mask`

Allows you to filter components based on types (e.g. Meshes, Animation, Cameras or Lights).

##### `LightIntensityFactor`

Whenever glTF lights appear too bright or dim, you can use this setting to adjust their intensity, which are multiplied by this factor.

Two common use-cases are

1. Scale-down (physically correct) intensities to compensate for the missing exposure control (or high sensitivity) of a render pipeline (e.g. Universal or Built-in Render Pipeline)
2. Boost implausibly low light intensities

See [Physical Light Units in glTF](./LightUnits.md) for a detailed explanation.

##### `SceneObjectCreation`

Determines whether a dedicated GameObject/Entity representing the scene should get created (or the provided root `Transform` is used as scene root; see [SceneObjectCreation][SceneObjectCreation]).

- `Always`: Create a dedicated scene root GameObject/Entity
- `Never`: Always use the provided `Transform` as scene root.
- `WhenMultipleRootNodes`: Create a scene object only if there is more than one root level node.

#### Instance Access

After a glTF scene was instanced, you can access selected components for further adjustments. Some of those are:

- Animation
- Cameras
- Lights

[`GameObjectInstantiator`][GameObjectInstantiator] provides a [`SceneInstance`][GameObjectSceneInstance] for that purpose. Here's some code that demonstrates how to access it

```csharp
async void Start()
{

    var gltfImport = new GltfImport();
    await gltfImport.Load("test.gltf");
    var instantiator = new GameObjectInstantiator(gltfImport,transform);
    var success = await gltfImport.InstantiateMainSceneAsync(instantiator);
    if (success) {

        // Get the SceneInstance to access the instance's properties
        var sceneInstance = instantiator.SceneInstance;

        // Enable the first imported camera (which are disabled by default)
        if (sceneInstance.Cameras is { Count: > 0 }) {
            sceneInstance.Cameras[0].enabled = true;
        }

        // Decrease lights' ranges
        if (sceneInstance.Lights != null) {
            foreach (var glTFLight in sceneInstance.Lights) {
                glTFLight.range *= 0.1f;
            }
        }

        // Play the default (i.e. the first) animation clip
        var legacyAnimation = instantiator.SceneInstance.LegacyAnimation;
        if (legacyAnimation != null) {
            legacyAnimation.Play();
        }
    }
}
```

### Logging

When loading a glTF file, *Unity glTFast* logs messages of varying severity (errors, warnings or infos). Developers can choose what to make of those log messages. Examples:

- Log to console in readable form
- Feed the information into an analytics framework
- Display details to the users

The provided component `GltfAsset` logs all of those messages to the console by default.

You can customize logging by providing an implementation of [`ICodeLogger`][ICodeLogger] to methods like `GltfImport.Load` or `GltfImport.InstantiateMainScene`.

There are two common implementations bundled. The `ConsoleLogger`, which logs straight to console (the default) and `CollectingLogger`, which stores messages in a list for users to process.

Look into [`ICodeLogger`][ICodeLogger] and [`LogMessages`][LogMessages] for details.

### Tune loading performance

When loading glTFs, *Unity glTFast* let's you optimize towards one of two diametrical goals

- A stable frame rate
- Fastest loading time

By default each `GltfAsset` instance tries not to block the main thread for longer than a certain time budget and defer the remaining loading process to the next frame / game loop iteration.

If you load many glTF files at once, by default they won't be aware of each other and collectively might block the main game loop for too long.

You can solve this by using a common "defer agent". It decides if work should continue right now or at the next game loop iteration. *Unity glTFast* comes with two defer agents

- `TimeBudgetPerFrameDeferAgent` for stable frame rate
- `UninterruptedDeferAgent` for fastest, uninterrupted loading

The recommended way is to set a global default defer agent. The easiest way to do this is to add the prefab `Runtime/Prefabs/glTF-StableFramerate.prefab` to your entrance scene. You can change the `FrameBudget` value of its `TimeBudgetPerFrameDeferAgent` component to tweak performance to your needs. An alternative for fastest loading is the prefab in `Runtime/Prefabs/glTF-FastestLoading.prefab`.

You can accomplish the same from script by calling `GltfImport.SetDefaultDeferAgent` (and `UnsetDefaultDeferAgent`, respectively).

For most granular control, you can pass a custom defer agent to each individual `GltfImport` instance:

```C#
async Task CustomDeferAgentPerGltfImport() {
    // Recommended: Use a common defer agent across multiple GltfImport instances!
    // For a stable frame rate:
    IDeferAgent deferAgent = gameObject.AddComponent<TimeBudgetPerFrameDeferAgent>();
    // Or for faster loading:
    deferAgent = new UninterruptedDeferAgent();

    var tasks = new List<Task>();

    foreach( var url in manyUrls) {
        var gltf = new GLTFast.GltfImport(null,deferAgent);
        var task = gltf.Load(url).ContinueWith(
            async t => {
                if (t.Result) {
                    await gltf.InstantiateMainSceneAsync(transform);
                }
            },
            TaskScheduler.FromCurrentSynchronizationContext()
            );
        tasks.Add(task);
    }

    await Task.WhenAll(tasks);
}
```

> Note 1: Depending on your glTF scene, using the `UninterruptedDeferAgent` may block the main thread for up to multiple seconds. Be sure to not do this during critical game play action.
>
> Note2 : Using the `TimeBudgetPerFrameDeferAgent` does **not** guarantee a stutter free frame rate. This is because some sub tasks of the loading routine (like uploading a texture to the GPU) may take too long, cannot be interrupted and **have** to be done on the main thread.

### Disposing Resources

When you no longer need a loaded instance of a glTF scene you might want to remove it and free up all its resources (mainly memory). For that purpose [`GltfImport`][GltfImport] implements `IDisposable`. Calling [`GltfImport.Dispose`][GltfImportDispose] will destroy all its resources, regardless whether there's still an instance that might references them.

## Trademarks

*Unity&reg;* is a registered trademark of [Unity Technologies][Unity].

*Khronos&reg;* is a registered trademark and *glTF&trade;* is a trademark of [The Khronos Group Inc][Khronos].

[GltfImport]: xref:GLTFast.GltfImport
[GltfImportDispose]: xref:GLTFast.GltfImport.Dispose
[GameObjectInstantiator]: xref:GLTFast.GameObjectInstantiator
[gltfasset_component]: Images/gltfasset_component.png  "Inspector showing a GltfAsset component added to a GameObject"
[ICodeLogger]: xref:GLTFast.Logging.ICodeLogger
[IDownload]: xref:GLTFast.Loading.IDownload
[IInstantiator]: xref:GLTFast.IInstantiator
[IMaterialGenerator]: xref:GLTFast.Materials.IMaterialGenerator
[ImportSettings]: xref:GLTFast.ImportSettings
[InstantiationSettings]: xref:GLTFast.InstantiationSettings
[Khronos]: https://www.khronos.org
[LogMessages]: xref:GLTFast.Logging.LogMessages
[GameObjectSceneInstance]: xref:GLTFast.GameObjectSceneInstance
[SceneObjectCreation]: xref:GLTFast.SceneObjectCreation
[Unity]: https://unity.com
