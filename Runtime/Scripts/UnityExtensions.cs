using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

// file originally from unitygltf
namespace GLTFast.Maths
{
    public struct PosRot
    {
        public Vector3 Position;
        public Quaternion Rotation;

        public static PosRot FromGlobalTransform(Transform t)
        {
            return new PosRot
            {
                Position = t.position,
                Rotation = t.rotation,
            };
        }
    }

    public class BlendShape
    {
        public string Name;

        public BlendShape(string name)
        {
            Name = name;
        }

        public List<Vector3> Positions = new List<Vector3>();
        public List<Vector3> Normals = new List<Vector3>();
        public List<Vector3> Tangents = new List<Vector3>();
    }

    public static class UnityExtensions
    {
#if true
        [ReadOnly]
        public static float4x4 GLTF_UNITY = new float4x4(
            new float4(-1f,0f,0f,0f), // Xg->-Xu
            new float4(0f,1f,0f,0f), // Yg->Yu
            new float4(0f,0f,1f,0f), // Zg->Zu
            new float4(0f,0f,0f,1f)
            );
        [ReadOnly]
        public static float4x4 UNITY_GLTF = new float4x4(
            new float4(-1f, 0f, 0f, 0f), // Xu->-Xg
            new float4(0f, 1f, 0f, 0f), // Yu->Yg
            new float4(0f, 0f, 1f, 0f), // Zu->Zg
            new float4(0f, 0f, 0f, 1f)
            );
#else
        [ReadOnly]
        public static float4x4 GLTF_UNITY = new float4x4(
            new float4(1f,0f,0f,0f), // Xg->Xu
            new float4(0f,1f,0f,0f), // Yg->Yu
            new float4(0f,0f,-1f,0f), // Zg->-Zu
            new float4(0f,0f,0f,1f)
            );
        [ReadOnly]
        public static float4x4 UNITY_GLTF = new float4x4(
            new float4(1f, 0f, 0f, 0f), // Xu->Xg
            new float4(0f, 1f, 0f, 0f), // Yu->Yg
            new float4(0f, 0f, -1f, 0f), // Zu->-Zg
            new float4(0f, 0f, 0f, 1f)
            );
#endif
        #region Unity.Mathematics
        public static float3 ToUnity(this float3 v)
        {
            float4 v4 = new float4(v.x, v.y, v.z, 1f);
            float4 vres = math.mul(v4, GLTF_UNITY);
            return new float3(vres.x, vres.y, vres.z);
        }
        public static float3 ToGLTF(this float3 v)
        {
            float4 v4 = new float4(v.x, v.y, v.z, 1f);
            float4 vres = math.mul(v4, UNITY_GLTF);
            return new float3(vres.x, vres.y, vres.z);
        }
        public static float3 ToUnityVector(this float3 v)
        {
            float4 v4 = new float4(v.x, v.y, v.z, 1f);
            float4 vres = math.mul(v4, GLTF_UNITY);
            float3 res = new float3(vres.x, vres.y, vres.z);
            res = math.normalize(res);
            return res;
        }
        public static float3 ToGLTFVector(this float3 v)
        {
            float4 v4 = new float4(v.x, v.y, v.z, 1f);
            float4 vres = math.mul(v4, UNITY_GLTF);
            float3 res = new float3(vres.x, vres.y, vres.z);
            res = math.normalize(res);
            return res;
        }
        public static float4 ToUnity(this float4 v)
        {
            float4 v4 = new float4(v.x, v.y, v.z, 1f);
            float4 vres = math.mul(v4, GLTF_UNITY);
            return vres;
        }
        public static float4 ToGLTF(this float4 v)
        {
            float4 v4 = new float4(v.x, v.y, v.z, 1f);
            float4 vres = math.mul(v4, UNITY_GLTF);
            return vres;
        }
        public static float4 ToUnityVector(this float4 v)
        {
            float4 v4 = new float4(v.x, v.y, v.z, 1f);
            float4 vres = math.mul(v4, GLTF_UNITY);
            return math.normalize(vres);
        }
        public static float4 ToGLTFVector(this float4 v)
        {
            float4 v4 = new float4(v.x, v.y, v.z, 1f);
            float4 vres = math.mul(v4, UNITY_GLTF);
            return math.normalize(vres);
        }
        public static quaternion ToUnity(this quaternion q)
        {
            float3 translation = new float3();
            float3 scale = new float3();
            float4 rotation = new float4();
            GLTF_UNITY.Decompose(out translation, out rotation, out scale);
            quaternion rot = rotation;
            quaternion rotInv = math.inverse(rot);
            quaternion res = math.mul(rot, q);
            res = math.mul(res, rotInv);
            return res;
        }
        public static quaternion ToGLTF(this quaternion q)
        {
            float3 translation = new float3();
            float3 scale = new float3();
            float4 rotation = new float4();
            UNITY_GLTF.Decompose(out translation, out rotation, out scale);
            quaternion rot = rotation;
            quaternion rotInv = math.inverse(rot);
            quaternion res = math.mul(rot, q);
            res = math.mul(res, rotInv);
            return res;
        }
        public static float4x4 ToUnity(this float4x4 m)
        {
            float4x4 inv = math.inverse(GLTF_UNITY);
            float4x4 res = new float4x4();
            res = float4x4.identity;
            res = math.mul(GLTF_UNITY, m);
            res = math.mul(res, inv);
            return res;
        }
        public static float4x4 ToGLTF(this float4x4 m)
        {
            float4x4 inv = math.inverse(UNITY_GLTF);
            float4x4 res = new float4x4();
            res = float4x4.identity;
            res = math.mul(UNITY_GLTF, m);
            res = math.mul(res, inv);
            return res;
        }

        #endregion

        #region Standard C#
        public static Vector3 ToUnity(this Vector3 v)
        {
            Vector3 gltf_vector = new Vector3();
            gltf_vector = ((Matrix4x4)GLTF_UNITY).MultiplyPoint(v);
            return gltf_vector;
        }
        public static Vector3 ToGLTF(this Vector3 v)
        {
            Vector3 unity_vector = new Vector3();
            unity_vector = ((Matrix4x4)GLTF_UNITY).MultiplyPoint(v);
            return unity_vector;
        }
        public static Vector3 ToUnityVector(this Vector3 v)
        {
            Vector3 gltf_vector = new Vector3();
            gltf_vector = ((Matrix4x4)GLTF_UNITY).MultiplyVector(v);
            return gltf_vector;
        }
        public static Vector3 ToGLTFVector(this Vector3 v)
        {
            Vector3 unity_vector = new Vector3();
            unity_vector = ((Matrix4x4)GLTF_UNITY).MultiplyVector(v);
            return unity_vector;
        }
        public static Vector4 ToUnity(this Vector4 v)
        {
            Vector4 gltf_vector = new Vector4();
            gltf_vector = ((Matrix4x4)GLTF_UNITY).MultiplyPoint(v);
            return gltf_vector;
        }
        public static Vector4 ToGLTF(this Vector4 v)
        {
            Vector4 unity_vector = new Vector4();
            unity_vector = ((Matrix4x4)GLTF_UNITY).MultiplyPoint(v);
            return unity_vector;
        }
        public static Vector4 ToUnityVector(this Vector4 v)
        {
            Vector4 gltf_vector = new Vector4();
            gltf_vector = ((Matrix4x4)GLTF_UNITY).MultiplyVector(v);
            return gltf_vector;
        }
        public static Vector4 ToGLTFVector(this Vector4 v)
        {
            Vector4 unity_vector = new Vector4();
            unity_vector = ((Matrix4x4)GLTF_UNITY).MultiplyVector(v);
            return unity_vector;
        }
        public static Quaternion ToUnity(this Quaternion r)
        {
            Quaternion q = new Quaternion();
            Vector3 mt;
            Vector3 ms;
            Quaternion mr;
            ((Matrix4x4)GLTF_UNITY).Decompose(out mt, out mr, out ms);
            q = mr * q * Quaternion.Inverse(mr);
            return q;
        }
        public static Quaternion ToGLTF(this Quaternion r)
        {
            Quaternion q = new Quaternion();
            Vector3 mt;
            Vector3 ms;
            Quaternion mr;
            ((Matrix4x4)GLTF_UNITY).Decompose(out mt, out mr, out ms);
            q = mr * q * Quaternion.Inverse(mr);
            return q;
        }
        public static Matrix4x4 ToUnity(this Matrix4x4 m)
        {
            Matrix4x4 res = new Matrix4x4();
            res = Matrix4x4.identity;
            res = ((Matrix4x4)GLTF_UNITY) * m * ((Matrix4x4)GLTF_UNITY).inverse;
            return res;
        }
        public static Matrix4x4 ToGLTF(this Matrix4x4 m)
        {
            Matrix4x4 res = new Matrix4x4();
            res = ((Matrix4x4)UNITY_GLTF) * m * ((Matrix4x4)UNITY_GLTF).inverse;
            return res;
        }
        #endregion


#if false
        public static Vector4 ReverseZ(this Vector4 v)
        {
            return new Vector4(v.x, v.y, -v.z, v.w);
        }

        public static Vector3 ReverseZ(this Vector3 v)
        {
            return new Vector3(v.x, v.y, -v.z);
        }

        [Obsolete]
        public static Vector2 ReverseY(this Vector2 v)
        {
            return new Vector2(v.x, -v.y);
        }

        public static Vector2 ReverseUV(this Vector2 v)
        {
            return new Vector2(v.x, 1.0f - v.y);
        }

        public static Quaternion ReverseZ(this Quaternion q)
        {
            float angle;
            Vector3 axis;
            q.ToAngleAxis(out angle, out axis);
            return Quaternion.AngleAxis(-angle, ReverseZ(axis));
        }

        public static Matrix4x4 Matrix4x4FromColumns(Vector4 c0, Vector4 c1, Vector4 c2, Vector4 c3)
        {
#if UNITY_2017_1_OR_NEWER
            return new Matrix4x4(c0, c1, c2, c3);
#else
            var m = default(Matrix4x4);
            m.SetColumn(0, c0);
            m.SetColumn(1, c1);
            m.SetColumn(2, c2);
            m.SetColumn(3, c3);
            return m;
#endif
        }

        public static Matrix4x4 Matrix4x4FromRotation(Quaternion q)
        {
#if UNITY_2017_1_OR_NEWER
            return Matrix4x4.Rotate(q);
#else
            var m = default(Matrix4x4);
            m.SetTRS(Vector3.zero, q, Vector3.one);
            return m;
#endif
        }

        public static Matrix4x4 ReverseZ(this Matrix4x4 m)
        {
            m.SetTRS(m.ExtractPosition().ReverseZ(), m.ExtractRotation().ReverseZ(), m.ExtractScale());
            return m;
        }

        public static Matrix4x4 MatrixFromArray(float[] values)
        {
            var m = new Matrix4x4();
            m.m00 = values[0];
            m.m10 = values[1];
            m.m20 = values[2];
            m.m30 = values[3];
            m.m01 = values[4];
            m.m11 = values[5];
            m.m21 = values[6];
            m.m31 = values[7];
            m.m02 = values[8];
            m.m12 = values[9];
            m.m22 = values[10];
            m.m32 = values[11];
            m.m03 = values[12];
            m.m13 = values[13];
            m.m23 = values[14];
            m.m33 = values[15];
            return m;
        }

        // https://forum.unity.com/threads/how-to-assign-matrix4x4-to-transform.121966/
        public static Quaternion ExtractRotation(this Matrix4x4 matrix)
        {
            Vector3 forward;
            forward.x = matrix.m02;
            forward.y = matrix.m12;
            forward.z = matrix.m22;

            Vector3 upwards;
            upwards.x = matrix.m01;
            upwards.y = matrix.m11;
            upwards.z = matrix.m21;

            return Quaternion.LookRotation(forward, upwards);
        }

        public static Vector3 ExtractPosition(this Matrix4x4 matrix)
        {
            Vector3 position;
            position.x = matrix.m03;
            position.y = matrix.m13;
            position.z = matrix.m23;
            return position;
        }

        public static Vector3 ExtractScale(this Matrix4x4 matrix)
        {
            Vector3 scale;
            scale.x = new Vector4(matrix.m00, matrix.m10, matrix.m20, matrix.m30).magnitude;
            scale.y = new Vector4(matrix.m01, matrix.m11, matrix.m21, matrix.m31).magnitude;
            scale.z = new Vector4(matrix.m02, matrix.m12, matrix.m22, matrix.m32).magnitude;
            return scale;
        }

        public static string RelativePathFrom(this Transform self, Transform root)
        {
            var path = new List<String>();
            for (var current = self; current != null; current = current.parent)
            {
                if (current == root)
                {
                    return String.Join("/", path.ToArray());
                }

                path.Insert(0, current.name);
            }

            throw new Exception("no RelativePath");
        }

        public static Transform GetChildByName(this Transform self, string childName)
        {
            foreach (Transform child in self)
            {
                if (child.name == childName)
                {
                    return child;
                }
            }

            throw new KeyNotFoundException();
        }

        public static Transform GetFromPath(this Transform self, string path)
        {
            var current = self;

            var splited = path.Split('/');

            foreach (var childName in splited)
            {
                current = current.GetChildByName(childName);
            }

            return current;
        }

        public static IEnumerable<Transform> GetChildren(this Transform self)
        {
            foreach (Transform child in self)
            {
                yield return child;
            }
        }

        public static IEnumerable<Transform> Traverse(this Transform t)
        {
            yield return t;
            foreach (Transform x in t)
            {
                foreach (Transform y in x.Traverse())
                {
                    yield return y;
                }
            }
        }

        public static Transform FindDescenedant(this Transform t, string name)
        {
            return t.Traverse().First(x => x.name == name);
        }

        public static IEnumerable<Transform> Ancestors(this Transform t)
        {
            yield return t;
            if (t.parent != null)
            {
                foreach (Transform x in t.parent.Ancestors())
                {
                    yield return x;
                }
            }
        }

        public static float[] ToArray(this Quaternion q)
        {
            return new float[] { q.x, q.y, q.z, q.w };
        }

        public static float[] ToArray(this Vector3 v)
        {
            return new float[] { v.x, v.y, v.z };
        }

        public static float[] ToArray(this Vector4 v)
        {
            return new float[] { v.x, v.y, v.z, v.w };
        }

        public static float[] ToArray(this Color c)
        {
            return new float[] { c.r, c.g, c.b, c.a };
        }

        public static void ReverseZRecursive(this Transform root)
        {
            var globalMap = root.Traverse().ToDictionary(x => x, x => PosRot.FromGlobalTransform(x));

            foreach (var x in root.Traverse())
            {
                x.position = globalMap[x].Position.ReverseZ();
                x.rotation = globalMap[x].Rotation.ReverseZ();
            }
        }

        public static Mesh GetSharedMesh(this Transform t)
        {
            var meshFilter = t.GetComponent<MeshFilter>();
            if (meshFilter != null)
            {
                return meshFilter.sharedMesh;
            }

            var skinnedMeshRenderer = t.GetComponent<SkinnedMeshRenderer>();
            if (skinnedMeshRenderer != null)
            {
                return skinnedMeshRenderer.sharedMesh;
            }

            return null;
        }

        public static Material[] GetSharedMaterials(this Transform t)
        {
            var renderer = t.GetComponent<Renderer>();
            if (renderer != null)
            {
                return renderer.sharedMaterials;
            }

            return new Material[] { };
        }

        public static bool Has<T>(this Transform transform, T t) where T : Component
        {
            return transform.GetComponent<T>() == t;
        }

        public static T GetOrAddComponent<T>(this GameObject go) where T : Component
        {
            var c = go.GetComponent<T>();
            if (c != null)
            {
                return c;
            }
            return go.AddComponent<T>();
        }
#endif
    }
}
