using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class VersusBallAgent : Agent
{
    [SerializeField] private GameObject goal;
    [SerializeField] private GameObject oponent;
    private SimpleCharacterController characterController;
    private VersusBallEnvController envController;
    new private Rigidbody rigidbody;
    public float moveSpeed = 5f;
    public float turnSpeed = 300f;

    public override void Initialize()
    {
        characterController = GetComponent<SimpleCharacterController>();
        rigidbody = GetComponent<Rigidbody>();
        envController = GetComponentInParent<VersusBallEnvController>();
    }

    /*private void FixedUpdate()
    {
        Debug.Log(transform.localPosition);
    }*/

    public override void OnEpisodeBegin()
    {
        transform.localPosition = new Vector3(Random.Range(-46.3f, 0.77f), 0, Random.Range(-23f, 23f));
        //Debug.Log(transform.localPosition);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float vertical = actions.DiscreteActions[0] <= 1 ? actions.DiscreteActions[0] : -1;
        float horizontal = actions.DiscreteActions[1] <= 1 ? actions.DiscreteActions[1] : -1;
        characterController.ForwardInput = vertical;
        characterController.TurnInput = horizontal;

        AddReward(-1f / 10000f); // penalizar el número de pasos que da
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        int vertical = Mathf.RoundToInt(Input.GetAxisRaw("Vertical"));
        int horizontal = Mathf.RoundToInt(Input.GetAxisRaw("Horizontal"));
        ActionSegment<int> actions = actionsOut.DiscreteActions;
        actions[0] = vertical >= 0 ? vertical : 2;
        actions[1] = horizontal >= 0 ? horizontal : 2;
    }

    /*public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(transform.forward);
        sensor.AddObservation((goal.transform.localPosition - transform.localPosition).normalized);
        sensor.AddObservation(goal.transform.localPosition);
    }*/

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Wall")
        {
            AddReward(-2f);
            EndEpisode();
        }
        if (collision.gameObject.tag == "Obstacle")
        {
            AddReward(-1f);
            envController.DeleteBall(collision.gameObject);
        }
        if (collision.gameObject.tag == "Reward")
        {
            AddReward(1f);
            envController.DeleteBall(collision.gameObject);
        }
    }
}
