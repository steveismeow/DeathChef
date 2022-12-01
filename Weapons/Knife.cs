using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Knife : MonoBehaviour
{
    public Projectile projectile;
    public Rigidbody2D rb;

    public GameObject projectileSprite;

    private void OnEnable()
    {
        OrientProjectile();
        ColliderBuffer();
    }

    // Start is called before the first frame update
    void Start()
    {
        projectile = this.GetComponent<Projectile>();
        rb = this.GetComponent<Rigidbody2D>();

    }

    // Update is called once per frame
    void FixedUpdate()
    {

    }

    private void ColliderBuffer()
    {
        StartCoroutine(ColliderTimer());
    }

    bool colliderTimeOut;
    IEnumerator ColliderTimer()
    {
        colliderTimeOut = true;

        float timer = 0.2f;
        
        projectile.collider.enabled = false;

        while(colliderTimeOut)
        {

            timer -= Time.deltaTime;

            if (timer <= 0)
            {
                projectile.collider.enabled = true;
            }

            yield return new WaitForFixedUpdate();
        }
    }


    private void OrientProjectile()
    {
        var angle = Mathf.Atan2(projectile.facing.x, projectile.facing.y) * Mathf.Rad2Deg;

        projectileSprite.transform.rotation = Quaternion.AngleAxis(angle, Vector3.back);

    }
}
