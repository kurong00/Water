using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveWater : MonoBehaviour {

    
    public GameObject target;

    void Update()
    {
        if (target == null)
            return;
        var pos = transform.position;
        pos.x = target.transform.position.x;
        pos.z = target.transform.position.z;
        transform.position = pos;
    }
}
