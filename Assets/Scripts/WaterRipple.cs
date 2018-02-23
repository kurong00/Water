﻿using System;
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
    Transform oldTransform;

    [Range(20, 200)]
    public int updateFPS = 60;
    //是否使用多线程计算
    public bool useMultipleThread;
    //计算波纹纹理的分辨率
    public int resolutionTexture = 128;
    //水波纹的阻尼
    public float damping;
    [Range(0.0001f, 2)]
    public float spreadSpeed = 1.5f;
    //是否模糊化处理水波
    public bool useSmoothWaves;
    //无限水面时需要同时开启
    public bool useProjectedWaves;
    public Texture2D cutOutTexture;

    void Awake()
    {
        oldTransform = transform;
        var water = GetComponent<Renderer>();
        amplitude = water.sharedMaterial.GetVector("_Amplitude");
        frequency = water.sharedMaterial.GetVector("_Frequency");
        steepness = water.sharedMaterial.GetVector("_Steepness");
        speed = water.sharedMaterial.GetVector("_Speed");
        directionAB = water.sharedMaterial.GetVector("_DirectionAB");
        directionCD = water.sharedMaterial.GetVector("_DirectionCD");
        scaleBounds = GetComponent<MeshRenderer>().bounds.size;
        InitRipple();
        if (useMultipleThread)
        {
            currentThread = new Thread(UpdateRipple);
            currentThread.Start();
        }

    }

    void Start () {
		
	}

    private void FixedUpdate()
    {
        if (!useMultipleThread)
            CalculateRippleTexture();
        waterDisplacementTexture.SetPixels(colors);
        waterDisplacementTexture.Apply(false);
    }

    void Update () {
        objectPos = new Vector2(oldTransform.position.x, oldTransform.position.y);
	}

    void InitRipple()
    {
        waterDisplacementTexture = new Texture2D(resolutionTexture, resolutionTexture, TextureFormat.RGBA32, false);
        waterDisplacementTexture.wrapMode = TextureWrapMode.Clamp;
        waterDisplacementTexture.filterMode = FilterMode.Bilinear;
        Shader.SetGlobalTexture("_WaterDisplacementTexture", waterDisplacementTexture);
        wavePoints = new Vector3[resolutionTexture * resolutionTexture];
        colors = new Color[resolutionTexture * resolutionTexture];
        waveAcceleration = new Vector2[resolutionTexture, resolutionTexture];
        for(int i = 0; i < resolutionTexture * resolutionTexture; i++)
        {
            colors[i] = Color.black;
            wavePoints[i] = Vector3.zero;
        }
        for (int i = 0; i < resolutionTexture; i++)
            for (int j = 0; j < resolutionTexture; j++)
                waveAcceleration[i, j] = Vector2.zero;
        if(!cutOutTexture)
        {
            //var scaleOfCutOutTexture = ScaleTexture(cutOutTexture, resolutionTexture, resolutionTexture);
            var scaleOfCutOutTexture = cutOutTexture;
            var colors = scaleOfCutOutTexture.GetPixels();
            cutOutTextureGray = new float[resolutionTexture * resolutionTexture];
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
        var sleepTime = (int)(1000f / updateFPS - threadDeltaTime);
        if (sleepTime > 0)
            Thread.Sleep(sleepTime);
        //TODO
    }
    void CalculateRippleTexture()
    {
        if (useProjectedWaves)
            UpdateProjector();
        int x, y;
        float waveSmooth,waveLines;
        for(int i = 0; i < wavePoints.Length; i++)
        {
            if (i >= resolutionTexture + 1 && 
                i < wavePoints.Length - 1 - resolutionTexture&&i%resolutionTexture>0)
            {
                x = i % resolutionTexture;
                y = i / resolutionTexture;
                waveSmooth = (wavePoints[i - 1].y + wavePoints[i + 1].y +
                    wavePoints[i + resolutionTexture].y + wavePoints[i - resolutionTexture].y) / 4;
                waveAcceleration[x, y].y += waveSmooth - waveAcceleration[x, y].x;
            }
        }
        float currentSpeed = spreadSpeed;
        if (!useMultipleThread)
            currentSpeed *= Time.fixedDeltaTime * updateFPS;
        for(int i = 0; i < resolutionTexture; i++)
        {
            for(int j = 0; j < resolutionTexture; j++)
            {
                waveAcceleration[j, i].x += waveAcceleration[j, i].y * currentSpeed;
                if(cutOutTextureInitialized)
                    waveAcceleration[j, i].x *= cutOutTextureGray[j + (i * resolutionTexture)];
                waveAcceleration[j, i].y *= 1-damping;
                waveAcceleration[j, i].x *= 1-damping;
                wavePoints[j + (i * resolutionTexture)].y = waveAcceleration[j, i].x;
                if (!useSmoothWaves)
                {
                    waveLines = waveAcceleration[j, i].x * textureColorMultiplier;
                    if (waveLines >= 0)
                        colors[j + (i * resolutionTexture)].r = waveLines;
                    else
                        colors[j + (i * resolutionTexture)].g = -waveLines;
                }
            }
        }
        if (useSmoothWaves)
        {
            for (int i = 2; i < resolutionTexture-2; i++)
            {
                for (int j = 2; j < resolutionTexture-2; j++)
                {
                    waveLines = (wavePoints[j + (i * resolutionTexture) - 2].y * 0.2f
                            + wavePoints[j + (i * resolutionTexture) - 1].y * 0.4f
                            + wavePoints[j + (i * resolutionTexture)].y * 0.6f
                            + wavePoints[j + (i * resolutionTexture) + 1].y * 0.4f
                            + wavePoints[j + (i * resolutionTexture) + 2].y * 0.2f
                            ) / 1.6f * textureColorMultiplier;
                    if (waveLines >= 0)
                        colors[j + (i * resolutionTexture)].r = waveLines;
                    else
                        colors[j + (i * resolutionTexture)].g = -waveLines;
                }
            }
            for (int j = 2; j < resolutionTexture - 2; j++)
            {
                for (int i = 2; i < resolutionTexture - 2; i++)
                {
                    waveLines = (wavePoints[i + (j * resolutionTexture) - 2].y * 0.2f
                            + wavePoints[i + (j * resolutionTexture) - 1].y * 0.4f
                            + wavePoints[i + (j * resolutionTexture)].y * 0.6f
                            + wavePoints[i + (j * resolutionTexture) + 1].y * 0.4f
                            + wavePoints[i + (j * resolutionTexture) + 2].y * 0.2f
                            ) / 1.6f * textureColorMultiplier;
                    if (waveLines >= 0)
                        colors[i + (j * resolutionTexture)].r = waveLines;
                    else
                        colors[i + (j * resolutionTexture)].g = -waveLines;
                }
            }
        }
    }

    void UpdateProjector()
    {
        var xOffset = (int)(resolutionTexture * objectPos.x / scaleBounds.x - projectorPosition.x);
        var yOffset = (int)(resolutionTexture * objectPos.y / scaleBounds.y - projectorPosition.y);
        projectorPosition.x += xOffset;
        projectorPosition.y += yOffset;
        if (xOffset == 0 && yOffset == 0)
            return;
        if (xOffset >= 0 && yOffset >= 0)
        {
            for (int i = 1; i < resolutionTexture; i++)
            {
                for (int j = 0; j < resolutionTexture; j++)
                {
                    if (i + yOffset > 0 && i + yOffset < resolutionTexture && j + xOffset > 0 && j + xOffset < resolutionTexture)
                    {
                        waveAcceleration[j, i] = waveAcceleration[j + xOffset, i + yOffset];
                        wavePoints[j + (i * resolutionTexture)] = wavePoints[j + xOffset + ((i + yOffset) * resolutionTexture)];
                    }
                }
            }
        }
        if (xOffset < 0 && yOffset >= 0)
        {
            for (int i = 0; i < resolutionTexture; i++)
            {
                for (int j = resolutionTexture - 1; j >= 0; j--)
                {
                    if (i + yOffset > 0 && i + yOffset < resolutionTexture && j + xOffset > 0 && j + xOffset < resolutionTexture)
                    {
                        waveAcceleration[j, i] = waveAcceleration[j + xOffset, i + yOffset];
                        wavePoints[j + (i * resolutionTexture)] = wavePoints[j + xOffset + ((i + yOffset) * resolutionTexture)];
                    }
                }
            }
        }

        if (xOffset >= 0 && yOffset < 0)
        {
            for (int i = resolutionTexture - 1; i >= 0; i--)
            {
                for (int j = 0; j < resolutionTexture; j++)
                {
                    if (i + yOffset > 0 && i + yOffset < resolutionTexture && j + xOffset > 0 && j + xOffset < resolutionTexture)
                    {
                        waveAcceleration[j, i] = waveAcceleration[j + xOffset, i + yOffset];
                        wavePoints[j + (i * resolutionTexture)] = wavePoints[j + xOffset + ((i + yOffset) * resolutionTexture)];
                    }
                }
            }
        }

        if (xOffset < 0 && yOffset < 0)
        {
            for (int i = resolutionTexture - 1; i >= 0; i--)
            {
                for (int j = resolutionTexture - 1; j >= 0; j--)
                {
                    if (i + yOffset > 0 && i + yOffset < resolutionTexture && j + xOffset > 0 && j + xOffset < resolutionTexture)
                    {
                        waveAcceleration[j, i] = waveAcceleration[j + xOffset, i + yOffset];
                        wavePoints[j + (i * resolutionTexture)] = wavePoints[j + xOffset + ((i + yOffset) * resolutionTexture)];

                    }
                }
            }
        }
        for(int i = 0; i < resolutionTexture; i++)
        {
            waveAcceleration[0, i] = Vector2.zero;
            waveAcceleration[resolutionTexture - 1, i] = Vector2.zero;
            wavePoints[(i * resolutionTexture)] = Vector2.zero;
            wavePoints[resolutionTexture - 1 + (i * resolutionTexture)] = Vector2.zero;

            waveAcceleration[i, 0] = Vector2.zero;
            waveAcceleration[i, resolutionTexture - 1] = Vector2.zero;
            wavePoints[i + resolutionTexture - 1] = Vector2.zero;
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
        position.x += scaleBounds.x / 2 - oldTransform.position.x;
        position.z += scaleBounds.z / 2 - oldTransform.position.z;
        position.x /= scaleBounds.x;
        position.z /= scaleBounds.z;
        position.x *= resolutionTexture;
        position.z *= resolutionTexture;
        SetRippleTexture((int)position.x, (int)position.z, velocity);
    }

    private void SetRippleTexture(int x, int y, float strength)
    {
        strength /= 100f;
        if (x >= 2 && x < resolutionTexture - 2 && y >= 2 && y < resolutionTexture - 2)
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

            if (x >= 3 && x < resolutionTexture - 3 && y >= 3 && y < resolutionTexture - 3)
            {
                waveAcceleration[x + 2, y].y -= strength * 0.5f;
                waveAcceleration[x - 2, y].y -= strength * 0.5f;
                waveAcceleration[x, y + 2].y -= strength * 0.5f;
                waveAcceleration[x, y - 2].y -= strength * 0.5f;
            }
        }
    }
}
