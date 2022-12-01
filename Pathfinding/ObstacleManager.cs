using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class ObstacleManager : MonoBehaviour
{

    // Ultimate goal is to make one or more functions that get called when an object's BoxCollider2D component changes.
    // This function takes the GameObject, accesses its BoxCollider2D component, then calls AstarPath.active.UpdateGraphs (gameObj.collider.bounds);
    public void PathfindingTargetedGraphUpdate(GameObject changedObstacle)
    {
        BoxCollider2D obstacleBoxCollider = changedObstacle.GetComponent<BoxCollider2D>();
        Debug.Log("          ObstacleManager.cs          " + changedObstacle.name + "'s collider: " + obstacleBoxCollider.bounds);
        AstarPath.active.UpdateGraphs(obstacleBoxCollider.bounds);
        
    }

    public void PathfindingFullGraphUpdate()
    {
        AstarPath.active.Scan();
    }

    public void AssignPlayerToEnemies(AIDestinationSetter enemyAI, Transform player)
    {
        if (enemyAI != null)
        {
            enemyAI.target = player;
        }
    }
}
