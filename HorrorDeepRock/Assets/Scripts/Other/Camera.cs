using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera : MonoBehaviour
{
    public float currentSpeed;
    private float speed = 50.0f;
    private float fast = 100.0f;
    private float sensitivity = 1000.0f;

    private float xRotationCamera = 0f;

    // Update is called once per frame
    private void Update()
    {
        Cursor.lockState = CursorLockMode.Locked;

        Movement();
    }

    private void Movement()
    {
        if(Input.GetKey(KeyCode.LeftShift))
        {
            currentSpeed = fast;
        }

        else
        {
            currentSpeed = speed;
        }

        transform.position += transform.forward * Input.GetAxis("Vertical") * currentSpeed * Time.deltaTime;
        transform.position += transform.right * Input.GetAxis("Horizontal") * currentSpeed * Time.deltaTime;

        float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;

        xRotationCamera -= mouseY;
        xRotationCamera = Mathf.Clamp(xRotationCamera, -90f, 90f);

        float yRotationCamera = transform.eulerAngles.y + mouseX;

        transform.rotation = Quaternion.Euler(xRotationCamera, yRotationCamera, 0);
    }
}
