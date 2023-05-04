using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCharacter : MonoBehaviour
{
    //Movement
    private CharacterController controller;
    private Vector3 playerVelocity;
    private float playerSpeed = 5.0f;
    private float currentSpeed;
    private float jumpHeight = 2.5f;
    private float gravity = -9.81f;

    //Sprinting
    private float sprintSpeed = 10f;
    private float stamina = 100f;
    private bool sprinting = false;
    private bool sprintCD = false;
    private float sprintTimer = 4.0f;
    public StaminaSlider staminaSlider;
    public Image staminaImg;

    //Camera Movement
    private float mouseSensitivity = 1000f;
    private Transform playerCamera;
    private float xRotationCamera = 0f;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        GetComponents();
        staminaSlider.SetMaxStamina(100f);
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

        Sprint();

        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * Time.deltaTime * currentSpeed);
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

    private void Sprint()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && !sprintCD)
        {
            sprinting = true;
        }

        if(Input.GetKeyUp(KeyCode.LeftShift))
        {
            sprinting = false;
        }

        if(sprinting)
        {
            if(stamina > 0)
            {
                currentSpeed = sprintSpeed;
                stamina -= 10 * Time.deltaTime;

                staminaSlider.SetStamina(stamina);
            }

            else
            {
                sprinting = false;

                sprintCD = true;

                sprintTimer = 4.0f;
            }
        }

        else
        {
            currentSpeed = playerSpeed;

            if(stamina < 100)
            {
                stamina += 10 * Time.deltaTime;

                staminaSlider.SetStamina(stamina);
            } 
        }

        if(sprintCD)
        {
            sprintTimer -= Time.deltaTime;

            Color fatigueColour = new Color(0.0f, 0.5f, 0.0f);

            staminaImg.color = fatigueColour;

            if(sprintTimer <= 0)
            {
                sprintCD = false;

                staminaImg.color = Color.green;
            }
        }
    }

    private void GetComponents()
    {
        controller = GetComponent<CharacterController>();
        playerCamera = transform.Find("Camera").transform;

        staminaSlider = GameObject.Find("Canvas").transform.Find("StaminaBar").GetComponent<StaminaSlider>();
        staminaImg = staminaSlider.transform.Find("Stamina").GetComponent<Image>();
    }
}
