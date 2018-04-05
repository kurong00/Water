using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseMove : MonoBehaviour {

    public Transform target;
    public float distance = 10.0f;
    public float xSpeed = 250f;
    public float ySpeed = 120.0f;
    public float yMinLimit = -20f;
    public float yMaxLimit = 80f;
    float x, y = 0f;
    void Start() {
        if (GetComponent<Rigidbody>())
            GetComponent<Rigidbody>().freezeRotation = true;
    }

    void LateUpdate() {
        if (target && (Input.GetMouseButton(0) || Input.GetMouseButton(1)))
        {
            var pos = Input.mousePosition;
            if (pos.x < 250 && Screen.height - pos.y < 340)
                return;
            Cursor.visible = false;
            x += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
            y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;

            y = ClampAngle(y, yMinLimit, yMaxLimit);

            var rotation = Quaternion.Euler(y, x, 0);
            var position = rotation * new Vector3(0.0f, 0.0f, -distance) + target.position;

            transform.rotation = rotation;
            transform.position = position;
        }
        else
            Cursor.visible = true;
    }

    float ClampAngle(float angle,float min,float max)
    {
        if (angle < -360)
            angle += 360;
        if (angle > 360)
            angle -= 360;
        return Mathf.Clamp(angle, min, max);
    }
}
