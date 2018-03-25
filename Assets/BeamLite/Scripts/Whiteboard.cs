using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class Whiteboard : MonoBehaviour
{
    private int _penSize = 5;
    public Color resetColor;

    private Texture2D texture;
    private Color[] color;

    private bool touching, touchingLast;
    private float positionX, positionY;
    private float lastX, lastY;

    private int width, height;
    private Color[] resetColorArray;

    // Use this for initialization
    void Start()
    {
        Renderer renderer = GetComponent<Renderer>();
        // create a new 2d texture for the whiteboard
        width = (int)(this.gameObject.transform.localScale.x * 500);
        height = (int)(this.gameObject.transform.localScale.y * 500);
        this.texture = new Texture2D(width, height);
        renderer.material.mainTexture = this.texture;

        // this is the color for writing (equals a square of penSize * penSize in black)
        this.color = Enumerable.Repeat<Color>(Color.black, _penSize * _penSize).ToArray<Color>();

        // defining a reset color
        resetColor = new Color(255, 255, 255, 0);

        //create a array for reseting the whiteboard
        resetColorArray = texture.GetPixels();
        for (int i = 0; i < resetColorArray.Length; i++)
        {
            resetColorArray[i] = resetColor;
        }
        texture.SetPixels(resetColorArray);
        texture.Apply();
    }

    /// <summary>
    /// clears the whiteboard
    /// </summary>
    public void Clear()
    {    
        texture.SetPixels(resetColorArray);
        texture.Apply();
    }

    // Update is called once per frame
    void Update()
    {
        int x = (int)(positionX * width - (_penSize / 2));
        int y = (int)(positionY * height - (_penSize / 2));

        if (ValidPosition(x, y))
        {
            if (touchingLast)
            {
                texture.SetPixels(x, y, _penSize, _penSize, color);
                for (float time = 0.01f; time < 1.00f; time += 0.01f)
                {
                    int lerpX = (int)Mathf.Lerp(lastX, (float)x, time);
                    int lerpY = (int)Mathf.Lerp(lastY, (float)y, time);
                    texture.SetPixels(lerpX, lerpY, _penSize, _penSize, color);
                }
                texture.Apply();
            }
        }

        this.lastX = (float)x;
        this.lastY = (float)y;

        this.touchingLast = this.touching;
    }

    /// <summary>
    /// Checks if x and y is a valid position on whiteboard
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    private bool ValidPosition(int x, int y)
    {
        return x >= 0 && y >= 0 && x <= width && y <= height;
    }

    public void ToggleTouch(bool touching)
    {
        this.touching = touching;
    }

    public void SetTexture(Color[] texture)
    {
        this.texture.SetPixels(texture);
    }

    /// <summary>
    /// set the touchposition and color
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="color"></param>
    public void SetTouchPosition(float x, float y, Color color, int penSize)
    {
        // change color only if new color is different to current color
        if(this.color[0] != color || _penSize != penSize)
        {
            SetColor(color,penSize);
        }
        
        this.positionX = x;
        this.positionY = y;
    }

    public void SetColor(Color color, int penSize)
    {
        this._penSize = penSize;
        this.color = Enumerable.Repeat<Color>(color, _penSize * _penSize).ToArray<Color>();
    }

    public Color[] GetTexture()
    {
        return texture.GetPixels();
    }
}
