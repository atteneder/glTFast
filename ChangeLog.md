# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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
