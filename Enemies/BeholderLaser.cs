using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeholderLaser : MonoBehaviour
{
    [SerializeField]
    private Enemy enemyData;

    [SerializeField]
    private float dOTInterval;

    private void OnEnable()
    {
        AudioManager.instance.PlaySoundFromList("Fire");
    }

    private void OnDisable()
    {
        AudioManager.instance.StopLoopingPlayback("Fire");
    }

    public void OnTriggerStay2D(Collider2D collision)
    {
        var player = collision.GetComponent<PlayerController>();

        if (player != null)
        {
            if(!isDamagingOverTime)
            {
                DamageOverTime(player);
            }
        }
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        var player = collision.GetComponent<PlayerController>();

        if (player != null)
        {
            isDamagingOverTime = false;
            StopAllCoroutines();
        }
    }

    private void DamageOverTime(PlayerController player)
    {
        StartCoroutine(DamagingOverTime(player));
    }
    bool isDamagingOverTime;
    IEnumerator DamagingOverTime(PlayerController player)
    {
        isDamagingOverTime = true;

        float timer = 0;

        player.DamagePlayer(enemyData.attackDamage);

        while (isDamagingOverTime)
        {
            timer += Time.deltaTime;

            if (timer >= dOTInterval)
            {
                player.DamagePlayer(enemyData.attackDamage);
            }

            yield return new WaitForEndOfFrame();
        }
    }
}
