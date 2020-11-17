# Features

- [x] Runtime import
- [x] Fast and small footprint JSON parsing
- [x] Multi-threading via C# job system
- [ ] Editor import
- [ ] Export

## Core glTF features

- [x] glTF (gltf + buffers + textures)
- [x] glTF binary (glb)

- [x] Scene
  - [x] Node hierarchy
  - [ ] Camera ([issue](../issues/12))
- [x] Buffers
  - [x] External URIs
  - [x] glTF binary main buffer
  - [x] Embed buffers or textures (base-64 encoded within JSON)
- [x] Images
  - [x] PNG
  - [x] Jpeg
  - [x] <sup>2</sup>KTX with Basis Universal super compression (via [KTX/Basis Texture Unity Package](https://github.com/atteneder/KtxUnity))
- [x] Materials
  - [x] Unity built-in pipeline
    - [x] PBR metallic-roughness
    - [x] PBR specular-glossiness (via extension)
    - [x] Unlit (via extension)
    - [x] Normal texture
    - [x] Occlusion texture
    - [x] Emission texture
    - [x] Metallic texture
    - [x] Roughness texture
    - [x] Alpha mode
    - [x] Double sided
    - [x] Vertex colors
    - [x] Emission(factor)
  - [ ] Universal Render Pipeline ([issue](../issues/41))
  - [ ] High Definition Render Pipeline ([issue](../issues/42))
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
  - [ ] Multiple texture coordinates sets ([issue](../issues/34))
  - [x] Joints (up to 4 per vertex)
  - [x] Weights (up to 4 per vertex)
- [x] Texture sampler
  - [x] Filtering (see ([limitations](#knownissues)))
  - [x] Wrap modes
- [ ] Morph targets ([issue](../issues/8))
  - [ ] Sparse accessors
- [x] [Skins](https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/README.md#skins) (sponsored by [Embibe](https://www.embibe.com))
- [ ] Animation

<sup>1</sup>: Untested due to lack of demo files.

<sup>2</sup>: Beta

## Extensions

### Official Khronos extensions

- [x] KHR_draco_mesh_compression
- [x] KHR_materials_pbrSpecularGlossiness
- [x] KHR_materials_unlit
- [x] KHR_texture_transform
- [x] KHR_mesh_quantization
- [x] <sup>1</sup>KHR_texture_basisu
- [ ] KHR_lights_punctual ([issue](../issues/17))
- [ ] KHR_materials_clearcoat ([issue](../issues/68))
- [ ] KHR_materials_sheen
- [ ] KHR_materials_transmission
- [ ] KHR_materials_variants
- [ ] KHR_xmp

<sup>1</sup>: Beta

Will not be supported:

- KHR_techniques_webgl

### Vendor extensions

- [ ] EXT_mesh_gpu_instancing ([issue](../issues/107))
- [ ] EXT_meshopt_compression ([issue](../issues/106))
- [ ] EXT_lights_image_based ([issue](../issues/108))

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

## <a name="knownissues">Known issues

- <sup>1</sup>Vertex accessors (positions, normals, etc.) that are used across meshes are duplicated and result in higher memory usage
- <sup>1</sup>When using more than one samplers on an image, that image is duplicated and results in higher memory usage
- Texture sampler minification/magnification filter limitations (see [issue](issues/61)):
  - <sup>1</sup>There's no differentiation between `minFilter` and `magFilter`. `minFilter` settings are prioritized.
  - <sup>1</sup>`minFilter` mode `NEAREST_MIPMAP_LINEAR` is not supported and will result in `NEAREST`.
- When building for WebGL with Unity 2018.1 you have to enable explicitly thrown exceptions (reason unknown - to be investigated)

<sup>1</sup>: A Unity API limitation.
