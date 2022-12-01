using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldBash : MonoBehaviour
{
    public Projectile projectile;

    public PlayerController player;


    [SerializeField]
    private Weapon weaponData;

    [SerializeField]
    private Rigidbody2D rb;

    [SerializeField]
    private float pushBackStrength;

    [SerializeField]
    private float speedReductionFactor;

    [SerializeField]
    private float maximumChargeTime;

    [SerializeField]
    private float minimumChargeTime;

    [SerializeField]
    private float chargeDistanceMultiplier;

    [SerializeField]
    private float maximumChargeMultiplier;


    [SerializeField]
    private float dOTInterval;

    private float minimumTolerance;

    private Transform playerTransform;

    private float secondsSinceEnabled;

    private Vector3 positionAtCharge;

    private Vector2 targetPos;

    private bool inputHasPressed;

    private bool spent;

    private Vector2 interpolValue;

    private float interpolMagnitude;

    private float numberOfAdds;

    private float errorDiff;

    [SerializeField]
    private CircleCollider2D potCollider;

    [SerializeField]
    private Transform startSpriteTransform;
    [SerializeField]
    private float colliderRadius;


    private void Awake()
    {
        weaponData = this.GetComponentInParent<Weapon>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        projectile = this.GetComponent<Projectile>();
    }

    // Ideally called when the player starts to hold the shoot button
    private void OnEnable()
    {
        ConfigureCollider();

        secondsSinceEnabled = 0f;

        numberOfAdds = 0f;

        //speedReductionFactor = 0.1f;

        minimumTolerance = 0.01f;

        errorDiff = 100.0f;

        pushBackStrength = 10.0f + (weaponData.attackDamage/10 + (player.attackDamageMultiplier - 1));

        //playerTransform = player.transform;

        positionAtCharge = player.gameObject.transform.position;

        inputHasPressed = false;

        spent = false;

        isPreparingDash = false;

        isDashing = false;
    }

    void ConfigureCollider()
    {
        float xOffset = startSpriteTransform.localPosition.x;

        xOffset = xOffset - (player.attackDamageMultiplier - 1);

        if (xOffset <= 0)
        {
            xOffset = 1;
        }
        Vector3 position = new Vector3(xOffset, startSpriteTransform.localPosition.y, startSpriteTransform.localPosition.z);
        startSpriteTransform.localPosition = position;
        colliderRadius = colliderRadius + (player.attackDamageMultiplier - 1);
        potCollider.radius = colliderRadius;
        potCollider.enabled = false;

        rb.mass = 50;

    }

    // Update is called once per frame
    void FixedUpdate()
    {

        if (player.Input.AttackInput && !spent)
        {
            if (!isPreparingDash && !isDashing)
            {
                isPreparingDash = true;
                StartCoroutine(PreparingDash());
            }

        }

    }

    bool isPreparingDash;
    IEnumerator PreparingDash()
    {        
        player.heldWeapon.GetComponent<SpriteRenderer>().enabled = false;
        
        inputHasPressed = true;

        player.ProhibitInput();

        float holdTimeMultiplier = 0;

        while (isPreparingDash)
        {
            player.Aim();

            secondsSinceEnabled += Time.fixedDeltaTime;

            if (secondsSinceEnabled >= (maximumChargeTime - (weaponData.rateOfFire/10 + player.rateOfFireMultiplier/10)))                                                                                    // Limit the maximum amount of charge time
            {
                print((maximumChargeTime - (weaponData.rateOfFire / 10 + player.rateOfFireMultiplier / 10)));

                secondsSinceEnabled = maximumChargeTime;
                holdTimeMultiplier = maximumChargeMultiplier;
                
                isPreparingDash = false;
                break;
            }
            else if (!player.Input.AttackInput)
            {
                if (secondsSinceEnabled <= minimumChargeTime)
                {
                    print("Dash cancelled");
                    player.attackInterrupted = true;
                    rb.velocity = Vector2.zero;
                    rb.mass = 500;
                    player.AllowInput();
                    spent = true;
                    inputHasPressed = false;
                    numberOfAdds = 0;
                    secondsSinceEnabled = 0;
                    this.gameObject.GetComponent<Projectile>().DestroyProjectile();
                    player.heldWeapon.GetComponent<SpriteRenderer>().enabled = true;
                    player.rb.drag = 10;
                    break;
                }
                holdTimeMultiplier = maximumChargeMultiplier * (secondsSinceEnabled/maximumChargeTime);

                isPreparingDash = false;
            }

            yield return new WaitForFixedUpdate();
        }

        if (!spent)
        {
            Dash(holdTimeMultiplier);
        }
    }

    private void Dash(float holdTimeMultiplier)
    {
        StartCoroutine(Dashing(holdTimeMultiplier));
    }

    bool isDashing;
    IEnumerator Dashing(float holdTimeMultiplier)
    {
        isDashing = true;

        potCollider.enabled = true;                                                                                         // Enable collider while moving

        targetPos = player.facing * chargeDistanceMultiplier * holdTimeMultiplier; //get the dash direction

        Vector3 targetPosThree = new Vector3(targetPos.x, targetPos.y, 0.0f);                                               // Create target position as a Vector3

        player.rb.drag = 0;

        while (isDashing)
        {
            //Vector3 interpolMagnitudeThree = Vector3.MoveTowards(positionAtCharge, targetPos, numberOfAdds * (weaponData.speed * playerInput.projectileSpeedMultiplier)); // Create vector that will move pot lid object

            //this.transform.position = Vector3.MoveTowards(positionAtCharge, targetPos, numberOfAdds * (weaponData.speed * playerInput.projectileSpeedMultiplier));  
            // Moves pot lid object

            Vector2 forceToAdd = targetPos * (weaponData.speed * (maximumChargeMultiplier * (secondsSinceEnabled / maximumChargeTime)) * player.projectileSpeedMultiplier) * Time.fixedDeltaTime;

            print(forceToAdd);

            if (forceToAdd.magnitude < 2000)
            {
                forceToAdd *= (maximumChargeMultiplier * (secondsSinceEnabled / maximumChargeTime)) * 5000;
            }

            rb.AddForce(forceToAdd);//, ForceMode2D.Impulse);

            player.gameObject.transform.position = this.transform.position;                                                                 // Moves player to match pot lit's position

            numberOfAdds += Time.fixedDeltaTime;

            bool playerHasBeenStopped = false;
            print(rb.velocity.magnitude);
            print(numberOfAdds);
            if (numberOfAdds >= 0.5f && rb.velocity.magnitude <= 0.1f)
            {
                print("Projectile stopped!");
                playerHasBeenStopped = true;
            }

            errorDiff = (targetPosThree - player.gameObject.transform.position).magnitude;

            //float distanceFromPlayer = Vector2.Distance(this.transform.position, player.gameObject.transform.position);

            if (((numberOfAdds >= secondsSinceEnabled) && inputHasPressed) || (errorDiff <= minimumTolerance) || playerHasBeenStopped)                      // If we've spent enough time moving or we've reached our destination
            {
                print("Exited Dash Successfully");

                projectile.StartCoroutine(projectile.TickLife());
                rb.velocity = Vector2.zero;
                rb.mass = 500;
                player.AllowInput();
                spent = true;
                inputHasPressed = false;
                numberOfAdds = 0;
                secondsSinceEnabled = 0;
                player.rb.drag = 10;

                isDashing = false;
                break;
            }

            //if (((numberOfAdds >= secondsSinceEnabled) && inputHasPressed) || (errorDiff <= minimumTolerance))                      // If we've spent enough time moving or we've reached our destination
            //{
            //    playerInput.AllowInput();
            //    spent = true;
            //    inputHasPressed = false;
            //    numberOfAdds = 0;
            //    secondsSinceEnabled = 0;
            //    //this.gameObject.GetComponent<Projectile>().DestroyProjectile();

            //    isDashing = false;
            //    break;
            //}


            yield return new WaitForFixedUpdate();

        }

        player.AllowInput();
        player.heldWeapon.GetComponent<SpriteRenderer>().enabled = true;

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        var enemy = collision.gameObject.GetComponent<Enemy>();

        if (enemy != null)
        {

            enemy.DamageEnemy(weaponData.attackDamage * player.attackDamageMultiplier);

            float pushBackTime = 1.0f;

            Vector3 pushBackAmount = collision.contacts[0].normal * pushBackStrength;


            StartCoroutine(PushTo(pushBackAmount, pushBackTime, enemy));

            //Debug.Log("          Shieldbash.cs          Collision occured");

            // Print how many points are colliding with this transform
            //Debug.Log("          Shieldbash.cs          Points colliding: " + collision.contacts.Length);

            // Print the normal of the first point in the collision.
            //Debug.Log("          Shieldbash.cs          Normal of the first point: " + collision.contacts[0].normal);


            //Debug.Log("          Shieldbash.cs          pushBackAmount: " + pushBackAmount);

            //enemy.transform.position += pushBackAmount;


            //enemy.DamageEnemy(weaponData.attackDamage);
            //AudioManager.instance.PlaySoundFromList(hitSFX);

            //if (destroyOnHitEnemy)
            //{
            //    DestroyProjectile();
            //}
        }
    }

    bool isPushing;
    IEnumerator PushTo(Vector3 endPosition, float totalTime, Enemy enemy)
    {
        Vector3 start = enemy.transform.position;
        Vector3 end = endPosition;
        Vector3 pushDistance;
        float currentTime = 0.0f;

        isPushing = true;

        print("Pushing");

        while(isPushing)
        {
            currentTime += Time.fixedDeltaTime / totalTime;

            pushDistance = Vector3.MoveTowards(start, end, currentTime);

            //enemy.transform.Translate(pushDistance);

            //enemy.transform.position = pushDistance;

            enemy.rb.AddForce(pushDistance * 10);

            if (currentTime >= totalTime)
            {
                isPushing = false;
                break;
            }
            yield return new WaitForEndOfFrame();
        }
    }

    public void OnCollisionStay(UnityEngine.Collision collision)
    {
        var enemy = collision.gameObject.GetComponent<Enemy>();

        if (enemy != null)
        {
            if (!isDamagingOverTime)
            {
                DamageOverTime(enemy);
            }
        }
    }

    public void OnCollisionExit(UnityEngine.Collision collision)
    {
        var enemy = collision.gameObject.GetComponent<Enemy>();

        if (enemy != null)
        {
            print("PotLidProjectile Stopping All Coroutines now that Enemy has left collision");
            isDamagingOverTime = false;
            StopAllCoroutines();
        }
    }

    private void DamageOverTime(Enemy enemy)
    {
        StartCoroutine(DamagingOverTime(enemy));
    }
    bool isDamagingOverTime;
    IEnumerator DamagingOverTime(Enemy enemy)
    {
        isDamagingOverTime = true;

        float timer = 0;

        print("Damaging over time");

        while (isDamagingOverTime)
        {
            timer += Time.deltaTime;

            if (timer >= dOTInterval)
            {
                enemy.DamageEnemy(weaponData.attackDamage * player.attackDamageMultiplier);
            }

            yield return new WaitForEndOfFrame();
        }
    }



}
