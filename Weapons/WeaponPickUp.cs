using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponPickUp : MonoBehaviour
{
    [SerializeField]
    private Canvas canvas;

    [SerializeField]
    private CanvasGroup interactionUI;

    [SerializeField]
    private string pickUpSFX;

    public GameObject weaponPrefab;

    private PlayerController player;

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
        player.weaponManager.ChangeEquippedWeapon(weaponPrefab);

        AudioManager.instance.PlaySoundFromList(pickUpSFX);
        DisallowInteration();
        RemovePickUp();

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<PlayerController>())
        {
            //player = collision.GetComponent<PlayerController>();
            //activate UI and system for interaction
            player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();

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

    public void RemovePickUp()
    {
        this.gameObject.SetActive(false);
    }
}
