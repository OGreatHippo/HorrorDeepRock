using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacter : MonoBehaviour
{
    //Movement
    private CharacterController controller;
    private Vector3 playerVelocity;
    public float playerSpeed = 7.0f;
    private float jumpHeight = 2.5f;
    private float gravity = -9.81f;
    private float sprintSpeed = 15f;
    public float stamina = 100f;
    public bool fatigued = false;

    //Camera Movement
    private float mouseSensitivity = 1000f;
    private Transform playerCamera;
    private float xRotationCamera = 0f;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        GetComponents();
    }

    // Update is called once per frame
    void Update()
    {
        MouseLook();
        Movement();
        Jump();
    }

    private void MouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotationCamera -= mouseY;
        xRotationCamera = Mathf.Clamp(xRotationCamera, -90f, 90f);

        playerCamera.localRotation = Quaternion.Euler(xRotationCamera, 0f, 0f);
        gameObject.transform.Rotate(Vector3.up * mouseX);
    }

    private void Movement()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        if (Input.GetKey(KeyCode.LeftShift))
        {
            if(stamina > 0f && !fatigued)
            {
                playerSpeed = sprintSpeed;

                stamina -= 10f * Time.deltaTime;
            }

            else
            {
                playerSpeed = 7.0f;

                fatigued = true;
            }
        }

        if(fatigued)
        {
            stamina += 10f * Time.deltaTime;

            if(stamina >= 35f)
            {
                fatigued = !fatigued;
            }
        }

        //stamina += 10f * Time.deltaTime;

        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * Time.deltaTime * playerSpeed);
    }

    private void Jump()
    {
        if (controller.isGrounded && playerVelocity.y <= 0)
        {
            playerVelocity.y = -2f;
        }

        playerVelocity.y += gravity * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);

        if(controller.isGrounded)
        {
            if(Input.GetKeyDown(KeyCode.Space))
            {
                playerVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
        }

        if ((controller.collisionFlags & CollisionFlags.Above) != 0)
        {
            playerVelocity.y = 0;
        }

    }

    private void GetComponents()
    {
        controller = GetComponent<CharacterController>();
        playerCamera = GameObject.Find("Camera").transform;
    }
}
