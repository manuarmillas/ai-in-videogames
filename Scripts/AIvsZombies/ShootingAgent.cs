using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Unity.VisualScripting;
using UnityEditor.Compilation;

public class ShootingAgent : Agent
{
    public int damage = 100;
    public float range = 87;
    public LayerMask enemyLayer;
    public Transform shootingPoint;
    public int shootingDelay = 100; // steps before can shoot

    private bool canShoot = true;
    private int stepsRemaining = 0; // steps remaining to shoot again
    private int totalEnemies;
    private int killingSpree;

    private Vector3 startingPos;
    private Rigidbody rb;

    [SerializeField] private Enemy[] enemies;
    public GameObject weapon;

    private float movementAction;
    private float rotationAction;
    private float shootingAction;
    public float moveSpeed = 5f;
    public float rotationSpeed = 100f;
    CharacterController characterController;

    /*private void Shoot()
    {
        if (!canShoot)
        {
            return;
        }
        int layerMask = 1 << LayerMask.NameToLayer("Enemy");
        Vector3 dir = transform.forward;
        Debug.DrawRay(shootingPoint.localPosition, dir, Color.green, 2f);
        if (Physics.Raycast(shootingPoint.localPosition, dir, out var hit, 200f, layerMask))
        {
            hit.transform.GetComponent<Enemy>().GetShot(damage, this);
        }
        canShoot = false;
        stepsRemaining = shootingDelay;
    }*/

    private void Shoot()
    {
        if (!canShoot)
        {
            return;
        }
        /*weapon.GetComponent<RaycastShoot>().Shoot();
        canShoot = false;
        stepsRemaining = shootingDelay;*/
        RaycastHit hit;
        //Debug.DrawRay(shootingPoint.position, shootingPoint.forward * range, Color.red, 200f);
        //Debug.Log(shootingPoint.position);
        if (Physics.Raycast(shootingPoint.position, shootingPoint.forward, out hit, range, enemyLayer))
        {
            hit.transform.GetComponent<Enemy>().GetShot(damage, this);
            //hit.collider.gameObject.SetActive(false);
        } else // Missed shot
        {
            AddReward(-.2f);
            Debug.Log("Missed");
        }
        canShoot = false;
        stepsRemaining = shootingDelay;
    }

    public void RegisterKill()
    {
        AddReward(1f);
        totalEnemies--;
        if (totalEnemies <= 0) 
            EndEpisode();
        Debug.Log("Enemy killed");
        killingSpree++;
        if (killingSpree == 3)
        {
            AddReward(1f);
            killingSpree= 0;
        }
    }

    public override void Initialize()
    {
        startingPos = transform.localPosition;
        rb = GetComponent<Rigidbody>();
        //enemies = GameObject.FindGameObjectsWithTag("Enemy");
        totalEnemies = enemies.Length;
        killingSpree = 0;
    }

    public override void OnEpisodeBegin()
    {
        //startingPos = transform.localPosition;
        rb.velocity = Vector3.zero;
        canShoot = true;
        Respawn();
        /*for (int i = 0; i < enemies.Length; i++)
        {
            enemies[i].Respawn();
        }
        totalEnemies = enemies.Length;*/
    }

    private void FixedUpdate()
    {
        if (!canShoot)
        {
            stepsRemaining--;
            if (stepsRemaining <= 0) 
            {
                canShoot = true;
            }
        }
        if (transform.localPosition.x > 75 || transform.localPosition.x < -75 || transform.localPosition.z > 75 || transform.localPosition.z < -75) EndEpisode();
    }

    private void Respawn()
    {
        transform.localPosition = new Vector3(0f, 3.27f, 0f);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        movementAction = (int)actions.DiscreteActions[0];
        rotationAction = (int)actions.DiscreteActions[1];

        // Move the agent based on the input values
        switch (movementAction)
        {
            case 0:
                transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
                break;
            case 1:
                transform.Translate(-Vector3.forward * moveSpeed * Time.deltaTime);
                break;
            case 2:
                transform.Translate(Vector3.right * moveSpeed * Time.deltaTime);
                break;
            case 3:
                transform.Translate(-Vector3.right * moveSpeed * Time.deltaTime);
                break;
            default:
                break;
        }

        switch (rotationAction)
        {
            case 0:
                transform.Rotate(Vector3.up, -rotationSpeed * Time.deltaTime);
                break;
            case 1:
                transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
                break;
            default:
                break;
        }


        if (actions.ContinuousActions[0] > 0.5f)
        {
            Shoot();
        }

        AddReward(-1 / MaxStep);
    }

    /*public void Move(float forwardAmount, float rightAmount)
    {
        Vector3 moveDirection = new Vector3(rightAmount, 0f, forwardAmount).normalized;
        Vector3 movement = moveDirection * moveSpeed * Time.deltaTime;

        // Use the CharacterController component to move the agent
        characterController.Move(movement);
    }

    public void Rotate(float rotateAmount)
    {
        // Use the CharacterController component to rotate the agent
        characterController.transform.Rotate(Vector3.up, rotateAmount * rotationSpeed * Time.deltaTime);
    }*/

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        /*var discreteActionsOut = actionsOut.ContinuousActions;
        discreteActionsOut[0] = Input.GetKey(KeyCode.P) ? 1f : 0f;*/
        //var continuousActions = actionsOut.ContinuousActions;
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetKey(KeyCode.Space) ? 1f : 0f;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(canShoot);
        sensor.AddObservation(transform.forward);
        sensor.AddObservation(killingSpree);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("Enemy"))
        {
            Debug.Log("Zombie killed shooter");
            AddReward(-1f);
            EndEpisode();
        }
        if (collision.transform.CompareTag("Wall"))
        {
            AddReward(-1f);
            EndEpisode();
        }
    }
}
