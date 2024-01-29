// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

#if UNITY_2021_2_OR_NEWER
#define HYPERLINK
#define COMBINED
#endif

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace GLTFast.Editor
{
    [InitializeOnLoad]
    static class PackageSetupCheck
    {
        static ListRequest s_ListRequest;

        static PackageReplacement[] s_Packages = new PackageReplacement[]
        {
            new PackageReplacement()
            {
                name = "Draco for Unity",
                identifier = "com.unity.cloud.draco",
                legacyName = "Draco 3D Data Compression",
                legacyIdentifier = "com.atteneder.draco",
                feature = "KHR_draco_mesh_compression",
                featureUri = "https://github.com/KhronosGroup/glTF/blob/main/extensions/2.0/Khronos/KHR_draco_mesh_compression/README.md",
                upgradeDocsUri = "https://docs.unity3d.com/Packages/com.unity.cloud.draco@5.0/manual/upgrade-guide.html#transition-to-draco-for-unity"
            },
            new PackageReplacement()
            {
                name = "KTX for Unity",
                identifier = "com.unity.cloud.ktx",
                legacyName = "KTX/Basis Universal Texture",
                legacyIdentifier = "com.atteneder.ktx",
                feature = "KHR_texture_basisu",
                featureUri = "https://github.com/KhronosGroup/glTF/blob/main/extensions/2.0/Khronos/KHR_texture_basisu/README.md",
                upgradeDocsUri = "https://docs.unity3d.com/Packages/com.unity.cloud.ktx@3.2/manual/upgrade-guide.html#transition-to-ktx-for-unity"
            }
        };

        static PackageSetupCheck()
        {
            EditorApplication.update += WaitForPackageList;
            s_ListRequest = Client.List(true);
#if HYPERLINK
            EditorGUI.hyperLinkClicked += HyperLinkClicked;
#endif
        }

        static void WaitForPackageList()
        {
            if (s_ListRequest != null && s_ListRequest.IsCompleted)
            {
                if (s_ListRequest.Error == null)
                {
                    foreach (var package in s_Packages)
                    {
                        CheckForLegacyPackage(s_ListRequest.Result, package);
                    }
                }

                s_ListRequest = null;
                EditorApplication.update -= WaitForPackageList;
            }
        }

        static void CheckForLegacyPackage(
            PackageCollection packages,
            PackageReplacement pkg
            )
        {
            var legacyFound = false;

            foreach (var packageInfo in packages)
            {
                if (packageInfo.name == pkg.legacyIdentifier)
                {
                    legacyFound = true;
                }
            }
            if (legacyFound)
            {
                pkg.LogUpgradeMessage();
            }
        }

#if HYPERLINK
        static void HyperLinkClicked(EditorWindow window, HyperLinkClickedEventArgs args)
        {
            if(args.hyperLinkData.TryGetValue("command", out var command) && command=="replace")
            {
                if (args.hyperLinkData.TryGetValue("arg", out var pkg))
                {
                    foreach (var package in s_Packages)
                    {
                        if (package.legacyIdentifier == pkg)
                        {
                            ReplacePackage(package);
                        }
                    }
                }
            }
        }
#endif

#if COMBINED
        static void ReplacePackage(PackageReplacement package)
        {
            if (EditorUtility.DisplayDialog(
                    "Package Upgrade",
                    $"Replace deprecated {package.legacyName} ({package.legacyIdentifier}) by {package.name} ({package.identifier})?",
                    "Replace",
                    "Cancel"
                    )
               )
            {
                Client.AddAndRemove(
                    new[] { package.identifier },
                    new[] { package.legacyIdentifier }
                    );
            }
        }
#endif
    }

    struct PackageReplacement
    {
        public string name;
        public string identifier;
        public string legacyName;
        public string legacyIdentifier;
        public string feature;
        public string featureUri;
        public string upgradeDocsUri;

        public void LogUpgradeMessage()
        {
            var message = $"Deprecated package <i>{legacyName}</i> (<i>{legacyIdentifier}</i>) detected!\n" +
                $"<i>glTFast</i> now requires <i>{name}</i> (<i>{identifier}</i>) instead to provide support for " +
                $"<a href=\"{featureUri}\">{feature}</a>.\n";
#if HYPERLINK
            message +=
                $"You can <a command=\"replace\" arg=\"{legacyIdentifier}\">automatically replace</a> the " +
                $"deprecated package or do it manually following the <a href=\"{upgradeDocsUri}\">documentation</a>.";
#else
            message += "To upgrade the package, follow the documentation at " +
                $"<a href=\"{upgradeDocsUri}\">{upgradeDocsUri}</a>";
#endif
            Debug.LogWarning(message);
        }
    }
}
