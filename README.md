# glTFast

glTFast is a Unity Package for loading glTF 3D files at runtime.

It's focus is on speed, specifically fast startup (due to small build footprint) and fast parsing/decoding.

Try the [WebGL Demo](https://atteneder.github.io/glTFastWebDemo) and check out the [demo project](https://github.com/atteneder/glTFastDemo).

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
glTFast uses the Unity Standard Shader. Including all its variants would be quite big. There's an easy way to find the right subset, if you already know what files you'll expect:

* Run your scene that loads all glTFs you expect in the editor.
* Go to Edit->Project Settings->Graphics
* At the bottom end you'll see the "Shader Preloading" section
* Save the currently tracked shaders/variants to an asset
* Take this ShaderVariantCollection asset and add it to the "Preloaded Shaders" list

An alternative way is to create placeholder materials for all feature combinations you expect and put them in a "Resource" folder in your project.

## Roadmap / Priorities

Besides speed, the focus at the moment is on users that:

* control the content (are able to create compatible glTFs)
* use it for static content (no animation, skinning or morphing)

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

* It's not an asset manager with instantiation and reference counting support.
* Also not a download manager for asset caching/re-usage.

Such stuff should be able to place on top of this library.

## Supported glTF extensions

* KHR_draco_mesh_compression
* KHR_materials_pbrSpecularGlossiness
* KHR_materials_unlit

## Missing features and known issues

This project is in an early development state and some things are missing:

* embed buffers or textures
* meshes without indices
* animation
* glTF 1.0 backwards compatibility

Known issues:

* When building for WebGL with Unity 2018.1 you have to enable explicitly thrown exceptions (reason unknown - to be investigated)

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
