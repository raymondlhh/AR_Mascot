using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MascotController : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private GameObject[] mascotGameObjects; // Array of mascot game objects to monitor
    public bool lookOnCamera;
    private void Update()
    {
        // Check if any mascot game object is active
        CheckMascotGameObjects();
        
        // if (lookOnCamera)
        // {
        //     LookOnPlayerHandler();
        // }
    }

    public void CheckMascotGameObjects()
    {
        if (mascotGameObjects == null) return;
        
        foreach (GameObject mascotObject in mascotGameObjects)
        {
            if (mascotObject != null && mascotObject.activeInHierarchy)
            {
                lookOnCamera = false;
                return; // Exit early if any mascot object is active
            }
            else
            {
                lookOnCamera = true;
            }
        }
    }

    private void LookOnPlayerHandler()
    {
        Vector3 direction = cameraTransform.position - transform.position;

        if (direction.magnitude < 0.01f) return;

        // Calculate the Y rotation angle only
        float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0, angle, 0);

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 5f * Time.deltaTime);
    }
}
