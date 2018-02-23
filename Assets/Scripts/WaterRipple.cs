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
    Thread currentThread;
    float textureColorMultiplier = 10;

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

    void Awake()
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
        if (UseMultipleThread)
        {
            currentThread = new Thread(UpdateRipple);
            currentThread.Start();
        }

    }

    void Start () {
		
	}

    private void FixedUpdate()
    {
        if (!UseMultipleThread)
            CalculateRippleTexture();
        waterDisplacementTexture.SetPixels(colors);
        waterDisplacementTexture.Apply(false);
    }

    void Update () {
        objectPos = new Vector2(transform.position.x, transform.position.y);
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
        int x, y;
        float waveSmooth,waveLines;
        for(int i = 0; i < wavePoints.Length; i++)
        {
            if (i >= ResolutionTexture + 1 && 
                i < wavePoints.Length - 1 - ResolutionTexture&&i%ResolutionTexture>0)
            {
                x = i % ResolutionTexture;
                y = i / ResolutionTexture;
                waveSmooth = (wavePoints[i - 1].y + wavePoints[i + 1].y +
                    wavePoints[i + ResolutionTexture].y + wavePoints[i - ResolutionTexture].y) / 4;
                waveAcceleration[x, y].y += waveSmooth - waveAcceleration[x, y].x;
            }
        }
        float currentSpeed = SpreadSpeed;
        if (!UseMultipleThread)
            currentSpeed *= Time.fixedDeltaTime * UpdateFPS;
        for(int i = 0; i < ResolutionTexture; i++)
        {
            for(int j = 0; j < ResolutionTexture; j++)
            {
                waveAcceleration[j, i].x += waveAcceleration[j, i].y * currentSpeed;
                if(cutOutTextureInitialized)
                    waveAcceleration[j, i].x *= cutOutTextureGray[j + (i * ResolutionTexture)];
                waveAcceleration[j, i].y *= 1-Damping;
                waveAcceleration[j, i].x *= 1-Damping;
                wavePoints[j + (i * ResolutionTexture)].y = waveAcceleration[j, i].x;
                if (!UseSmoothWaves)
                {
                    waveLines = waveAcceleration[j, i].x * textureColorMultiplier;
                    if (waveLines >= 0)
                        colors[j + (i * ResolutionTexture)].r = waveLines;
                    else
                        colors[j + (i * ResolutionTexture)].g = -waveLines;
                }
            }
        }
        if (UseSmoothWaves)
        {
            for (int i = 2; i < ResolutionTexture-2; i++)
            {
                for (int j = 2; j < ResolutionTexture-2; j++)
                {
                    waveLines = (wavePoints[j + (i * ResolutionTexture) - 2].y * 0.2f
                            + wavePoints[j + (i * ResolutionTexture) - 1].y * 0.4f
                            + wavePoints[j + (i * ResolutionTexture)].y * 0.6f
                            + wavePoints[j + (i * ResolutionTexture) + 1].y * 0.4f
                            + wavePoints[j + (i * ResolutionTexture) + 2].y * 0.2f
                            ) / 1.6f * textureColorMultiplier;
                    if (waveLines >= 0)
                        colors[j + (i * ResolutionTexture)].r = waveLines;
                    else
                        colors[j + (i * ResolutionTexture)].g = -waveLines;
                }
            }
            for (int j = 2; j < ResolutionTexture - 2; j++)
            {
                for (int i = 2; i < ResolutionTexture - 2; i++)
                {
                    waveLines = (wavePoints[i + (j * ResolutionTexture) - 2].y * 0.2f
                            + wavePoints[i + (j * ResolutionTexture) - 1].y * 0.4f
                            + wavePoints[i + (j * ResolutionTexture)].y * 0.6f
                            + wavePoints[i + (j * ResolutionTexture) + 1].y * 0.4f
                            + wavePoints[i + (j * ResolutionTexture) + 2].y * 0.2f
                            ) / 1.6f * textureColorMultiplier;
                    if (waveLines >= 0)
                        colors[i + (j * ResolutionTexture)].r = waveLines;
                    else
                        colors[i + (j * ResolutionTexture)].g = -waveLines;
                }
            }
        }
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
        {
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
        if (xOffset < 0 && yOffset >= 0)
        {
            for (int i = 0; i < ResolutionTexture; i++)
            {
                for (int j = ResolutionTexture - 1; j >= 0; j--)
                {
                    if (i + yOffset > 0 && i + yOffset < ResolutionTexture && j + xOffset > 0 && j + xOffset < ResolutionTexture)
                    {
                        waveAcceleration[j, i] = waveAcceleration[j + xOffset, i + yOffset];
                        wavePoints[j + (i * ResolutionTexture)] = wavePoints[j + xOffset + ((i + yOffset) * ResolutionTexture)];
                    }
                }
            }
        }

        if (xOffset >= 0 && yOffset < 0)
        {
            for (int i = ResolutionTexture - 1; i >= 0; i--)
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

        if (xOffset < 0 && yOffset < 0)
        {
            for (int i = ResolutionTexture - 1; i >= 0; i--)
            {
                for (int j = ResolutionTexture - 1; j >= 0; j--)
                {
                    if (i + yOffset > 0 && i + yOffset < ResolutionTexture && j + xOffset > 0 && j + xOffset < ResolutionTexture)
                    {
                        waveAcceleration[j, i] = waveAcceleration[j + xOffset, i + yOffset];
                        wavePoints[j + (i * ResolutionTexture)] = wavePoints[j + xOffset + ((i + yOffset) * ResolutionTexture)];

                    }
                }
            }
        }
        for(int i = 0; i < ResolutionTexture; i++)
        {
            waveAcceleration[0, i] = Vector2.zero;
            waveAcceleration[ResolutionTexture - 1, i] = Vector2.zero;
            wavePoints[(i * ResolutionTexture)] = Vector2.zero;
            wavePoints[ResolutionTexture - 1 + (i * ResolutionTexture)] = Vector2.zero;

            waveAcceleration[i, 0] = Vector2.zero;
            waveAcceleration[i, ResolutionTexture - 1] = Vector2.zero;
            wavePoints[i + ResolutionTexture - 1] = Vector2.zero;
            wavePoints[i] = Vector2.zero;
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


    //Tools 
    public Vector3 GetOffsetByPosition(Vector3 position)
    {

        //TODO
        return Vector3.zero;
    }

    public void CreateRippleByPosition(Vector3 position, float velocity)
    {
        position.x += scaleBounds.x / 2 - transform.position.x;
        position.z += scaleBounds.z / 2 - transform.position.z;
        position.x /= scaleBounds.x;
        position.z /= scaleBounds.z;
        position.x *= ResolutionTexture;
        position.z *= ResolutionTexture;
        SetRippleTexture((int)position.x, (int)position.z, velocity);
    }

    private void SetRippleTexture(int x, int y, float strength)
    {
        strength /= 100f;
        if (x >= 2 && x < ResolutionTexture - 2 && y >= 2 && y < ResolutionTexture - 2)
        {
            waveAcceleration[x, y].y -= strength;
            waveAcceleration[x + 1, y].y -= strength * 0.8f;
            waveAcceleration[x - 1, y].y -= strength * 0.8f;
            waveAcceleration[x, y + 1].y -= strength * 0.8f;
            waveAcceleration[x, y - 1].y -= strength * 0.8f;
            waveAcceleration[x + 1, y + 1].y -= strength * 0.7f;
            waveAcceleration[x + 1, y - 1].y -= strength * 0.7f;
            waveAcceleration[x - 1, y + 1].y -= strength * 0.7f;
            waveAcceleration[x - 1, y - 1].y -= strength * 0.7f;

            if (x >= 3 && x < ResolutionTexture - 3 && y >= 3 && y < ResolutionTexture - 3)
            {
                waveAcceleration[x + 2, y].y -= strength * 0.5f;
                waveAcceleration[x - 2, y].y -= strength * 0.5f;
                waveAcceleration[x, y + 2].y -= strength * 0.5f;
                waveAcceleration[x, y - 2].y -= strength * 0.5f;
            }
        }
    }
}
