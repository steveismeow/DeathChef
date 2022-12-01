using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    public HeldWeapon heldWeapon;

    public PlayerController player;

    public List<GameObject> weapons = new List<GameObject>();

    [SerializeField]
    private List<GameObject> weaponPickUps = new List<GameObject>();

    public int startingWeaponIndex;

    private Transform pickUpContainer;


    private void OnEnable()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();

        pickUpContainer = this.transform.GetChild(0);

        foreach (Transform child in transform)
        {
            weapons.Add(child.gameObject);
        }
    }

    //Gameobject version
    public void ChangeEquippedWeapon(GameObject incomingWeapon)
    {
        if (player.equippedWeapon != null)
        {
            DropCurrentWeapon();

        }

        AudioManager.instance.PlaySoundFromList("Cocked");


        foreach (Transform child in transform)
        {
            if (child.name == incomingWeapon.name + "(Clone)")
            {
                player.equippedWeapon = child.GetComponent<Weapon>();
                heldWeapon.ChangeEquippedWeaponSprite(player.equippedWeapon.weaponSprite);
                return;

            }
        }

        //instantiate new Weapon gameobject as child, and generate as many projectiles as the Weapon script indicates is required?
        GameObject newWeapon = Instantiate(incomingWeapon, this.transform);

        player.equippedWeapon = newWeapon.GetComponent<Weapon>();
        heldWeapon.ChangeEquippedWeaponSprite(player.equippedWeapon.weaponSprite);

    }

    //string version
    public void ChangeEquippedWeapon(string incomingWeapon)
    {
        //alter the specific projectile stack/weapon that the player has access to
        //PlayerController.instance.equippedWeapon = incomingWeapon;

        //AudioManager.instance.PlaySoundFromList("Cocked");


        string[] data = incomingWeapon.Split('(', ')');

        foreach (GameObject weapon in weapons)
        {
            if (weapon.name == data[0])
            {
                foreach (Transform child in transform)
                {
                    if (child.name == incomingWeapon + "(Clone)")
                    {
                        player.equippedWeapon = child.GetComponent<Weapon>();
                        heldWeapon.ChangeEquippedWeaponSprite(player.equippedWeapon.weaponSprite);
                        return;

                    }
                }

                //instantiate new Weapon gameobject as child, and generate as many projectiles as the Weapon script indicates is required?
                GameObject newWeaponFromList = Instantiate(weapon, this.transform);

                player.equippedWeapon = newWeaponFromList.GetComponent<Weapon>();
                heldWeapon.ChangeEquippedWeaponSprite(player.equippedWeapon.weaponSprite);

                return;
            }
        }

        print("Couldn't find weapon, instantiating default");

        GameObject newWeapon = Instantiate(weapons[startingWeaponIndex], this.transform);

        player.equippedWeapon = newWeapon.GetComponent<Weapon>();
        heldWeapon.ChangeEquippedWeaponSprite(player.equippedWeapon.weaponSprite);


    }

    private void DropCurrentWeapon()
    {
        foreach (GameObject pickUp in weaponPickUps)
        {
            if (player.equippedWeapon.name == pickUp.GetComponent<WeaponPickUp>().weaponPrefab.name + "(Clone)")
            {

                foreach(Transform child in pickUpContainer)
                {

                    if (child.name == pickUp.name + "(Clone)")
                    {

                        if (!child.gameObject.activeInHierarchy)
                        {

                            child.transform.position = player.transform.position;
                            child.gameObject.SetActive(true);
                            return;
                        }
                        else
                        {
                            continue;
                        }
                    }

                }

                GameObject newWeaponPickUp = Instantiate(pickUp, player.transform.position, Quaternion.identity, pickUpContainer.transform);
                return;
            }
        }

    }


}
