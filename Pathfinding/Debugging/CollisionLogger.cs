using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class CollisionLogger : MonoBehaviour
{
    void OnCollisionEnter2D(Collision2D collision) => MyLog(collision);
    void OnCollisionStay2D(Collision2D collision) => MyLog(collision);
    //void OnCollisionExit2D(Collision2D collision) => MyLog(collision);

    void Log(Collision2D collision, [CallerMemberName] string message = null)
    {
        Debug.Log($"{message} called on {name} because a collision with {collision.collider.name}");
    }

    void MyLog(Collision2D collision, [CallerMemberName] string message = null)
    {
        Debug.Log($"{message} called on {name} because a collision with {collision.collider.name}");
        //Debug.Log("           CollisionLogger.cs");
    
        if (collision.gameObject.layer == 10)
        {
            Debug.Log("           CollisionLogger.cs              obstacle Object: " + collision.collider.name + "          obstacle layer: " + collision.gameObject.layer);
            Vector3 displacementVector = new Vector3(-1.05f, -1.05f, 0.0f);
            //gameObject.transform.Translate(Vector3.left * Time.deltaTime * 10.0f);
            gameObject.transform.Translate(displacementVector);
        }
    }

    /*
    void OnCollisionEnter2D(Collision2D collision)
    {
        //Debug.Log("           CollisionLogger.cs              original collider: " + collision.collider.name);                      // original collider is the counter top
        //Debug.Log("           CollisionLogger.cs              other collider: " + collision.otherCollider.name);                    // other collider is the enemy game object
        //Debug.Log("           CollisionLogger.cs              gameObject: " + gameObject.name + "          gameObject layer: " + gameObject.layer);
        //Debug.Log("           CollisionLogger.cs              obstacle Object: " + collision.collider.name + "          obstacle layer: " + collision.gameObject.layer);
        // if collision is an obstacle collision
        if (collision.gameObject.layer == 10)
        {
            Debug.Log("           CollisionLogger.cs              obstacle Object: " + collision.collider.name + "          obstacle layer: " + collision.gameObject.layer);
            Vector3 displacementVector = new Vector3(0.5f, 0.5f, 0.0f);
            //gameObject.transform.Translate(Vector3.left * Time.deltaTime * 10.0f);
            gameObject.transform.Translate(displacementVector);
        }



    }
    */

}