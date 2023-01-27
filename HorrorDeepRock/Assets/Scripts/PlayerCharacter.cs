using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacter : MonoBehaviour
{
    private CharacterController controller;
    private Vector3 playerVelocity;
    private float playerSpeed = 7.0f;
    private float jumpHeight = 1.0f;
    private float gravity = -9.81f;
    private bool grounded = false;

    // Start is called before the first frame update
    void Start()
    {
        GetComponents();
    }

    // Update is called once per frame
    void Update()
    {
        MouseLook();
        Movement();
    }

    private void MouseLook()
    {

    }

    private void Movement()
    {
        //grounded = controller.collisionFlags & CollisionFlags.Below

        if(grounded && playerVelocity.y < 0)
        {
            playerVelocity.y = gravity * Time.deltaTime;
        }

        if((controller.collisionFlags & CollisionFlags.Above) != 0)
        {
            playerVelocity.y = 0;
        }

        Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        controller.Move(move * Time.deltaTime * playerSpeed);

        if(Input.GetKeyDown(KeyCode.Space) && grounded)
        {
            playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravity);
        }

        playerVelocity.y += gravity * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);
    }

    private void GetComponents()
    {
        controller = GetComponent<CharacterController>();
    }
}
