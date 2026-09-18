// SPDX-FileCopyrightText: 2026 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

#if UNITY_ANIMATION

using System;
using System.Collections.Generic;
using Unity.Cloud.Gltfast.Addons;
using Unity.Cloud.Gltfast.Objects;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Profiling;
using WrapMode = UnityEngine.WrapMode;

namespace Unity.Cloud.Gltfast.Animations
{
    sealed class AnimationModuleProcessor : IAnimationProcessor
    {
        const float k_TimeEpsilon = 0.00001f;

        const int k_TranslationPropertyIndex = 0;
        const int k_ScalePropertyIndex = 1;
        const string k_BlendShapePropertyPrefix = "blendShape.";

        static readonly string[][] k_Vec3PropertyNames = {
            new[] { "localPosition.x", "localPosition.y", "localPosition.z" },
            new[] { "localScale.x", "localScale.y", "localScale.z" },
        };

        static readonly string[] k_RotationPropertyNames =
        {
            "localRotation.x",
            "localRotation.y",
            "localRotation.z",
            "localRotation.w",
        };

#if UNITY_6000_2_OR_NEWER
        readonly NativeArrayPool<Keyframe> m_KeyframesPool = new(4);
#endif

        public AnimationClip[] AnimationClips { get; }

        readonly bool m_Legacy;

        public AnimationModuleProcessor(int clipCount, bool legacy)
        {
            m_Legacy = legacy;
            AnimationClips = new AnimationClip[clipCount];
        }

        public void AddClip(int index, string name)
        {
            AnimationClips[index] = new AnimationClip
            {
                name = name,

                // Legacy Animation requirement
                legacy = m_Legacy,
                wrapMode = WrapMode.Loop
            };
        }

        public void AddTranslationCurves(
            int clipIndex, int targetNode, INodeHierarchyInfo nodeHierarchyInfo, NativeArray<float>.ReadOnly times,
            NativeArray<float3>.ReadOnly values, Interpolation interpolation)
        {
            AddTranslationCurves(
                AnimationClips[clipIndex],
                targetNode,
                null,
                nodeHierarchyInfo,
                times,
                values,
                interpolation
            );
        }

        public void AddRotationCurves(
            int clipIndex, int targetNode, INodeHierarchyInfo nodeHierarchyInfo, NativeArray<float>.ReadOnly times,
            NativeArray<quaternion>.ReadOnly values, Interpolation interpolation)
        {
            AddRotationCurves(
                AnimationClips[clipIndex],
                targetNode,
                null,
                nodeHierarchyInfo,
                times,
                values,
                interpolation
            );
        }

        public void AddScaleCurves(
            int clipIndex, int targetNode, INodeHierarchyInfo nodeHierarchyInfo, NativeArray<float>.ReadOnly times,
            NativeArray<float3>.ReadOnly values, Interpolation interpolation)
        {
            AddScaleCurves(
                AnimationClips[clipIndex],
                targetNode,
                null,
                nodeHierarchyInfo,
                times,
                values,
                interpolation
            );
        }

        public void AddMorphTargetWeightCurves(
            int clipIndex,
            int targetNode,
            int meshNumeration,
            string meshName,
            INodeHierarchyInfo nodeHierarchyInfo,
            NativeArray<float>.ReadOnly times,
            NativeArray<float>.ReadOnly values,
            Interpolation interpolation,
            IReadOnlyList<string> morphTargetNames = null
            )
        {
            AddMorphTargetWeightCurves(
                AnimationClips[clipIndex],
                targetNode,
                meshName,
                nodeHierarchyInfo,
                times,
                values,
                interpolation,
                morphTargetNames
            );
        }

        public IDataInstanceApplierFactory Complete()
        {
            return AnimationClips is { Length: > 0 }
                ? new AnimationModuleDataInstanceApplierFactory(AnimationClips)
                : null;
        }

        public void Dispose()
        {
#if UNITY_6000_2_OR_NEWER
            m_KeyframesPool.Dispose();
#endif
        }

#if !UNITY_6000_2_OR_NEWER
        static
#endif
        void AddTranslationCurves(
            AnimationClip clip,
            int targetNode,
            string subPath,
            INodeHierarchyInfo nodeHierarchyInfo,
            NativeArray<float>.ReadOnly times,
            NativeArray<float3>.ReadOnly values,
            Interpolation interpolation
            )
        {
            var animationPath = AnimationUtils.CreateAnimationPath(targetNode, nodeHierarchyInfo, subPath);
            if (values.IsCreated)
            {
#if UNITY_6000_2_OR_NEWER
                AddVec3Curves(
                    clip, animationPath, k_TranslationPropertyIndex, times, values, interpolation, m_KeyframesPool);
#else
                AddVec3Curves(
                    clip, animationPath, k_TranslationPropertyIndex, times, values, interpolation);
#endif
            }
            else
            {
                AddVec3Curves(
                    clip, animationPath, k_TranslationPropertyIndex, times, interpolation);
            }
        }

#if !UNITY_6000_2_OR_NEWER
        static
#endif
        void AddRotationCurves(
            AnimationClip clip,
            int targetNode,
            string subPath,
            INodeHierarchyInfo nodeHierarchyInfo,
            NativeArray<float>.ReadOnly times,
            NativeArray<quaternion>.ReadOnly values,
            Interpolation interpolation
        )
        {
            var animationPath = AnimationUtils.CreateAnimationPath(targetNode, nodeHierarchyInfo, subPath);
            if (values.IsCreated)
            {
#if UNITY_6000_2_OR_NEWER
                AddQuaternionCurves(clip, animationPath, times, values, interpolation, m_KeyframesPool);
#else
                AddQuaternionCurves(clip, animationPath, times, values, interpolation);
#endif
            }
            else
            {
                AddQuaternionCurves(clip, animationPath, times, interpolation);
            }
        }

#if !UNITY_6000_2_OR_NEWER
        static
#endif
        void AddScaleCurves(
            AnimationClip clip,
            int targetNode,
            string subPath,
            INodeHierarchyInfo nodeHierarchyInfo,
            NativeArray<float>.ReadOnly times,
            NativeArray<float3>.ReadOnly values,
            Interpolation interpolation
            )
        {
            var animationPath = AnimationUtils.CreateAnimationPath(targetNode, nodeHierarchyInfo, subPath);
            if (values.IsCreated)
            {
#if UNITY_6000_2_OR_NEWER
                AddVec3Curves(
                    clip, animationPath, k_ScalePropertyIndex, times, values, interpolation, m_KeyframesPool);
#else
                AddVec3Curves(
                    clip, animationPath, k_ScalePropertyIndex, times, values, interpolation);
#endif
            }
            else
            {
                AddVec3Curves(clip, animationPath, k_ScalePropertyIndex, times, interpolation);
            }
        }

        static void AddMorphTargetWeightCurves(
            AnimationClip clip,
            int targetNode,
            string subPath,
            INodeHierarchyInfo nodeHierarchyInfo,
            NativeArray<float>.ReadOnly times,
            NativeArray<float>.ReadOnly values,
            Interpolation interpolation,
            IReadOnlyList<string> morphTargetNames = null
            )
        {
            Profiler.BeginSample("AnimationModuleLoader.AddMorphTargetWeightCurves");
            int morphTargetCount;
            if (morphTargetNames == null)
            {
                morphTargetCount = values.Length / times.Length;
                if (interpolation == Interpolation.CubicSpline)
                {
                    // 3 values per key (in-tangent, out-tangent and value)
                    morphTargetCount /= 3;
                }
            }
            else
            {
                morphTargetCount = morphTargetNames.Count;
            }

            var animationPath = AnimationUtils.CreateAnimationPath(targetNode, nodeHierarchyInfo, subPath);

            if (values.IsCreated)
            {
                for (var i = 0; i < morphTargetCount; i++)
                {
                    var morphTargetName = morphTargetNames == null ? i.ToString() : morphTargetNames[i];
                    AddScalarCurve(
                        clip,
                        animationPath,
                        string.Concat(k_BlendShapePropertyPrefix, morphTargetName),
                        i,
                        morphTargetCount,
                        times,
                        values,
                        interpolation
                        );
                }
            }
            else
            {
                for (var i = 0; i < morphTargetCount; i++)
                {
                    var morphTargetName = morphTargetNames == null ? i.ToString() : morphTargetNames[i];
                    AddScalarCurve(
                        clip,
                        animationPath,
                        string.Concat(k_BlendShapePropertyPrefix, morphTargetName),
                        times,
                        interpolation
                    );
                }
            }
            Profiler.EndSample();
        }

        static void AddQuaternionCurves(
            AnimationClip clip,
            string animationPath,
            NativeArray<float>.ReadOnly times,
            Interpolation interpolation
            )
        {
            Profiler.BeginSample("AnimationModuleLoader.AddQuaternionCurves");
            var rotX = new AnimationCurve();
            var rotY = new AnimationCurve();
            var rotZ = new AnimationCurve();
            var rotW = new AnimationCurve();

#if DEBUG
            uint duplicates = 0;
#endif

            switch (interpolation)
            {
                case Interpolation.Step:
                case Interpolation.CubicSpline:
                {
                    foreach (var time in times)
                    {
                        rotX.AddKey(new Keyframe(time, 0));
                        rotY.AddKey(new Keyframe(time, 0));
                        rotZ.AddKey(new Keyframe(time, 0));
                        rotW.AddKey(new Keyframe(time, 0));
                    }

                    break;
                }
                case Interpolation.Linear:
                default:
                {
                    var prevTime = times[0];

                    for (var i = 1; i < times.Length; i++)
                    {
                        var time = times[i];

                        if (prevTime >= time)
                        {
                            // Time value is not increasing, so we ignore this keyframe
                            // This happened on some Sketchfab files (see #298)
#if DEBUG
                            duplicates++;
#endif
                            continue;
                        }

                        rotX.AddKey(new Keyframe(prevTime, 0));
                        rotY.AddKey(new Keyframe(prevTime, 0));
                        rotZ.AddKey(new Keyframe(prevTime, 0));
                        rotW.AddKey(new Keyframe(prevTime, 0));

                        prevTime = time;
                    }

                    rotX.AddKey(new Keyframe(prevTime, 0));
                    rotY.AddKey(new Keyframe(prevTime, 0));
                    rotZ.AddKey(new Keyframe(prevTime, 0));
                    rotW.AddKey(new Keyframe(prevTime, 0));

                    break;
                }
            }

            clip.SetCurve(animationPath, typeof(Transform), k_RotationPropertyNames[0], rotX);
            clip.SetCurve(animationPath, typeof(Transform), k_RotationPropertyNames[1], rotY);
            clip.SetCurve(animationPath, typeof(Transform), k_RotationPropertyNames[2], rotZ);
            clip.SetCurve(animationPath, typeof(Transform), k_RotationPropertyNames[3], rotW);
            Profiler.EndSample();

#if DEBUG
            if (duplicates > 0)
            {
                ReportDuplicateKeyframes();
            }
#endif
        }

#if UNITY_6000_2_OR_NEWER
        static void AddQuaternionCurves(
            AnimationClip clip,
            string animationPath,
            NativeArray<float>.ReadOnly times,
            NativeArray<quaternion>.ReadOnly values,
            Interpolation interpolation,
            NativeArrayPool<Keyframe> keyframeArrayPool
        )
        {
            Profiler.BeginSample("AnimationModuleLoader.AddQuaternionCurves");
            keyframeArrayPool.ReserveBuffers(times.Length, 4);
            var keyframesX = keyframeArrayPool.GetBuffer(0);
            var keyframesY = keyframeArrayPool.GetBuffer(1);
            var keyframesZ = keyframeArrayPool.GetBuffer(2);
            var keyframesW = keyframeArrayPool.GetBuffer(3);
            var count = 0;

#if DEBUG
            uint duplicates = 0;
#endif

            switch (interpolation)
            {
                case Interpolation.Step:
                {
                    for (var i = 0; i < times.Length; i++)
                    {
                        var time = times[i];
                        var value = values[i];
                        keyframesX[i] = new Keyframe(time, value.value.x, float.PositiveInfinity, 0);
                        keyframesY[i] = new Keyframe(time, value.value.y, float.PositiveInfinity, 0);
                        keyframesZ[i] = new Keyframe(time, value.value.z, float.PositiveInfinity, 0);
                        keyframesW[i] = new Keyframe(time, value.value.w, float.PositiveInfinity, 0);
                    }

                    count = times.Length;
                    break;
                }
                case Interpolation.CubicSpline:
                {
                    for (var i = 0; i < times.Length; i++)
                    {
                        var time = times[i];
                        var inTangent = values[i * 3];
                        var value = values[i * 3 + 1];
                        var outTangent = values[i * 3 + 2];
                        keyframesX[i] = new Keyframe(time, value.value.x, inTangent.value.x, outTangent.value.x, .5f,
                            .5f);
                        keyframesY[i] = new Keyframe(time, value.value.y, inTangent.value.y, outTangent.value.y, .5f,
                            .5f);
                        keyframesZ[i] = new Keyframe(time, value.value.z, inTangent.value.z, outTangent.value.z, .5f,
                            .5f);
                        keyframesW[i] = new Keyframe(time, value.value.w, inTangent.value.w, outTangent.value.w, .5f,
                            .5f);
                    }

                    count = times.Length;
                    break;
                }
                default:
                {
                    // LINEAR
                    var prevTime = times[0];
                    var prevValue = values[0];
                    var inTangent = new quaternion(new float4(0f));

                    Assert.AreEqual(times.Length, values.Length);
                    for (var i = 1; i < times.Length; i++)
                    {
                        var time = times[i];
                        var value = values[i];

                        if (prevTime >= time)
                        {
                            // Time value is not increasing, so we ignore this keyframe
                            // This happened on some Sketchfab files (see #298)
#if DEBUG
                            duplicates++;
#endif
                            continue;
                        }

                        // Ensure shortest path rotation ( see https://www.khronos.org/registry/glTF/specs/2.0/glTF-2.0.html#interpolation-slerp )
                        if (math.dot(prevValue, value) < 0)
                        {
                            value.value = -value.value;
                        }

                        var dT = time - prevTime;
                        var dV = value.value - prevValue.value;
                        quaternion outTangent;
                        if (dT < k_TimeEpsilon)
                        {
                            outTangent.value.x =
                                (dV.x < 0f) ^ (dT < 0f) ? float.NegativeInfinity : float.PositiveInfinity;
                            outTangent.value.y =
                                (dV.y < 0f) ^ (dT < 0f) ? float.NegativeInfinity : float.PositiveInfinity;
                            outTangent.value.z =
                                (dV.z < 0f) ^ (dT < 0f) ? float.NegativeInfinity : float.PositiveInfinity;
                            outTangent.value.w =
                                (dV.w < 0f) ^ (dT < 0f) ? float.NegativeInfinity : float.PositiveInfinity;
                        }
                        else
                        {
                            outTangent = dV / dT;
                        }

                        keyframesX[count] = new Keyframe(prevTime, prevValue.value.x, inTangent.value.x,
                            outTangent.value.x);
                        keyframesY[count] = new Keyframe(prevTime, prevValue.value.y, inTangent.value.y,
                            outTangent.value.y);
                        keyframesZ[count] = new Keyframe(prevTime, prevValue.value.z, inTangent.value.z,
                            outTangent.value.z);
                        keyframesW[count] = new Keyframe(prevTime, prevValue.value.w, inTangent.value.w,
                            outTangent.value.w);
                        count++;

                        inTangent = outTangent;
                        prevTime = time;
                        prevValue = value;
                    }

                    keyframesX[count] = new Keyframe(prevTime, prevValue.value.x, inTangent.value.x, 0);
                    keyframesY[count] = new Keyframe(prevTime, prevValue.value.y, inTangent.value.y, 0);
                    keyframesZ[count] = new Keyframe(prevTime, prevValue.value.z, inTangent.value.z, 0);
                    keyframesW[count] = new Keyframe(prevTime, prevValue.value.w, inTangent.value.w, 0);
                    count++;
                    break;
                }
            }

            var rotX = new AnimationCurve();
            var rotY = new AnimationCurve();
            var rotZ = new AnimationCurve();
            var rotW = new AnimationCurve();
            rotX.SetKeys(keyframesX.AsReadOnlySpan()[..count]);
            rotY.SetKeys(keyframesY.AsReadOnlySpan()[..count]);
            rotZ.SetKeys(keyframesZ.AsReadOnlySpan()[..count]);
            rotW.SetKeys(keyframesW.AsReadOnlySpan()[..count]);

            clip.SetCurve(animationPath, typeof(Transform), k_RotationPropertyNames[0], rotX);
            clip.SetCurve(animationPath, typeof(Transform), k_RotationPropertyNames[1], rotY);
            clip.SetCurve(animationPath, typeof(Transform), k_RotationPropertyNames[2], rotZ);
            clip.SetCurve(animationPath, typeof(Transform), k_RotationPropertyNames[3], rotW);

            Profiler.EndSample();
#if DEBUG
            if (duplicates > 0)
            {
                ReportDuplicateKeyframes();
            }
#endif
        }

#else // UNITY_6000_2_OR_NEWER

        static void AddQuaternionCurves(
            AnimationClip clip,
            string animationPath,
            NativeArray<float>.ReadOnly times,
            NativeArray<quaternion>.ReadOnly values,
            Interpolation interpolation
            )
        {
            Profiler.BeginSample("AnimationModuleLoader.AddQuaternionCurves");
            var rotX = new AnimationCurve();
            var rotY = new AnimationCurve();
            var rotZ = new AnimationCurve();
            var rotW = new AnimationCurve();

#if DEBUG
            uint duplicates = 0;
#endif

            switch (interpolation)
            {
                case Interpolation.Step:
                {
                    for (var i = 0; i < times.Length; i++)
                    {
                        var time = times[i];
                        var value = values[i];
                        rotX.AddKey(new Keyframe(time, value.value.x, float.PositiveInfinity, 0));
                        rotY.AddKey(new Keyframe(time, value.value.y, float.PositiveInfinity, 0));
                        rotZ.AddKey(new Keyframe(time, value.value.z, float.PositiveInfinity, 0));
                        rotW.AddKey(new Keyframe(time, value.value.w, float.PositiveInfinity, 0));
                    }
                    break;
                }
                case Interpolation.CubicSpline:
                {
                    for (var i = 0; i < times.Length; i++)
                    {
                        var time = times[i];
                        var inTangent = values[i * 3];
                        var value = values[i * 3 + 1];
                        var outTangent = values[i * 3 + 2];
                        rotX.AddKey(new Keyframe(time, value.value.x, inTangent.value.x, outTangent.value.x, .5f, .5f));
                        rotY.AddKey(new Keyframe(time, value.value.y, inTangent.value.y, outTangent.value.y, .5f, .5f));
                        rotZ.AddKey(new Keyframe(time, value.value.z, inTangent.value.z, outTangent.value.z, .5f, .5f));
                        rotW.AddKey(new Keyframe(time, value.value.w, inTangent.value.w, outTangent.value.w, .5f, .5f));
                    }
                    break;
                }
                default:
                {
                    // LINEAR
                    var prevTime = times[0];
                    var prevValue = values[0];
                    var inTangent = new quaternion(new float4(0f));

                    Assert.AreEqual(times.Length, values.Length);
                    for (var i = 1; i < times.Length; i++)
                    {
                        var time = times[i];
                        var value = values[i];

                        if (prevTime >= time)
                        {
                            // Time value is not increasing, so we ignore this keyframe
                            // This happened on some Sketchfab files (see #298)
#if DEBUG
                            duplicates++;
#endif
                            continue;
                        }

                        // Ensure shortest path rotation ( see https://www.khronos.org/registry/glTF/specs/2.0/glTF-2.0.html#interpolation-slerp )
                        if (math.dot(prevValue, value) < 0)
                        {
                            value.value = -value.value;
                        }

                        var dT = time - prevTime;
                        var dV = value.value - prevValue.value;
                        quaternion outTangent;
                        if (dT < k_TimeEpsilon)
                        {
                            outTangent.value.x = (dV.x < 0f) ^ (dT < 0f) ? float.NegativeInfinity : float.PositiveInfinity;
                            outTangent.value.y = (dV.y < 0f) ^ (dT < 0f) ? float.NegativeInfinity : float.PositiveInfinity;
                            outTangent.value.z = (dV.z < 0f) ^ (dT < 0f) ? float.NegativeInfinity : float.PositiveInfinity;
                            outTangent.value.w = (dV.w < 0f) ^ (dT < 0f) ? float.NegativeInfinity : float.PositiveInfinity;
                        }
                        else
                        {
                            outTangent = dV / dT;
                        }

                        rotX.AddKey(new Keyframe(prevTime, prevValue.value.x, inTangent.value.x, outTangent.value.x));
                        rotY.AddKey(new Keyframe(prevTime, prevValue.value.y, inTangent.value.y, outTangent.value.y));
                        rotZ.AddKey(new Keyframe(prevTime, prevValue.value.z, inTangent.value.z, outTangent.value.z));
                        rotW.AddKey(new Keyframe(prevTime, prevValue.value.w, inTangent.value.w, outTangent.value.w));

                        inTangent = outTangent;
                        prevTime = time;
                        prevValue = value;
                    }

                    rotX.AddKey(new Keyframe(prevTime, prevValue.value.x, inTangent.value.x, 0));
                    rotY.AddKey(new Keyframe(prevTime, prevValue.value.y, inTangent.value.y, 0));
                    rotZ.AddKey(new Keyframe(prevTime, prevValue.value.z, inTangent.value.z, 0));
                    rotW.AddKey(new Keyframe(prevTime, prevValue.value.w, inTangent.value.w, 0));

                    break;
                }
            }

            clip.SetCurve(animationPath, typeof(Transform), k_RotationPropertyNames[0], rotX);
            clip.SetCurve(animationPath, typeof(Transform), k_RotationPropertyNames[1], rotY);
            clip.SetCurve(animationPath, typeof(Transform), k_RotationPropertyNames[2], rotZ);
            clip.SetCurve(animationPath, typeof(Transform), k_RotationPropertyNames[3], rotW);
            Profiler.EndSample();

#if DEBUG
            if (duplicates > 0)
            {
                ReportDuplicateKeyframes();
            }
#endif
        }
#endif // UNITY_6000_2_OR_NEWER

        static void AddVec3Curves(
            AnimationClip clip,
            string animationPath,
            int propertyIndex,
            NativeArray<float>.ReadOnly times,
            Interpolation interpolation
            )
        {
            Profiler.BeginSample("AnimationModuleLoader.AddVec3Curves");
            var curveX = new AnimationCurve();
            var curveY = new AnimationCurve();
            var curveZ = new AnimationCurve();

#if DEBUG
            var duplicates = 0u;
#endif

            switch (interpolation)
            {
                case Interpolation.Step:
                {
                    foreach (var time in times)
                    {
                        curveX.AddKey(new Keyframe(time, 0, float.PositiveInfinity, 0));
                        curveY.AddKey(new Keyframe(time, 0, float.PositiveInfinity, 0));
                        curveZ.AddKey(new Keyframe(time, 0, float.PositiveInfinity, 0));
                    }

                    break;
                }
                case Interpolation.CubicSpline:
                {
                    foreach (var time in times)
                    {
                        curveX.AddKey(new Keyframe(time, 0, 0, 0, .5f, .5f));
                        curveY.AddKey(new Keyframe(time, 0, 0, 0, .5f, .5f));
                        curveZ.AddKey(new Keyframe(time, 0, 0, 0, .5f, .5f));
                    }

                    break;
                }
                case Interpolation.Linear:
                default:
                {
                    var prevTime = times[0];

                    for (var i = 1; i < times.Length; i++)
                    {
                        var time = times[i];

                        if (prevTime >= time)
                        {
                            // Time value is not increasing, so we ignore this keyframe
                            // This happened on some Sketchfab files (see #298)
#if DEBUG
                            duplicates++;
#endif
                            continue;
                        }

                        curveX.AddKey(new Keyframe(prevTime, 0, 0, 0));
                        curveY.AddKey(new Keyframe(prevTime, 0, 0, 0));
                        curveZ.AddKey(new Keyframe(prevTime, 0, 0, 0));

                        prevTime = time;
                    }

                    curveX.AddKey(new Keyframe(prevTime, 0, 0, 0));
                    curveY.AddKey(new Keyframe(prevTime, 0, 0, 0));
                    curveZ.AddKey(new Keyframe(prevTime, 0, 0, 0));

                    break;
                }
            }

            var propNames = k_Vec3PropertyNames[propertyIndex];
            clip.SetCurve(animationPath, typeof(Transform), propNames[0], curveX);
            clip.SetCurve(animationPath, typeof(Transform), propNames[1], curveY);
            clip.SetCurve(animationPath, typeof(Transform), propNames[2], curveZ);
            Profiler.EndSample();
#if DEBUG
            if (duplicates > 0)
            {
                ReportDuplicateKeyframes();
            }
#endif
        }

#if UNITY_6000_2_OR_NEWER
        static void AddVec3Curves(
            AnimationClip clip,
            string animationPath,
            int propertyIndex,
            NativeArray<float>.ReadOnly times,
            NativeArray<float3>.ReadOnly values,
            Interpolation interpolation,
            NativeArrayPool<Keyframe> keyframeArrayPool
        )
        {
            Profiler.BeginSample("AnimationModuleLoader.AddVec3Curves");
            keyframeArrayPool.ReserveBuffers(times.Length, 3);
            var keyframesX = keyframeArrayPool.GetBuffer(0);
            var keyframesY = keyframeArrayPool.GetBuffer(1);
            var keyframesZ = keyframeArrayPool.GetBuffer(2);
            var count = 0;

#if DEBUG
            uint duplicates = 0;
#endif

            Profiler.BeginSample("AnimationModuleLoader.AddVec3Curves.PopulateBuffers");
            switch (interpolation)
            {
                case Interpolation.Step:
                {
                    for (var i = 0; i < times.Length; i++)
                    {
                        var time = times[i];
                        var value = values[i];
                        keyframesX[i] = new Keyframe(time, value.x, float.PositiveInfinity, 0);
                        keyframesY[i] = new Keyframe(time, value.y, float.PositiveInfinity, 0);
                        keyframesZ[i] = new Keyframe(time, value.z, float.PositiveInfinity, 0);
                    }

                    count = times.Length;
                    break;
                }
                case Interpolation.CubicSpline:
                {
                    for (var i = 0; i < times.Length; i++)
                    {
                        var time = times[i];
                        var inTangent = values[i * 3];
                        var value = values[i * 3 + 1];
                        var outTangent = values[i * 3 + 2];
                        keyframesX[i] = new Keyframe(time, value.x, inTangent.x, outTangent.x, .5f, .5f);
                        keyframesY[i] = new Keyframe(time, value.y, inTangent.y, outTangent.y, .5f, .5f);
                        keyframesZ[i] = new Keyframe(time, value.z, inTangent.z, outTangent.z, .5f, .5f);
                    }

                    count = times.Length;
                    break;
                }
                default:
                {
                    // LINEAR
                    var prevTime = times[0];
                    var prevValue = values[0];
                    var inTangent = new float3(0f);

                    for (var i = 1; i < times.Length; i++)
                    {
                        var time = times[i];
                        var value = values[i];

                        if (prevTime >= time)
                        {
                            // Time value is not increasing, so we ignore this keyframe
                            // This happened on some Sketchfab files (see #298)
#if DEBUG
                            duplicates++;
#endif
                            continue;
                        }

                        var dT = time - prevTime;
                        var dV = value - prevValue;
                        float3 outTangent;
                        if (dT < k_TimeEpsilon)
                        {
                            outTangent.x = (dV.x < 0f) ^ (dT < 0f) ? float.NegativeInfinity : float.PositiveInfinity;
                            outTangent.y = (dV.y < 0f) ^ (dT < 0f) ? float.NegativeInfinity : float.PositiveInfinity;
                            outTangent.z = (dV.z < 0f) ^ (dT < 0f) ? float.NegativeInfinity : float.PositiveInfinity;
                        }
                        else
                        {
                            outTangent = dV / dT;
                        }

                        keyframesX[count] = new Keyframe(prevTime, prevValue.x, inTangent.x, outTangent.x);
                        keyframesY[count] = new Keyframe(prevTime, prevValue.y, inTangent.y, outTangent.y);
                        keyframesZ[count] = new Keyframe(prevTime, prevValue.z, inTangent.z, outTangent.z);
                        count++;

                        inTangent = outTangent;
                        prevTime = time;
                        prevValue = value;
                    }

                    keyframesX[count] = new Keyframe(prevTime, prevValue.x, inTangent.x, 0);
                    keyframesY[count] = new Keyframe(prevTime, prevValue.y, inTangent.y, 0);
                    keyframesZ[count] = new Keyframe(prevTime, prevValue.z, inTangent.z, 0);
                    count++;
                    break;
                }
            }

            Profiler.EndSample();

            var curveX = new AnimationCurve();
            var curveY = new AnimationCurve();
            var curveZ = new AnimationCurve();
            curveX.SetKeys(keyframesX.AsReadOnlySpan()[..count]);
            curveY.SetKeys(keyframesY.AsReadOnlySpan()[..count]);
            curveZ.SetKeys(keyframesZ.AsReadOnlySpan()[..count]);

            var propNames = k_Vec3PropertyNames[propertyIndex];
            clip.SetCurve(animationPath, typeof(Transform), propNames[0], curveX);
            clip.SetCurve(animationPath, typeof(Transform), propNames[1], curveY);
            clip.SetCurve(animationPath, typeof(Transform), propNames[2], curveZ);

            Profiler.EndSample();
#if DEBUG
            if (duplicates > 0)
            {
                ReportDuplicateKeyframes();
            }
#endif
        }

#else // UNITY_6000_2_OR_NEWER

        static void AddVec3Curves(
            AnimationClip clip,
            string animationPath,
            int propertyIndex,
            NativeArray<float>.ReadOnly times,
            NativeArray<float3>.ReadOnly values,
            Interpolation interpolation
            )
        {
            Profiler.BeginSample("AnimationModuleLoader.AddVec3Curves");
            var curveX = new AnimationCurve();
            var curveY = new AnimationCurve();
            var curveZ = new AnimationCurve();

#if DEBUG
            uint duplicates = 0;
#endif

            switch (interpolation)
            {
                case Interpolation.Step:
                {
                    for (var i = 0; i < times.Length; i++)
                    {
                        var time = times[i];
                        var value = values[i];
                        curveX.AddKey(new Keyframe(time, value.x, float.PositiveInfinity, 0));
                        curveY.AddKey(new Keyframe(time, value.y, float.PositiveInfinity, 0));
                        curveZ.AddKey(new Keyframe(time, value.z, float.PositiveInfinity, 0));
                    }
                    break;
                }
                case Interpolation.CubicSpline:
                {
                    for (var i = 0; i < times.Length; i++)
                    {
                        var time = times[i];
                        var inTangent = values[i * 3];
                        var value = values[i * 3 + 1];
                        var outTangent = values[i * 3 + 2];
                        curveX.AddKey(new Keyframe(time, value.x, inTangent.x, outTangent.x, .5f, .5f));
                        curveY.AddKey(new Keyframe(time, value.y, inTangent.y, outTangent.y, .5f, .5f));
                        curveZ.AddKey(new Keyframe(time, value.z, inTangent.z, outTangent.z, .5f, .5f));
                    }
                    break;
                }
                default:
                { // LINEAR
                    var prevTime = times[0];
                    var prevValue = values[0];
                    var inTangent = new float3(0f);

                    for (var i = 1; i < times.Length; i++)
                    {
                        var time = times[i];
                        var value = values[i];

                        if (prevTime >= time)
                        {
                            // Time value is not increasing, so we ignore this keyframe
                            // This happened on some Sketchfab files (see #298)
#if DEBUG
                            duplicates++;
#endif
                            continue;
                        }

                        var dT = time - prevTime;
                        var dV = value - prevValue;
                        float3 outTangent;
                        if (dT < k_TimeEpsilon)
                        {
                            outTangent.x = (dV.x < 0f) ^ (dT < 0f) ? float.NegativeInfinity : float.PositiveInfinity;
                            outTangent.y = (dV.y < 0f) ^ (dT < 0f) ? float.NegativeInfinity : float.PositiveInfinity;
                            outTangent.z = (dV.z < 0f) ^ (dT < 0f) ? float.NegativeInfinity : float.PositiveInfinity;
                        }
                        else
                        {
                            outTangent = dV / dT;
                        }

                        curveX.AddKey(new Keyframe(prevTime, prevValue.x, inTangent.x, outTangent.x));
                        curveY.AddKey(new Keyframe(prevTime, prevValue.y, inTangent.y, outTangent.y));
                        curveZ.AddKey(new Keyframe(prevTime, prevValue.z, inTangent.z, outTangent.z));

                        inTangent = outTangent;
                        prevTime = time;
                        prevValue = value;
                    }

                    curveX.AddKey(new Keyframe(prevTime, prevValue.x, inTangent.x, 0));
                    curveY.AddKey(new Keyframe(prevTime, prevValue.y, inTangent.y, 0));
                    curveZ.AddKey(new Keyframe(prevTime, prevValue.z, inTangent.z, 0));

                    break;
                }
            }

            var propNames = k_Vec3PropertyNames[propertyIndex];
            clip.SetCurve(animationPath, typeof(Transform), propNames[0], curveX);
            clip.SetCurve(animationPath, typeof(Transform), propNames[1], curveY);
            clip.SetCurve(animationPath, typeof(Transform), propNames[2], curveZ);
            Profiler.EndSample();
#if DEBUG
            if (duplicates > 0)
            {
                ReportDuplicateKeyframes();
            }
#endif
        }
#endif // UNITY_6000_2_OR_NEWER

        static void AddScalarCurve(
            AnimationClip clip,
            string animationPath,
            string propertyName,
            NativeArray<float>.ReadOnly times,
            Interpolation interpolation
            )
        {
            Profiler.BeginSample("AnimationModuleLoader.AddScalarCurve");
            var curve = new AnimationCurve();

#if DEBUG
            uint duplicates = 0;
#endif

            switch (interpolation)
            {
                case Interpolation.Step:
                case Interpolation.CubicSpline:
                {
                    foreach (var time in times)
                    {
                        curve.AddKey(new Keyframe(time, 0));
                    }

                    break;
                }
                case Interpolation.Linear:
                default:
                {
                    var prevTime = times[0];

                    for (var i = 1; i < times.Length; i++)
                    {
                        var time = times[i];

                        if (prevTime >= time)
                        {
                            // Time value is not increasing, so we ignore this keyframe
                            // This happened on some Sketchfab files (see #298)
#if DEBUG
                            duplicates++;
#endif
                            continue;
                        }

                        curve.AddKey(new Keyframe(prevTime, 0));
                        prevTime = time;
                    }

                    curve.AddKey(new Keyframe(prevTime, 0));
                    break;
                }
            }

            clip.SetCurve(animationPath, typeof(SkinnedMeshRenderer), propertyName, curve);
            Profiler.EndSample();
#if DEBUG
            if (duplicates > 0)
            {
                ReportDuplicateKeyframes();
            }
#endif
        }

        static void AddScalarCurve(
            AnimationClip clip,
            string animationPath,
            string propertyName,
            int curveIndex,
            int valueStride,
            NativeArray<float>.ReadOnly times,
            NativeArray<float>.ReadOnly values,
            Interpolation interpolation
            )
        {
            Profiler.BeginSample("AnimationModuleLoader.AddScalarCurve");
            var curve = new AnimationCurve();

#if DEBUG
            uint duplicates = 0;
#endif

            switch (interpolation)
            {
                case Interpolation.Step:
                {
                    for (var i = 0; i < times.Length; i++)
                    {
                        var time = times[i];
                        var valueIndex = i * valueStride + curveIndex;
                        var value = values[valueIndex];
                        curve.AddKey(new Keyframe(time, value, float.PositiveInfinity, 0));
                    }
                    break;
                }
                case Interpolation.CubicSpline:
                {
                    for (var i = 0; i < times.Length; i++)
                    {
                        var time = times[i];
                        var valueIndex = i * valueStride + curveIndex;
                        var inTangent = values[valueIndex * 3];
                        var value = values[valueIndex * 3 + 1];
                        var outTangent = values[valueIndex * 3 + 2];
                        curve.AddKey(new Keyframe(time, value, inTangent, outTangent, .5f, .5f));
                    }
                    break;
                }
                default:
                { // LINEAR
                    var prevTime = times[0];
                    var prevValue = values[curveIndex];
                    var inTangent = 0f;

                    for (var i = 1; i < times.Length; i++)
                    {
                        var time = times[i];
                        var valueIndex = i * valueStride + curveIndex;
                        var value = values[valueIndex];

                        if (prevTime >= time)
                        {
                            // Time value is not increasing, so we ignore this keyframe
                            // This happened on some Sketchfab files (see #298)
#if DEBUG
                            duplicates++;
#endif
                            continue;
                        }

                        var dT = time - prevTime;
                        var dV = value - prevValue;
                        float outTangent;
                        if (dT < k_TimeEpsilon)
                        {
                            outTangent = (dV < 0f) ^ (dT < 0f) ? float.NegativeInfinity : float.PositiveInfinity;
                        }
                        else
                        {
                            outTangent = dV / dT;
                        }

                        curve.AddKey(new Keyframe(prevTime, prevValue, inTangent, outTangent));

                        inTangent = outTangent;
                        prevTime = time;
                        prevValue = value;
                    }

                    curve.AddKey(new Keyframe(prevTime, prevValue, inTangent, 0));

                    break;
                }
            }

            clip.SetCurve(animationPath, typeof(SkinnedMeshRenderer), propertyName, curve);
            Profiler.EndSample();
#if DEBUG
            if (duplicates > 0)
            {
                ReportDuplicateKeyframes();
            }
#endif
        }

#if DEBUG
        static void ReportDuplicateKeyframes()
        {
            Debug.LogError("Time of subsequent animation keyframes is not increasing (glTF-Validator error ACCESSOR_ANIMATION_INPUT_NON_INCREASING)");
        }
#endif
    }
}
#endif // UNITY_ANIMATION
