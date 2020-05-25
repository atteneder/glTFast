# glTFast 🚀

[![openupm](https://img.shields.io/npm/v/com.atteneder.gltfast?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.atteneder.gltfast/)

glTFast is a Unity package for loading [glTF 3D](https://www.khronos.org/gltf) files at runtime.

It focuses on speed, memory efficiency and a small build footprint.

Try the [WebGL Demo](https://atteneder.github.io/glTFastWebDemo) and check out the [demo project](https://github.com/atteneder/glTFastDemo).

## Features

glTFast supports runtime loading of all sorts of glTF 2.0 files. It runs on WebGL, iOS, Android, Windows, macOS and Linux and supports the majority of glTF's features and official extensions.

Most notable restrictions

- Just for static scenes. No animations, skinning/rigs or morph targets supported.
- Unity's built-in render pipeline only (URP and HDRP are planned)

See the [list of features/extensions](./Documentation~/features.md) for details and limitations.

## Installing

Add glTFast via Unity's Package Manager ( Window -> Package Manager ). Click the ➕ on the top left and choose *Add package from GIT URL*.

![Package Manager -> + -> Add Package from git URL][upm_install]

Enter the following URL:

`https://github.com/atteneder/glTFast.git`

To add support for Draco mesh compression, repeat the last step and also add the DracoUnity packages using this URL:

`https://gitlab.com/atteneder/DracoUnity.git`

> Note: You have to have a GIT LFS client (large file support) installed on your system. Otherwise you will get an error that the native library file (dll on Windows) is corrupt!

If you use Unity older than 2019.1, you additionally have to add `DRACO_UNITY` to your projects scripting define symbols in the player settings.

### Open Source Unity Package Registry

glTFast can also be installed from the [Open Source Unity Package Registry](https://openupm.com/packages/com.atteneder.gltfast/) (experimental).

### Legacy installation

With older versions of Unity and the Package Manager you have to add the package in a manifest file manually. Add the package's URL into your [project manifest](https://docs.unity3d.com/Manual/upm-manifestPrj.html)

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
- Access data of glTF scene (for example get material)
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
…
void YourCallbackMethod(GltfAssetBase gltfAsset, bool success) {
    // Good practice: remove listener right away
    gltfAsset.onLoadComplete -= YourCallbackMethod;
    if(success) {
        // Get the first material
        var material = gltfAsset.GetMaterial();
        Debug.LogFormat("The first material is called {0}", material.name);

        // Instantiate the scene multiple times
        gltfAsset.Instantiate( new GameObject("Instance 1").transform );
        gltfAsset.Instantiate( new GameObject("Instance 2").transform );
        gltfAsset.Instantiate( new GameObject("Instance 3").transform );
    } else {
        Debug.LogError("Loading glTF failed!");
    }
}
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

To enable the experimental support for KTX / Basis Universal support, add the [KtxUnity package](https://github.com/atteneder/KtxUnity) via the Package Manager (see [Installing](#installing))

`https://github.com/atteneder/KtxUnity.git`

Or the manual/legacy way, add this to your manifest.json file:

```json
"com.atteneder.ktx": "https://github.com/atteneder/KtxUnity.git",
```

If you use Unity older than 2019.1, you additionally have to add `KTX_UNITY` to your projects scripting define symbols in the player settings.

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
[upm_install]: ./Documentation~/img/upm_install.png  "Unity Package Manager add menu"
