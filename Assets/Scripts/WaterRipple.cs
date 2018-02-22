using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
public class WaterRipple : MonoBehaviour {

    Vector4 amplitude;
    Vector4 frequency;
    Vector4 steepness;
    Vector4 speed;
    Vector4 directionAB;
    Vector4 directionCD;
    Texture2D waterDisplacementTexture;
    Vector3[] wavePoints;
    Color[] colors;
    Vector2[,] waveAcceleration;
    float[] cutOutTextureGray;
    bool cutOutTextureInitialized;
    DateTime updateDateTime;
    double threadDeltaTime;
    Vector2 objectPos, projectorPosition;
    Vector3 scaleBounds;

    [Range(20, 200)]
    public int UpdateFPS = 60;
    //是否使用多线程计算
    public bool UseMultipleThread;
    //计算波纹纹理的分辨率
    public int ResolutionTexture = 128;
    //水波纹的阻尼
    public float Damping;
    [Range(0.0001f, 2)]
    public float SpreadSpeed = 1.5f;
    //是否模糊化处理水波
    public bool UseSmoothWaves;
    //无限水面时需要同时开启
    public bool UseProjectedWaves;
    public Texture2D CutOutTexture;

    private void Awake()
    {
        var water = GetComponent<Renderer>();
        amplitude = water.sharedMaterial.GetVector("_Amplitude");
        frequency = water.sharedMaterial.GetVector("_Frequency");
        steepness = water.sharedMaterial.GetVector("_Steepness");
        speed = water.sharedMaterial.GetVector("_Speed");
        directionAB = water.sharedMaterial.GetVector("_DirectionAB");
        directionCD = water.sharedMaterial.GetVector("_DirectionCD");
        scaleBounds = GetComponent<MeshRenderer>().bounds.size;
        InitRipple();
    }
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void InitRipple()
    {
        waterDisplacementTexture = new Texture2D(ResolutionTexture, ResolutionTexture, TextureFormat.RGBA32, false);
        waterDisplacementTexture.wrapMode = TextureWrapMode.Clamp;
        waterDisplacementTexture.filterMode = FilterMode.Bilinear;
        Shader.SetGlobalTexture("_WaterDisplacementTexture", waterDisplacementTexture);
        wavePoints = new Vector3[ResolutionTexture * ResolutionTexture];
        colors = new Color[ResolutionTexture * ResolutionTexture];
        waveAcceleration = new Vector2[ResolutionTexture, ResolutionTexture];
        for(int i = 0; i < ResolutionTexture * ResolutionTexture; i++)
        {
            colors[i] = Color.black;
            wavePoints[i] = Vector3.zero;
        }
        for (int i = 0; i < ResolutionTexture; i++)
            for (int j = 0; j < ResolutionTexture; j++)
                waveAcceleration[i, j] = Vector2.zero;
        if(!CutOutTexture)
        {
            //var scaleOfCutOutTexture = ScaleTexture(CutOutTexture, ResolutionTexture, ResolutionTexture);
            var scaleOfCutOutTexture = CutOutTexture;
            var colors = scaleOfCutOutTexture.GetPixels();
            cutOutTextureGray = new float[ResolutionTexture * ResolutionTexture];
            for (int i = 0; i < colors.Length; i++)
                //直接采用心理学灰度值公式
                //https://en.wikipedia.org/wiki/Grayscale
                cutOutTextureGray[i] = colors[i].r * 0.299f + colors[i].g * 0.587f + colors[i].b * 0.114f;
            cutOutTextureInitialized = true;
        }
    }

    void UpdateRipple()
    {
        updateDateTime = DateTime.UtcNow;
        threadDeltaTime = (DateTime.UtcNow - updateDateTime).TotalMilliseconds / 1000;
        var sleepTime = (int)(1000f / UpdateFPS - threadDeltaTime);
        if (sleepTime > 0)
            Thread.Sleep(sleepTime);
        //TODO
    }
    void CalculateRippleTexture()
    {
        if (UseProjectedWaves)
            UpdateProjector();
        var length = wavePoints.Length;

    }

    void UpdateProjector()
    {
        var xOffset = (int)(ResolutionTexture * objectPos.x / scaleBounds.x - projectorPosition.x);
        var yOffset = (int)(ResolutionTexture * objectPos.y / scaleBounds.y - projectorPosition.y);
        projectorPosition.x += xOffset;
        projectorPosition.y += yOffset;
        if (xOffset == 0 && yOffset == 0)
            return;
        if (xOffset >= 0 && yOffset >= 0)
            for (int i = 1; i < ResolutionTexture; i++)
            {
                for (int j = 0; j < ResolutionTexture; j++)
                {
                    if (i + yOffset > 0 && i + yOffset < ResolutionTexture && j + xOffset > 0 && j + xOffset < ResolutionTexture)
                    {
                        waveAcceleration[j, i] = waveAcceleration[j + xOffset, i + yOffset];
                        wavePoints[j + (i * ResolutionTexture)] = wavePoints[j + xOffset + ((i + yOffset) * ResolutionTexture)];
                    }
                }
            }

    }

    private Texture2D ScaleTexture(Texture2D source, int width, int height)
    {
        Texture2D result = new Texture2D(source.width, source.height, TextureFormat.ARGB32, false);
        var pixels = source.GetPixels();
        result.SetPixels(pixels);
        result.Apply();
        return result;
    }
}
