using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
public class WaterRipple : MonoBehaviour {

    Vector4 Amplitude;
    Vector4 Frequency;
    Vector4 Steepness;
    Vector4 Speed;
    Vector4 DirectionAB;
    Vector4 DirectionCD;
    Texture2D WaterDisplacementTexture;
    Vector3[] WavePoints;
    Color[] Colors;
    Vector2[,] WaveAcceleration;
    float[] CutOutTextureGray;
    bool CutOutTextureInitialized;
    DateTime UpdateDateTime;
    double ThreadDeltaTime;

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
        Amplitude = water.sharedMaterial.GetVector("_Amplitude");
        Frequency = water.sharedMaterial.GetVector("_Frequency");
        Steepness = water.sharedMaterial.GetVector("_Steepness");
        Speed = water.sharedMaterial.GetVector("_Speed");
        DirectionAB = water.sharedMaterial.GetVector("_DirectionAB");
        DirectionCD = water.sharedMaterial.GetVector("_DirectionCD");
        InitRipple();
    }
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void InitRipple()
    {
        WaterDisplacementTexture = new Texture2D(ResolutionTexture, ResolutionTexture, TextureFormat.RGBA32, false);
        WaterDisplacementTexture.wrapMode = TextureWrapMode.Clamp;
        WaterDisplacementTexture.filterMode = FilterMode.Bilinear;
        Shader.SetGlobalTexture("_WaterDisplacementTexture", WaterDisplacementTexture);
        WavePoints = new Vector3[ResolutionTexture * ResolutionTexture];
        Colors = new Color[ResolutionTexture * ResolutionTexture];
        WaveAcceleration = new Vector2[ResolutionTexture, ResolutionTexture];
        for(int i = 0; i < ResolutionTexture * ResolutionTexture; i++)
        {
            Colors[i] = Color.black;
            WavePoints[i] = Vector3.zero;
        }
        for (int i = 0; i < ResolutionTexture; i++)
            for (int j = 0; j < ResolutionTexture; j++)
                WaveAcceleration[i, j] = Vector2.zero;
        if(!CutOutTexture)
        {
            //var scaleOfCutOutTexture = ScaleTexture(CutOutTexture, ResolutionTexture, ResolutionTexture);
            var scaleOfCutOutTexture = CutOutTexture;
            var colors = scaleOfCutOutTexture.GetPixels();
            CutOutTextureGray = new float[ResolutionTexture * ResolutionTexture];
            for (int i = 0; i < colors.Length; i++)
                //直接采用心理学灰度值公式
                //https://en.wikipedia.org/wiki/Grayscale
                CutOutTextureGray[i] = colors[i].r * 0.299f + colors[i].g * 0.587f + colors[i].b * 0.114f;
            CutOutTextureInitialized = true;
        }
    }

    void UpdateRipple()
    {
        UpdateDateTime = DateTime.UtcNow;
        ThreadDeltaTime = (DateTime.UtcNow - UpdateDateTime).TotalMilliseconds / 1000;
        var sleepTime = (int)(1000f / UpdateFPS - ThreadDeltaTime);
        if (sleepTime > 0)
            Thread.Sleep(sleepTime);
        //TODO
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
