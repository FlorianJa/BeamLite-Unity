using HoloToolkit.Unity.InputModule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class WhiteboardPenColorChanger : MonoBehaviour, IInputClickHandler
{

    /// <summary>
    /// Pen to change writingcolor and material
    /// </summary>
	public GameObject WhiteboardPen;

    /// <summary>
    /// WritingColor to change to when button is touched
    /// </summary>
    public Color WritingColor;

    /// <summary>
    /// Material of cap and back of the pen, showing which writing color the pen has
    /// </summary>
    public Material PenCapMaterial;

    /// <summary>
    /// Material of the body of the pen, between cap and back
    /// </summary>
    public Material PenBodyMaterial;

    public int PenSizeForThisColor;

    private void Start()
    {
#if !UNITY_WSA
        GetComponent<VRTK_InteractableObject>().InteractableObjectTouched += WhiteboardPenColorChanger_InteractableObjectTouched;

        GetComponent<VRTK_InteractableObject>().InteractableObjectUsed += WhiteboardPenColorChanger_InteractableObjectUsed;


#endif
    }

    private void WhiteboardPenColorChanger_InteractableObjectUsed(object sender, InteractableObjectEventArgs e)
    {
        ChangeColor();
    }

    /// <summary>
    /// Callback function is called when the controller is touching the button
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void WhiteboardPenColorChanger_InteractableObjectTouched(object sender, InteractableObjectEventArgs e)
    {
        ChangeColor();
    }

    public void OnInputClicked(InputClickedEventData eventData)
    {
        ChangeColor();
    }

    private void ChangeColor()
    {
        if (WhiteboardPen != null)
        {
            // set the writingcolor of the pen
            WhiteboardPen.GetComponent<NewWhiteboardPen>().SetColorAndPenSize(WritingColor,PenSizeForThisColor);

            // change the material of the pen according to the writingcolor
            var renderer = WhiteboardPen.GetComponentInChildren<Renderer>();
            var materials = renderer.materials;
            materials[0] = PenCapMaterial;
            materials[1] = PenBodyMaterial;
            renderer.materials = materials;
        }
    }
}
