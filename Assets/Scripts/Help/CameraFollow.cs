using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour {

    public GameObject target;
    public int speed;
    Vector3 offset;
    void Start()
    {
        offset = transform.position - target.transform.position;
    }


    void LateUpdate()
    {
        Vector3 cameraPos = target.transform.position + offset;
        transform.position = Vector3.Lerp(transform.position, cameraPos, Time.deltaTime * speed);

    }
}
