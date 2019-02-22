# glTFast

glTFast is a Unity Plug-In for loading glTF 3D files at runtime with focus speed.

This means fast startup (due to small build footprint) and fast parsing/decoding.

## Missing features

This project is in an early development state and some things are missing:

* embed buffers or textures
* meshes without indices
* mesh compression
* no animation
* no glTF 1.0 backwards compatibility
* further extensions

See details on missing features in the [issues](https://github.com/atteneder/glTFast/issues) section.

## Roadmap / Priorities

Besides speed, the focus at the moment is on users that:

* control the content (are able to create compatible glTFs)
* use it for static content (no animation, skinning or morphing)

I try to keep an up-to-date, detailed roadmap in the [milestones](https://github.com/atteneder/glTFast/milestones)
 section.

## Get involved

Contributions like ideas, comments, critique, bug reports, pull requests are highly appreciated. Feel free to get in contact if you consider using or improving glTFast.

Also, you can show your appreciation and...

[![Buy me a coffee](https://az743702.vo.msecnd.net/cdn/kofi1.png?v=0)](https://ko-fi.com/C0C3BW7G)

## Why?
The Khronos group (creators of glTF) already provides an excellent Unity Plug-In here:
https://github.com/KhronosGroup/UnityGLTF

It is very well written, quite feature complete and stable. However, building a project with it (especially WebGL) will lead to huge binary files.
This project aims to be a low-profile alternative.

You can test and compare both libs here:
https://atteneder.github.io/glTFCompare

## Why is it smaller?
It uses [Unity's JsonUtility](https://docs.unity3d.com/ScriptReference/JsonUtility.html) for parsing, which has little overhead, is fast and memory-efficient (See https://docs.unity3d.com/Manual/JSONSerialization.html).

It also uses fast low-level memory copy methods whenever possible.

## What it is NOT
...and probably never will be:
* It's not an asset manager with instantiation and reference counting support. 
* Also not a download manager for asset caching/re-usage.

Such stuff should be able to place on top of this library.

# Usage
Copy the Assets/GLTFast folder into you Unity project's Assets folder.

Minimum code to load a glTF file:

```C#
var gltf = new GameObject().AddComponent<GLTFast.GltfAsset>();
gltf.url = "https://raw.githubusercontent.com/KhronosGroup/glTF-Sample-Models/master/2.0/Duck/glTF/Duck.gltf";
```

Minimum code to load a glTF binary (.glb) file:

```C#
var gltf = new GameObject().AddComponent<GLTFast.GlbAsset>();
gltf.url = "https://raw.githubusercontent.com/KhronosGroup/glTF-Sample-Models/master/2.0/Duck/glTF-Binary/Duck.glb";
```

In case you need to know when loading finished, add an event callback:

```C#
gltf.onLoadComplete += YourCallbackMethod;
```

## Materials and Shader Variants
glTF files can contain lots of materials making use of various shader features. You have to make sure all shader variants you project will probably use are included in the build. If not, the materials will be fine in the editor, but not in the builds.
glTFast uses the Unity Standard Shader. Including all its variants would be quite big. There's an easy way to find the right subset, if you already know what files you'll expect:
* Run your scene that loads all glTFs you expect in the editor.
* Go to Edit->Project Settings->Graphics
* At the bottom end you'll see the "Shader Preloading" section
* Save the currently tracked shaders/variants to an asset
* Take this ShaderVariantCollection asset and add it to the "Preloaded Shaders" list

An alternative way is to create placeholder materials for all feature combinations you expect and put them in a "Resource" folder in your project.

## Known issues
When building for WebGL with Unity 2018.1 you have to enable explicitly thrown exceptions (reason unknown - to be investigated)

The imported scene is scaled negative in one axis. I did not encounter any trouble doing this, but in case shading errors show up, this has to be fixed.
