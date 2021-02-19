# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased] - 
### Fixed
- Texture offset/tiling values don't get lost when switching shaders (#140 by [@hybridherbst](https://github.com/hybridherbst))

## [3.0.2] - 2020-02-07
### Changed
- Had to bring back `GltfAsset.isDone` for render tests
### Fixed
- WebGL loading by not using unsupported `System.Threading.Task.Run` (fixes #131)
- Escaped, relative buffer/texture URIs now work on local file system consistently 
- Rendertests work again

## [3.0.1] - 2020-02-04
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
- Support for Basis Universal UASTC supercompression mode (higher quality)

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
- Abstract interface `IDownloadProvider` let's users implement custom download behavior (useful for authentification or caching)
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
- Tranformed Project into a Unity Package, which can easily be installed via Package Manager

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

[0.3.0]: https://github.com/atteneder/glTFast/compare/v0.3.0...v0.2.0
[0.2.0]: https://github.com/atteneder/glTFast/compare/v0.2.0...v0.1.0
