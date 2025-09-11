using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Switch : MonoBehaviour
{
    [SerializeField] private GameObject Switch3D;
    [SerializeField] private GameObject Switch2D;
    [SerializeField] private ChatGPT chatGPT;
    [SerializeField] private GameObject MascotProfile;

    int index;

    void Start()
    {

    }

    void Update()
    {
        // Check if display text is still being processed, if so return early
        if (chatGPT != null && !chatGPT.isDisplayTextFinished)
        {
            return;
        }

        // Check if is3DText is true, then set index = 1
        if (chatGPT != null && !chatGPT.is3DText)
        {
            SwitchTo2D();
        }
        else if (chatGPT != null && chatGPT.is3DText)
        {
            SwitchTo3D();
        }

        if (index == 0)
        {
            // 2D mode - disable 3D text
            if (chatGPT != null)
            {
                
                chatGPT.Set3DTextMode(false);
            }
        }

        if (index == 1)
        {
            // 3D mode - enable 3D text
            if (chatGPT != null)
            {
                
                chatGPT.Set3DTextMode(true);
            }
        }
    }

public void SwitchTo2D()
{
    if (MascotProfile != null && !MascotProfile.activeInHierarchy)
    {
        return;
    }
    index = 0;
    Switch2D.gameObject.SetActive(true);
    Switch3D.gameObject.SetActive(false);
    
    // Disable 3D text when switching to 2D mode
    if (chatGPT != null)
    {
        chatGPT.Set3DTextMode(false);
    }
}

public void SwitchTo3D()
{
    if (MascotProfile != null && !MascotProfile.activeInHierarchy)
    {
        return;
    }
    index = 1;
    Switch3D.gameObject.SetActive(true);
    Switch2D.gameObject.SetActive(false);
    
    // Enable 3D text when switching to 3D mode
    if (chatGPT != null)
    {
        chatGPT.Set3DTextMode(true);
    }
}
}
