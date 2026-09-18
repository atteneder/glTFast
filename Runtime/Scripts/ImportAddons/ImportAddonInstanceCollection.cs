// SPDX-FileCopyrightText: 2026 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Unity.Cloud.Gltfast.Addons
{
    sealed class ImportAddonInstanceCollection : QueryableList<ImportAddonInstance>, IDisposable
    {
        public bool AnySupportsGltfExtension(string extensionName)
        {
            foreach (var instance in this)
            {
                if (instance.SupportsGltfExtension(extensionName))
                {
                    return true;
                }
            }

            return false;
        }

        public void Dispose()
        {
            foreach (var importInstance in this)
            {
                importInstance.Dispose();
            }
            Clear();
        }
    }
}
