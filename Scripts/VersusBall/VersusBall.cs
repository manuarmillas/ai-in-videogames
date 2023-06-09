using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.VisualScripting;

public class VersusBall : MonoBehaviour
{
    /*public VersusBallEnvController envController;

    [SerializeField] private GameObject redPlayer;
    [SerializeField] private GameObject bluePlayer;

    private float timer = 500;

    //Collider redCol;
    //Collider blueCol;

    private void Start()
    {
        //redCol= redPlayer.GetComponent<Collider>();
        //blueCol= bluePlayer.GetComponent<Collider>();
        envController = GetComponentInParent<VersusBallEnvController>();
    }

    private void FixedUpdate()
    {
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            envController.ResetBall();
            timer = 500;
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "redAgent")
        {
            envController.ResolveEvent(1);
            timer = 500;
        }
        if (other.gameObject.tag == "blueAgent")
        {
            envController.ResolveEvent(2);
            timer = 500;
        }
        if (other.gameObject.tag == "Wall")
        {
            envController.ResolveEvent(3);
            timer = 500;
        }
        if (other.gameObject.tag == "Obstacle")
        {
            envController.ResolveEvent(4);
            timer = 500;
        }
    }*/
}
