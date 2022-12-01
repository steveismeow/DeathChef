using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public PlayerController player;
    public AstarAI ai;
    public Animator anim;

    public Rigidbody2D rb;

    [SerializeField]
    private GameObject bloodSpray;
    [SerializeField]
    private GameObject bloodSprayOnDeath;


    [SerializeField]
    private string damageSFX;

   
    public float
        maxHealth,
        health,
        movementSpeed,
        attackDamage,
        attackBufferTime;

    [SerializeField]
    private bool doesntTrackPlayer;


    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();

        health = maxHealth;

    }

    // Update is called once per frame
    void Update()
    {
        Animate();
    }

    private void Animate()
    {
        Vector2 facing = player.transform.position - this.transform.position;

        if (!doesntTrackPlayer)
        {
            anim.SetFloat("directionX", facing.x);
            anim.SetFloat("directionY", facing.y);
        }
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject == player.gameObject && !damageTimeOut)
        {
            player.DamagePlayer(attackDamage);
            StartCoroutine(DamageCooldown());
        }
    }

    bool damageTimeOut;
    IEnumerator DamageCooldown()
    {
        while (damageTimeOut)
        {
            float timer = attackBufferTime;

            timer -= Time.deltaTime;

            if (timer <= 0)
            {
                damageTimeOut = false;
            }

            yield return new WaitForEndOfFrame();
        }
    }

    public void DamageEnemy(float damageValue)
    {
        health -= damageValue;

        BloodSpray();
        AudioManager.instance.PlaySoundFromList(damageSFX);

        if (health <= 0)
        {
            DisableAndRecycleEnemy();
            WaveManager.instance.CheckWinState();
        }
    }

    private void BloodSpray()
    {
        GameObject newBloodSpray = Instantiate(bloodSpray, this.transform.position, Quaternion.identity);
    }

    public void DisableAndRecycleEnemy()
    {
        GameObject newBloodSpray = Instantiate(bloodSprayOnDeath, this.transform.position, Quaternion.identity);

        this.gameObject.SetActive(false);
        health = maxHealth;
        WaveManager.instance.EnemiesInPlay.Remove(this);
        Debug.Log("Enemy" + this.gameObject.name + "destroyed!");
    }
}
