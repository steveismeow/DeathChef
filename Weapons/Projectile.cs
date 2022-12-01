using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{

    public Vector2 facing;

    [SerializeField]
    private Rigidbody2D rb;

    public BoxCollider2D collider;

    public Weapon weaponData;

    [SerializeField]
    private string hitSFX;

    public bool destroyOnHitEnemy;
    public bool destroyOnHitWall;


    public bool dontMoveOnAwake;

    private void OnEnable()
    {
        if (!dontMoveOnAwake)
        {
            Coroutine tickLife = StartCoroutine(TickLife());
        }
    }

    private void Awake()
    {
        rb = this.GetComponent<Rigidbody2D>();
        collider = this.GetComponent<BoxCollider2D>();
        weaponData = this.GetComponentInParent<Weapon>();
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        if (dontMoveOnAwake)
        {

        }
        else
        {
            transform.Translate(facing * weaponData.speed * weaponData.player.projectileSpeedMultiplier * Time.deltaTime);
        }

        //rb.MovePosition(facing * speed * Time.deltaTime);
    }

    public IEnumerator TickLife()
    {
        float lifeSpan = weaponData.lifeSpan * weaponData.player.projectileLifeMultiplier;
        
        while (true)
        {

            lifeSpan -= Time.deltaTime;

            yield return new WaitForEndOfFrame();

            if (lifeSpan <= 0)
            {
                break;
            }
        }

        DestroyProjectile();
    }

    public void DestroyProjectile()
    {
        gameObject.SetActive(false);
        gameObject.transform.position = Vector3.zero;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        var enemy = collision.gameObject.GetComponent<Enemy>();

        if (enemy != null)
        {
            enemy.DamageEnemy(weaponData.attackDamage);
            AudioManager.instance.PlaySoundFromList(hitSFX);

            if (destroyOnHitEnemy)
            {
                DestroyProjectile();
            }
        }

        if (collision.gameObject.CompareTag("Walls"))
        {
            if (destroyOnHitWall)
            {
                DestroyProjectile();
            }
        }
    }
}
