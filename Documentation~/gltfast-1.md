# Legacy Installation

Unity versions between 2018.2 and 2019.2 are supported by [glTFast 1.x](https://github.com/atteneder/glTFast/tree/gltfast-1), which can be found in the [`gltfast-1` branch](https://github.com/atteneder/glTFast/tree/gltfast-1)

So the GIT URL you have to install in the package manager becomes `https://github.com/atteneder/glTFast.git#gltfast-1` (note the `#` and the branch at the end).

With older versions of Unity and the Package Manager you have to add the package in a manifest file manually. Add the package's URL into your [project manifest](https://docs.unity3d.com/Manual/upm-manifestPrj.html)

Inside your Unity project there's the folder `Packages` containing a file called `manifest.json`. You have to open it and add the following lines inside the `dependencies` category:

```json
"com.atteneder.draco": "https://github.com/atteneder/DracoUnity.git",
"com.atteneder.gltfast": "https://github.com/atteneder/glTFast.git#gltfast-1",
```

It should look something like this:

```json
{
  "dependencies": {
    "com.atteneder.draco": "https://github.com/atteneder/DracoUnity.git",
    "com.atteneder.gltfast": "https://github.com/atteneder/glTFast.git#gltfast-1",
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

## Draco support

If you use Unity older than 2019.1 and glTFast with DracoUnity, you additionally have to add `DRACO_UNITY` to your projects scripting define symbols in the player settings.

## Trouble shooting

If you run into an error like `com.atteneder.gltfast: Version is invalid. Expected a pattern like 'x.x.x[-prerelease]', got 'https://github.com/atteneder/glTFast.git' instead.`:

This is a legacy Unity bug. A workaround would be to make a local clone of the [`gltfast-1` branch](https://github.com/atteneder/glTFast/tree/gltfast-1) and add it via a local path (see the [documentation](https://docs.unity3d.com/Manual/upm-localpath.html)).
