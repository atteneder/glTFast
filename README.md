# glTFast

glTFast is a Unity package for loading [glTF 3D](https://www.khronos.org/gltf) files at runtime.

It focuses on speed, memory efficiency and a small build footprint.

Try the [WebGL Demo](https://atteneder.github.io/glTFastWebDemo) and check out the [demo project](https://github.com/atteneder/glTFastDemo).

## Features

- [x] Runtime import
- [x] Fast and small footprint JSON parsing
- [x] Multi-threading via C# job system
- [ ] Editor import
- [ ] Export

### Core glTF features

- [x] glTF (gltf + buffers + textures)
- [x] glTF binary (glb)

- [x] Scene
  - [x] Node hierarchy
  - [ ] Camera ([issue](issues/12))
- [x] Buffers
  - [x] External URIs
  - [x] glTF binary main buffer
  - [x] Embed buffers or textures (base-64 encoded within JSON)
- [x] Images
  - [x] PNG
  - [x] Jpeg
- [x] Materials
  - [x] Unity built-in pipeline
    - [x] PBR metallic-roughness
    - [x] PBR specular-glossiness (via extension)
    - [x] Unlit (via extension)
    - [x] Normal texture
    - [x] <sup>1</sup>Occlusion texture
    - [x] <sup>1</sup>Emission texture
    - [x] <sup>1</sup>Metallic texture
    - [x] <sup>1</sup>Roughness texture
    - [x] Alpha mode
    - [x] Double sided
    - [x] Vertex colors
    - [x] Emission(factor)
  - [ ] Universal Render Pipeline ([issue](issues/41))
  - [ ] High Definition Render Pipeline ([issue](issues/42))
- Primitive Types
  - [x] TRIANGLES
  - [x] <sup>2</sup>POINTS
  - [x] <sup>2</sup>LINES
  - [x] <sup>2</sup>LINE_STRIP
  - [x] <sup>2</sup>LINE_LOOP
  - [ ] TRIANGLE_STRIP
  - [ ] TRIANGLE_FAN
- [x] Meshes
  - [x] Positions
  - [x] Normals
  - [x] Tangents
  - [x] Texture coordinates
  - [x] Vertex colors
  - [x] Draco mesh compression (via extension)
  - [x] Implicit (no) indices
  - [ ] Per primitive material ([issue](issues/32))
  - [ ] Multiple texture coordinates sets ([issue](issues/34))
  - [ ] Joints
  - [ ] Weights
- [ ] Texture sampler ([issue](issues/45))
  - [ ] Filtering
  - [ ] Wrap mode
- [ ] Morph targets ([issue](issues/8))
  - [ ] Sparse accessors
- [ ] Skinning ([issue](issues/13))
- [ ] Animation

<sup>1</sup>: Slow operation at the moment, since the image channels have to be converted to fit the shader.

<sup>2</sup>: Untested due to lack of demo files.

### Extensions

- [x] KHR_draco_mesh_compression
- [x] KHR_materials_pbrSpecularGlossiness
- [x] KHR_materials_unlit
- [x] KHR_texture_transform
- [ ] KHR_mesh_quantization ([issue](issues/51))
- [ ] KHR_lights_punctual ([issue](issues/17))

## Installing

You have to manually add the package's URL into your [project manifest](https://docs.unity3d.com/Manual/upm-manifestPrj.html)

glTFast has a dependency to [DracoUnity](https://gitlab.com/atteneder/DracoUnity) (which provides support for compressed meshes), which also needs to be added.

Inside your Unity project there's the folder `Packages` containing a file called `manifest.json`. You have to open it and add the following lines inside the `dependencies` category:

```json
"com.atteneder.draco": "https://gitlab.com/atteneder/DracoUnity.git",
"com.atteneder.gltfast": "https://github.com/atteneder/glTFast.git",
```

It should look something like this:

```json
{
  "dependencies": {
    "com.atteneder.draco": "https://gitlab.com/atteneder/DracoUnity.git",
    "com.atteneder.gltfast": "https://github.com/atteneder/glTFast.git",
    "com.unity.package-manager-ui": "2.1.2",
    "com.unity.modules.imageconversion": "1.0.0",
    "com.unity.modules.imgui": "1.0.0",
    "com.unity.modules.jsonserialize": "1.0.0",
    "com.unity.modules.unitywebrequest": "1.0.0",
    "com.unity.modules.unitywebrequesttexture": "1.0.0"
  }
}
```

Note: You have to have a GIT LFS client (large file support) installed on your system. Otherwise you will get an error that the native library file (dll on Windows) is corrupt!

Next time you open your project in Unity, it will download the packages automatically. There's more detail about how to add packages via GIT URLs in the [Unity documentation](https://docs.unity3d.com/Manual/upm-git.html).

## Usage

Minimum code to load a glTF file:

```csharp
var gltf = new GameObject().AddComponent<GLTFast.GltfAsset>();
gltf.url = "https://raw.githubusercontent.com/KhronosGroup/glTF-Sample-Models/master/2.0/Duck/glTF/Duck.gltf";
```

Minimum code to load a glTF binary (.glb) file:

```csharp
var gltf = new GameObject().AddComponent<GLTFast.GlbAsset>();
gltf.url = "https://raw.githubusercontent.com/KhronosGroup/glTF-Sample-Models/master/2.0/Duck/glTF-Binary/Duck.glb";
```

In case you need to know when loading finished, add an event callback:

```csharp
gltf.onLoadComplete += YourCallbackMethod;
```

### Materials and Shader Variants

glTF files can contain lots of materials making use of various shader features. You have to make sure all shader variants your project will probably use are included in the build. If not, the materials will be fine in the editor, but not in the builds.
glTFast uses custom shaders that are derived from the Unity Standard shaders (and have a similar big number of variants). Including all those variants can make your build big. There's an easy way to find the right subset, if you already know what files you'll expect:

- Run your scene that loads all glTFs you expect in the editor.
- Go to Edit->Project Settings->Graphics
- At the bottom end you'll see the "Shader Preloading" section
- Save the currently tracked shaders/variants to an asset
- Take this ShaderVariantCollection asset and add it to the "Preloaded Shaders" list

An alternative way is to create placeholder materials for all feature combinations you expect and put them in a "Resource" folder in your project.

## Roadmap / Priorities

Besides speed, the focus at the moment is on users that:

- control the content (are able to create compatible glTFs)
- use it for static content (no animation, skinning or morphing)

I try to keep an up-to-date, detailed roadmap in the [milestones](https://github.com/atteneder/glTFast/milestones)
 section.

## Motivation

The Khronos group (creators of glTF) already provides an excellent Unity Plug-In called [UnityGLTF](https://github.com/KhronosGroup/UnityGLTF).

It is very well written, has many features and is stable. However, building a project with it (especially WebGL) will lead to huge binary files.
This project aims to be a low-profile alternative.

## Why is it smaller

It uses [Unity's JsonUtility](https://docs.unity3d.com/ScriptReference/JsonUtility.html) for parsing, which has little overhead, is fast and memory-efficient (See <https://docs.unity3d.com/Manual/JSONSerialization.html>).

It also uses fast low-level memory copy methods and [Unity's Job system](https://docs.unity3d.com/Manual/JobSystem.html).

## What it is NOT

...and probably never will be:

- It won't be backwards compatible to glTF 1.0
- It's not an asset manager with instantiation and reference counting support.
- Also not a download manager for asset caching/re-usage.
Such stuff should be able to place on top of this library.

## Known issues

- Some shader variants (observed with double sided, textured metallic-roughness materials) don't work on older Unity versions (pre 2019.1). Probably cause is the shader feature keyword limit.
- When building for WebGL with Unity 2018.1 you have to enable explicitly thrown exceptions (reason unknown - to be investigated)

See details in the [issues](https://github.com/atteneder/glTFast/issues) section.

## Get involved

Contributions like ideas, comments, critique, bug reports, pull requests are highly appreciated. Feel free to get in contact if you consider using or improving glTFast.

Also, you can show your appreciation and...

[![Buy me a coffee](https://az743702.vo.msecnd.net/cdn/kofi1.png?v=0)](https://ko-fi.com/C0C3BW7G)

## License

Copyright (c) 2019 Andreas Atteneder, All Rights Reserved.

Licensed under the Apache License, Version 2.0 (the "License");
you may not use files in this repository except in compliance with the License.
You may obtain a copy of the License at

   <http://www.apache.org/licenses/LICENSE-2.0>

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
