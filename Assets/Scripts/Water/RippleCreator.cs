using UnityEngine;
using System.Collections.Generic;

public class RippleCreator : MonoBehaviour
{
    class ReversedRipple
    {
        public Vector3 Position;
        public float Velocity;
    }
    public bool isReversedRipple;
    public float rippleStrenght = 0.1f;
    public float maxSpeed = 1.5f;
    public float randomRipplesInterval = 0;
    public float reversedRippleDelay = 0.2f;
    public GameObject splashEffect;
    public GameObject splashEffectMoved;
    public AudioSource splashAudioSource;
    public float splashSizeMultiplier = 2;

    int fadeInVelocityLimit = 10;
    int fadeInVelocity = 1;
    WaterRipple waterRipple;
    Vector3 oldPos;
    float currentVelocity;
    Transform oldTransform;
    Queue<ReversedRipple> reversedVelocityQueue;
    float triggeredTime;
    bool canUpdate;
    float randomRipplesCurrentTime;
    bool canCreateRandomRipple;
    GameObject splashMovedInstance;
    ParticleSystem splashParticleSystem;
    

    void Start()
    {
        oldTransform = transform;
        reversedVelocityQueue = new Queue<ReversedRipple>();
    }

    void OnEnable()
    {
        waterRipple = null;
        canUpdate = false;
        if (splashMovedInstance != null)
        {
            Destroy(splashMovedInstance);
        }
    }

    void FixedUpdate()
    {
        if (!waterRipple)
            return;

        if (randomRipplesInterval > 0.0001f && Time.time - randomRipplesCurrentTime > randomRipplesInterval)
        {
            randomRipplesCurrentTime = Time.time;
            canCreateRandomRipple = true;
        }

        if (canUpdate)
        {
            currentVelocity = ((oldTransform.position - oldPos).magnitude / Time.fixedDeltaTime) * rippleStrenght;
            if (currentVelocity > maxSpeed)
                currentVelocity = maxSpeed;
            if (isReversedRipple)
                currentVelocity = -currentVelocity;
            reversedVelocityQueue.Enqueue(new ReversedRipple { Position = oldTransform.position, Velocity = -currentVelocity / fadeInVelocity });
            oldPos = oldTransform.position;
            waterRipple.CreateRippleByPosition(oldTransform.position, currentVelocity / fadeInVelocity);
            if (canCreateRandomRipple)
                waterRipple.CreateRippleByPosition(oldTransform.position, Random.Range(currentVelocity / 5, currentVelocity));
            UpdateMovedSplash();
        }

        if (Time.time - triggeredTime > reversedRippleDelay)
        {
            var reversedRipple = reversedVelocityQueue.Dequeue();
            if (isReversedRipple)
                reversedRipple.Velocity = -reversedRipple.Velocity;
            waterRipple.CreateRippleByPosition(reversedRipple.Position, reversedRipple.Velocity);
            if (canCreateRandomRipple)
                waterRipple.CreateRippleByPosition(reversedRipple.Position, Random.Range(reversedRipple.Velocity / 5, reversedRipple.Velocity));
        }
        ++fadeInVelocity;
        if (fadeInVelocity > fadeInVelocityLimit)
            fadeInVelocity = 1;
        if (canCreateRandomRipple)
            canCreateRandomRipple = false;
    }

     void OnTriggerEnter(Collider collidedObj)
    {
        var temp = collidedObj.GetComponent<WaterRipple>();
        if (temp)
            waterRipple = temp;
        else
            return;
        canUpdate = true;
        reversedVelocityQueue.Clear();
        triggeredTime = Time.time;
        fadeInVelocity = 1;

        if (splashAudioSource != null) splashAudioSource.Play();
        if (splashEffect != null)
        {
            var offset = waterRipple.GetOffsetByPosition(oldTransform.position);
            offset.x = oldTransform.position.x;
            offset.z = oldTransform.position.z;
            var splash = Instantiate(splashEffect, offset, new Quaternion());
            Destroy(splash, 2);
        }
        UpdateMovedSplash();
    }

    void UpdateMovedSplash()
    {
        if (splashMovedInstance)
        {
            var offset = waterRipple.GetOffsetByPosition(oldTransform.position);
            offset.x = oldTransform.position.x;
            offset.z = oldTransform.position.z;
            splashMovedInstance.transform.position = offset;
            var main = splashParticleSystem.main;
            main.startSize = currentVelocity * splashSizeMultiplier;
        }
        else if (splashEffectMoved)
        {
            splashMovedInstance = Instantiate(splashEffectMoved, oldTransform.position, new Quaternion()) as GameObject;
            splashMovedInstance.transform.parent = waterRipple.transform;
            var offset = waterRipple.GetOffsetByPosition(oldTransform.position);
            offset.x = oldTransform.position.x;
            offset.z = oldTransform.position.z;
            splashMovedInstance.transform.position = offset;
            splashParticleSystem = splashMovedInstance.GetComponentInChildren<ParticleSystem>();
            var main = splashParticleSystem.main;
            main.startSize = currentVelocity * splashSizeMultiplier;
        }
    }
}