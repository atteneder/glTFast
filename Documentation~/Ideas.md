# glTFast Ideas

Ideas to improve this project

## Features

### glTF 2.0 Specification

Missing:

- Primitive types
  - TRIANGLE_STRIP
  - TRIANGLE_FAN

#### glTF 2.0 official extensions

- KHR_materials_clearcoat
- KHR_materials_sheen
- KHR_materials_transmission
- KHR_materials_variants
- KHR_lights_punctual
- KHR_xmp

- KHR_texture_basisu: XY-normal map support

#### glTF 2.0 vendor extensions

Interesting:

- EXT_meshopt_compression
- EXT_lights_image_based

Not investigated:

- AGI_articulations
- AGI_stk_metadata
- CESIUM_primitive_outline
- MSFT_lod
- MSFT_packing_normalRoughnessMetallic
- MSFT_packing_occlusionRoughnessMetallic

Not interesting:

- ADOBE_materials_clearcoat_specular (prefer KHR_materials_clearcoat)
- ADOBE_materials_thin_transparency (prefer KHR_materials_transmission)
- EXT_texture_webp (prefer KTX/basisu)
- FB_geometry_metadata (prefer KTX_xmp)
- MSFT_texture_dds (prefer KTX/basisu)

### Import Customization

- Provide physics collider instantiator example

#### Make extras property available

Many glTF entities like nodes can have custom data in form of an `extras` dictionary. It should be accessible from custom scripts (see #90).

#### Low-Level Interfaces

- Loading progress
  - meshes ready
  - materials/textures ready
  - animation ready
  - everything ready

#### Partial Loading

- By scene/object level
- By feature (e.g. no animation)
- Only parts loaded
- Load texture failed

## Technical Improvements

## Quality of Life improvements

### Shader GUI

Like Standard Shader inspector, but for glTF shaders

### Unit Tests

- [x] Import sample set
- [x] Measure speed and detect regressions
- [x] Render image and compare for changes
- [ ] Check key indicators like node/mesh/vertex count etc

## Optimization

- Investigate [Mesh API 2020.1 improvements](https://docs.google.com/document/d/1QC7NV7JQcvibeelORJvsaTReTyszllOlxdfEsaVL2oA/edit#heading=h.3lwfbowmy03j)
- Use Mesh.AllocateWritableMeshData
- Move as much as possible to threads
  - [x] Parsing JSON

### Minimize Requirements

Minimize build size by making as many dependencies as possible optional.

- Make Image Conversion / Unity Web Request Texture optional (no JPEG/PNG support)
- KtxUnity: ETC1s/UASTC-only mode ?

### DOTS

#### Job System

Used already. Extend usage.

#### Burst compiler

- Speed up jobs
- Use Unity.Mathematics data types

#### Entity Component System

Create entities instead of classic GameObjects

### Stream support

Start import during download

#### Custom download

- cheap glTF-binary detection

### Native texture upload

Move GPU texture upload off of main thread into native lib.
No need to reverse mipmap order.

### Memcopy

Optimize by comparing copy methods like:
<http://code4k.blogspot.co.at/2010/10/high-performance-memcpy-gotchas-in-c.html>
or maybe implement native (SIMD-based) copy methods.

### Mesh API

Copy values directly into unity buffer:
<https://docs.unity3d.com/ScriptReference/Mesh.GetNativeVertexBufferPtr.html>
<https://bitbucket.org/Unity-Technologies/graphicsdemos/src/6fd22b55d6a0c21f2a4d18629ba1b6c61d44ca23/NativeRenderingPlugin/UnityProject/Assets/UseRenderingPlugin.cs?at=default&fileviewer=file-view-default>

Faster than new Mesh API?

## Demo

- CI/CD

### WebGL

- Full-screen
- Drag'n'drop support
- Tiny

### Mobile

- Open file from file system
- Official App/Play store apps

### Desktop

- Open file from file system
- Drag'n'drop support

### Compare

Make up-to-date comparisons/benchmarks with similar projects (UnityGLTF, GLTFUtility, ...)

## Far Future Features

### Asset Analyzing

Give Feedback whenever assets are not optimal and ideally offer solutions (like re-encoding/optimizing).

- Unnecessary Shader Variant usage. Examples:
  - occlusion textures is not roughness texture
  - different texture transforms on different maps
  - two UV sets
  - texture rotation
- Embed buffers
- Shared vertex attribute buffers (see #52)
- Jpeg/PNG textures (instead of KTX)

### Editor Export

Export prefabs to glTF optimized for maximum glTFast speed

#### Custom Unity Extension

Serialize components and add to nodes

### Runtime Export

Export Scene/GameObjects/Entities to glTF at runtime
