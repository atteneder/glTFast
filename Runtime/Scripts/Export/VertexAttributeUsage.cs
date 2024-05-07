// SPDX-FileCopyrightText: 2024 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using UnityEngine.Rendering;

namespace GLTFast.Export
{
    /// <summary>
    /// Vertex attribute mask.
    /// </summary>
    [Flags]
    public enum VertexAttributeUsage
    {
        /// <summary>No attribute.</summary>
        None = 0,
        /// <inheritdoc cref="VertexAttribute.Position"/>
        Position = 1,
        /// <inheritdoc cref="VertexAttribute.Normal"/>
        Normal = 1 << 1,
        /// <inheritdoc cref="VertexAttribute.Tangent"/>
        Tangent = 1 << 2,
        /// <inheritdoc cref="VertexAttribute.Color"/>
        Color = 1 << 3,
        /// <inheritdoc cref="VertexAttribute.TexCoord0"/>
        TexCoord0 = 1 << 4,
        /// <inheritdoc cref="VertexAttribute.TexCoord1"/>
        TexCoord1 = 1 << 5,
        /// <inheritdoc cref="VertexAttribute.TexCoord2"/>
        TexCoord2 = 1 << 6,
        /// <inheritdoc cref="VertexAttribute.TexCoord3"/>
        TexCoord3 = 1 << 7,
        /// <inheritdoc cref="VertexAttribute.TexCoord4"/>
        TexCoord4 = 1 << 8,
        /// <inheritdoc cref="VertexAttribute.TexCoord5"/>
        TexCoord5 = 1 << 9,
        /// <inheritdoc cref="VertexAttribute.TexCoord6"/>
        TexCoord6 = 1 << 10,
        /// <inheritdoc cref="VertexAttribute.TexCoord7"/>
        TexCoord7 = 1 << 11,
        /// <inheritdoc cref="VertexAttribute.BlendWeight"/>
        BlendWeight = 1 << 12,
        /// <inheritdoc cref="VertexAttribute.BlendIndices"/>
        BlendIndices = 1 << 13,
        /// <summary>The first two texture coordinate channels.</summary>
        TwoTexCoords = TexCoord0 | TexCoord1,
        /// <summary>All eight texture coordinate channels.</summary>
        AllTexCoords = TexCoord0 | TexCoord1 | TexCoord2 | TexCoord3 | TexCoord4 | TexCoord5 | TexCoord6 | TexCoord7,
        /// <summary>Blend indices and weights, required for skinning/morph targets.</summary>
        Skinning = BlendWeight | BlendIndices,
    }

    /// <summary>
    /// Extension methods for <see cref="VertexAttribute"/>.
    /// </summary>
    public static class VertexAttributeUsageExtension
    {
        /// <summary>
        /// Converts a <see cref="VertexAttribute"/> to a <see cref="VertexAttributeUsage"/> mask with corresponding
        /// flag enabled.
        /// </summary>
        /// <param name="attr">Vertex attribute.</param>
        /// <returns>Vertex attribute mask with corresponding flag enabled.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static VertexAttributeUsage ToVertexAttributeUsage(this VertexAttribute attr)
        {
            return attr switch
            {
                VertexAttribute.Position => VertexAttributeUsage.Position,
                VertexAttribute.Normal => VertexAttributeUsage.Normal,
                VertexAttribute.Tangent => VertexAttributeUsage.Tangent,
                VertexAttribute.Color => VertexAttributeUsage.Color,
                VertexAttribute.TexCoord0 => VertexAttributeUsage.TexCoord0,
                VertexAttribute.TexCoord1 => VertexAttributeUsage.TexCoord1,
                VertexAttribute.TexCoord2 => VertexAttributeUsage.TexCoord2,
                VertexAttribute.TexCoord3 => VertexAttributeUsage.TexCoord3,
                VertexAttribute.TexCoord4 => VertexAttributeUsage.TexCoord4,
                VertexAttribute.TexCoord5 => VertexAttributeUsage.TexCoord5,
                VertexAttribute.TexCoord6 => VertexAttributeUsage.TexCoord6,
                VertexAttribute.TexCoord7 => VertexAttributeUsage.TexCoord7,
                VertexAttribute.BlendWeight => VertexAttributeUsage.BlendWeight,
                VertexAttribute.BlendIndices => VertexAttributeUsage.BlendIndices,
                _ => throw new ArgumentOutOfRangeException(nameof(attr), attr, null)
            };
        }
    }
}
