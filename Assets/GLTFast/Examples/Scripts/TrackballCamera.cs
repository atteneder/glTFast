using UnityEngine;
using System.Collections;
 
public class TrackballCamera : MonoBehaviour
{
 
	public float yawSensitivity = 1;
	public float pitchSensitivity = 1;
    public float scrollSensitivity = 1;
    
    public Transform target;
    public float distance = 2f;
    float yaw = 25;
    float pitch = -30;


    private Vector3? lastMousePosition;
    // Use this for initialization
    void Start ()
    {
    }
 
    // Update is called once per frame
    void LateUpdate ()
    {
        var mousePosn = Input.mousePosition;
 
        var mouseBtn = Input.GetMouseButton (0);

        var scroll = Input.GetAxis("Mouse ScrollWheel");
        distance = Mathf.Clamp(distance - scroll*scrollSensitivity,.1f,50);
        if (mouseBtn) {
            var pos = Input.mousePosition;
            if(lastMousePosition.HasValue) {
                yaw = (yaw + (pos.x-lastMousePosition.Value.x)*yawSensitivity) % 360;
                pitch = Mathf.Clamp(pitch+(pos.y-lastMousePosition.Value.y)*pitchSensitivity,-80,80);
            }
            lastMousePosition = pos;
        } else {
            lastMousePosition = null;
        }
        transform.rotation = Quaternion.AngleAxis(yaw,Vector3.up) * Quaternion.AngleAxis(pitch,Vector3.left);
        var dir = transform.forward;
        transform.position = (target==null? Vector3.zero:target.position) - dir*distance;
    }
}