# glTFast Documentation

*glTFast* enables use of [glTF™ (GL Transmission Format)][gltf] asset files in [Unity][unity].

It focuses on speed, memory efficiency and a small build footprint while also providing:

- 100% [glTF 2.0 specification][gltf-spec] compliance
- Ease of use
- Robustness and Stability
- Customization and extensibility for advanced users

## Features

*glTFast* supports the full [glTF 2.0 specification][gltf-spec] and many extensions. It works with Universal, High Definition and the Built-In Render Pipelines on all platforms.

See the [comprehensive list of supported features and extensions](./features.md).

### Workflows

There are four use-cases for glTF within Unity

- Import
    - [Runtime Import/Loading](ImportRuntime.md) in games/applications
    - [Editor Import](ImportEditor.md) (i.e. import assets at design-time)
- Export
    - [Runtime Export](ExportRuntime.md) (save and share dynamic, user-generated 3D content)
    - [Editor Export](ExportEditor.md) (Unity as glTF authoring tool)

![Schematic diagram of the four glTF workflows](Images/Unity-glTF-workflows.png "The four glTF workflows")

#### Runtime Import/Loading

Load and instantiate glTF files at runtime in your game/application via Script or the `GltfAsset` component.

#### Benefits of Runtime Import

- Efficiently load dynamic and/or third-party assets
    - Make use of state-of-the art mesh and texture compression methods, like KTX/Basis Universal, Draco and meshoptimizer.
- No need to re-build your application or Asset Bundles upon Unity version upgrades

*glTF* was specifically designed for vendor-independent transmission and runtime loading and naturally plays its strengths there.

#### Editor Import (Design-Time)

Although primarily designed for runtime, *glTF*'s effective design and its modern, physically-based material definition make it great for most simple DCC (digital content creation) interchange as well.

Read about [usage](ImportEditor.md) below.

##### Benefits of Editor Import

- Less friction between artists and developers due to *glTF* as standardized interface
    - Example: artists don't need to know or follow Unity shader specific conventions and thus developers don't need to instruct them
- Enables adding rich interaction and behaviour to assets (e.g. custom scripts or animation controllers)
- In conjunction with [Editor Export](ExportEditor.md), Unity becomes a complete tool for re-mixing 3D content
- <sup>1</sup>Use default Lit (URP/HDRP) or Standard (Built-in render pipeline) materials

<sup>1</sup>: Not yet supported (see [issue](https://github.com/atteneder/glTFast/issues/258))

#### Editor Export

Use the Unity Editor as an authoring tool and export your scenes and GameObjects as *glTFs*.

> Note: This feature is experimental

##### Use-cases for Editor Export

- [Unity runtime loading](ImportRuntime.md)
- Social media sharing
- Use within the [vast glTF eco system][gltf-projects], like third-party viewers or asset pipelines
- Archiving

#### Runtime Export

Allows your Unity-powered application/game to export scenes/GameObjects to glTF at runtime.

##### Use-cases for Runtime Export

- Preserve dynamic, user-generated 3D content
    - Create metaverse-ready 3D snapshots of a current state / game action
    - 3D product configurations (e-commerce)
- Build high level editing and authoring tools with Unity
- Social media sharing

> Note: This feature is coming soon (see [issue](https://github.com/atteneder/glTFast/issues/259))

[gltf]: https://www.khronos.org/gltf
[gltf-projects]: https://github.khronos.org/glTF-Project-Explorer
[unity]: https://unity.com
[gltf-spec]: https://www.khronos.org/registry/glTF/specs/2.0/glTF-2.0.html
