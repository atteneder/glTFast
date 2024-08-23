# Known Issues

## Import

- ¹Vertex accessors (positions, normals, etc.) that are used across meshes are duplicated and result in higher memory usage and slower loading (see [this comment](https://github.com/atteneder/glTFast/issues/52#issuecomment-583837852))
- ¹When using more than one sampler on one image, that image is duplicated and results in higher memory usage
- Texture sampler minification/magnification filter limitations (see [issue][SamplerFilter]):
  - ¹There's no differentiation between `minFilter` and `magFilter`. `minFilter` settings are prioritized.
  - ¹`minFilter` mode `NEAREST_MIPMAP_LINEAR` is not supported and will result in `NEAREST`.

¹: A Unity API limitation.

## Export

### Non-readable Meshes

Exporting non-readable meshes is not supported!

Turn on *Read/Write Enabled* on all model importer settings that you intend to export to glTF later at runtime to ensure it works reliably (see [FBX importer settings](https://docs.unity3d.com/6000.0/Documentation/Manual/FBXImporter-Model.html) as example).

While exporting non-readable meshes in general is feasible (via reading back index and vertex buffers from GPU memory) it has proven to be unreliable across platforms and graphics APIs, especially builds made with Unity version 2022 and older. The fact that it seems to work stable in Editor playmode is deceptive. The problem seems to be better in Unity 6 Preview, but there's not enough data to support that so use at own risk.

## Trademarks

*Unity&reg;* is a registered trademark of [Unity Technologies][Unity].

*Khronos&reg;* is a registered trademark and *glTF&trade;* is a trademark of [The Khronos Group Inc][Khronos].

[Khronos]: https://www.khronos.org
[SamplerFilter]: https://github.com/atteneder/glTFast/issues/61
[Unity]: https://unity.com
