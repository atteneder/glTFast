# glTFast Ideas

## Optimization

Optimize by comparing copy methods like:
<http://code4k.blogspot.co.at/2010/10/high-performance-memcpy-gotchas-in-c.html>
or maybe implement native (SIMD-based) copy methods.

copy values directly into unity buffer:
<https://docs.unity3d.com/ScriptReference/Mesh.GetNativeVertexBufferPtr.html>
<https://bitbucket.org/Unity-Technologies/graphicsdemos/src/6fd22b55d6a0c21f2a4d18629ba1b6c61d44ca23/NativeRenderingPlugin/UnityProject/Assets/UseRenderingPlugin.cs?at=default&fileviewer=file-view-default>

- Use Unity's new mesh API
- Burst compiler
- Unity.Mathematics

DOTS support

- Don't depend on/generate regular GameObjects
- Create entities instead
- Abstract instantiation interface

## Events

- meshes ready
- materials/textures ready
- animation ready
- everything ready
- Loading progress

## Partial Loading

- By scene/object level
- By feature (e.g. no animation)
- Only parts loaded
- Load texture failed
