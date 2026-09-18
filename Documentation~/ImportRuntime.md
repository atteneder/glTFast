# Runtime Loading

You can load a glTF&trade; asset from an URL or a file path.

> [!NOTE]
> By default glTFs are loaded via [UnityWebRequests](https://docs.unity3d.com/ScriptReference/Networking.UnityWebRequest.html). File paths thus have to be prefixed with `file://` in the Unity Editor and on certain platforms (e.g. iOS).

## Runtime Loading via Component

Add a [GltfAsset] component to a GameObject. It offers a lot of settings for import and instantiation.

![GltfAsset component][gltfasset_component]

## Runtime Loading via Script

Conveniently you can re-use the [GltfAsset] component to load from script:

[!code-cs [load-via-component](../Runtime/DocExamples/LoadGltfFromMemory.cs#LoadViaComponent)]

To load from sources other than a URI or for advanced [customization](#customize-loading-behavior), loading is performed with these generalized steps:

1. Create a [GltfImport] instance.
2. Call one of the instance's loading methods, depending on your source.
   - From URI, [LoadAsync(Uri,…)](xref:Unity.Cloud.Gltfast.GltfImportBase.LoadAsync(System.Uri,Unity.Cloud.Gltfast.ImportSettings,System.Threading.CancellationToken)) or [LoadAsync(string,…)](xref:Unity.Cloud.Gltfast.GltfImportBase.LoadAsync(System.String,Unity.Cloud.Gltfast.ImportSettings,System.Threading.CancellationToken))
   - From a buffer [LoadAsync(NativeArray&lt;byte&gt;.ReadOnly,…)](xref:Unity.Cloud.Gltfast.GltfImportBase.LoadAsync(Unity.Collections.NativeArray{System.Byte}.ReadOnly,System.Uri,Unity.Cloud.Gltfast.ImportSettings,System.Threading.CancellationToken))
   - From a managed buffer [LoadAsync(byte[],…)](xref:Unity.Cloud.Gltfast.GltfImportBase.LoadAsync(System.Byte[],System.Uri,Unity.Cloud.Gltfast.ImportSettings,System.Threading.CancellationToken))
   - From a file path [LoadFileAsync(string,…)](xref:Unity.Cloud.Gltfast.GltfImportBase.LoadFileAsync*)
   - From a glTF JSON string [LoadGltfJsonAsync(string,…)][GltfImportLoadGltfJson]
   - From a [Stream] [LoadStreamAsync(Stream,…)][GltfImportLoadStream]
3. Instantiate one ore more scenes however often you need.
   - The main scene [InstantiateMainSceneAsync](xref:Unity.Cloud.Gltfast.GltfImportBase.InstantiateMainSceneAsync*)
   - Or select one by index [InstantiateSceneAsync](xref:Unity.Cloud.Gltfast.GltfImportBase.InstantiateSceneAsync*)
4. Destroy your scene instances after they're no longer needed.
5. Call [Dispose](xref:Unity.Cloud.Gltfast.GltfImportBase.Dispose) on your [GltfImport] instance.

Both the loading and instantiation methods return a boolean value indicating if the procedure was successful.

> [!IMPORTANT]
> Loading/instantiation methods returning `true` merely indicates that no critical error occurred. That includes partially loaded scenes (e.g. a texture failed to load). To enforce stricter behavior one has to consider the log items in addition (see [Logging](#logging)).

### Example: Load from byte array

[!code-cs [load-gltf-from-memory](../Runtime/DocExamples/LoadGltfFromMemory.cs#LoadGltfFromMemory)]

> [!TIP]
> Provide the original URI of glTF-binary file as `uri` parameter to [LoadAsync][GltfImportLoad], so that it is able to resolve relative URIs in non-self-contained glTFs.

### Example: Load from NativeArray

> [!IMPORTANT]
> The buffer you pass to [LoadAsync][GltfImportLoad] is not copied. It has to stay allocated and unmodified until the returned `Task` completed, because glTFast reads from it throughout loading, potentially from worker threads. When loading from a [NativeArray&lt;byte&gt;.ReadOnly][NativeArrayReadOnly], you keep ownership of the underlying [NativeArray&lt;byte&gt;][NativeArray] and thus have to dispose of it *after* awaiting.

[!code-cs [load-gltf-from-native-array](../Runtime/DocExamples/LoadGltfFromMemory.cs#LoadGltfFromNativeArray)]

Disposing of the data earlier results in undefined behavior (a safety check exception in the Unity Editor, invalid reads otherwise). Do **not** do this:

[!code-cs [load-gltf-from-native-array-invalid](../Runtime/DocExamples/LoadGltfFromMemory.cs#LoadGltfFromNativeArrayInvalid)]

## Customize loading behavior

Loading via script allows you to:

- Custom download or file loading behavior (see [`IDownloadProvider`][IDownload])
- Customize loading behavior (like texture settings) via [`ImportSettings`](#import-settings)
- Custom material generation (see [`IMaterialGenerator`][IMaterialGenerator]])
- Customize [instantiation](#instantiation)
- Load glTF once and instantiate its scenes many times (see example [below](#custom-post-loading-behavior))
- Access data of glTF scene (for example get material; see example [below](#custom-post-loading-behavior))
- [Logging](#logging) allows reacting to and communicating incidents during loading and instantiation
- Tweak and optimize loading performance

### Import Settings

All [`GltfImport.LoadAsync`][GltfImportLoad] overloads accept an optional instance of [`ImportSettings`][ImportSettings] as parameter. Have a look at this class to see all options available. Here's an example usage:

[!code-cs [import-settings](../Runtime/DocExamples/LoadGltfFromMemory.cs#ImportSettings)]

### Custom Post-Loading Behavior

The async `LoadAsync` method can be awaited and followed up by custom behavior.

[!code-cs [instantiation](../Runtime/DocExamples/LoadGltfFromMemory.cs#Instantiation)]

### Reading Buffer Data

[`IGltfBufferData`][IGltfBufferData] provides read access to a glTF asset's buffer view and accessor data, for example to run your own analysis or feed a Burst job.

Buffer data comes with a lease: the import keeps its buffers alive until every lease is disposed. Buffer data only exists while an import is running, so the entry point is the [`IBufferDataConsumer`][IBufferDataConsumer] add-on hook, which is called once every buffer is loaded and before the import converts that data into Unity resources.

[!code-cs [buffer-data-addon](../Runtime/DocExamples/BufferDataAccess.cs#PositionSumAddon)]

Inject the add-on before loading:

[!code-cs [read-buffer-data](../Runtime/DocExamples/BufferDataAccess.cs#ReadBufferDataDuringImport)]

The lease handed to the hook is disposed as soon as the hook returns. To keep reading after the import finished, lease your own via [`GltfImport.LeaseBufferData`][LeaseBufferData] and dispose it when done:

[!code-cs [retain-buffer-data](../Runtime/DocExamples/BufferDataAccess.cs#RetainBufferDataBeyondImport)]

Data is provided in glTF's own coordinate system and value range. No conversion, normalization or coordinate flip is applied — use `ComponentType`, `Type` and `Normalized` on the [`Accessor`][Accessor] to interpret it. Sparse accessors are not provided.

Use it from the main thread. The containers it provides may be read from C# jobs and from threads of your own, for as long as the lease has not been disposed. The hook is called on the main thread and has to return on it, so schedule jobs or start threads in between when the work is heavy.

Add-ons implementing the hook are invoked in unspecified order and potentially concurrently, so do not assume yours is the only one running, or that it runs before or after another. The glTF asset is read-only there: allocate and create resources of your own freely, but do not modify the [`Root`][Root], any glTF object or any buffer data.

[IGltfBufferData]: xref:Unity.Cloud.Gltfast.IGltfBufferData
[IBufferDataConsumer]: xref:Unity.Cloud.Gltfast.Addons.IBufferDataConsumer
[LeaseBufferData]: xref:Unity.Cloud.Gltfast.GltfImport.LeaseBufferData*
[Root]: xref:Unity.Cloud.Gltfast.Objects.Root
[Accessor]: xref:Unity.Cloud.Gltfast.Objects.Accessor

### Instantiation

Creating actual GameObjects (or Entities) from the imported data (nodes, meshes, materials) is called instantiation.

You can customize it by providing an implementation of [`IInstantiator`][IInstantiator] (see [source][IInstantiator] and the reference implementation [`GameObjectInstantiator`][GameObjectInstantiator] for details).

Inject your custom instantiation like so

```csharp
public class YourCustomInstantiator : Unity.Cloud.Gltfast.IInstantiator {
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

[!code-cs [SceneInstanceAccess](../Runtime/DocExamples/LoadGltfFromMemory.cs#SceneInstanceAccess)]

### Logging

When loading a glTF file, *Unity glTFast* logs messages of varying severity (errors, warnings or infos). Developers can choose what to make of those log messages. Examples:

- Log to console in readable form
- React to non-critical errors (like an image texture failed to load) in a nuanced way
- Feed the information into an analytics framework
- Display details to the users

The [GltfAsset] component logs all of those messages to the console by default.

You can customize logging by providing an implementation of [ICodeLogger][ICodeLogger] to the constructors of [GltfImport] or [GameObjectInstantiator].

> [!IMPORTANT]
> Not providing an `ICodeLogger` will disable logging altogether, which makes finding the cause of problems hard! Always use a logger like the `ConsoleLogger` during development.

There are two common implementations bundled. The [ConsoleLogger][ConsoleLogger], which logs straight to console and [CollectingLogger][CollectingLogger], which stores messages in a list for users to process.

Look into [ICodeLogger][ICodeLogger] and [LogMessages][LogMessages] for details.

### Tune loading performance

When loading glTFs, *Unity glTFast* let's you optimize towards one of two diametrical goals

- A stable frame rate
- Fastest loading time

By default each [GltfAsset] instance tries not to block the main thread for longer than a certain time budget and defer the remaining loading process to the next frame / game loop iteration.

If you load many glTF files at once, by default they won't be aware of each other and collectively might block the main game loop for too long.

You can solve this by using a common "defer agent". It decides if work should continue right now or at the next game loop iteration. *Unity glTFast* comes with two defer agents

- `TimeBudgetPerFrameDeferAgent` for stable frame rate
- `UninterruptedDeferAgent` for fastest, uninterrupted loading

The recommended way is to set a global default defer agent. The easiest way to do this is to add the prefab `Runtime/Prefabs/glTF-StableFramerate.prefab` to your entrance scene. You can change the `FrameBudget` value of its `TimeBudgetPerFrameDeferAgent` component to tweak performance to your needs. An alternative for fastest loading is the prefab in `Runtime/Prefabs/glTF-FastestLoading.prefab`.

You can accomplish the same from script by calling `GltfImport.SetDefaultDeferAgent` (and `UnsetDefaultDeferAgent`, respectively).

For most granular control, you can pass a custom defer agent to each individual `GltfImport` instance:

[!code-cs [CustomDeferAgent](../Runtime/DocExamples/LoadGltfFromMemory.cs#CustomDeferAgent)]

> [!NOTE]
> Depending on your glTF scene, using the `UninterruptedDeferAgent` may block the main thread for up to multiple seconds. Be sure to not do this during critical game play action.
>
> Using the `TimeBudgetPerFrameDeferAgent` does **not** guarantee a stutter free frame rate. This is because some sub tasks of the loading routine (like uploading a texture to the GPU) may take too long, cannot be interrupted and **have** to be done on the main thread.

### Disposing Resources

When you no longer need a loaded instance of a glTF scene you might want to remove it and free up all its resources (mainly memory). For that purpose [`GltfImport`][GltfImport] implements `IDisposable`. Calling [`GltfImport.Dispose`][GltfImportDispose] will destroy all its resources, regardless whether there's still an instance that might references them.

## Trademarks

*Unity&reg;* is a registered trademark of [Unity Technologies][Unity].

*Khronos&reg;* is a registered trademark and *glTF&trade;* is a trademark of [The Khronos Group Inc][Khronos].

[CollectingLogger]: xref:Unity.Cloud.Gltfast.Logging.CollectingLogger
[ConsoleLogger]: xref:Unity.Cloud.Gltfast.Logging.ConsoleLogger
[GltfAsset]: xref:Unity.Cloud.Gltfast.GltfAsset
[GltfImport]: xref:Unity.Cloud.Gltfast.GltfImport
[GltfImportDispose]: xref:Unity.Cloud.Gltfast.GltfImportBase.Dispose
[GltfImportLoad]: xref:Unity.Cloud.Gltfast.GltfImportBase.LoadAsync*
[GltfImportLoadGltfJson]: xref:Unity.Cloud.Gltfast.GltfImportBase.LoadGltfJsonAsync*
[GltfImportLoadStream]: xref:Unity.Cloud.Gltfast.GltfImportBase.LoadStreamAsync*
[GameObjectInstantiator]: xref:Unity.Cloud.Gltfast.GameObjectInstantiator
[gltfasset_component]: Images/gltfasset_component.png  "Inspector showing a GltfAsset component added to a GameObject"
[ICodeLogger]: xref:Unity.Cloud.Gltfast.Logging.ICodeLogger
[IDownload]: xref:Unity.Cloud.Gltfast.Loading.IDownload
[IInstantiator]: xref:Unity.Cloud.Gltfast.IInstantiator
[IMaterialGenerator]: xref:Unity.Cloud.Gltfast.Materials.IMaterialGenerator
[ImportSettings]: xref:Unity.Cloud.Gltfast.ImportSettings
[InstantiationSettings]: xref:Unity.Cloud.Gltfast.InstantiationSettings
[Khronos]: https://www.khronos.org
[LogMessages]: xref:Unity.Cloud.Gltfast.Logging.LogMessages
[NativeArray]: xref:Unity.Collections.NativeArray`1
[NativeArrayReadOnly]: xref:Unity.Collections.NativeArray`1.ReadOnly
[GameObjectSceneInstance]: xref:Unity.Cloud.Gltfast.GameObjectSceneInstance
[SceneObjectCreation]: xref:Unity.Cloud.Gltfast.SceneObjectCreation
[Stream]: xref:System.IO.Stream
[Unity]: https://unity.com
