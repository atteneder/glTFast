# Features

## Workflows

|          | Runtime | Editor (design-time)
|----------| ------ | ------
| | |
| **GameObject**
| Import   | ✅️ | ✅
| Export   | <sup>1</sup>☑️ | <sup>1</sup> ☑️
| | |
| **Entities (see [DOTS](#data-oriented-technology-stack))**
| Import   | [☑️](#data-oriented-technology-stack) | `n/a`
| Export   |  | `n/a`

<sup>1</sup>: Experimental. Core features missing

## Core glTF&trade; features

The glTF 2.0 specification is fully supported, with only a few minor remarks.

| | Import | Export
|------------| ------ | ------
| **Format**
|glTF (.gltf) | ✅ | ✅
|glTF-Binary (.glb) | ✅ | ✅
| | |
| **Buffer**
| External URIs | ✅ | ✅
| GLB main buffer | ✅ | ✅
| Embed buffers or textures (base-64 encoded within JSON) | ✅ |
| [meshoptimizer compression][MeshOpt] (via [package][MeshOptPkg])| ✅ |
| | |
| **Basics**
| Scenes | ✅ | ✅
| Node hierarchies | ✅ | ✅
| Cameras | ✅ | ✅
| | |
| **Images**
| PNG | ✅ | ✅
| Jpeg | ✅ | ✅
| KTX&trade; with Basis Universal compression (via [KtxUnity]) | ✅ |
| | |
| **Texture sampler**
| Filtering  | ✅ with [limitations](./KnownIssues.md) | ✅ with [limitations](./KnownIssues.md) |
| Wrap modes | ✅ | ✅ |
| | |
| **Materials Overview** (see [details](#materials-details))
| [Universal Render Pipeline (URP)][URP] | ✅ | ☑️ |
| [High Definition Render Pipeline (HDRP)][HDRP] | ✅ | ☑️ |
| Built-in Render Pipeline | ✅ | ☑️ |
| | |
| **Topologies / Primitive Types**
| TRIANGLES | ✅ | ✅
| POINTS | ✅ | ✅
| LINES | ✅ | ✅
| LINE_STRIP | ✅ | ✅
| <sup>1</sup>LINE_LOOP | ✅ | ✅
| TRIANGLE_STRIP |  |
| TRIANGLE_FAN |  |
| Quads | `n/a` | ✅ via triangulation
| | |
| **Meshes**
| Positions | ✅ | ✅
| Normals | ✅ | ✅
| Tangents | ✅ | ✅
| Texture coordinates / UV sets | ✅ | `?`
| Three or more texture coordinates / UV sets | <sup>2</sup>☑️ | `?`
| Vertex colors | ✅ | `?`
| Draco&trade; mesh compression (via [DracoForUnity]) | ✅ | ✅
| Implicit (no) indices | ✅ |
| Per primitive material | ✅ | ✅
| Joints (up to 4 per vertex) | ✅ |
| Weights (up to 4 per vertex) | ✅ |
| | |
| **Morph Targets / Blend Shapes**
| Sparse accessors | <sup>3</sup> ✅ |
| [Skins][Skins] | ✅ |
| | |
| **Animation**
| via legacy Animation System | ✅ |
| via Playable API ([issue][AnimationPlayables]) |  |
| via Mecanim ([issue][AnimationMecanim]) |  |

<sup>1</sup>: Untested due to lack of demo files.

<sup>2</sup>: Up to eight UV sets can imported, but *Unity glTFast* shaders only support two (see [issue][UVsets]).

<sup>3</sup>: Not on all accessor types; morph targets and vertex positions only

## Extensions

### Official Khronos&reg; extensions

| | Import | Export
|------------| ------ | ------
| | |
| **Khronos**
| KHR_draco_mesh_compression | ✅ | ✅
| KHR_materials_pbrSpecularGlossiness | ✅ |
| KHR_materials_unlit | ✅ | ✅
| KHR_texture_transform | ✅ | ✅
| KHR_mesh_quantization | ✅ |
| KHR_texture_basisu | ✅ |
| KHR_lights_punctual | ✅ | ✅
| KHR_materials_clearcoat | ✅ | ✅
| KHR_materials_sheen | [ℹ️][Sheen] |
| KHR_materials_transmission | [ℹ️][Transmission] |
| KHR_materials_variants | [ℹ️][Variants] |
| KHR_materials_ior | [ℹ️][IOR] |
| KHR_materials_specular | [ℹ️][Specular] |
| KHR_materials_volume | [ℹ️][Volume] |
| KHR_xmp_json_ld |️ |
| | |
| **Vendor**
| <sup>1</sup>EXT_mesh_gpu_instancing | ✅ |
| EXT_meshopt_compression | ✅ |
| EXT_lights_image_based | [ℹ️][IBL] |

<sup>1</sup>: Without support for custom vertex attributes (e.g. `_ID`)

Not investigated yet:

- AGI_articulations
- AGI_stk_metadata
- CESIUM_primitive_outline
- MSFT_lod
- MSFT_packing_normalRoughnessMetallic
- MSFT_packing_occlusionRoughnessMetallic

 Will not become supported (reason in brackets):

- KHR_xmp (archived; prefer KHR_xmp_json_ld)
- KHR_techniques_webgl (archived)
- ADOBE_materials_clearcoat_specular (prefer KHR_materials_clearcoat)
- ADOBE_materials_thin_transparency (prefer KHR_materials_transmission)
- EXT_texture_webp (prefer KTX/basisu)
- FB_geometry_metadata (prefer KTX_xmp)
- MSFT_texture_dds (prefer KTX/basisu)

### Custom extras and extensions

Optional `extras` and `extensions` object properties are supported. glTFast uses Newtonsoft JSON parser to access these additional properties.

See [glTFast Add-on API](UseCaseCustomExtras.md) for an example to import the `extras` property in a gltf asset.

## Materials Details

### Material Import

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
| Clear coat                    | ☑️<sup>3</sup>  | ✅  | [⛔️][ClearCoat] |
| Sheen                         | [ℹ️][Sheen] | [ℹ️][Sheen] | [⛔️][Sheen] |
| Transmission                  | [☑️][Transmission]<sup>4</sup> | [☑️][Transmission]<sup>5</sup> | [☑️][Transmission]<sup>5</sup> |
| Variants                      | [ℹ️][Variants] | [ℹ️][Variants] | [ℹ️][Variants] |
| IOR                           | [ℹ️][IOR]      | [ℹ️][IOR]      | [⛔️][IOR]      |
| Specular                      | [ℹ️][Specular] | [ℹ️][Specular] | [⛔️][Specular] |
| Volume                        | [ℹ️][Volume]   | [ℹ️][Volume]   | [⛔️][Volume]   |
| Point clouds                  |      |     | Unlit only |

<sup>1</sup>: Physically-Based Rendering (PBR) material model

<sup>2</sup>: Two sets of texture coordinates (as required by the glTF 2.0 specification) are supported, but not three or more ([issue][UVSets])

<sup>3</sup>: Only supports Universal Render Pipeline versions >= 12.0; Only coat mask and smoothness are supported, other coat related properties, such as coat normal, are not supported

<sup>4</sup>: There are two approximation implementations for transmission in Universal render pipeline. If the Opaque Texture is enabled (in the Universal RP Asset settings), it is sampled to provide proper transmissive filtering. The downside of this approach is transparent objects are not rendered on top of each other. If the opaque texture is not available, the common approximation (see <sup>4</sup> below) is used.

<sup>5</sup>: Transmission in Built-In and HD render pipeline does not support transmission textures and is only 100% correct in certain cases like clear glass (100% transmission, white base color). Otherwise it's an approximation.

### Material Export

Material export is currently only tested on the following shaders:

- Universal and High Definition render pipeline
  - `Lit`
  - `Unlit`
- Built-In render pipeline
  - `Standard`
  - `Unlit`

Other shaders might (partially) work if they have similar properties (with identical names).

| Material Feature              | URP<sup>1</sup> | HDRP<sup>2</sup> | Built-In<sup>3</sup> |
|-------------------------------|-----|------|----------|
| PBR Metallic-Roughness        | ✅ | ✅ | ✅ |
| PBR Specular-Glossiness       |  |  |  |
| Unlit                         | ✅ | ✅ | ✅ |
| Normal texture                | ✅ | ✅ | ✅ |
| Occlusion texture             | ✅ | ✅ | ✅ |
| Emission texture              | ✅ | ✅ | ✅ |
| Alpha modes OPAQUE/MASK/BLEND | ✅ | ✅ | ✅ |
| Double sided / Two sided      | ✅ | ✅ | ✅ |
| Vertex colors                 | `?` | `?` | `?` |
| Multiple UV sets              | `?` | `?` | `?` |
| Texture Transform             | ✅ | ✅ | ✅ |
| Clear coat                    | `n/a` | ✅ | `n/a` |
| Sheen                         | `?` | `?` | `n/a` |
| Transmission                  |  |  | `n/a` |
| Variants                      |  |  |  |
| IOR                           |  |  | `n/a` |
| Specular                      |  |  |  |
| Volume                        |  |  | `n/a` |

<sup>1</sup>: Universal Render Pipeline Lit Shader

<sup>2</sup>: High Definition Render Pipeline Lit Shader

<sup>3</sup>: Built-In Render Pipeline Standard and Unlit Shader

## Data-Oriented Technology Stack

> ⚠️ Note: DOTS is highly experimental and many features don't work yet. Do not use it for production ready projects!

Unity's [Data-Oriented Technology Stack (DOTS)][DOTS] allows users to create high performance gameplay. *Unity glTFast* has experimental import support for it.

Instead of traditional GameObjects, *Unity glTFast* will instantiate [Entities][Entities] and render them via [Entities Graphics][EntitiesGraphics].

Possibly incomplete list of things that are known to not work with Entities yet:

- Animation
- Skinning
- Morph targets
- Cameras
- Lights

### DOTS Setup

- Install the [Entities Graphics][EntitiesGraphics] package
- Use `GltfEntityAsset` instead of `GltfAsset`
- For customized behavior, use the `EntityInstantiator` instead of the `GameObjectInstantiator`

## Unity Version Support

*Unity glTFast* requires Unity 2020.1 or newer.

## Legend

- ✅ Fully supported
- ☑️ Partially supported
- ℹ️ Planned (click for issue)
- ⛔️ No plan to support (click for issue)
- `?`: Unknown / Untested
- `n/a`: Not available

## Trademarks

*Unity&reg;* is a registered trademark of [Unity Technologies][Unity].

*Khronos&reg;* is a registered trademark and *glTF&trade;* is a trademark of [The Khronos Group Inc][Khronos].

*KTX&trade;* and the KTX logo are trademarks of the [The Khronos Group Inc][Khronos].

*Draco&trade;* is a trademark of [*Google LLC*][GoogleLLC].

[AnimationMecanim]: https://github.com/atteneder/glTFast/issues/167
[AnimationPlayables]: https://github.com/atteneder/glTFast/issues/166
[ClearCoat]: https://github.com/atteneder/glTFast/issues/68
[DracoForUnity]: https://docs.unity3d.com/Packages/com.unity.cloud.draco@latest
[DOTS]: https://unity.com/dots
[Entities]: https://docs.unity3d.com/Packages/com.unity.entities@latest
[EntitiesGraphics]: https://docs.unity3d.com/Packages/com.unity.entities.graphics@latest
[GoogleLLC]: https://about.google/
[HDRP]: https://unity.com/srp/High-Definition-Render-Pipeline
[IBL]: https://github.com/atteneder/glTFast/issues/108
[IOR]: https://github.com/atteneder/glTFast/issues/207
[Khronos]: https://www.khronos.org
[KtxUnity]: https://docs.unity3d.com/Packages/com.unity.cloud.ktx@latest
[MeshOpt]: https://github.com/KhronosGroup/glTF/tree/main/extensions/2.0/Vendor/EXT_meshopt_compression
[MeshOptPkg]: https://docs.unity3d.com/Packages/com.unity.meshopt.decompress@0.1/manual/index.html
[Sheen]: https://github.com/atteneder/glTFast/issues/110
[Skins]: https://www.khronos.org/registry/glTF/specs/2.0/glTF-2.0.html#skins
[Specular]: https://github.com/atteneder/glTFast/issues/208
[Transmission]: https://github.com/atteneder/glTFast/issues/111
[Unity]: https://unity.com
[URP]: https://unity.com/srp/universal-render-pipeline
[UVsets]: https://github.com/atteneder/glTFast/issues/206
[Variants]: https://github.com/atteneder/glTFast/issues/112
[Volume]: https://github.com/atteneder/glTFast/issues/209
