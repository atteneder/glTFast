# Runtime Loading

You can load a glTF asset from an URL or a file path.

> Note: glTFs are loaded via [UnityWebRequests](https://docs.unity3d.com/ScriptReference/Networking.UnityWebRequest.html). File paths have to be prefixed with `file://` in the Unity Editor and on certain platforms (e.g. iOS).

## Runtime Loading via Component

Add a `GltfAsset` component to a GameObject.

![GltfAsset component][gltfasset_component]

## Runtime Loading via Script

```C#
var gltf = gameObject.AddComponent<GLTFast.GltfAsset>();
gltf.url = "https://raw.githubusercontent.com/KhronosGroup/glTF-Sample-Models/master/2.0/Duck/glTF/Duck.gltf";
```

### Load from byte array

In case you want to handle download/file loading yourself, you can load glTF binary files directly from C# byte[] like so:

```csharp
async void LoadGltfBinaryFromMemory() {
    byte[] data = File.ReadAllBytes("/path/to/file.glb");
    var gltf = new GltfImport();
    bool success = await gltf.LoadGltfBinary(data, new Uri(m_Path));
    if (success) {
        success = gltf.InstantiateMainScene(transform);
    }
}
```

> Note: Most users want to load self-contained glTF binary files this way, but `LoadGltfBinary` also takes the original URI of glTF file as second parameter, so it can resolve relative URIs.

## Customize loading behavior

Loading via script allows you to:

- Custom download or file loading behaviour (see [`IDownloadProvider`][IDownload])
- Customize loading behaviour (like texture settings) via [`ImportSettings`](#import-settings)
- Custom material generation (see [`IMaterialGenerator`][IMaterialGenerator]])
- Customize [instantiation](#instantiation)
- Load glTF once and instantiate its scenes many times (see example [below](#custom-post-loading-behaviour))
- Access data of glTF scene (for example get material; see example [below](#custom-post-loading-behaviour))
- [Logging](#logging) allow reacting and communicating incidents during loading and instantiation
- Tweak and optimize loading performance

### Import Settings

`GltfImport.Load` accepts an optional instance of [`ImportSettings`][ImportSettings] as parameter. Have a look at this class to see all options available. Here's an example usage:

```C#
async void Start() {
    var gltf = new GLTFast.GltfImport();

    // Create a settings object and configure it accordingly
    var settings = new ImportSettings {
        generateMipMaps = true,
        anisotropicFilterLevel = 3,
        nodeNameMethod = ImportSettings.NameImportMethod.OriginalUnique
    };
    
    // Load the glTF and pass along the settings
    var success = await gltf.Load("file:///path/to/file.gltf", settings);

    if (success) {
        gltf.InstantiateMainScene(new GameObject("glTF").transform);
    }
    else {
        Debug.LogError("Loading glTF failed!");
    }
}
```

### Custom Post-Loading Behaviour

The async `Load` method can be awaited and followed up by custom behaviour.

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
        gltf.InstantiateMainScene( new GameObject("Instance 1").transform );
        // Instantiate the glTF's main scene
        gltf.InstantiateMainScene( new GameObject("Instance 2").transform );

        // Instantiate each of the glTF's scenes
        for (int sceneId = 0; sceneId < gltf.sceneCount; sceneId++) {
            gltf.InstantiateScene(transform, sceneId);
        }
    } else {
        Debug.LogError("Loading glTF failed!");
    }
}
```

### Instantiation

Creating actual GameObjects (or Entities) from the imported data (Meshes, Materials) is called instantiation.

You can customize it by providing an implementation of [`IInstantiator`][IInstantiator] ( see [source][IInstantiator] and the reference implementation [`GameObjectInstantiator`][GameObjectInstantiator] for details).

Inject your custom instantiation like so

```csharp
public class YourCustomInstantiator : GLTFast.IInstantiator {
  // Your code here
}
…

  // In your custom post-loading script, use it like this
  gltfAsset.InstantiateMainScene( new YourCustomInstantiator() );
```

#### GameObjectInstantiator Setup

The [`GameObjectInstantiator`][GameObjectInstantiator] accepts settings (via the constructor's `settings` parameter).

##### `skinUpdateWhenOffscreen`

Meshes that are skinned or have morph targets and are animated might move way outside their initial bounding box and thus break the culling. To prevent this the `SkinnedMeshRenderer`'s *Update When Offscreen* property is enabled by default. This comes at a runtime performance cost (see [Determining a GameObject’s visibility](https://docs.unity3d.com/2021.2/Documentation/Manual/class-SkinnedMeshRenderer.html) from the documentation).

You can disable this by setting `skinUpdateWhenOffscreen` to false.

### Logging

When loading a glTF file, glTFast logs messages of varying severity (errors, warnigns or infos). Developers can choose what to make of those log messages. Examples:

- Log to console in readable form
- Feed the information into an analytics framework
- Display details to the users

The provided component `GltfAsset` logs all of those messages to the console by default.  

You can customize logging by providing an implementation of [`ICodeLogger`][ICodeLogger] to methods like `GltfImport.Load` or `GltfImport.InstanciateMainScene`.

There are two common implementations bundled. The `ConsoleLogger`, which logs straight to console (the default) and `CollectingLogger`, which stores messages in a list for users to process.

Look into [`ICodeLogger`][ICodeLogger] and [`LogMessages`][[LogMessages]] for details.

### Tune loading performance

When loading glTFs, *glTFast* let's you optimize for two diametrical extremes

- A stable frame rate
- Fastest loading time

By default each `GltfAsset` instance tries not to block the main thread for longer than a certain time budget and defer the remaining loading process to the next frame / game loop iteration.

If you load many glTF files at once, by default they won't be aware of each other and collectively might block the main game loop for too long.

You can solve this by using a common "defer agent". It decides if work should continue right now or at the next game loop iteration. *glTFast* comes with two defer agents

- `TimeBudgetPerFrameDeferAgent` for stable frame rate
- `UninterruptedDeferAgent` for fastest, uninterrupted loading

Usage example

```C#
async Task CustomDeferAgent() {
    // Recommended: Use a common defer agent across multiple GltfImport instances!
    // For a stable frame rate:
    IDeferAgent deferAgent = gameObject.AddComponent<TimeBudgetPerFrameDeferAgent>();
    // Or for faster loading:
    deferAgent = new UninterruptedDeferAgent();

    var tasks = new List<Task>();
    
    foreach( var url in manyUrls) {
        var gltf = new GLTFast.GltfImport(null,deferAgent);
        var task = gltf.Load(url).ContinueWith(
            t => {
                if (t.Result) {
                    gltf.InstantiateMainScene(transform);
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

> Note2 : Using the `TimeBudgetPerFrameDeferAgent` does **not** guarantee a stutter free frame rate. This is because some sub tasks of the loading routine (like uploading a texture to the GPU) may take too long, cannot be interrupted and **have** to be done on the main thread.

[GameObjectInstantiator]: xref:GLTFast.GameObjectInstantiator
[gltfasset_component]: Images/gltfasset_component.png  "Inspector showing a GltfAsset component added to a GameObject"
[ICodeLogger]: xref:GLTFast.ICodeLogger
[IDownload]: xref:GLTFast.Loading.IDownload
[IInstantiator]: xref:GLTFast.IInstantiator
[IMaterialGenerator]: xref:GLTFast.IMaterialGenerator
[ImportSettings]: xref:GLTFast.ImportSettings
[LogMessages]: xref:GLTFast.LogMessages
