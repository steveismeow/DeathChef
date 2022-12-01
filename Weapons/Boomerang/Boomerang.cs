using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boomerang : MonoBehaviour
{

    public Projectile projectile;

    public bool autoCatchOn;

    public float secondsTillReverse;

    public float catchDistance;

    private Transform playerTransform;

    private bool canBeCaught;

    //private Vector2 originalFacing;

    private float secondsSinceEnabled = 0f;
    private void OnEnable()
    {
        secondsSinceEnabled = 0f;
        canBeCaught = false;

        //originalFacing = projectile.facing;
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
    }

    // Start is called before the first frame update
    void Start()
    {
        projectile = this.GetComponent<Projectile>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        secondsSinceEnabled += Time.deltaTime;
        if (secondsSinceEnabled > secondsTillReverse)
        {
            projectile.facing = Vector2.Lerp(projectile.facing, playerTransform.position - this.transform.position, Time.deltaTime);
            canBeCaught = true;
        }

        if (autoCatchOn)
        {
            if (canBeCaught)
            {
                var distanceToPlayer = Vector2.Distance(this.transform.position, playerTransform.position);

                if (distanceToPlayer <= catchDistance)
                {
                    this.gameObject.GetComponent<Projectile>().DestroyProjectile();
                }
            }
        }

        //if (secondsSinceEnabled > secondsTillReverse) {
        //    projectile.facing = Vector2.Lerp(projectile.facing, originalFacing * -1, reverseDirectionLerpFactor);
        //}
        //rb.MovePosition(facing * speed * Time.deltaTime);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
