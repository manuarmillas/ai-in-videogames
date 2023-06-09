using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.VisualScripting;

public class FootballEnvController : MonoBehaviour
{
    [System.Serializable]
    public class PlayerInfo
    {
        public AgentFootball agent;
        [HideInInspector] public Vector3 startingPos;
        [HideInInspector] public Quaternion startingRot;
        [HideInInspector] public Rigidbody rb;
    }
    public int MaxEnvSteps = 50000;

    public GameObject ball;
    [HideInInspector] public Rigidbody ballRB;
    Vector3 ballStartingPos;

    public List<PlayerInfo> AgentsList = new List<PlayerInfo>();
    public List<PlayerInfo> AgentsListObs = new List<PlayerInfo>();

    private FootballParams footballParams;

    private SimpleMultiAgentGroup blueAgentGroup;
    private SimpleMultiAgentGroup redAgentGroup;

    private int resetTimer;

    void Start()
    {
        footballParams = FindObjectOfType<FootballParams>();
        blueAgentGroup = new SimpleMultiAgentGroup();
        redAgentGroup = new SimpleMultiAgentGroup();
        ballRB = ball.GetComponent<Rigidbody>();
        ballStartingPos = new Vector3(ball.transform.localPosition.x, ball.transform.localPosition.y, ball.transform.localPosition.z);
        foreach (var i in AgentsList)
        {
            i.startingPos = i.agent.transform.localPosition;
            i.startingRot = i.agent.transform.rotation;
            i.rb = i.agent.GetComponent<Rigidbody>();
            if (i.agent.team == Team.Blue)
            {
                blueAgentGroup.RegisterAgent(i.agent);
            }
            else
            {
                redAgentGroup.RegisterAgent(i.agent);
            }
        }
        foreach (var i in AgentsListObs)
        {
            i.startingPos = i.agent.transform.localPosition;
            i.startingRot = i.agent.transform.rotation;
            i.rb = i.agent.GetComponent<Rigidbody>();
            if (i.agent.team == Team.Blue)
            {
                blueAgentGroup.RegisterAgent(i.agent);
            }
            else
            {
                redAgentGroup.RegisterAgent(i.agent);
            }
        }
        ResetScene();
    }

    private void FixedUpdate()
    {
        resetTimer += 1;
        if (resetTimer >= MaxEnvSteps && MaxEnvSteps > 0)
        {
            //blueAgentGroup.GroupEpisodeInterrupted();
            //redAgentGroup.GroupEpisodeInterrupted();
            ResetScene();
        }
        if (ball.transform.localPosition.y < 0)
        {
            ResetBall();
        }
    }

    public void ResetBall()
    {
        var randomPosX = Random.Range(-2.5f, 2.5f);
        var randomPosZ = Random.Range(-2.5f, 2.5f);

        ball.transform.localPosition = ballStartingPos + new Vector3(randomPosX, 0f, randomPosZ);
        ballRB.velocity = Vector3.zero;
        ballRB.angularVelocity = Vector3.zero;
    }

    public void ResetScene()
    {
        resetTimer= 0;

        //aqui iba un AgentsList
        foreach (var i in AgentsListObs)
        {
            var randomPosZ = Random.Range(-5f,5f);
            var newStartPos = i.agent.initialPos + new Vector3(0, 1, randomPosZ);
            var rot = i.agent.rotationSign * Random.Range(80.0f, 100.0f);
            var newRot = Quaternion.Euler(0, rot, 0);
            i.agent.transform.SetLocalPositionAndRotation(newStartPos, newRot);
            i.rb.velocity = Vector3.zero;
            i.rb.angularVelocity= Vector3.zero;
        }
        ResetBall();
    }

    public void Goal(Team scored)
    {
        if (scored == Team.Blue)
        {
            Debug.Log("gol azul");
            blueAgentGroup.AddGroupReward(1 - (float)resetTimer / MaxEnvSteps);
            redAgentGroup.AddGroupReward(-1f);
        } else
        {
            Debug.Log("gol rojo");
            redAgentGroup.AddGroupReward(1 - (float)resetTimer / MaxEnvSteps);
            blueAgentGroup.AddGroupReward(-1f);
        }
        //redAgentGroup.EndGroupEpisode();
        //blueAgentGroup.EndGroupEpisode();
        ResetScene();
    }
}
