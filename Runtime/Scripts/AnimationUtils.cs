// Copyright 2020-2022 Andreas Atteneder
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


#if UNITY_ANIMATION

using System;
using System.Text;
using GLTFast.Schema;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;

namespace GLTFast {

    static class AnimationUtils {

        const float k_TimeEpsilon = 0.00001f;

        public static void AddCurve(AnimationClip clip, string animationPath, AnimationPointerData data, NativeArray<float> times, AccessorDataBase values, InterpolationType interpolationType) {
            switch(data.accessorType) {
                case GltfAccessorAttributeType.SCALAR:
                    var scalarVals = ((AccessorNativeData<float>)values).data;
                    AddScalarCurve(clip, animationPath, data.AnimationProperties[0], data.AnimationTargetType, times, scalarVals, interpolationType);
                    break;
                case GltfAccessorAttributeType.VEC2:
                    var float2Vals = ((AccessorNativeData<Vector2>)values).data.Reinterpret<float2>();
                    AddVec2Curves(clip, animationPath, data.AnimationProperties, data.AnimationTargetType, times, float2Vals, interpolationType);
                    break;
                case GltfAccessorAttributeType.VEC3:
                    var float3Vals = ((AccessorNativeData<Vector3>)values).data.Reinterpret<float3>();
                    AddVec3Curves(clip, animationPath, data.AnimationProperties, data.AnimationTargetType, times, float3Vals, interpolationType);
                    break;
                case GltfAccessorAttributeType.VEC4:
                    var float4Vals = ((AccessorNativeData<Vector4>)values).data.Reinterpret<float4>();
                    AddVec4Curves(clip, animationPath, data.AnimationProperties, data.AnimationTargetType, times, float4Vals, interpolationType);
                    break;
            }
        }

        public static void AddTranslationCurves(AnimationClip clip, string animationPath, NativeArray<float> times, NativeArray<Vector3> translations, InterpolationType interpolationType) {
            // TODO: Refactor interface to use Unity.Mathematics types and remove this Reinterpret
            var values = translations.Reinterpret<float3>();
            string[] translationNames = {
                "localPosition.x",
                "localPosition.y",
                "localPosition.z"
            };
            AddVec3Curves(clip, animationPath, translationNames, typeof(Transform), times, values, interpolationType);
        }

        public static void AddScaleCurves(AnimationClip clip, string animationPath, NativeArray<float> times, NativeArray<Vector3> translations, InterpolationType interpolationType) {
            // TODO: Refactor interface to use Unity.Mathematics types and remove this Reinterpret
            var values = translations.Reinterpret<float3>();
            string[] scaleNames = {
                "localScale.x",
                "localScale.y",
                "localScale.z"
            };
            AddVec3Curves(clip, animationPath, scaleNames, typeof(Transform), times, values, interpolationType);
        }

        public static void AddRotationCurves(AnimationClip clip, string animationPath, NativeArray<float> times, NativeArray<Quaternion> rotations, InterpolationType interpolationType) {
            string[] rotationNames = {
                "localRotation.x",
                "localRotation.y",
                "localRotation.z",
                "localRotation.w"
            };
            AddQuaternionCurves(clip, animationPath, rotationNames, typeof(Transform), times, rotations, interpolationType);
        }

        public static void AddScalarCurve(AnimationClip clip, string animationPath, string propertyName, Type targetType, NativeArray<float> times, NativeArray<float> values, InterpolationType interpolationType) {
            Profiler.BeginSample("AnimationUtils.AddScalarCurve");
            var curve = new AnimationCurve();

#if DEBUG
            uint duplicates = 0;
#endif

            switch (interpolationType) {
                case InterpolationType.Step: {
                    for (var i = 0; i < times.Length; i++) {
                        var time = times[i];
                        var value = values[i];
                        curve.AddKey( new Keyframe(time, value, float.PositiveInfinity, 0) );
                    }
                    break;
                }
                case InterpolationType.CubicSpline: {
                    for (var i = 0; i < times.Length; i++) {
                        var time = times[i];
                        var inTangent = values[i*3];
                        var value = values[i*3 + 1];
                        var outTangent = values[i*3 + 2];
                        curve.AddKey( new Keyframe(time, value, inTangent, outTangent, .5f, .5f ) );
                    }
                    break;
                }
                default: { // LINEAR
                    var prevTime = times[0];
                    var prevValue = values[0];
                    float inTangent = values[0];

                    for (var i = 1; i < times.Length; i++) {
                        var time = times[i];
                        var value = values[i];

                        if (prevTime >= time) {
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
                        if (dT < k_TimeEpsilon) {
                            outTangent = (dV < 0f) ^ (dT < 0f) ? float.NegativeInfinity : float.PositiveInfinity;
                        } else {
                            outTangent = dV / dT;
                        }

                        curve.AddKey( new Keyframe(prevTime, prevValue, inTangent, outTangent ) );

                        inTangent = outTangent;
                        prevTime = time;
                        prevValue = value;
                    }

                    curve.AddKey( new Keyframe(prevTime, prevValue, inTangent, 0 ) );

                    break;
                }
            }

            clip.SetCurve(animationPath, targetType, propertyName, curve);
            Profiler.EndSample();
#if DEBUG
            if (duplicates > 0) {
                ReportDuplicateKeyframes();
            }
#endif
        }

        public static void AddVec2Curves(AnimationClip clip, string animationPath, string[] propertyNames, Type targetType, NativeArray<float> times, NativeArray<float2> values, InterpolationType interpolationType) {
            Profiler.BeginSample("AnimationUtils.AddVec2Curves");
            var curveX = new AnimationCurve();
            var curveY = new AnimationCurve();

#if DEBUG
            uint duplicates = 0;
#endif

            switch (interpolationType) {
                case InterpolationType.Step: {
                    for (var i = 0; i < times.Length; i++) {
                        var time = times[i];
                        var value = values[i];
                        curveX.AddKey( new Keyframe(time, value.x, float.PositiveInfinity, 0) );
                        curveY.AddKey( new Keyframe(time, value.y, float.PositiveInfinity, 0) );
                    }
                    break;
                }
                case InterpolationType.CubicSpline: {
                    for (var i = 0; i < times.Length; i++) {
                        var time = times[i];
                        var inTangent = values[i*3];
                        var value = values[i*3 + 1];
                        var outTangent = values[i*3 + 2];
                        curveX.AddKey( new Keyframe(time, value.x, inTangent.x, outTangent.x, .5f, .5f ) );
                        curveY.AddKey( new Keyframe(time, value.y, inTangent.y, outTangent.y, .5f, .5f ) );
                    }
                    break;
                }
                default: { // LINEAR
                    var prevTime = times[0];
                    var prevValue = values[0];
                    var inTangent = new float2(0f);

                    for (var i = 1; i < times.Length; i++) {
                        var time = times[i];
                        var value = values[i];

                        if (prevTime >= time) {
                            // Time value is not increasing, so we ignore this keyframe
                            // This happened on some Sketchfab files (see #298)
#if DEBUG
                            duplicates++;
#endif
                            continue;
                        }

                        var dT = time - prevTime;
                        var dV = value - prevValue;
                        float2 outTangent;
                        if (dT < k_TimeEpsilon) {
                            outTangent.x = (dV.x < 0f) ^ (dT < 0f) ? float.NegativeInfinity : float.PositiveInfinity;
                            outTangent.y = (dV.y < 0f) ^ (dT < 0f) ? float.NegativeInfinity : float.PositiveInfinity;
                        } else {
                            outTangent = dV / dT;
                        }

                        curveX.AddKey( new Keyframe(prevTime, prevValue.x, inTangent.x, outTangent.x ) );
                        curveY.AddKey( new Keyframe(prevTime, prevValue.y, inTangent.y, outTangent.y ) );

                        inTangent = outTangent;
                        prevTime = time;
                        prevValue = value;
                    }

                    curveX.AddKey( new Keyframe(prevTime, prevValue.x, inTangent.x, 0 ) );
                    curveY.AddKey( new Keyframe(prevTime, prevValue.y, inTangent.y, 0 ) );

                    break;
                }
            }

            clip.SetCurve(animationPath, targetType, propertyNames[0], curveX);
            clip.SetCurve(animationPath, targetType, propertyNames[1], curveY);
            Profiler.EndSample();
#if DEBUG
            if (duplicates > 0) {
                ReportDuplicateKeyframes();
            }
#endif
        }

        public static void AddVec3Curves(AnimationClip clip, string animationPath, string[] propertyNames, Type targetType, NativeArray<float> times, NativeArray<float3> values, InterpolationType interpolationType) {
            Profiler.BeginSample("AnimationUtils.AddVec3Curves");
            var curveX = new AnimationCurve();
            var curveY = new AnimationCurve();
            var curveZ = new AnimationCurve();

#if DEBUG
            uint duplicates = 0;
#endif

            switch (interpolationType) {
                case InterpolationType.Step: {
                    for (var i = 0; i < times.Length; i++) {
                        var time = times[i];
                        var value = values[i];
                        curveX.AddKey( new Keyframe(time, value.x, float.PositiveInfinity, 0) );
                        curveY.AddKey( new Keyframe(time, value.y, float.PositiveInfinity, 0) );
                        curveZ.AddKey( new Keyframe(time, value.z, float.PositiveInfinity, 0) );
                    }
                    break;
                }
                case InterpolationType.CubicSpline: {
                    for (var i = 0; i < times.Length; i++) {
                        var time = times[i];
                        var inTangent = values[i*3];
                        var value = values[i*3 + 1];
                        var outTangent = values[i*3 + 2];
                        curveX.AddKey( new Keyframe(time, value.x, inTangent.x, outTangent.x, .5f, .5f ) );
                        curveY.AddKey( new Keyframe(time, value.y, inTangent.y, outTangent.y, .5f, .5f ) );
                        curveZ.AddKey( new Keyframe(time, value.z, inTangent.z, outTangent.z, .5f, .5f ) );
                    }
                    break;
                }
                default: { // LINEAR
                    var prevTime = times[0];
                    var prevValue = values[0];
                    var inTangent = new float3(0f);

                    for (var i = 1; i < times.Length; i++) {
                        var time = times[i];
                        var value = values[i];

                        if (prevTime >= time) {
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
                        if (dT < k_TimeEpsilon) {
                            outTangent.x = (dV.x < 0f) ^ (dT < 0f) ? float.NegativeInfinity : float.PositiveInfinity;
                            outTangent.y = (dV.y < 0f) ^ (dT < 0f) ? float.NegativeInfinity : float.PositiveInfinity;
                            outTangent.z = (dV.z < 0f) ^ (dT < 0f) ? float.NegativeInfinity : float.PositiveInfinity;
                        } else {
                            outTangent = dV / dT;
                        }

                        curveX.AddKey( new Keyframe(prevTime, prevValue.x, inTangent.x, outTangent.x ) );
                        curveY.AddKey( new Keyframe(prevTime, prevValue.y, inTangent.y, outTangent.y ) );
                        curveZ.AddKey( new Keyframe(prevTime, prevValue.z, inTangent.z, outTangent.z ) );

                        inTangent = outTangent;
                        prevTime = time;
                        prevValue = value;
                    }

                    curveX.AddKey( new Keyframe(prevTime, prevValue.x, inTangent.x, 0 ) );
                    curveY.AddKey( new Keyframe(prevTime, prevValue.y, inTangent.y, 0 ) );
                    curveZ.AddKey( new Keyframe(prevTime, prevValue.z, inTangent.z, 0 ) );

                    break;
                }
            }

            clip.SetCurve(animationPath, targetType, propertyNames[0], curveX);
            clip.SetCurve(animationPath, targetType, propertyNames[1], curveY);
            clip.SetCurve(animationPath, targetType, propertyNames[2], curveZ);
            Profiler.EndSample();
#if DEBUG
            if (duplicates > 0) {
                ReportDuplicateKeyframes();
            }
#endif
        }

        public static void AddVec4Curves(AnimationClip clip, string animationPath, string[] propertyNames, Type targetType, NativeArray<float> times, NativeArray<float4> values, InterpolationType interpolationType) {
            Profiler.BeginSample("AnimationUtils.AddVec4Curves");
            var curveX = new AnimationCurve();
            var curveY = new AnimationCurve();
            var curveZ = new AnimationCurve();
            var curveW = new AnimationCurve();

#if DEBUG
            uint duplicates = 0;
#endif

            switch (interpolationType) {
                case InterpolationType.Step: {
                    for (var i = 0; i < times.Length; i++) {
                        var time = times[i];
                        var value = values[i];
                        curveX.AddKey( new Keyframe(time, value.x, float.PositiveInfinity, 0) );
                        curveY.AddKey( new Keyframe(time, value.y, float.PositiveInfinity, 0) );
                        curveZ.AddKey( new Keyframe(time, value.z, float.PositiveInfinity, 0) );
                        curveW.AddKey( new Keyframe(time, value.w, float.PositiveInfinity, 0) );
                    }
                    break;
                }
                case InterpolationType.CubicSpline: {
                    for (var i = 0; i < times.Length; i++) {
                        var time = times[i];
                        var inTangent = values[i*3];
                        var value = values[i*3 + 1];
                        var outTangent = values[i*3 + 2];
                        curveX.AddKey( new Keyframe(time, value.x, inTangent.x, outTangent.x, .5f, .5f ) );
                        curveY.AddKey( new Keyframe(time, value.y, inTangent.y, outTangent.y, .5f, .5f ) );
                        curveZ.AddKey( new Keyframe(time, value.z, inTangent.z, outTangent.z, .5f, .5f ) );
                        curveW.AddKey( new Keyframe(time, value.w, inTangent.w, outTangent.w, .5f, .5f ) );
                    }
                    break;
                }
                default: { // LINEAR
                    var prevTime = times[0];
                    var prevValue = values[0];
                    var inTangent = new float4(0f);

                    for (var i = 1; i < times.Length; i++) {
                        var time = times[i];
                        var value = values[i];

                        if (prevTime >= time) {
                            // Time value is not increasing, so we ignore this keyframe
                            // This happened on some Sketchfab files (see #298)
#if DEBUG
                            duplicates++;
#endif
                            continue;
                        }

                        var dT = time - prevTime;
                        var dV = value - prevValue;
                        float4 outTangent;
                        if (dT < k_TimeEpsilon) {
                            outTangent.x = (dV.x < 0f) ^ (dT < 0f) ? float.NegativeInfinity : float.PositiveInfinity;
                            outTangent.y = (dV.y < 0f) ^ (dT < 0f) ? float.NegativeInfinity : float.PositiveInfinity;
                            outTangent.z = (dV.z < 0f) ^ (dT < 0f) ? float.NegativeInfinity : float.PositiveInfinity;
                            outTangent.w = (dV.z < 0f) ^ (dT < 0f) ? float.NegativeInfinity : float.PositiveInfinity;
                        } else {
                            outTangent = dV / dT;
                        }

                        curveX.AddKey( new Keyframe(prevTime, prevValue.x, inTangent.x, outTangent.x ) );
                        curveY.AddKey( new Keyframe(prevTime, prevValue.y, inTangent.y, outTangent.y ) );
                        curveZ.AddKey( new Keyframe(prevTime, prevValue.z, inTangent.z, outTangent.z ) );
                        curveW.AddKey( new Keyframe(prevTime, prevValue.w, inTangent.w, outTangent.w ) );

                        inTangent = outTangent;
                        prevTime = time;
                        prevValue = value;
                    }

                    curveX.AddKey( new Keyframe(prevTime, prevValue.x, inTangent.x, 0 ) );
                    curveY.AddKey( new Keyframe(prevTime, prevValue.y, inTangent.y, 0 ) );
                    curveZ.AddKey( new Keyframe(prevTime, prevValue.z, inTangent.z, 0 ) );
                    curveW.AddKey( new Keyframe(prevTime, prevValue.z, inTangent.w, 0 ) );

                    break;
                }
            }
            clip.SetCurve(animationPath, targetType, propertyNames[0], curveX);
            clip.SetCurve(animationPath, targetType, propertyNames[1], curveY);
            clip.SetCurve(animationPath, targetType, propertyNames[2], curveZ);
            clip.SetCurve(animationPath, targetType, propertyNames[3], curveW);
            Profiler.EndSample();
#if DEBUG
            if (duplicates > 0) {
                ReportDuplicateKeyframes();
            }
#endif
        }

        public static void AddQuaternionCurves(AnimationClip clip, string animationPath, string[] propertyNames, Type targetType, NativeArray<float> times, NativeArray<Quaternion> quaternions, InterpolationType interpolationType) {
            Profiler.BeginSample("AnimationUtils.AddQuaternionCurves");
            var rotX = new AnimationCurve();
            var rotY = new AnimationCurve();
            var rotZ = new AnimationCurve();
            var rotW = new AnimationCurve();

            // TODO: Refactor interface to use Unity.Mathematics types and remove this Reinterpret
            var values = quaternions.Reinterpret<quaternion>();

#if DEBUG
            uint duplicates = 0;
#endif

            switch (interpolationType) {
                case InterpolationType.Step: {
                    for (var i = 0; i < times.Length; i++) {
                        var time = times[i];
                        var value = values[i];
                        rotX.AddKey( new Keyframe(time, value.value.x, float.PositiveInfinity, 0) );
                        rotY.AddKey( new Keyframe(time, value.value.y, float.PositiveInfinity, 0) );
                        rotZ.AddKey( new Keyframe(time, value.value.z, float.PositiveInfinity, 0) );
                        rotW.AddKey( new Keyframe(time, value.value.w, float.PositiveInfinity, 0) );
                    }
                    break;
                }
                case InterpolationType.CubicSpline: {
                    for (var i = 0; i < times.Length; i++) {
                        var time = times[i];
                        var inTangent = values[i*3];
                        var value = values[i*3 + 1];
                        var outTangent = values[i*3 + 2];
                        rotX.AddKey( new Keyframe(time, value.value.x, inTangent.value.x, outTangent.value.x, .5f, .5f ) );
                        rotY.AddKey( new Keyframe(time, value.value.y, inTangent.value.y, outTangent.value.y, .5f, .5f ) );
                        rotZ.AddKey( new Keyframe(time, value.value.z, inTangent.value.z, outTangent.value.z, .5f, .5f ) );
                        rotW.AddKey( new Keyframe(time, value.value.w, inTangent.value.w, outTangent.value.w, .5f, .5f ) );
                    }
                    break;
                }
                default: { // LINEAR
                    var prevTime = times[0];
                    var prevValue = values[0];
                    var inTangent = new quaternion(new float4(0f));

                    for (var i = 1; i < times.Length; i++) {
                        var time = times[i];
                        var value = values[i];

                        if (prevTime >= time) {
                            // Time value is not increasing, so we ignore this keyframe
                            // This happened on some Sketchfab files (see #298)
#if DEBUG
                            duplicates++;
#endif
                            continue;
                        }

                        // Ensure shortest path rotation ( see https://www.khronos.org/registry/glTF/specs/2.0/glTF-2.0.html#interpolation-slerp )
                        if (math.dot(prevValue, value) < 0) {
                            value.value = -value.value;
                        }

                        var dT = time - prevTime;
                        var dV = value.value - prevValue.value;
                        quaternion outTangent;
                        if (dT < k_TimeEpsilon) {
                            outTangent.value.x = (dV.x < 0f) ^ (dT < 0f) ? float.NegativeInfinity : float.PositiveInfinity;
                            outTangent.value.y = (dV.y < 0f) ^ (dT < 0f) ? float.NegativeInfinity : float.PositiveInfinity;
                            outTangent.value.z = (dV.z < 0f) ^ (dT < 0f) ? float.NegativeInfinity : float.PositiveInfinity;
                            outTangent.value.w = (dV.w < 0f) ^ (dT < 0f) ? float.NegativeInfinity : float.PositiveInfinity;
                        } else {
                            outTangent = dV / dT;
                        }

                        rotX.AddKey( new Keyframe(prevTime, prevValue.value.x, inTangent.value.x, outTangent.value.x ) );
                        rotY.AddKey( new Keyframe(prevTime, prevValue.value.y, inTangent.value.y, outTangent.value.y ) );
                        rotZ.AddKey( new Keyframe(prevTime, prevValue.value.z, inTangent.value.z, outTangent.value.z ) );
                        rotW.AddKey( new Keyframe(prevTime, prevValue.value.w, inTangent.value.w, outTangent.value.w ) );

                        inTangent = outTangent;
                        prevTime = time;
                        prevValue = value;
                    }

                    rotX.AddKey( new Keyframe(prevTime, prevValue.value.x, inTangent.value.x, 0 ) );
                    rotY.AddKey( new Keyframe(prevTime, prevValue.value.y, inTangent.value.y, 0 ) );
                    rotZ.AddKey( new Keyframe(prevTime, prevValue.value.z, inTangent.value.z, 0 ) );
                    rotW.AddKey( new Keyframe(prevTime, prevValue.value.w, inTangent.value.w, 0 ) );

                    break;
                }
            }

            clip.SetCurve(animationPath, targetType, propertyNames[0], rotX);
            clip.SetCurve(animationPath, targetType, propertyNames[1], rotY);
            clip.SetCurve(animationPath, targetType, propertyNames[2], rotZ);
            clip.SetCurve(animationPath, targetType, propertyNames[3], rotW);
            Profiler.EndSample();

#if DEBUG
            if (duplicates > 0) {
                ReportDuplicateKeyframes();
            }
#endif
        }

        public static string CreateAnimationPath(int nodeIndex, string[] nodeNames, int[] parentIndex) {
            Profiler.BeginSample("AnimationUtils.CreateAnimationPath");
            var sb = new StringBuilder();
            do {
                if (sb.Length > 0) {
                    sb.Insert(0,'/');
                }
                sb.Insert(0,nodeNames[nodeIndex]);
                nodeIndex = parentIndex[nodeIndex];
            } while (nodeIndex>=0);
            Profiler.EndSample();
            return sb.ToString();
        }

        public static void AddMorphTargetWeightCurves(
            AnimationClip clip,
            string animationPath,
            NativeArray<float> times,
            NativeArray<float> values,
            InterpolationType interpolationType,
            string[] morphTargetNames = null
            )
        {
            Profiler.BeginSample("AnimationUtils.AddMorphTargetWeightCurves");
            int morphTargetCount;
            if (morphTargetNames == null) {
                morphTargetCount = values.Length / times.Length;
                if (interpolationType == InterpolationType.CubicSpline) {
                    // 3 values per key (in-tangent, out-tangent and value)
                    morphTargetCount /= 3;
                }
            }
            else {
                morphTargetCount = morphTargetNames.Length;
            }

            for (var i = 0; i < morphTargetCount; i++) {
                var morphTargetName = morphTargetNames==null ? i.ToString() : morphTargetNames[i];
                AddBlendCurve(
                    clip,
                    animationPath,
                    morphTargetName,
                    i,
                    morphTargetCount,
                    times,
                    values,
                    interpolationType
                    );
            }
            Profiler.EndSample();
        }


        public static void AddBlendCurve(AnimationClip clip, string animationPath, string propertyPrefix, int curveIndex, int valueStride, NativeArray<float> times, NativeArray<float> values, InterpolationType interpolationType) {
            Profiler.BeginSample("AnimationUtils.AddBlendCurve");
            var curve = new AnimationCurve();

#if DEBUG
            uint duplicates = 0;
#endif

            switch (interpolationType) {
                case InterpolationType.Step: {
                    for (var i = 0; i < times.Length; i++) {
                        var time = times[i];
                        var valueIndex = i * valueStride + curveIndex;
                        var value = values[valueIndex];
                        curve.AddKey( new Keyframe(time, value, float.PositiveInfinity, 0) );
                    }
                    break;
                }
                case InterpolationType.CubicSpline: {
                    for (var i = 0; i < times.Length; i++) {
                        var time = times[i];
                        var valueIndex = i * valueStride + curveIndex;
                        var inTangent = values[valueIndex*3];
                        var value = values[valueIndex*3 + 1];
                        var outTangent = values[valueIndex*3 + 2];
                        curve.AddKey( new Keyframe(time, value, inTangent, outTangent, .5f, .5f ) );
                    }
                    break;
                }
                default: { // LINEAR
                    var prevTime = times[0];
                    var prevValue = values[curveIndex];
                    var inTangent = 0f;

                    for (var i = 1; i < times.Length; i++) {
                        var time = times[i];
                        var valueIndex = i * valueStride + curveIndex;
                        var value = values[valueIndex];

                        if (prevTime >= time) {
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
                        if (dT < k_TimeEpsilon) {
                            outTangent = (dV < 0f) ^ (dT < 0f) ? float.NegativeInfinity : float.PositiveInfinity;
                        } else {
                            outTangent = dV / dT;
                        }

                        curve.AddKey( new Keyframe(prevTime, prevValue, inTangent, outTangent ) );

                        inTangent = outTangent;
                        prevTime = time;
                        prevValue = value;
                    }

                    curve.AddKey( new Keyframe(prevTime, prevValue, inTangent, 0 ) );

                    break;
                }
            }

            clip.SetCurve(animationPath, typeof(SkinnedMeshRenderer), $"blendShape.{propertyPrefix}", curve);
            Profiler.EndSample();
#if DEBUG
            if (duplicates > 0) {
                ReportDuplicateKeyframes();
            }
#endif
        }

        public static void AddTexSTCurves(AnimationClip clip, string animationPath, string propertyPrefix, NativeArray<float> times, NativeArray<Vector2> values, InterpolationType interpolationType, bool zw = false) {
            Profiler.BeginSample("AnimationUtils.AddTexSTCurves");
            var curveX = new AnimationCurve();
            var curveY = new AnimationCurve();

#if DEBUG
            uint duplicates = 0;
#endif

            switch (interpolationType) {
                case InterpolationType.Step: {
                    for (var i = 0; i < times.Length; i++) {
                        var time = times[i];
                        var value = values[i];
                        curveX.AddKey( new Keyframe(time, value.x, float.PositiveInfinity, 0) );
                        curveY.AddKey( new Keyframe(time, value.y, float.PositiveInfinity, 0) );
                    }
                    break;
                }
                case InterpolationType.CubicSpline: {
                    for (var i = 0; i < times.Length; i++) {
                        var time = times[i];
                        var inTangent = values[i*3];
                        var value = values[i*3 + 1];
                        var outTangent = values[i*3 + 2];
                        curveX.AddKey( new Keyframe(time, value.x, inTangent.x, outTangent.x, .5f, .5f ) );
                        curveY.AddKey( new Keyframe(time, value.y, inTangent.y, outTangent.y, .5f, .5f ) );
                    }
                    break;
                }
                default: { // LINEAR
                    var prevTime = times[0];
                    var prevValue = values[0];
                    var inTangent = new float2(0f);

                    for (var i = 1; i < times.Length; i++) {
                        var time = times[i];
                        var value = values[i];
                
                        if (prevTime >= time) {
                            // Time value is not increasing, so we ignore this keyframe
                            // This happened on some Sketchfab files (see #298)
#if DEBUG
                            duplicates++;
#endif
                            continue;
                        }

                        var dT = time - prevTime;
                        var dV = value - prevValue;
                        float2 outTangent;
                        if (dT < k_TimeEpsilon) {
                            outTangent.x = (dV.x < 0f) ^ (dT < 0f) ? float.NegativeInfinity : float.PositiveInfinity;
                            outTangent.y = (dV.y < 0f) ^ (dT < 0f) ? float.NegativeInfinity : float.PositiveInfinity;
                        } else {
                            outTangent = dV / dT;
                        }
                        curveX.AddKey( new Keyframe(prevTime, prevValue.x, inTangent.x, outTangent.x ) );
                        curveY.AddKey( new Keyframe(prevTime, prevValue.y, inTangent.y, outTangent.y ) );

                        inTangent = outTangent;
                        prevTime = time;
                        prevValue = value;
                    }

                    curveX.AddKey( new Keyframe(prevTime, prevValue.x, inTangent.x, 0 ) );
                    curveY.AddKey( new Keyframe(prevTime, prevValue.y, inTangent.y, 0 ) );

                    break;
                }
            }
            clip.SetCurve(animationPath, typeof(MeshRenderer), $"{propertyPrefix}{(zw?"z":"x")}", curveX);
            clip.SetCurve(animationPath, typeof(MeshRenderer), $"{propertyPrefix}{(zw?"w":"y")}", curveY);
            Profiler.EndSample();
#if DEBUG
            if (duplicates > 0) {
                ReportDuplicateKeyframes();
            }
#endif
        }
        
#if DEBUG
        static void ReportDuplicateKeyframes() {
            Debug.LogError("Time of subsequent animation keyframes is not increasing (glTF-Validator error ACCESSOR_ANIMATION_INPUT_NON_INCREASING)");
        }
#endif
    }
}

#endif // UNITY_ANIMATION
