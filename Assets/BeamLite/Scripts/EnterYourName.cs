using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnterYourName : MonoBehaviour {

    public void ShowEnterYourNamePanel()
    {
        gameObject.SetActive(true);
    }

    public void DestroyEnterNamePanelAndKeyboard()
    {
        Destroy(transform.parent.gameObject);
    }
}
