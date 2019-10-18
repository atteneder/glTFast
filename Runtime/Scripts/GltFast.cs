using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Networking;
using UnityEngine.Profiling;
using Unity.Jobs;
using Unity.Collections;
using System.Runtime.InteropServices;

namespace GLTFast {

    using Schema;

    public class GLTFast {

        const uint GLB_MAGIC = 0x46546c67;

        const string ErrorUnsupportedColorFormat = "Unsupported Color format {0}";

        public static readonly HashSet<string> supportedExtensions = new HashSet<string> {
            "KHR_draco_mesh_compression",
            "KHR_materials_pbrSpecularGlossiness",
            "KHR_materials_unlit"
        };

        enum ChunkFormat : uint
        {
            JSON = 0x4e4f534a,
            BIN = 0x004e4942
        }

        struct Primitive {
            public UnityEngine.Mesh mesh;
            public int materialIndex;

            public Primitive( UnityEngine.Mesh mesh, int materialIndex ) {
                this.mesh = mesh;
                this.materialIndex = materialIndex;
            }
        }
              
        struct ImageCreateContext {
            public int imageIndex;
            public byte[] buffer;
            public GCHandle gcHandle;
            public JobHandle jobHandle;
        }

        abstract class PrimitiveCreateContextBase {
            public int primtiveIndex;
            public MeshPrimitive primitive;
            public abstract bool IsCompleted {get;}
            public abstract Primitive? CreatePrimitive();
        }

        class PrimitiveCreateContext : PrimitiveCreateContextBase {

            public Mesh mesh;

#if GLTFAST_NO_JOB
            public int[] indices;
            public Vector3[] positions;
            public Vector3[] normals;
            public Vector2[] uvs0;
            public Vector2[] uvs1;
            public Vector4[] tangents;
            public Color32[] colors32;
            public Color[] colors;
#else
            /// TODO remove begin
            public Vector3[] positions;
            public Vector3[] normals;
            public Vector2[] uvs0;
            public Vector2[] uvs1;
            public Vector4[] tangents;
            public Color32[] colors32;
            public Color[] colors;
            /// TODO remove end

            public JobHandle jobHandle;
            public int[] indices;

            public GCHandle[] gcHandles;

            public override bool IsCompleted {
                get {
                    return jobHandle.IsCompleted;
                }  
            }

            public override Primitive? CreatePrimitive() {
                Profiler.BeginSample("CreatePrimitive");
#if !GLTFAST_NO_JOB
                jobHandle.Complete();
#endif
                var msh = new UnityEngine.Mesh();
                if( positions.Length > 65536 ) {
#if UNITY_2017_3_OR_NEWER
                    msh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
#else
                    throw new System.Exception("Meshes with more than 65536 vertices are only supported from Unity 2017.3 onwards.");
#endif
                }
                msh.name = mesh.name;
                msh.vertices = positions;

                msh.SetIndices(
                    indices
                    ,MeshTopology.Triangles
                    ,0
                    );

                if(uvs0!=null) {
                    msh.uv = uvs0;
                }
                if(uvs1!=null) {
                    msh.uv2 = uvs1;
                }
                if(normals!=null) {
                    msh.normals = normals;
                } else {
                    msh.RecalculateNormals();
                }
                if (colors!=null) {
                    msh.colors = colors;
                } else if(colors32!=null) {
                    msh.colors32 = colors32;
                }
                if(tangents!=null) {
                    msh.tangents = tangents;
                } else {
                    msh.RecalculateTangents();
                }
                // primitives[c.primtiveIndex] = new Primitive(msh,c.primitive.material);
                // resources.Add(msh);

#if !GLTFAST_NO_JOB
                Dispose();
#endif
                Profiler.EndSample();
                return new Primitive(msh,primitive.material);
            }

            void Dispose() {
                for(int i=0;i<gcHandles.Length;i++) {
                    gcHandles[i].Free();
                }
            }
#endif
        }

        class PrimitiveDracoCreateContext : PrimitiveCreateContextBase {
            public JobHandle jobHandle;
            public NativeArray<int> dracoResult;
            public NativeArray<IntPtr> dracoPtr;

            public override bool IsCompleted {
                get {
                    return jobHandle.IsCompleted;
                }  
            }

            public override Primitive? CreatePrimitive() {
                jobHandle.Complete();
                int result = dracoResult[0];
                IntPtr dracoMesh = dracoPtr[0];

                dracoResult.Dispose();
                dracoPtr.Dispose();

                if (result <= 0) {
                    Debug.LogError ("Failed: Decoding error.");
                    return null;
                }

                var msh = DracoMeshLoader.CreateMesh(dracoMesh);

                return new Primitive(msh,primitive.material);
            }
        }

#if GLTFAST_NO_JOB
        public delegate CompType[] ExtractAccessor<CompType>(ref byte[] bytes, int start, int count);
        public delegate CompType[] ExtractInterleavedAccessor<CompType>(ref byte[] bytes, int start, int count, int byteStride);
#endif

        byte[][] buffers;
        NativeArray<byte>[] nativeBuffers;

        GlbBinChunk[] binChunks;
        UnityEngine.Material[] materials;
        List<UnityEngine.Object> resources;

        PrimitiveCreateContextBase[] primitiveContexts;

        Primitive[] primitives;
        int[] meshPrimitiveIndex;

        IMaterialGenerator materialGenerator;

        /// TODO: Some of these class members maybe could be passed
        /// between loading routines. Turn them into parameters or at
        /// least dispose them once all ingredients are ready.

        /// Main glTF data structure
        Root gltfRoot;

        /// optional glTF-binary buffer
        /// https://github.com/KhronosGroup/glTF/tree/master/specification/2.0#binary-buffer
        GlbBinChunk? glbBinChunk;
        Texture2D[] images = null;
        List<ImageCreateContext> imageCreateContexts;

        bool loadingError = false;
        public bool LoadingError { get { return loadingError; } private set { this.loadingError = value; } }

        static string GetUriBase( string url ) {
            var uri = new Uri(url);
            return new Uri( uri, ".").AbsoluteUri;
        }

        public GLTFast() {
            materialGenerator = new DefaultMaterialGenerator();
        }

        void ParseJsonAndLoadBuffers( string json, string baseUri ) {
            gltfRoot = JsonUtility.FromJson<Root>(json);

            if(!CheckExtensionSupport(gltfRoot)) {
                loadingError = true;
                return;
            }

            for( int i=0; i<gltfRoot.buffers.Length;i++) {
                var buffer = gltfRoot.buffers[i];
                if( !string.IsNullOrEmpty(buffer.uri) ) {
                    if(buffer.uri.StartsWith("data:")) {
                        Debug.LogError("Embed buffer not supported");
                        loadingError = true;
                    } else {
                        LoadBuffer( i, baseUri+buffer.uri );
                    }
                }
            }

            var bufferCount = gltfRoot.buffers.Length;
            if(bufferCount>0) {
                buffers = new byte[bufferCount][];
                nativeBuffers = new NativeArray<byte>[bufferCount];
                binChunks = new GlbBinChunk[bufferCount];
            }
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
                        Debug.LogErrorFormat("Required glTF extension {0} is not supported!",ext);
                        return false;
                    }
                }
            }
            if(gltfRoot.extensionsUsed!=null) {
                foreach(var ext in gltfRoot.extensionsUsed) {
                    var supported = supportedExtensions.Contains(ext);
                    if(!supported) {
                        Debug.LogWarningFormat("glTF extension {0} is not supported!",ext);
                    }
                }
            }
            return true;
        }

        public void LoadGltf( string json, string url ) {
            var baseUri = GetUriBase(url);
            ParseJsonAndLoadBuffers(json,baseUri);
            if(!loadingError) {
                LoadImages(baseUri);
            }
        }

        void LoadImages( string baseUri ) {

            if (gltfRoot.images != null) {
                images = new Texture2D[gltfRoot.images.Length];
                for (int i = 0; i < images.Length; i++) {
                    var img = gltfRoot.images[i];
                    bool knownImageType = false;
                    if(string.IsNullOrEmpty(img.mimeType)) {
                        Debug.LogWarning("Image is missing mime type");
                        knownImageType = img.uri.EndsWith(".png",StringComparison.OrdinalIgnoreCase)
                            || img.uri.EndsWith(".jpg",StringComparison.OrdinalIgnoreCase)
                            || img.uri.EndsWith(".jpeg",StringComparison.OrdinalIgnoreCase);
                    } else {
                        knownImageType = img.mimeType == "image/jpeg" || img.mimeType == "image/png";
                    }

                    if (knownImageType)
                    {
                        if (img.bufferView >= 0)
                        {
                            // Inside buffer
                        } else
                        if(!string.IsNullOrEmpty(img.uri)) {
                            LoadTexture(i,baseUri+img.uri);
                        }
                    }
                }
            }
        }

        public IEnumerator WaitForBufferDownloads() {
            if(downloads!=null) {
                foreach( var dl in downloads ) {
                    yield return dl.Value;
                    var www = dl.Value.webRequest;
                    if(www.isNetworkError || www.isHttpError) {
                        Debug.LogError(www.error);
                    }
                    else {
                        buffers[dl.Key] = www.downloadHandler.data;
                    }
                }
            }

            for( int i=0; i<buffers.Length; i++ ) {
                if(i==0 && glbBinChunk.HasValue) {
                    // Already assigned in LoadGlb
                    continue;
                }
                var b = buffers[i];
                binChunks[i] = new GlbBinChunk(0,(uint) b.Length);
            }
        }

        public IEnumerator WaitForTextureDownloads() {
            if(textureDownloads!=null) {
                foreach( var dl in textureDownloads ) {
                    yield return dl.Value;
                    var www = dl.Value.webRequest;
                    if(www.isNetworkError || www.isHttpError) {
                        Debug.LogError(www.error);
                    }
                    else {
                        images[dl.Key] = ( www.downloadHandler as  DownloadHandlerTexture ).texture;
                    }
                }
            }
        }

        public bool InstanciateGltf( Transform parent ) {
            CreateGameObjects( gltfRoot, parent );
            return !loadingError;
        }

        Dictionary<int,UnityWebRequestAsyncOperation> downloads;
        Dictionary<int,UnityWebRequestAsyncOperation> textureDownloads;

        void LoadBuffer( int index, string url ) {
            UnityWebRequest www = UnityWebRequest.Get(url);

            if(downloads==null) {
                downloads = new Dictionary<int, UnityWebRequestAsyncOperation>();
            }

            downloads[index] = www.SendWebRequest();
        }

        void LoadTexture( int index, string url ) {
            var www = UnityWebRequestTexture.GetTexture(url);

            if(textureDownloads==null) {
                textureDownloads = new Dictionary<int, UnityWebRequestAsyncOperation>();
            }

            textureDownloads[index] = www.SendWebRequest();
        }

        public bool LoadGlb( byte[] bytes, string url ) {
            uint magic = BitConverter.ToUInt32( bytes, 0 );

            if (magic != GLB_MAGIC) {
                loadingError = true;
                return false;
            }
    

            uint version = BitConverter.ToUInt32( bytes, 4 );
            //uint length = BitConverter.ToUInt32( bytes, 8 );

            //Debug.Log( string.Format("version: {0:X}; length: {1}", version, length ) );

            if (version != 2) {
                loadingError = true;
                return false;
            }

            int index = 12; // first chung header

            var baseUri = GetUriBase(url);

            while( index < bytes.Length ) {
                uint chLength = BitConverter.ToUInt32( bytes, index );
                index += 4;
                uint chType = BitConverter.ToUInt32( bytes, index );
                index += 4;

                //Debug.Log( string.Format("chunk: {0:X}; length: {1}", chType, chLength) );

                if (chType == (uint)ChunkFormat.BIN) {
                    //Debug.Log( string.Format("chunk: BIN; length: {0}", chLength) );
                    Assert.IsFalse(glbBinChunk.HasValue); // There can only be one binary chunk
                    glbBinChunk = new GlbBinChunk( index, chLength);
                }
                else if (chType == (uint)ChunkFormat.JSON) {
                    Assert.IsNull(gltfRoot);
                    string json = System.Text.Encoding.UTF8.GetString(bytes, index, (int)chLength );
                    //Debug.Log( string.Format("chunk: JSON; length: {0}", json ) );
                    ParseJsonAndLoadBuffers(json,baseUri);
                    if(loadingError) {
                        return false;
                    }
                }
 
                index += (int) chLength;
            }

            //Debug.Log(index);
            if(gltfRoot!=null) {
                //Debug.Log(gltf);
                if(glbBinChunk.HasValue) {
                    binChunks[0] = glbBinChunk.Value;
                    buffers[0] = bytes;
                }
                LoadImages(baseUri);
                return !loadingError;
            } else {
                Debug.LogError("Invalid JSON chunk");
                loadingError = true;
            }
            return false;
        }

        byte[] GetBuffer(int index) {
            return buffers[index];
        }

        NativeArray<byte> GetNativeBuffer(int index) {
            if(!nativeBuffers[index].IsCreated) {
                nativeBuffers[index] = new NativeArray<byte>(GetBuffer(index),Allocator.Persistent);
            }
            return nativeBuffers[index];
        }

        public IEnumerator Prepare() {
            meshPrimitiveIndex = new int[gltfRoot.meshes.Length+1];

            resources = new List<UnityEngine.Object>();

            Profiler.BeginSample("CreateTexturesFromBuffers");
            if(gltfRoot.images!=null) {
                if(images==null) {
                    images = new Texture2D[gltfRoot.images.Length];
                } else {
                    Assert.AreEqual(images.Length,gltfRoot.images.Length);
                }
                imageCreateContexts = new List<ImageCreateContext>();
                CreateTexturesFromBuffers(gltfRoot.images,gltfRoot.bufferViews,imageCreateContexts);
            }
            Profiler.EndSample();
            yield return null;

            Profiler.BeginSample("GenerateMaterial");
            if(gltfRoot.materials!=null) {
                materials = new UnityEngine.Material[gltfRoot.materials.Length];
                for(int i=0;i<materials.Length;i++) {
                    materials[i] = materialGenerator.GenerateMaterial( gltfRoot.materials[i], gltfRoot.textures, images, resources );
                }
            }
            Profiler.EndSample();
            yield return null;

            PreparePrimitives(gltfRoot);
            yield return null;

#if !GLTFAST_NO_JOB
            if(imageCreateContexts!=null) {
                foreach(var jh in imageCreateContexts) {
                    while(!jh.jobHandle.IsCompleted) {
                        yield return null;
                    }
                    jh.jobHandle.Complete();
                    images[jh.imageIndex].LoadImage(jh.buffer);
                    jh.gcHandle.Free();
                }
                imageCreateContexts = null;
            }
#endif

            for(int i=0;i<primitiveContexts.Length;i++) {
#if !GLTFAST_NO_JOB
                var primitiveContext = primitiveContexts[i];
                
                while(!primitiveContext.IsCompleted) {
                    yield return null;
                }
#endif
                var primitive = primitiveContext.CreatePrimitive();
                if(primitive.HasValue) {
                    primitives[primitiveContext.primtiveIndex] = primitive.Value;
                    resources.Add(primitive.Value.mesh);
                } else {
                    loadingError = true;
                }

                yield return null;
            }

            // Free temp resources
            primitiveContexts = null;
            DisposeBuffers();

#if GLTFAST_NO_JOB
            yield return null;
#endif
        }

        void DisposeBuffers() {
            foreach (var nativeBuffer in nativeBuffers)
            {
                if(nativeBuffer.IsCreated) {
                    nativeBuffer.Dispose();
                }
            }
            nativeBuffers = null;
            buffers = null;
        }

        void CreateGameObjects( Root gltf, Transform parent ) {

            Profiler.BeginSample("CreateGameObjects");
            var nodes = new Transform[gltf.nodes.Length];
            var relations = new Dictionary<uint,uint>();

            for( uint nodeIndex = 0; nodeIndex < gltf.nodes.Length; nodeIndex++ ) {
                var node = gltf.nodes[nodeIndex];

                if( node.children==null && node.mesh<0 ) {
                    continue;
                }

                var go = new GameObject(node.name ?? "Node");
                nodes[nodeIndex] = go.transform;

                if(node.children!=null) {
                    foreach( var child in node.children ) {
                        relations[child] = nodeIndex;
                    }
                }

                if(node.matrix!=null) {
                    Matrix4x4 m = new Matrix4x4();
                    m.m00 = node.matrix[0];
                    m.m10 = node.matrix[1];
                    m.m20 = node.matrix[2];
                    m.m30 = node.matrix[3];
                    m.m01 = node.matrix[4];
                    m.m11 = node.matrix[5];
                    m.m21 = node.matrix[6];
                    m.m31 = node.matrix[7];
                    m.m02 = node.matrix[8];
                    m.m12 = node.matrix[9];
                    m.m22 = node.matrix[10];
                    m.m32 = node.matrix[11];
                    m.m03 = node.matrix[12];
                    m.m13 = node.matrix[13];
                    m.m23 = node.matrix[14];
                    m.m33 = node.matrix[15];

                    if(m.ValidTRS()) {
                        go.transform.localPosition = new Vector3( m.m03, m.m13, m.m23 );
                        go.transform.localRotation = m.rotation;
                        go.transform.localScale = m.lossyScale;
                    } else {
                        Debug.LogErrorFormat("Invalid matrix on node {0}",nodeIndex);
                        Profiler.EndSample();
                        loadingError = true;
                        return;
                    }
                } else {
                    if(node.translation!=null) {
                        Assert.AreEqual( node.translation.Length, 3 );
                        go.transform.localPosition = new Vector3(
                            node.translation[0],
                            node.translation[1],
                            node.translation[2]
                        );
                    }
                    if(node.rotation!=null) {
                        Assert.AreEqual( node.rotation.Length, 4 );
                        go.transform.localRotation = new Quaternion(
                            node.rotation[0],
                            node.rotation[1],
                            node.rotation[2],
                            node.rotation[3]
                        );
                    }
                    if(node.scale!=null) {
                        Assert.AreEqual( node.scale.Length, 3 );
                        go.transform.localScale = new Vector3(
                            node.scale[0],
                            node.scale[1],
                            node.scale[2]
                        );
                    }
                }

                if(node.mesh>=0) {
                    int end = meshPrimitiveIndex[node.mesh+1];
                    GameObject meshGo = null;
                    for( int i=meshPrimitiveIndex[node.mesh]; i<end; i++ ) {
                        if(meshGo==null) {
                            meshGo = go;
                        } else {
                            meshGo = new GameObject( "Primitive" );
                            meshGo.transform.SetParent(go.transform,false);
                        }
                        var mf = meshGo.AddComponent<MeshFilter>();
                        mf.mesh = primitives[i].mesh;
                        var mr = meshGo.AddComponent<MeshRenderer>();
                        
                        int materialIndex = primitives[i].materialIndex;
                        if(materials!=null && materialIndex>=0 && materialIndex<materials.Length ) {
                            mr.material = materials[primitives[i].materialIndex];
                        } else {
                            mr.material = materialGenerator.GetDefaultMaterial();
                        }
                    }
                }
            }

            foreach( var rel in relations ) {
                nodes[rel.Key]?.SetParent( nodes[rel.Value], false );
            }

            foreach(var scene in gltf.scenes) {
                var go = new GameObject(scene.name ?? "Scene");
                go.transform.SetParent( parent, false);

                // glTF to unity space ( -z forward to z forward )
                go.transform.localScale = new Vector3(1,1,-1);

                foreach(var nodeIndex in scene.nodes) {
                    nodes[nodeIndex]?.SetParent( go.transform, false );
                }
            }

            foreach( var bv in gltf.bufferViews ) {
                if(gltf.buffers[bv.buffer].uri == null) {
                    
                }
            }
            Profiler.EndSample();
        }

        unsafe void CreateTexturesFromBuffers( Schema.Image[] src_images, Schema.BufferView[] bufferViews, List<ImageCreateContext> contexts ) {
            for (int i = 0; i < images.Length; i++) {
                if(images[i]!=null) {
                    resources.Add(images[i]);
                }
                var img = src_images[i];
                bool knownImageType = false;
                if(string.IsNullOrEmpty(img.mimeType)) {
                    Debug.LogWarning("Image is missing mime type");
                    knownImageType = img.uri.EndsWith(".png",StringComparison.OrdinalIgnoreCase)
                        || img.uri.EndsWith(".jpg",StringComparison.OrdinalIgnoreCase)
                        || img.uri.EndsWith(".jpeg",StringComparison.OrdinalIgnoreCase);
                } else {
                    knownImageType = img.mimeType == "image/jpeg" || img.mimeType == "image/png";
                }

                if (knownImageType)
                {
                    if (img.bufferView >= 0)
                    {
                        var bufferView = bufferViews[img.bufferView];
                        var buffer = GetBuffer(bufferView.buffer);
                        var chunk = binChunks[bufferView.buffer];
                        var txt = new UnityEngine.Texture2D(4, 4);
                        txt.name = string.IsNullOrEmpty(img.name) ? string.Format("glb embed texture {0}",i) : img.name;
#if GLTFAST_NO_JOB
                        var imgBytes = Extractor.CreateBufferViewCopy(bufferView,chunk,buffer);
                        txt.LoadImage(imgBytes);
#else
                        var icc = new ImageCreateContext();
                        icc.imageIndex = i;
                        icc.buffer = new byte[bufferView.byteLength];
                        icc.gcHandle = GCHandle.Alloc(icc.buffer,GCHandleType.Pinned);
                        var job = new Jobs.MemCopyJob();
                        job.bufferSize = bufferView.byteLength;
                        fixed( void* src = &(buffer[bufferView.byteOffset + chunk.start]), dst = &(icc.buffer[0]) ) {
                            job.input = src;
                            job.result = dst;
                        }
                        icc.jobHandle = job.Schedule();
                        contexts.Add(icc);
#endif
                        
                        images[i] = txt;
                        resources.Add(txt);
                    }
                }
            }
        }
        public void Destroy() {
            if(materials!=null) {
                foreach( var material in materials ) {
                    UnityEngine.Object.Destroy(material);
                }
                materials = null;
            }

            if(resources!=null) {
                foreach( var resource in resources ) {
                    UnityEngine.Object.Destroy(resource);
                }
                resources = null;
            }
        }

        void PreparePrimitives( Root gltf ) {
            Profiler.BeginSample("PreparePrimitives");
            int totalPrimitives = 0;
            for( int meshIndex = 0; meshIndex<gltf.meshes.Length; meshIndex++ ) {
                var mesh = gltf.meshes[meshIndex];
                meshPrimitiveIndex[meshIndex] = totalPrimitives;
                totalPrimitives += mesh.primitives.Length;
            }
            meshPrimitiveIndex[gltf.meshes.Length] = totalPrimitives;

            primitives = new Primitive[totalPrimitives];
            primitiveContexts = new PrimitiveCreateContextBase[totalPrimitives];

            int i=0;
            for( int meshIndex = 0; meshIndex<gltf.meshes.Length; meshIndex++ ) {
                var mesh = gltf.meshes[meshIndex];
                foreach( var primitive in mesh.primitives ) {
                    
                    PrimitiveCreateContextBase context = null;

                    if( primitive.extensions!=null &&
                        primitive.extensions.KHR_draco_mesh_compression != null )
                    {
                        var c = new PrimitiveDracoCreateContext();
                        PreparePrimitiveDraco(gltf,mesh,primitive,ref c);
                        context = c;
                    } else {
                        var c = new PrimitiveCreateContext();
                        PreparePrimitive(gltf,mesh,primitive,ref c);
                        context = c;
                    }
                    context.primtiveIndex = i;
                    context.primitive = primitive;
                    primitiveContexts[i] = context;
                    i++;
                }
            }
            Profiler.EndSample();
        }

        unsafe void PreparePrimitive( Root gltf, Mesh mesh, MeshPrimitive primitive, ref PrimitiveCreateContext c ) {

            Profiler.BeginSample("PreparePrimitivePrepare");
            c.mesh = mesh;
            c.primitive = primitive;

            // index
            var accessor = gltf.accessors[primitive.indices];
            var bufferView = gltf.bufferViews[accessor.bufferView];
            var bufferIndex = bufferView.buffer;
            var buffer = GetBuffer(bufferIndex);

            GlbBinChunk chunk = binChunks[bufferIndex];
            Assert.AreEqual(accessor.typeEnum, GLTFAccessorAttributeType.SCALAR);
            //Assert.AreEqual(accessor.count * GetLength(accessor.typeEnum) * 4 , (int) chunk.length);
            int start = accessor.byteOffset + bufferView.byteOffset + chunk.start;
#if !GLTFAST_NO_JOB
            int jobHandlesCount = 2;
            if(primitive.attributes.NORMAL>=0) {
                jobHandlesCount++;
            }
            if(primitive.attributes.TANGENT>=0) {
                jobHandlesCount++;
            }
            if(primitive.attributes.TEXCOORD_0>=0) {
                jobHandlesCount++;
            }
            if(primitive.attributes.TEXCOORD_1>=0) {
                jobHandlesCount++;
            }
            if(primitive.attributes.COLOR_0>=0) {
                jobHandlesCount++;
            }
            NativeArray<JobHandle> jobHandles = new NativeArray<JobHandle>(jobHandlesCount, Allocator.TempJob);
            c.gcHandles = new GCHandle[jobHandlesCount];
            // from now on use it as a counter
            jobHandlesCount = 0;
#endif
            Profiler.EndSample();

            Profiler.BeginSample("PrepareIndicesJob");
#if !GLTFAST_NO_JOB
            c.indices = new int[accessor.count];
            c.gcHandles[jobHandlesCount] = GCHandle.Alloc(c.indices, GCHandleType.Pinned);
#endif
            switch( accessor.componentType ) {
            case GLTFComponentType.UnsignedByte:
#if GLTFAST_NO_JOB
                c.indices = Extractor.GetIndicesUInt8(buffer, start, accessor.count);
#else
                var job8 = new Jobs.GetIndicesUInt8Job();
                job8.count = accessor.count;
                fixed( void* src = &(buffer[start]), dst = &(c.indices[0]) ) {
                    job8.input = (byte*)src;
                    job8.result = (int*)dst;
                }
                jobHandles[jobHandlesCount] = job8.Schedule();
#endif
                break;
            case GLTFComponentType.UnsignedShort:
#if GLTFAST_NO_JOB
                c.indices = Extractor.GetIndicesUInt16(buffer, start, accessor.count);
#else
                var job16 = new Jobs.GetIndicesUInt16Job();
                job16.count = accessor.count;
                fixed( void* src = &(buffer[start]), dst = &(c.indices[0]) ) {
                    job16.input = (System.UInt16*) src;
                    job16.result = (int*) dst;
                }
                jobHandles[jobHandlesCount] = job16.Schedule();
#endif
                break;
            case GLTFComponentType.UnsignedInt:
#if GLTFAST_NO_JOB
                c.indices = Extractor.GetIndicesUInt32(buffer, start, accessor.count);
#else
                var job32 = new Jobs.GetIndicesUInt32Job();
                job32.count = accessor.count;
                fixed( void* src = &(buffer[start]), dst = &(c.indices[0]) ) {
                    job32.input = (System.UInt32*) src;
                    job32.result = (int*) dst;
                }
                jobHandles[jobHandlesCount] = job32.Schedule();
#endif
                break;
            default:
                Debug.LogErrorFormat( "Invalid index format {0}", accessor.componentType );
                break;
            }
            jobHandlesCount++;
            Profiler.EndSample();

            // TODO: re-enable test for jobs as well
            #if DEBUG && GLTFAST_NO_JOB
            Profiler.BeginSample("PrepareIndicesSanityTest");
            if( accessor.min!=null && accessor.min.Length>0 && accessor.max!=null && accessor.max.Length>0 ) {
                int minInt = (int) accessor.min[0];
                int maxInt = (int) accessor.max[0];
                int minIndex = int.MaxValue;
                int maxIndex = int.MinValue;
                foreach (var index in c.indices) {
                    Assert.IsTrue( index >= minInt );
                    Assert.IsTrue( index <= maxInt );
                    minIndex = Math.Min(minIndex,index);
                    maxIndex = Math.Max(maxIndex,index);
                }
                if( minIndex!=minInt
                    || maxIndex!=maxInt
                ) {
                    Debug.LogErrorFormat("Faulty index bounds: is {0}:{1} expected:{2}:{3}",minIndex,maxIndex,minInt,maxInt);
                }
            }
            Profiler.EndSample();
            #endif

            Profiler.BeginSample("PreparePositionsJob");
            // position
            int pos = primitive.attributes.POSITION;
            Assert.IsTrue(pos>=0);
            #if DEBUG
            Assert.AreEqual( GetAccessorTye(gltf.accessors[pos].typeEnum), typeof(Vector3) );
            #endif

#if GLTFAST_NO_JOB
            c.positions = gltf.IsAccessorInterleaved(pos)
                ? GetAccessorDataInterleaved<Vector3>( gltf, pos, ref buffer, Extractor.GetVector3sInterleaved )
                : GetAccessorData<Vector3>( gltf, pos, ref buffer, Extractor.GetVector3s );
#else
            // TODO: unify with normals/tangent getter
            accessor = gltf.accessors[pos];
            bufferView = gltf.bufferViews[accessor.bufferView];
            chunk = binChunks[bufferView.buffer];
            c.positions = new Vector3[accessor.count];
            c.gcHandles[jobHandlesCount] = GCHandle.Alloc(c.positions, GCHandleType.Pinned);
            start = accessor.byteOffset + bufferView.byteOffset + chunk.start;
            if (gltf.IsAccessorInterleaved(pos)) {
                var job = new Jobs.GetVector3sInterleavedJob();
                job.count = accessor.count;
                job.byteStride = bufferView.byteStride;
                fixed( void* src = &(buffer[start]), dst = &(c.positions[0]) ) {
                    job.input = (byte*)src;
                    job.result = (Vector3*)dst;
                }
                jobHandles[jobHandlesCount] = job.Schedule();
            } else {
                var job = new Jobs.MemCopyJob();
                job.bufferSize = accessor.count * 12;
                fixed( void* src = &(buffer[start]), dst = &(c.positions[0]) ) {
                    job.input = src;
                    job.result = dst;
                }
                jobHandles[jobHandlesCount] = job.Schedule();
            }
            jobHandlesCount++;
#endif
            Profiler.EndSample();

            #if DEBUG && GLTFAST_NO_JOB
            Profiler.BeginSample("PreparePosSanityCheck");
            var posAcc = gltf.accessors[pos];
            Vector3 minPos = new Vector3( (float) posAcc.min[0], (float) posAcc.min[1], (float) posAcc.min[2] );
            Vector3 maxPos = new Vector3( (float) posAcc.max[0], (float) posAcc.max[1], (float) posAcc.max[2] );
            foreach (var p in c.positions) {
                if( ! (p.x >= minPos.x
                    && p.y >= minPos.y
                    && p.z >= minPos.z
                    && p.x <= maxPos.x
                    && p.y <= maxPos.y
                    && p.z <= maxPos.z
                    ))
                {
                    Debug.LogError("Vertex outside of limits");
                    break;
                }
            }

            var pUsage = new int[c.positions.Length];
            foreach (var index in c.indices) {
                pUsage[index] += 1;
            }
            int pMin = int.MaxValue;
            foreach (var u in pUsage) {
                pMin = Math.Min(pMin,u);
            }
            if(pMin<1) {
                Debug.LogError("Unused vertices");
            }
            Profiler.EndSample();
            #endif

            Profiler.BeginSample("PrepareNormals");
            if(primitive.attributes.NORMAL>=0) {
                pos = primitive.attributes.NORMAL;
                #if DEBUG
                Assert.AreEqual( GetAccessorTye(gltf.accessors[pos].typeEnum), typeof(Vector3) );
                #endif
#if GLTFAST_NO_JOB
                c.normals = gltf.IsAccessorInterleaved(pos)
                    ? GetAccessorDataInterleaved<Vector3>( gltf, pos, ref buffer, Extractor.GetVector3sInterleaved )
                    : GetAccessorData<Vector3>( gltf, pos, ref buffer, Extractor.GetVector3s );
#else
                accessor = gltf.accessors[pos];
                bufferView = gltf.bufferViews[accessor.bufferView];
                chunk = binChunks[bufferView.buffer];
                c.normals = new Vector3[accessor.count];
                c.gcHandles[jobHandlesCount] = GCHandle.Alloc(c.normals, GCHandleType.Pinned);
                start = accessor.byteOffset + bufferView.byteOffset + chunk.start;
                if (gltf.IsAccessorInterleaved(pos)) {
                    var job = new Jobs.GetVector3sInterleavedJob();
                    job.count = accessor.count;
                    job.byteStride = bufferView.byteStride;
                    fixed( void* src = &(buffer[start]), dst = &(c.normals[0]) ) {
                        job.input = (byte*)src;
                        job.result = (Vector3*)dst;
                    }
                    jobHandles[jobHandlesCount] = job.Schedule();
                } else {
                    var job = new Jobs.MemCopyJob();
                    job.bufferSize = accessor.count * 12;
                    fixed( void* src = &(buffer[start]), dst = &(c.normals[0]) ) {
                        job.input = src;
                        job.result = dst;
                    }
                    jobHandles[jobHandlesCount] = job.Schedule();
                }
                jobHandlesCount++;
#endif
            }
            Profiler.EndSample();

            Profiler.BeginSample("PrepareUVs");
#if GLTFAST_NO_JOB
            c.uvs0 = GetUvs(gltf,primitive.attributes.TEXCOORD_0, ref buffer);
            c.uvs1 = GetUvs(gltf,primitive.attributes.TEXCOORD_1, ref buffer);
#else
            if(primitive.attributes.TEXCOORD_0>=0) {
                JobHandle? jh;
                c.uvs0 = GetUvsJob(gltf,primitive.attributes.TEXCOORD_0, ref buffer, out jh, out c.gcHandles[jobHandlesCount] );
                jobHandles[jobHandlesCount] = jh.Value;
                jobHandlesCount++;
            }
            if(primitive.attributes.TEXCOORD_1>=0) {
                JobHandle? jh;
                c.uvs1 = GetUvsJob(gltf,primitive.attributes.TEXCOORD_0, ref buffer, out jh, out c.gcHandles[jobHandlesCount] );
                jobHandles[jobHandlesCount] = jh.Value;
                jobHandlesCount++;
            }
#endif
            Profiler.EndSample();

            Profiler.BeginSample("PrepareTangents");
            if(primitive.attributes.TANGENT>=0) {
                pos = primitive.attributes.TANGENT;
                #if DEBUG
                Assert.AreEqual( GetAccessorTye(gltf.accessors[pos].typeEnum), typeof(Vector4) );
                #endif
#if GLTFAST_NO_JOB
                c.tangents = gltf.IsAccessorInterleaved(pos)
                    ? GetAccessorDataInterleaved<Vector4>( gltf, pos, ref buffer, Extractor.GetVector4sInterleaved)
                    : GetAccessorData<Vector4>( gltf, pos, ref buffer, Extractor.GetVector4s );
#else
                accessor = gltf.accessors[pos];
                bufferView = gltf.bufferViews[accessor.bufferView];
                chunk = binChunks[bufferView.buffer];
                c.tangents = new Vector4[accessor.count];
                c.gcHandles[jobHandlesCount] = GCHandle.Alloc(c.tangents, GCHandleType.Pinned);
                start = accessor.byteOffset + bufferView.byteOffset + chunk.start;
                if (gltf.IsAccessorInterleaved(pos)) {
                    var job = new Jobs.GetVector4sInterleavedJob();
                    job.count = accessor.count;
                    job.byteStride = bufferView.byteStride;
                    fixed( void* src = &(buffer[start]), dst = &(c.tangents[0]) ) {
                        job.input = (byte*)src;
                        job.result = (Vector4*)dst;
                    }
                    jobHandles[jobHandlesCount] = job.Schedule();
                } else {
                    var job = new Jobs.MemCopyJob();
                    job.bufferSize = accessor.count * 16;
                    fixed( void* src = &(buffer[start]), dst = &(c.tangents[0]) ) {
                        job.input = src;
                        job.result = dst;
                    }
                    jobHandles[jobHandlesCount] = job.Schedule();
                }
                jobHandlesCount++;
#endif
            }
            Profiler.EndSample();

            Profiler.BeginSample("PrepareColors");
#if GLTFAST_NO_JOB
            GetColors(gltf,primitive.attributes.COLOR_0, ref buffer, out c.colors32, out c.colors);
#else
            if(primitive.attributes.COLOR_0>=0) {
                JobHandle? jh;
                GetColorsJob(gltf,primitive.attributes.COLOR_0, ref buffer, out c.colors32, out c.colors, out jh, out c.gcHandles[jobHandlesCount] );
                jobHandles[jobHandlesCount] = jh.Value;
                jobHandlesCount++;
            }
#endif
            Profiler.EndSample();

#if !GLTFAST_NO_JOB
            c.jobHandle = JobHandle.CombineDependencies(jobHandles);
            jobHandles.Dispose();
#endif
        }

        void PreparePrimitiveDraco( Root gltf, Mesh mesh, MeshPrimitive primitive, ref PrimitiveDracoCreateContext c ) {
            var draco_ext = primitive.extensions.KHR_draco_mesh_compression;
            
            var bufferView = gltf.bufferViews[draco_ext.bufferView];
            var buffer = GetNativeBuffer(bufferView.buffer);

            var job = new DracoMeshLoader.DracoJob();

            c.dracoResult = new NativeArray<int>(1,DracoMeshLoader.defaultAllocator);
            c.dracoPtr = new NativeArray<IntPtr>(1,DracoMeshLoader.defaultAllocator);

            job.data = buffer.Slice(bufferView.byteOffset,bufferView.byteLength);
            job.result = c.dracoResult;
            job.outMesh = c.dracoPtr;

            c.jobHandle = job.Schedule();
        }

        void OnMeshesLoaded( Mesh mesh ) {
            Debug.Log("draco is ready");
        }

#if GLTFAST_NO_JOB

        CompType[] GetAccessorData<CompType>( Root gltf, int accessorIndex, ref byte[] bytes, ExtractAccessor<CompType> extractor ) {
            Assert.IsTrue(accessorIndex>=0);
            var accessor = gltf.accessors[accessorIndex];
            var bufferView = gltf.bufferViews[accessor.bufferView];
            var chunk = binChunks[bufferView.buffer];

            int start = accessor.byteOffset + bufferView.byteOffset + chunk.start;

            #if DEBUG
            int dataLength = ( accessor.count
                * GetAccessorAttriuteTypeLength( accessor.typeEnum )
                * GetAccessorComponentTypeLength( accessor.componentType ) );
            // inside bufferView boundary?
            Assert.IsTrue( accessor.byteOffset + dataLength <= bufferView.byteLength );
            // inside chunk boundary?
            Assert.IsTrue( accessor.byteOffset + bufferView.byteOffset + dataLength <= (int) chunk.length );
            // inside bytes boundary?
            Assert.IsTrue( start + dataLength <= bytes.Length );
            #endif

            return extractor(
                ref bytes
                ,start
                ,accessor.count
                );
        }

        CompType[] GetAccessorDataInterleaved<CompType>( Root gltf, int accessorIndex, ref byte[] bytes, ExtractInterleavedAccessor<CompType> extractor)
        {
            Assert.IsTrue(accessorIndex >= 0);
            var accessor = gltf.accessors[accessorIndex];
            var bufferView = gltf.bufferViews[accessor.bufferView];
            var chunk = binChunks[bufferView.buffer];

            int start = accessor.byteOffset + bufferView.byteOffset + chunk.start;

#if DEBUG
            int dataLength = (accessor.count * bufferView.byteStride);
            // inside bufferView boundary?
            Assert.IsTrue(dataLength <= bufferView.byteLength);
            // inside chunk boundary?
            Assert.IsTrue(bufferView.byteOffset + dataLength <= (int)chunk.length);
            // inside bytes boundary?
            Assert.IsTrue(start + dataLength <= bytes.Length);
#endif

            return extractor(
                ref bytes
                , start
                , accessor.count
                , bufferView.byteStride
                );
        }

        Vector2[] GetUvs( Root gltf, int accessorIndex, ref byte[] bytes ) {
            if(accessorIndex>=0) {
                var uvAccessor = gltf.accessors[accessorIndex];
                Assert.AreEqual( uvAccessor.typeEnum, GLTFAccessorAttributeType.VEC2 );
                #if DEBUG
                Assert.AreEqual( GetAccessorTye(uvAccessor.typeEnum), typeof(Vector2) );
                #endif
                bool interleaved = gltf.IsAccessorInterleaved(accessorIndex);
                switch( uvAccessor.componentType ) {
                case GLTFComponentType.Float:
                    return interleaved
                        ? GetAccessorDataInterleaved<Vector2>( gltf, accessorIndex, ref bytes, Extractor.GetUVsFloatInterleaved)
                        : GetAccessorData<Vector2>( gltf, accessorIndex, ref bytes, Extractor.GetUVsFloat );
                case GLTFComponentType.UnsignedByte:
                    return interleaved
                        ? GetAccessorDataInterleaved<Vector2>( gltf, accessorIndex, ref bytes, Extractor.GetUVsUInt8Interleaved )
                        : GetAccessorData<Vector2>( gltf, accessorIndex, ref bytes, Extractor.GetUVsUInt8 );
                case GLTFComponentType.UnsignedShort:
                    return interleaved
                        ? GetAccessorDataInterleaved<Vector2>( gltf, accessorIndex, ref bytes, Extractor.GetUVsUInt16Interleaved )
                        : GetAccessorData<Vector2>( gltf, accessorIndex, ref bytes, Extractor.GetUVsUInt16 );
                default:
                    Debug.LogErrorFormat("Unsupported UV format {0}", uvAccessor.componentType);
                    break;
                }
            }
            return null;
        }

        void GetColors( Root gltf, int accessorIndex, ref byte[] bytes, out Color32[] colors32, out Color[] colors ) {

            colors = null;
            colors32 = null;
            if(accessorIndex>=0) {
                var colorAccessor = gltf.accessors[accessorIndex];
                var interleaved = gltf.IsAccessorInterleaved( accessorIndex );
                if (colorAccessor.typeEnum == GLTFAccessorAttributeType.VEC3)
                {
                    switch (colorAccessor.componentType)
                    {
                        case GLTFComponentType.Float:
                            colors = interleaved
                                ? GetAccessorDataInterleaved<Color>( gltf, accessorIndex, ref bytes, Extractor.GetColorsVec3FloatInterleaved)
                                : GetAccessorData<Color>( gltf, accessorIndex, ref bytes, Extractor.GetColorsVec3Float);;
                            break;
                        case GLTFComponentType.UnsignedByte:
                            colors32 = interleaved
                                ? GetAccessorDataInterleaved<Color32>( gltf, accessorIndex, ref bytes, Extractor.GetColorsVec3UInt8Interleaved)
                                : GetAccessorData<Color32>( gltf, accessorIndex, ref bytes, Extractor.GetColorsVec3UInt8);
                            break;
                        case GLTFComponentType.UnsignedShort:
                            colors = interleaved
                                ? GetAccessorDataInterleaved<Color>( gltf, accessorIndex, ref bytes, Extractor.GetColorsVec3UInt16Interleaved )
                                : GetAccessorData<Color>( gltf, accessorIndex, ref bytes, Extractor.GetColorsVec3UInt16 );
                            break;
                        default:
                            Debug.LogErrorFormat(ErrorUnsupportedColorFormat, colorAccessor.componentType);
                            break;
                    }
                }
                else if (colorAccessor.typeEnum == GLTFAccessorAttributeType.VEC4)
                {
                    switch (colorAccessor.componentType)
                    {
                        case GLTFComponentType.Float:
                            colors = interleaved
                                ? GetAccessorDataInterleaved<Color>( gltf, accessorIndex, ref bytes, Extractor.GetColorsVec4FloatInterleaved)
                                : GetAccessorData<Color>( gltf, accessorIndex, ref bytes, Extractor.GetColorsVec4Float);
                            break;
                        case GLTFComponentType.UnsignedByte:
                            colors32 = interleaved
                                ? GetAccessorDataInterleaved<Color32>( gltf, accessorIndex, ref bytes, Extractor.GetColorsVec4UInt8Interleaved)
                                : GetAccessorData<Color32>( gltf, accessorIndex, ref bytes, Extractor.GetColorsVec4UInt8);
                            break;
                        case GLTFComponentType.UnsignedShort:
                            colors = interleaved
                                ? GetAccessorDataInterleaved<Color>( gltf, accessorIndex, ref bytes, Extractor.GetColorsVec4UInt16Interleaved)
                                : GetAccessorData<Color>( gltf, accessorIndex, ref bytes, Extractor.GetColorsVec4UInt16);
                            break;
                        default:
                            Debug.LogErrorFormat(ErrorUnsupportedColorFormat, colorAccessor.componentType);
                            break;
                    }
                } else {
                    Debug.LogErrorFormat("Unsupported color accessor type {0}", colorAccessor.typeEnum );
                }
            }
        }

#else

        unsafe Vector2[] GetUvsJob( Root gltf, int accessorIndex, ref byte[] bytes, out JobHandle? jobHandle, out GCHandle resultHandle ) {
            if(accessorIndex>=0) {
                var uvAccessor = gltf.accessors[accessorIndex];
                Assert.AreEqual( uvAccessor.typeEnum, GLTFAccessorAttributeType.VEC2 );
                #if DEBUG
                Assert.AreEqual( GetAccessorTye(uvAccessor.typeEnum), typeof(Vector2) );
                #endif
                bool interleaved = gltf.IsAccessorInterleaved(accessorIndex);

                var bufferView = gltf.bufferViews[uvAccessor.bufferView];
                var chunk = binChunks[bufferView.buffer];
                var result = new Vector2[uvAccessor.count];
                resultHandle = GCHandle.Alloc(result, GCHandleType.Pinned);
                int start = uvAccessor.byteOffset + bufferView.byteOffset + chunk.start;

                switch( uvAccessor.componentType ) {
                case GLTFComponentType.Float:
                    if (gltf.IsAccessorInterleaved(accessorIndex)) {
                        var job = new Jobs.GetVector2sInterleavedJob();
                        job.count = uvAccessor.count;
                        job.byteStride = bufferView.byteStride;
                        fixed( void* src = &(bytes[start]), dst = &(result[0]) ) {
                            job.input = (byte*)src;
                            job.result = (Vector2*)dst;
                        }
                        jobHandle = job.Schedule();
                    } else {
                        var job = new Jobs.GetUVsFloatJob();
                        job.count = uvAccessor.count;
                        fixed( void* src = &(bytes[start]), dst = &(result[0]) ) {
                            job.input = src;
                            job.result = (Vector2*)dst;
                        }
                        jobHandle = job.Schedule();
                    }
                    break;
                case GLTFComponentType.UnsignedByte:
                    if (gltf.IsAccessorInterleaved(accessorIndex)) {
                        var job = new Jobs.GetUVsUInt8InterleavedJob();
                        job.count = uvAccessor.count;
                        job.byteStride = bufferView.byteStride;
                        fixed( void* src = &(bytes[start]), dst = &(result[0]) ) {
                            job.input = (byte*) src;
                            job.result = (Vector2*)dst;
                        }
                        jobHandle = job.Schedule();
                    } else {
                        var job = new Jobs.GetUVsUInt8Job();
                        job.count = uvAccessor.count;
                        fixed( void* src = &(bytes[start]), dst = &(result[0]) ) {
                            job.input = (byte*) src;
                            job.result = (Vector2*)dst;
                        }
                        jobHandle = job.Schedule();
                    }
                    break;
                case GLTFComponentType.UnsignedShort:
                    if (gltf.IsAccessorInterleaved(accessorIndex)) {
                        var job = new Jobs.GetUVsUInt16InterleavedJob();
                        job.count = uvAccessor.count;
                        job.byteStride = bufferView.byteStride;
                        fixed( void* src = &(bytes[start]), dst = &(result[0]) ) {
                            job.input = (byte*) src;
                            job.result = (Vector2*)dst;
                        }
                        jobHandle = job.Schedule();
                    } else {
                        var job = new Jobs.GetUVsUInt16Job();
                        job.count = uvAccessor.count;
                        fixed( void* src = &(bytes[start]), dst = &(result[0]) ) {
                            job.input = (System.UInt16*) src;
                            job.result = (Vector2*)dst;
                        }
                        jobHandle = job.Schedule();
                    }
                    break;
                default:
                    jobHandle = null;
                    Debug.LogErrorFormat("Unsupported UV format {0}", uvAccessor.componentType);
                    break;
                }
                return result;
            }
            jobHandle = null;
            return null;
        }

        unsafe void GetColorsJob( Root gltf, int accessorIndex, ref byte[] bytes, out Color32[] colors32, out Color[] colors, out JobHandle? jobHandle, out GCHandle resultHandle ) {
            
            var colorAccessor = gltf.accessors[accessorIndex];
            var bufferView = gltf.bufferViews[colorAccessor.bufferView];
            var chunk = binChunks[bufferView.buffer];
            var interleaved = gltf.IsAccessorInterleaved( accessorIndex );
            int start = colorAccessor.byteOffset + bufferView.byteOffset + chunk.start;

            if(colorAccessor.componentType == GLTFComponentType.UnsignedByte ) {
                colors32 = new Color32[colorAccessor.count];
                resultHandle = GCHandle.Alloc(colors32,GCHandleType.Pinned);
                colors = null;
            } else {
                colors = new Color[colorAccessor.count];
                resultHandle = GCHandle.Alloc(colors,GCHandleType.Pinned);
                colors32 = null;
            }
            jobHandle = null;

            if (colorAccessor.typeEnum == GLTFAccessorAttributeType.VEC3)
            {
                switch (colorAccessor.componentType)
                {
                    case GLTFComponentType.Float:
                        if(interleaved) {
                            Debug.LogError("Not jobified yet!");
                        } else {
                            var job = new Jobs.GetColorsVec3FloatJob();
                            job.count = colorAccessor.count;
                            fixed( void* src = &(bytes[start]), dst = &(colors[0]) ) {
                                job.input = (float*) src;
                                job.result = (Color*)dst;
                            }
                            jobHandle = job.Schedule();
                        }
                        break;
                    case GLTFComponentType.UnsignedByte:
                        if(interleaved) {
                            Debug.LogError("Not jobified yet!");
                        } else {
                            var job = new Jobs.GetColorsVec3UInt8Job();
                            job.count = colorAccessor.count;
                            fixed( void* src = &(bytes[start]), dst = &(colors32[0]) ) {
                                job.input = (byte*) src;
                                job.result = (Color32*)dst;
                            }
                            jobHandle = job.Schedule();
                        }
                        break;
                    case GLTFComponentType.UnsignedShort:
                        if(interleaved) {
                            Debug.LogError("Not jobified yet!");
                        } else {
                            var job = new Jobs.GetColorsVec3UInt16Job();
                            job.count = colorAccessor.count;
                            fixed( void* src = &(bytes[start]), dst = &(colors[0]) ) {
                                job.input = (System.UInt16*) src;
                                job.result = (Color*)dst;
                            }
                            jobHandle = job.Schedule();
                        }
                        break;
                    default:
                        Debug.LogErrorFormat(ErrorUnsupportedColorFormat, colorAccessor.componentType);
                        break;
                }
            }
            else if (colorAccessor.typeEnum == GLTFAccessorAttributeType.VEC4)
            {
                switch (colorAccessor.componentType)
                {
                    case GLTFComponentType.Float:
                        if(interleaved) {
                            Debug.LogError("Not jobified yet!");
                        } else {
                            var job = new Jobs.MemCopyJob();
                            job.bufferSize = colorAccessor.count*16;
                            fixed( void* src = &(bytes[start]), dst = &(colors[0]) ) {
                                job.input = src;
                                job.result = dst;
                            }
                            jobHandle = job.Schedule();
                        }
                        break;
                    case GLTFComponentType.UnsignedByte:
                        if(interleaved) {
                            Debug.LogError("Not jobified yet!");
                        } else {
                            var job = new Jobs.MemCopyJob();
                            job.bufferSize = colorAccessor.count*4;
                            fixed( void* src = &(bytes[start]), dst = &(colors32[0]) ) {
                                job.input = src;
                                job.result = dst;
                            }
                            jobHandle = job.Schedule();
                        }
                        break;
                    case GLTFComponentType.UnsignedShort:
                        if(interleaved) {
                            Debug.LogError("Not jobified yet!");
                        } else {
                            var job = new Jobs.GetColorsVec4UInt16Job();
                            job.count = colorAccessor.count;
                            fixed( void* src = &(bytes[start]), dst = &(colors[0]) ) {
                                job.input = (System.UInt16*) src;
                                job.result = (Color*)dst;
                            }
                            jobHandle = job.Schedule();
                        }
                        break;
                    default:
                        Debug.LogErrorFormat(ErrorUnsupportedColorFormat, colorAccessor.componentType);
                        break;
                }
            } else {
                Debug.LogErrorFormat("Unsupported color accessor type {0}", colorAccessor.typeEnum );
            }
        }
#endif

#if DEBUG
        static Type GetAccessorTye( GLTFAccessorAttributeType accessorAttributeType ) {
            switch (accessorAttributeType)
            {
                case GLTFAccessorAttributeType.SCALAR:
                    return typeof(float);
                case GLTFAccessorAttributeType.VEC2:
                    return typeof(Vector2);
                case GLTFAccessorAttributeType.VEC3:
                    return typeof(Vector3);
                case GLTFAccessorAttributeType.VEC4:
                case GLTFAccessorAttributeType.MAT2:
                    return typeof(Vector4);
                case GLTFAccessorAttributeType.MAT3:
                    return typeof(Matrix4x4);
                case GLTFAccessorAttributeType.MAT4:
                default:
                    Debug.LogError("Unknown/Unsupported GLTFAccessorAttributeType");
                    return typeof(float);
            }
        }

        static Type GetAccessorComponentType( GLTFComponentType componentType ) {
            switch (componentType)
            {
                case GLTFComponentType.Byte:
                    return typeof(byte);
                case GLTFComponentType.Float:
                    return typeof(float);
                case GLTFComponentType.Short:
                    return typeof(System.Int16);
                case GLTFComponentType.UnsignedByte:
                    return typeof(byte);
                case GLTFComponentType.UnsignedInt:
                    return typeof(int);
                case GLTFComponentType.UnsignedShort:
                    return typeof(System.UInt16);
                default:
                    Debug.LogError("Unknown GLTFComponentType");
                    return null;
            }
        }
#endif // DEBUG
    }
}