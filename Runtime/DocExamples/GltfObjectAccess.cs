// SPDX-FileCopyrightText: 2026 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using Unity.Cloud.Gltfast.Objects;

namespace Unity.Cloud.Gltfast.Documentation.Examples
{
    static class GltfObjectAccess
    {
        #region BufferViewIndex
        public static Buffer GetBuffer(Root root, BufferView bufferView)
        {
            if (bufferView.Buffer is { } bufferIndex
                && bufferIndex >= 0
                && root.Buffers != null
                && bufferIndex < root.Buffers.Count)
            {
                return root.Buffers[bufferIndex];
            }

            // Absent, or referencing an element that does not exist.
            return null;
        }
        #endregion

        #region ExtrasValue
        public static bool TryGetWeights(Node node, out float[] weights)
        {
            var extras = node.Extras;

            if (extras == null)
            {
                // No extras at all, or an explicit `"extras": null`.
                weights = null;
                return false;
            }

            if (extras.Kind == ValueKind.Object)
            {
                // For example `"extras": { "weights": [1.0, 0.5] }`.
                return extras.TryGetValue("weights", out weights);
            }

            // Any other kind is the value itself, for example `"extras": [1.0, 0.5]`.
            return extras.RawValue.TryGetValue(out weights);
        }
        #endregion
    }
}
