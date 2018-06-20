using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;

namespace GLTFast {

    using Schema;

    public class GLTFast {

        const uint GLB_MAGIC = 0x46546c67;

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
              
		public delegate CompType[] ExtractAccessor<CompType>(ref byte[] bytes, int start, int count);
        public delegate CompType[] ExtractInterleavedAccessor<CompType>(ref byte[] bytes, int start, int count, int byteStride);
        
        Root gltf;
        GlbBinChunk[] binChunks;
        UnityEngine.Material[] materials;
        List<UnityEngine.Object> resources;

        IMaterialGenerator materialGenerator;

        public GLTFast( byte[] bytes, Transform parent = null ) {
            materialGenerator = new DefaultMaterialGenerator();
            LoadGlb(bytes,parent);
        }

        public static GLTFast LoadGlbFile( string path, Transform parent = null )
        {
            var bytes = File.ReadAllBytes(path);

            if (bytes == null || bytes.Length < 12)
            {
                return null;
            }
            return new GLTFast( bytes, parent );
        }

        bool LoadGlb( byte[] bytes, Transform parent = null ) {
            uint magic = BitConverter.ToUInt32( bytes, 0 );

            if (magic != GLB_MAGIC)
                return false;
    

            uint version = BitConverter.ToUInt32( bytes, 4 );
            //uint length = BitConverter.ToUInt32( bytes, 8 );

            //Debug.Log( string.Format("version: {0:X}; length: {1}", version, length ) );

            if (version != 2)
                return false;

            int index = 12; // first chung header

            gltf = null;

            var binChunksList = new List<GlbBinChunk>();

            while( index < bytes.Length ) {
                uint chLength = BitConverter.ToUInt32( bytes, index );
                index += 4;
                uint chType = BitConverter.ToUInt32( bytes, index );
                index += 4;

                //Debug.Log( string.Format("chunk: {0:X}; length: {1}", chType, chLength) );

                if (chType == (uint)ChunkFormat.BIN) {
					//Debug.Log( string.Format("chunk: BIN; length: {0}", chLength) );
                    binChunksList.Add(new GlbBinChunk( index, chLength));
                }
                else if (chType == (uint)ChunkFormat.JSON) {
                    Assert.IsNull(gltf);
                    string json = System.Text.Encoding.UTF8.GetString(bytes, index, (int)chLength );
					//Debug.Log( string.Format("chunk: JSON; length: {0}", json ) );
                    gltf = JsonUtility.FromJson<Root>(json);
                }
 
                index += (int) chLength;
            }

            //Debug.Log(index);

            if(gltf!=null) {
                //Debug.Log(gltf);
                binChunks = binChunksList.ToArray();
                CreateGameObjects( parent, bytes );
            }
            return true;
        }

        void CreateGameObjects( Transform parent, byte[] bytes ) {

            var primitives = new List<Primitive>(gltf.meshes.Length);
            var meshPrimitiveIndex = new int[gltf.meshes.Length+1];

            Texture2D[] images = null;

            resources = new List<UnityEngine.Object>();

            if (gltf.images != null) {
                images = new Texture2D[gltf.images.Length];
                for (int i = 0; i < images.Length; i++) {
                    var img = gltf.images[i];
                    if (img.mimeType == "image/jpeg" || img.mimeType == "image/png")
                    {
                        if (img.bufferView >= 0)
                        {
                            var bufferView = gltf.bufferViews[img.bufferView];
                            var chunk = binChunks[bufferView.buffer];
                            var imgBytes = Extractor.CreateBufferViewCopy(bufferView,chunk,bytes);
                            var txt = new UnityEngine.Texture2D(4, 4);
                            txt.name = string.IsNullOrEmpty(img.name) ? string.Format("glb embed texture {0}",i) : img.name;
                            txt.LoadImage(imgBytes);
                            images[i] = txt;
                            resources.Add(txt);
                        } else
                        if(!string.IsNullOrEmpty(img.uri)) {
                            Debug.LogError("Loading from URI not supported");
                        }
                    } else {
                        Debug.LogErrorFormat("Unknown image mime type {0}",img.mimeType);
                    }
                }
            }

            if(gltf.materials!=null) {
                materials = new UnityEngine.Material[gltf.materials.Length];
                for(int i=0;i<materials.Length;i++) {
					materials[i] = materialGenerator.GenerateMaterial( gltf.materials[i], gltf.textures, images, resources );
				}
            }

            //foreach( var mesh in gltf.meshes ) {
            for( int meshIndex = 0; meshIndex<gltf.meshes.Length; meshIndex++ ) {
                var mesh = gltf.meshes[meshIndex];
                meshPrimitiveIndex[meshIndex] = primitives.Count;

                foreach( var primitive in mesh.primitives ) {
                    
                    // index
                    var accessor = gltf.accessors[primitive.indices];
                    var bufferView = gltf.bufferViews[accessor.bufferView];
                    var buffer = bufferView.buffer;

                    GlbBinChunk chunk = binChunks[buffer];
                    Assert.AreEqual(accessor.typeEnum, GLTFAccessorAttributeType.SCALAR);
                    //Assert.AreEqual(accessor.count * GetLength(accessor.typeEnum) * 4 , (int) chunk.length);
                    int[] indices = null;
                    switch( accessor.componentType ) {
                    case GLTFComponentType.UnsignedByte:
                        indices = Extractor.GetIndicesUInt8(bytes, accessor.byteOffset + bufferView.byteOffset + chunk.start, accessor.count);
                        break;
                    case GLTFComponentType.UnsignedShort:
						indices = Extractor.GetIndicesUInt16(bytes, accessor.byteOffset + bufferView.byteOffset + chunk.start, accessor.count);
                        break;
                    case GLTFComponentType.UnsignedInt:
						indices = Extractor.GetIndicesUInt32(bytes, accessor.byteOffset + bufferView.byteOffset + chunk.start, accessor.count);
                        break;
                    default:
                        Debug.LogErrorFormat( "Invalid index format {0}", accessor.componentType );
                        break;
                    }

                    // position
                    int pos = primitive.attributes.POSITION;
                    Assert.IsTrue(pos>=0);
                    #if DEBUG
                    Assert.AreEqual( GetAccessorTye(gltf.accessors[pos].typeEnum), typeof(Vector3) );
#endif
					var positions = gltf.IsAccessorInterleaved(pos)
		                ? GetAccessorDataInterleaved<Vector3>( pos, ref bytes, Extractor.GetVector3sInterleaved )
		                : GetAccessorData<Vector3>( pos, ref bytes, Extractor.GetVector3s );

                    Vector3[] normals = null;
                    if(primitive.attributes.NORMAL>=0) {
                        #if DEBUG
                        Assert.AreEqual( GetAccessorTye(gltf.accessors[primitive.attributes.NORMAL].typeEnum), typeof(Vector3) );
                        #endif
						normals = gltf.IsAccessorInterleaved(pos)
						    ? GetAccessorDataInterleaved<Vector3>( primitive.attributes.NORMAL, ref bytes, Extractor.GetVector3sInterleaved )
						    : GetAccessorData<Vector3>( primitive.attributes.NORMAL, ref bytes, Extractor.GetVector3s );
                    }

                    Vector2[] uvs0 = GetUvs(primitive.attributes.TEXCOORD_0, ref bytes);
                    Vector2[] uvs1 = GetUvs(primitive.attributes.TEXCOORD_1, ref bytes);
                    
                    Vector4[] tangents = null;
                    if(primitive.attributes.TANGENT>=0) {
                        #if DEBUG
                        Assert.AreEqual( GetAccessorTye(gltf.accessors[primitive.attributes.TANGENT].typeEnum), typeof(Vector4) );
                        #endif
						tangents = gltf.IsAccessorInterleaved(pos)
    					    ? GetAccessorDataInterleaved<Vector4>(primitive.attributes.TANGENT, ref bytes, Extractor.GetVector4sInterleaved)
    						: GetAccessorData<Vector4>( primitive.attributes.TANGENT, ref bytes, Extractor.GetVector4s );
                    }

                    Color32[] colors32;
                    Color[] colors;
                    GetColors(primitive.attributes.COLOR_0, ref bytes, out colors32, out colors);

                    var msh = new UnityEngine.Mesh();
                    msh.name = mesh.name;
                    msh.vertices = positions;
                    msh.SetIndices(indices, MeshTopology.Triangles, 0);
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
                    primitives.Add( new Primitive(msh,primitive.material) );
                    resources.Add(msh);
                }
            }

            meshPrimitiveIndex[gltf.meshes.Length] = primitives.Count;

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
        }

        public void Destroy() {
            foreach( var material in materials ) {
                UnityEngine.Object.Destroy(material);
            }
            materials = null;

            foreach( var resource in resources ) {
                UnityEngine.Object.Destroy(resource);
            }
            resources = null;
        }

        CompType[] GetAccessorData<CompType>( int accessorIndex, ref byte[] bytes, ExtractAccessor<CompType> extractor ) {
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

		CompType[] GetAccessorDataInterleaved<CompType>(int accessorIndex, ref byte[] bytes, ExtractInterleavedAccessor<CompType> extractor)
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

        Vector2[] GetUvs( int accessorIndex, ref byte[] bytes ) {
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
                        ? GetAccessorDataInterleaved<Vector2>( accessorIndex, ref bytes, Extractor.GetUVsFloatInterleaved)
						: GetAccessorData<Vector2>( accessorIndex, ref bytes, Extractor.GetUVsFloat );
                case GLTFComponentType.UnsignedByte:
					return interleaved
						? GetAccessorDataInterleaved<Vector2>( accessorIndex, ref bytes, Extractor.GetUVsUInt8Interleaved )
						: GetAccessorData<Vector2>( accessorIndex, ref bytes, Extractor.GetUVsUInt8 );
                case GLTFComponentType.UnsignedShort:
					return interleaved
                        ? GetAccessorDataInterleaved<Vector2>( accessorIndex, ref bytes, Extractor.GetUVsUInt16Interleaved )
                        : GetAccessorData<Vector2>( accessorIndex, ref bytes, Extractor.GetUVsUInt16 );
                default:
                    Debug.LogErrorFormat("Unsupported UV format {0}", uvAccessor.componentType);
                    break;
                }
            }
            return null;
        }

        void GetColors( int accessorIndex, ref byte[] bytes, out Color32[] colors32, out Color[] colors ) {

			const string ErrorUnsupportedColorFormat = "Unsupported Color format {0}";

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
                                ? GetAccessorDataInterleaved<Color>(accessorIndex, ref bytes, Extractor.GetColorsVec3FloatInterleaved)
                                : GetAccessorData<Color>(accessorIndex, ref bytes, Extractor.GetColorsVec3Float);;
							break;
						case GLTFComponentType.UnsignedByte:
							colors32 = interleaved
                                ? GetAccessorDataInterleaved<Color32>(accessorIndex, ref bytes, Extractor.GetColorsVec3UInt8Interleaved)
                                : GetAccessorData<Color32>(accessorIndex, ref bytes, Extractor.GetColorsVec3UInt8);
							break;
						case GLTFComponentType.UnsignedShort:
							colors = interleaved
                                ? GetAccessorDataInterleaved<Color>( accessorIndex, ref bytes, Extractor.GetColorsVec3UInt16Interleaved )
                                : GetAccessorData<Color>( accessorIndex, ref bytes, Extractor.GetColorsVec3UInt16 );
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
                                ? GetAccessorDataInterleaved<Color>(accessorIndex, ref bytes, Extractor.GetColorsVec4FloatInterleaved)
                                : GetAccessorData<Color>(accessorIndex, ref bytes, Extractor.GetColorsVec4Float);
                            break;
                        case GLTFComponentType.UnsignedByte:
							colors32 = interleaved
                                ? GetAccessorDataInterleaved<Color32>(accessorIndex, ref bytes, Extractor.GetColorsVec4UInt8Interleaved)
                                : GetAccessorData<Color32>(accessorIndex, ref bytes, Extractor.GetColorsVec4UInt8);
                            break;
                        case GLTFComponentType.UnsignedShort:
							colors = interleaved
                                ? GetAccessorDataInterleaved<Color>(accessorIndex, ref bytes, Extractor.GetColorsVec4UInt16Interleaved)
                                : GetAccessorData<Color>(accessorIndex, ref bytes, Extractor.GetColorsVec4UInt16);
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

        public static int GetAccessorComponentTypeLength( GLTFComponentType componentType ) {
            switch (componentType)
            {
                case GLTFComponentType.Byte:
                case GLTFComponentType.UnsignedByte:
                    return 1;
                case GLTFComponentType.Short:
                case GLTFComponentType.UnsignedShort:
                    return 2;
        		case GLTFComponentType.Float:
        		case GLTFComponentType.UnsignedInt:
            		return 4;
                default:
                    Debug.LogError("Unknown GLTFComponentType");
                    return 0;
            }
        }

        public static int GetAccessorAttriuteTypeLength( GLTFAccessorAttributeType type ) {
            switch (type)
            {
                case GLTFAccessorAttributeType.SCALAR:
                    return 1;
                case GLTFAccessorAttributeType.VEC2:
                    return 2;
                case GLTFAccessorAttributeType.VEC3:
                    return 3;
                case GLTFAccessorAttributeType.VEC4:
                case GLTFAccessorAttributeType.MAT2:
                    return 4;
                case GLTFAccessorAttributeType.MAT3:
                    return 9;
                case GLTFAccessorAttributeType.MAT4:
                    return 16;
                default:
                    Debug.LogError("Unknown GLTFAccessorAttributeType");
                    return 0;
            }
        }
    }
}