// Copyright 2020-2021 Andreas Atteneder
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

#if !UNITY_WEBGL || UNITY_EDITOR
#define GLTFAST_THREADS
#endif

// #define MEASURE_TIMINGS

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Profiling;
using Unity.Collections;
using Unity.Jobs;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using GLTFast.Jobs;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using Debug = UnityEngine.Debug;
#if MESHOPT
using Meshoptimizer;
#endif
#if MEASURE_TIMINGS
using GLTFast.Tests;
#endif

[assembly: InternalsVisibleTo("glTFastEditorTests")]
[assembly: InternalsVisibleTo("glTFast.Export")]

namespace GLTFast {

    using Schema;
    using Loading;

    public class GltfImport : IGltfReadable, IGltfBuffers {

        /// <summary>
        /// JSON parse speed in bytes per second
        /// Measurements based on a MacBook Pro Intel(R) Core(TM) i9-9980HK CPU @ 2.40GHz
        /// and reduced by ~ 20%
        /// </summary>
        const int k_JsonParseSpeed = 
#if UNITY_EDITOR
            45_000_000;
#else
            80_000_000;
#endif
        /// <summary>
        /// Base 64 string to byte array decode speed in bytes per second
        /// Measurements based on a MacBook Pro Intel(R) Core(TM) i9-9980HK CPU @ 2.40GHz
        /// and reduced by ~ 20%
        /// </summary>
        const int k_Base64DecodeSpeed =
#if UNITY_EDITOR
            60_000_000;
#else
            150_000_000;
#endif
        
        public const int DefaultBatchCount = 512;

        const string PrimitiveName = "Primitive";

        public static readonly HashSet<string> supportedExtensions = new HashSet<string> {
#if DRACO_UNITY
            Extensions.DracoMeshCompression,
#endif
#if KTX_UNITY
            Extensions.TextureBasisUniversal,
#endif // KTX_UNITY
#if MESHOPT
            Extensions.MeshoptCompression,
#endif
            Extensions.MaterialsPbrSpecularGlossiness,
            Extensions.MaterialsUnlit,
            Extensions.TextureTransform,
            Extensions.MeshQuantization,
            Extensions.MaterialsTransmission,
            Extensions.MeshGPUInstancing,
        };

        static IDeferAgent defaultDeferAgent;
        
        IDownloadProvider downloadProvider;
        IMaterialGenerator materialGenerator;
        IDeferAgent deferAgent;

        ImportSettings settings;

#region VolatileData

        /// <summary>
        /// These members are only used during loading phase.
        /// </summary>
        byte[][] buffers;

        /// <summary>
        /// GCHandles for pinned managed arrays <see cref="buffers"/>
        /// </summary>
        GCHandle?[] bufferHandles;

        /// <summary>
        /// NativeArray views into <see cref="buffers"/>
        /// </summary>
        NativeArray<byte>[] nativeBuffers;

        GlbBinChunk[] binChunks;

        Dictionary<int,Task<IDownload>> downloadTasks;
#if KTX_UNITY
        Dictionary<int,Task<IDownload>> ktxDownloadTasks;
#endif
        Dictionary<int,Task<ITextureDownload>> textureDownloadTasks;

        AccessorDataBase[] accessorData;
        AccessorUsage[] accessorUsage;
        JobHandle accessorJobsHandle;
        PrimitiveCreateContextBase[] primitiveContexts;
        Dictionary<MeshPrimitive,VertexBufferConfigBase> vertexAttributes;
        /// <summary>
        /// Array of dictionaries, indexed by mesh ID
        /// The dictionary contains all the mesh's primitives, clustered
        /// by Vertex Attribute and Morph Target usage (Primitives with identical vertex
        /// data will be clustered; see MeshPrimitive.Equals).
        /// </summary>
        Dictionary<MeshPrimitive,List<MeshPrimitive>>[] meshPrimitiveCluster;
        List<ImageCreateContext> imageCreateContexts;
#if KTX_UNITY
        List<KtxLoadContextBase> ktxLoadContextsBuffer;
#endif // KTX_UNITY

        
        /// <summary>
        /// Loaded glTF images (Raw texture without sampler settings)
        /// <seealso cref="textures"/>
        /// </summary>
        Texture2D[] images;
        
        /// <summary>
        /// In glTF a texture is an image with a certain sampler setting applied.
        /// So any `images` member is also in `textures`, but not necessary the
        /// other way around.
        /// /// <seealso cref="images"/>
        /// </summary>
        Texture2D[] textures;
        
        ImageFormat[] imageFormats;
        bool[] imageReadable;
        bool[] imageGamma;

        /// optional glTF-binary buffer
        /// https://github.com/KhronosGroup/glTF/tree/master/specification/2.0#binary-buffer
        GlbBinChunk? glbBinChunk;

#if MESHOPT
        Dictionary<int, NativeArray<byte>> meshoptBufferViews;
        NativeArray<int> meshoptReturnValues;
        JobHandle meshoptJobHandle;
#endif

        /// <summary>
        /// Unity's animation system addresses target GameObjects by hierarchical name.
        /// To make sure names are consistent and have no conflicts they are precalculated
        /// and stored in this array.
        /// </summary>
        string[] nodeNames;
        
#endregion VolatileData

#region VolatileDataInstantiation

        /// <summary>
        /// These members are only used during loading and instantiation phase.
        /// TODO: Provide dispose method to free up memory after all instantiations
        /// happened. Maybe a plain Destroy/OnDestroy.
        /// </summary>

        /// Main glTF data structure
        Root gltfRoot;
        UnityEngine.Material[] materials;
        List<UnityEngine.Object> resources;

        Primitive[] primitives;
        int[] meshPrimitiveIndex;
        Matrix4x4[][] skinsInverseBindMatrices;
#if UNITY_ANIMATION
        AnimationClip[] animationClips;
#endif

#if UNITY_EDITOR
        /// <summary>
        /// Required for Editor import only to preserve default/fallback materials
        /// </summary>
        public UnityEngine.Material defaultMaterial;
#endif
        
#endregion VolatileDataInstantiation

        bool loadingDone = false;
        /// <summary>
        /// True, when loading has finished and glTF can be instantiated
        /// </summary>
        public bool LoadingDone { get { return loadingDone; } private set { this.loadingDone = value; } }
        
        bool loadingError = false;
        /// <summary>
        /// True if an error happened during glTF loading
        /// </summary>
        public bool LoadingError { get { return loadingError; } private set { this.loadingError = value; } }

        ICodeLogger logger;
        
        public GltfImport(
            IDownloadProvider downloadProvider=null,
            IDeferAgent deferAgent=null,
            IMaterialGenerator materialGenerator=null,
            ICodeLogger logger = null
            )
        {
            this.downloadProvider = downloadProvider ?? new DefaultDownloadProvider();

            if (deferAgent == null) {
                if (defaultDeferAgent == null) {
                    defaultDeferAgent = new GameObject("glTFast_DeferAgent").AddComponent<TimeBudgetPerFrameDeferAgent>(); 
                }
                this.deferAgent = defaultDeferAgent;
            } else {
                this.deferAgent = deferAgent; 
            }
            this.materialGenerator = materialGenerator ?? Materials.MaterialGenerator.GetDefaultMaterialGenerator();

            this.logger = logger;
        }

#region Public

        /// <summary>
        /// Load a glTF file (JSON or binary)
        /// The URL can be a file path (using the "file://" scheme) or a web adress.
        /// </summary>
        /// <param name="url">Uniform Resource Locator. Can be a file path (using the "file://" scheme) or a web adress.</param>
        /// <param name="importSettings">Import Settings (<see cref="ImportSettings"/> for details)</param>
        public async Task<bool> Load( string url, ImportSettings importSettings = null ) {
            return await Load(new Uri(url,UriKind.RelativeOrAbsolute), importSettings);
        }
        
        /// <summary>
        /// Load a glTF file (JSON or binary)
        /// The URL can be a file path (using the "file://" scheme) or a web adress.
        /// </summary>
        /// <param name="url">Uniform Resource Locator. Can be a file path (using the "file://" scheme) or a web adress.</param>
        /// <param name="importSettings">Import Settings (<see cref="ImportSettings"/> for details)</param>
        public async Task<bool> Load( Uri url, ImportSettings importSettings = null) {
            settings = importSettings ?? new ImportSettings();
            return await LoadRoutine(url);
        }
        
        /// <summary>
        /// Load a glTF-binary asset from a byte array.
        /// </summary>
        /// <param name="bytes">byte array containing glTF-binary</param>
        /// <param name="uri">Base URI for relative paths of external buffers or images</param>
        /// <param name="importSettings">Import Settings (<see cref="ImportSettings"/> for details)</param>
        /// <returns>True if loading was successful, false otherwise</returns>
        public async Task<bool> LoadGltfBinary(byte[] bytes, Uri uri = null, ImportSettings importSettings = null) {
            settings = importSettings ?? new ImportSettings();
            var success = await LoadGltfBinaryBuffer(bytes,uri);
            if(success) await LoadContent();
            success = success && await Prepare();
            DisposeVolatileData();
            loadingError = !success;
            loadingDone = true;
            return success;
        }

        /// <summary>
        /// Creates an instance of the main scene of the glTF ( "scene" property in the JSON at root level; <seealso cref="defaultSceneIndex"/>)
        /// If the main scene index is not set, it instantiates nothing (as defined in the glTF 2.0 specification)
        /// </summary>
        /// <param name="parent">Transform that the scene will get parented to</param>
        /// <returns></returns>
        public bool InstantiateMainScene( Transform parent ) {
            var instantiator = new GameObjectInstantiator(this, parent);
            var success = InstantiateMainScene(instantiator);
            return success;
        }

        /// <summary>
        /// Creates an instance of the main scene of the glTF ( "scene" property in the JSON at root level; <seealso cref="defaultSceneIndex"/>)
        /// If the main scene index is not set, it instantiates nothing (as defined in the glTF 2.0 specification)
        /// </summary>
        /// <param name="instantiator">Instantiator implementation; Receives and processes the scene data</param>
        /// <returns></returns>
        public bool InstantiateMainScene(IInstantiator instantiator) {
            if (!loadingDone || loadingError) return false;
            // According to glTF specification, loading nothing is
            // the correct behavior
            if (gltfRoot.scene < 0) return true;
            return InstantiateScene(instantiator, gltfRoot.scene);
        }

        /// <summary>
        /// Creates an instance of the scene specified by the scene index.
        /// <seealso cref="sceneCount"/>
        /// <seealso cref="GetSceneName"/>
        /// </summary>
        /// <param name="parent">Transform that the scene will get parented to</param>
        /// <param name="sceneIndex">Index of the scene to be instantiated</param>
        /// <returns></returns>
        public bool InstantiateScene( Transform parent, int sceneIndex = 0) {
            if (!loadingDone || loadingError) return false;
            if (sceneIndex < 0 || sceneIndex > gltfRoot.scenes.Length) return false;
            var instantiator = new GameObjectInstantiator(this, parent);
            var success = InstantiateScene(instantiator,sceneIndex);
            return success;
        }

        /// <summary>
        /// Creates an instance of the scene specified by the scene index.
        /// <seealso cref="sceneCount"/>
        /// <seealso cref="GetSceneName"/>
        /// </summary>
        /// <param name="instantiator">Instantiator implementation; Receives and processes the scene data</param>
        /// <param name="sceneIndex">Index of the scene to be instantiated</param>
        /// <returns></returns>
        public bool InstantiateScene( IInstantiator instantiator, int sceneIndex = 0 ) {
            if (!loadingDone || loadingError) return false;
            if (sceneIndex < 0 || sceneIndex > gltfRoot.scenes.Length) return false;
            InstantiateSceneInternal( gltfRoot, instantiator, sceneIndex );
            return true;
        }
        
        /// <summary>
        /// Creates an instance of the entire glTF (all scenes).
        /// Please prefer to instantiate the main scene or selected scenes instead.
        /// </summary>
        /// <param name="parent">Transform that the scenes will get parented to</param>
        /// <returns></returns>
        [Obsolete("Replace with InstantiateMainScene or InstantiateScene")]
        public bool InstantiateGltf(Transform parent) {
            var instantiator = new GameObjectInstantiator(this, parent, new ConsoleLogger());
            var success = InstantiateGltf(instantiator);
            return success;
        }

        /// <summary>
        /// Instantiates the entire glTF (all scenes).
        /// Please prefer to instantiate the main scene or selected scenes instead.
        /// </summary>
        /// <param name="instantiator">Instantiator implementation; Receives and processes the scenes' data</param>
        /// <returns></returns>
        [Obsolete("Replace with InstantiateMainScene or InstantiateScene")]
        public bool InstantiateGltf(IInstantiator instantiator) {
            if (!loadingDone || loadingError) return false;
            for (int sceneIndex = 0; sceneIndex < sceneCount; sceneIndex++) {
                InstantiateScene(instantiator, sceneIndex);
            }
            return true;
        }

        [Obsolete("Use Dispose instead")]
        public void Destroy() {
            Dispose();
        }

        /// <summary>
        /// Frees up memory by disposing all sub assets.
        /// There can be no instantiation or other element access afterwards.
        /// </summary>
        public void Dispose() {
            if(materials!=null) {
                foreach( var material in materials ) {
                    SafeDestroy(material);
                }
                materials = null;
            }
            
#if UNITY_ANIMATION
            if (animationClips != null) {
                foreach( var clip in animationClips ) {
                    SafeDestroy(clip);
                }
                animationClips = null;
            }
#endif

            if(textures!=null) {
                foreach( var texture in textures ) {
                    SafeDestroy(texture);
                }
                textures = null;
            }

            if (accessorData != null) {
                foreach (var ad in accessorData) {
                    ad?.Dispose();
                }
                accessorData = null;
            }
            
            if(resources!=null) {
                foreach( var resource in resources ) {
                    SafeDestroy(resource);
                }
                resources = null;
            }
        }

        public int materialCount => materials?.Length ?? 0;
        public int imageCount => images?.Length ?? 0;
        public int textureCount => textures?.Length ?? 0;
        public int? defaultSceneIndex => gltfRoot != null && gltfRoot.scene >= 0 ? gltfRoot.scene : (int?) null; 
        public int sceneCount => gltfRoot?.scenes?.Length ?? 0;

        public string GetSceneName(int sceneIndex) {
            return gltfRoot?.scenes?[sceneIndex]?.name;
        }
        
        public UnityEngine.Material GetMaterial( int index = 0 ) {
            if(materials!=null && index >= 0 && index < materials.Length ) {
                return materials[index];
            }
            return null;
        }

        public UnityEngine.Material GetDefaultMaterial() {
#if UNITY_EDITOR
            if (defaultMaterial == null) {
                defaultMaterial = materialGenerator.GetDefaultMaterial();
            }
            return defaultMaterial;
#else
            return materialGenerator.GetDefaultMaterial();
#endif
        }
        
        public Texture2D GetImage( int index = 0 ) {
            if(images!=null && index >= 0 && index < images.Length ) {
                return images[index];
            }
            return null;
        }

        public Texture2D GetTexture( int index = 0 ) {
            if(textures!=null && index >= 0 && index < textures.Length ) {
                return textures[index];
            }
            return null;
        }
        
#if UNITY_ANIMATION
        public AnimationClip[] GetAnimationClips() {
            return animationClips;
        }
#endif

        public UnityEngine.Mesh[] GetMeshes() {
            if (primitives == null || primitives.Length < 1) return null;
            var result = new UnityEngine.Mesh[primitives.Length];
            for (var index = 0; index < primitives.Length; index++) {
                var primitive = primitives[index];
                result[index] = primitive.mesh;
            }
            return result;
        }

        public Camera GetSourceCamera(uint index) {
            if (gltfRoot?.cameras != null && index < gltfRoot.cameras.Length) {
                return gltfRoot.cameras[index];
            }
            return null;
        }
        
        public Material GetSourceMaterial(int index = 0) {
            if (gltfRoot?.materials != null && index >= 0 && index < gltfRoot.materials.Length) {
                return gltfRoot.materials[index];
            }
            return null;
        }

        public Texture GetSourceTexture(int index = 0) {
            if (gltfRoot?.textures != null && index >= 0 && index < gltfRoot.textures.Length) {
                return gltfRoot.textures[index];
            }
            return null;
        }

        public Image GetSourceImage(int index = 0) {
            if (gltfRoot?.images != null && index >= 0 && index < gltfRoot.images.Length) {
                return gltfRoot.images[index];
            }
            return null;
        }
        
#endregion Public

        async Task<bool> LoadRoutine( Uri url ) {

            var download = await downloadProvider.Request(url);
            var success = download.success;
            
            if(success) {

                bool? gltfBinary = download.isBinary;
                if (!gltfBinary.HasValue)
                {
                    gltfBinary = UriHelper.IsGltfBinary(url);
                }

                if (gltfBinary ?? false) {
                    success = await LoadGltfBinaryBuffer(download.data,url);
                } else {
                    success = await LoadGltf(download.text,url);
                }
                if(success) {
                    success = await LoadContent();
                }
                success = success && await Prepare();
            } else {
                logger?.Error(LogCode.Download,download.error,url.ToString());
            }

            DisposeVolatileData();
            loadingError = !success;
            loadingDone = true;
            return success;
        }

        async Task<bool> LoadContent() {

            var success = await WaitForBufferDownloads();
            downloadTasks?.Clear();

#if MESHOPT
            if (success) {
                MeshoptDecode();
            }
#endif
            
            if (textureDownloadTasks != null) {
                success = success && await WaitForTextureDownloads();
                textureDownloadTasks.Clear();
            }
            
#if KTX_UNITY
            if (ktxDownloadTasks != null) {
                success = success && await WaitForKtxDownloads();
                ktxDownloadTasks.Clear();
            }
#endif // KTX_UNITY

            return success;
        }

        async Task<bool> ParseJsonAndLoadBuffers( string json, Uri baseUri ) {

            var predictedTime = json.Length / (float)k_JsonParseSpeed;
#if GLTFAST_THREADS && !MEASURE_TIMINGS
            if (deferAgent.ShouldDefer(predictedTime)) {
                // JSON is larger than threshold
                // => parse in a thread
                gltfRoot = await Task.Run( () => JsonParser.ParseJson(json) );
            } else
#endif
            {
                // Parse immediately on main thread
                gltfRoot = JsonParser.ParseJson(json);
                // Loading subsequent buffers and images has to start asap.
                // That's why parsing JSON right away is *very* important. 
            }

            if(!CheckExtensionSupport(gltfRoot)) {
                return false;
            }

            var bufferCount = gltfRoot.buffers?.Length ?? 0;
            if(bufferCount>0) {
                buffers = new byte[bufferCount][];
                bufferHandles = new GCHandle?[bufferCount];
                nativeBuffers = new NativeArray<byte>[bufferCount];
                binChunks = new GlbBinChunk[bufferCount];
            }

            for( int i=0; i<bufferCount;i++) {
                var buffer = gltfRoot.buffers[i];
                if( !string.IsNullOrEmpty(buffer.uri) ) {
                    if(buffer.uri.StartsWith("data:")) {
                        var decodedBuffer = await DecodeEmbedBufferAsync(
                            buffer.uri,
                            true // usually there's just one buffer and it's time-critical
                            );
                        buffers[i] = decodedBuffer.Item1;
                        if(buffers[i]==null) {
                            logger?.Error(LogCode.EmbedBufferLoadFailed);
                            return false;
                        }
                    } else {
                        LoadBuffer( i, UriHelper.GetUriString(buffer.uri,baseUri) );
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Validates required and used glTF extensions and reports unsupported ones.
        /// </summary>
        /// <param name="gltfRoot"></param>
        /// <returns>False if a required extension is not supported. True otherwise.</returns>
        bool CheckExtensionSupport (Root gltfRoot) {
            if(gltfRoot.extensionsRequired!=null) {
                foreach(var ext in gltfRoot.extensionsRequired) {
                    var supported = supportedExtensions.Contains(ext);
                    if(!supported) {
#if !DRACO_UNITY
                        if(ext==Extensions.DracoMeshCompression) {
                            logger?.Error(LogCode.PackageMissing,"DracoUnity",ext);
                        } else
#endif
#if !KTX_UNITY
                        if(ext==Extensions.TextureBasisUniversal) {
                            logger?.Error(LogCode.PackageMissing,"KtxUnity",ext);
                        } else
#endif
                        {
                            logger?.Error(LogCode.ExtensionUnsupported,ext);
                        }
                        return false;
                    }
                }
            }
            if(gltfRoot.extensionsUsed!=null) {
                foreach(var ext in gltfRoot.extensionsUsed) {
                    var supported = supportedExtensions.Contains(ext);
                    if(!supported) {
#if !DRACO_UNITY
                        if(ext==Extensions.DracoMeshCompression) {
                            logger?.Warning(LogCode.PackageMissing,"DracoUnity",ext);
                        } else
#endif
#if !KTX_UNITY
                        if(ext==Extensions.TextureBasisUniversal) {
                            logger?.Warning(LogCode.PackageMissing,"KtxUnity",ext);
                        } else
#endif
                        {
                            logger?.Warning(LogCode.ExtensionUnsupported,ext);
                        }
                    }
                }
            }
            return true;
        }

        async Task<bool> LoadGltf( string json, Uri url ) {
            var baseUri = UriHelper.GetBaseUri(url);
            var success = await ParseJsonAndLoadBuffers(json,baseUri);
            if(success) await LoadImages(baseUri);
            return success;
        }

        async Task LoadImages( Uri baseUri ) {
            
            if (gltfRoot.textures != null && gltfRoot.images != null && gltfRoot.materials!=null) {
                
                Profiler.BeginSample("LoadImages.Prepare");
                
                images = new Texture2D[gltfRoot.images.Length];
                imageFormats = new ImageFormat[gltfRoot.images.Length];

                if(QualitySettings.activeColorSpace==ColorSpace.Linear) {

                    imageGamma = new bool[gltfRoot.images.Length];

                    void SetImageGamma(TextureInfo txtInfo) {
                        if(
                            txtInfo != null &&
                            txtInfo.index >= 0 &&
                            txtInfo.index < gltfRoot.textures.Length
                        ) {
                            var imageIndex = gltfRoot.textures[txtInfo.index].GetImageIndex();
                            imageGamma[imageIndex] = true;
                        }
                    }
                    
                    for(int i=0;i<gltfRoot.materials.Length;i++) {
                        var mat = gltfRoot.materials[i];
                        if( mat.pbrMetallicRoughness != null ) {
                            SetImageGamma(mat.pbrMetallicRoughness.baseColorTexture);
                        }
                        SetImageGamma(mat.emissiveTexture);
                        if( mat.extensions?.KHR_materials_pbrSpecularGlossiness != null )
                        {
                            SetImageGamma(mat.extensions.KHR_materials_pbrSpecularGlossiness.diffuseTexture);
                            SetImageGamma(mat.extensions.KHR_materials_pbrSpecularGlossiness.specularGlossinessTexture);
                        }
                    }
                }

#if KTX_UNITY
                // Derive image type from texture extension
                for (int i = 0; i < gltfRoot.textures.Length; i++) {
                    var texture = gltfRoot.textures[i];
                    if(texture.isKtx) {
                        var imgIndex = texture.GetImageIndex();
                        imageFormats[imgIndex] = ImageFormat.KTX;
                    }
                }
#endif // KTX_UNITY
                
                // Determine which images need to be readable, because they
                // are applied using different samplers.
                var imageVariants = new HashSet<int>[images.Length];
                foreach (var txt in gltfRoot.textures) {
                    var imageIndex = txt.GetImageIndex();
                    if(imageVariants[imageIndex]==null) {
                        imageVariants[imageIndex] = new HashSet<int>();
                    }
                    imageVariants[imageIndex].Add(txt.sampler);
                }

                imageReadable = new bool[images.Length];
                for (int i = 0; i < images.Length; i++) {
                    imageReadable[i] = imageVariants[i]!=null && imageVariants[i].Count > 1;
                }

                Profiler.EndSample();
                List<Task> imageTasks = null;

                for (int i = 0; i < gltfRoot.images.Length; i++) {
                    var img = gltfRoot.images[i];

                    if(!string.IsNullOrEmpty(img.uri) && img.uri.StartsWith("data:")) {
                        var decodedBufferTask = DecodeEmbedBufferAsync(img.uri);
                        if (imageTasks == null) {
                            imageTasks = new List<Task>();
                        }
                        var imageTask = LoadImageFromBuffer(decodedBufferTask, i, img);
                        imageTasks.Add(imageTask);
                    } else {
                        ImageFormat imgFormat;
                        if(imageFormats[i]==ImageFormat.Unknown) {
                            if(string.IsNullOrEmpty(img.mimeType)) {
                                imgFormat = UriHelper.GetImageFormatFromUri(img.uri);
                            } else {
                                imgFormat = GetImageFormatFromMimeType(img.mimeType);
                            }
                            imageFormats[i] = imgFormat;
                        } else {
                            imgFormat=imageFormats[i];
                        }

                        if (imgFormat!=ImageFormat.Unknown) {
                            if (img.bufferView < 0) {
                                // Not Inside buffer
                                if(!string.IsNullOrEmpty(img.uri)) {
                                    LoadTexture(i,UriHelper.GetUriString(img.uri,baseUri), !imageReadable[i], imgFormat==ImageFormat.KTX);
                                } else {
                                    logger?.Error(LogCode.MissingImageURL);
                                }
                            } 
                        } else {
                            logger?.Error(LogCode.ImageFormatUnknown,i.ToString(),img.uri);
                        }
                    }
                }

                if (imageTasks != null) {
                    await Task.WhenAll(imageTasks);
                }
            }
        }

        async Task LoadImageFromBuffer(Task<Tuple<byte[],string>> decodeBufferTask, int imageIndex, Image img) {
            var decodedBuffer = await decodeBufferTask;
            await deferAgent.BreakPoint();
            Profiler.BeginSample("LoadImages.FromBase64");
            var data = decodedBuffer.Item1;
            string mimeType = decodedBuffer.Item2;
            var imgFormat = GetImageFormatFromMimeType(mimeType);
            if (data == null || imgFormat == ImageFormat.Unknown) {
                logger?.Error(LogCode.EmbedImageLoadFailed);
                return;
            }

            if (imageFormats[imageIndex] != ImageFormat.Unknown && imageFormats[imageIndex] != imgFormat) {
                logger?.Error(LogCode.EmbedImageInconsistentType, imageFormats[imageIndex].ToString(), imgFormat.ToString());
            }

            imageFormats[imageIndex] = imgFormat;
            if (imageFormats[imageIndex] != ImageFormat.Jpeg && imageFormats[imageIndex] != ImageFormat.PNG) {
                // TODO: support embed KTX textures
                logger?.Error(LogCode.EmbedImageUnsupportedType, imageFormats[imageIndex].ToString());
            }

            // TODO: Investigate alternative: native texture creation in worker thread
            bool forceSampleLinear = imageGamma != null && !imageGamma[imageIndex];
            var txt = CreateEmptyTexture(img, imageIndex, forceSampleLinear);
            txt.LoadImage(data,!imageReadable[imageIndex]);
            images[imageIndex] = txt;
            Profiler.EndSample();
        }

        async Task<bool> WaitForBufferDownloads() {
            if(downloadTasks!=null) {
                foreach( var downloadPair in downloadTasks ) {
                    var download = await downloadPair.Value;
                    if (download.success) {
                        Profiler.BeginSample("GetData");
                        buffers[downloadPair.Key] = download.data;
                        Profiler.EndSample();
                    } else {
                        logger?.Error(LogCode.BufferLoadFailed,download.error,downloadPair.Key.ToString());
                        return false;
                    }
                }
            }

            if(buffers!=null) {
                Profiler.BeginSample("CreateGlbBinChungs");
                for( int i=0; i<buffers.Length; i++ ) {
                    if(i==0 && glbBinChunk.HasValue) {
                        // Already assigned in LoadGltfBinary
                        continue;
                    }
                    var b = buffers[i];
                    if (b != null) {
                        binChunks[i] = new GlbBinChunk(0,(uint) b.Length);
                    }
                }
                Profiler.EndSample();
            }
            return true;
        }

        async Task<bool> WaitForTextureDownloads() {
            foreach( var dl in textureDownloadTasks ) {
                var www = await dl.Value;
                
                if(www.success) {
                    var imageIndex = dl.Key;
                    bool forceSampleLinear = imageGamma!=null && !imageGamma[imageIndex];
                    Texture2D txt;
                    // TODO: Loading Jpeg/PNG textures like this creates major frame stalls. Main thread is waiting
                    // on Render thread, which is occupied by Gfx.UploadTextureData for 19 ms for a 2k by 2k texture
                    if(forceSampleLinear || settings.generateMipMaps) {
                        txt = CreateEmptyTexture(gltfRoot.images[imageIndex], imageIndex, forceSampleLinear);
                        // TODO: Investigate for NativeArray variant to avoid `www.data`
                        txt.LoadImage(www.data,!imageReadable[imageIndex]);
                    } else {
                        txt = www.texture;
                        txt.name = GetImageName(gltfRoot.images[imageIndex], imageIndex);
                    }
                    images[imageIndex] = txt;
                    await deferAgent.BreakPoint();
                } else {
                    logger?.Error(LogCode.TextureDownloadFailed,www.error,dl.Key.ToString());
                    return false;
                }
            }
            return true;
        }


#if KTX_UNITY
        async Task<bool> WaitForKtxDownloads() {
            var tasks = new Task<bool>[ktxDownloadTasks.Count];
            var i = 0;
            foreach( var dl in ktxDownloadTasks ) {
                tasks[i] = ProcessKtxDownload(dl.Key, dl.Value);
                i++;
            }
            await Task.WhenAll(tasks);
            foreach (var task in tasks) {
                if (!task.Result) return false;
            }
            return true;
        }
        
        async Task<bool> ProcessKtxDownload(int index, Task<IDownload> downloadTask) {
            var www = await downloadTask;
            if(www.success) {
                var ktxContext = new KtxLoadContext(index,www.data);
                var forceSampleLinear = imageGamma!=null && !imageGamma[ktxContext.imageIndex];
                var textureResult = await ktxContext.LoadKtx(forceSampleLinear);
                images[ktxContext.imageIndex] = textureResult.texture;
                return true;
            } else {
                logger?.Error(LogCode.TextureDownloadFailed,www.error,index.ToString());
                return false;
            }
        }
#endif // KTX_UNITY

        void LoadBuffer( int index, Uri url ) {
            Profiler.BeginSample("LoadBuffer");
            if(downloadTasks==null) {
                downloadTasks = new Dictionary<int, Task<IDownload>>();
            }
            downloadTasks.Add(index,downloadProvider.Request(url));
            Profiler.EndSample();
        }

        async Task<Tuple<byte[],string>> DecodeEmbedBufferAsync(string encodedBytes,bool timeCritical = false) {
            var predictedTime = encodedBytes.Length / (float)k_Base64DecodeSpeed;
#if MEASURE_TIMINGS
            var stopWatch = new Stopwatch();
            stopWatch.Start();
#elif GLTFAST_THREADS
            if (!timeCritical || deferAgent.ShouldDefer(predictedTime)) {
                // TODO: Not sure if thread safe? Maybe create a dedicated Report for the thread and merge them afterwards? 
                return await Task.Run(() => DecodeEmbedBuffer(encodedBytes,logger));
            }
#endif
            await deferAgent.BreakPoint(predictedTime);
            var decodedBuffer = DecodeEmbedBuffer(encodedBytes,logger);
#if MEASURE_TIMINGS
            stopWatch.Stop();
            var elapsedSeconds = stopWatch.ElapsedMilliseconds / 1000f;
            var relativeDiff = (elapsedSeconds-predictedTime) / predictedTime;
            if (Mathf.Abs(relativeDiff) > .2f) {
                Debug.LogWarning($"Base 64 unexpected duration! diff: {relativeDiff:0.00}% predicted: {predictedTime} sec actual: {elapsedSeconds} sec");
            }
            var throughput = encodedBytes.Length / elapsedSeconds;
            Debug.Log($"Base 64 throughput: {throughput} bytes/sec ({encodedBytes.Length} bytes in {elapsedSeconds} seconds)");
#endif
            return decodedBuffer;
        }

        static Tuple<byte[],string> DecodeEmbedBuffer(string encodedBytes,ICodeLogger logger) {
            Profiler.BeginSample("DecodeEmbedBuffer");
            logger?.Warning(LogCode.EmbedSlow);
            var mediaTypeEnd = encodedBytes.IndexOf(';',5,Math.Min(encodedBytes.Length-5,1000) );
            if(mediaTypeEnd<0) {
                Profiler.EndSample();
                return null;
            }
            var mimeType = encodedBytes.Substring(5,mediaTypeEnd-5);
            var tmp = encodedBytes.Substring(mediaTypeEnd+1,7);
            if(tmp!="base64,") {
                Profiler.EndSample();
                return null;
            }
            var data = System.Convert.FromBase64String(encodedBytes.Substring(mediaTypeEnd+8));
            Profiler.EndSample();
            return new Tuple<byte[], string>(data, mimeType);
        }

        void LoadTexture( int index, Uri url, bool nonReadable, bool isKtx ) {

            Profiler.BeginSample("LoadTexture");

            if(isKtx) {
#if KTX_UNITY
                var downloadTask = downloadProvider.Request(url);
                if(ktxDownloadTasks==null) {
                    ktxDownloadTasks = new Dictionary<int, Task<IDownload>>();
                }
                ktxDownloadTasks.Add(index, downloadTask);
#else
                logger?.Error(LogCode.PackageMissing,"KtxUnity",Extensions.TextureBasisUniversal);
                Profiler.EndSample();
                return;
#endif // KTX_UNITY
            } else {
                var downloadTask = downloadProvider.RequestTexture(url,nonReadable);
                if(textureDownloadTasks==null) {
                    textureDownloadTasks = new Dictionary<int, Task<ITextureDownload>>();
                }
                textureDownloadTasks.Add(index, downloadTask);
            }
            Profiler.EndSample();
        }

        async Task<bool> LoadGltfBinaryBuffer( byte[] bytes, Uri uri = null ) {
            Profiler.BeginSample("LoadGltfBinary.Phase1");
            
            if (!GltfGlobals.IsGltfBinary(bytes)) {
                logger?.Error(LogCode.GltfNotBinary);
                Profiler.EndSample();
                return false;
            }

            uint version = BitConverter.ToUInt32( bytes, 4 );
            //uint length = BitConverter.ToUInt32( bytes, 8 );
            
            if (version != 2) {
                logger?.Error(LogCode.GltfUnsupportedVersion,version.ToString());
                Profiler.EndSample();
                return false;
            }

            int index = 12; // first chunk header

            var baseUri = UriHelper.GetBaseUri(uri);

            Profiler.EndSample();
            
            while( index < bytes.Length ) {
                uint chLength = BitConverter.ToUInt32( bytes, index );
                index += 4;
                uint chType = BitConverter.ToUInt32( bytes, index );
                index += 4;

                if (chType == (uint)ChunkFormat.BIN) {
                    Assert.IsFalse(glbBinChunk.HasValue); // There can only be one binary chunk
                    glbBinChunk = new GlbBinChunk( index, chLength);
                }
                else if (chType == (uint)ChunkFormat.JSON) {
                    Assert.IsNull(gltfRoot);

                    Profiler.BeginSample("GetJSON");
                    string json = System.Text.Encoding.UTF8.GetString(bytes, index, (int)chLength );
                    Profiler.EndSample();
                    
                    var success = await ParseJsonAndLoadBuffers(json,baseUri);

                    if(!success) {
                        return false;
                    }
                }
 
                index += (int) chLength;
            }
            
            if(gltfRoot==null) {
                logger?.Error(LogCode.ChunkJsonInvalid);
                return false;
            }
            
            if(glbBinChunk.HasValue && binChunks!=null) {
                binChunks[0] = glbBinChunk.Value;
                buffers[0] = bytes;
            }
            await LoadImages(baseUri);
            return true;
        }

        byte[] GetBuffer(int index) {
            return buffers[index];
        }

        NativeSlice<byte> GetBufferView(int bufferViewIndex,int offset = 0, int length = 0) {
            var bufferView = gltfRoot.bufferViews[bufferViewIndex];
#if MESHOPT
            if (bufferView.extensions?.EXT_meshopt_compression != null) {
                var fullSlice = meshoptBufferViews[bufferViewIndex];
                if (offset == 0 && length <= 0) {
                    return fullSlice;
                }
                Assert.IsTrue(offset >= 0);
                if (length <= 0) {
                    length = fullSlice.Length - offset;
                }
                Assert.IsTrue(offset+length <= fullSlice.Length);
                return  new NativeSlice<byte>(fullSlice,offset,length);
            } 
#endif
            return GetBufferViewSlice(bufferView,offset,length);
        }

        unsafe NativeSlice<byte> GetBufferViewSlice(
            BufferViewBase bufferView,
            int offset = 0,
            int length = 0
            )
        {
            Assert.IsTrue(offset >= 0);
            if (length <= 0) {
                length = bufferView.byteLength - offset;
            }
            Assert.IsTrue( offset+length <= bufferView.byteLength);
            
            var bufferIndex = bufferView.buffer;
            if(!nativeBuffers[bufferIndex].IsCreated) {
                Profiler.BeginSample("ConvertToNativeArray");
                var buffer = GetBuffer(bufferIndex);
                bufferHandles[bufferIndex] = GCHandle.Alloc(buffer,GCHandleType.Pinned);
                fixed (void* bufferAddress = &(buffer[0])) {
                    nativeBuffers[bufferIndex] = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<byte>(bufferAddress,buffer.Length,Allocator.None);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                    var safetyHandle = AtomicSafetyHandle.Create();
                    NativeArrayUnsafeUtility.SetAtomicSafetyHandle(array: ref nativeBuffers[bufferIndex], safetyHandle);
#endif
                }
                Profiler.EndSample();
            }
            var chunk = binChunks[bufferIndex];
            return new NativeSlice<byte>(
                nativeBuffers[bufferIndex],
                chunk.start + bufferView.byteOffset + offset,
                length
                );
        }

#if MESHOPT
        void MeshoptDecode() {
            if(gltfRoot.bufferViews!=null) {
                List<JobHandle> jobHandlesList = null;
                for (var i = 0; i < gltfRoot.bufferViews.Length; i++) {
                    var bufferView = gltfRoot.bufferViews[i];
                    if (bufferView.extensions?.EXT_meshopt_compression != null) {
                        var meshopt = bufferView.extensions?.EXT_meshopt_compression;
                        if (jobHandlesList == null) {
                            meshoptBufferViews = new Dictionary<int, NativeArray<byte>>();
                            jobHandlesList = new List<JobHandle>(gltfRoot.bufferViews.Length);
                            meshoptReturnValues = new NativeArray<int>(gltfRoot.bufferViews.Length, Allocator.TempJob);
                        }

                        var arr = new NativeArray<byte>(meshopt.count * meshopt.byteStride, Allocator.Persistent);
                        
                        var origBufferView = GetBufferViewSlice(meshopt);
                        
                        var jobHandle = Decode.DecodeGltfBuffer(
                            new NativeSlice<int>(meshoptReturnValues,i,1),
                            arr,
                            meshopt.count,
                            meshopt.byteStride,
                            origBufferView,
                            meshopt.modeEnum,
                            meshopt.filterEnum
                        );
                        jobHandlesList.Add(jobHandle);
                        meshoptBufferViews[i] = arr;
                    }
                }

                if (jobHandlesList != null) {
                    using (var jobHandles = new NativeArray<JobHandle>(jobHandlesList.ToArray(), Allocator.Temp)) {
                        meshoptJobHandle = JobHandle.CombineDependencies(jobHandles);
                    }
                }
            }
        }
        
        async Task<bool> WaitForMeshoptDecode() {
            var success = true;
            if (meshoptBufferViews != null) {
                while (!meshoptJobHandle.IsCompleted) {
                    await Task.Yield();
                }
                meshoptJobHandle.Complete();

                foreach (var returnValue in meshoptReturnValues) {
                    success &= returnValue == 0;
                }
                meshoptReturnValues.Dispose();
            }
            return success;
        }

#endif // MESHOPT

        async Task<bool> Prepare() {
            if(gltfRoot.meshes!=null) {
                meshPrimitiveIndex = new int[gltfRoot.meshes.Length+1];
            }

            resources = new List<UnityEngine.Object>();
            
            if( gltfRoot.images != null && gltfRoot.textures != null && gltfRoot.materials != null ) {
                if(images==null) {
                    images = new Texture2D[gltfRoot.images.Length];
                } else {
                    Assert.AreEqual(images.Length,gltfRoot.images.Length);
                }
                imageCreateContexts = new List<ImageCreateContext>();
#if KTX_UNITY
                await
#endif
                CreateTexturesFromBuffers(gltfRoot.images,gltfRoot.bufferViews,imageCreateContexts);
            }
            await deferAgent.BreakPoint();

            var success = true;

#if MESHOPT
            success = await WaitForMeshoptDecode();
            if (!success) return false;
#endif

            if(gltfRoot.accessors!=null) {
                success = await LoadAccessorData(gltfRoot);
                await deferAgent.BreakPoint();

                while(!accessorJobsHandle.IsCompleted) {
                    await Task.Yield();
                }
                accessorJobsHandle.Complete();
                foreach(var ad in accessorData) {
                    ad?.Unpin();
                }
            }
            if (!success) return success;

            if(gltfRoot.meshes!=null) {
                await CreatePrimitiveContexts(gltfRoot);
            }

#if KTX_UNITY
            if(ktxLoadContextsBuffer!=null) {

                var ktxTasks = new Task<KtxUnity.TextureResult>[ktxLoadContextsBuffer.Count];
                for (var i = 0; i < ktxLoadContextsBuffer.Count; i++) {
                    var ktx = ktxLoadContextsBuffer[i];
                    var forceSampleLinear = imageGamma!=null && !imageGamma[ktx.imageIndex];
                    ktxTasks[i] = ktx.LoadKtx(forceSampleLinear);
                    await deferAgent.BreakPoint();
                }
                await Task.WhenAll(ktxTasks);

                for (var i = 0; i < ktxLoadContextsBuffer.Count; i++) {
                    var ktx = ktxLoadContextsBuffer[i];
                    images[ktx.imageIndex] = ktxTasks[i].Result.texture;
                }
                ktxLoadContextsBuffer.Clear();
            }
#endif // KTX_UNITY

            if(imageCreateContexts!=null) {
                var imageCreateContextsLeft = true;
                while (imageCreateContextsLeft) {
                    var loadedAny = false;
                    for (int i = imageCreateContexts.Count - 1; i >= 0; i--) {
                        var jh = imageCreateContexts[i];
                        if(jh.jobHandle.IsCompleted) {
                            jh.jobHandle.Complete();
                            images[jh.imageIndex].LoadImage(jh.buffer,!imageReadable[jh.imageIndex]);
                            jh.gcHandle.Free();
                            imageCreateContexts.RemoveAt(i);
                            loadedAny = true;
                            await deferAgent.BreakPoint();
                        }
                    }
                    imageCreateContextsLeft = imageCreateContexts.Count > 0;
                    if(!loadedAny && imageCreateContextsLeft) {
                        await Task.Yield();
                    }
                }
                imageCreateContexts = null;
            }

            if(images!=null && gltfRoot.textures!=null) {
                textures = new Texture2D[gltfRoot.textures.Length];
                var imageVariants = new Dictionary<int,Texture2D>[images.Length];
                for (int textureIndex = 0; textureIndex < gltfRoot.textures.Length; textureIndex++)
                {
                    var txt = gltfRoot.textures[textureIndex];
                    var imageIndex = txt.GetImageIndex();
                    var img = images[imageIndex];
                    if(imageVariants[imageIndex]==null) {
                        if(txt.sampler>=0) {
                            gltfRoot.samplers[txt.sampler].Apply(img, settings.defaultMinFilterMode, settings.defaultMagFilterMode);
                        }
                        imageVariants[imageIndex] = new Dictionary<int, Texture2D>();
                        imageVariants[imageIndex][txt.sampler] = img;
                        textures[textureIndex] = img;
                    } else {
                        if (imageVariants[imageIndex].TryGetValue(txt.sampler, out var imgVariant)) {
                            textures[textureIndex] = imgVariant;
                        } else {
                            var newImg = Texture2D.Instantiate(img);
                            resources.Add(newImg);
#if DEBUG
                            newImg.name = string.Format("{0}_sampler{1}",img.name,txt.sampler);
                            logger?.Warning(LogCode.ImageMultipleSamplers,imageIndex.ToString());
#endif
                            if(txt.sampler>=0) {
                                gltfRoot.samplers[txt.sampler].Apply(newImg, settings.defaultMinFilterMode, settings.defaultMagFilterMode);
                            }
                            imageVariants[imageIndex][txt.sampler] = newImg;
                            textures[textureIndex] = newImg;
                        }
                        
                    }
                }
            }

            if(gltfRoot.materials!=null) {
                materials = new UnityEngine.Material[gltfRoot.materials.Length];
                for(int i=0;i<materials.Length;i++) {
                    await deferAgent.BreakPoint(.0001f);
                    Profiler.BeginSample("GenerateMaterial");
                    materialGenerator.SetLogger(logger);
                    materials[i] = materialGenerator.GenerateMaterial(gltfRoot.materials[i],this);
                    materialGenerator.SetLogger(null);
                    Profiler.EndSample();
                }
            }
            await deferAgent.BreakPoint();

            if(primitiveContexts!=null) {
                for(int i=0;i<primitiveContexts.Length;i++) {
                    var primitiveContext = primitiveContexts[i];
                    if(primitiveContext==null) continue;
                    while(!primitiveContext.IsCompleted) {
                        await Task.Yield();
                    }
                }
                await deferAgent.BreakPoint();
                
                await AssignAllAccessorData(gltfRoot);

                for(int i=0;i<primitiveContexts.Length;i++) {
                    var primitiveContext = primitiveContexts[i];
                    while(!primitiveContext.IsCompleted) {
                        await Task.Yield();
                    }
                    var primitive = primitiveContext.CreatePrimitive();
                    if(primitive.HasValue) {
                        primitives[primitiveContext.primtiveIndex] = primitive.Value;
                        resources.Add(primitive.Value.mesh);
                    } else {
                        success = false;
                        break;
                    }

                    await deferAgent.BreakPoint();
                }
            }

#if UNITY_ANIMATION
            if (gltfRoot.hasAnimation) {
                if (settings.nodeNameMethod != ImportSettings.NameImportMethod.OriginalUnique) {
                    logger?.Info(LogCode.NamingOverride);
                    settings.nodeNameMethod = ImportSettings.NameImportMethod.OriginalUnique;
                }
            }
#endif

            int[] parentIndex = null;

            if (settings.nodeNameMethod == ImportSettings.NameImportMethod.OriginalUnique) {
                parentIndex = CreateUniqueNames();
            }

#if UNITY_ANIMATION
            if (gltfRoot.hasAnimation && settings.animationMethod != ImportSettings.AnimationMethod.None) {
                
                animationClips = new AnimationClip[gltfRoot.animations.Length];
                for (var i = 0; i < gltfRoot.animations.Length; i++) {
                    var animation = gltfRoot.animations[i];
                    animationClips[i] = new AnimationClip();
                    animationClips[i].name = animation.name ?? $"Clip_{i}";
                    
                    // Legacy Animation requirement
                    animationClips[i].legacy = settings.animationMethod == ImportSettings.AnimationMethod.Legacy;
                    animationClips[i].wrapMode = WrapMode.Loop;

                    for (int j = 0; j < animation.channels.Length; j++) {
                        var channel = animation.channels[j];
                        if (channel.sampler < 0 || channel.sampler >= animation.samplers.Length) {
                            logger?.Error(LogCode.AnimationChannelSamplerInvalid, j.ToString());
                            continue;
                        }
                        var sampler = animation.samplers[channel.sampler];
                        if (channel.target.node < 0 || channel.target.node >= gltfRoot.nodes.Length) {
                            logger?.Error(LogCode.AnimationChannelNodeInvalid, j.ToString());
                            continue;
                        }
                        
                        var path = AnimationUtils.CreateAnimationPath(channel.target.node,nodeNames,parentIndex);
                        
                        var times = ((AccessorNativeData<float>) accessorData[sampler.input]).data;
                        
                        switch (channel.target.pathEnum) {
                            case AnimationChannel.Path.translation: {
                                var values= ((AccessorNativeData<Vector3>) accessorData[sampler.output]).data;
                                AnimationUtils.AddTranslationCurves(animationClips[i], path, times, values, sampler.interpolationEnum);
                                break;
                            }
                            case AnimationChannel.Path.rotation: {
                                var values= ((AccessorNativeData<Quaternion>) accessorData[sampler.output]).data;
                                AnimationUtils.AddRotationCurves(animationClips[i], path, times, values, sampler.interpolationEnum);
                                break;
                            }
                            case AnimationChannel.Path.scale: {
                                var values= ((AccessorNativeData<Vector3>) accessorData[sampler.output]).data;
                                AnimationUtils.AddScaleCurves(animationClips[i], path, times, values, sampler.interpolationEnum);
                                break;
                            }
                            case AnimationChannel.Path.weights: {
                                var values= ((AccessorNativeData<float>) accessorData[sampler.output]).data;
                                var node = gltfRoot.nodes[channel.target.node];
                                if (node.mesh < 0 || node.mesh >= gltfRoot.meshes.Length) {
                                    break;
                                }
                                var mesh = gltfRoot.meshes[node.mesh];
                                AnimationUtils.AddMorphTargetWeightCurves(
                                    animationClips[i],
                                    path,
                                    times,
                                    values,
                                    sampler.interpolationEnum,
                                    mesh.extras?.targetNames
                                    );
                                
                                // HACK BEGIN:
                                // Since meshes with multiple primitives that are not using
                                // identical vertex buffers are split up into separate Unity
                                // Meshes. Because of this, we have to duplicate the animation
                                // curves, so that all primitives are animated.
                                // TODO: Refactor primitive sub-meshing and remove this hack
                                // https://github.com/atteneder/glTFast/issues/153
                                var meshName = string.IsNullOrEmpty(mesh.name) ? PrimitiveName : mesh.name;
                                var primitiveCount = meshPrimitiveIndex[node.mesh + 1] - meshPrimitiveIndex[node.mesh];
                                for (var k = 1; k < primitiveCount; k++) {
                                    var primitiveName = $"{meshName}_{k}";
                                    AnimationUtils.AddMorphTargetWeightCurves(
                                        animationClips[i],
                                        $"{path}/{primitiveName}",
                                        times,
                                        values,
                                        sampler.interpolationEnum,
                                        mesh.extras?.targetNames
                                    );                                    
                                }
                                // HACK END
                                break;
                            }
                            default:
                                logger?.Error(LogCode.AnimationTargetPathUnsupported,channel.target.pathEnum.ToString());
                                break;
                        }
                    }
                }
            }
#endif

            // Dispose all accessor data buffers, except the ones needed for instantiation
            if (accessorData != null) {
                for (var index = 0; index < accessorData.Length; index++) {
                    if ((accessorUsage[index] & AccessorUsage.RequiredForInstantiation) == 0) {
                        accessorData[index]?.Dispose();
                        accessorData[index] = null;
                    }
                }
            }
            return success;
        }

        /// <summary>
        /// glTF nodes have no requirement to be named or have specific names.
        /// Some Unity systems like animation and importers require unique
        /// names for Nodes with the same parent. For each node this method creates
        /// names that are:
        /// - Not empty
        /// - Unique amongst nodes with identical parent node
        /// </summary>
        /// <returns>Array containing each node's parent node index (or -1 for root nodes)</returns>
        int[] CreateUniqueNames() {
            nodeNames = new string[gltfRoot.nodes.Length];
            var parentIndex = new int[gltfRoot.nodes.Length];

            for (var nodeIndex = 0; nodeIndex < gltfRoot.nodes.Length; nodeIndex++) {
                parentIndex[nodeIndex] = -1;
            }

            var childNames = new HashSet<string>();

            for (var nodeIndex = 0; nodeIndex < gltfRoot.nodes.Length; nodeIndex++) {
                var node = gltfRoot.nodes[nodeIndex];
                if (node.children != null) {
                    childNames.Clear();
                    foreach (var child in node.children) {
                        parentIndex[child] = nodeIndex;
                        nodeNames[child] = GetUniqueNodeName(gltfRoot, child, childNames);
                    }
                }
            }

            for (int sceneId = 0; sceneId < gltfRoot.scenes.Length; sceneId++) {
                childNames.Clear();
                var scene = gltfRoot.scenes[sceneId];
                if (scene.nodes != null) {
                    foreach (var nodeIndex in scene.nodes) {
                        nodeNames[nodeIndex] = GetUniqueNodeName(gltfRoot, nodeIndex, childNames);
                    }
                }
            }

            return parentIndex;
        }

        static string GetUniqueNodeName(Root gltf, uint index, ICollection<string> excludeNames) {
            if (gltf.nodes == null || index >= gltf.nodes.Length) return null;
            var name = gltf.nodes[index].name;
            if (string.IsNullOrWhiteSpace(name)) {
                var meshIndex = gltf.nodes[index].mesh;
                if (meshIndex >= 0) {
                    name = gltf.meshes[meshIndex].name;
                }
            }

            if (string.IsNullOrWhiteSpace(name)) {
                name = $"Node-{index}";
            }

            if (excludeNames != null) {
                if (excludeNames.Contains(name)) {
                    var i = 0;
                    string extName;
                    do {
                        extName = $"{name}_{i++}";
                    } while (excludeNames.Contains(extName));
                    excludeNames.Add(extName);
                    return extName;
                }
                excludeNames.Add(name);
            }
            return name;
        }

        /// <summary>
        /// Free up volatile loading resources
        /// </summary>
        void DisposeVolatileData() {

            if (vertexAttributes != null) {
                foreach (var vac in vertexAttributes.Values) {
                    vac.Dispose();
                }
            }
            vertexAttributes = null;
            
            primitiveContexts = null;

            // Unpin managed buffer arrays
            if (bufferHandles != null) {
                foreach (var t in bufferHandles) {
                    t?.Free();
                }
            }
            bufferHandles = null;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if(nativeBuffers!=null) {
                foreach (var nativeBuffer in nativeBuffers)
                {
                    if(nativeBuffer.IsCreated) {
                        var safetyHandle = NativeArrayUnsafeUtility.GetAtomicSafetyHandle(nativeBuffer);
                        AtomicSafetyHandle.Release(safetyHandle);
                    }
                }
            }
#endif
            nativeBuffers = null;

            buffers = null;
            
            binChunks = null;

            downloadTasks = null;
            textureDownloadTasks = null;
            
            accessorUsage = null;
            primitiveContexts = null;
            meshPrimitiveCluster = null;
            imageCreateContexts = null;
            images = null;
            imageFormats = null;
            imageReadable = null;
            imageGamma = null;
            glbBinChunk = null;
            
#if MESHOPT
            if(meshoptBufferViews!=null) {
                foreach (var nativeBuffer in meshoptBufferViews.Values) {
                    nativeBuffer.Dispose();
                }
                meshoptBufferViews = null;
            }
            if (meshoptReturnValues.IsCreated) {
                meshoptReturnValues.Dispose();
            }
#endif
        }

        void InstantiateSceneInternal( Root gltf, IInstantiator instantiator, int sceneId ) {
            
            // TODO: Make instantiation preemptive (via deferAgent) as well!

            void IterateNodes(uint nodeIndex, uint? parentIndex, Action<uint,uint?> callback) {
                var node = gltfRoot.nodes[nodeIndex];
                callback(nodeIndex,parentIndex);
                if (node.children != null) {
                    foreach (var child in node.children) {
                        IterateNodes(child,nodeIndex,callback);
                    }
                }
            }

            void CreateHierarchy(uint nodeIndex, uint? parentIndex) {
                var node = gltfRoot.nodes[nodeIndex];
                node.GetTransform(out var position, out var rotation, out var scale);

                instantiator.CreateNode(nodeIndex, position, rotation, scale);

                if (parentIndex.HasValue) {
                    instantiator.SetParent(nodeIndex, parentIndex.Value);
                }
            }

            void PopulateHierarchy(uint nodeIndex, uint? parentIndex) {
                
                var node = gltfRoot.nodes[nodeIndex];
                
                var goName = nodeNames==null ? node.name : nodeNames[nodeIndex];

                if(node.mesh>=0) {
                    var end = meshPrimitiveIndex[node.mesh+1];
                    var primitiveCount = 0;
                    for( var i=meshPrimitiveIndex[node.mesh]; i<end; i++ ) {
                        var primitive = primitives[i];
                        var mesh = primitive.mesh;
                        var meshName = string.IsNullOrEmpty(mesh.name) ? null : mesh.name;
                        // Fallback name for Node is first valid Mesh name
                        goName = goName ?? meshName;
                        uint[] joints = null;
                        uint? rootJoint = null;

                        if( mesh.HasVertexAttribute(UnityEngine.Rendering.VertexAttribute.BlendIndices) ) {
                            if(node.skin>=0) {
                                var skin = gltf.skins[node.skin];
                                // TODO: see if this can be moved to mesh creation phase / before instantiation
                                mesh.bindposes = skinsInverseBindMatrices[node.skin];
                                if (skin.skeleton >= 0) {
                                    rootJoint = (uint) skin.skeleton;
                                }
                                joints = skin.joints;
                            } else {
                                logger?.Warning(LogCode.SkinMissing);
                            }
                        }
                        
                        var meshInstancing = node.extensions?.EXT_mesh_gpu_instancing;

                        var primitiveName =
                            primitiveCount > 0
                                ? $"{meshName ?? PrimitiveName}_{primitiveCount}"
                                : meshName ?? PrimitiveName;

                        if (meshInstancing == null) {
                            instantiator.AddPrimitive(
                                nodeIndex,
                                primitiveName,
                                mesh,
                                primitive.materialIndices,
                                joints,
                                rootJoint,
                                gltf.meshes[node.mesh].weights,
                                primitiveCount
                            );
                        } else {

                            var hasTranslations = meshInstancing.attributes.TRANSLATION > -1;
                            var hasRotations = meshInstancing.attributes.ROTATION > -1;
                            var hasScales = meshInstancing.attributes.SCALE > -1;

                            NativeArray<Vector3>? positions = null;
                            NativeArray<Quaternion>? rotations = null;
                            NativeArray<Vector3>? scales = null;
                            uint instanceCount = 0;
                            
                            if (hasTranslations) {
                                positions = ((AccessorNativeData<Vector3>) accessorData[meshInstancing.attributes.TRANSLATION]).data;
                                instanceCount = (uint)positions.Value.Length;
                            }

                            if (hasRotations) {
                                rotations = ((AccessorNativeData<Quaternion>) accessorData[meshInstancing.attributes.ROTATION]).data;
                                instanceCount = (uint)rotations.Value.Length;
                            }

                            if (hasScales) {
                                scales = ((AccessorNativeData<Vector3>) accessorData[meshInstancing.attributes.SCALE]).data;
                                instanceCount = (uint)scales.Value.Length;
                            }

                            instantiator.AddPrimitiveInstanced(
                                nodeIndex,
                                primitiveName,
                                mesh,
                                primitives[i].materialIndices,
                                instanceCount,
                                positions,
                                rotations,
                                scales,
                                primitiveCount
                            );
                        }
                        
                        primitiveCount++;
                    }
                }
                
                instantiator.SetNodeName(nodeIndex,goName);

                if (node.camera >= 0
                    && gltf.cameras!=null
                    && node.camera < gltf.cameras.Length
                    )
                {
                    instantiator.AddCamera(nodeIndex,(uint)node.camera);
                }
            }
            
            Profiler.BeginSample("CreateGameObjects");

            var scene = gltfRoot.scenes[sceneId];
            instantiator.Init();

            if (scene.nodes != null) {
                foreach (var nodeId in scene.nodes) {
                    IterateNodes(nodeId,null,CreateHierarchy);
                }
            
            
                foreach (var nodeId in scene.nodes) {
                    IterateNodes(nodeId,null,PopulateHierarchy);
                }
            }

#if UNITY_ANIMATION
            instantiator.AddScene(scene.name,scene.nodes,animationClips);
#else
            instantiator.AddScene(scene.name,scene.nodes);
#endif

            Profiler.EndSample();
        }

        /// <summary>
        /// Reinterprets a NativeSlice<byte> to another type of NativeArray.
        /// TODO: Remove once Unity.Collections supports this for NativeSlice (NativeArray only atm)
        /// </summary>
        /// <param name="slice"></param>
        /// <param name="count">Target type element count</param>
        /// <typeparam name="T">Target type</typeparam>
        /// <returns></returns>
        static unsafe NativeArray<T> Reinterpret<T>(NativeSlice<byte> slice, int count, int offset = 0 ) where T : struct {
            var address = (byte*) slice.GetUnsafeReadOnlyPtr();
            var result = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(address+offset, count, Allocator.None);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            var safetyHandle = AtomicSafetyHandle.Create();
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(array: ref result, safetyHandle);
#endif
            return result;
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        static void ReleaseReinterpret<T>(NativeArray<T> array) where T : struct {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            var safetyHandle = NativeArrayUnsafeUtility.GetAtomicSafetyHandle(array);
            AtomicSafetyHandle.Release(safetyHandle);
#endif
        }

#if KTX_UNITY
        async Task
#else
        void
#endif
        CreateTexturesFromBuffers( Schema.Image[] src_images, Schema.BufferView[] bufferViews, List<ImageCreateContext> contexts ) {
            for (int i = 0; i < images.Length; i++) {
                Profiler.BeginSample("CreateTexturesFromBuffers.ImageFormat");
                if(images[i]!=null) {
                    resources.Add(images[i]);
                }
                var img = src_images[i];
                ImageFormat imgFormat = imageFormats[i];
                if(imgFormat==ImageFormat.Unknown) {
                    if(string.IsNullOrEmpty(img.mimeType)) {
                        // Image is missing mime type
                        // try to determine type by file extension
                        imgFormat = UriHelper.GetImageFormatFromUri(img.uri);
                    } else {
                        imgFormat = GetImageFormatFromMimeType(img.mimeType);
                    }
                }
                Profiler.EndSample();

                if (imgFormat!=ImageFormat.Unknown) {
                    if (img.bufferView >= 0) {
                        
                        if(imgFormat == ImageFormat.KTX) {
#if KTX_UNITY
                            Profiler.BeginSample("CreateTexturesFromBuffers.KtxLoadNativeContext");
                            if(ktxLoadContextsBuffer==null) {
                                ktxLoadContextsBuffer = new List<KtxLoadContextBase>();
                            }
                            var ktxContext = new KtxLoadNativeContext(i,GetBufferView(img.bufferView));
                            ktxLoadContextsBuffer.Add(ktxContext);
                            Profiler.EndSample();
                            await deferAgent.BreakPoint();
#else
                            logger?.Error(LogCode.PackageMissing,"KtxUnity",Extensions.TextureBasisUniversal);
#endif // KTX_UNITY
                        } else {
                            Profiler.BeginSample("CreateTexturesFromBuffers.ExtractBuffer");
                            var bufferView = bufferViews[img.bufferView];
                            var buffer = GetBuffer(bufferView.buffer);
                            var chunk = binChunks[bufferView.buffer];

                            bool forceSampleLinear = imageGamma!=null && !imageGamma[i];
                            var txt = CreateEmptyTexture(img,i,forceSampleLinear);
                            var icc = new ImageCreateContext();
                            icc.imageIndex = i;
                            icc.buffer = new byte[bufferView.byteLength];
                            icc.gcHandle = GCHandle.Alloc(icc.buffer,GCHandleType.Pinned);

                            var job = CreateMemCopyJob(bufferView, buffer, chunk, icc);
                            icc.jobHandle = job.Schedule();

                            contexts.Add(icc);
                            
                            images[i] = txt;
                            resources.Add(txt);
                            Profiler.EndSample();
                        }
                    }
                }
            }
        }

        static unsafe MemCopyJob CreateMemCopyJob(BufferView bufferView, byte[] buffer, GlbBinChunk chunk, ImageCreateContext icc) {
            var job = new Jobs.MemCopyJob();
            job.bufferSize = bufferView.byteLength;
            fixed (void* src = &(buffer[bufferView.byteOffset + chunk.start]), dst = &(icc.buffer[0])) {
                job.input = src;
                job.result = dst;
            }

            return job;
        }

        Texture2D CreateEmptyTexture(Schema.Image img, int index, bool forceSampleLinear) {
            Texture2D txt;
            if(forceSampleLinear) {
                TextureCreationFlags mipmapFlags = settings.generateMipMaps ? TextureCreationFlags.MipChain : TextureCreationFlags.None;
                txt = new Texture2D(4, 4, GraphicsFormat.R8G8B8A8_UNorm, mipmapFlags);
            } else {
                txt = new Texture2D(4, 4, UnityEngine.TextureFormat.RGBA32, mipChain: settings.generateMipMaps);
            }
            txt.anisoLevel = settings.anisotropicFilterLevel;
            txt.name = GetImageName(img, index);
            return txt;
        }

        static string GetImageName(Image img, int index) {
            return string.IsNullOrEmpty(img.name) ? string.Format("image_{0}",index) : img.name;
        }

        private void SafeDestroy(UnityEngine.Object obj) {
            if (Application.isPlaying) {
                UnityEngine.Object.Destroy(obj);
            }
            else {
                UnityEngine.Object.DestroyImmediate(obj);
            }
        }
        
        async Task<bool> LoadAccessorData( Root gltf ) {

            Profiler.BeginSample("LoadAccessorData.Init");

            var mainBufferTypes = new Dictionary<MeshPrimitive,MainBufferType>();
            var meshCount = gltf.meshes == null ? 0 : gltf.meshes.Length;
            meshPrimitiveCluster = gltf.meshes==null ? null : new Dictionary<MeshPrimitive,List<MeshPrimitive>>[meshCount];
            Dictionary<MeshPrimitive, MorphTargetsContext> morphTargetsContexts = null;
#if DEBUG
            var perAttributeMeshCollection = new Dictionary<Attributes,HashSet<int>>();
#endif
            
            /// Iterate all primitive vertex attributes and remember the accessors usage.
            accessorUsage = new AccessorUsage[gltf.accessors.Length];
            int totalPrimitives = 0;
            for (int meshIndex = 0; meshIndex < meshCount; meshIndex++)
            {
                var mesh = gltf.meshes[meshIndex];
                meshPrimitiveIndex[meshIndex] = totalPrimitives;
                var cluster = new Dictionary<MeshPrimitive, List<MeshPrimitive>>();
                
                foreach(var primitive in mesh.primitives) {
                    
                    if(!cluster.ContainsKey(primitive)) {
                        cluster[primitive] = new List<MeshPrimitive>();
                    }
                    cluster[primitive].Add(primitive);
                    
                    if (primitive.targets != null) {
                        if (morphTargetsContexts == null) {
                            morphTargetsContexts = new Dictionary<MeshPrimitive, MorphTargetsContext>();
                        } else if (morphTargetsContexts.ContainsKey(primitive)) {
                            continue;
                        }
                            
                        var morphTargetsContext = CreateMorphTargetsContext(primitive,mesh.extras?.targetNames);
                        morphTargetsContexts[primitive] = morphTargetsContext;
                    }
#if DRACO_UNITY
                    var isDraco = primitive.isDracoCompressed;
                    if (isDraco) {
                        continue;
                    }
#else
                    var isDraco = false;
#endif
                    var att = primitive.attributes;
                    if(primitive.indices>=0) {
                        var usage = (
                            primitive.mode == DrawMode.Triangles
                            || primitive.mode == DrawMode.TriangleStrip
                            || primitive.mode == DrawMode.TriangleFan
                            )
                        ? AccessorUsage.IndexFlipped
                        : AccessorUsage.Index;
                        SetAccessorUsage(primitive.indices, isDraco ? AccessorUsage.Ignore : usage );
                    }

                    if(!mainBufferTypes.TryGetValue(primitive,out var mainBufferType)) {
                        if(att.TANGENT>=0) {
                            mainBufferType = MainBufferType.PosNormTan;
                        } else
                        if(att.NORMAL>=0) {
                            mainBufferType = MainBufferType.PosNorm;
                        } else {
                            mainBufferType = MainBufferType.Position;
                        }
                    }
                    if (primitive.mode == DrawMode.Triangles || primitive.mode == DrawMode.TriangleFan ||
                        primitive.mode == DrawMode.TriangleStrip)
                    {
                        if (primitive.material < 0 || gltf.materials[primitive.material].requiresNormals) {
                            mainBufferType |= MainBufferType.Normal;
                        }
                        if (primitive.material >= 0 && gltf.materials[primitive.material].requiresTangents) {
                            mainBufferType |= MainBufferType.Tangent;
                        }
                    }
                    mainBufferTypes[primitive] = mainBufferType;
                    
#if DEBUG
                    if(!perAttributeMeshCollection.TryGetValue(att, out var attributeMesh)) {
                        attributeMesh = new HashSet<int>();
                        perAttributeMeshCollection[att] = attributeMesh;
                    }
                    attributeMesh.Add(meshIndex);
#endif
                }
                meshPrimitiveCluster[meshIndex] = cluster;
                totalPrimitives += cluster.Count;
            }

            if(gltf.skins!=null) {
                skinsInverseBindMatrices = new Matrix4x4[gltf.skins.Length][];
                foreach(var skin in gltf.skins) {
                    SetAccessorUsage(skin.inverseBindMatrices,AccessorUsage.InverseBindMatrix);
                }
            }

            foreach (var node in gltf.nodes) {
                var attr = node.extensions?.EXT_mesh_gpu_instancing?.attributes;
                if ( attr != null) {
                    if (attr.TRANSLATION >= 0) {
                        SetAccessorUsage(attr.TRANSLATION,AccessorUsage.Translation | AccessorUsage.RequiredForInstantiation);
                    }
                    if (attr.ROTATION >= 0) {
                        SetAccessorUsage(attr.ROTATION,AccessorUsage.Rotation | AccessorUsage.RequiredForInstantiation);
                    }
                    if (attr.SCALE >= 0) {
                        SetAccessorUsage(attr.SCALE,AccessorUsage.Scale | AccessorUsage.RequiredForInstantiation);
                    }
                }
            }

            if (meshPrimitiveIndex != null) {
                meshPrimitiveIndex[meshCount] = totalPrimitives;
            }
            primitives = new Primitive[totalPrimitives];
            primitiveContexts = new PrimitiveCreateContextBase[totalPrimitives];
            var tmpList = new List<JobHandle>(mainBufferTypes.Count);
            vertexAttributes = new Dictionary<MeshPrimitive,VertexBufferConfigBase>(mainBufferTypes.Count);
#if DEBUG
            foreach (var perAttributeMeshes in perAttributeMeshCollection) {
                if(perAttributeMeshes.Value.Count>1) {
                    logger?.Warning(LogCode.AccessorsShared);
                    break;
                }
            }
#endif
            Profiler.EndSample();

            var success = true;

            foreach(var mainBufferType in mainBufferTypes) {

                Profiler.BeginSample("LoadAccessorData.ScheduleVertexJob");

                var primitive = mainBufferType.Key;
                var att = primitive.attributes;
                
                bool hasNormals = att.NORMAL >= 0;
                bool hasTangents = att.TANGENT >= 0;

                int[] uvInputs = null;
                if (att.TEXCOORD_0 >= 0) {
                    int uvCount = 1;
                    if (att.TEXCOORD_1 >= 0) uvCount++;
                    uvInputs = new int[uvCount];
                    uvInputs[0] = att.TEXCOORD_0;
                    if (att.TEXCOORD_1 >= 0) {
                        uvInputs[1] = att.TEXCOORD_1;
                    }
                    if (att.TEXCOORD_2 >= 0) {
                        logger?.Warning(LogCode.UVLimit);
                    }
                }

                VertexBufferConfigBase config;
                switch (mainBufferType.Value) {
                    case MainBufferType.Position:
                        config = new VertexBufferConfig<Vertex.VPos>(logger);
                        break;
                    case MainBufferType.PosNorm:
                        config = new VertexBufferConfig<Vertex.VPosNorm>(logger);
                        break;
                    case MainBufferType.PosNormTan:
                        config = new VertexBufferConfig<Vertex.VPosNormTan>(logger);
                        break;
                    default:
                        logger?.Error(LogCode.BufferMainInvalidType,mainBufferType.ToString());
                        return false;
                }
                config.calculateNormals = !hasNormals && (mainBufferType.Value & MainBufferType.Normal) > 0;
                config.calculateTangents = !hasTangents && (mainBufferType.Value & MainBufferType.Tangent) > 0;
                vertexAttributes[primitive] = config;
                
                var jh = config.ScheduleVertexJobs(
                    this,
                    att.POSITION,
                    att.NORMAL,
                    att.TANGENT,
                    uvInputs,
                    att.COLOR_0,
                    att.WEIGHTS_0,
                    att.JOINTS_0
                );

                if (jh.HasValue) {
                    tmpList.Add(jh.Value);
                } else {
                    success = false;
                    break;
                }

                Profiler.EndSample();

                await deferAgent.BreakPoint();
            }

            if (!success) {
                return false;
            }

            if (morphTargetsContexts != null) {
                foreach (var morphTargetsContext in morphTargetsContexts) {
                    var jobHandle = morphTargetsContext.Value.GetJobHandle();
                    tmpList.Add(jobHandle);
                }
            }

#if UNITY_ANIMATION
            if (gltf.hasAnimation) {
                for (int i = 0; i < gltf.animations.Length; i++) {
                    var animation = gltf.animations[i];
                    foreach (var sampler in animation.samplers) {
                        SetAccessorUsage(sampler.input,AccessorUsage.AnimationTimes);
                    }

                    foreach (var channel in animation.channels) {
                        var accessorIndex = animation.samplers[channel.sampler].output;
                        switch (channel.target.pathEnum) {
                            case AnimationChannel.Path.translation:
                                SetAccessorUsage(accessorIndex,AccessorUsage.Translation);
                                break;
                            case AnimationChannel.Path.rotation:
                                SetAccessorUsage(accessorIndex,AccessorUsage.Rotation);
                                break;
                            case AnimationChannel.Path.scale:
                                SetAccessorUsage(accessorIndex,AccessorUsage.Scale);
                                break;
                            case AnimationChannel.Path.weights:
                                SetAccessorUsage(accessorIndex,AccessorUsage.Weight);
                                break;
                        }
                    }
                }
            }
#endif

            /// Retrieve indices data jobified
            accessorData = new AccessorDataBase[gltf.accessors.Length];

            for(int i=0; i<accessorData.Length; i++) {
                Profiler.BeginSample("LoadAccessorData.IndicesMatrixJob");
                var acc = gltf.accessors[i];
                if(acc.bufferView<0) {
                    // Not actual accessor to data
                    // Common for draco meshes
                    // the accessor only holds meta information
                    continue;
                }
                switch (acc.typeEnum) {
                    case GLTFAccessorAttributeType.SCALAR when accessorUsage[i]==AccessorUsage.IndexFlipped ||
                        accessorUsage[i]==AccessorUsage.Index:
                    {
                        var ads = new  AccessorData<int>();
                        GetIndicesJob(gltf,i,out ads.data, out var jh, out ads.gcHandle, accessorUsage[i]==AccessorUsage.IndexFlipped);
                        tmpList.Add(jh.Value);
                        accessorData[i] = ads;
                        break;
                    }
                    case GLTFAccessorAttributeType.MAT4 when accessorUsage[i]==AccessorUsage.InverseBindMatrix: {
                        // TODO: Maybe use AccessorData, since Mesh.bindposes only accepts C# arrays.
                        var ads = new  AccessorNativeData<Matrix4x4>();
                        GetMatricesJob(gltf,i,out ads.data, out var jh);
                        tmpList.Add(jh.Value);
                        accessorData[i] = ads;
                        break;
                    }
                    case GLTFAccessorAttributeType.VEC3 when (accessorUsage[i]&AccessorUsage.Translation)!=0:
                    {
                        var ads = new AccessorNativeData<Vector3>();
                        GetVector3Job(gltf,i,out ads.data, out var jh, true);
                        tmpList.Add(jh.Value);
                        accessorData[i] = ads;
                        break;
                    }
                    case GLTFAccessorAttributeType.VEC4 when (accessorUsage[i]&AccessorUsage.Rotation)!=0:
                    {
                        var ads = new AccessorNativeData<Quaternion>();
                        GetVector4Job(gltf,i,out ads.data, out var jh);
                        tmpList.Add(jh.Value);
                        accessorData[i] = ads;
                        break;
                    }
                    case GLTFAccessorAttributeType.VEC3 when (accessorUsage[i]&AccessorUsage.Scale)!=0:
                    {
                        var ads = new AccessorNativeData<Vector3>();
                        GetVector3Job(gltf,i,out ads.data, out var jh, false);
                        tmpList.Add(jh.Value);
                        accessorData[i] = ads;
                        break;
                    }
#if UNITY_ANIMATION
                    case GLTFAccessorAttributeType.SCALAR when accessorUsage[i]==AccessorUsage.AnimationTimes || accessorUsage[i]==AccessorUsage.Weight:
                    {
                        // JobHandle? jh;
                        var ads = new  AccessorNativeData<float>();
                        GetScalarJob(gltf, i, out var times, out var jh);
                        if (times.HasValue) {
                            ads.data = times.Value;
                        }
                        if (jh.HasValue) {
                            tmpList.Add(jh.Value);
                        }
                        accessorData[i] = ads;
                        break;
                    }
#endif
                }
                Profiler.EndSample();
                await deferAgent.BreakPoint();
            }

            Profiler.BeginSample("LoadAccessorData.PrimitiveCreateContexts");
            int primitiveIndex=0;
            for( int meshIndex = 0; meshIndex<meshCount; meshIndex++ ) {
                var mesh = gltf.meshes[meshIndex];
                foreach( var cluster in meshPrimitiveCluster[meshIndex].Values) {

                    PrimitiveCreateContextBase context = null;

                    for (int primIndex = 0; primIndex < cluster.Count; primIndex++) {
                        var primitive = cluster[primIndex];
#if DRACO_UNITY
                        if (primitive.isDracoCompressed) {
                            Bounds? bounds = null;
                            var posAccessorIndex = primitive?.attributes.POSITION ?? -1;
                            if (posAccessorIndex >= 0 && posAccessorIndex < gltf.accessors.Length) {
                                var posAccessor = gltf.accessors[posAccessorIndex];
                                bounds = posAccessor.TryGetBounds();
                            }

                            if (!bounds.HasValue) {
                                logger.Error(LogCode.MeshBoundsMissing, meshIndex.ToString());
                            }
                            context = new PrimitiveDracoCreateContext(bounds);
                            context.materials = new int[1];
                        }
                        else
#endif
                        {
                            PrimitiveCreateContext c;
                            if(context==null) {
                                c = new PrimitiveCreateContext();
                                c.indices = new int[cluster.Count][];
                                c.materials = new int[cluster.Count];
                            } else {
                                c = (context as PrimitiveCreateContext);
                            }
                            // PreparePrimitiveIndices(gltf,mesh,primitive,ref c,primIndex);
                            context = c;
                        }

                        if (primitive.targets != null) {
                            context.morphTargetsContext = morphTargetsContexts[primitive];
                        }
                        
                        context.primtiveIndex = primitiveIndex;
                        context.materials[primIndex] = primitive.material;

                        context.needsNormals |= primitive.material<0 || gltf.materials[primitive.material].requiresNormals;
                        context.needsTangents |= primitive.material>=0 && gltf.materials[primitive.material].requiresTangents;
                    }

                    primitiveContexts[primitiveIndex] = context;
                    primitiveIndex++;
                }
            }
            Profiler.EndSample();
            
            Profiler.BeginSample("LoadAccessorData.Schedule");
            NativeArray<JobHandle> jobHandles = new NativeArray<JobHandle>(tmpList.ToArray(), Allocator.Persistent);
            accessorJobsHandle = JobHandle.CombineDependencies(jobHandles);
            jobHandles.Dispose();
            JobHandle.ScheduleBatchedJobs();

            Profiler.EndSample();
            return success;
        }

        MorphTargetsContext CreateMorphTargetsContext(MeshPrimitive primitive,string[] meshTargetNames) {
            var morphTargetsContext = new MorphTargetsContext(primitive.targets.Length,meshTargetNames);
            foreach (var morphTarget in primitive.targets) {
                var success = morphTargetsContext.AddMorphTarget(
                    this,
                    morphTarget.POSITION,
                    morphTarget.NORMAL,
                    morphTarget.TANGENT,
                    logger
                );
                if (!success) {
                    logger.Error(LogCode.MorphTargetContextFail);
                    break;
                }
            }

            return morphTargetsContext;
        }

        void SetAccessorUsage(int index, AccessorUsage newUsage) {
#if DEBUG
            if(accessorUsage[index]!=AccessorUsage.Unknown && newUsage!=accessorUsage[index]) {
                logger?.Error(LogCode.AccessorInconsistentUsage, accessorUsage[index].ToString(), newUsage.ToString());
            }
#endif
            accessorUsage[index] = newUsage;
        }

        async Task CreatePrimitiveContexts( Root gltf ) {
            int i=0;
            bool schedule = false;
            for( int meshIndex = 0; meshIndex<gltf.meshes.Length; meshIndex++ ) {
                var mesh = gltf.meshes[meshIndex];
                foreach( var kvp in meshPrimitiveCluster[meshIndex]) {
                    var cluster = kvp.Value;
                    Profiler.BeginSample( "CreatePrimitiveContext");
                    PrimitiveCreateContextBase context = primitiveContexts[i];

                    for (int primIndex = 0; primIndex < cluster.Count; primIndex++) {
                        var primitive = cluster[primIndex];
#if DRACO_UNITY
                        if( primitive.isDracoCompressed ) {
                            var c = (PrimitiveDracoCreateContext) context;
                            PreparePrimitiveDraco(gltf,mesh,primitive,ref c);
                            schedule = true;
                        } else
#endif
                        {
                            PrimitiveCreateContext c = (PrimitiveCreateContext) context;
                            c.vertexData = vertexAttributes[kvp.Key];
                            PreparePrimitiveIndices(gltf,mesh,primitive,ref c,primIndex);
                        }
                    }   
                    Profiler.EndSample();
                    await deferAgent.BreakPoint();
                    i++;
                }
            }
            // TODO: not necessary with ECS
            // https://docs.unity3d.com/Manual/JobSystemTroubleshooting.html
            if (schedule) {
                JobHandle.ScheduleBatchedJobs();
            }
        }

        async Task AssignAllAccessorData( Root gltf ) {
            if (gltf.meshes != null) {
                Profiler.BeginSample("AssignAllAccessorData.Primitive");
                int i=0;
                for( int meshIndex = 0; meshIndex<gltf.meshes.Length; meshIndex++ ) {
                    var mesh = gltf.meshes[meshIndex];

                    foreach( var cluster in meshPrimitiveCluster[meshIndex]) {
    #if DRACO_UNITY
                        if( !cluster.Value[0].isDracoCompressed )
    #endif
                        {
                            // Create one PrimitiveCreateContext per Primitive cluster
                            PrimitiveCreateContext c = (PrimitiveCreateContext) primitiveContexts[i];
                            c.mesh = mesh;
                        }
                        i++;
                    }
                }
                Profiler.EndSample();
            }
            
            if(gltf.skins!=null) {
                for (int s = 0; s < gltf.skins.Length; s++)
                {
                    Profiler.BeginSample("AssignAllAccessorData.Skin");
                    var skin = gltf.skins[s];
                    skinsInverseBindMatrices[s] = (accessorData[skin.inverseBindMatrices] as AccessorNativeData<Matrix4x4>).data.ToArray();
                    Profiler.EndSample();
                    await deferAgent.BreakPoint();
                }
            }
        }

        void PreparePrimitiveIndices( Root gltf, Mesh mesh, MeshPrimitive primitive, ref PrimitiveCreateContext c, int submeshIndex = 0 ) {
            Profiler.BeginSample("PreparePrimitiveIndices");
            switch(primitive.mode) {
            case DrawMode.Triangles:
                c.topology = MeshTopology.Triangles;
                break;
            case DrawMode.Points:
                c.topology = MeshTopology.Points;
                break;
            case DrawMode.Lines:
                logger?.Error(LogCode.PrimitiveModeUnsupported,primitive.mode.ToString());
                c.topology = MeshTopology.Lines;
                break;
            case DrawMode.LineLoop:
                logger?.Error(LogCode.PrimitiveModeUnsupported,primitive.mode.ToString());
                c.topology = MeshTopology.LineStrip;
                break;
            case DrawMode.LineStrip:
                c.topology = MeshTopology.LineStrip;
                break;
            case DrawMode.TriangleStrip:
            case DrawMode.TriangleFan:
            default:
                logger?.Error(LogCode.PrimitiveModeUnsupported,primitive.mode.ToString());
                c.topology = MeshTopology.Triangles;
                break;
            }

            if(primitive.indices >= 0) {
                c.indices[submeshIndex] = (accessorData[primitive.indices] as AccessorData<int>).data;
            } else {
                int vertexCount = gltf.accessors[primitive.attributes.POSITION].count;
                JobHandle? jh;
                CalculateIndicesJob(gltf,primitive, vertexCount, c.topology, out c.indices[submeshIndex], out jh, out c.calculatedIndicesHandle );
                c.jobHandle = jh.Value;
            }
            Profiler.EndSample();
        }
        
#if DRACO_UNITY
        void PreparePrimitiveDraco( Root gltf, Mesh mesh, MeshPrimitive primitive, ref PrimitiveDracoCreateContext c ) {
            var draco_ext = primitive.extensions.KHR_draco_mesh_compression;
            
            var bufferView = gltf.bufferViews[draco_ext.bufferView];
            var buffer = GetBufferViewSlice(bufferView);

            c.StartDecode(buffer, draco_ext.attributes.WEIGHTS_0, draco_ext.attributes.JOINTS_0);
        }
#endif

        unsafe void CalculateIndicesJob(Root gltf, MeshPrimitive primitive, int vertexCount, MeshTopology topology, out int[] indices, out JobHandle? jobHandle, out GCHandle resultHandle ) {
            Profiler.BeginSample("CalculateIndicesJob");
            // No indices: calculate them
            bool lineLoop = primitive.mode == DrawMode.LineLoop;
            // extra index (first vertex again) for closing line loop
            indices = new int[vertexCount+(lineLoop?1:0)];
            resultHandle = GCHandle.Alloc(indices, GCHandleType.Pinned);
            if(topology == MeshTopology.Triangles) {
                var job8 = new Jobs.CreateIndicesInt32FlippedJob();
                fixed( void* dst = &(indices[0]) ) {
                    job8.result = (int*)dst;
                }
                jobHandle = job8.Schedule(indices.Length,DefaultBatchCount);
            } else {
                var job8 = new Jobs.CreateIndicesInt32Job();
                if(lineLoop) {
                    // Set the last index to the first vertex
                    indices[vertexCount] = 0;
                }
                fixed( void* dst = &(indices[0]) ) {
                    job8.result = (int*)dst;
                }
                jobHandle = job8.Schedule(vertexCount,DefaultBatchCount);
            }
            Profiler.EndSample();
        }

        unsafe void GetIndicesJob(Root gltf, int accessorIndex, out int[] indices, out JobHandle? jobHandle, out GCHandle resultHandle, bool flip) {
            Profiler.BeginSample("PrepareGetIndicesJob");
            // index
            var accessor = gltf.accessors[accessorIndex];
            var bufferView = GetBufferView(accessor.bufferView,accessor.byteOffset);

            Profiler.BeginSample("Alloc");
            indices = new int[accessor.count];
            Profiler.EndSample();
            Profiler.BeginSample("Pin");
            resultHandle = GCHandle.Alloc(indices, GCHandleType.Pinned);
            Profiler.EndSample();

            Assert.AreEqual(accessor.typeEnum, GLTFAccessorAttributeType.SCALAR);
            //Assert.AreEqual(accessor.count * GetLength(accessor.typeEnum) * 4 , (int) chunk.length);
            if (accessor.isSparse) {
                logger.Error(LogCode.SparseAccessor,"indices");
            }

            Profiler.BeginSample("CreateJob");
            switch( accessor.componentType ) {
            case GLTFComponentType.UnsignedByte:
                if(flip) {
                    var job8 = new Jobs.ConvertIndicesUInt8ToInt32FlippedJob();
                    fixed( void* dst = &(indices[0]) ) {
                        job8.input = (byte*)bufferView.GetUnsafeReadOnlyPtr();
                        job8.result = (int3*)dst;
                    }
                    jobHandle = job8.Schedule(accessor.count/3,DefaultBatchCount);
                } else {
                    var job8 = new Jobs.ConvertIndicesUInt8ToInt32Job();
                    fixed( void* dst = &(indices[0]) ) {
                        job8.input = (byte*)bufferView.GetUnsafeReadOnlyPtr();
                        job8.result = (int*)dst;
                    }
                    jobHandle = job8.Schedule(accessor.count,DefaultBatchCount);
                }
                break;
            case GLTFComponentType.UnsignedShort:
                if(flip) {
                    var job16 = new Jobs.ConvertIndicesUInt16ToInt32FlippedJob();
                    fixed( void* dst = &(indices[0]) ) {
                        job16.input = (ushort*) bufferView.GetUnsafeReadOnlyPtr();
                        job16.result = (int3*) dst;
                    }
                    jobHandle = job16.Schedule(accessor.count/3,DefaultBatchCount);
                } else {
                    var job16 = new Jobs.ConvertIndicesUInt16ToInt32Job();
                    fixed( void* dst = &(indices[0]) ) {
                        job16.input = (ushort*) bufferView.GetUnsafeReadOnlyPtr();
                        job16.result = (int*) dst;
                    }
                    jobHandle = job16.Schedule(accessor.count,DefaultBatchCount);
                }
                break;
            case GLTFComponentType.UnsignedInt:
                if(flip) {
                    var job32 = new Jobs.ConvertIndicesUInt32ToInt32FlippedJob();
                    fixed( void* dst = &(indices[0]) ) {
                        job32.input = (uint*) bufferView.GetUnsafeReadOnlyPtr();
                        job32.result = (int3*) dst;
                    }
                    jobHandle = job32.Schedule(accessor.count/3,DefaultBatchCount);
                } else {
                    var job32 = new Jobs.ConvertIndicesUInt32ToInt32Job();
                    fixed( void* dst = &(indices[0]) ) {
                        job32.input = (uint*) bufferView.GetUnsafeReadOnlyPtr();
                        job32.result = (int*) dst;
                    }
                    jobHandle = job32.Schedule(accessor.count,DefaultBatchCount);
                }
                break;
            default:
                logger?.Error(LogCode.IndexFormatInvalid, accessor.componentType.ToString());
                jobHandle = null;
                break;
            }
            Profiler.EndSample();
            Profiler.EndSample();
        }

        unsafe void GetMatricesJob(Root gltf, int accessorIndex, out NativeArray<Matrix4x4> matrices, out JobHandle? jobHandle) {
            Profiler.BeginSample("GetMatricesJob");
            // index
            var accessor = gltf.accessors[accessorIndex];
            var bufferView = GetBufferView(accessor.bufferView,accessor.byteOffset);
            
            Profiler.BeginSample("Alloc");
            matrices = new NativeArray<Matrix4x4>(accessor.count,Allocator.Persistent);
            Profiler.EndSample();
            
            Assert.AreEqual(accessor.typeEnum, GLTFAccessorAttributeType.MAT4);
            //Assert.AreEqual(accessor.count * GetLength(accessor.typeEnum) * 4 , (int) chunk.length);
            if (accessor.isSparse) {
                logger.Error(LogCode.SparseAccessor,"Matrix");
            }

            Profiler.BeginSample("CreateJob");
            switch( accessor.componentType ) {
            case GLTFComponentType.Float:
                var job32 = new Jobs.ConvertMatricesJob {
                    input = (float4x4*)bufferView.GetUnsafeReadOnlyPtr(),
                    result = (float4x4*)matrices.GetUnsafePtr()
                };
                jobHandle = job32.Schedule(accessor.count,DefaultBatchCount);
                break;
            default:
                logger?.Error(LogCode.IndexFormatInvalid, accessor.componentType.ToString());
                jobHandle = null;
                break;
            }
            Profiler.EndSample();
            Profiler.EndSample();
        }

        unsafe void GetVector3Job(Root gltf, int accessorIndex, out NativeArray<Vector3> vectors, out JobHandle? jobHandle, bool flip) {
            Profiler.BeginSample("GetVector3Job");
            var accessor = gltf.accessors[accessorIndex];
            var bufferView = GetBufferView(accessor.bufferView,accessor.byteOffset);

            Profiler.BeginSample("Alloc");
            vectors = new NativeArray<Vector3>(accessor.count,Allocator.Persistent);
            Profiler.EndSample();
            
            Assert.AreEqual(accessor.typeEnum, GLTFAccessorAttributeType.VEC3);
            if (accessor.isSparse) {
                logger.Error(LogCode.SparseAccessor,"Vector3");
            }

            Profiler.BeginSample("CreateJob");
            switch( accessor.componentType ) {
            case GLTFComponentType.Float when flip: {
                var job = new ConvertVector3FloatToFloatJob {
                    input = (float3*)bufferView.GetUnsafeReadOnlyPtr(),
                    result = (float3*)vectors.GetUnsafePtr()
                };
                jobHandle = job.Schedule(accessor.count,DefaultBatchCount);
                break;
            }
            case GLTFComponentType.Float when !flip: {
                var job = new MemCopyJob {
                    input = (float*)bufferView.GetUnsafeReadOnlyPtr(),
                    bufferSize = accessor.count * 12,
                    result = (float*)vectors.GetUnsafePtr()
                };
                jobHandle = job.Schedule();
                break;
            }
            default:
                logger?.Error(LogCode.IndexFormatInvalid, accessor.componentType.ToString());
                jobHandle = null;
                break;
            }
            Profiler.EndSample();
            Profiler.EndSample();
        }
        
        unsafe void GetVector4Job(Root gltf, int accessorIndex, out NativeArray<Quaternion> vectors, out JobHandle? jobHandle) {
            Profiler.BeginSample("GetVector4Job");
            // index
            var accessor = gltf.accessors[accessorIndex];
            var bufferView = GetBufferView(accessor.bufferView,accessor.byteOffset);

            Profiler.BeginSample("Alloc");
            vectors = new NativeArray<Quaternion>(accessor.count,Allocator.Persistent);
            Profiler.EndSample();
            
            Assert.AreEqual(accessor.typeEnum, GLTFAccessorAttributeType.VEC4);
            if (accessor.isSparse) {
                logger.Error(LogCode.SparseAccessor,"Vector4");
            }

            Profiler.BeginSample("CreateJob");
            switch( accessor.componentType ) {
            case GLTFComponentType.Float: {
                var job = new ConvertRotationsFloatToFloatJob {
                    input = (float4*)bufferView.GetUnsafeReadOnlyPtr(),
                    result = (float4*)vectors.GetUnsafePtr()
                };
                jobHandle = job.Schedule(accessor.count,DefaultBatchCount);
                break;
            }
            case GLTFComponentType.Short: {
                var job = new ConvertRotationsInt16ToFloatJob {
                    input = (short*)bufferView.GetUnsafeReadOnlyPtr(),
                    result = (float*)vectors.GetUnsafePtr()
                };
                jobHandle = job.Schedule(accessor.count,DefaultBatchCount);
                break;
            }
            case GLTFComponentType.Byte: {
                var job = new ConvertRotationsInt8ToFloatJob {
                    input = (sbyte*)bufferView.GetUnsafeReadOnlyPtr(),
                    result = (float*)vectors.GetUnsafePtr()
                };
                jobHandle = job.Schedule(accessor.count,DefaultBatchCount);
                break;
            }
            default:
                logger?.Error(LogCode.IndexFormatInvalid, accessor.componentType.ToString());
                jobHandle = null;
                break;
            }
            Profiler.EndSample();
            Profiler.EndSample();
        }
        
#if UNITY_ANIMATION
        unsafe void GetScalarJob(Root gltf, int accessorIndex, out NativeArray<float>? scalars, out JobHandle? jobHandle) {
            Profiler.BeginSample("GetScalarJob");
            scalars = null;
            jobHandle = null;
            var accessor = gltf.accessors[accessorIndex];
            var buffer = GetBufferView(accessor.bufferView,accessor.byteOffset);

            Assert.AreEqual(accessor.typeEnum, GLTFAccessorAttributeType.SCALAR);
            if (accessor.isSparse) {
                logger.Error(LogCode.SparseAccessor,"scalars");
            }
            
            if (accessor.componentType == GLTFComponentType.Float) {
                Profiler.BeginSample("CopyAnimationTimes");
                // TODO: For long animations with lots of times, threading this just like everything else maybe makes sense.
                var bufferTimes = Reinterpret<float>(buffer, accessor.count);
                // Copy values
                scalars = new NativeArray<float>(bufferTimes, Allocator.Persistent);
                ReleaseReinterpret(bufferTimes);
                Profiler.EndSample();
            } else
            if( accessor.normalized ) {
                Profiler.BeginSample("Alloc");
                scalars = new NativeArray<float>(accessor.count,Allocator.Persistent);
                Profiler.EndSample();
                
                switch( accessor.componentType ) {
                    case GLTFComponentType.Byte: {
                        var job = new ConvertScalarInt8ToFloatNormalizedJob {
                            input = (sbyte*)buffer.GetUnsafeReadOnlyPtr(),
                            result = scalars.Value
                        };
                        jobHandle = job.Schedule(accessor.count,DefaultBatchCount);
                        break;
                    }
                    case GLTFComponentType.UnsignedByte: {
                        var job = new ConvertScalarUInt8ToFloatNormalizedJob {
                            input = (byte*)buffer.GetUnsafeReadOnlyPtr(),
                            result = scalars.Value
                        };
                        jobHandle = job.Schedule(accessor.count,DefaultBatchCount);
                        break;
                    }
                    case GLTFComponentType.Short: {
                        var job = new ConvertScalarInt16ToFloatNormalizedJob {
                            input = (short*) ((byte*)buffer.GetUnsafeReadOnlyPtr()),
                            result = scalars.Value
                        };
                        jobHandle = job.Schedule(accessor.count,DefaultBatchCount);
                        break;
                    }
                    case GLTFComponentType.UnsignedShort: {
                        var job = new ConvertScalarUInt16ToFloatNormalizedJob {
                            input = (ushort*) ((byte*)buffer.GetUnsafeReadOnlyPtr()),
                            result = scalars.Value
                        };
                        jobHandle = job.Schedule(accessor.count,DefaultBatchCount);
                        break;
                    }
                    default:
                        logger?.Error(LogCode.AnimationFormatInvalid, accessor.componentType.ToString());
                        break;
                }
            } else {
                // Non-normalized
                logger?.Error(LogCode.AnimationFormatInvalid, accessor.componentType.ToString());
            }
            Profiler.EndSample();
        }

#endif // UNITY_ANIMATION

#region IGltfBuffers
        public unsafe void GetAccessor(int index, out Accessor accessor, out void* data, out int byteStride) {
            accessor = gltfRoot.accessors[index];
            if (accessor.bufferView < 0 || accessor.bufferView >= gltfRoot.bufferViews.Length) {
                data = null;
                byteStride = 0;
                return;
            }
            var bufferView = gltfRoot.bufferViews[accessor.bufferView];
#if MESHOPT
            var meshopt = bufferView.extensions?.EXT_meshopt_compression;
            if (meshopt != null) {
                byteStride = meshopt.byteStride;
                data = (byte*)meshoptBufferViews[accessor.bufferView].GetUnsafeReadOnlyPtr() + accessor.byteOffset;
            } else
#endif
            {
                byteStride = bufferView.byteStride;
                var bufferIndex = bufferView.buffer;
                var buffer = GetBuffer(bufferIndex);
                fixed(void* src = &(buffer[accessor.byteOffset + bufferView.byteOffset + binChunks[bufferIndex].start])) {
                    data = src;
                }
            }
            
            // // Alternative that uses NativeArray/Slice
            // var bufferViewData = GetBufferView(bufferView);
            // data =  (byte*)bufferViewData.GetUnsafeReadOnlyPtr() + accessor.byteOffset;
        }
        
        public unsafe void GetAccessorSparseIndices(AccessorSparseIndices sparseIndices, out void* data) {
            var bufferView = gltfRoot.bufferViews[sparseIndices.bufferView];
#if MESHOPT
            var meshopt = bufferView.extensions?.EXT_meshopt_compression;
            if (meshopt != null) {
                data = (byte*)meshoptBufferViews[(int)sparseIndices.bufferView].GetUnsafeReadOnlyPtr() + sparseIndices.byteOffset;
            }
            else
#endif
            {
                var bufferIndex = bufferView.buffer;
                var buffer = GetBuffer(bufferIndex);
                fixed (void* src = &(buffer[sparseIndices.byteOffset + bufferView.byteOffset + binChunks[bufferIndex].start])) {
                    data = src;
                }
            }
        }

        public unsafe void GetAccessorSparseValues(AccessorSparseValues sparseValues, out void* data) {
            var bufferView = gltfRoot.bufferViews[sparseValues.bufferView];
#if MESHOPT
            var meshopt = bufferView.extensions?.EXT_meshopt_compression;
            if (meshopt != null) {
                data = (byte*)meshoptBufferViews[(int)sparseValues.bufferView].GetUnsafeReadOnlyPtr() + sparseValues.byteOffset;
            }
            else
#endif
            {
                var bufferIndex = bufferView.buffer;
                var buffer = GetBuffer(bufferIndex);
                fixed (void* src = &(buffer[sparseValues.byteOffset + bufferView.byteOffset + binChunks[bufferIndex].start])) {
                    data = src;
                }
            }
        }
#endregion IGltfBuffers

        /// <summary>
        /// Determines whether color accessor data can be retrieved as Color[] (floats) or Color32[] (unsigned bytes)
        /// </summary>
        /// <param name="gltf"></param>
        /// <param name="accessorIndex"></param>
        /// <returns>True if unsinged byte based colors are sufficient, false otherwise.</returns>
        bool IsColorAccessorByte( Accessor colorAccessor ) {
            return colorAccessor.componentType == GLTFComponentType.UnsignedByte;
        }

        bool IsKnownImageMimeType(string mimeType) {
            return GetImageFormatFromMimeType(mimeType) != ImageFormat.Unknown;
        }
        
        bool IsKnownImageFileExtension(string path) {
            return UriHelper.GetImageFormatFromUri(path) != ImageFormat.Unknown;
        }

        ImageFormat GetImageFormatFromMimeType(string mimeType) {
            if(!mimeType.StartsWith("image/")) return ImageFormat.Unknown;
            var sub = mimeType.Substring(6);
            switch(sub) {
                case "jpeg":
                    return ImageFormat.Jpeg;
                case "png":
                    return ImageFormat.PNG;
                case "ktx":
                case "ktx2":
                    return ImageFormat.KTX;
                default:
                    return ImageFormat.Unknown;
            }
        }
    }
}
