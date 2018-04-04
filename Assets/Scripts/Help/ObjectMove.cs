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
        if (Input.GetAxis("Horizontal") != 0)
        {
            if (Input.GetAxis("Horizontal") != 0)
            {
                transform.Rotate(Vector3.down * rotateSpeed * Input.GetAxis("Horizontal") * 2 * Time.deltaTime);
            }
            else if (JoyStick.instance != null && JoyStick.instance.Horizon != 0)
                transform.Rotate(Vector3.down * rotateSpeed * Time.deltaTime / 2 *
                JoyStick.instance.Horizon / JoyStick.instance.dragSpeed);
        }
        if (Input.GetAxis("Vertical") != 0)
        {
            if (Input.GetAxis("Vertical") != 0)
            {
                transform.Translate(Vector3.right * Input.GetAxis("Vertical") * moveSpeed * 2 * Time.deltaTime);
            }
            else if (JoyStick.instance != null && JoyStick.instance.Vertical != 0)
                transform.Translate(Vector3.right * moveSpeed * Time.deltaTime / 2 *
              JoyStick.instance.Vertical / JoyStick.instance.dragSpeed);
        }
    }
}
