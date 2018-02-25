using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Buoyancy : MonoBehaviour {

    float voxelHalfHeight;
    List<Vector3> voxels;
    List<Vector3[]> forces;

    public float Density = 700;
    //物体的受力点，物体越复杂，受力点需要的越多
    public int SlicedAxis = 2;
    void Start () {
        forces = new List<Vector3[]>();
        var originalRotation = transform.rotation;
        var originalPosition = transform.position;
        transform.rotation = Quaternion.identity;
        transform.position = Vector3.zero;
        if(!GetComponent<Collider>())
            gameObject.AddComponent<MeshCollider>();
        var bounds = GetComponent<Collider>().bounds;
        voxelHalfHeight = bounds.size.x < bounds.size.y ? bounds.size.x : bounds.size.y;
        voxelHalfHeight = bounds.size.z < voxelHalfHeight ? bounds.size.z : voxelHalfHeight;
        voxelHalfHeight /= 2 * SlicedAxis;
        if (!GetComponent<Rigidbody>())
            gameObject.AddComponent<Rigidbody>();
    }
	
	void Update () {
		
	}
}
