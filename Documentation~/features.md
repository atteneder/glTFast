# Features

- [x] Run-time import
- [x] Fast and small footprint JSON parsing
- [x] Multi-threading via C# job system
- [x] Design-time (Editor) import
- [ ] Export

## Core glTF features

The glTF 2.0 specification is fully supported, with only a few minor remarks.

<details><summary>Detailed list of glTF 2.0 core feature support</summary>

- [x] glTF (gltf + buffers + textures)
- [x] glTF binary (glb)

- [x] Scene
  - [x] Node hierarchy
  - [x] Camera
- [x] Buffers
  - [x] External URIs
  - [x] glTF binary main buffer
  - [x] Embed buffers or textures (base-64 encoded within JSON)
- [x] Images
  - [x] PNG
  - [x] Jpeg
  - [x] <sup>2</sup>KTX with Basis Universal super compression (via [KTX/Basis Texture Unity Package](https://github.com/atteneder/KtxUnity))
- [x] Materials (see section [Materials](#materials) for details)
  - [x] [Universal Render Pipeline (URP)][URP]
  - [x] [High Definition Render Pipeline (HDRP)][HDRP]
  - [x] Built-in Render Pipeline
- Primitive Types
  - [x] TRIANGLES
  - [x] <sup>1</sup>POINTS
  - [x] <sup>1</sup>LINES
  - [x] LINE_STRIP
  - [x] <sup>1</sup>LINE_LOOP
  - [ ] TRIANGLE_STRIP
  - [ ] TRIANGLE_FAN
- [x] Meshes
  - [x] Positions
  - [x] Normals
  - [x] Tangents
  - [x] Texture coordinates
  - [x] Vertex colors
  - [x] Draco mesh compression (via [Draco 3D Data Compression Unity Package](https://github.com/atteneder/DracoUnity))
  - [x] Implicit (no) indices
  - [x] Per primitive material
  - [x] Two texture coordinates / UV sets
    - [ ] Three or more texture coordinates / UV sets ([issue][UVsets])
  - [x] Joints (up to 4 per vertex)
  - [x] Weights (up to 4 per vertex)
- [x] Texture sampler
  - [x] Filtering (see ([limitations](#Known-issues)))
  - [x] Wrap modes
- [x] Morph targets
- [x] <sup>3</sup>Sparse accessors
- [x] [Skins][Skins] (sponsored by [Embibe](https://www.embibe.com))
- [x] Animation
  - [x] via legacy Animation System
  - [ ] via Playable API ([issue][AnimationPlayables])
  - [ ] via Mecanim ([issue][AnimationMecanim])

<sup>1</sup>: Untested due to lack of demo files.

<sup>2</sup>: Beta

<sup>3</sup>: Not on all accessor types; morph targets and vertex positions only

</details>

## Extensions

### Official Khronos extensions

- [x] KHR_draco_mesh_compression
- [x] KHR_materials_pbrSpecularGlossiness
- [x] KHR_materials_unlit
- [x] KHR_texture_transform
- [x] KHR_mesh_quantization
- [x] KHR_texture_basisu
- [ ] KHR_lights_punctual ([issue][PointLights])
- [ ] KHR_materials_clearcoat ([issue][ClearCoat])
- [ ] KHR_materials_sheen ([issue][Sheen])
- [ ] KHR_materials_transmission ([issue][Transmission])
- [ ] KHR_materials_variants ([issue][Variants])
- [ ] KHR_materials_ior ([issue][IOR])
- [ ] KHR_materials_specular ([issue][Specular])
- [ ] KHR_materials_volume ([issue][Volume])
- [ ] KHR_xmp

Will not be supported:

- KHR_techniques_webgl

### Vendor extensions

- [x] <sup>1</sup>EXT_mesh_gpu_instancing
- [ ] EXT_meshopt_compression ([issue][MeshOpt])
- [ ] EXT_lights_image_based ([issue][IBL])

<sup>1</sup>: Without support for custom vertex attributes (e.g. `_ID`)

Not investigated yet:

- AGI_articulations
- AGI_stk_metadata
- CESIUM_primitive_outline
- MSFT_lod
- MSFT_packing_normalRoughnessMetallic
- MSFT_packing_occlusionRoughnessMetallic

Will not be supported:

- ADOBE_materials_clearcoat_specular (prefer KHR_materials_clearcoat)
- ADOBE_materials_thin_transparency (prefer KHR_materials_transmission)
- EXT_texture_webp (prefer KTX/basisu)
- FB_geometry_metadata (prefer KTX_xmp)
- MSFT_texture_dds (prefer KTX/basisu)

## Materials

| Material Feature              | URP | HDRP | Built-In |
|-------------------------------|-----|------|----------|
| PBR<sup>1</sup> Metallic-Roughness        | ✅  | ✅   | ✅       |
| PBR<sup>1</sup> Specular-Glossiness       | ✅  | ✅   | ✅       |
| Unlit                         | ✅  | ✅   | ✅       |
| Normal texture                | ✅  | ✅   | ✅       |
| Occlusion texture             | ✅  | ✅   | ✅       |
| Emission texture              | ✅  | ✅   | ✅       |
| Alpha modes OPAQUE/MASK/BLEND | ✅  | ✅   | ✅       |
| Double sided / Two sided      | ✅  | ✅   | ✅       |
| Vertex colors                 | ✅  | ✅   | ✅       |
| Multiple UV sets              | ✅<sup>2</sup>  | ✅<sup>2</sup>   | ✅<sup>2</sup>       |
| Texture Transform             | ✅  | ✅   | ✅       |
| Clear coat                    | [ℹ][ClearCoat] | [ℹ][ClearCoat] | [❌][ClearCoat] |
| Sheen                         | [ℹ][Sheen] | [ℹ][Sheen] | [❌][Sheen] |
| Transmission                  | [✓][Transmission]<sup>3</sup> | [✓][Transmission]<sup>4</sup> | [✓][Transmission]<sup>4</sup> |
| Variants                      | [ℹ][Variants] | [ℹ][Variants] | [ℹ][Variants] |
| IOR                           | [ℹ][IOR]      | [ℹ][IOR]      | [❌][IOR]      |
| Specular                      | [ℹ][Specular] | [ℹ][Specular] | [❌][Specular] |
| Volume                        | [ℹ][Volume]   | [ℹ][Volume]   | [❌][Volume]   |

<sup>1</sup>: Physically-Based Rendering (PBR) material model

<sup>2</sup>: Two sets of texture coordinates (as required by the glTF 2.0 specification) are supported, but not three or more ([issue][UVSets])

<sup>3</sup>: There are two approximation implementations for transmission in Universal render pipeline. If the Opaque Texture is enabled (in the Universal RP Asset settings), it is sampled to provide proper transmissive filtering. The downside of this approach is transparent objects are not rendered on top of each other. If the opaque texture is not available, the common approximation (see <sup>4</sup> below) is used.

<sup>4</sup>: Transmission in Built-In and HD render pipeline does not support transmission textures and is only 100% correct in certain cases like clear glass (100% transmission, white base color). Otherwise it's an approximation.

Legend:

- ✅ Fully supported
- ✓ Supported partially
- ℹ Planned (click for issue)
- ❌ No plan to support (click to create issue)

## Known issues

- <sup>1</sup>Vertex accessors (positions, normals, etc.) that are used across meshes are duplicated and result in higher memory usage and slower loading (see [this comment](https://github.com/atteneder/glTFast/issues/52#issuecomment-583837852))
- <sup>1</sup>When using more than one samplers on an image, that image is duplicated and results in higher memory usage
- Texture sampler minification/magnification filter limitations (see [issue][SamplerFilter]):
  - <sup>1</sup>There's no differentiation between `minFilter` and `magFilter`. `minFilter` settings are prioritized.
  - <sup>1</sup>`minFilter` mode `NEAREST_MIPMAP_LINEAR` is not supported and will result in `NEAREST`.

<sup>1</sup>: A Unity API limitation.

[AnimationMecanim]: https://github.com/atteneder/glTFast/issues/167
[AnimationPlayables]: https://github.com/atteneder/glTFast/issues/166  
[ClearCoat]: https://github.com/atteneder/glTFast/issues/68
[HDRP]: https://unity.com/srp/High-Definition-Render-Pipeline
[IBL]: https://github.com/atteneder/glTFast/issues/108
[IOR]: https://github.com/atteneder/glTFast/issues/207
[MeshOpt]: https://github.com/atteneder/glTFast/issues/106
[newIssue]: https://github.com/atteneder/glTFast/issues/new
[PointLights]: https://github.com/atteneder/glTFast/issues/17
[SamplerFilter]: https://github.com/atteneder/glTFast/issues/61 
[Sheen]: https://github.com/atteneder/glTFast/issues/110
[Skins]: https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/README.md#skins
[Specular]: https://github.com/atteneder/glTFast/issues/208
[Transmission]: https://github.com/atteneder/glTFast/issues/111
[URP]: https://unity.com/srp/universal-render-pipeline
[UVsets]: https://github.com/atteneder/glTFast/issues/206
[Variants]: https://github.com/atteneder/glTFast/issues/112
[Volume]: https://github.com/atteneder/glTFast/issues/209
