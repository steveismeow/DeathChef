using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Discus : MonoBehaviour
{
    public Projectile projectile;

    [SerializeField]
    float colliderBufferTime;

    // Start is called before the first frame update
    void Start()
    {
        projectile = this.GetComponent<Projectile>();
    }

    private void OnEnable()
    {
        this.GetComponent<CircleCollider2D>().enabled = false;
        StartCoroutine(ColliderBuffer());
    }

    bool colliderOff;
    IEnumerator ColliderBuffer()
    {
        colliderOff = true;
        float timer = colliderBufferTime;
        while(colliderOff)
        {
            timer -= Time.deltaTime;

            if (timer <= 0)
            {
                this.GetComponent<CircleCollider2D>().enabled = true;
                break;
            }

            yield return new WaitForEndOfFrame();
        }
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        var direction = Vector2.Reflect(projectile.facing, collision.contacts[0].normal);

        projectile.facing = direction;
    }
}
