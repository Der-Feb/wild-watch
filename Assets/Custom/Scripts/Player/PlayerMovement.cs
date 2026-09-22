using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;
 
    public float speed = 12f, jumpHeight = 3f;
    public float gravity = -9.81f * 2.5f;
 
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    float horizontalInput, verticalInput;
 
    Vector3 velocity;
 
    bool isGrounded;

    void Start()
    {
        groundMask = LayerMask.GetMask("Ground");
    }
 
    // Update is called once per frame
    void Update()
    {
        MyInput();
    }

    void FixedUpdate()
    {
        MovePlayer();
    }

    private void MovePlayer()
    {
        //checking if we hit the ground to reset our falling velocity, otherwise we will fall faster the next time
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
 
        if (isGrounded && velocity.y < 0) velocity.y = -2f;
 
        //right is the red Axis, foward is the blue axis
        Vector3 move = transform.right * horizontalInput + transform.forward * verticalInput;
 
        controller.Move(move * speed * Time.deltaTime);
 
        //check if the player is on the ground so they can jump
        if (Input.GetButtonDown("Jump") && isGrounded)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity); //the equation for jumping
 
        velocity.y += gravity * Time.deltaTime;
 
        controller.Move(velocity * Time.deltaTime);
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
    }
}