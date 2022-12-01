using UnityEngine;

public class Fire : TerrainBase
{
    [SerializeField]
    private float damagePerSecond;

    [SerializeField]
    private float lifeSpan;

    [SerializeField]
    private string fireSFX;

    private float lifeTimer;

    public override void ApplyEffect(GameObject other) 
    {
        Debug.LogFormat("{0} touched fire!", other);

        var enemy = other.GetComponent<Enemy>();
        var player = other.GetComponent<PlayerController>();

        if (enemy != null)
        {
            enemy.DamageEnemy(damagePerSecond);
        }
        if (player != null)
        {
            player.DamagePlayer(damagePerSecond);
        }

    }

    private void OnEnable()
    {
        lifeTimer = Random.Range(lifeSpan -1, lifeSpan + 1);

        AudioManager.instance.PlaySoundFromList(fireSFX);
    }

    private void Update()
    {
        lifeTimer -= Time.deltaTime;

        if(lifeTimer <= 0)
        {
            DisableFire();

            AudioManager.instance.StopLoopingPlayback(fireSFX);
        }
    }

    private void DisableFire()
    {
        this.gameObject.SetActive(false);
    }    
}
