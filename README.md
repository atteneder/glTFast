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
  - [x] <sup>2</sup>KTX with Basis Universal super compression ([instructions](#basisu))
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
  - [ ] Universal Render Pipeline ([issue](issues/41))
  - [ ] High Definition Render Pipeline ([issue](issues/42))
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
  - [x] Draco mesh compression (via extension)
  - [x] Implicit (no) indices
  - [ ] Per primitive material ([issue](issues/32))
  - [ ] Multiple texture coordinates sets ([issue](issues/34))
  - [ ] Joints
  - [ ] Weights
- [x] Texture sampler
  - [x] Filtering (see ([limitations](#knownissues)))
  - [x] Wrap modes
- [ ] Morph targets ([issue](issues/8))
  - [ ] Sparse accessors
- [ ] Skinning ([issue](issues/13))
- [ ] Animation

<sup>1</sup>: Untested due to lack of demo files.

<sup>2</sup>: Experimental

### Extensions

- [x] KHR_draco_mesh_compression
- [x] KHR_materials_pbrSpecularGlossiness
- [x] KHR_materials_unlit
- [x] KHR_texture_transform
- [x] KHR_mesh_quantization
- [x] <sup>1</sup>KHR_texture_basisu ([instructions](#basisu))
- [ ] KHR_lights_punctual ([issue](issues/17))

<sup>1</sup>: Experimental

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

You can load a glTF asset via HTTP(S) URL or a file path. You can load JSON based glTFs (`*.gltf`) and glTF-binary files (`*.glb`)

> Note: glTFs are loaded via UnityWebRequests. As a result, in the Unity Editor and on certain platforms (e.g. iOS) file paths have to be prefixed with `file://`

### Via adding the `GltfAsset` component

The simplest way to load a glTF file is to add a `GltfAsset` component to a GameObject.

![GltfAsset component][gltfasset_component]

### Via Script

Loading via script can have additional benefits

- Add custom listeners to loading events
- Load glTF once and instantiate it many times
- Tweak and optimize loading performance

#### Simple example

This is the most simple way to load a glTF scene from script

```csharp
var gltf = gameObject.AddComponent<GLTFast.GltfAsset>();
gltf.url = "https://raw.githubusercontent.com/KhronosGroup/glTF-Sample-Models/master/2.0/Duck/glTF/Duck.gltf";
```

#### Custom loading event listeners

In case you want to trigger custom logic when loading finished, add an event callback:

```csharp
gltf.onLoadComplete += YourCallbackMethod;
```

#### Instantiation

This section is under construction.

#### Tune loading performance

When loading glTFs, glTFast let's you optimize for two diametrical extremes

- A stable frame rate
- Fastest loading time

By default each `GltfAsset` instance tries not to block the main thread for longer than a certain time budget and defer the remaining loading process to the next frame / game loop iteration.

If you load many glTF files at once, by default they won't be aware of each other and collectively might block the main game loop for too long.

You can solve this by using a common "defer agent". It decides if work should continue right now or at the next game loop iteration. glTFast comes with two defer agents

- `TimeBudgetPerFrameDeferAgent` for stable frame rate
- `UninterruptedDeferAgent` for fastest, uninterrupted loading

Usage example

```csharp
IDeferAgent deferAgent;
// For a stable frame rate:
deferAgent = gameObject.AddComponent<GLTFast.TimeBudgetPerFrameDeferAgent>();
// Or for faster loading:
deferAgent = new GLTFast.UninterruptedDeferAgent();
foreach( var url in manyUrls) {
  var gltf = go.AddComponent<GLTFast.GltfAsset>();
  gltf.loadOnStartup = false; // prevent auto-loading
  gltf.Load(url,deferAgent); // load manually with custom defer agent
}
```

> Note 1: Depending on your glTF scene, using the `UninterruptedDeferAgent` may block the main thread for up to multiple seconds. Be sure to not do this during critical game play action.

> Note2 : Using the `TimeBudgetPerFrameDeferAgent` does **not** guarantee a stutter free frame rate. This is because some sub tasks of the loading routine (like uploading a texture to the GPU) may take too long, cannot be interrupted and **have** to be done on the main thread.

### Materials and Shader Variants

glTF files can contain lots of materials making use of various shader features. You have to make sure all shader variants your project will probably use are included in the build. If not, the materials will be fine in the editor, but not in the builds.
glTFast uses custom shaders that are derived from the Unity Standard shaders (and have a similar big number of variants). Including all those variants can make your build big. There's an easy way to find the right subset, if you already know what files you'll expect:

- Run your scene that loads all glTFs you expect in the editor.
- Go to Edit->Project Settings->Graphics
- At the bottom end you'll see the "Shader Preloading" section
- Save the currently tracked shaders/variants to an asset
- Take this ShaderVariantCollection asset and add it to the "Preloaded Shaders" list

An alternative way is to create placeholder materials for all feature combinations you expect and put them in a "Resource" folder in your project.

## <a name="basisu"></a>Experimental KTX / Basis Universal support

To enable the experimental support for KTX / Basis Universal support, first add the [KtxUnity package](https://github.com/atteneder/KtxUnity) to your manifest.json

```json
"com.atteneder.ktx": "https://github.com/atteneder/KtxUnity.git",
```

Second, add `GLTFAST_BASISU` to your projects scripting define symbols in the player settings.

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

## <a name="knownissues">Known issues

- <sup>1</sup>Vertex accessors (positions, normals, etc.) that are used across meshes are duplicated and result in higher memory usage
- <sup>1</sup>When using more than one samplers on an image, that image is duplicated and results in higher memory usage
- Texture sampler minification/magnification filter limitations (see [issue](issues/61)):
  - <sup>1</sup>There's no differentiation between `minFilter` and `magFilter`. `minFilter` settings are prioritized.
  - <sup>1</sup>`minFilter` mode `NEAREST_MIPMAP_LINEAR` is not supported and will result in `NEAREST`.
- When building for WebGL with Unity 2018.1 you have to enable explicitly thrown exceptions (reason unknown - to be investigated)

<sup>1</sup>: A Unity API limitation.

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

[gltfasset_component]: ./Documentation~/img/gltfasset_component.png  "Inspector showing a GltfAsset component added to a GameObject"
