using System;
using UnityEngine;

public class WorldCameraController : MonoBehaviour
{
    [SerializeField] private Camera worldCamera;
    [SerializeField] private Transform targetTransform;

    [Header("Camera Settings")] [SerializeField]
    private float followSpeed = 0.05f;

    private void LateUpdate()
    {
        if (targetTransform)
        {
            transform.position =
                Vector3.Lerp(transform.position, targetTransform.position + new Vector3(0, 0, -10),
                    followSpeed * Time.deltaTime * 60);

        }
    }
}
