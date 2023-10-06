using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera : MonoBehaviour
{
    private float currentSpeed;
    private float speed = 50.0f;
    private float fast = 100.0f;
    private float sensitivity = 1000.0f;

    private float xRotationCamera = 0f;

    //Menu
    public GameObject menu;
    private bool inMenu;

    // Update is called once per frame
    private void Update()
    {
        Cursor.lockState = CursorLockMode.Locked;

        if(!inMenu)
        {
            Movement();
        }
        
        OpenMenu();
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

    public void OpenMenu()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            inMenu = !inMenu;
        }

        if (inMenu)
        {
            menu.SetActive(true);

            Cursor.lockState = CursorLockMode.Confined;
        }

        else
        {
            menu.SetActive(false);

            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    public void menuStuff()
    {
        inMenu = !inMenu;
    }
}
