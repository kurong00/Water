using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateRipple : MonoBehaviour {

    class ReversedRipple
    {
        public Vector3 Position;
        public float Velocity;
    }
    WaterRipple waterRipple;
    Queue<ReversedRipple> reversedVelocityQueue;
    float randomRipplesCurrentTime;
    bool canCreateRandomRipple;
    bool canUpdate;
    float currentSpeed;
    Transform oldTransform;
    Vector3 oldPosition;
    int fadeInSpeed = 1;
    public float randomRippleIntervalTime = 0;
    public float maxSpeed = 1.5f;
    bool isReversedRipple;
    void Awake () {
        oldTransform = transform;
        reversedVelocityQueue = new Queue<ReversedRipple>();
	}

    void FixedUpdate()
    {
        if (!waterRipple)
            return;
        if(randomRippleIntervalTime>0.0001f&&Time.time-randomRipplesCurrentTime>randomRippleIntervalTime)
        {
            randomRipplesCurrentTime = Time.time;
            canCreateRandomRipple = true;
        }
        if (canUpdate)
        {
            currentSpeed = ((oldTransform.position - oldPosition).magnitude / Time.fixedDeltaTime);
            if (currentSpeed > maxSpeed)
                currentSpeed = maxSpeed;
            if (isReversedRipple)
                currentSpeed = -currentSpeed;
            reversedVelocityQueue.Enqueue(new ReversedRipple { Position = oldTransform.position, Velocity = -currentSpeed / fadeInSpeed });
            oldPosition = oldTransform.position;
        }
    }

    void Update () {
		
	}
}
