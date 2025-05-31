using System;
using UnityEngine;

public class PullTargetLimiter : MonoBehaviour
{
    [SerializeField] private Transform targetCamera;
    [SerializeField] private Transform targetOrientation;
    [SerializeField] private float distance = 1.82f;
    [SerializeField] private float maxHeightAbove = 0.3f;
    [SerializeField] private float maxHeightBelow = 0.2f;
    [SerializeField] private float minWidthDifference = 0.2f;

    void Update()
    {
        Vector3 cameraPosition = targetCamera.position;
        Vector3 orientationPosition = targetOrientation.position;
        Vector3 forwardDirection = targetCamera.forward;

        Vector3 targetPosition = cameraPosition + forwardDirection * distance;

        float heightDifference = targetPosition.y - orientationPosition.y;

        if (heightDifference > maxHeightAbove)
            targetPosition.y = orientationPosition.y + maxHeightAbove;
        else if (heightDifference < -maxHeightBelow)
            targetPosition.y = orientationPosition.y - maxHeightBelow;

        Vector3 horizontalOffset = new Vector3(
            targetPosition.x - orientationPosition.x,
            0,
            targetPosition.z - orientationPosition.z
        );
        float horizontalDistance = horizontalOffset.magnitude;

        if (horizontalDistance < minWidthDifference)
        {
            horizontalOffset = horizontalOffset.normalized * minWidthDifference;
            targetPosition.x = orientationPosition.x + horizontalOffset.x;
            targetPosition.z = orientationPosition.z + horizontalOffset.z;
        }

        transform.position = targetPosition;
    }
}
