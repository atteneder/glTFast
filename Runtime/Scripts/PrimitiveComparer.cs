// SPDX-FileCopyrightText: 2024 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Cloud.Gltfast.Objects;
using Unity.Cloud.Gltfast.Text.Json;

namespace Unity.Cloud.Gltfast
{
    static class PrimitiveComparer
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HaveEqualVertexBuffers(MeshPrimitive x, MeshPrimitive y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x is null) return false;
            if (y is null) return false;
            return Equals(x.Attributes, y.Attributes)
                && Equals(x.Targets, y.Targets);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CalculateHashCode(MeshPrimitive primitive)
        {
            return HashCode.Combine(
                GetHashCode(primitive.Attributes),
                GetHashCode(primitive.Targets)
                );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool Equals(Attributes x, Attributes y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x == null || y == null) return false;
            return x.Position == y.Position
                && x.Normal == y.Normal
                && x.Tangent == y.Tangent
                && ChannelEquals(x.TexCoords, y.TexCoords)
                && ChannelEquals(x.Colors, y.Colors)
                && ChannelEquals(x.Joints, y.Joints)
                && ChannelEquals(x.Weights, y.Weights)
                && ExtensionDataEquals(x.ExtensionData, y.ExtensionData);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static bool ChannelEquals(List<int?> x, List<int?> y)
        {
            if (ReferenceEquals(x, y)) return true;
            var xCount = x?.Count ?? 0;
            var yCount = y?.Count ?? 0;
            if (xCount != yCount) return false;
            for (var i = 0; i < xCount; i++)
            {
                if (x[i] != y[i]) return false;
            }
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static bool ExtensionDataEquals(Dictionary<string, JsonElement> x, Dictionary<string, JsonElement> y)
        {
            if (ReferenceEquals(x, y)) return true;
            var xCount = x?.Count ?? 0;
            var yCount = y?.Count ?? 0;
            if (xCount != yCount) return false;
            if (xCount == 0) return true;
            foreach (var pair in x)
            {
                if (!y.TryGetValue(pair.Key, out var v)) return false;
                if (pair.Value.ValueKind == JsonValueKind.Number && v.ValueKind == JsonValueKind.Number)
                {
                    if (pair.Value.TryGetDouble(out var xVal) && v.TryGetDouble(out var yVal))
                    {
                        if (xVal.Equals(yVal)) continue;
                        return false;
                    }
                }
                if (pair.Value.GetRawText() != v.GetRawText()) return false;
            }
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool Equals(IReadOnlyList<MorphTarget> x, IReadOnlyList<MorphTarget> y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x == null || y == null) return false;
            if (x.Count != y.Count) return false;
            for (var i = 0; i < x.Count; i++)
            {
                if (!Equals(x[i], y[i]))
                    return false;
            }
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static bool Equals(MorphTarget x, MorphTarget y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x == null || y == null) return false;
            return x.Position == y.Position
                && x.Normal == y.Normal
                && x.Tangent == y.Tangent;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int GetHashCode(Attributes x)
        {
            if (x == null) return 0;
            var hash = new HashCode();
            hash.Add(x.Position);
            hash.Add(x.Normal);
            hash.Add(x.Tangent);
            AddChannel(ref hash, x.TexCoords);
            AddChannel(ref hash, x.Colors);
            AddChannel(ref hash, x.Joints);
            AddChannel(ref hash, x.Weights);
            if (x.ExtensionData != null)
            {
                // XOR to keep the contribution order-independent — Dictionary
                // iteration order isn't part of the glTF object's identity.
                var extHash = 0;
                foreach (var pair in x.ExtensionData)
                {
                    var valHash = pair.Value.ValueKind == JsonValueKind.Number && pair.Value.TryGetDouble(out var d)
                        ? d.GetHashCode()
                        : (pair.Value.GetRawText()?.GetHashCode() ?? 0);
                    extHash ^= HashCode.Combine(pair.Key, valHash);
                }
                hash.Add(extHash);
            }
            return hash.ToHashCode();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void AddChannel(ref HashCode hash, List<int?> channel)
        {
            if (channel == null)
            {
                hash.Add(0);
                return;
            }
            hash.Add(channel.Count);
            for (var i = 0; i < channel.Count; i++)
            {
                hash.Add(channel[i]);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static int GetHashCode(IReadOnlyList<MorphTarget> x)
        {
            if (x == null) return 0;
            HashCode hash = new();
            hash.Add(x.Count);
            for (var index = 0; index < x.Count; index++)
            {
                var target = x[index];
                if (target == null)
                {
                    hash.Add(0);
                    continue;
                }

                hash.Add(target.Position);
                hash.Add(target.Normal);
                hash.Add(target.Tangent);
            }

            return hash.ToHashCode();
        }
    }
}
