using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {

    public GameObject target;
    public int speed;
    Vector3 offset;
	void Start () {
        offset = transform.position - target.transform.position;
	}
	

	void FixedUpdate () {
        Vector3 cameraPos = target.transform.position + offset;
        transform.position = Vector3.Lerp(transform.position, cameraPos, Time.deltaTime * speed);

	}
}
