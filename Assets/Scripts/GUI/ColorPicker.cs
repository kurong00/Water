﻿using UnityEngine;
using System.Collections;

public class ColorPicker : MonoBehaviour {

	public Texture2D colorSpace;
	public Texture2D alphaGradient;
	public string Title = "Color Picker";
	public Vector2 startPos = new Vector2(20, 20);
	public GameObject receiver;
	public string colorSetFunctionName = "OnSetNewColor";
	public string colorGetFunctionName = "OnGetColor";
	public bool useExternalDrawer = false;
	public int drawOrder = 0;

    const int buttonWide = 60;
    const int buttonHigh = 40;
    const int textPadding = 60;
    Color TempColor; 
	Color SelectedColor;
	static ColorPicker activeColorPicker = null;

	enum ESTATE
	{
		Hidden,
		Showed,
		Showing,
		Hidding
	}; 
	ESTATE mState = ESTATE.Hidden;
	
	int sizeFull = 400;
	int sizeHidden = 40;
	float animTime = 0.25f;
	float dt = 0;

	float sizeCurrent = 0;
	float alphaGradientHeight = 16;

	GUIStyle titleStyle = null;
    GUIStyle textStyle = null;
    GUIStyle buttonStyle = null;
    Color textColor = Color.white;
	Texture2D txColorDisplay;

	string txtR, txtG, txtB, txtA;
	float valR, valG, valB, valA;

	void Start()
	{
		sizeCurrent = sizeHidden;
		txColorDisplay = new Texture2D(1, 1, TextureFormat.ARGB32, false);
		if(receiver)
		{
			receiver.SendMessage(colorGetFunctionName, this, SendMessageOptions.DontRequireReceiver);
		}
	}

	void OnGUI()
	{
		if(!useExternalDrawer)
		{
			DrawGUI();
		}
	}

	void UpdateColorSliders(bool isFocused)
	{
		if(!isFocused)
		{
			valR = TempColor.r;
			valG = TempColor.g;
			valB = TempColor.b;
			valA = TempColor.a;
		}
		else
		{
			SetColor(new Color(valR, valG, valB, valA));
		}
	}

	void UpdateColorEditFields(bool isFocused)
	{
		if(!isFocused)
		{
			txtR = (255 * TempColor.r).ToString();
			txtG = (255 * TempColor.g).ToString();
			txtB = (255 * TempColor.b).ToString();
			txtA = (255 * TempColor.a).ToString();
		}
		else
		{
			byte r = 0;
			byte g = 0;
			byte b = 0;
			byte a = 0;
			if(!string.IsNullOrEmpty(txtR)) {
				r = byte.Parse(txtR, System.Globalization.NumberStyles.Any);
			}
			if(!string.IsNullOrEmpty(txtG)) {
				g = byte.Parse(txtG, System.Globalization.NumberStyles.Any);
			}
			if(!string.IsNullOrEmpty(txtB)) {
				b = byte.Parse(txtB, System.Globalization.NumberStyles.Any);
			}
			if(!string.IsNullOrEmpty(txtA)) {
				a = byte.Parse(txtA, System.Globalization.NumberStyles.Any);
			}
			SetColor(new Color32(r, g, b, a));
		}
	}

	public void DrawGUI () 
	{
        if (titleStyle == null)
        {
            titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.normal.textColor = textColor;
            titleStyle.fontSize = 25;
        }
        if (textStyle == null)
        {
            textStyle = new GUIStyle(GUI.skin.textField);
            textStyle.normal.textColor = textColor;
            textStyle.fontSize = 25;
            textStyle.padding = new RectOffset(5,5,5,5);
        }
        if (buttonStyle == null)
        {
            buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.normal.textColor = textColor;
            buttonStyle.fontSize = 25;
            buttonStyle.padding = new RectOffset(5, 5, 5, 5);
        }
        Rect rectColorEdit = new Rect(startPos.x + sizeCurrent + 10, startPos.y + 30, 40, 140);
        Rect rectColorSlider = new Rect(startPos.x + sizeCurrent + 50, startPos.y + 30, 60, 140);

        GUI.Label(new Rect(startPos.x + sizeCurrent + 80, startPos.y, 200, 30), Title, titleStyle);

        GUI.DrawTexture(new Rect(startPos.x + sizeCurrent + 10, startPos.y, 60, 40), txColorDisplay);

        if (mState == ESTATE.Showed)
        {
            txtR = GUI.TextField(new Rect(startPos.x + sizeCurrent + 10, startPos.y + textPadding * 1, buttonWide,buttonHigh), txtR, 3, textStyle);
            txtG = GUI.TextField(new Rect(startPos.x + sizeCurrent + 10, startPos.y + textPadding * 2, buttonWide,buttonHigh), txtG, 3, textStyle);
            txtB = GUI.TextField(new Rect(startPos.x + sizeCurrent + 10, startPos.y + textPadding * 3, buttonWide,buttonHigh), txtB, 3, textStyle);
            txtA = GUI.TextField(new Rect(startPos.x + sizeCurrent + 10, startPos.y + textPadding * 4, buttonWide,buttonHigh), txtA, 3, textStyle);
            if (GUI.Button(new Rect(startPos.x + sizeCurrent + 10, startPos.y + textPadding * 5, 100, 60), "Apply", buttonStyle))
            {
                ApplyColor();
                SelectedColor = TempColor;
                if (receiver)
                {
                    receiver.SendMessage(colorSetFunctionName, SelectedColor, SendMessageOptions.DontRequireReceiver);
                }
            }

            GUIStyle labelStyleRGBA = new GUIStyle(GUI.skin.label);
            labelStyleRGBA.fontSize = 25;
            labelStyleRGBA.normal.textColor = Color.white;
            GUI.Label(new Rect(startPos.x + sizeCurrent + 100, startPos.y + textPadding * 1, 80, 80), "R", labelStyleRGBA);
            GUI.Label(new Rect(startPos.x + sizeCurrent + 100, startPos.y + textPadding * 2, 80, 80), "G", labelStyleRGBA);
            GUI.Label(new Rect(startPos.x + sizeCurrent + 100, startPos.y + textPadding * 3, 80, 80), "B", labelStyleRGBA);
            GUI.Label(new Rect(startPos.x + sizeCurrent + 100, startPos.y + textPadding * 4, 80, 80), "A", labelStyleRGBA);
        }

        if (mState == ESTATE.Showing)
        {
            sizeCurrent = Mathf.Lerp(sizeHidden, sizeFull, dt / animTime);
            if (dt / animTime > 1.0f)
            {
                mState = ESTATE.Showed;
            }
            dt += Time.deltaTime;
        }
        if (mState == ESTATE.Hidding)
        {
            sizeCurrent = Mathf.Lerp(sizeFull, sizeHidden, dt / animTime);
            if (dt / animTime > 1.0f)
            {
                mState = ESTATE.Hidden;
            }
            dt += Time.deltaTime;
        }

        Rect rect = new Rect(startPos.x, startPos.y, sizeCurrent, sizeCurrent);
        GUI.DrawTexture(rect, colorSpace);

        float alphaGradHeight = alphaGradientHeight * (sizeCurrent / sizeFull);
        Vector2 startPosAlpha = startPos + new Vector2(0, sizeCurrent);
        Rect rectAlpha = new Rect(startPosAlpha.x, startPosAlpha.y, sizeCurrent, alphaGradHeight);
        GUI.DrawTexture(rectAlpha, alphaGradient);

        Rect rectFullSize = new Rect(startPos.x, startPos.y, sizeCurrent, sizeCurrent + alphaGradHeight);

        Vector2 mousePos = Event.current.mousePosition;
        Event e = Event.current;
        bool isLeftMBtnClicked = e.type == EventType.MouseUp;
        bool isLeftMBtnDragging = e.type == EventType.MouseDrag;
        bool openCondition = (rectFullSize.Contains(e.mousePosition) && (((e.type == EventType.MouseUp || e.type == EventType.MouseDrag || e.type == EventType.MouseMove) && e.isMouse)));
        bool closeCondition = isLeftMBtnClicked || (!rectFullSize.Contains(e.mousePosition)) && (e.isMouse && (e.type == EventType.MouseMove || e.type == EventType.MouseDown));
        if (openCondition && (activeColorPicker == null || activeColorPicker.mState == ESTATE.Hidden))
        {
            if (mState == ESTATE.Hidden)
            {
                mState = ESTATE.Showing;
                activeColorPicker = this;
                dt = 0;
            }
        }
        if (closeCondition)
        {
            if (mState == ESTATE.Showed)
            {
                if (isLeftMBtnClicked)
                {
                    ApplyColor();
                }
                else
                {
                    SetColor(SelectedColor);
                }

                mState = ESTATE.Hidding;
                dt = 0;
            }
        }
        if (mState == ESTATE.Showed)
        {
            if (rect.Contains(e.mousePosition))
            {
                float coeffX = colorSpace.width / sizeCurrent;
                float coeffY = colorSpace.height / sizeCurrent;
                Vector2 localImagePos = (mousePos - startPos);
                Color res = colorSpace.GetPixel((int)(coeffX * localImagePos.x), colorSpace.height - (int)(coeffY * localImagePos.y) - 1);
                SetColor(res);
                if (isLeftMBtnDragging)
                {
                    ApplyColor();
                }
                UpdateColorEditFields(false);
                UpdateColorSliders(false);
            }
            else if (rectAlpha.Contains(e.mousePosition))
            {
                float coeffX = alphaGradient.width / sizeCurrent;
                float coeffY = alphaGradient.height / sizeCurrent;
                Vector2 localImagePos = (mousePos - startPosAlpha);
                Color res = alphaGradient.GetPixel((int)(coeffX * localImagePos.x), colorSpace.height - (int)(coeffY * localImagePos.y) - 1);
                Color curr = GetColor();
                curr.a = res.r;
                SetColor(curr);
                if (isLeftMBtnDragging)
                {
                    ApplyColor();
                }
                UpdateColorEditFields(false);
                UpdateColorSliders(false);
            }
            else if (rectColorEdit.Contains(e.mousePosition))
            {
                UpdateColorEditFields(true);
                UpdateColorSliders(false);
            }
            else if (rectColorSlider.Contains(e.mousePosition))
            {
                UpdateColorEditFields(false);
                UpdateColorSliders(true);
            }
            else
            {
                SetColor(SelectedColor);

            }
        }
    }

	public void SetColor(Color color)
	{
		TempColor = color;
		if(txColorDisplay != null)
		{
			txColorDisplay.SetPixel(0, 0, color);
			txColorDisplay.Apply();
		}
	}

	public Color GetColor()
	{
		return TempColor;
	}

	public void SetTitle(string title, Color textColor)
	{
		this.Title = title;
		this.textColor = textColor;
	}

	public void ApplyColor()
	{
		SelectedColor = TempColor;
		if(receiver)
		{
			receiver.SendMessage(colorSetFunctionName, SelectedColor, SendMessageOptions.DontRequireReceiver);
		}
	}

    public void NotifyColor(Color color)
    {
        SetColor(color);
        SelectedColor = color;
        UpdateColorEditFields(false);
        UpdateColorSliders(false);
    }
}
