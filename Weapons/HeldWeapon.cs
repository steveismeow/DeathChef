using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeldWeapon : MonoBehaviour
{
    public WeaponManager weaponManager;
    public SpriteRenderer spriteRenderer;

    public Sprite defaultNullSprite;

    private void Awake()
    {
        spriteRenderer = this.GetComponent<SpriteRenderer>();
    }

    public void ChangeEquippedWeaponSprite(Sprite incomingWeaponSprite)
    {
        if (incomingWeaponSprite == null)
        {
            spriteRenderer.sprite = defaultNullSprite;
        }
        else
        {
            spriteRenderer.sprite = incomingWeaponSprite;
        }

        GetComponentInParent<PlayerController>().gameplayUI.equippedWeaponSprite.sprite = incomingWeaponSprite;

    }
}
