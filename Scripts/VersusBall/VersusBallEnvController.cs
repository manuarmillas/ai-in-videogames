using Palmmedia.ReportGenerator.Core.Reporting.Builders;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class VersusBallEnvController : MonoBehaviour
{
    private VersusBallAgent[] agentList = new VersusBallAgent[2];
    private GameObject[] greens; // green balls
    private GameObject[] reds; // red balls
    [SerializeField] private VersusBallAgent bluePlayer;
    [SerializeField] private VersusBallAgent redPlayer;
    //[SerializeField] private GameObject goal;

    private int redScore = 0;
    private int blueScore = 0;
    private int greenBalls = 17;
    private int redBalls = 12;

    private void Start()
    {
        agentList[0] = bluePlayer;
        agentList[1] = redPlayer;
        greens = GameObject.FindGameObjectsWithTag("Reward");
        reds = GameObject.FindGameObjectsWithTag("Obstacle");
        BallsSpawner();
    }

    public void BallsSpawner()
    {
        for (int i = 0; i < greens.Length; i++)
        {
            greens[i].transform.localPosition = new Vector3(Random.Range(-70f, 22f), 0, Random.Range(-47f, 47f));
            greens[i].SetActive(true);
        }
        for (int i = 0; i < reds.Length; i++)
        {
            reds[i].transform.localPosition = new Vector3(Random.Range(-70f, 22f), 0, Random.Range(-47f, 47f));
            reds[i].SetActive(true);
        }
    }

    public void DeleteBall(GameObject ball)
    {
        ball.SetActive(false);
        if (ball.gameObject.tag == "Reward")
        {
            greenBalls -= 1;
        } else
        {
            redBalls -= 1;
        }
        if (greenBalls == 0) 
        {
            redBalls = 12;
            greenBalls = 17;
            FinishGame();
        }
    }

    private void FinishGame()
    {
        BallsSpawner(); // por el momento se queda así, para la fase de entrenamiento
        bluePlayer.EndEpisode();
        redPlayer.EndEpisode();
    }

    public void Score(int flag)
    {
        if (flag == 0)
        {
            redScore += 1;
        } else if (flag == 1)
        {
            blueScore+= 1;
        } else if (flag == 2)
        {
            redScore -= 1;
        } else
        {
            blueScore -= 1;
        }
        Debug.Log("Rojo: " + redScore + " - " + "Azul: " + blueScore);
    }

    /*public void ResolveEvent(int flag)
    {
        switch (flag)
        {
            case 1:
                bluePlayer.AddReward(-1f);
                redPlayer.AddReward(1f);
                Score(0);
                break;
            case 2:
                bluePlayer.AddReward(1f);
                redPlayer.AddReward(-1f);
                Score(1);
                break;
            case 3:
                bluePlayer.EndEpisode();
                redPlayer.EndEpisode();
                break;
            case 4:
                bluePlayer.EndEpisode();
                redPlayer.EndEpisode();
                break;
        }
    }*/
}
