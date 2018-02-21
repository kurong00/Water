using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterRipple : MonoBehaviour {

    Vector4 Amplitude;
    Vector4 Frequency;
    Vector4 Steepness;
    Vector4 Speed;
    Vector4 DirectionAB;
    Vector4 DirectionCD;

    private void Awake()
    {
        var water = GetComponent<Renderer>();
        Amplitude = water.sharedMaterial.GetVector("_Amplitude");
        Frequency = water.sharedMaterial.GetVector("_Frequency");
        Steepness = water.sharedMaterial.GetVector("_Steepness");
        Speed = water.sharedMaterial.GetVector("_Speed");
        DirectionAB = water.sharedMaterial.GetVector("_DirectionAB");
        DirectionCD = water.sharedMaterial.GetVector("_DirectionCD");
    }
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
