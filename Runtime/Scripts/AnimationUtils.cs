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

        public static void AddTranslationCurves(AnimationClip clip, string animationPath, NativeArray<float> times, NativeArray<Vector3> values, InterpolationType interpolationType) {
            AddVec3Curves(clip, animationPath, "localPosition.", times, values, interpolationType);
        }

        public static void AddScaleCurves(AnimationClip clip, string animationPath, NativeArray<float> times, NativeArray<Vector3> values, InterpolationType interpolationType) {
            AddVec3Curves(clip, animationPath, "localScale.", times, values, interpolationType);
        }

        public static void AddRotationCurves(AnimationClip clip, string animationPath, NativeArray<float> times, NativeArray<Quaternion> quaternions, InterpolationType interpolationType) {
            Profiler.BeginSample("AnimationUtils.AddRotationCurves");
            var rotX = new AnimationCurve();
            var rotY = new AnimationCurve();
            var rotZ = new AnimationCurve();
            var rotW = new AnimationCurve();
            for (var i = 0; i < times.Length; i++) {
                rotX.AddKey(CreateKeyframe(i, times, quaternions, x => x.x, interpolationType));
                rotY.AddKey(CreateKeyframe(i, times, quaternions, x => x.y, interpolationType));
                rotZ.AddKey(CreateKeyframe(i, times, quaternions, x => x.z, interpolationType));
                rotW.AddKey(CreateKeyframe(i, times, quaternions, x => x.w, interpolationType));
            }

            if (interpolationType == InterpolationType.LINEAR) {
                FixRotations(rotX, rotY, rotZ, rotW);
                CalculateLinearTangents(times, rotX);
                CalculateLinearTangents(times, rotY);
                CalculateLinearTangents(times, rotZ);
                CalculateLinearTangents(times, rotW);
            }
            
            clip.SetCurve(animationPath, typeof(Transform), "localRotation.x", rotX);
            clip.SetCurve(animationPath, typeof(Transform), "localRotation.y", rotY);
            clip.SetCurve(animationPath, typeof(Transform), "localRotation.z", rotZ);
            clip.SetCurve(animationPath, typeof(Transform), "localRotation.w", rotW);
            Profiler.EndSample();
        }

        static void FixRotations(AnimationCurve rotX, AnimationCurve rotY, AnimationCurve rotZ, AnimationCurve rotW) {
            Profiler.BeginSample("AnimationUtils.FixRotations");
            var prev = new quaternion(
                rotX.keys[0].value,
                rotY.keys[0].value,
                rotZ.keys[0].value,
                rotW.keys[0].value
            );
            // prev = math.normalize(prev);
            for (var i = 1; i < rotX.keys.Length; i++) {
                var keyX = rotX.keys[i];
                var keyY = rotY.keys[i];
                var keyZ = rotZ.keys[i];
                var keyW = rotW.keys[i];
                var value = new quaternion(
                    keyX.value,
                    keyY.value,
                    keyZ.value,
                    keyW.value
                );
                // value = math.normalize(value);

                if (math.dot(prev, value) < 0) {
                    value.value = -value.value;

                    keyX.value = -keyX.value;
                    rotX.MoveKey(i, keyX);
                    
                    keyY.value = -keyY.value;
                    rotY.MoveKey(i, keyY);
                    
                    keyZ.value = -keyZ.value;
                    rotZ.MoveKey(i, keyZ);
                    
                    keyW.value = -keyW.value;
                    rotW.MoveKey(i, keyW);
                }
                prev = value;
            }
            Profiler.EndSample();
        }
        
        static void CalculateLinearTangents(NativeArray<float> times, AnimationCurve curve) {
            Profiler.BeginSample("AnimationUtils.CalculateLinearTangents");
            var prev = curve.keys[0];

            for (var i = 1; i < times.Length; i++) {
                var key = curve.keys[i];
                var dT = key.time - prev.time;
                var dV = key.value - prev.value;
                float inTangent;
                if (dT < k_TimeEpsilon) {
                    inTangent = (dV < 0f) ^ (dT < 0f) ? float.NegativeInfinity : float.PositiveInfinity;
                } else {
                    inTangent = dV / dT;
                }
                
                key.inTangent = inTangent;
                prev.outTangent = inTangent;
                curve.MoveKey(i-1, prev);
                prev = key;
            }
            curve.MoveKey(times.Length-1, prev);
            Profiler.EndSample();
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
                if (interpolationType == InterpolationType.CUBICSPLINE) {
                    // 3 values per key (in-tangent, out-tangent and value)
                    morphTargetCount /= 3;
                }
            }
            else {
                morphTargetCount = morphTargetNames.Length;
            }
            
            for (var i = 0; i < morphTargetCount; i++) {
                var morphTargetName = morphTargetNames==null ? i.ToString() : morphTargetNames[i];
                AddScalarCurve(
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

        static void AddVec3Curves(AnimationClip clip, string animationPath, string propertyPrefix, NativeArray<float> times, NativeArray<Vector3> values, InterpolationType interpolationType) {
            Profiler.BeginSample("AnimationUtils.AddVec3Curves");
            var curveX = new AnimationCurve();
            var curveY = new AnimationCurve();
            var curveZ = new AnimationCurve();
            for (var i = 0; i < times.Length; i++) {
                curveX.AddKey(CreateKeyframe(i, times, values, x => x.x, interpolationType));
                curveY.AddKey(CreateKeyframe(i, times, values, x => x.y, interpolationType));
                curveZ.AddKey(CreateKeyframe(i, times, values, x => x.z, interpolationType));
            }
            
            if (interpolationType == InterpolationType.LINEAR) {
                CalculateLinearTangents(times, curveX);
                CalculateLinearTangents(times, curveY);
                CalculateLinearTangents(times, curveZ);
            }
            
            clip.SetCurve(animationPath, typeof(Transform), $"{propertyPrefix}x", curveX);
            clip.SetCurve(animationPath, typeof(Transform), $"{propertyPrefix}y", curveY);
            clip.SetCurve(animationPath, typeof(Transform), $"{propertyPrefix}z", curveZ);
            Profiler.EndSample();
        }
        
        static void AddScalarCurve(AnimationClip clip, string animationPath, string propertyPrefix, int curveIndex, int valueStride, NativeArray<float> times, NativeArray<float> values, InterpolationType interpolationType) {
            Profiler.BeginSample("AnimationUtils.AddScalarCurve");
            var curve = new AnimationCurve();
            for (var timeIndex = 0; timeIndex < times.Length; timeIndex++) {
                curve.AddKey(CreateScalarKeyframe(timeIndex, times, curveIndex, valueStride, values, interpolationType));
            }
            
            if (interpolationType == InterpolationType.LINEAR) {
                CalculateLinearTangents(times, curve);
            }
            clip.SetCurve(animationPath, typeof(SkinnedMeshRenderer), $"blendShape.{propertyPrefix}", curve);
            Profiler.EndSample();
        }
        
        static Keyframe CreateKeyframe<T>(int index, NativeArray<float> timeArray, NativeArray<T> valueArray, Func<T, float> getValue, InterpolationType interpolationType) where T : struct {
            var time = timeArray[index];
            Keyframe keyframe;
            switch (interpolationType) {
                case InterpolationType.STEP:
                    keyframe = new Keyframe(time, getValue(valueArray[index]), float.PositiveInfinity, 0);
                    break;
                case InterpolationType.CUBICSPLINE: {
                    var inTangent = getValue(valueArray[index*3]);
                    var value = getValue(valueArray[index*3 + 1]);
                    var outTangent = getValue(valueArray[index*3 + 2]);
                    keyframe = new Keyframe(time, value, inTangent, outTangent, .5f, .5f);
                    break;
                }
                default: // LINEAR
                    keyframe = new Keyframe(time, getValue(valueArray[index]));
                    break;
            }
            return keyframe;
        }
        
        static Keyframe CreateScalarKeyframe(
            int index,
            NativeArray<float> timeArray,
            int curveIndex,
            int valueStride,
            NativeArray<float> valueArray,
            InterpolationType interpolationType
            )
        {
            var time = timeArray[index];
            Keyframe keyframe;
            var baseIndex = index * valueStride + curveIndex;
            switch (interpolationType) {
                case InterpolationType.STEP:
#if DEBUG
                    // TODO: Test and remove warning
                    Debug.LogWarning("STEP interpolation on weights is not tested!");
#endif
                    keyframe = new Keyframe(time, valueArray[baseIndex], float.PositiveInfinity, 0);
                    break;
                case InterpolationType.CUBICSPLINE: {
#if DEBUG
                    // TODO: Test and remove warning
                    Debug.LogWarning("CUBICSPLINE interpolation on weights is not tested!");
#endif
                    var inTangent = valueArray[baseIndex*3];
                    var value = valueArray[baseIndex*3 + 1];
                    var outTangent = valueArray[baseIndex*3 + 2];
                    keyframe = new Keyframe(time, value, inTangent, outTangent, .5f, .5f);
                    break;
                }
                default: // LINEAR
                    keyframe = new Keyframe(time, valueArray[baseIndex]);
                    break;
            }
            return keyframe;
        }
    }
}

#endif // UNITY_ANIMATION
