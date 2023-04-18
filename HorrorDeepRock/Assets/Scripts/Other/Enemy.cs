using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class Enemy : MonoBehaviour
{
    //Movement
    private float walkSpeed;
    private float currentSpeed;
    private float sprintSpeed;
    private float gravity = -9.81f;

    //Components
    private Animator animator;

    private void Start()
    {
        animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        Animate();
    }

    private void Animate()
    {
        animator.SetFloat("MovementSpeed", currentSpeed);
    }
}
