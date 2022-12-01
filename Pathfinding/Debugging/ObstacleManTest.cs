using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class ObstacleManTest : MonoBehaviour
{
    [SerializeField]
    public GameObject obstacle1;
    public GameObject obstacle2;
    public float obstacleBlinkFrequency = 2.0f;
    public float timeLog = 0.0f;

    public BoxCollider2D obstacle1Collider;
    public BoxCollider2D obstacle2Collider;


    // Start is called before the first frame update
    void Awake()
    {

        //obstacle1.SetActive(true);
        //obstacle2.SetActive(true);


        obstacle1Collider = obstacle1.GetComponent<BoxCollider2D>();
        obstacle2Collider = obstacle2.GetComponent<BoxCollider2D>();        
        //Debug.Log("          ObstacleManager.cs          obstacle1 collider: " + obstacle1Collider.bounds);

        //Debug.Log("          ObstacleManager.cs          obstacle1 collider: " + obstacle1.collider.bounds);


    }

    // Update is called once per frame
    void Update()
    {
        timeLog += Time.deltaTime;
        if (Mathf.Floor(timeLog) % obstacleBlinkFrequency == 0)
        {
            obstacle1.SetActive(false);
            obstacle2.SetActive(true);

            //Debug.Log("          ObstacleManager.cs          obstacle1: not active, obstacle2: active");
            

        }
        else
        {
            obstacle1.SetActive(true);
            obstacle2.SetActive(false);

            //Debug.Log("          ObstacleManager.cs          obstacle1: active, obstacle2: not active");
        }

        if (obstacle1Collider.enabled)
        {

            Debug.Log("          ObstacleManager.cs          obstacle1 collider: " + obstacle1Collider.bounds);
            AstarPath.active.UpdateGraphs(obstacle1Collider.bounds);

        }

        if (obstacle2Collider.enabled)
        {
            Debug.Log("          ObstacleManager.cs          obstacle2 collider: " + obstacle2Collider.bounds);
            AstarPath.active.UpdateGraphs(obstacle2Collider.bounds);
        }

    }

}
