using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public PlayerController player;

    public float
        lifeSpan,
        rateOfFire,
        attackDamage,
        speed;

    public List<GameObject> projectiles = new List<GameObject>();

    public Sprite weaponSprite;

    [SerializeField]
    private string fireSFX;

    private void Awake()
    {
        foreach (Transform child in transform)
        {
            projectiles.Add(child.gameObject);
            child.gameObject.SetActive(false);

        }

        player = GetComponentInParent<WeaponManager>().player;
    }

    //Finds first active projectile in pool
    public int FindProjectile()
    {
        for (int i = 0; i < projectiles.Count; i++)
        {
            if (!projectiles[i].activeInHierarchy)
            {
                return i;
            }

        }
        CreateNewProjectile();
        return projectiles.Count - 1;
    }

    private void CreateNewProjectile()
    {

        GameObject newProjectile = Instantiate(projectiles[0], this.transform);
        projectiles.Add(newProjectile);
        newProjectile.SetActive(false);
    }

    public void Fire(Vector3 position, Vector2 facing) 
    {
        GameObject projectile = this.projectiles[this.FindProjectile()];
        projectile.transform.position = position;
        projectile.GetComponent<Projectile>().facing = facing;
        projectile.SetActive(true);
        AudioManager.instance.PlaySoundFromList(fireSFX, 1, Random.Range(-1, 2), false, false);
    }
}
