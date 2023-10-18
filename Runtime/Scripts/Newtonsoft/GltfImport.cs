// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

#if NEWTONSOFT_JSON

using GLTFast.Loading;
using GLTFast.Logging;
using GLTFast.Materials;
using GLTFast.Schema;
using Newtonsoft.Json;
using UnityEngine;
using Root = GLTFast.Newtonsoft.Schema.Root;

namespace GLTFast.Newtonsoft
{
    /// <summary>
    /// Loads a glTF's content, converts it to Unity resources and is able to
    /// feed it to an <see cref="IInstantiator"/> for instantiation.
    /// Uses the powerful Newtonsoft JSON and flexible <see cref="Root"/> schema class for JSON de-serialization.
    /// </summary>
    public class GltfImport : GltfImportBase<Root>
    {
        public GltfImport(
            IDownloadProvider downloadProvider = null,
            IDeferAgent deferAgent = null,
            IMaterialGenerator materialGenerator = null,
            ICodeLogger logger = null
        ) : base(downloadProvider,deferAgent,materialGenerator, logger) { }

        protected override RootBase ParseJson(string json)
        {
            return JsonConvert.DeserializeObject<Root>(json);
        }
    }
}
#endif
