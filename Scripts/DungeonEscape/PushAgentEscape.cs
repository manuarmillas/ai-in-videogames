using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class PushAgentEscape : Agent
{

    public GameObject MyKey; //my key gameobject. will be enabled when key picked up.
    public bool IHaveAKey; //have i picked up a key
    private PushBlockSettings m_PushBlockSettings;
    private Rigidbody m_AgentRb;
    private DungeonEscapeEnvController m_GameController;

    // vars for agent movement
    private int movementAction;
    private int rotationAction;
    public float moveSpeed = 5f;
    public float rotationSpeed = 100f;

    public override void Initialize()
    {
        m_GameController = GetComponentInParent<DungeonEscapeEnvController>();
        m_AgentRb = GetComponent<Rigidbody>();
        m_PushBlockSettings = FindObjectOfType<PushBlockSettings>();
        MyKey.SetActive(false);
        IHaveAKey = false;
    }

    public override void OnEpisodeBegin()
    {
        MyKey.SetActive(false);
        IHaveAKey = false;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(IHaveAKey);
    }

    private void FixedUpdate()
    {
        if (transform.localPosition.x < -37.5 || transform.localPosition.x > 37.5
            || transform.localPosition.z < -37.5 || transform.localPosition.z > 37.5
            || transform.localPosition.y < 0)
        {
            m_GameController.RespawnUnique(this);
        }
    }

    /// <summary>
    /// Moves the agent according to the selected action.
    /// </summary>
    public void MoveAgent(ActionSegment<int> act)
    {
        var direction = Vector3.zero;
        var rotateDir = Vector3.zero;

        var forwardAxis = act[0];
        var rightAxis = act[1];
        var rotateAxis = act[2];

        switch (forwardAxis)
        {
            case 1:
                direction = transform.forward * 1;
                break;
            case 2:
                direction = transform.forward * -1;
                break;
        }

        switch (rightAxis)
        {
            case 1:
                direction = transform.right * 1;
                break;
            case 2:
                direction = transform.right * -1;
                break;
        }

        switch (rotateAxis)
        {
            case 1:
                rotateDir = transform.up * -1f;
                break;
            case 2:
                rotateDir = transform.up * 1f;
                break;
        }
        transform.Rotate(rotateDir, Time.fixedDeltaTime * 200f);
        m_AgentRb.AddForce(direction * m_PushBlockSettings.agentRunSpeed,
            ForceMode.VelocityChange);
        /*var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;

        var action = act[0];

        switch (action)
        {
            case 1:
                dirToGo = transform.forward * 1f;
                break;
            case 2:
                dirToGo = transform.forward * -1f;
                break;
            case 3:
                rotateDir = transform.up * 1f;
                break;
            case 4:
                rotateDir = transform.up * -1f;
                break;
            case 5:
                dirToGo = transform.right * -0.75f;
                break;
            case 6:
                dirToGo = transform.right * 0.75f;
                break;
        }
        transform.Rotate(rotateDir, Time.fixedDeltaTime * 200f);
        m_AgentRb.AddForce(dirToGo, ForceMode.VelocityChange);*/
    }

    /// <summary>
    /// Called every step of the engine. Here the agent takes an action.
    /// </summary>
    public override void OnActionReceived(ActionBuffers actions)
    {
        // Move the agent using the action.
        //MoveAgent(actions.DiscreteActions);
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
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.transform.CompareTag("Door"))
        {
            if (IHaveAKey)
            {
                MyKey.SetActive(false);
                IHaveAKey = false;
                m_GameController.UnlockDoor();
            }
        }
        if (col.transform.CompareTag("dragon"))
        {
            m_GameController.KilledByBaddie(this, col);
            //MyKey.SetActive(false);
            //IHaveAKey = false;
        }
        if (col.transform.CompareTag("portal"))
        {
            m_GameController.TouchedHazard(this);
        }
        if (col.transform.CompareTag("Obstacle"))
        {
            AddReward(-.01f);
        }
        if (col.transform.CompareTag("Wall"))
        {
            AddReward(-0.1f);
            m_GameController.RespawnUnique(this);
        }
        if (col.transform.CompareTag("thief"))
        {
            m_GameController.ThiefDead(this, col);
        }
    }

    void OnTriggerEnter(Collider col)
    {
        //if we find a key and it's parent is the main platform we can pick it up
        if (col.transform.CompareTag("key") && col.transform.parent == transform.parent && gameObject.activeInHierarchy)
        {
            print("Picked up key");
            MyKey.SetActive(true);
            IHaveAKey = true;
            col.gameObject.SetActive(false);
            m_GameController.m_AgentGroup.AddGroupReward(.5f);
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        if (Input.GetKey(KeyCode.D))
        {
            discreteActionsOut[0] = 3;
        }
        else if (Input.GetKey(KeyCode.W))
        {
            discreteActionsOut[0] = 1;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            discreteActionsOut[0] = 4;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            discreteActionsOut[0] = 2;
        }
    }
}