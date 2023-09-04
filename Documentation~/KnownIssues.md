# Known Issues

- <sup>1</sup>Vertex accessors (positions, normals, etc.) that are used across meshes are duplicated and result in higher memory usage and slower loading (see [this comment](https://github.com/atteneder/glTFast/issues/52#issuecomment-583837852))
- <sup>1</sup>When using more than one sampler on one image, that image is duplicated and results in higher memory usage
- Texture sampler minification/magnification filter limitations (see [issue][SamplerFilter]):
  - <sup>1</sup>There's no differentiation between `minFilter` and `magFilter`. `minFilter` settings are prioritized.
  - <sup>1</sup>`minFilter` mode `NEAREST_MIPMAP_LINEAR` is not supported and will result in `NEAREST`.

<sup>1</sup>: A Unity API limitation.

## Trademarks

*Unity&reg;* is a registered trademark of [Unity Technologies][Unity].

*Khronos&reg;* is a registered trademark and *glTF&trade;* is a trademark of [The Khronos Group Inc][Khronos].

[Khronos]: https://www.khronos.org
[SamplerFilter]: https://github.com/atteneder/glTFast/issues/61
[Unity]: https://unity.com
