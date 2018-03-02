using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {

    public Transform target;
    public int speed;
    Vector3 offset;
	void Start () {
        offset = transform.position - target.transform.position;
	}
	

	void Update () {
        Vector3 cameraPos = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, cameraPos, Time.deltaTime * speed);

	}
}
