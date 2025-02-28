using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour
{
    public Transform cameraTransform;
    public float Sensitivity;

    private float X;
    private float Y;

    void Awake()
    {
        Vector3 euler = transform.rotation.eulerAngles;
        X = euler.y;
        Y = cameraTransform.localRotation.eulerAngles.x;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        const float MIN_Y = -90.0f;
        const float MAX_Y = 90.0f;

        X += Input.GetAxis("Mouse X") * (Sensitivity * Time.deltaTime);
        Y -= Input.GetAxis("Mouse Y") * (Sensitivity * Time.deltaTime);
        Y = Mathf.Clamp(Y, MIN_Y, MAX_Y);

        transform.rotation = Quaternion.Euler(0, X, 0);
        cameraTransform.localRotation = Quaternion.Euler(Y, 0, 0);
    }
}
