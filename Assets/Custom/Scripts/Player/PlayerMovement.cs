using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;
    public Animator animator; // Drag your Animator component here in the Inspector

    public float speed = 12f, jumpHeight = 3f;
    public float gravity = -9.81f * 2.5f;

    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    float horizontalInput, verticalInput;

    Vector3 velocity;
    bool isGrounded;
    bool jumpQueued;

    void Update()
    {
        MyInput();

        if (Input.GetKeyDown(KeyCode.Space))
            jumpQueued = true;

        // Check if player is pressing any direction keys
        bool isMoving = (horizontalInput != 0 || verticalInput != 0);

        // Update the Animator boolean parameter
        if (animator != null)
        {
            animator.SetBool("isRunning", isMoving);
        }
    }

    void FixedUpdate()
    {
        MovePlayer();
    }

    private void MovePlayer()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0) velocity.y = -2f;

        Vector3 move = transform.right * horizontalInput + transform.forward * verticalInput;

        controller.Move(move * speed * Time.fixedDeltaTime); // Use Time.fixedDeltaTime in FixedUpdate

        if (jumpQueued && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jumpQueued = false;
        }

        velocity.y += gravity * Time.fixedDeltaTime;

        controller.Move(velocity * Time.fixedDeltaTime);
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
    }
}