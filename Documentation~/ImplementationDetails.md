# Implementation Details

*Unity glTFast* uses [System.Text.Json](https://www.nuget.org/packages/system.text.json/)'s source-generation-backed JSON (de-)serialization, which provides great speed, minimizes memory allocation overhead and offers comprehensive JSON access for advanced customizations.

*glTFast* also uses fast low-level memory copy methods, the [C# Job System](https://docs.unity3d.com/Manual/JobSystem.html), [Mathematics](https://docs.unity3d.com/Packages/com.unity.mathematics@1.0/manual/index.html), the [Burst compiler](https://docs.unity3d.com/Packages/com.unity.burst@1.6/manual/index.html) and the [Advanced Mesh API](https://docs.unity3d.com/ScriptReference/Mesh.html) to optimize loading times.

## Working with glTF objects

The glTF objects in `Unity.Cloud.Gltfast.Objects` do not enforce the glTF&trade; specification's requirements: de-serialization does not fail on a missing required property, serialization does not fail on an unset one. Extensions may relax base specification requirements, so validity depends on which extensions are in play &mdash; that judgement is left to you.

Properties holding an index into a root-level array are nullable (`int?`), where `null` means the property was absent from the JSON. This applies whether or not the specification marks the property as required. Check indices where you use them &mdash; a range check is needed in any case, since a malformed document may reference an element that does not exist:

[!code-cs [buffer-view-index](../Runtime/DocExamples/GltfObjectAccess.cs#BufferViewIndex)]

Sizes and counts &mdash; [Accessor.Count](xref:Unity.Cloud.Gltfast.Objects.Accessor.Count), `AccessorSparse.Count`, [BufferView.ByteLength](xref:Unity.Cloud.Gltfast.Objects.BufferView.ByteLength), [Buffer.ByteLength](xref:Unity.Cloud.Gltfast.Objects.Buffer.ByteLength) &mdash; are not nullable. The specification requires at least `1`, so `0` denotes an absent property.

Integer widths follow what a member describes: external resource sizes are `long`, in-memory offsets and slice lengths are `int`, matching the native collections they address. [Buffer.ByteLength](xref:Unity.Cloud.Gltfast.Objects.Buffer.ByteLength) being `long` therefore does **not** imply support for buffers above `int.MaxValue` &mdash; buffer data lives in native collections whose length is `int`, and buffer view offsets are `int` too.

The specification allows `extras` to be any JSON value, not just an object, so [ExtrasContainer.Kind](xref:Unity.Cloud.Gltfast.Objects.ExtrasContainer.Kind) reports what the JSON actually carried. It is [ValueKind.Object](xref:Unity.Cloud.Gltfast.Objects.ValueKind) in the common case, where the properties are reachable through `Count`, `Keys`, the indexer and `TryGetValue`. For any other kind the container has no properties and the value is read through [ExtrasContainer.RawValue](xref:Unity.Cloud.Gltfast.Objects.ExtrasContainer.RawValue):

[!code-cs [extras-value](../Runtime/DocExamples/GltfObjectAccess.cs#ExtrasValue)]

`extensions`, which the specification requires to be an object, does not allow this.

## Trademarks

*Unity&reg;* is a registered trademark of [Unity Technologies][unity].

*Khronos&reg;* is a registered trademark and *glTF&trade;* is a trademark of [The Khronos Group Inc][khronos].

[khronos]: https://www.khronos.org
[unity]: https://unity.com
