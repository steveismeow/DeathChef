using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffPickup : MonoBehaviour
{
    [SerializeField]
    private Canvas canvas;

    [SerializeField]
    private CanvasGroup interactionUI;

    [SerializeField]
    private string pickUpSFX;

    private PlayerController player;

    public enum StatToModify
    {
        attackDamage,
        health,
        movementSpeed,
        projectileLife,
        projectileNumber,
        projectileSpeed,
        rateOfFire,


    }

    public StatToModify statToModify;

    public float value;

    // Start is called before the first frame update
    void Start()
    {
        DisallowInteration();

        canvas.worldCamera = Camera.main;
    }

    private void Update()
    {
        if (interactionUI.alpha == 1)
        {
            if (player.Input.InteractionInput)
            {
                player.Input.UseInteractionInput();

                PickUp();
            }
        }
    }

    private void PickUp()
    {
        DisallowInteration();

        player.ModifyStat(statToModify, value);

        player.UpdateBuffUI(this.GetComponent<SpriteRenderer>().sprite);

        AudioManager.instance.PlaySoundFromList(pickUpSFX);

        InterWaveManager.instance.RemoveBuffsFromField();

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<PlayerController>())
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
