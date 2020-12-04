using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class FrameBoundsCamera : MonoBehaviour {

    public BoxCollider boxCollider;

    private Camera _camera;

    // Start is called before the first frame update
    void Start() {
        _camera = GetComponent<Camera>();
        if (_camera == null) {
            Destroy(this);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (boxCollider == null) return;
        Bounds bounds = new Bounds(boxCollider.center,boxCollider.size);
        FrameBounds(_camera, boxCollider.transform, bounds);
    }

    public static void FrameBounds(Camera camera, Transform boundsTransform, Bounds bounds)
    {
        float3 scale = boundsTransform.localScale;
        float3 boundsSize = bounds.size;
        var distance = math.length(scale*boundsSize);
        var angle = math.radians(20);

        var centerPosition = boundsTransform.TransformPoint(bounds.center);
        var cameraPos = centerPosition +
                        new Vector3(0, distance * math.sin(angle), -distance * math.cos(angle));
        camera.transform.position = cameraPos;
        camera.transform.LookAt(centerPosition);

        camera.nearClipPlane = distance * .001f;
        camera.farClipPlane = distance * 3;
    }
}
