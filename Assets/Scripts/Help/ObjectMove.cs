using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectMove : MonoBehaviour {

    public int speedMultiple;
    Rigidbody rigid;
    bool goUp, goLeft, goBack, goRight;
    void Start()
    {
        rigid = GetComponent<Rigidbody>();
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            goUp = true;
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            goLeft = true;
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            goBack = true;
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            goRight = true;

        if (Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.UpArrow))
            goUp = false;
        if (Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.LeftArrow))
            goLeft = false;
        if (Input.GetKeyUp(KeyCode.S) || Input.GetKeyUp(KeyCode.DownArrow))
            goBack = false;
        if (Input.GetKeyUp(KeyCode.D) || Input.GetKeyUp(KeyCode.RightArrow))
            goRight = false;

        if (goUp)
        {
            transform.Translate(-transform.forward * 5 * speedMultiple * Time.deltaTime);
            //rigid.AddForce(transform.right * 5 * speedMultiple * Time.deltaTime);
        }
        if (goBack)
        {
            transform.Translate(transform.forward * 5 * speedMultiple * Time.deltaTime);
            //rigid.AddForce(transform.right * 5 * speedMultiple * Time.deltaTime);
        }
        if (goRight)
        {
            transform.Translate(-transform.right * 5 * speedMultiple * Time.deltaTime);
            //rigid.AddTorque(transform.forward * 2 * speedMultiple * Time.deltaTime);
        }
        if (goLeft)
        {
            transform.Translate(transform.right * 5 * speedMultiple * Time.deltaTime);
            //rigid.AddTorque(-transform.forward * 2 * speedMultiple * Time.deltaTime);
        }
    }
}
