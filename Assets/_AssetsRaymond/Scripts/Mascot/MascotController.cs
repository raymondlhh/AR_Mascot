using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MascotController : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;
    public bool lookOnCamera;
    private void Update()
    {
        if (lookOnCamera)
        {
            LookOnPlayerHandler();
        }
    }

    private void LookOnPlayerHandler()
    {
        Vector3 direction = cameraTransform.position - transform.position;

        if (direction.magnitude < 0.01f) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        targetRotation.x = 0;
        targetRotation.z = 0;

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 5f * Time.deltaTime);
    }
}
