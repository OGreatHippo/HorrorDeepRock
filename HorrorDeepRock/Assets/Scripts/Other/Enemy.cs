using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    //Movement
    private float walkSpeed = 5f;
    private float currentSpeed;
    private float sprintSpeed = 8f;
    private float gravity = -9.81f;

    //Components
    private Animator animator;
    private NavMeshAgent agent;

    private float distanceFromPlayer = 0.7f;

    public bool menuMode;

    private void Start()
    {
        animator = GetComponentInChildren<Animator>();
        agent = GetComponent<NavMeshAgent>();   
    }

    private void Update()
    {
        Movement();
        Animate();
    }

    private void Movement()
    {
        if(!menuMode)
        {
            //transform.LookAt(test);

            //agent.SetDestination(test.position);

            currentSpeed = walkSpeed;

            //if (Vector3.Distance(transform.position, test.position) < distanceFromPlayer)
            //{
            //    gameObject.GetComponent<NavMeshAgent>().velocity = Vector3.zero;

            //    currentSpeed = 3;

            //    Debug.Log("Hits Player");
            //}
        }
    }

    private void Animate()
    {
        animator.SetFloat("MovementSpeed", currentSpeed);
    }
}
