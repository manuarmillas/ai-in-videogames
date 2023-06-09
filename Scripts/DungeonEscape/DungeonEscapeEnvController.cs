using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;
using static UnityEditor.Progress;

public class DungeonEscapeEnvController : MonoBehaviour
{
    [System.Serializable]
    public class PlayerInfo
    {
        public PushAgentEscape Agent;
        [HideInInspector]
        public Vector3 StartingPos;
        [HideInInspector]
        public Quaternion StartingRot;
        [HideInInspector]
        public Rigidbody Rb;
        [HideInInspector]
        public Collider Col;
    }

    [System.Serializable]
    public class DragonInfo
    {
        public SimpleNPC Agent;
        [HideInInspector]
        public Vector3 StartingPos;
        [HideInInspector]
        public Quaternion StartingRot;
        [HideInInspector]
        public Rigidbody Rb;
        [HideInInspector]
        public Collider Col;
        public Transform T;
        public bool IsDead;
    }

    [System.Serializable]
    public class ThiefInfo
    {
        public ThiefScript Agent;
        [HideInInspector]
        public Vector3 StartingPos;
        [HideInInspector]
        public Quaternion StartingRot;
        [HideInInspector]
        public Rigidbody Rb;
        [HideInInspector]
        public Collider Col;
        public Transform T;
        public bool IsDead;
    }

    public int MaxEnvironmentSteps = 25000;
    [SerializeField] private int stateOfGame;
    private int m_ResetTimer;

    /// <summary>
    /// The area bounds.
    /// </summary>
    [HideInInspector]
    public Bounds areaBounds;
    /// <summary>
    /// The ground. The bounds are used to spawn the elements.
    /// </summary>
    public GameObject ground;

    Material m_GroundMaterial; //cached on Awake()

    /// <summary>
    /// We will be changing the ground material based on success/failue
    /// </summary>
    Renderer m_GroundRenderer;

    public List<PlayerInfo> AgentsList = new List<PlayerInfo>();
    public List<DragonInfo> DragonsList = new List<DragonInfo>();
    public List<ThiefInfo> ThiefList = new List<ThiefInfo>();
    private Dictionary<PushAgentEscape, PlayerInfo> m_PlayerDict = new Dictionary<PushAgentEscape, PlayerInfo>();
    public bool UseRandomAgentRotation = true;
    public bool UseRandomAgentPosition = true;
    PushBlockSettings m_PushBlockSettings;

    private int m_NumberOfRemainingPlayers;
    public GameObject Key;
    //public GameObject Tombstone;
    public SimpleMultiAgentGroup m_AgentGroup;
    void Start()
    {

        // Get the ground's bounds
        areaBounds = ground.GetComponent<Collider>().bounds;
        // Get the ground renderer so we can change the material when a goal is scored
        m_GroundRenderer = ground.GetComponent<Renderer>();
        // Starting material
        m_GroundMaterial = m_GroundRenderer.material;
        m_PushBlockSettings = FindObjectOfType<PushBlockSettings>();

        //Reset Players Remaining
        m_NumberOfRemainingPlayers = AgentsList.Count;

        //Hide The Key
        Key.SetActive(false);

        // Initialize TeamManager
        m_AgentGroup = new SimpleMultiAgentGroup();
        foreach (var item in AgentsList)
        {
            item.StartingPos = item.Agent.transform.position;
            item.StartingRot = item.Agent.transform.rotation;
            item.Rb = item.Agent.GetComponent<Rigidbody>();
            item.Col = item.Agent.GetComponent<Collider>();
            // Add to team manager
            m_AgentGroup.RegisterAgent(item.Agent);
        }
        if (stateOfGame == 0)
        {
            foreach (var item in DragonsList)
            {
                item.StartingPos = item.Agent.transform.position;
                item.StartingRot = item.Agent.transform.rotation;
                item.T = item.Agent.transform;
                item.Col = item.Agent.GetComponent<Collider>();
            }
        } else if (stateOfGame == 1)
        {
            foreach (var item in ThiefList)
            {
                item.StartingPos = item.Agent.transform.position;
                item.StartingRot = item.Agent.transform.rotation;
                item.T = item.Agent.transform;
                item.Col = item.Agent.GetComponent<Collider>();
            }
        }

        ResetScene();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        m_ResetTimer += 1;
        if (m_ResetTimer >= MaxEnvironmentSteps && MaxEnvironmentSteps > 0)
        {
            m_AgentGroup.GroupEpisodeInterrupted();
            ResetScene();
        }
    }

    public void TouchedHazard(PushAgentEscape agent)
    {
        m_NumberOfRemainingPlayers--;
        if (m_NumberOfRemainingPlayers == 0 || agent.IHaveAKey)
        {
            m_AgentGroup.EndGroupEpisode();
            ResetScene();
        }
        else
        {
            agent.gameObject.SetActive(false);
        }
    }

    public void UnlockDoor()
    {
        m_AgentGroup.AddGroupReward(1f);
        //StartCoroutine(GoalScoredSwapGroundMaterial(m_PushBlockSettings.goalScoredMaterial, 0.5f));

        print("Unlocked Door");
        m_AgentGroup.EndGroupEpisode();

        ResetScene();
    }

    public void KilledByBaddie(PushAgentEscape agent, Collision baddieCol)
    {
        baddieCol.gameObject.SetActive(false);
        m_NumberOfRemainingPlayers--;
        agent.gameObject.SetActive(false);
        print($"{baddieCol.gameObject.name} ate {agent.transform.name}");

        //Spawn Tombstone
        /*Tombstone.transform.SetPositionAndRotation(agent.transform.localPosition, agent.transform.rotation);
        Tombstone.SetActive(true);*/

        //Spawn the Key Pickup
        Key.transform.localPosition = new Vector3(baddieCol.collider.transform.localPosition.x, 1f, baddieCol.collider.transform.localPosition.z);//(baddieCol.collider.transform.localPosition, baddieCol.collider.transform.rotation);
        //Debug.Log("Key location: " + Key.transform.localPosition);
        Key.SetActive(true);
    }

    /// <summary>
    /// Use the ground's bounds to pick a random spawn position.
    /// </summary>
    public Vector3 GetRandomSpawnPos()
    {
        //var foundNewSpawnLocation = false;
        var randomSpawnPos = Vector3.zero;
        /*while (foundNewSpawnLocation == false)
        {*/
            var randomPosX = Random.Range(-37.5f * 0.7f, 37.5f * 0.7f);
            var randomPosZ = Random.Range(-37.5f * 0.7f, 37.5f * 0.7f);
            randomSpawnPos = /*ground.transform.position + */new Vector3(randomPosX, 1f, randomPosZ);
            /*if (Physics.CheckBox(randomSpawnPos, new Vector3(2.5f, 0.01f, 2.5f)) == false)
            {
                foundNewSpawnLocation = true;
            }*/
        //}
        return randomSpawnPos;
    }

    /// <summary>
    /// Swap ground material, wait time seconds, then swap back to the regular material.
    /// </summary>
    IEnumerator GoalScoredSwapGroundMaterial(Material mat, float time)
    {
        m_GroundRenderer.material = mat;
        yield return new WaitForSeconds(time); // Wait for 2 sec
        m_GroundRenderer.material = m_GroundMaterial;
    }

    public void BaddieTouchedBlock()
    {
        m_AgentGroup.EndGroupEpisode();

        // Swap ground material for a bit to indicate we scored.
        //StartCoroutine(GoalScoredSwapGroundMaterial(m_PushBlockSettings.failMaterial, 0.5f));
        ResetScene();
    }

    Quaternion GetRandomRot()
    {
        return Quaternion.Euler(0, Random.Range(0.0f, 360.0f), 0);
    }

    void ResetScene()
    {

        //Reset counter
        m_ResetTimer = 0;

        //Reset Players Remaining
        m_NumberOfRemainingPlayers = AgentsList.Count;

        //Random platform rot
        /*var rotation = Random.Range(0, 4);
        var rotationAngle = rotation * 90f;
        transform.Rotate(new Vector3(0f, rotationAngle, 0f));*/

        //Reset Agents
        foreach (var item in AgentsList)
        {
            var pos = UseRandomAgentPosition ? GetRandomSpawnPos() : item.StartingPos;
            var rot = UseRandomAgentRotation ? GetRandomRot() : item.StartingRot;

            item.Agent.transform.SetPositionAndRotation(pos, rot);
            item.Rb.velocity = Vector3.zero;
            item.Rb.angularVelocity = Vector3.zero;
            item.Agent.MyKey.SetActive(false);
            item.Agent.IHaveAKey = false;
            item.Agent.gameObject.SetActive(true);
            m_AgentGroup.RegisterAgent(item.Agent);
        }

        //Reset Key
        Key.SetActive(false);

        //Reset Tombstone
        //Tombstone.SetActive(false);

        //End Episode
        if (stateOfGame == 0)
        {
            foreach (var item in DragonsList)
            {
                if (!item.Agent)
                {
                    return;
                }
                item.Agent.transform.SetPositionAndRotation(item.StartingPos, item.StartingRot);
                item.Agent.SetRandomWalkSpeed();
                item.Agent.gameObject.SetActive(true);
            }
        } else if (stateOfGame == 1)
        {
            foreach (var item in ThiefList)
            {
                if (!item.Agent)
                {
                    return;
                }
                item.Agent.transform.SetPositionAndRotation(item.StartingPos, item.StartingRot);
                //item.Agent.SetRandomDirection();
                item.Agent.gameObject.SetActive(true);
            }
        }
    }

    public void RespawnUnique(PushAgentEscape agent)
    {
        var pos = GetRandomSpawnPos();
        var rot = GetRandomRot();
        agent.transform.SetPositionAndRotation(pos, rot);
        agent.GetComponent<Rigidbody>().velocity = Vector3.zero;
        agent.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        agent.gameObject.SetActive(true);
    }

    public void RespawnUnique(PushAgentEscapeThief agent)
    {
        var pos = GetRandomSpawnPos();
        var rot = GetRandomRot();
        agent.transform.SetPositionAndRotation(pos, rot);
        agent.GetComponent<Rigidbody>().velocity = Vector3.zero;
        agent.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        agent.gameObject.SetActive(true);
    }

    public void ThiefDead(PushAgentEscape agent, Collision col)
    {
        col.gameObject.SetActive(false);
        m_NumberOfRemainingPlayers--;
        print($"{col.gameObject.name} killed by {agent.transform.name}");
        m_AgentGroup.AddGroupReward(1f);
        float keyPosX = 0;
        float keyPosZ = 0;

        //Spawn Tombstone
        /*Tombstone.transform.SetPositionAndRotation(agent.transform.localPosition, agent.transform.rotation);
        Tombstone.SetActive(true);*/
        if (col.collider.transform.localPosition.x + 4 >= 36)
        {
            keyPosX = col.collider.transform.localPosition.x - 4;
        } else
        {
            keyPosX = col.collider.transform.localPosition.x - 4;
        }

        if (col.collider.transform.localPosition.z + 4 >= 36)
        {
            keyPosZ = col.collider.transform.localPosition.x - 4;
        }
        else
        {
            keyPosZ = col.collider.transform.localPosition.x - 4;
        }

        //Spawn the Key Pickup
        Key.transform.localPosition = new Vector3(keyPosX, 1f, keyPosZ);//(baddieCol.collider.transform.localPosition, baddieCol.collider.transform.rotation);
        //Debug.Log("Key location: " + Key.transform.localPosition);
        Key.SetActive(true);
    }
}
