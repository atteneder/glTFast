# More Information

## Implementation Details

*glTFast* uses [Unity's JsonUtility](https://docs.unity3d.com/ScriptReference/JsonUtility.html) for parsing, which has little overhead, is fast and memory-efficient (See <https://docs.unity3d.com/Manual/JSONSerialization.html>).

It also uses fast low-level memory copy methods, [Unity's Job system](https://docs.unity3d.com/Manual/JobSystem.html), [Mathematics](https://docs.unity3d.com/Packages/com.unity.mathematics@1.0/manual/index.html), the [Burst compiler](https://docs.unity3d.com/Packages/com.unity.burst@1.6/manual/index.html) and the [Advanced Mesh API](https://docs.unity3d.com/ScriptReference/Mesh.html).
