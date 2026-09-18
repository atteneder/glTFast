// SPDX-FileCopyrightText: 2024 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

#nullable enable
using System;
using Unity.Cloud.Gltfast.Objects;

namespace Unity.Cloud.Gltfast
{
    readonly struct VertexBufferDescriptor : IEquatable<VertexBufferDescriptor>
    {
        readonly bool m_HasNormals;
        readonly bool m_HasTangents;

        readonly int m_TexCoordCount;
        readonly bool m_HasColors;

        readonly bool m_HasBones;
        readonly int m_MorphTargetCount;

        VertexBufferDescriptor(
            bool hasNormals,
            bool hasTangents,
            int texCoordCount,
            bool hasColors,
            bool hasBones,
            int morphTargetCount
            )
        {
            m_HasNormals = hasNormals;
            m_HasTangents = hasTangents;
            m_TexCoordCount = texCoordCount;
            m_HasColors = hasColors;
            m_HasBones = hasBones;
            m_MorphTargetCount = morphTargetCount;
        }

        public static VertexBufferDescriptor FromPrimitive(MeshPrimitive primitive)
        {
            return new VertexBufferDescriptor(
                primitive.Attributes.Normal.HasValue,
                primitive.Attributes.Tangent.HasValue,
                primitive.Attributes.GetTexCoordsCount(),
                primitive.Attributes.GetColor(0).HasValue,
                primitive.Attributes.GetWeight(0).HasValue && primitive.Attributes.GetJoint(0).HasValue,
                primitive.Targets?.Count ?? 0
            );
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                m_HasNormals,
                m_HasTangents,
                m_TexCoordCount,
                m_HasColors,
                m_HasBones,
                m_MorphTargetCount
            );
        }

        public override bool Equals(object? obj) => obj is VertexBufferDescriptor other && Equals(other);

        public bool Equals(VertexBufferDescriptor other)
        {
            return m_HasNormals == other.m_HasNormals
                && m_HasTangents == other.m_HasTangents
                && m_TexCoordCount == other.m_TexCoordCount
                && m_HasColors == other.m_HasColors
                && m_HasBones == other.m_HasBones
                && m_MorphTargetCount == other.m_MorphTargetCount;
        }

        public static bool operator ==(VertexBufferDescriptor lhs, VertexBufferDescriptor rhs) => lhs.Equals(rhs);

        public static bool operator !=(VertexBufferDescriptor lhs, VertexBufferDescriptor rhs) => !(lhs == rhs);
    }
}
