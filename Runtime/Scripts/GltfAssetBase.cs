﻿// Copyright 2020-2021 Andreas Atteneder
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

using System.Threading.Tasks;
using UnityEngine;
using Mesh = GLTFast.Schema.Mesh;

namespace GLTFast
{
    using Loading;

    public class GltfAssetBase : MonoBehaviour
    {
        protected GLTFast gLTFastInstance;
        
        /// <summary>
        /// Indicates wheter the glTF was loaded (no matter if successfully or not)
        /// </summary>
        /// <value>True when loading routine ended, false otherwise.</value>
        public bool isDone => gLTFastInstance!=null && gLTFastInstance.LoadingDone;

        /// <summary>
        /// Method for manual loading with custom <see cref="IDownloadProvider"/> and <see cref="IDeferAgent"/>.
        /// </summary>
        /// <param name="url">URL of the glTF file.</param>
        /// <param name="downloadProvider">Download Provider for custom loading (e.g. caching or HTTP authorization)</param>
        /// <param name="deferAgent">Defer Agent takes care of interrupting the
        /// loading procedure in order to keep the frame rate responsive.</param>
        /// <param name="disposeData">If true, volatile data will be disposed automatically after loading.</param>
        public virtual async Task<bool> Load( string url, IDownloadProvider downloadProvider=null, IDeferAgent deferAgent=null, IMaterialGenerator materialGenerator=null, bool disposeData=true ) {
            gLTFastInstance = new GLTFast(downloadProvider,deferAgent, materialGenerator);
            return await gLTFastInstance.Load(url, disposeData);
        }

        /// <summary>
        /// Creates an instance of a glTF file underneath the provided Transform's GameObject.
        /// </summary>
        /// <param name="transform">Transform that will become the parent of the new instance.</param>
        /// <returns>True if instatiation was successful.</returns>
        public bool Instantiate( Transform transform ) {
            return gLTFastInstance != null && gLTFastInstance.InstantiateGltf(transform);
        }

        /// <summary>
        /// Returns an imported glTF material.
        /// Note: Asset has to have finished loading before!
        /// </summary>
        /// <param name="index">Index of material in glTF file.</param>
        /// <returns>glTF material if it was loaded successfully and index is correct, null otherwise.</returns>
        public UnityEngine.Material GetMaterial( int index = 0 ) {
            return gLTFastInstance?.GetMaterial(index);
        }
        
        /// <summary>
        /// Get an Accessor given it's index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public VertexInputData GetAccessor(int index)
        {
            return gLTFastInstance.GetAccessorParams(gLTFastInstance.gltfRoot, index);
        }

        /// <summary>
        /// Get a list of the gltfRoot's meshes
        /// </summary>
        /// <returns></returns>
        public Mesh[] GetGltfMeshes()
        {
            return gLTFastInstance.gltfRoot.meshes;
        }

        /// <summary>
        /// Get the UnityMesh given the glTF mesh's index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public UnityEngine.Mesh GetUnityMesh(int index)
        {
            return gLTFastInstance.GetMesh(index);
        }

        /// <summary>
        /// Dispose volatile data in gltFastInstance.
        /// </summary>
        public void DisposeData()
        {
            gLTFastInstance.DisposeVolatileData();
        }

        protected virtual void OnDestroy()
        {
            if(gLTFastInstance!=null) {
                gLTFastInstance.Destroy();
                gLTFastInstance=null;
            }
        }
    }
}
