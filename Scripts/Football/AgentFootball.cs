using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.VisualScripting;
using Grpc.Core;
using System;
using Unity.MLAgents.Integrations.Match3;
using UnityStandardAssets.Vehicles.Ball;
using Unity.MLAgents.Sensors;

public enum Team
{
    Blue = 0,
    Red = 1
}

// Para este agente usaremos un perceptor de rayos 3D puesto que el número de observaciones es muy dinámico (mov de la bola, agentes...)
public class AgentFootball : Agent
{
    public enum Position
    {
        Striker,
        Goalkeeper,
        Generic
    }

    [HideInInspector] public Team team;
    float kickPower;
    [HideInInspector] public float ballTouch;
    public Position pos;

    const float kPower = 1500f;
    float existReward; // penalizacion con existir
    float lateralSpeed;
    float forwardSpeed;

    [HideInInspector] public Rigidbody agentRB;
    BehaviorParameters behaviorParameters;
    FootballParams footballParams;
    public Vector3 initialPos;
    public float rotationSign;
    [SerializeField] private GameObject redGoal;
    [SerializeField] private GameObject blueGoal;

    // parámetros del ejemplo de chatgpt
    public float moveSpeed = 5f;
    public float rotationSpeed = 100f;
    private int movementAction;
    private int rotationAction;

    EnvironmentParameters resetParams;
    FootballEnvController envController;

    public override void Initialize()
    {
        envController = GetComponentInParent<FootballEnvController>();
        if (envController != null )
        {
            existReward = 1f / envController.MaxEnvSteps;
        } else
        {
            existReward = 1f / MaxStep;
        }

        behaviorParameters = gameObject.GetComponent<BehaviorParameters>();
        if (behaviorParameters.TeamId == (int)Team.Blue)
        {
            team = Team.Blue;
            initialPos = new Vector3(transform.localPosition.x, 1f, transform.localPosition.z);
            rotationSign = 1f;
        } else
        {
            team = Team.Red;
            initialPos = new Vector3(transform.localPosition.x, 1f, transform.localPosition.z);
            rotationSign = -1f;
        }
        if (pos == Position.Goalkeeper)
        {
            lateralSpeed = 1.0f;
            forwardSpeed = 1.0f;
        } else if (pos == Position.Striker)
        {
            lateralSpeed = .3f;
            forwardSpeed= 1.0f;
        } else // Generic/Defense
        {
            lateralSpeed = .3f;
            forwardSpeed = 1.0f;
        }
        footballParams = FindObjectOfType<FootballParams>();
        agentRB = GetComponent<Rigidbody>();
        agentRB.maxAngularVelocity = 500;

        resetParams = Academy.Instance.EnvironmentParameters;
    }

    public override void OnEpisodeBegin()
    {
        ballTouch = resetParams.GetWithDefault("ball touch", 0);
    }

    public void MoveAgent(ActionSegment<int> act)
    {
        var direction = Vector3.zero;
        var rotateDir = Vector3.zero;

        kickPower = 0f;

        var forwardAxis = act[0];
        var rightAxis = act[1];
        var rotateAxis = act[2];

        switch (forwardAxis)
        {
            case 1:
                direction = transform.forward * forwardSpeed;
                kickPower= 1f;
                break;
            case 2:
                direction = transform.forward * -forwardSpeed;
                break;
        }

        switch (rightAxis)
        {
            case 1:
                direction = transform.right * lateralSpeed;
                break;
            case 2:
                direction = transform.right * -lateralSpeed;
                break;
        }

        switch(rotateAxis)
        {
            case 1:
                rotateDir = transform.up * -1f;
                break;
            case 2:
                rotateDir = transform.up * 1f;
                break;
        }
        transform.Rotate(rotateDir, Time.deltaTime * 100f);
        agentRB.AddForce(direction * footballParams.agentRunSpeed, ForceMode.VelocityChange);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (pos == Position.Goalkeeper)
        {
            AddReward(existReward);
            if (team == Team.Red)
            {
                if (transform.localPosition.x >= 12f && transform.localPosition.x <= 18.8f && transform.localPosition.z >= -4.5f && transform.localPosition.z <= 4.5f)
                {
                    AddReward(existReward);
                    Debug.Log("Portero rojo en posicion");
                }
            }
            if (team == Team.Blue)
            {
                if (transform.localPosition.x >= -18.8f && transform.localPosition.x <= -12f && transform.localPosition.z >= -4.5f && transform.localPosition.z <= 4.5f)
                {
                    AddReward(existReward);
                    Debug.Log("Portero azul en posicion");
                }
            }
            //AddReward(existReward);
        } else if (pos == Position.Striker)
        {
            AddReward(-existReward);
        }
        //MoveAgent(actions.DiscreteActions);

        //ChatGPT
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

        // Reward the agent for moving towards the ball
        if ((Vector3.Distance(transform.localPosition, envController.ball.transform.localPosition) < 1.5f) && (pos == Position.Striker))
        {
           AddReward(0.2f);
        }
        // para cuando el agente cae del campo
        if (transform.localPosition.y < 0)
        {
            envController.ResetScene();
            AddReward(-1f);
        }
    }

    /*public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        //forward
        if (Input.GetKey(KeyCode.W))
        {
            discreteActionsOut[0] = 1;
        }
        if (Input.GetKey(KeyCode.S))
        {
            discreteActionsOut[0] = 2;
        }
        //rotate
        if (Input.GetKey(KeyCode.A))
        {
            discreteActionsOut[2] = 1;
        }
        if (Input.GetKey(KeyCode.D))
        {
            discreteActionsOut[2] = 2;
        }
        //right
        if (Input.GetKey(KeyCode.E))
        {
            discreteActionsOut[1] = 1;
        }
        if (Input.GetKey(KeyCode.Q))
        {
            discreteActionsOut[1] = 2;
        }
    }*/

    public override void CollectObservations(VectorSensor sensor)
    {
        // 16 observaciones
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(transform.forward);
        sensor.AddObservation(envController.ball.transform.localPosition);
        sensor.AddObservation(envController.ballRB.velocity);
        sensor.AddObservation(agentRB.velocity);
        sensor.AddObservation(redGoal.transform.localPosition);
        sensor.AddObservation(blueGoal.transform.localPosition);
        // los muros los omitimos por el momento y vemos como evoluciona
        sensor.AddObservation(behaviorParameters.TeamId);
    }

    private void OnCollisionEnter(Collision collision)
    {
        var force = kPower * kickPower;
        if (pos == Position.Goalkeeper)
        {
            force = kPower;
        }
        if (collision.gameObject.CompareTag("ball"))
        {
            AddReward(.2f * ballTouch);
            Debug.Log("Balltouch: " + ballTouch);
            var dir = collision.contacts[0].point - transform.localPosition;
            dir = dir.normalized;
            collision.gameObject.GetComponent<Rigidbody>().AddForce(dir * force);
        }
    }
}
