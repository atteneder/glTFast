// SPDX-FileCopyrightText: 2026 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using Unity.Cloud.Gltfast.Text.Json.Serialization;
using UnityEngine;

namespace Unity.Cloud.Gltfast.Objects
{
    [JsonSourceGenerationOptions(DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault)]
    [JsonSerializable(typeof(Root))]
    [JsonSerializable(typeof(Extension))]
    partial class GltfJsonContext : JsonSerializerContext { }
}
