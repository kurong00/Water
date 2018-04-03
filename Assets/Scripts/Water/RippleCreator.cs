using UnityEngine;
using System.Collections.Generic;

public class RippleCreator : MonoBehaviour
{
    class ReversedRipple
    {
        public Vector3 Position;
        public float Velocity;
    }
    WaterRipple waterRipple;
    Queue<ReversedRipple> reversedVelocityQueue;
    float randomRipplesCurrentTime;
    float triggeredTime;
    bool canCreateRandomRipple;
    bool canUpdate;

    float currentSpeed;
    Transform oldTransform;
    Vector3 oldPosition;
    int fadeInSpeed = 1;
    int fadeInMaxSpeed = 10;
    GameObject splashObject;
    ParticleSystem splashParticleSystem;
    public float randomRippleIntervalTime = 0;
    public float maxSpeed = 1.5f;
    public float reversedRippleDelay = 0.2f;
    public float splashSizeMultiplier = 2;
    public GameObject splashEffect;
    public GameObject splashEffectMoved;
    public bool isReversedRipple;
    public AudioSource splashAudioSource;
    public float rippleStrength = 0.1f;
    void Start()
    {
        oldTransform = transform;
        reversedVelocityQueue = new Queue<ReversedRipple>();
    }

    void FixedUpdate()
    {
        if (!waterRipple)
            return;
        if (randomRippleIntervalTime > 0.0001f && Time.time - randomRipplesCurrentTime > randomRippleIntervalTime)
        {
            randomRipplesCurrentTime = Time.time;
            canCreateRandomRipple = true;
        }
        if (canUpdate)
        {
            currentSpeed = ((oldTransform.position - oldPosition).magnitude / Time.fixedDeltaTime) * rippleStrength;
            if (currentSpeed > maxSpeed)
                currentSpeed = maxSpeed;
            if (isReversedRipple)
                currentSpeed = -currentSpeed;
            reversedVelocityQueue.Enqueue(new ReversedRipple { Position = oldTransform.position, Velocity = -currentSpeed / fadeInSpeed });
            oldPosition = oldTransform.position;
            waterRipple.CreateRippleByPosition(oldTransform.position, currentSpeed);
            if (canCreateRandomRipple)
                waterRipple.CreateRippleByPosition(oldTransform.position, Random.Range(currentSpeed / 2, currentSpeed));
            UpdateSplash();
        }
        if (Time.time - triggeredTime > reversedRippleDelay)
        {
            var reversedRipple = reversedVelocityQueue.Dequeue();
            if (isReversedRipple)
                reversedRipple.Velocity = -reversedRipple.Velocity;
            waterRipple.CreateRippleByPosition(reversedRipple.Position, reversedRipple.Velocity);
            if (canCreateRandomRipple)
                waterRipple.CreateRippleByPosition(reversedRipple.Position, Random.Range(reversedRipple.Velocity / 2, reversedRipple.Velocity));
        }
        ++fadeInSpeed;
        if (fadeInSpeed > fadeInMaxSpeed)
            fadeInSpeed = 1;
        if (canCreateRandomRipple)
            canCreateRandomRipple = false;
    }

    void UpdateSplash()
    {
        if (splashObject)
        {
            var offset = waterRipple.GetOffsetByPosition(oldTransform.position);
            offset.x = oldTransform.position.x;
            offset.y = oldTransform.position.y;
            splashObject.transform.position = offset;
            var main = splashParticleSystem.main;
            main.startSize = currentSpeed * splashSizeMultiplier;
        }
        else if (splashEffectMoved)
        {
            splashObject = Instantiate(splashEffectMoved, oldTransform.position, new Quaternion(), waterRipple.transform);
            var offset = waterRipple.GetOffsetByPosition(oldTransform.position);
            offset.x = oldTransform.position.x;
            offset.y = oldTransform.position.y;
            splashObject.transform.position = offset;
            splashParticleSystem = splashObject.GetComponentInChildren<ParticleSystem>();
            var main = splashParticleSystem.main;
            main.startSize = currentSpeed * splashSizeMultiplier;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        var temp = other.GetComponent<WaterRipple>();
        if (temp)
            waterRipple = temp;
        else
            return;
        canUpdate = true;
        reversedVelocityQueue.Clear();
        triggeredTime = Time.time;
        fadeInSpeed = 1;
        if (splashAudioSource)
            splashAudioSource.Play();
        if (splashEffect)
        {
            var offset = waterRipple.GetOffsetByPosition(oldTransform.position);
            offset.x = oldTransform.position.x;
            offset.y = oldTransform.position.y;
            var splash = Instantiate(splashEffect, transform.position, new Quaternion());
            Destroy(splash, 2);
        }
        UpdateSplash();
    }
}