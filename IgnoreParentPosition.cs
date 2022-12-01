using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IgnoreParentPosition : MonoBehaviour
{
    public Vector3 position;

    // Update is called once per frame
    void LateUpdate()
    {
        transform.position = position;
    }
}
