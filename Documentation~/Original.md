# Original glTFast

This package's original identifier was `com.atteneder.gltfast` (see [Unity Fork][fork] for details). You can still use/install it with that identifier.

## Via [OpenUPM][OpenUPM]

The recommended way to install is to download and open the [Installer Package](https://package-installer.glitch.me/v1/installer/OpenUPM/com.atteneder.gltfast?registry=https%3A%2F%2Fpackage.openupm.com&scope=com.atteneder)

It runs a script that installs *glTFast* via the [OpenUPM][OpenUPM] [scoped registry](https://docs.unity3d.com/Manual/upm-scoped.html).

Afterwards *glTFast* and further, optional packages are listed in the *Package Manager* (under *My Registries*) and can be installed and updated from there.

## Using GIT

If you want to clone the GIT repository and installed the package locally (which is useful if you intend to develop glTFast itself), you have to checkout the branch `openupm`.

The package identifier in the `main` branch was changed, which likely leads to errors if you've cloned it before and pulled that change. In that case either switch to the `openupm` branch or [transition to the new package identifier][transition].

[fork]: ./UpgradeGuides#unity-fork
[transition]: ./UpgradeGuides#transition-to-unity-gltfast
[OpenUPM]: https://openupm.com/
