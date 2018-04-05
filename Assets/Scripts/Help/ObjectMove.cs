using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectMove : MonoBehaviour {

    const float PRECISION = 0.000001f;
    Rigidbody rigid;
    bool up, left, down, right;
    public float moveSpeed = 5;
    public float rotateSpeed = 15;
    bool useJoyStick;

    private void Start()
    {
        rigid = GetComponent<Rigidbody>();
        if (JoyStick.instance)
            useJoyStick = true;
    }

    void Update()
    {
        if (useJoyStick)
        {
            Move();
        }
            if (Input.GetKeyDown(KeyCode.W))
                up = true;
            if (Input.GetKeyDown(KeyCode.A))
                left = true;
            if (Input.GetKeyDown(KeyCode.S))
                down = true;
            if (Input.GetKeyDown(KeyCode.D))
                right = true;

            if (Input.GetKeyUp(KeyCode.W))
                up = false;
            if (Input.GetKeyUp(KeyCode.A))
                left = false;
            if (Input.GetKeyUp(KeyCode.S))
                down = false;
            if (Input.GetKeyUp(KeyCode.D))
                right = false;
            if (up)
            {
                rigid.AddForce(transform.right * 500 * Time.deltaTime);
            }
            if (down)
            {
                rigid.AddForce(-transform.right * 500 * Time.deltaTime);
            }
            if (right)
            {
                rigid.AddTorque(transform.up * 200 * Time.deltaTime);
            }
            if (left)
            {
                rigid.AddTorque(-transform.up * 200 * Time.deltaTime);
            }
        
        
    }

    void Move()
    {
        if (Math.Abs(JoyStick.instance.Horizon) > PRECISION)
        {
            transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime / 2 *JoyStick.instance.Horizon / JoyStick.instance.dragSpeed);
        }
        if (Math.Abs(JoyStick.instance.Vertical) > PRECISION)
        {
            transform.Translate(Vector3.right * moveSpeed * Time.deltaTime / 2 *JoyStick.instance.Vertical / JoyStick.instance.dragSpeed);
        }
    }
}
