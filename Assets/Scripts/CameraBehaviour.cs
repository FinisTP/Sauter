using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehaviour : MonoBehaviour
{
    public Transform Target;
    public float SmoothSpeed = 0.125f;
    public Vector3 Offset;

    private void Start()
    {
        Offset = new Vector3(0,0,-10);
    }
    private void FixedUpdate()
    {
        Vector3 desiredPosition = Target.position + Offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, SmoothSpeed);

        transform.position = smoothedPosition;

    }
}
