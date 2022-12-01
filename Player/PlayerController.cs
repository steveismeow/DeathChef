using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public InputManager Input;
    public GameObject playerRoot;
    public Weapon equippedWeapon;
    public WeaponManager weaponManager;
    public Rigidbody2D rb;
    public Animator anim;
    public HeldWeapon heldWeapon;
    public GameplayUI gameplayUI;


    private HealthBar healthBar;

    [SerializeField]
    private string
        dropSFX,
        liftSFX,
        damageSFX;

    private bool canDoStuff;
    private bool cantAim;
    private bool dead;

    [Header("PlayerStats")]
    [SerializeField]
    private float
        movementSpeed = 10,
        damageBufferTime;
    public float
        maxHealth = 50,
        health = 50,
        attackDamageMultiplier,
        movementSpeedMultiplier,
        projectileLifeMultiplier,
        projectileNumber,
        projectileSpeedMultiplier,
        rateOfFireMultiplier;

    public Vector2 facing;

    [SerializeField]
    private GameObject bloodSpray;


    private void OnEnable()
    {
        Input = this.GetComponent<InputManager>();
        rb = this.GetComponent<Rigidbody2D>();
        anim = this.GetComponent<Animator>();
        playerRoot = transform.parent.gameObject;
        weaponManager = playerRoot.GetComponentInChildren<WeaponManager>();
        healthBar = GameObject.FindGameObjectWithTag("HealthBar").GetComponent<HealthBar>();
        heldWeapon = GetComponentInChildren<HeldWeapon>();
        gameplayUI = GameObject.FindGameObjectWithTag("GameplayUI").GetComponent<GameplayUI>();

        PauseManager.instance.player = this;

        UpdatePlayerHealthUI();

        canDoStuff = false;
        cantAim = false;
        canAttack = true;
        dead = false;
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        if (canDoStuff)
        {
            Move();
            Attack();
            CheckPause();
            Aim();
        }


    }

    #region Input
    public bool AllowInput() => canDoStuff = true;
    public void ProhibitInput()
    {
        canDoStuff = false;

    }
    #endregion

    #region Player Actions
    private void Move()
    {
        if (!Input.MovementInput)
        {
            anim.Play("Idle");
            return;
        }

        anim.Play("Walk");
        rb.MovePosition(rb.position + Input.RawMoveInput * (movementSpeed * movementSpeedMultiplier) * Time.fixedDeltaTime);
        //rb.AddForce(Input.RawMoveInput * (movementSpeed * movementSpeedMultiplier) * Time.fixedDeltaTime);
    }

    public void Aim()
    {
        if (cantAim)
        {
            anim.SetFloat("facingX", 1);
            anim.SetFloat("facingY", -1);
        }
        else
        {
            anim.SetFloat("facingX", facing.x);
            anim.SetFloat("facingY", facing.y);
        }
       

        //if (mouse & keyboard)
        //{
        //get vector from player to mouse position
        facing = (Camera.main.ScreenToWorldPoint(Input.RawAimInput) - this.transform.position);

        //normalize to get values between -1 and 1
        facing.Normalize();

        //}
        //else
        //{
        //    //Gamepad Right Stick Conversion
        //}
    }

    private void Attack()
    {

        if (Input.AttackInput && canAttack)
        {
            //ensures only one projectile is generated per click, will likely remove to play more like Binding of Isaac
            //Hold for attack will require a coroutine to pace the rate of fire methinks
            //Input.UseAttackInput();


            StartCoroutine(Attacking());
        }

    }

    public bool attackInterrupted;
    bool canAttack;
    IEnumerator Attacking()
    {
        canAttack = false;
        attackInterrupted = false;

        FireProjectile();

        float rateOfFire = equippedWeapon.rateOfFire * rateOfFireMultiplier;

        float timer = 0;
        Color32 startColor = gameplayUI.equippedWeaponSprite.color;
        Color32 coolDownAlpha = new Color32(140, 140, 140, 255);
        gameplayUI.equippedWeaponSprite.color = coolDownAlpha;

        while (!canAttack)
        {


            timer += Time.deltaTime;
            if (timer > 1 / rateOfFire || attackInterrupted)
            {
                canAttack = true;
                attackInterrupted = false;
                gameplayUI.equippedWeaponSprite.color = startColor;
                break;
            }

            yield return new WaitForEndOfFrame();

        }
    }

    private void FireProjectile()
    {
        equippedWeapon.Fire(this.transform.position, this.facing);
    }

    private void CheckPause()
    {
        if (Input.PauseInput)
        {
            PauseManager.instance.Pause();
        }
    }
    #endregion

    #region Health
    public void DamagePlayer(float damageValue)
    {
        if (!damageCooldown)
        {
            //StartCoroutine(DamageCooldown());

            health -= damageValue;

            if (health <= 0)
            {
                health = 0;
            }

            UpdatePlayerHealthUI();

            BloodSpray();
            AudioManager.instance.PlaySoundFromList(damageSFX);

            Debug.Log("Player" + this.gameObject.name + "health at:" + health);

            if (health == 0)
            {
                LoseState();
            }


        }
        else
        {
            return;
        }
    }

    bool damageCooldown;
    IEnumerator DamageCooldown()
    {
        damageCooldown = true;

        while (damageCooldown)
        {
            float timer = damageBufferTime;

            timer  -= Time.deltaTime;

            if (timer <= 0)
            {
                damageCooldown = false;
                break;
            }

            yield return new WaitForEndOfFrame();
        }
    }

    private void UpdatePlayerHealthUI()
    {
        //var healthBar_Max = healthBar.healthBar_BackgroundSprite;
        var healthBar_Fill = healthBar.healthBar_FillSprite;

        //healthBar_Max.size += new Vector2(maxHealth/10, 0);
        healthBar_Fill.size = new Vector2(Mathf.Clamp(((health/maxHealth)*10)*0.969f, 0, 10), healthBar_Fill.size.y);

    }

    private void ModifyHealthUIMaxHealth(float value)
    {
        var healthBar_Max = healthBar.healthBar_BackgroundSprite;
        var healthBar_Fill = healthBar.healthBar_FillSprite;

        if (healthBar_Max.size.x >= 20)
        {
            //healthBar_Fill.size += new Vector2(healthBar_Max.size.x - 10, 0);
            return;
        }
        else
        {
            healthBar_Max.size += new Vector2(value/10, 0);
            healthBar_Fill.size += new Vector2(value/10, 0);
        }

        health += value;

        if (health > maxHealth)
        {
            health = maxHealth;
        }

    }

    private void BloodSpray()
    {
        GameObject newBloodSpray = Instantiate(bloodSpray, this.transform.position, Quaternion.identity);
    }
    #endregion

    public void LoseState()
    {
        ProhibitInput();

        Debug.Log("Player dead!");
        dead = true;
        PlayerDataManager.instance.hasDied = true;

        WaveManager.instance.ReturnToStartScene();
    }

    #region Animations
    public void DropAnimation()
    {
        anim.Play("Spin");
        cantAim = true;

        StartCoroutine(Dropping());
    }

    bool isDropping;
    IEnumerator Dropping()
    {
        Vector3 startPosition = this.gameObject.transform.position;
        Vector3 dropPosition = startPosition + new Vector3(0, 15, 0);
        this.gameObject.transform.position = dropPosition;

        this.gameObject.GetComponent<BoxCollider2D>().enabled = false;

        this.gameObject.GetComponent<SpriteRenderer>().enabled = true;

        isDropping = true;

        AudioManager.instance.PlaySoundFromList(dropSFX);

        while (isDropping)
        { 
            this.gameObject.transform.Translate(Vector2.down * movementSpeed * Time.deltaTime);

            if (this.gameObject.transform.position.y <= startPosition.y)
            {
                anim.Rebind();
                this.gameObject.GetComponent<BoxCollider2D>().enabled = true;
                isDropping = false;
                cantAim = false;
                break;
            }

            yield return new WaitForEndOfFrame();
        }


    }

    public void LiftAnimation()
    {
        if(dead)
        {
            anim.Play("Death");
        }
        else
        {
            anim.Play("Spin");
        }

        cantAim = true;

        StartCoroutine(Lifting());
    }
    bool isLifting;
    IEnumerator Lifting()
    {
        Vector3 startPosition = this.gameObject.transform.position;
        Vector3 liftPosition = startPosition + new Vector3(0, 15, 0);

        this.gameObject.GetComponent<BoxCollider2D>().enabled = false;

        isLifting = true;

        AudioManager.instance.PlaySoundFromList(liftSFX);

        while (isLifting)
        {
            this.gameObject.transform.Translate(Vector2.up * movementSpeed * Time.deltaTime);

            if (this.gameObject.transform.position.y == liftPosition.y)
            {
                anim.Rebind();
                isLifting = false;
                cantAim = false;
                break;
            }

            yield return new WaitForEndOfFrame();

        }
    }
    #endregion

    public void ModifyStat(BuffPickup.StatToModify statToModify, float value)
    {
        switch (statToModify)
        {
            case BuffPickup.StatToModify.attackDamage:
                attackDamageMultiplier += value;
                break;
            case BuffPickup.StatToModify.health:
                maxHealth += value;
                ModifyHealthUIMaxHealth(value);
                break;
            case BuffPickup.StatToModify.movementSpeed:
                movementSpeedMultiplier += value;
                break;
            case BuffPickup.StatToModify.projectileLife:
                projectileLifeMultiplier += value;
                break;
            case BuffPickup.StatToModify.projectileNumber:
                projectileNumber += value;
                break;
            case BuffPickup.StatToModify.projectileSpeed:
                projectileSpeedMultiplier += value;
                break;
            case BuffPickup.StatToModify.rateOfFire:
                    rateOfFireMultiplier += value;
                    break;
        }
    }

    public void UpdateBuffUI(Sprite buffSprite)
    {
        gameplayUI.CheckBuffTableForBuff(buffSprite);
    }
}
