using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FacePlayer : MonoBehaviour
{
    private Transform player;

    private GameObject plane;

    private void Start()
    {
        player = GameObject.Find("Player(Clone)").gameObject.transform.Find("Camera");
        plane = gameObject.transform.Find("Plane").gameObject;
    }

    private void Update()
    {
        LookAtPlayer();
    }

    private void LookAtPlayer()
    {
       plane.transform.LookAt(player);
    }
}
