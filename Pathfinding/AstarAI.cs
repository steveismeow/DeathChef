using UnityEngine;
using System.Collections;
// Note this line, if it is left out, the script won't know that the class 'Path' exists and it will throw compiler errors
// This line should always be present at the top of scripts which use pathfinding
using Pathfinding;
public class AstarAI : MonoBehaviour {
    public Vector3 targetPosition;
    public void Start () {
        // Get a reference to the Seeker component we added earlier
        Seeker seeker = GetComponent<Seeker>();
        // Start to calculate a new path to the targetPosition, return the result to the OnPathComplete method.
        // Path requests are asynchronous, so when the OnPathComplete method is called depends on how long it
        // takes to calculate the path. Usually it is called the next frame.
        seeker.StartPath(transform.position, targetPosition, OnPathComplete);
    }
    public void OnPathComplete (Path p) {
        Debug.Log("Yay, we got a path back. Did it have an error? " + p.error);
    }
} 