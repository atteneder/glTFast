# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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
