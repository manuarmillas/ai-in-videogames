using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootballController : MonoBehaviour
{
    public GameObject area;
    [HideInInspector] public FootballEnvController envController;
    public string blueGoalTag;
    public string redGoalTag;

    // Start is called before the first frame update
    void Start()
    {
        envController= GetComponentInParent<FootballEnvController>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(redGoalTag))
        {
            envController.Goal(Team.Blue);
        }
        if (collision.gameObject.CompareTag(blueGoalTag) ) 
        { 
            envController.Goal(Team.Red);
        }
    }

}
