using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectMove : MonoBehaviour {

    public float moveSpeed = 5;
    public float rotateSpeed = 15;

    void Update()
    {
        Move();
    }

    void Move()
    {
        if (Input.GetAxis("Horizontal") != 0 || JoyStick.instance.Horizon != 0)
        {
            if (Input.GetAxis("Horizontal") != 0)
            {
                transform.Rotate(Vector3.up * rotateSpeed * Input.GetAxis("Horizontal") * 2 * Time.deltaTime);
            }
            else
                transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime *
                JoyStick.instance.Horizon / JoyStick.instance.dragSpeed);
        }
        if (Input.GetAxis("Vertical") != 0 || JoyStick.instance.Vertical != 0)
        {
            if (Input.GetAxis("Vertical") != 0)
            {
                transform.Translate(Vector3.right * Input.GetAxis("Vertical") * moveSpeed * 2 * Time.deltaTime);
            }
            else
                transform.Translate(Vector3.right * moveSpeed * Time.deltaTime *
              JoyStick.instance.Vertical / JoyStick.instance.dragSpeed);
        }
    }
}
