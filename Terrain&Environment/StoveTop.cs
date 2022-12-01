using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StoveTop : MonoBehaviour
{
    [SerializeField]
    private Canvas canvas;

    [SerializeField]
    private CanvasGroup interactionUI;
    [SerializeField]
    private GameObject timerUI;


    [SerializeField]
    private GameObject explosion;

    [SerializeField]
    private string explosionSFX;
    [SerializeField]
    private string hissingSFX;


    private RoomManager roomManager;

    public float 
        damage,
        splashRange,
        force,
        timeToExplode;

    private PlayerController player;

    private bool spent;

    private void Awake()
    {
        roomManager = GameObject.FindGameObjectWithTag("RoomManager").GetComponent<RoomManager>();
        isTicking = false;
        spent = false;

        timerUI.SetActive(false);
    }

    // Start is called before the first frame update
    void Start()
    {
        DisallowInteration();

        canvas.worldCamera = Camera.main;
    }

    private void Update()
    {
        if (!spent)
        {
            if (interactionUI.alpha == 1 && !isTicking)
            {
                if (player.Input.InteractionInput)
                {
                    player.Input.UseInteractionInput();
                    DisallowInteration();

                    TimeBomb();
                }
            }
        }
    }

    private void TimeBomb()
    {
        print("Time bomb activated");

        StartCoroutine(TickingTimeBomb());
    }

    bool isTicking = false;
    IEnumerator TickingTimeBomb()
    {
        isTicking = true;

        float timer = timeToExplode;

        timerUI.SetActive(true);

        AudioManager.instance.PlaySoundFromList(hissingSFX);

        while (isTicking)
        {

            timer -= Time.deltaTime;

            timerUI.GetComponent<TextMeshProUGUI>().text = timer.ToString("F0");


            if (timer <= 0)
            {
                GenerateExplosion();
                isTicking = false;
                spent = true;
                timerUI.SetActive(false);
                break;
            }

            yield return new WaitForEndOfFrame();
        }
    }

    private void GenerateExplosion()
    {
        print("Explosion generated!");
        GameObject newExplosion = Instantiate(explosion, this.transform.position, Quaternion.identity);

        AudioManager.instance.PlaySoundFromList(explosionSFX);


        var colliders = Physics2D.OverlapCircleAll(transform.position, splashRange);

        foreach (Collider2D collider in colliders)
        {
            var enemy = collider.GetComponent<Enemy>();
            var player = collider.GetComponent<PlayerController>();

            if (player != null)
            {
                player.DamagePlayer(damage);
                player.rb.AddForce((player.transform.position - this.transform.position)*force, ForceMode2D.Impulse);
            }

            if (enemy != null)
            {
                enemy.DamageEnemy(damage);
            }
        }

        roomManager.SpawnFire(this.transform.position, splashRange);

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<PlayerController>() && !spent)
        {
            player = collision.GetComponent<PlayerController>();
            //activate UI and system for interaction
            AllowInteraction();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponent<PlayerController>())
        {
            //activate UI and system for interaction
            DisallowInteration();
        }

    }

    private void AllowInteraction()
    {
        interactionUI.alpha = 1;
    }

    private void DisallowInteration()
    {
        interactionUI.alpha = 0;

    }
}
