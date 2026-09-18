# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [6.20.0] - 2026-08-25

### Added
- [IInstantiator.AddMesh](xref:GLTFast.IInstantiator.AddMesh*) and [IInstantiator.AddMeshInstanced](xref:GLTFast.IInstantiator.AddMeshInstanced*), replacing the `Primitive` naming.
- `LogCode.AnimationComponentFail`
- [SaveToFileAndDispose](xref:GLTFast.Export.GameObjectExport.SaveToFileAndDispose*) and [SaveToStreamAndDispose](xref:GLTFast.Export.GameObjectExport.SaveToStreamAndDispose*) overloads with `forceSync` parameter to enforce synchronous I/O. Recommended when exporting from Editor scripts (menu items, inspectors, post-processors), where the main-thread `SynchronizationContext` is not pumped in Edit Mode and awaited I/O continuations may never resume.
- [IInstantiator.CreateNode](xref:GLTFast.IInstantiator.CreateNode*) overload that receives the node's name, so instantiators no longer need a separate naming call. It has a default implementation that routes to the previous `CreateNode` and `SetNodeName`, so existing implementations keep working unchanged. [GameObjectInstantiator](xref:GLTFast.GameObjectInstantiator) and `EntityInstantiator` implement it as a virtual method.

### Changed
- (Performance) Internal mesh export, buffer view and image data methods return `ValueTask` instead of `Task`, which removes an allocation per vertex stream and per index buffer when exporting a readable mesh.
- Moved documentation code examples from `DocExamples` into `Runtime/DocExamples` to comply with package assembly layout requirements.
- Clarified [IDeferAgent.ShouldDefer](xref:GLTFast.IDeferAgent.ShouldDefer) documentation to note that it must eventually return `false`, otherwise imports may stall indefinitely without raising an error.
- Removed legacy .NET Framework fallback code paths (`#if NET_STANDARD` / `#if NET_STANDARD_2_1`). They were only needed for Unity versions prior to 2021.2, which are no longer supported (minimum is now Unity 6.0 LTS).
- Bumped `com.unity.collections` to 2.6.8 and `com.unity.burst` to 1.8.30, the versions recommended for Unity 6000.0.
- (Documentation) Improved installation instructions.
- (Test) Updated tests dependency Graphics Test Framework (com.unity.testframework.graphics) to 9.0.0-pre.25.
- (Test) Graphics tests generate their test cases via the Graphics Test Framework, so reference images are resolved by the framework (including per-platform overrides) and failures are inspectable in the Graphics Tests window. The per-view test methods collapsed into a single parameterized one.
- (Import) (Performance) The Editor's glTF importer reads glTF, buffer and image files straight into native memory via [AsyncReadManager](xref:Unity.IO.LowLevel.Unsafe.AsyncReadManager). It previously read each file into a managed `byte[]` that stayed pinned for the duration of the import.

### Fixed
- Corrected invalid `cref` references in XML documentation comments (parameters, type parameters and a stale method reference) that produced warnings during documentation generation.
- Fixed false positives in export to stream tests because it actually validated results from non-stream tests.
- Prevent exception when Animation component was not created successfully.
- Removed useless `SerializeFieldAttribute` from `MaterialsVariantsComponent.Control` to avoid compiler warning in Unity 6.6 and newer.
- Removed usage of obsolete `FindObjectsByType` overloads.
- (Test) Avoid `Animation` component conflict by accidentally loading glTF twice in `DocExamplesTest`.
- [IsTextureYFlipped](xref:GLTFast.GltfImportBase.IsTextureYFlipped(System.Int32)) returns correct value when multiple textures reference one image.
- (Export) Exceptions thrown during synchronous mesh export (`BakeMesh` / `BakeMeshDraco`) are now surfaced instead of being silently swallowed by unobserved tasks.
- (Documentation) Updated the features' animation section to reflect that Playables support was removed and to list animation import into custom animation systems via add-ons.
- (Performance) Removed unnecessary texture file read during Editor imports.

### Deprecated
- `IInstantiator.CreateNode` without a `name` parameter and `IInstantiator.SetNodeName`. Implement the [CreateNode](xref:GLTFast.IInstantiator.CreateNode*) overload that receives the name instead. Both remain functional.
- `IInstantiator.AddPrimitive` and `IInstantiator.AddPrimitiveInstanced`, including their `GameObjectInstantiator`, `GameObjectBoundsInstantiator` and `EntityInstantiator` implementations. Call [IInstantiator.AddMesh](xref:GLTFast.IInstantiator.AddMesh*) and [IInstantiator.AddMeshInstanced](xref:GLTFast.IInstantiator.AddMeshInstanced*) through an `IInstantiator` reference instead.

## [6.19.0] - 2026-05-19

### Added
- (Add-Ons) Import glTF animations to custom animation systems.
  - [IAnimationProcessor](xref:GLTFast.Animations.IAnimationProcessor) &mdash; animation clips conversion
  - [IAnimationProcessorFactory](xref:GLTFast.Animations.IAnimationProcessorFactory) &mdash; add-ons need to implement this
- (Add-Ons) Lifetime management of converted glTF data and application during scene instantiation. Is used for custom animation import only for now.
  - [IDataInstanceApplierFactory](xref:GLTFast.Addons.IDataInstanceApplierFactory) &mdash; owns converted data and forwards it to scene instantiation
    - [IDataCache](xref:GLTFast.Addons.IDataCache)
    - [IInstanceApplierFactory](xref:GLTFast.Addons.IInstanceApplierFactory)
  - [IInstanceApplier](xref:GLTFast.Addons.IInstanceApplier) &mdash; applies data on scene instance
- [IGltfAccessors](xref:GLTFast.IGltfAccessors) provides read-only access to typed glTF accessor data.
- [INodeHierarchyInfo](xref:GLTFast.INodeHierarchyInfo) provides glTF node hierarchy information.
- (Test) Inspector materials variant selection for `OpenGltfDialog`.
- Instructions/guidelines for coding agents (`AGENTS.md`/`CLAUDE.md`)

### Changed
- Increased minimum required Unity version to 6.0 LTS.
- Bumped dependency versions
  - com.unity.burst to 1.8.29
  - com.unity.collections to 2.6.6
  - com.unity.mathematics to 1.3.3
- GameObjectSceneInstance's [AddCamera](xref:GLTFast.GameObjectSceneInstance.AddCamera*), [AddLight](xref:GLTFast.GameObjectSceneInstance.AddLight*) and [SetLegacyAnimation](xref:GLTFast.GameObjectSceneInstance.SetLegacyAnimation*) are now public, so custom [IInstantiators](xref:GLTFast.IInstantiator) can utilize it.
- Refactored `ImportAddonInstanceCollection` to derive directly from `List`/`QueryableList`.
- (Performance) Importing animation curves got much faster and requires less managed memory. Keyframes are prepared in native arrays and set en bloc via `AnimationCurve.SetKeys(ReadOnlySpan<Keyframe>)` instead of being slowly added individually (thanks [jverral](https://github.com/jverral) for initiating this via [#44](https://github.com/Unity-Technologies/com.unity.cloud.gltfast/pull/44)).
- (Performance) Reduced memory waste by using cached, static identifiers instead of inner-loop format strings for [AnimationClip.SetCurve](xref:UnityEngine.AnimationClip.SetCurve(System.String,System.Type,System.String,UnityEngine.AnimationCurve))'s `propertyName` parameter (thanks [jverral](https://github.com/jverral) for [#44](https://github.com/Unity-Technologies/com.unity.cloud.gltfast/pull/44)).
- Enabled Render Graph in graphics settings by default on all projects (not using compatibility mode anymore).
  - (Test) Compatibility mode is still enabled for Unity 6.0 tests.

### Fixed
- Improved comma placement in export summary (thanks anonymous for [#46](https://github.com/Unity-Technologies/com.unity.cloud.gltfast/pull/46)).
- [Materials variants](xref:GLTFast.MaterialsVariantsComponent)' inspector shows correct variant when regaining focus (thanks [anonymous2585](https://github.com/anonymous2585) for [#48](https://github.com/Unity-Technologies/com.unity.cloud.gltfast/pull/48)).
- (Import) Fixed merging of mesh primitives with shared vertex buffer, but different indices type.

## [6.18.0] - 2026-04-01

### Added
- (Add-Ons) [IPostJsonDeserialization](xref:GLTFast.Addons.IPostJsonDeserialization) for intercepting the loading process right after glTF JSON deserialization.
- (Test) Model *SubMeshIncompatible*, which features animated morph targets and primitives of incompatible vertex buffer structures.

### Fixed
- (Shader) Back-face normals are now correctly flipped in URP (fixes [#38](https://github.com/Unity-Technologies/com.unity.cloud.gltfast/issues/38)).
- (Import) Textures are named after the corresponding glTF image again to ensure consistency with previous imports (reverts a change introduced in 6.17.0).
- (Import) Log error instead of throwing exception when failing to download a texture with a relative URI when a base URI is not provided.

## [6.17.0] - 2026-03-17

### Added
- Texture loading is fully customizable via Add-Ons.
  - Inject support for glTF texture extensions (see [ITextureImageLoader](xref:GLTFast.ITextureImageLoader)).
  - Customize PNG and Jpeg loading (see [IDefaultImageFormatLoader](xref:GLTFast.IDefaultImageFormatLoader)).
  - See *ImageAddonTest* scene in the tests package which use the *AddOnsImage* samples from the `DocExamples` directory.
- Explicit error message when unsupported image format WebP is detected ([LogCode.ImageFormatUnsupported](xref:GLTFast.Logging.LogCode.ImageFormatUnsupported)).
- [ImageResult](xref:GLTFast.ImageResult) which depicts an imported glTF image.
- (Test) TextureVariants test glTFs with WebP image format.
- [Extension.TextureWebP](xref:GLTFast.Extension.TextureWebP) (no general WebP support, just for handling WebP cases).

### Changed
- (Add-Ons) GltfImport now accepts multiple import add-on instances of the same type.
  - [GetImportAddonInstance](xref:GLTFast.GltfImportBase.GetImportAddonInstance*) returns the first instance that matches the type.
- Ensured compatibility with [Fast Enter Play Mode](https://unity.com/blog/engine-platform/enter-play-mode-faster-in-unity-2019-3).
- Image type detection is based on content (instead of mime-type depicted in glTF JSON, data URI mediatype or file extension).
- Improved error message when image format is detected, but not supported (e.g. WebP).

### Fixed
- (Import) [AnisotropicFilterLevel setting](xref:GLTFast.ImportSettings.AnisotropicFilterLevel) is applied to KTX textures as well.
- (Import) Leak of textures in case of loading errors.
- (Export) Meshes with zero vertices or indices will now be skipped and an error will be logged instead of an exception being thrown (fixes [#806](https://github.com/atteneder/glTFast/issues/806)).

## [6.16.1] - 2026-02-19

### Added
- (Test) *Empty Scene* test asset.
- (Import) Support for 16-bit mesh indices reduces memory footprint for meshes with less than 65k vertices per sub-mesh.
  - 16-bit indices are not converted to 32-bit anymore.
  - 8-bit indices ar converted to 16-bit (instead of 32-bit).

### Changed
- [GltfImport.Load](xref:GLTFast.GltfImportBase.Load*), [GltfImport.InstantiateSceneAsync](xref:GLTFast.GltfImportBase.InstantiateSceneAsync*) and their variants now throw an `OperationCanceledException` when cancelled before completion.
- Replaced the generic `StandardMaterialExport` with specializations.
  - [LitMaterialExport](xref:GLTFast.Export.LitMaterialExport) for Universal Render Pipeline Lit shader material export.
  - [BuiltInStandardMaterialExport](xref:GLTFast.Export.BuiltInStandardMaterialExport) for Built-in Render Pipeline Standard shader material export.
- (Performance) Improved performance of mesh indices conversion for draw modes line loop, triangle strips and triangle fan.

### Fixed
- [GltfImport.InstantiateSceneAsync](xref:GLTFast.GltfImportBase.InstantiateSceneAsync*) properly handles an invalid scene index parameter.
- [GltfImport](xref:GLTFast.GltfImportBase) waits for downloads to complete before attempting disposal during cancellation.
- Projects depending on an outdated version of [meshoptimizer mesh compression for Unity] may suppress the corresponding compiler error by using the `GLTFAST_IGNORE_MESHOPT_OUTDATED_ERROR` scripting define symbol.
- (Shader) Incorrect alpha blending/clipping due to invalid color space conversion on the base color map alpha and vertex color alpha values (affected shader graphs only; fixes [#800](https://github.com/atteneder/glTFast/issues/800)).
- (Shader) Built-in render pipeline shaders metallic-roughness and specular-glossiness now factor in vertex color alpha values.
- (Shader) All built-in render pipeline shaders apply vertex color alpha values linearly.
- (Shader) Shader graph *glTF-pbrSpecularGlossiness* does not disregard the *Glossiness* parameter anymore.
- (Import) Cases when specular-glossiness material setup would be incomplete at runtime.
- (Import) Removed remains of incorrectly signed integer indices.
- (Export) Smoothness value property is exported correctly across more combination of settings for Universal Render Pipeline Lit shader based materials (fixes [#795](https://github.com/atteneder/glTFast/issues/795) and [796](https://github.com/atteneder/glTFast/issues/796)).
- (Export) When a Lit/Standard material has a smoothness texture, their smoothness value is baked into the resulting roughness channel (of the ORM map). This preserves the visual appearance, but is a lossy operation if the smoothness value is not `1.0` (fixes [#795](https://github.com/atteneder/glTFast/issues/795) and [796](https://github.com/atteneder/glTFast/issues/796)).
- (Export) `MetaMaterialExportBuiltIn` is used for built-in material export (unless `GLTFAST_BUILTIN_SHADER_GRAPH` is set).
- (Import) Solved exception when scenes with no nodes are loaded.
- (Import) Triangle fan meshes with the center vertex not being the first vertex import correctly now.
- (Test) Graphics tests are more stable/consistent due to dedicated scene.
- (Shader) Incorrect normal unpacking when using default normal map on Android (fixes [#791](https://github.com/atteneder/glTFast/issues/791) and [802](https://github.com/atteneder/glTFast/issues/802)).
- (Export) Texture scaling not preserved on URP/Lit export (fixes [#805](https://github.com/atteneder/glTFast/issues/805)).

### Deprecated
- Generic built-in/Universal render pipeline `StandardMaterialExport`.

## [6.16.0] - 2026-01-20

### Added
- [GltfAsset](xref:GLTFast.GltfAsset) property setters to allow change of behavior from script.
  - [PlayAutomatically](xref:GLTFast.GltfAsset.PlayAutomatically).
  - [SceneId](xref:GLTFast.GltfAsset.SceneId).
- HDRP material validation (fixes [#561](https://github.com/atteneder/glTFast/issues/561)).
- Various overridable shader loading methods to [MaterialGenerator](xref:GLTFast.Materials.MaterialGenerator)-based classes so users can customize shader lookup. This is useful when working with Addressables (fixes [#715](https://github.com/atteneder/glTFast/issues/715)).
  - [MaterialGenerator.FindShader](xref:GLTFast.Materials.MaterialGenerator.FindShader(System.String)) for generic, runtime shader resolution.
  - [BuiltInMaterialGenerator.FindShaderMetallicRoughness](xref:GLTFast.Materials.BuiltInMaterialGenerator.FindShaderMetallicRoughness).
  - [BuiltInMaterialGenerator.FindShaderSpecularGlossiness](xref:GLTFast.Materials.BuiltInMaterialGenerator.FindShaderSpecularGlossiness).
  - [BuiltInMaterialGenerator.FindShaderUnlit](xref:GLTFast.Materials.BuiltInMaterialGenerator.FindShaderUnlit).
  - [ShaderGraphMaterialGenerator.LoadShaderByName](xref:GLTFast.Materials.ShaderGraphMaterialGenerator.LoadShaderByName(System.String)).

### Fixed
- Emission color (emissiveFactor) is now in correct color space for shader graphs and built-in shaders.
- Normal map scale on shader graphs.
- (Export) Avoid creating empty ORM textures if no smoothness texture is assigned (fixes [#801](https://github.com/atteneder/glTFast/issues/801)).

### Deprecated
- Access to glTF images via [IGltfReadable.ImageCount](xref:GLTFast.IGltfReadable.ImageCount) and [IGltfReadable.GetImage](xref:GLTFast.IGltfReadable.GetImage*). This has not been used internally and was unreliable. Please access glTF textures instead (see [TextureCount](xref:GLTFast.IGltfReadable.TextureCount) and [GetTexture](xref:GLTFast.IGltfReadable.GetTexture*)).

## [6.15.1] - 2025-12-09

### Added
- Assigned glTF logo to `GltfEntityAsset` component.
- (Test) Test glTF asset *CylinderWithMaterial* that's procedurally generated at runtime.
- (Test) Tests for documentation examples.
- (Test) `OpenGltfScene` improvements.
  - Refactored to use custom load method.
  - Load method option to choose between loading from file or URI.
  - Scene index option.
  - Informative console logs with file path/URI and load time.
  - Now works at runtime as well (without file dialog).
  - Re-positions camera so that the loaded glTF scene is framed.
  - Can load via `EntityInstantiator`.

### Changed
- (Entities) Entities of a scene are grouped via `LinkedEntityGroup`.
- (Performance) Import mesh indices as unsigned integers and don't convert to signed integers anymore.
- (Performance) Limited copy buffer size, so that garbage allocations do not scale with glTF-Binary content size anymore (when loading from file or `Stream`).
- (Performance) Large glTF-Binary content is now loaded into memory in smaller chunks, which keeps the frame rate smooth (when loading from file or `Stream`).
- (Performance) Shift loading glTF-Binary from stream to memory to a background thread, if it won't likely fit within the current update loop.
- (Performance) glTF-Binary buffers are not initialized with zeros before population.
- [meshoptimizer mesh compression for Unity] minimum required version was raised to 0.2.0-exp.1.
- (Test) Updated tests dependency Graphics Test Framework (com.unity.testframework.graphics) to 8.13.1-exp.1.

### Fixed
- Returning a proper error for glTF-Binary with a content length shorter than what's depicted in the header.
- NotSupportedException when loading a glTF-Binary file with excess length (fixes [#786](https://github.com/atteneder/glTFast/issues/786)).
- (Documentation) Clarified how to add export shader variants.
- (Test) Fixed generated test glTFs by exporting them in synchronous mode (only available internally).
- (Importer) Synchronization Context is now properly reset after an exception (thanks [Bruno](https://github.com/bruno1308) for [#29](https://github.com/Unity-Technologies/com.unity.cloud.gltfast/pull/29)).
- Removed compiler warning when `GLTFAST_SAFE` scripting define is active.
- (Test) Stabilize tests by executing `LogAssert.Expect` before actual tests.
- (Entities) Sub-meshes are rendered properly.
- (Test) Tests destroy the glTF entities before disposing meshes/materials to avoid batch rendering errors.

## [6.15.0] - 2025-11-17

### Added
- Loading KTX textures from data URIs.
- (Test) OpenGltfScene: Shortcut Control+X clears the previously loaded glTF. Useful for testing resource deallocation.
- (Test) Assets variants of *SubMesh* and *Rainbow Cuboid* for testing import of compressed/uncompressed multi-primitive meshes.
- (Test) glTFs with different kinds of image formats and sources.
- (Test) *No Normal* test asset.

### Changed
- *glTFast* will return a success value of `true` even if an image fails to load (which has not been consistent across the API before). This makes it easier to display assets despite of non-critical loading errors. Users who need stricter behavior can resort to monitoring for error logs (see the runtime import manual about logging or the `logger` parameter of [GltfImport constructor](xref:GLTFast.GltfImportBase.%23ctor*)).
- Primitives of a Draco compressed mesh will be decoded into in a single Unity mesh with multiple sub-meshes instead of multiple Unity meshes (thanks [Kibsgaard](https://github.com/Kibsgaard) for initiating this in [#33](https://github.com/Unity-Technologies/com.unity.cloud.gltfast/pull/33)).
- [Draco for Unity] minimum required version was raised to 5.4.0.
- (Performance) Texture data is not copied into managed memory before loading via [Texture2D.LoadImage](xref:UnityEngine.ImageConversion.LoadImage(UnityEngine.Texture2D,System.Byte[],System.Boolean)) (applies for Unity 6.0 or newer).
- (Performance) Avoid copy of entire data URI string by using `ReadOnlySpan` instead sub-stringing.
- (Performance) Base64-encoded data URIs are now decoded into [NativeArray&lt;byte&gt;](xref:Unity.Collections.NativeArray`1) instead of `byte[]`, reducing GC allocations.
- [KTX for Unity] minimum required version was raised to 3.6.0.
- Up-to-date versions of soft-dependencies [KTX for Unity] or [Draco for Unity] are enforced by raising a compiler error if an outdated version of either of those is installed.

### Fixed
- Corrected test cases for `GltfTestModels` importer tests.
- Properly dispose GPU instancing buffers after Editor imports.
- Multi-primitive, skinned meshes import properly to Unity meshes with multiple sub-meshes.
- For Draco compressed meshes, it's now ensured that morph targets are retrieved to completion and resources are disposed properly.
- Sampler settings conflicts result in a proper warning message if they cannot be resolved by instantiating the Texture2D (can occur on design-time imports).
- Implicitly add normals to the vertex attribute layout if tangents are required (fixes [#41](https://github.com/Unity-Technologies/com.unity.cloud.gltfast/issues/41)).
- Properly release native resources bound by morph targets jobs in context of decoding Draco compressed meshes.
- Avoid pointless copying of glb-embedded textures if ImageConversion module is disabled anyways.
- Avoid download of textures if ImageConversion module is disabled anyways.
- Gracefully fail when buffer data URI has incorrect media-type field or undersized content length.
- Just like PNG or Jpeg, KTX textures are now loaded readable as well if it's required by the import settings, the platform or for applying multiple sampler settings.
- Properly abort if loading of a data URI encoded image failed.
- Don't crash when a buffer failed to load when loading from `string`, `byte` arrays or `Stream`.
- Return value of [GltfImport](xref:GLTFast.GltfImportBase) loading methods ([Load](xref:GLTFast.GltfImportBase.Load*), [LoadFile](xref:GLTFast.GltfImportBase.LoadFile*), [LoadGltfBinary](xref:GLTFast.GltfImportBase.LoadGltfBinary*), [LoadGltfBinary](xref:GLTFast.GltfImportBase.LoadGltfJson*) and [LoadStream](xref:GLTFast.GltfImportBase.LoadStream*)) has been made consistent.
- (Documentation) various broken links fixed.

### Removed
- Broken consistency check between image data URI mediatype against image's mimeType.
- Support for obsolete Hybrid Renderer (com.unity.rendering.hybrid).

## [6.14.1] - 2025-09-30

### Changed
- [TryGetAllUVAccessors](xref:GLTFast.Schema.Attributes.TryGetAllUVAccessors*) only returns the first 8, actually supported UV sets (and not the ninth anymore).

### Fixed
- Avoid crash when loading meshes with more than 8 texture coordinate sets by properly limiting them.

## [6.14.0] - 2025-09-12

### Added
- Graphics Tests.
- [EditorConfig](https://editorconfig.org/) for keeping a consistent code-style.
- [IBufferView.ByteStride](xref:GLTFast.Schema.IBufferView.ByteStride).

### Changed
- (CI) Consolidated multiple redundant packaging and vetting/API validation jobs.
- Changed internal buffer representation to custom native collection `ReadOnlyBuffer<byte>` (instead of [NativeArray&lt;byte&gt;.ReadOnly](xref:Unity.Collections.NativeArray`1.ReadOnly)) This enables sub-array slicing, has in-Editor safety checks and prepares for decommissioning misuse of NativeSlice.
- Bumped Burst dependency version to 1.8.24, which is the recommended version in 2021 xLTS.
- [KTX for Unity] minimum required version was raised to 3.5.0.
- [Draco for Unity] minimum required version was raised to 5.2.0.

### Fixed
- (Export) Spotlight's inner cone angle is exported correctly on HDRP now.
- (Test) Disabled URP compatibility mode in URP presets as it's obsolete and unsupported from Unity 6.3 onward.
- (Import) Spotlight's inner cone angle is imported correctly on HDRP now.

### Removed
- (CI) SonarQube scan job.

### Deprecated
- `GLTFast.ManagedNativeArray`. It will be removed from public API in a future release. For internal development it's been replaced by `ReadOnlyNativeArrayFromManagedArray<T>`.
- `GLTFast.Export.ManagedNativeArray`. It will get sealed or removed from public API in a future release.
- [IGltfReadable.GetAccessorData](xref:GLTFast.IGltfReadable.GetAccessorData(System.Int32)). Along with [IGltfReadable.GetAccessor](xref:GLTFast.IGltfReadable.GetAccessor(System.Int32)) it is going to be removed and replaced with an improved way to access accessors' data in a future release.

## [6.13.1] - 2025-07-17

### Added
- (Test) Menu items that make switching test setups and render pipelines more accessible.
- (Test) JSON parsing performance tests.
- (Tools) Scripts for resetting test materials and settings.

### Changed
- Refactored `JsonParsingTests`.
- Ensured loggers are used in all tests and examples.

### Fixed
- (Import) Prevented `NullReferenceException` on transmissive materials with no transmissive texture.
- (Import) Potential `NullReferenceException` when clearcoat is applied without a texture.
- Incorrect version define for the Unity Collections package. glTFast now properly uses older versions (1.4.0) as well.

### Removed
- [Playables](https://docs.unity3d.com/Manual/Playables.html) option for runtime animation imports.
- (Documentation) Use case for custom [Playables](https://docs.unity3d.com/Manual/Playables.html) animation implementation.

## [6.13.0] - 2025-06-10

### Added
- (Documentation) Use case for custom [Playables](https://docs.unity3d.com/Manual/Playables.html) animation implementation.

### Fixed
- Use XYZ-style normals in shaders even if DXT5nm-style is enabled.
- (Import) When a node has morph target weights, they are applied properly (instead of the primitive's weights; fixes [#531](https://github.com/atteneder/glTFast/issues/531)).
- (Import) Specular-Glossiness materials with alpha mode `MASK` are not blended anymore in URP/HDRP (fixes [#757](https://github.com/atteneder/glTFast/issues/757)).

## [6.12.1] - 2025-04-08

### Fixed
- Incorrect using for Assert from NUnit.Framework to UnityEngine.Assertions

## [6.12.0] - 2025-03-31

### Added
- [Playables](https://docs.unity3d.com/Manual/Playables.html) option for runtime animation imports.
- Support for accessors without a buffer view when importing animation clips.

### Fixed
- (Import)`InvalidOperationException` on multi-primitive meshes with vertex colors thrown by the native container safety system (fixes [#30](https://github.com/Unity-Technologies/com.unity.cloud.gltfast/issues/30)).
- (Export) Sub-meshes that have a base vertex other than zero are exported with correct indices now.
- Reliability issues related to lack of certain async calls in [GltfImport](xref:GLTFast.GltfImport) and [GltfWriter](xref:GLTFast.Export.GltfWriter).

## [6.11.0] - 2025-03-13

### Added
- [GltfImport.Load](xref:GLTFast.GltfImportBase.Load(Unity.Collections.NativeArray{System.Byte}.ReadOnly,System.Uri,GLTFast.ImportSettings,System.Threading.CancellationToken)) overload that accept glTF data in form of [NativeArray&lt;byte&gt;.ReadOnly](xref:Unity.Collections.NativeArray`1.ReadOnly).
- [INativeDownload](xref:GLTFast.Loading.INativeDownload), which can be used to expand [IDownload](xref:GLTFast.Loading.IDownload) implementations to provide access to downloaded data directly without creating a copy in managed memory.
- Content-based glTF JSON vs. glTF-Binary detection (limited to Unity 2021 LTS or newer; resolves [#193](https://github.com/atteneder/glTFast/issues/193)).

### Changed
- (Performance) By default glTF and KTX data is not copied to managed memory implicitly when loading glTFs (true for Unity 2021 LTS or newer).

### Deprecated
- [GltfImport.LoadGltfBinary](xref:GLTFast.GltfImportBase.LoadGltfBinary*) (in favor of the generic [GltfImport.Load](xref:GLTFast.GltfImportBase.Load*))

## [6.10.3] - 2025-02-21

### Fixed
- (Import) Morph targets on multi-primitive meshes (where primitives reference identical vertex buffers; fixes [#755](https://github.com/atteneder/glTFast/issues/755)).

## [6.10.2] - 2025-02-03

### Added
- (Importer) *Textures Readable* checkbox in the importer inspector (*Textures* section).
- (Export) Error message when attempting to export with unsupported meshopt compression.
- (Tests) Runtime import performance tests.
- (Tests) Procedurally generated glTFs for testing purpose.
- (Tests) Editor export tests.

### Changed
- (Tests) Reduced jobs performance test duration by lowering buffer sizes and switching to dynamic measurement counts.
- (Tests) Performance tests are not run, unless the `RUN_PERFORMANCE_TESTS` scripting define is set.

### Fixed
- Performance test compilation if *Collections* package >= 1.5.0 is not installed.
- Inconsistent profiler markers.
- (Export) Unity Editor not responding anymore after export glTF with non-readable meshes.
- (Export) Missing `inverseBindMatrices`/`bindPoses` on skinned meshes when exporting with Draco compression.
- (CI) Ensuring the development documentation and the `Tools` code is checked for code formatting as well.
- Compilation for `ICodeLogger` implementors by adding a default implementation for `Log` (works for Unity 2021 LTS and newer).

## [6.10.1] - 2025-01-09

### Added
- Test for `ConvertBoneWeightsUInt8ToFloatInterleavedJob`
- Test for `ConvertBoneWeightsUInt16ToFloatInterleavedJob`
- *BoundsTests* which certifies correct mesh bounds.

### Changed
- Downgraded package dependencies to version bundled with Editor.
  - `com.unity.collections` to version `1.2.4` (from `1.5.1`)
  - `com.unity.mathematics` to version `1.2.6` (from `1.3.1`)
- When a position accessor lacks min/max properties, the corresponding error message is communicated via the `ICodeLogger` instead of a plain console log.

### Fixed
- Build error when used along with packages that depend on `com.unity.collections` versions older than 1.5 (e.g. Polyspatial 1.x; fixes [#730](https://github.com/atteneder/glTFast/issues/730)).
- Invalid mesh bounds on meshes with one submesh (fixes [#743](https://github.com/atteneder/glTFast/issues/743)).

## [6.10.0] - 2024-12-16

### Added
- Extended access to meshes.
  - `IGltfReadable.GetSourceMesh` returns source (de-serialized glTF) mesh.
  - `GltfImportBase.Meshes` to retrieve all imported meshes.
  - `GltfImportBase.GetMeshCount` returns the number of imported meshes per glTF mesh.
  - `GltfImportBase.GetMeshes` to iterate the imported meshes of a single glTF mesh.
  - `GltfImportBase.GetMesh` to access a single imported meshes.
- Test asset *SubMesh*.
- (CI) Automatically generated CI jobs (via Wrench/RecipeEngine; required for PackageWorks).
- (CI) Renovate action to auto-update dependencies in CI jobs.
- (CI) Renovate validation action.
- `JpgQuality` option in `ExportSettings` for finer control of jpg image exports.
- Project versions to test projects.

### Changed
- Mesh primitives of equal vertex buffer layout will result in a single Unity mesh with multiple sub-meshes instead of multiple Unity meshes (fixes [#153](https://github.com/atteneder/glTFast/issues/153)).

### Deprecated
- `IGltfReadable.GetAccessor` (replaced by `IGltfReadable.GetAccessorData`).
- `GltfImportBase.GetMeshes` (replaced by `GltfImportBase.Meshes`).

### Fixed
- Preserve per-submesh bounding box.
- `GltfAsset` properly cleans up scene instance's `Animation` component, which fixes repeated loading of animated glTFs.

## [6.9.1] - 2024-11-15

### Added
- (Test) `OpenGltfScene` with open glTF file dialog for convenient testing.
- (Test) Tests for C# jobs that calculate or re-order indices.
- Third party notices.
- `GltfImportBase.Logger` grants access to the logger in use.
- `GltfImportBase.DeferAgent` grants access to the defer agent in use.

### Changed
- Convert one mesh's primitives of identical target vertex buffer structure into sub-meshes of one Unity mesh instead of splitting them up into multiple Unity meshes (fixes [#153](https://github.com/atteneder/glTFast/issues/153)).
- Node name is assigned earlier during instantiation, enabling easier node identification by name (partially fixes [#724](https://github.com/atteneder/glTFast/issues/724)).
- (Test) Updated test project dependencies.
- (CI) Migrated code coverage.
- Code refactoring
  - Flattened `GltfImportBase.Prepare` by extracting many large code blocks into dedicated methods.
  - Improved class/field naming. It's now less deceptive and more uniform.
  - Using `NativeArray` directly/only instead of `AccessorData` classes/managed arrays. With this change `NativeArrays` are used for index buffers as well which reduces the amount of managed memory allocated.
- Facilitated use of safer NativeCollections in C# jobs that calculate or re-order indices instead of `unsafe` / pointers.

### Fixed
- Made sure that mesh primitives of different drawmode (topology) are not mixed up.
- Apply morph targets/blend shapes before uploading to GPU in `GLTFAST_KEEP_MESH_DATA` mode.

## [6.9.0] - 2024-10-30

### Added
- Package coherence tests that make sure package versions match across exported generator string and documentation.

### Changed
- Moved code examples that are referenced by the documentation into a different folder (`DocExamples`).
- Renamed code example assembly/namespace to `GLTFast.Documentation.Examples` for consistency.

### Fixed
- (Test) LoadTests on Android now succeed by using `UnityWebRequest` to retrieve data from the compressed JAR file.
- Loading glTFs from `StreamingAssets` with relative URIs containing Unicode characters on Android. UriHelper.GetBaseUri and UriHelper.GetUriString handle Android `jar:file://` schema URIs with unicode characters correctly (fixes [#667](https://github.com/atteneder/glTFast/issues/667)).
- XML documentation fixes
- Removed unnecessary "type" property from `package.json`.
- Removed warning about obsolete `GraphicsDeviceType.OpenGLES2` in Unity 2023.1 or newer.
- (Export) Missing texture transform if texture on glTFast material was scaled vertically only.
- Improved reliability by adding null checks and imprecision-aware floating-point comparisons in various places.
- Using immutable fields only in hash code calculation for `ImageExport` classes.
- Refactored `GetHashCode` implementations referencing mutable fields to avoid potential unexpected behavior.
  - `TextureComparer.Equals` made `GetHashCode`/`Equals` for `TextureBase` obsolete, so they've been removed.
  - `MeshPrimitiveComparer` is now used for clustering mesh primitives (instead of `GetHashCode`/`Equals` on `MeshPrimitive` and sub-types).
- Set minimum required Unity version to 2020.3.48f1 in the documentation.
- (Export) Avoid potential loss of data by allocating output streams persistently.
- (Test) Render export test inconclusive if the result has not been validated.
- (Test) More explicit error message by throwing innermost exception while preserving the stack trace during async tests.
- (Documentation) Various clarifications, improvements and fixes, based on user feedback.

### Removed
- Outdated and unused code coverage badge.

## [6.8.0] - 2024-09-05

### Added
- (Import) Setting to create textures readable. This allows users to access resulting textures from their scripts.
- (Export) Non-readable meshes can be exported as well now.
- (Export) Added support for exporting meshes with vertex compression enabled (effectively converting 16-bit float positions/normals/tangents/texture coordinates to 32-bit floats).
- (Export) Skinned meshes export support (thanks [Hugo Pereira][Hugo-Didimo] for [#512](https://github.com/atteneder/glTFast/pull/512)).
- (Export) [Buffer view targets](https://registry.khronos.org/glTF/specs/2.0/glTF-2.0.html#_bufferview_target) are set properly now.
- (Import) Support for mesh primitive modes `TRIANGLE_STRIP` and `TRIANGLE_FAN` (thanks [Hexer611][Hexer611] for [#22](https://github.com/Unity-Technologies/com.unity.cloud.gltfast/pull/22))

### Fixed
- (Export) Writing to files on the web via IndexedDB now works (fixes [#625](https://github.com/atteneder/glTFast/issues/625))
- (Export) test results are validated again.
- (Export) Removed expendable JSON content when exporting unlit materials without color or texture applied.
- Primitve mode LINE_LOOP works as expected (thanks [Hexer611][Hexer611] for [#22](https://github.com/Unity-Technologies/com.unity.cloud.gltfast/pull/22)).
- (Test) Fail export test if glTF JSON contains unexpected or misses expected properties.
- Increased resilience against invalid animation data.
- Broken link in `CONTRIBUTING.md` (thanks [Hexer611][Hexer611] for [#22](https://github.com/Unity-Technologies/com.unity.cloud.gltfast/pull/23)).
- Loading glTFs with unknown texture extensions (e.g. WebP, `EXT_texture_webp`) now works (fixes [#705](https://github.com/atteneder/glTFast/issues/705)).

## [6.7.1] - 2024-08-07

### Added
- Test for correct handling of Android JAR URIs.

### Fixed
- (Export) Cases of corrupt glTFs when not all vertex attributes of a mesh were exported.
- Alpha blending via baseColorTexture's alpha value is now in correct color space, less opaque and as a result consistent with other glTF viewers (affected URP and built-in render pipeline projects in linear color space; fixes [#700](https://github.com/atteneder/glTFast/issues/700)).

## [6.7.0] - 2024-06-25

### Added
- (Import) Support for [materials variants extension](https://github.com/KhronosGroup/glTF/tree/main/extensions/2.0/Khronos/KHR_materials_variants).
- Serialization support for material extensions IOR, Sheen and Specular.
- (Import) Ability to load a glTF from a generic `Stream` (`GltfImport.LoadStream`; thanks [sandr01d][sandr01d] for [#10](https://github.com/Unity-Technologies/com.unity.cloud.gltfast/pull/10)).

### Changed
- (Import) Prefabs imported from glTF assets (at design-time) don't have the glTF logo icon assigned to them anymore. This makes it more consistent with other file types (like FBX; fixes [#557](https://github.com/atteneder/glTFast/issues/557)).

### Deprecated
- `MetaMaterialExport`. Always use `MaterialExport.GetDefaultMaterialExport` to get the correct material export.

### Fixed
- (Export) glTFast shader based materials and textures are exported correctly when using the default render pipeline.
- Added missing entries to the API documentation.
- (Export) Base colors are now in correct, linear color space.
- Alpha mode blend now works as expected in HDRP 11 and newer as well (fixes [#699](https://github.com/atteneder/glTFast/issues/699)).
- (Export) Fixed mesh min/max when using Draco compression.

## [6.6.0] - 2024-05-29

### Added
- Serialization/de-serialization (only) support for the KHR_materials_variants extension.

### Fixed
- Compatible with [Entities 1.2.0][Entities1.2].
- Black materials when using low standard shader quality with the built-in render pipeline (thanks [Victor Beaupuy][Kushulain] for [#595](https://github.com/atteneder/glTFast/pull/595)).
- (UI) Quantity of report items is not shown in importer inspector anymore. Report items cannot be removed anymore (thanks [krisrok][krisrok] for [#428](https://github.com/atteneder/glTFast/pull/630)).

## [6.5.0] - 2024-05-15

### Added
- (Export) Support for exporting glTFast shader based materials. This reduces data loss on import-export round trips considerably.
- (Export) Support for setting a custom scene origin via transform matrix.
- Dependency on [Unity Collections package][Collections].
- Added Apple Privacy Manifest documentation.
- Export sample code.
- XML documentation comments.
- `float4x4.Decompose` overload that outputs rotation as type `quaternion`.

### Changed
- Faster buffer conversion jobs due to batching via [`IJobParallelForBatch`](https://docs.unity3d.com/Packages/com.unity.collections@2.4/api/Unity.Jobs.IJobParallelForBatch.html).
- (Export) Material exporter implementation is chosen based on used shader by default.
- (Export) Vertex attributes are discarded if they are not used/referenced.
- (Export) Root level nodes' positions are based on their GameObject's world positions (and not their local position anymore).

### Fixed
- (Export) Discrepancy in color due to export of unused vertex colors.
- Incorrect copyright text in some SPDX headers.

### Deprecated
- `float4x4.Decompose` overload that outputs rotation as type `float4` (quaternion values).

### Removed
- Soft dependency on deprecated [Unity Jobs package][JobsPkg].
- Legacy code for Unity versions older than the minimum required 2020 LTS.

## [6.4.0] - 2024-04-17

### Added
- Tests for all `GltfImport.Load` overloads.
- Tests for all import Burst jobs.
- `ICodeLogger.Log` for dynamic LogType usage.

### Changed
- *Emission* sub graph uses shader define `SHADEROPTIONS_PRE_EXPOSITION` for HDRP usage detection (replacing a custom function node that checked for `UNITY_HEADER_HD_INCLUDED`).
- *BaseColor* sub graph uses built-in shader define [`UNITY_COLORSPACE_GAMMA`](https://docs.unity3d.com/ScriptReference/Rendering.BuiltinShaderDefine.UNITY_COLORSPACE_GAMMA.html) for project color space detection (replacing a custom function node).

### Fixed
- Shader sub graphs *BaseColor* and *Emission* are now compatible with [PolySpatial visionOS][PolySpatialVisionOS].
- On Apple visionOS, textures are always created readable, so that [PolySpatial visionOS][PolySpatialVisionOS] is able to convert them.
- Draco compressed tangents import tangents correctly now.
- Removed invalid attempt to calculate normals or tangents on point or line meshes.
- Consistent log message when a glTF extension cannot be supported due to a missing Unity package depenency (e.g. [KTX for Unity]).
  - All missing extensions are logged (not just the first one).
  - There's now a single message per missing package.
  - Depending on whether that extension is required the message's type is warning or error.
  - Added explicit message when [meshoptimizer mesh compression for Unity] is missing.

## [6.3.0] - 2024-03-27

### Added
- Runtime import tests.
- Runtime export tests.
- (Export) Added development-time checks for valid JSON string literals.
- Added Apple Privacy Manifest file to `/Plugins` directory.

### Changed
- Refactored test scripts folder layout.
- (Export) Normal maps are exported in PNG format by default.
- (Export) HDRP area lights are still exported as spot-lights, but their intensity is taken from `Light.intensity` (still incorrect, but more consistent).
- Switched from asset-path-based to GUID-based shader loading (in the Editor 2021 and newer) in order to allow for a flexible folder layout without risking breaks/regressions should the layout change in the future.
- Avoid expensive UnityEngine.Object null check when accessing cached default shaders.

### Fixed
- Exception when required glTF shader is not included.
- Compiler errors when safe mode (`GLTFAST_SAFE` scripting define) is enabled.
- Compiler error with High Definition Render Pipeline version 17 (2023.3)
- Removed usage of obsolete APIs in High Definition Render Pipeline version 17 (2023.3)
- (Export) Area light's range value is exported accurately (as shown in the inspector).
- Various occasions of `NullReferenceException` when no logger is used/provided.
- Proper error handling when trying to load unsupported sparse texture coordinates.
- Ensure that special chars in string values don't lead to invalid JSON.
- Using invariant culture `ToLower`/`ToUpper` variants on all non-language-specific data.
- Added missing `GetHashCode` implementation (removes compiler warning).
- Compiler errors and warnings on newer HDRP versions (16.x/17.x)
- URP clearcoat shader loading at runtime.
- HDRP stack-lit shader loading at runtime.

## [6.2.0] - 2024-01-29

### Added
- Deprecated soft-dependency packages are detected and a warning with upgrade instructions is shown in the console.

### Changed
- Support for Draco 3D Data Compression is now provided by [*Draco for Unity* (com.unity.cloud.draco)][Draco For Unity], which is a fork of and replaces [*DracoUnity* (com.atteneder.draco)][DracoUnity].

### Fixed
- Compiler error when Newtonsoft JSON package was not installed.
- All Draco vertex attributes are assigned by identifier instead of type. As a result, tangents are now decoded properly instead of recalculated.
- Compilation error when scripting define `GLTFAST_BUILTIN_SHADER_GRAPH` is set.
- `GltfImport.IsTextureYFlipped` returns correct result for non-KTX textures.

## [6.1.0] - 2024-01-17

### Added
- (Documentation) Explanation and user case for the add-on API
- `GltfImport.IsTextureYFlipped` to support non-default texture orientations

### Changed
- Documentation improvements
- Auto-formatted all markdown, USS, UXML and shader code
- CI maintenance

### Fixed
- Updated references to [KTX for Unity]

## [6.0.1] - 2023-10-11

### Fixed
- Compilation error when Animation module is disabled and Newtonsoft JSON package installed.
- Compilation on Unity 2020 LTS

## [6.0.0] - 2023-10-04

### Added
- Custom Add-On API (`GLTFast.Addons` namespace)
- Support for alternative JSON parsing via Newtonsoft JSON
- `Accessor.ElementByteSize`: Byte size of one element of that accessor
- `Accessor.ByteSize`: Overall byte size
- `IGltfReadable.GetAccessor`: Generic byte-array view into an accessor
- `GameObjectInstantiator` events that allow further instantiation customizations
  - `NodeCreated`
  - `MeshAdded`
  - `EndSceneCompleted`
- Value array JSON parsing tests
- String/enum conversions tests
- (Import) Clearcoat material support in HDRP and URP (via [KHR_materials_clearcoat](https://github.com/KhronosGroup/glTF/blob/main/extensions/2.0/Khronos/KHR_materials_clearcoat/) extension)
- (Export) Clearcoat material export support for HDRP Lit shader

### Changed
- Bumped minimum Unity version to 2020.3.48f1
- Renamed `GltfAnimation` to `Animation` for consistent naming.
- Bumped Burst dependency version to 1.8.4
- Bumped Mathematics dependency version to 1.3.1

### Fixed
- Added Obsolete attribute to public schema class fields that are for serialization only and should not get modified directly.
- More robust parsing of (invalid) enum values

## [5.2.0] - 2023-09-14

### Added
- Runtime tests
- (Export) Setting for deterministic export (limits concurrency to ensure consistent output)
- (DOTS) Support for [Entities 1.0][Entities1.0]

### Changed
- Optimized `Accessor.GetAccessorAttributeType`
- Optimized `GltfEntityAsset.ClearScenes` via Burst
- Bump minimum Unity version from 2019.4.7f1 to 2019.4.40f1

### Fixed
- Compiler errors and warnings on Unity 2023.2 (and newer) due to using obsolete types.

## [5.1.0] - 2023-05-25

### Added
- (Export) Support for Draco mesh compressed exports

### Changed
- Added proper root namespace to all assembly definitions
- License and copyright notices
- (Export) Increased performance due to concurrent buffer conversions
- Support for KTX (Khronos Texture) is now provided by [*KTX for Unity* (com.unity.cloud.ktx)][Ktx for Unity], which is a fork of and replaces [*KtxUnity* (com.atteneder.ktx)][KtxUnity].

## [5.0.4] - 2023-03-30

### Fixed
- Texture transform offset is calculated correctly now

## [5.0.3] - 2023-03-29

### Fixed
- Update licensing (internal only)

## [5.0.2] - 2023-03-03

### Fixed
- Define constraints for KTX package (internal only)

## [5.0.1] - 2023-03-02

### Changed
- (Export) Texture coordinates are now flipped vertically, similar to how it's performed at import. This ensures round-trip consistency (#342).

### Fixed
- (Export) Invalid blend indices or blend weights are not exported anymore (as skinning is not supported yet; #556)
- Compiler error when using .NET Framework on 2021.3 and newer (#550)
- `GltfBoundsAsset`'s instantiation settings are applied now
- `GltfBoundsAsset`'s `BoxCollider` is positioned correctly, even if GameObject is not at scene origin (#565)

## [5.0.0] - 2022-12-09
This release contains multiple breaking changes. Please read the [upgrade guide](xref:doc-upgrade-guides#upgrade-to-50) for details.

### Added
- `settings` parameter to `GameObjectBoundsInstantiator`'s constructor
- (Import) Support for lights via KHR_lights_punctual extension (#17)
- (Import) Exclude/include certain features (e.g. camera, animation, lights) via `InstantiationSettings.Mask` (of type `ComponentType`)
- DOTS instantiation settings support
- (Import) Additional load methods in `GltfImport` (#409)
  - `Load` override to load from a `byte[]`
  - `LoadFile` to load from local files
  - `LoadGltfJson` to load a glTF JSON from string
- (Import) `SceneObjectCreation` instantiation setting. It controls whether/when a GameObject/Entity should be created for the scene. Options: `Always`, `Never`, `WhenSingleRootNode`. (#320)
- (Import) Design-time import inspector now offers many more settings (feature parity with run-time settings)
- Extended access to `IGltfReadable`
  - `GetSourceRoot`
  - `GetSourceNode`
  - `GetBindPoses`
- `GltfAsset` component got new properties for code-less setup
  - Import Settings
  - Instantiation Settings
- Warning when trying to load the main scene if it is not defined (Editor and development builds only; #450)
- (Export) Support for camera export
- (Export) Support for lights export
- glTF icon assigned to imported glTF assets, `GltfAsset*` components and and various setting classes
- (Import) Support for up to 8 UV sets (note: glTF shaders still support only two sets; part of #206)
- `IMaterialGenerator` was extended with support for points topology
- (Export) `GameObjectExportSettings.DisabledComponents` to explicitly enable export of disabled components (e.g. `MeshRenderer`, `Camera`, or `Light`)
- (Export) `ExportSettings.ComponentMask` to include or exclude components from export based on type
- (Export) `GameObjectExportSettings.LayerMask` to include or exclude GameObjects from export based on their layer
- (Import) Async instantiation methods. This helps to ensure a stable frame rate when loading bigger glTF scenes (#205)
- `GltfGlobals` is public now
- `GameObjectInstantiator.SceneTransform` is public now

### Changed
- The API was changed considerably to conform closer to Unity's coding standard and the Microsoft's Framework Design Guidelines. Some notable items:
  - PascalCase on properties
  - Removed direct access to fields
  - More consistent naming of assemblies, namespaces, classes, constants, static members, etc.
  - Removed majority of Rider code analysis warnings and suggestions
- Converted a lot of unintentionally public classes, types and properties to internal ones
- Replaced `CollectingLogger.item` with `.Count` and `.Items` iterator
- Moved logging related code into `GLTFast.Logging` namespace
- Renamed `Schema.RootChild` to `Schema.NamedObject` and made it abstract
- Converted  `GameObjectInstantiator.Settings` to `InstantiationSettings`
- Removed `RenderPipelineUtils.DetectRenderPipeline` in favor of `RenderPipelineUtils.RenderPipeline`
- Additional methods/properties (e.g. from class `GameObjectInstantiator`) are virtual, so they can be overridden
- `GltfImport` implements `IDisposable` now (#194)
- Support for PNG/Jpeg textures (via built-in packages *Unity Web Request Texture* and *Image Conversion*) is now optional (#321)
- Root entity created by `GltfEntityAsset` will inherit its GameObject's name, position, rotation and scale (at instantiation time)
- Removed `GltfImport.GetAccessor` from public API (to be replace by a better API; see #426 for details)
- Converted `emissiveFactor` shader property from low to high dynamic range (HDR) and removed the now obsolete `emissiveIntensity` shader property (float)
- Shader keyword `_UV_ROTATION` was replaced by `_TEXTURE_TRANSFORM`, which now controls tiling, offset and rotation all together
- Animation is not played by default anymore (check the upgrade guide on how to restore this behavior; #339)
- (Import) Deprecated existing, sync instantiation methods in favor of new async ones
- KTX textures load much smoother thanks to bumping KtxUnity to 1.3.0 or 2.2.1
- Sped up loading of external KTX textures by avoid making a redundant memory copy.
- `IDownload` does not derive from `IEnumerator` anymore
- (Import) Successfully tested mesh primitive draw mode `lines` and removed error message about it being untested
- (Export) Disabled components (e.g. `MeshRenderer`, `Camera`, or `Light`) are not exported by default (see also: new `GameObjectExportSettings.DisabledComponents` setting to get old behavior)
- (Export) GameObjects with tag `EditorOnly` (including children) don't get exported (similar to building a scene)
- Added optional `CancellationToken` parameter to async import/export methods. This is preparation work for proper cancellation. Does not work as expected just yet.
- Refactored Assembly Definitions
  - `glTFastSchema` was merged into `glTFast` and thus removed
  - `glTFastEditor` was renamed to `glTFast.Editor`
  - `glTFastEditorTests` was renamed to `glTFast.Editor.Tests`
- `GltfAsset.FullUrl` is public now (convenient for some tests)
- `IInstantiator` changes
  - `IInstantiator.BeginScene` signature dropped third parameter `AnimationClip[] animationClips` that was depending on built-in Animation module to be enabled.
  - `IInstantiator.AddAnimation` was added. Only available when built-in Animation module is enabled.
- Converted properties that were hiding conversion logic or caching into methods
  - `Accessor`: `typeEnum` to `GetAttributeType`/`SetAttributeType`
  - `BufferView`: `modeEnum` to `GetMode`
  - `BufferView`: `filterEnum` to `GetFilter`
  - `AnimationChannelTarget`: `pathEnum` to `GetPath`
  - `AnimationSampler`: `interpolationEnum` to `GetInterpolationType`
  - `Camera`: `typeEnum` to `GetCameraType`/`SetCameraType`
  - `LightPunctual`: `typeEnum` to `GetLightType`/`SetLightType`
  - `Material`: `alphaModeEnum` to `GetAlphaMode`/`SetAlphaMode`
- Moved some nested classes into dedicated files and up the namespace hierarchy
  - `GameObjectInstantiator.SceneInstance` is now `GameObjectSceneInstance`
  - `ImportSettings.NameImportMethod` is now `NameImportMethod`
  - `InstantiationSettings.SceneObjectCreation` is now `SceneObjectCreation`
- `HttpHeader`'s properties are readonly now. A constructor was added as compensation.
### Removed
- Obsolete code
  - `GltfImport.Destroy` (was renamed to `GltfImport.Dispose`)
  - `GLTFast.GltFast` (was renamed to `GltfImport`)
  - `GltfImport.InstantiateGltf` (was replaced by `InstantiateMainScene` and `InstantiateScene`)
  - Remains of Basis Universal extension draft state
    - `Schema.Image.extensions`
    - `Schema.Image.ImageExtension`
    - `Schema.Image.ImageKtx2`

### Fixed
- Shader graphs' BaseColor, BaseColorTexture and vertex color calculations are now in correct color space
- Export MeshRenderer where number of materials does not match number of sub-meshes (thanks [Dan Dando][DanDovi] for #428)
- Shaders and shader graphs now have a proper main color and main texture assigned (except legacy shader graphs where this is not supported)
- No more redundant default (fallback) materials are being generated
- (JSON parsing) Potential NPDR when just one of many node extensions is present (#464)
- (Import) Draco meshes are correctly named (#527)
- (Import) Gracefully fallback to loading textures from byte arrays if UnityWebRequestTexture module is not enabled and trigger a warning.
- (Import) `GltfBoundsAsset.Load` properly passes on the logger now.
- (Import) Exception upon loading a file that uses the `KHR_animation_pointer` extension.

## [4.9.1] - 2022-11-28

### Changed
- (Import) An `Animator` component is added to the scene root GameObject when Mecanim is used as animation method (thanks [@hybridherbst][hybridherbst] for #519). This is convenient at design-time and a preparation for Playable API support.
- (Import) Frame rate improvement when using Draco compression (thanks [@hybridherbst][hybridherbst] for #520).

## [4.9.0] - 2022-11-11

### Added
- (Export) HDRP metallic/roughness texture assignment can be omitted by setting the corresponding smoothness remap range min equal to max and metallic factor to 0. Useful for only exporting the ambient occlusion channel of a mask map.
- (Export) HDRP occlusion texture assignment can be omitted by setting the corresponding AO remap minimum to 1.0. Useful for only exporting the metallic/smoothness channels of a mask map.

### Changed
- (Export) Reduced memory footprint when exporting textures
- (Export) Faster temporary texture construction in Unity 2022 and newer
- (Import) Faster texture creation in Unity 2022 and newer
- (Import) Default (fallback) material now gets named `glTF-Default-Material` instead of shader's name, which is deterministic across render pipelines
- (Export) Don't use HDRP Lit MaskMap metallic/smoothness channels if they are not used (i.e. metallicFactor is zero and smoothness remap range is zero)
- (Export) HDRP Lit base color map is exported as Jpeg, if alpha channel is not used (similar to other render pipelines)
- `IDownload` now has to implement `IDisposable` as well which ensures resources are disposed correctly.

### Fixed
- (Export) No empty filename for textures with no valid name (e.g. `.jpg`;#458)
- (Export) Memory leak: Temporary textures are properly destroyed (happened on non-readable or ORM textures; fixes #502)
- (Import) Don't duplicate texture assets (textures referenced by relative URI; #508)
- (Shader) Built-in pbrMetallicRougness shader's metallicFactor property defaults to 1.0, according to the glTF spec
- (Export) HDRP Lit shader's normal scale is exported correctly now
- (Export) HDRP Lit shader's double sided property is exported correctly now
- (Export) HDRP Lit shader's smoothness remap property is exported correctly now
- (Export) HDRP Lit shader's occlusion texture has correct transform now (was vertically inverted before)
- (Export) HDRP Unlit color is exported correctly
- (Import) Unity 2020+ crash in Editor and builds due to undisposed `DownloadHandler`s
- (Export) Case of duplicate meshes (even with identical primitives/attributes/indices/materials) when using .NET Standard in your project

## [4.8.5] - 2022-08-30

### Fixed
- (Export) Meshes with point topology are exported correctly now (#434)
- Incorrect texture transform calculation when using rotation (#413)

## [4.8.4] - 2022-08-26

### Changed
- (Import) Double-sided GI is enabled on all materials for Editor imports (#452)

### Fixed
- Diffuse texture transform on specular glossiness materials (#454)
- Corrected pointer math in accessor conversions
  - Int16 texture coordinates
  - Normalized Int16 texture coordinates (#439)
  - Normalized Int16 tangents

## [4.8.3] - 2022-06-04

### Fixed
- Loading glTFs with nothing but accessors/bufferViews/buffers (#422)
- Loading glTFs with invalid embed buffers (#422)
- Corrected unsigned short joint weights import (#419)

## [4.8.2] - 2022-06-15

### Changed
- Load textures/images, even when not referenced by material (#418)

### Fixed
- glTFs without nodes (#417)

## [4.8.1] - 2022-06-10

### Changed
- Bumped Burst dependency version to 1.6.6

### Fixed
- UWP build (#400)
- Shader compile errors in  2021.2 and later due to incorrectly named property in shader graph `glTF-pbrSpecularGlossiness-Opaque-double`

## [4.8.0] - 2022-05-30

### Added
- A target layer can be defined for instantiated GameObjects via `GameObjectInstantiator.Settings.layer` (thanks [Krzysztof Lesiak][Holo-Krzysztof] for #393)
- Re-normalize bone weights (always for design-time import and opt-in at runtime via `GLTFAST_SAFE` scripting define)
- `GltfAssetBase.Dispose` for releasing resources

### Changed
- Mecanim (non-legacy) is now the default for importing animation clips at design-time (thanks [@hybridherbst][hybridherbst] for #388)
- Mipmaps are generated by default now when importing at design-time (thanks [@hybridherbst][hybridherbst] for #388)
- All four bone weights are imported at design-time, regardless of quality setting
- SkinnedMeshRenderer's rootBone property is now set to the lowest common ancestor node of all joints. This enables future culling optimization (see #301)

### Fixed
- Fail more gracefully when parsing invalid JSON
- Proper error handling on glTF-binary files with invalid chunks (unknown type or invalid length; #389)
- Properly handle skins without inverse bind matrices
- Avoid loading Jpeg/PNG textures twice when they are sampled linearly or mipmaps are generated

## [4.7.0] - 2022-04-25

### Added
- `RenderPipelineUtils` to detect current render pipeline
- Option to make glTFast an alternative `.glb`/`.gltf` importer (not default anymore; via scripting define `GLTFAST_FORCE_DEFAULT_IMPORTER_OFF`). Useful in projects where you have another default importer for glTF (thanks [@hybridherbst][hybridherbst] for #367)
- Prefabs `glTF-StableFramerate` and `glTF-FastestLoading` for easy, no-code setup of global runtime loading behavior (via `IDeferAgent`)
- `GltfImport.SetDefaultDeferAgent` and `GltfImport.UnsetDefaultDeferAgent` for setup of global runtime loading behavior (via `IDeferAgent`)
- `TimeBudgetPerFrameDeferAgent` component now has a `frameBudget` property with a nice slider in the inspector
- `UninterruptedDefaultDeferAgent`, a Monobehavior wrapping `UninterruptedDeferAgent`

### Changed
- (DOTS) Update to Entities 0.50
- (DOTS) Removed unused `GltfComponent`
- Bumped Mathematics and Burst package dependency versions to current 2019 LTS verified versions
- Renamed `UniveralRPMaterialGenerator` to `UniversalRPMaterialGenerator` (typo)

### Fixed
- Using correct file API for reading bytes in `EditorDownloadProvider` (thanks [@hybridherbst][hybridherbst] for #360)
- GUID conflict with UnityGLTF
- (Export) Correct float serialization on systems with non-English culture configuration (relates to #335)
- Documentation link in error message about missing shaders (#368)
- Slow loading after scene loading due to reference to destroyed default `IDeferAgent` (#165)
- (Import) Better error handling when textures are missing
- (Export) Remember destination path when exporting individual GameObjects from menu
- (Export) Vertical texture transform offset is correct now
- Improved relative file path handling on platforms with non-forward slash directory separator (Windows)
- (Import) Draco compressed meshes' submeshes now have bounds set from the accessor's min/max values (just like regular/uncompressed meshes; #384)
- (Export) De-duplication by properly re-using glTF `mesh` if accessors and materials are identical (#364)
- (Export) Removed error messages about non-matching Profiler calls (#357)
- (Export) Re-encoded (blitted) textures are in correct sRGB color space, even when the project is in linear color space (#353)
- (Export) Removed incorrect color space conversion on normal maps (#346)
- For projects using the built-in render pipeline in gamma color space, vertex colors are now applied in the correct color space

## [4.6.0] - 2022-02-23

### Added
- (Export) Runtime glTF export to files
- (Export) Export for Unity versions older than 2020.2
- (Export) Save to `System.IO.Stream`
- (Export) Occlusion map support
- (Export) Metallic-gloss map support (converted to roughness-metallic)
- (Export) Combine multiple maps to single occlusion-roughness-metallic map
- (Export) Emission support
- (Export) Correct texture filter and wrap modes by creating glTF `sampler`
- (Export) Support for injecting custom material conversion via `IMaterialExport`
- (Documentation) XML documentation comments on many types
- (Documentation) Initial setup for DocFX generator

### Changed
- glTF export menu entries moved from `File -> Export` to
  - `File -> Export Scene` to export the active scene
  - `Assets -> Export glTF` for assets (may also be accessed from project view context menu)
  - `GameObject -> Export glTF` for GameObjects (may also be accessed from hierarchy view context menu)
- (Documentation) Split up monolithic docs into multiple markdown files
- (Documentation) Changelog links to code are now `xref` (for DocFX)
### Removed
- Converted a lot of unintentionally public classes, types and properties to internal ones
- `StopWatch`, a class used for measuring load times in tests, was moved to a dedicated test repository

### Fixed
- Point meshes are rendered consistently on more platforms (iOS, Vulkan) due to explicitly setting `PSIZE` (thanks [Kim Wonkee][wonkee-kim] for #309)
- Removed Editor markup resources from builds
- Misformatted XML documentation comments
- Correct render pipeline detection in case of quality settings override
- (Documentation) Many minor fixes like XML doc linter errors/warnings
- (Export) Removed redundant texture entries in glTF schema
- (Export) Properly closing buffer file stream
- (Export) Conflict of textures with identical names
- (Export) Exporting assets/prefabs from project view created empty glTFs
- (Export) Correct float array serialization on systems with non-english culture configuration (#335)
- Textures are not duplicated anymore if they use different samplers resulting in equal Unity settings (saves memory on corner-case glTFs)
- (Export) Various material fixes and improvements
- (Import) First-time imports work now, because it is ensured that the shaders are loaded correctly (#315)
- (Import) HDRP >= 10.0: Alpha blended materials are not invisible anymore
- (Import) URP >= 12.0: Alpha masked materials are correctly alpha tested now
- (Import) URP >= 12.0: Alpha blended `pbrMetallicRoughness` materials are correctly blended now
- (Import) Improved error logs in Editor imports
- 2019 HDRP compiler errors
- Correct bounds calculation of meshes with normalized position accessors (applies for most quantized meshes; #323)
- Removed precautious error message (#281)

## [4.5.0] - 2022-01-24

### Added
- Generic shader graphs (to reduce the amount of shader graphs to maintain and reduce shader variants)
  - `glTF-pbrMetallicRoughness`
  - `glTF-pbrSpecularGlossiness`
  - `glTF-unlit`

### Changed
- The new, generic shader graphs are used for
  - Universal render pipe 12 or newer
  - High-Definition render pipe 10 or newer
  - Optional/Experimental for the Built-In render pipe (see [Shader Graphs and the Built-In Render Pipeline](xref:doc-project-setup#shader-graphs-and-the-built-in-render-pipeline) in the documentation for details)

### Fixed
- Correct emission in HDRP 12 and later
- (Shader Graph) Vertex color alpha channel is used properly
- (Shader Graph) Correct vertex colors when project uses linear color space
- (Shader Graph) Emission is now in correct color space

## [4.4.11] - 2022-01-24

### Changed
- `SkinnedMeshRenderer` created by the `GameObjectInstantiator` will have `updateWhenOffscreen` set to *true* to avoid culling issues (at a performance cost; #301)
- (Editor Import): Imported Mecanim AnimationClips now have Loop Time set to true (fixes #291)

### Fixed
- Improved skin deformation on unordered-joints-glTFs in projects with `Skin Weights` (quality setting) below 4 (#294)
- Textures are not duplicated anymore if they reference different samplers with equal settings (yields huge memory savings, depending on some glTFs; thanks [Vadim Andriyanov][Battlehub0x] for #304)

## [4.4.10] - 2022-01-14

### Changed
- Improved frame rate when loading glTFs with many morph targets (thanks [Eric Beets][EricBeetsOfficial-Opuscope] for #287)
- `GameObjectInstantiator.SetNodeName` can be overridden now (thanks [STUDIO NYX][NyxStudio] for #297)

### Fixed
- Matrix decompose error (thanks [weichx][weichx])
- Flickering animation on invalid glTFs from Sketchfab (#298)

## [4.4.9] - 2021-12-20

### Fixed
- (URP/HDRP) Materials with `alphaMode` `MASK` are alpha tested (and not blended as well) as specified in the specification (thanks [rt-nikowiss][rt-nikowiss] for #296)

## [4.4.8] - 2021-12-06

### Fixed
- Morph target animation curves have correct first keyframe value now (thanks [Eric Beets][EricBeetsOfficial-Opuscope] for #277)
- (URH/HDRP) UV transform and UV channel on blended materials
- Error when using transmission approximation without a logger provided
- `ConsoleLogger` non-`LogCode` messages have the correct log level now
- Correct blending on URP 12 / HDRP 10 alpha blended materials

## [4.4.7] - 2021-11-12

### Changed
- (HDRP): Configuring materials via settings and shader keywords instead of using duplicated shader graphs. This reduces the total shader variant count.

### Fixed
- Correct blend mode for transmission in URP
- Correct transparency on HDRP >= 10.x (Unity 2020.3 and newer)
- (URP/HDRP) Using the second UV set on double-sided materials
- (URP/HDRP) Corrected baseColorTexture UV transform on double-sided materials

## [4.4.6] - 2021-11-10

### Added
- Added warning when more than two UV sets are supposed to be imported (not supported yet)

### Changed
- Major performance improvement when loading glTFs with many KTX textures

### Fixed
- Correct import of interleaved float RGBA vertex colors (thanks [@mikejurka][mikejurka] for #266)
- Corrected potential pitfall by incorrect UV import job handling (thanks [@mikejurka][mikejurka] for reporting)
- (Export) Exception due to incorrect property ID usage
- JSON parse tests
- Added missing Job variant for users of the Jobs package
- `GltfBoundsAsset` now has correct `sceneInstance` and `currentSceneId` properties
- Documentation: Fixed and improved export via script section (#270)
- Removed precautious error message after testing real world example (#268)

## [4.4.5] - 2021-11-01

### Fixed
- Error when animation package is not enabled (#267)

## [4.4.4] - 2021-10-28

### Fixed
- Build compiler error about missing variable (#265)

## [4.4.3] - 2021-10-27

### Fixed
- Release build only compiler errors

## [4.4.2] - 2021-10-27

### Fixed
- Offset of accessor into buffer was incorrect for some scalar accessors (#262)

## [4.4.1] - 2021-10-27

### Fixed
- .NET 4.6 compiler issue (#261)

## [4.4.0] - 2021-10-27

### Added
- Experimental glTF Editor Export (under main menu `File > Export` and via API `GLTFast.Export.GameObjectExport`; #249)
- Support for meshopt compressed glTFs (EXT_meshopt_compression; #106)
- *Generate Lightmap UVs* option in the glTF import inspector lets you create a secondary texture coordinate set (similar to the Model Import Settings from other formats; #238)
- Generic `ICodeLogger` methods that don't require a `LogCode`

### Changed
- Raised required Unity version to 2019.4.7f1 (fixes Burst 1.4 compiler issue #252). If you're on 2019.x, make sure to update to the latest LTS release!
- Less GC due to `CollectingLogger` creating the item list on demand

## [4.3.4] - 2021-10-26

### Added
- Option to turn off Editor import by adding `GLTFAST_EDITOR_IMPORT_OFF` to the project's *Scripting Define Symbols* in the *Player Settings* (#256)

### Fixed
- Import of glTFs with no meshes (#257)

## [4.3.3] - 2021-10-15

### Fixed
- Corrected mesh bounds (calculated from accessor's min/max)
- No errors when importing empty scenes
- Removed redundant code

## [4.3.2] - 2021-10-13

### Added
- Completed quantization by supporting UInt8/UInt16 skin bone weights
### Changes
- If `skin.skeleton` is properly set, `SkinnedMeshRendererRoot`'s root bone property will be assigned accordingly
- Major animation loading performance improvements by inlining and optimizing hot for-loops

### Fixed
- Animation sampler properly defaults to `LINEAR` interpolation in case it is not specified
- Correct `LINEAR` animation interpolation due to fixing tangent calculation
- Correct `LINEAR` animation interpolation on (quaternion) rotations by ensuring shortest path (#250, #251)
- Unlit built-in render pipeline materials have correct texture transform again
- Correct quantized morph target shading by fixing (not normalizing) delta normals and delta tangents

## [4.3.1] - 2021-09-14

### Changed
- Point clouds (POINTS primitive mode) are approved now - removed error log

### Fixed
- Avoid Burst compiler issue on Windows by using `UnsafeUtility.MemCpy` over `System.Buffer.MemoryCopy` (#245)

## [4.3.0] - 2021-09-10

### Added
- Multiple texture related import settings (thanks [@aurorahcx][aurorahcx] for #215)
  - `generateMipMaps` (default is false)
  - `defaultMinFilterMode` (minification; default is linear)
  - `defaultMagFilterMode` (magnification; default is linear)
  - `anisotropicFilterLevel` (default is 1)
- Unit tests for all vertex/index buffer conversion jobs

### Changed
- Performance improvement due to enabling Burst compiler on all vertex/index buffer conversion jobs
- `defaultMinFilterMode` was changed to `Linear` (from `NearestMipmapLinear`). This way textures will fall back to bilinear filtering (`FilterMode.Bilinear`) when it was not specified explicitly.
- `GameObject` specifics were moved from `GltfAssetBase` into `GltfAsset` in preparation for ECS
- Exposing glTFast assembly internals to glTF-test-framework

### Fixed
- Memory corruption when using unsigned byte positions or signed short UVs
- Set `_METALLICGLOSSMAP` and `_OCCLUSION` keywords in material editor on texture import (thanks [@hybridherbst][hybridherbst] for #237)
- Missing name on some textures
- Incorrect rotations from signed byte quaternions
- Incorrect UVs when using unsigned byte or signed/unsigned short texture coordinates
- Incorrect values converting signed byte encoded tangents
- Correct specular-glossiness materials in spite of (correct or incorrect) presence of metallic-roughness properties (fixes #241)

## [4.2.1] - 2021-08-26

### Changed
- Added Burst as dependency

### Fixed
- Improved handling corrupted glTF files (thanks [@zharry][zharry] for #230)
- Loading [Ready Player Me][ReadyPlayerMe] avatars with unsupported node extension (`MOZ_hubs_components`)
- Loading glTF-binary files that have no buffers or an empty binary chunk (#227)
- Crash and incorrect mesh clustering caused in `MeshPrimitive.Equals` (#224)
- Compiler error when Burst is not installed (#222)

## [4.2.0] - 2021-07-16

### Added
- Support for morph targets / blend shapes (#8)
- Support for animated morph targets / blend shapes
- Support for sparse accessors (morph targets and vertex positions only for now)
- Safe build option for more robust loading (`GLTFAST_SAFE` scripting define)
- Burst as dependency

### Changed
- Minor primitive GameObject name change. `GltfImport` is now fully responsible for `GameObject` names in order to ensure consistency between animation paths and model hierarchy.
- glTF importer inspector
  - Removed "Node Name Method" option from glTF importer inspector. It still an option at run-time, but is always `OriginalUnique` at design-time imports.
  - `Animation` setting is disabled if built-in package animation is disabled
- For better clarity, changed type of `Sampler` properties `minFilter`, `magFilter`, `wrapS` and `wrapT` from into to enum types and added tests
- Optional dependencies
  - [KtxUnity][KtxUnity]: raised required version to 1.1.0
  - [DracoUnity][DracoUnity]: raised required version to 3.1.0

### Fixed
- Works again with built-in package animation disabled (thanks [@Bersaelor][Bersaelor] for #204)
- Resolve dot segments ("." and "..") in URIs according to RFC 3986, section 5.2.4 (fixes #213)
- Corrected vertex attribute order when loading meshes with both texture coordinates and vertex colors
- Added some sanity checks

## [4.1.0] - 2021-07-06

### Added
- Import setting to create non-legacy animation clips (thanks [@hybridherbst][hybridherbst] for #196)
- Support for two texture coordinate sets in materials (URP, HDRP and Built-in; fixes #34)
- Support for individual texture transform per texture type (URP, HDRP and Built-in)
- Support for occlusion maps on specular-glossiness materials (extension KHR_materials_pbrSpecularGlossiness)

### Fixed
- Editor import: Separate textures are only referenced in AssetDatabase (not re-added)
- Warnings due to conflicting script file names `Animation.cs` and `Camera.cs` (#198)

## [4.0.1] - 2021-06-10

### Changed
- Renamed `GLTFast.ILogger` to `GLTFast.ICodeLogger` to avoid confusion with `UnityEngine.ILogger`

### Fixed
- Null pointer dereference exception on `accessorData` (thanks [@hybridherbst][hybridherbst])
- Corrected flipped texture transform for KTX texture (#176)

## [4.0.0] - 2021-05-21

### Added
- Import glTF files at design-time in the Editor
- Custom inspector for imported glTF files, featuring import log messages
- `ImportSettings` can be provided to `GltfImport.Load` (optionally) to customize the loading behavior (quite limited at the moment, but gives room to grow)
  - `ImportSettings.nodeNameMethod` to allow customizing Node/GameObject naming convention
- `IGltfReadable` interface for `GltfImporter`
- Import and instantiation logging customization (see `ILogger`). Allows users to analyze log messages and/or opt out of logging all messages to the console (which is still done by default if you're using `GltfAsset`).
- Scene support. glTF can contain multiple scenes and now it is possible to instantiate them selectively
  - `GltfImport.InstantiateMainScene` to create an instance of the main scene (or nothing if the `scene` is not set; following the glTF 2.0 specification)
  - `GltfImport.InstantiateScene` to create an instance of a specific scene
- GPU instancing via [`EXT_mesh_gpu_instancing` glTF extension](https://github.com/KhronosGroup/glTF/blob/master/extensions/2.0/Vendor/EXT_mesh_gpu_instancing/README.md) (#107).
- Camera support (via `IInstantiator.AddCamera`; #12)

### Changed
- Coordinate space conversion from glTF's right-handed to Unity's left-handed system changed. Please see the [upgrade guide](xref:doc-upgrade-guides#upgrade-to-4x) for details and the motivation behind it.
- Nodes' names are made unique (within their hierarchical position) by supplementing a continuous number. This is required for correct animation target lookup and import continuity.
- `IInstantiator.AddPrimitive` extended parameter `first` (`bool`; true for the first primitive) to primitiveNumeration (`int`; counting upwards from zero). This allows for creating unique GameObject names.
- Renamed the main class `GltFast` to `GltfImporter` to properly reflect its purpose. There is a fallback `GltFast` class for backwards compatibility
- Renamed `GltfImporter.Destroy` to `GltfImporter.Dispose` to have more consistent naming similar to native containers
- `IMaterialGenerator` overhaul that allows more flexible generation of materials (ahead of time)
  - `GenerateMaterial` instead of passing on all require data (like full texture arrays), data has to be fetched from the `GltfImporter`/`IGltfReadable`.
- `IInstantiator.AddPrimitive`: Instead of `Material` the IDs/indices of materials are provided and the materials themselves have to be fetched from the `IGltfReadable`/`GltfImporter` (allowing more flexible usage)
- `GltfImport.InstantiateGltf` (instantiates all scenes at once) is marked obsolete in favour of `InstantiateMainScene` and `InstantiateScene`
- Performance improvement: `NativeArray` buffers are not created copying memory. Instead they are created from pinned managed byte arrays. This should have some positive effect on binary glTFs with Draco meshes and KTX textures.
- Update to [DracoUnity 3.0.0](https://github.com/atteneder/DracoUnity/releases/tag/v3.0.0)
### Removed
- Runtime tests. They were moved into a [dedicated test package](https://github.com/atteneder/gltf-test-framework).

## [3.3.1] - 2021-05-21

### Fixed
- `GltfBoundsAsset` create just one instances (was two before; fixes #182)

## [3.3.0] - 2021-05-19

### Added
- Support for alpha modes `BLEND` and `MASK` on unlit materials (thanks [Sehyun av Kim](https://github.com/avseoul) for #181; fixes #180)

### Fixed
- Ignore / don't show errors when newer DracoUnity versions with incompatible API are installed

## [3.2.1] - 2021-05-05

### Fixed
- Properly freeing up memory of animation clips
- `GameObjectBoundsInstantiator` correctly calculates bounds for scenes that contain multi-primitive meshes (fixes #173)
- Corrected linear/gamma sampling whenever texture index does not equal image index (fixes #172)

## [3.2.0] - 2021-04-13

### Added
- Support for animations via Unity's legacy animation system (`Animation` component; #124)

### Fixed
- Image format is properly detected from URIs with HTTP queries (thanks [JonathanB-Vobling](https://github.com/JonathanB-Vobling) for #160; fixes #158)
- Unlit shaders are now correctly assigned for double-sided variants (thanks [@hybridherbst][hybridherbst] for #163)
- Sample code for custom defer agent is now thread safe (fixes #161)
- Meshes with two UV sets and vertex colors now work (fixes #162)

## [3.1.0] - 2021-03-16

### Added
- Unlit alpha blended ShaderGraph variants (thanks [@hybridherbst][hybridherbst] for #144)
- Support for unsigned byte joint indices

### Changed
- Accelerated loading meshes by obtaining and setting bounds from accessors min/max values instead of recalculating them
- Improved log message when DracoUnity/KtxUnity packages are missing
- Restored/simplified `GLTFast.LoadGltfBinary`, allowing users to load glTF binary files from byte arrays directly (also added documentation; fixes #148)

### Fixed
- Texture offset/tiling values don't get lost when switching shaders (thanks [@hybridherbst][hybridherbst] for #140)
- Correct vertex colors for RGB/unsigned short, RGBA/unsigned short and RGBA/unsigned byte. (thanks [@camogram](https://github.com/camogram) for #139)
- Error when trying to set texture offset/scale but material doesn't have _MainTex property (thanks [@hybridherbst][hybridherbst] for #142)
- Crash when trying to combine meshes created by glTFast by setting proper sub-mesh vertex count (fixes #100)

## [3.0.2] - 2021-02-07

### Changed
- Had to bring back `GltfAsset.isDone` for render tests

### Fixed
- WebGL loading by not using unsupported `System.Threading.Task.Run` (fixes #131)
- Escaped, relative buffer/texture URIs now work on local file system consistently
- Rendertests work again

## [3.0.1] - 2021-02-04

### Added
- Error message when a UV set other than the first one is used (is unsupported; see issue #34)
- Unit test for loading all models once (good for quick checks in comparison to performance tests, which take very long)

### Fixed
- No more exception on models with `KHR_materials_variants` glTF extension ([not supported](https://github.com/atteneder/glTFast/issues/112) yet)
- Compiler errors in Tests assembly due to inconsistent/incomplete class names/namespaces changes

## [3.0.0] - 2021-02-04

### Changed
- Moved `SampleSet` related code into dedicated Assembly, so it can be used in unit tests as well client applications (but doesn't have to).

### Fixed
- Build size optimization: Physics package is not required anymore (`GltfBoundsAsset` won't work as expected in that case)
- Build size optimization: Removed usage of `System.Linq`
- Removed compiler warnings (in case KtxUnity is missing)
- KtxUnity required version >=1.0.0
- DracoUnity required version >=1.4.0

## [3.0.0-preview] - 2021-02-01

### Changed
- Converted API and internals to async/await. This is more convenient in some cases and eases future optimizations.
- Performance improvements
  - Non-trivial JSONs are parsed in a thread now
  - More consistent frame rates due to task duration estimation in various places along the loading code
  - Embed base 64 buffers are decoded in a thread now
  - Less memory usage (and likely faster) du to Jpeg and PNG textures being loaded non-readable (if possible)

## [2.6.0] - 2021-01-31

### Added
- Support for performance benchmark package

### Fixed
- Unit tests are working in builds again (not just in the Editor)

## [2.5.1] - 2021-01-22

### Changed
- Renamed glTF shader graph properties to match Unity Lit/BuiltIn Standard shader properties. Switching shaders preserves more attributes this way.

### Fixed
- Consistent casing in shader graph names
- Apply material's occlusion strength properly
- Removed artifacts on double sided opaque materials
- Properly clean up volatile download dictionaries
- Build compilation when targeting URP/HDRP

## [2.5.0] - 2020-12-14

### Added
- Ported partial support for transmission materials to URP/HDRP 7.x
- Improved/alternative transmission mode for Universal Render Pipeline that kicks in if `Opaque Texture` is enabled in URP settings

## [2.4.0] - 2020-12-10

### Added
- Partial support for transmission materials in built-in render pipeline (extension [KHR_materials_transmission](https://github.com/KhronosGroup/glTF/tree/master/extensions/2.0/Khronos/KHR_materials_transmission); see #111 for details)

### Changed
- Performance improvement: Avoid redundant Shader.Find invocations by making cached shader references static
- Built-In shaders can customized now by overriding `BuiltInMaterialGenerator.FindShader*` methods

### Fixed
- Unlit double sided shader graph materials

## [2.3.0] - 2020-12-04

### Added
- Support for [Shader Graph](https://unity.com/shader-graph) based Render Pipelines including [Universal Render Pipeline (URP)](https://unity.com/srp/universal-render-pipeline) and [High Definition Render Pipeline (HDRP)](https://unity.com/srp/High-Definition-Render-Pipeline) (#41,#42)
- Material inspector: texture rotation value (in degrees) for both built-in and Shader Graph materials
- `GltfAsset` now provides a `streamingAssets` option (default is off), for loading relative paths from the [StreamingAssets](https://docs.unity3d.com/Manual/StreamingAssets.html) folder
- `GameObjectBoundsInstantiator`, a derived version of `GameObjectInstantiator` that calculates the glTF's axis-aligned bounding box
- `GltfBoundsAsset`, a derived version of `GltfAsset` that adds a BoxCollider to instantiations
- Render Tests: Minimize chance of visual regression by checking import results against reference images

### Changed
- Texture transform UV rotation: Using standard `_ST` property (Scale-Translation) by default. When rotation is enabled, scale values act as m00/m11 values of 2-by-2 rotation/scale matrix and are supplemented by two rotation values (for m01/m10).
- Textures that fail to load don't cause the whole loading process to fail (thanks @Bersaelor for #117)
- Unit Tests: Sample model list is now part of GltfSampleSet objects and not loaded from text file anymore

### Fixed
- Removed shader compiler warnings for built-in shaders
- Removed compiler warnings in Unity 2020.1/2020.2
- Changes to materials (in custom shader GUI) are saved now
- Invalid matrix error. ValidTRS reports error in matrix data that does look correct (fixes #116)
- Removed potential memory leak warnings by allocating all buffers permanently (#115)

## [2.2.0] - 2020-11-20

### Added
- Blend mode can be set in inspector for glTF materials via custom ShaderGUI (thanks @camnewnham for #89)
- Option to make all mesh data readable via `GLTFAST_KEEP_MESH_DATA` scripting define (alternative to #86)
- Better support for URLs without file extension. glTF type (JSON or binary) is derived from HTTP Content-Type header, if present. (thanks @camnewnham for #87)
- Method `GltFast.LoadGltfBinary` to load .glb files from byte arrays is public now (#81)

### Changed
- Switched internal URL type from `string` to `Uri`
- Dependency on com.unity.mathematics was added (for matrix decomposition; see fix below)

### Fixed
- Unit tests updated to latest glTF-Sample-Models
- Absolute URI in external resources
- Special characters in URL (#79)
- Corner-case matrix decomposition errors (#99)
- Missing `Shader` results in error message instead of exception (#88)

## [2.1.0] - 2020-10-25

### Changed
- Updated KTX/Basis Texture Unity Package to 0.8.x
- The KTX specification changed (from ~draft20 to pr-draft2), thus older KTX files cannot be loaded anymore.

### Added
- Support for KTX specification 2.0 pr-draft2 (fixes #16)
- Support for Basis Universal UASTC super-compression mode (higher quality)

## [2.0.0] - 2020-09-05

### Added
- Support for skinning
- Instantiation can now be customized via injection

### Changed
- Complete refactor to allow more optimization by using Unity's new Mesh API (introduced in 2019.1)
- Required Unity version was raised to 2019.1 or newer

## [1.2.0] - 2020-09-05

### Added
- Material generator (IMaterialGenerator) is now properly exposed and can be injected ( thanks [@p-skakun](https://github.com/p-skakun) for #80 )

### Changed
- Reduced memory usage by uploading mesh data instantly and make it no longer readable

## [1.1.1] - 2020-05-28

### Fixed
- Unlit shader now works with vertex colors

## [1.1.0] - 2020-05-25

### Added
- `GltFast.LoadingDone` state property indicates if loading routine has finished
- `GltfAssetBase`, a minimum asset component for manual loading via script
- `GetMaterial` interface, to retrieved imported materials by index.

### Changed
- Added loading state sanity checks to instantiation

### Fixed
- Loading glTFs with materials only (no scene/geometry)
- Normal texture scale is applied correctly now

## [1.0.1] - 2020-04-29

### Added
- Abstract interface `IDownloadProvider` let's users implement custom download behavior (useful for authentication or caching)
- Added `CustomHeaderDownloadProvider`, a reference implementation that downloads glTF's files with custom HTTP headers

### Changed
- Removed support for obsolete draft extensions `KHR_texture_cttf` and `KHR_image_ktx2`

### Fixed
- Correct (brighter) colors due to color-space conversion (conversion from linear to gamma before applying to material)
- Correct shading in linear color space projects due to correct (linear) sampling of normal, occlusion and metallic-roughness maps
- Memory leak: free up volatile array `imageFormats`

## [1.0.0] - 2020-03-13

### Changed
- Support for Draco mesh compression is now optional (install DracoUnity package to enable it)
- Support for KTX2/Basis Universal textures is now optional (install KtxUnity package to enable it)
- Faster mesh creation due to using the advanced Mesh API on Unity 2019.3 and newer.

## [0.11.0] - 2020-03-07

### Added
- Support for texture samplers' wrapping mode
- Support for texture samplers' filter modes (partial; see [issue](/atteneder/glTFast/issues/61))

### Changed
- Increased performance due to more balanced threading by making all C# Jobs parallel
- Refactored loading behavior
  - Main loading class does not interfere with it's IDeferAgent anymore. It just follows its order.
  - `GltfAsset` now has a `loadOnStartup` flat to disable automatic loading
  - `GltfAsset.onLoadComplete` now also returns its `GltfAsset` instance for convenience

### Fixed
- Redundant Load calls when using `UninterruptedDeferAgent`

## [0.10.2] - 2020-02-26

### Changed
- Normals and tangents (if not present) are only calculated if the assigned material actually requires them.

## [0.10.1] - 2020-02-24

### Added
- Experimental KTX / Basis Universal support was merged (off by default)

### Fixed
- Proper error handling invalid URL/path
- Improved glTF-binary URL extension detection
- Correct index order for line strip primitives (#59)

## [0.10.0] - 2020-02-22

### Added
- Support for Universal Windows Platform (not verified/tested myself)

### Changed
- Refactored GltFast class to control loading coroutine in an effort to make usage and future port to async easier.
- Optimization: Data loading is now based on accessors (rather than primitives). This reduces redundant loading jobs wherever accessors are used across primitives.
- Optimization: Primitives of a mesh, that share vertex attributes now become sub-meshes of one Unity Mesh. This reduces memory usage and creates less Renderers/GameObjects.
- glTF type (JSON or binary) is now auto-detected based on file name extension. Removed obsolete `GlbAsset`. This was done so `GltfAsset` can be derived off more flexible.

## [0.9.0] - 2020-02-02

### Added
- Support for quantized mesh data via `KHR_mesh_quantization` extension

### Changed
- UV space conversion now happens per UV coordinate (not negatively scaled via texture tiling anymore). This helped to fix tangent calculation.
- glTF standard shaders now have a cull mode, allowing them to be double-sided. The now obsolete `Double` variants were removed (thanks to Ben Golus for support)

### Fixed
- Certified correct normal mapping by making normals, UVs and tangents consistent
- Double sided material fixes

## [0.8.1] - 2019-12-05

### Fixed
- Shader compilation error on Vulkan/GLES3

## [0.8.0] - 2019-12-05

### Added
- Support for texture transform (extension KHR_texture_transform)
- Support for double sided materials
- Support for data URI / embedded buffers and images
- Support for vertex colors in materials
- Support for implicit/undefined primitive indices
- Experimental support for primitive modes points, lines, line strip and line loop

### Changed
- Using custom glTF shaders instead of Unity Standard shaders. This speeds up occlusion and roughness/metallic texture loading since they don't have to be converted at runtime anymore.

### Fixed
- Factor and texture (for metallic-roughness and specular-glossiness) are now multiplied as defined in spec.
- Unlit materials now support baseColorTexture and texture transforms

## [0.7.1] - 2019-11-29

### Fixed
- glTF binary with Draco compression (decoding error due to invalid buffer view access)
- Legacy .NET speed regression

## [0.7.0] - 2019-11-22

### Added
- Unity backwards compatibility (tested with 2018.2 with .NET 3.5)

### Changed
- Removed job-less support
- The node or primitive GameObjects now have their mesh's name, if there is no node name provided

### Fixed
- Correct transforms and coordinate space. The glTF scene's root node is not scaled negative in any axis anymore
- Texture default wrap mode is repeat (not set to clamp anymore)

## [0.6.0] - 2019-11-15

### Added
- Support for unlit materials (KHR_materials_unlit extension)
- Support for specular-glossiness type materials (KHR_materials_pbrSpecularGlossiness extension)

### Fixed
- Fixed broken assembly references by switching to non-GUID refs (thanks Stephen Gower for pointing it out)
- Metallic-Roughness texture not working. Now they are created only after their source was properly loaded.

## [0.5.0] - 2019-09-14

### Added
- Draco mesh compression support

### Fixed
- Report unsupported glTF extensions and gracefully fail if a required extension is not supported.

## [0.4.0] - 2019-07-24

### Changed
- Transformed Project into a Unity Package, which can easily be installed via Package Manager

## [0.3.0] - 2019-06-30

### Added
- Threaded glTF loading via Unity Job System

### Changed
- Update to Unity 2019.1.7f1
- Formatted ChangeLog markdown file

## [0.2.0] - 2019-02-22

### Added
- Support for regular JSON glTFs (non-binary)

## [0.1.0] - 2018-11-27

### Added
- First pre-release

## [0.0.5] - 2018-09-02

### Fixed
- Support for meshes with more than 65k vertices.

## [0.0.4] - 2018-06-20

### Fixed
- free up memory when destroying content

## [0.0.3] - 2018-05-29

### Added
- Added support for interleaved vertex data

## [0.0.2] - 2018-05-20

### Added
- added support for 3 component vertex colors (rgb without alpha)
- added support for uint16 vertex colors

### Fixed
- fixed metallic roughness texture usage (workaround)
- fixed occlusion texture usage (workaround)

## [0.0.1] - 2018-05-12

### Added
- initial version

[Entities1.0]: https://docs.unity3d.com/Packages/com.unity.entities@1.0
[Entities1.2]: https://docs.unity3d.com/Packages/com.unity.entities@1.2
[Collections]: https://docs.unity3d.com/Packages/com.unity.collections@latest/
[JobsPkg]: https://docs.unity3d.com/Packages/com.unity.jobs@latest/
[KtxUnity]: https://github.com/atteneder/KtxUnity
[Ktx for Unity]: https://docs.unity3d.com/Packages/com.unity.cloud.ktx@latest/
[DanDovi]: https://github.com/DanDovi
[Draco for Unity]: https://docs.unity3d.com/Packages/com.unity.cloud.draco@latest
[DracoUnity]: https://github.com/atteneder/DracoUnity
[PolySpatialVisionOS]: https://docs.unity3d.com/Packages/com.unity.polyspatial.visionos@latest/
[meshoptimizer mesh compression for Unity]: https://docs.unity3d.com/Packages/com.unity.meshopt.decompress@latest/
[aurorahcx]: https://github.com/aurorahcx
[Battlehub0x]: https://github.com/Battlehub0x
[Bersaelor]: https://github.com/Bersaelor
[EricBeetsOfficial-Opuscope]: https://github.com/EricBeetsOfficial-Opuscope
[Hexer611]: https://github.com/Hexer611
[Holo-Krzysztof]: https://github.com/Holo-Krzysztof
[Hugo-Didimo]: https://github.com/Hugo-Didimo
[hybridherbst]: https://github.com/hybridherbst
[krisrok]: https://github.com/krisrok
[Kushulain]: https://github.com/Kushulain
[mikejurka]: https://github.com/mikejurka
[ReadyPlayerMe]: https://readyplayer.me
[rt-nikowiss]: https://github.com/rt-nikowiss
[sandr01d]: https://github.com/sandr01d
[NyxStudio]: https://github.com/NyxStudio
[zharry]: https://github.com/zharry
[weichx]: https://gist.github.com/weichx
[wonkee-kim]: https://github.com/wonkee-kim
