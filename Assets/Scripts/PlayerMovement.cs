using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody rb;
    private Vector3 moveVector;
    public float moveSpeed = 5f;
    private float crouchSpeed = 2.5f;
    public bool isWalking;
    public bool isCrouching;

    private Vector3 crouchScale = new Vector3(1, 0.5f, 1);
    private Vector3 playerScale = new Vector3(1, 1f, 1);

    private float crouchHeightOffset = 0.5f;

    public bool gotCaught = false;

    private bool isStaminaEmpty;
    private Stamina stamina;

    void Start()
    {
        isWalking = false;
        isCrouching = false;
        rb = GetComponent<Rigidbody>();
        stamina = GetComponent<Stamina>();
        
    }

    void Update()
    {
        isStaminaEmpty = stamina.isEmpty;

        if (!gotCaught)
        {
            moveVector = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
            Crouching();
        }
    }

    private void FixedUpdate()
    {
        if (moveVector != Vector3.zero && !gotCaught)
        {
            isWalking = true;
            float speed = isCrouching ? crouchSpeed : moveSpeed;
            Vector3 moveDirection = transform.TransformDirection(moveVector.normalized);
            rb.MovePosition(rb.position + moveDirection * speed * Time.fixedDeltaTime);
        }
        else
        {
            isWalking = false;
            rb.velocity = Vector3.zero;
        }
    }


    private void Crouching()
    {
        bool crouchKeyPressed = Input.GetKey(KeyCode.C) || Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.Z);

        if (crouchKeyPressed && !isCrouching && !isStaminaEmpty)
        {
            isCrouching = true;
            transform.localScale = crouchScale;
            transform.position = new Vector3(transform.position.x, transform.position.y - crouchHeightOffset, transform.position.z);
        }
        else if ((!crouchKeyPressed || isStaminaEmpty) && isCrouching) // Release to stand up or when stamina is empty
        {
            isCrouching = false;
            transform.localScale = playerScale;
            transform.position = new Vector3(transform.position.x, transform.position.y + crouchHeightOffset, transform.position.z);
        }
    }

}
