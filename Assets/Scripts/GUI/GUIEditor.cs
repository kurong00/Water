using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GUIEditor : MonoBehaviour
{

    public float UpdateInterval = 0.5F;
    public int MaxScenes = 3;

    public bool IsMobileScene;
    public Light Sun;
    public GameObject SunTransform;
    public GameObject Boat;
    public GameObject water1;
    public GameObject water2;
    public float angle = 130;

    bool canUpdateTestMaterial;
    GameObject cam;
    GUIStyle guiStyleHeader = new GUIStyle();
    Material currentWaterMaterial, causticMaterial;
    GameObject currentWater;
    float transparent, fadeBlend, refl, foam;
    float waterWaveScaleXZ = 1;
    Vector4 waterDirection, causticDirection, foamDirection, ABDirection, CDDirection;
    float direction = 1;
    Color reflectionColor;
    Vector3 oldCausticScale;
    float oldTextureScale, oldWaveScale;
    GameObject caustic;
    float startSunIntencity;

    GUIStyle buttonStyle = null;


    void Start()
    {
        guiStyleHeader.fontSize = 18;
        guiStyleHeader.normal.textColor = new Color(1, 0, 0);
        UpdateCurrentWater();
    }

    void UpdateCurrentWater()
    {
        if (Boat != null)
        {
            Boat.SetActive(false);
            Boat.SetActive(true);
        }
        startSunIntencity = Sun.intensity;
        currentWater = GameObject.Find("Water");
        currentWaterMaterial = currentWater.GetComponent<Renderer>().material;

        refl = currentWaterMaterial.GetColor("_ReflectionColor").r;
        if (!IsMobileScene) transparent = currentWaterMaterial.GetFloat("_TransperentDepth");
        if (!IsMobileScene) fadeBlend = currentWaterMaterial.GetFloat("_FadeDepth");
        foam = currentWaterMaterial.GetFloat("_DistortionNormal");
        oldTextureScale = currentWaterMaterial.GetFloat("_TexturesScale");
        oldWaveScale = currentWaterMaterial.GetFloat("_WaveScale");
        var infiniteMesh = GameObject.Find("InfiniteWaterMesh");
        if (infiniteMesh != null) infiniteMesh.GetComponent<Renderer>().material = currentWaterMaterial;
        var projectorCausticScale = GameObject.Find("ProjectorCausticScale");
        if (projectorCausticScale != null) oldCausticScale = projectorCausticScale.transform.localScale;

        caustic = GameObject.Find("Caustic");
        if (IsMobileScene) caustic.SetActive(false);

        if (!IsMobileScene) causticMaterial = caustic.GetComponent<Projector>().material;
        waterDirection = currentWaterMaterial.GetVector("_Direction");
        if (!IsMobileScene) foamDirection = currentWaterMaterial.GetVector("_FoamDirection");
        if (!IsMobileScene) causticDirection = causticMaterial.GetVector("_CausticDirection");
        ABDirection = currentWaterMaterial.GetVector("_DirectionAB");
        CDDirection = currentWaterMaterial.GetVector("_DirectionCD");
    }

    void OnGUI()
    {
        buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.normal.textColor = Color.white;
        buttonStyle.fontSize = 20;
        buttonStyle.padding = new RectOffset(5, 5, 5, 5);
        if (IsMobileScene)
            GUIMobile();
        else
            GUIPC();
    }

    void GUIMobile()
    {
        if (currentWaterMaterial == null)
            return;

        if (GUI.Button(new Rect(10, 35, 150, 40), "On/Off Ripples", buttonStyle))
        {
            caustic.SetActive(true);
            water1.SetActive(!water1.activeSelf);
            water2.SetActive(!water2.activeSelf);
            caustic = GameObject.Find("Caustic");
            if (IsMobileScene)
                caustic.SetActive(false);
        }

        if (GUI.Button(new Rect(10, 190, 150, 40), "On/Off caustic", buttonStyle))
        {
            caustic.SetActive(!caustic.activeSelf);
        }
        var labelTexColor = new GUIStyle();
        labelTexColor.normal.textColor = Color.white;
        labelTexColor.fontSize = 25;

        angle = GUI.HorizontalSlider(new Rect(10, 102, 120, 15), angle, 30, 240);
        GUI.Label(new Rect(140, 100, 30, 30), "Day Time", labelTexColor);
        var intensity = Mathf.Sin((angle - 60) / 50);
        Sun.intensity = Mathf.Clamp01(intensity) * startSunIntencity + 0.05f;
        SunTransform.transform.rotation = Quaternion.Euler(0, 0, angle);


        refl = GUI.HorizontalSlider(new Rect(10, 122, 120, 15), refl, 0, 1);
        reflectionColor = new Color(refl, refl, refl, 1);
        GUI.Label(new Rect(140, 120, 30, 30), "Reflection", labelTexColor);
        currentWaterMaterial.SetColor("_ReflectionColor", reflectionColor);

        foam = GUI.HorizontalSlider(new Rect(10, 142, 120, 15), foam, 0, 700);
        GUI.Label(new Rect(140, 140, 30, 30), "Foam", labelTexColor);
        currentWaterMaterial.SetFloat("_FoamIntensity", foam);

        waterWaveScaleXZ = GUI.HorizontalSlider(new Rect(10, 162, 120, 15), waterWaveScaleXZ, 0.3f, 3);
        GUI.Label(new Rect(140, 160, 30, 30), "Scale", labelTexColor);

        currentWaterMaterial.SetFloat("_WaveScale", oldWaveScale * waterWaveScaleXZ);
        currentWaterMaterial.SetFloat("_TexturesScale", oldTextureScale * waterWaveScaleXZ);

    }

    void GUIPC()
    {

        if (currentWaterMaterial == null)
            return;
        if (GUI.Button(new Rect(10, 35, 200, 50), "Change Scene ", buttonStyle))
        {
            SceneManager.LoadScene((SceneManager.GetActiveScene().buildIndex + 1) %
                SceneManager.sceneCountInBuildSettings);
            UpdateCurrentWater();
        }

        var labelTexColor = new GUIStyle();
        labelTexColor.normal.textColor = Color.white;
        labelTexColor.fontSize = 20;

        angle = GUI.HorizontalSlider(new Rect(10, 102, 120, 15), angle, 30, 240);
        GUI.Label(new Rect(140, 100, 30, 30), "Day Time", labelTexColor);
        var intensity = Mathf.Sin((angle - 60) / 50);
        Sun.intensity = Mathf.Clamp01(intensity) * startSunIntencity + 0.05f;
        SunTransform.transform.rotation = Quaternion.Euler(0, 0, angle);

        transparent = GUI.HorizontalSlider(new Rect(10, 132, 120, 15), transparent, 0, 1);
        GUI.Label(new Rect(140, 130, 30, 30), "Depth Transperent", labelTexColor);
        currentWaterMaterial.SetFloat("_TransperentDepth", transparent);

        fadeBlend = GUI.HorizontalSlider(new Rect(10, 162, 120, 15), fadeBlend, 0, 1);
        GUI.Label(new Rect(140, 160, 30, 30), "Fade Depth", labelTexColor);
        currentWaterMaterial.SetFloat("_FadeDepth", fadeBlend);

        refl = GUI.HorizontalSlider(new Rect(10, 192, 120, 15), refl, 0, 1);
        reflectionColor = new Color(refl, refl, refl, 1);
        GUI.Label(new Rect(140, 190, 30, 30), "Reflection", labelTexColor);
        currentWaterMaterial.SetColor("_ReflectionColor", reflectionColor);

        foam = GUI.HorizontalSlider(new Rect(10, 222, 120, 15), foam, 0, 700);
        GUI.Label(new Rect(140, 220, 30, 30), "Foam", labelTexColor);
        currentWaterMaterial.SetFloat("_FoamIntensity", foam);

        waterWaveScaleXZ = GUI.HorizontalSlider(new Rect(10, 252, 120, 15), waterWaveScaleXZ, 0.3f, 3);
        GUI.Label(new Rect(140, 250, 30, 30), "Scale", labelTexColor);

        currentWaterMaterial.SetFloat("_WaveScale", oldWaveScale * waterWaveScaleXZ);
        currentWaterMaterial.SetFloat("_TexturesScale", oldTextureScale * waterWaveScaleXZ);

        var projectorCausticScale = GameObject.Find("ProjectorCausticScale");
        var currentCausticScale = oldCausticScale * waterWaveScaleXZ;
        if ((projectorCausticScale.transform.localScale - currentCausticScale).magnitude > 0.01)
        {
            projectorCausticScale.transform.localScale = currentCausticScale;
            caustic.GetComponent<ProjectorMatrix>().UpdateMatrix();
        }

        direction = GUI.HorizontalSlider(new Rect(10, 282, 120, 15), direction, 1, -1);
        GUI.Label(new Rect(140, 280, 30, 30), "Direction", labelTexColor);
        currentWaterMaterial.SetVector("_Direction", waterDirection * direction);
        currentWaterMaterial.SetVector("_FoamDirection", foamDirection * direction);
        causticMaterial.SetVector("_CausticDirection", causticDirection * direction);
        currentWaterMaterial.SetVector("_DirectionAB", ABDirection * direction);
        currentWaterMaterial.SetVector("_DirectionCD", CDDirection * direction);
    }

    void OnDestroy()
    {
        if (!IsMobileScene) causticMaterial.SetVector("_CausticDirection", causticDirection);
    }

    void OnSetColorMain(Color color)
    {
        currentWaterMaterial.SetColor("_Color", color);
    }

    void OnGetColorMain(ColorPicker picker)
    {
        if (picker != null && currentWaterMaterial != null)
            picker.NotifyColor(currentWaterMaterial.GetColor("_Color"));
    }

    void OnSetColorFade(Color color)
    {
        currentWaterMaterial.SetColor("_FadeColor", color);
    }

    void OnGetColorFade(ColorPicker picker)
    {
        if (picker != null && currentWaterMaterial != null)
            picker.NotifyColor(currentWaterMaterial.GetColor("_FadeColor"));
    }

}