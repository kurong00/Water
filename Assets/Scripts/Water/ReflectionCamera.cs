using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReflectionCamera : MonoBehaviour {
    //Camera一些标准参数
    public float ClipPlaneOffset = 0.07f;
    public LayerMask CullingMask = 1;
    public bool HDR;
    public bool OcclusionCulling = true;
    public RenderingPath RenderingPath;

    //Camera额外附加的一些参数
    [Range(0,1)]
    public float TextureScale = 1f;//反射纹理贴图的分辨率，scale越大，质量越高，计算速度也越慢
    //用于优化
    public RenderTextureFormat RenderTextureFormat;
    //纹理过滤
    public FilterMode FilterMode = FilterMode.Point;

    //当时True时下面两个FPS的数值不做处理
    public bool UseRealtimeUpdate;
    public int FPSWhenMoveCamera = 40;
    public int FPSWhenStaticCamera = 20;


    RenderTexture renderTexture;
    GameObject go;
    Vector3 oldPosition;
    Quaternion oldRotation;
    Transform instanceCameraTransform;
    int frameCountWhenCameraIsStatic;
    bool canUpdateCamera, isStaticUpdate;
    WaitForSeconds fpsMove, fpsStatic;
    const int DropedFrames = 50;
    Camera currentCamera;
    Camera reflectionCamera;

    private void OnEnable()
    {
        Shader.EnableKeyword("editor_off");
        Shader.EnableKeyword("cubeMap_off");
        currentCamera = Camera.main;
        fpsMove = new WaitForSeconds(1.0f / FPSWhenMoveCamera);
        fpsStatic = new WaitForSeconds(1.0f / FPSWhenStaticCamera);
        if (!UseRealtimeUpdate)
        {
            StartCoroutine(RepeatCameraMove());
            StartCoroutine(RepeatCameraStatic());
        }
        else canUpdateCamera = true;
    }

    private IEnumerator RepeatCameraMove()
    {
        while (true)
        {
            if (!isStaticUpdate)
                canUpdateCamera = true;
            yield return fpsMove;
        }
    }

    private IEnumerator RepeatCameraStatic()
    {
        while (true)
        {
            if (isStaticUpdate)
                canUpdateCamera = true;
            yield return fpsStatic;
        }
    }

    private void OnBecameVisible()
    {
        if (go != null)
            go.SetActive(true);
    }
    
    private void Update()
    {
        Vector3 pos = transform.position;
        Vector3 normal = transform.up;
        float dot = -Vector3.Dot(normal, pos) - ClipPlaneOffset;
        Vector4 reflectionPlane = new Vector4(normal.x, normal.y, normal.z, dot);

        Matrix4x4 reflection = Matrix4x4.zero;
        CalculateReflectionMatrix(ref reflection, reflectionPlane);
        Vector3 oldpos = currentCamera.transform.position;
        Vector3 newpos = reflection.MultiplyPoint(oldpos);
        if (go == null)
        {
            renderTexture = new RenderTexture((int)(Screen.width * TextureScale), 
                (int)(Screen.height * TextureScale), 16, RenderTextureFormat);
            renderTexture.DiscardContents();
            go = new GameObject("Water Reflect Camera");
            reflectionCamera = go.AddComponent<Camera>();
            reflectionCamera.depth = currentCamera.depth - 1;
            reflectionCamera.renderingPath = RenderingPath;
            reflectionCamera.depthTextureMode = DepthTextureMode.None;
            go.transform.position = transform.position;
            go.transform.rotation = transform.rotation;
            reflectionCamera.cullingMask = CullingMask;
            reflectionCamera.targetTexture = renderTexture;
            reflectionCamera.allowHDR = HDR;
            reflectionCamera.useOcclusionCulling = OcclusionCulling;
            Shader.SetGlobalTexture("_ReflectionTex", renderTexture);
            instanceCameraTransform = reflectionCamera.transform;
        }
        reflectionCamera.worldToCameraMatrix = currentCamera.worldToCameraMatrix * reflection;
        Vector4 clipPlane = CameraSpacePlane(reflectionCamera, pos, normal, 1.0f);
        reflectionCamera.projectionMatrix = currentCamera.CalculateObliqueMatrix(clipPlane);
        GL.invertCulling = true;
        go.transform.position = newpos;
        Vector3 euler = currentCamera.transform.eulerAngles;
        go.transform.eulerAngles = new Vector3(-euler.x, euler.y, euler.z);
        UpdateCameraPosition();
        GL.invertCulling = false;
    }

    void UpdateCameraPosition()
    {
        if (reflectionCamera == null)
            return;
        if (Vector3.SqrMagnitude(instanceCameraTransform.position - oldPosition) <= 0.00001f 
            && instanceCameraTransform.rotation == oldRotation)
        {
            ++frameCountWhenCameraIsStatic;
            if (frameCountWhenCameraIsStatic >= DropedFrames)
                isStaticUpdate = true;
        }
        else
        {
            frameCountWhenCameraIsStatic = 0;
            isStaticUpdate = false;
        }
        oldPosition = instanceCameraTransform.position;
        oldRotation = instanceCameraTransform.rotation;
        if (canUpdateCamera)
        {
            reflectionCamera.enabled = true;
            if (!UseRealtimeUpdate) canUpdateCamera = false;
        }
        else if (reflectionCamera.enabled)
            reflectionCamera.enabled = false;
    }

    static void CalculateReflectionMatrix(ref Matrix4x4 matrix, Vector4 plane)
    {
        matrix.m00 = (1F - 2F * plane[0] * plane[0]);
        matrix.m01 = (-2F * plane[0] * plane[1]);
        matrix.m02 = (-2F * plane[0] * plane[2]);
        matrix.m03 = (-2F * plane[3] * plane[0]);

        matrix.m10 = (-2F * plane[1] * plane[0]);
        matrix.m11 = (1F - 2F * plane[1] * plane[1]);
        matrix.m12 = (-2F * plane[1] * plane[2]);
        matrix.m13 = (-2F * plane[3] * plane[1]);

        matrix.m20 = (-2F * plane[2] * plane[0]);
        matrix.m21 = (-2F * plane[2] * plane[1]);
        matrix.m22 = (1F - 2F * plane[2] * plane[2]);
        matrix.m23 = (-2F * plane[3] * plane[2]);

        matrix.m30 = 0F;
        matrix.m31 = 0F;
        matrix.m32 = 0F;
        matrix.m33 = 1F;
    }

    private Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float sideSign)
    {
        Vector3 offsetPos = pos + normal * ClipPlaneOffset;
        Matrix4x4 m = cam.worldToCameraMatrix;
        Vector3 cpos = m.MultiplyPoint(offsetPos);
        Vector3 cnormal = m.MultiplyVector(normal).normalized * sideSign;
        return new Vector4(cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos, cnormal));
    }

    private void OnDisable()
    {
        ClearCamera();
        Shader.DisableKeyword("editor_off");
        Shader.DisableKeyword("cubeMap_off");
    }

    private void ClearCamera()
    {
        if (go)
        {
            DestroyImmediate(go);
            go = null;
        }
        if (renderTexture)
        {
            DestroyImmediate(renderTexture);
            renderTexture = null;
        }
    }
}
