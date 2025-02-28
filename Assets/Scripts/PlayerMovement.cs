using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody rb;
    private Vector3 moveVector;
    public float moveSpeed = 5f;
    private float crouchSpeed = 2.5f; // Slower speed when crouching
    public bool isWalking;
    public bool isCrouching;

    private Vector3 crouchScale = new Vector3(1, 0.5f, 1);
    private Vector3 playerScale = new Vector3(1, 1f, 1);

    private float crouchHeightOffset = 0.5f; // Adjust position when crouching

    void Start()
    {
        isWalking = false;
        isCrouching = false;
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        moveVector = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        Crouching();
    }

    private void FixedUpdate()
    {
        if (moveVector != Vector3.zero)
        {
            isWalking = true;
            float speed = isCrouching ? crouchSpeed : moveSpeed;
            Vector3 moveDirection = transform.TransformDirection(moveVector.normalized);
            rb.MovePosition(rb.position + moveDirection * speed * Time.fixedDeltaTime);
        }
        else
        {
            isWalking = false;
            //Debug.Log(isWalking);
        }
    }

    private void Crouching()
    {
        if (Input.GetKey(KeyCode.C))
        {
            if (!isCrouching)
            {
                isCrouching = true;
                transform.localScale = crouchScale;
                transform.position = new Vector3(transform.position.x, transform.position.y - crouchHeightOffset, transform.position.z);
                // Debug.Log(isWalking);
                // Debug.Log(isCrouching);
            }
        }
        else // Release to stand up
        {
            if (isCrouching)
            {
                isCrouching = false;
                transform.localScale = playerScale;
                transform.position = new Vector3(transform.position.x, transform.position.y + crouchHeightOffset, transform.position.z);
            }
        }
    }
}
