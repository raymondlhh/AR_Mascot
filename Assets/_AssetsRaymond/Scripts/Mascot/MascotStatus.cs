using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MascotStatus : MonoBehaviour
{
    [Header("Mascot Object Names")]
    public GameObject[] mascotNamesToHide = new GameObject[3];
    
    [Header("Target Object to Show")]
    public GameObject targetToShow;

    public void HideAllTargets()
    {
        foreach (GameObject mascotObject in mascotNamesToHide)
        {
            if (mascotObject != null)
            {
                mascotObject.SetActive(false);
            }
        }
    }
    
    /// <summary>
    /// Shows the target object
    /// </summary>
    public void ShowTargetObject()
    {
        if (targetToShow != null)
        {
            targetToShow.SetActive(true);
        }
    }
}
