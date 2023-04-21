using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GLTFast {
    public class AnimationCameraGameObject : MonoBehaviour {
        public Camera targetCamera;
        public bool orthographic;
        public float localScale;

        public float nearClipPlane;
        float nearClipPlaneOld;
        public float farClipPlane;
        float farClipPlaneOld;

        public float xMag;
        public float yMag;
        float xMagOld;
        float yMagOld;

        public float fov;
        float fovOld;

        void OnEnable() {
            if (targetCamera == null) {
                Debug.LogError("Target camera not set on animated camera!");
            }
        }

        void LateUpdate() {
            bool orthoChanged = false;
            if(farClipPlane != farClipPlaneOld) {
                if(orthographic) {
                    orthoChanged = true;
                    farClipPlane = farClipPlane >= 0 ? farClipPlane : float.MaxValue;
                }
                targetCamera.farClipPlane = localScale * farClipPlane;
                farClipPlaneOld = farClipPlane;
            }

            if(nearClipPlane != nearClipPlaneOld) {
                targetCamera.nearClipPlane = localScale * nearClipPlane;
                nearClipPlaneOld = nearClipPlane;
                orthoChanged = true;
            }

            if(xMag != xMagOld) {
                xMagOld = xMag;
                orthoChanged = true;
            }

            if(yMag != yMagOld) {
                yMagOld = yMag;
                orthoChanged = true;
            }

            if(fov != fovOld) {
                targetCamera.fieldOfView = fov * Mathf.Rad2Deg;
                fovOld = fov;
            }

            if(orthographic && orthoChanged) {
                targetCamera.projectionMatrix = Matrix4x4.Ortho(
                    -xMag,
                    xMag,
                    -yMag,
                    yMag,
                    nearClipPlane,
                    farClipPlane
                );
            }
        }
    }
}
