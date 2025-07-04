using UnityEngine;
using System.Collections.Generic;

public class PlayerAttack : MonoBehaviour
{
    [Header("Melee Attack")]
    public Transform meleeAttackPoint;
    public float meleeAttackRange = 0.5f;
    public int meleeDamage = 40;
    public LayerMask enemyLayers;
    public float meleeAttackCooldown = 0.5f;
    private float meleeCooldownTimer = 0f;

    [Header("Ranged Attack")]
    public Projectile projectilePrefab;
    public Transform firePoint;
    public float minLaunchForce = 5f;
    public float maxLaunchForce = 20f;
    public float maxChargeTime = 2f;
    private float chargeTime = 0f;
    private bool isCharging = false;

    [Header("Enhancements")]
    // This list will hold all active arrow upgrades.
    public List<ArrowEnhancement> activeEnhancements = new List<ArrowEnhancement>();

    void Update()
    {
        // Cooldown timer
        if (meleeCooldownTimer > 0)
        {
            meleeCooldownTimer -= Time.deltaTime;
        }

        // --- Handle Melee Attack ---
        if (Input.GetButtonDown("Fire1") && meleeCooldownTimer <= 0)
        {
            PerformMeleeAttack();
            meleeCooldownTimer = meleeAttackCooldown;
        }

        // --- Handle Ranged Attack ---
        if (Input.GetButtonDown("Fire2"))
        {
            isCharging = true;
            chargeTime = 0f;
        }

        if (isCharging)
        {
            chargeTime += Time.deltaTime;
        }

        if (Input.GetButtonUp("Fire2") && isCharging)
        {
            PerformRangedAttack();
            isCharging = false;
            chargeTime = 0f;
        }
    }

    void PerformMeleeAttack()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(meleeAttackPoint.position, meleeAttackRange, enemyLayers);
        foreach (Collider2D enemy in hitEnemies)
        {
            enemy.GetComponent<Enemy>()?.TakeDamage(meleeDamage);
        }
    }

    void PerformRangedAttack()
    {
        if (projectilePrefab == null) return;

        float launchForce = Mathf.Lerp(minLaunchForce, maxLaunchForce, chargeTime / maxChargeTime);
        int direction = transform.localScale.x > 0 ? 1 : -1;
        Vector2 launchDirection = new Vector2(direction, 0);

        // --- Fire the main projectile ---
        Projectile mainProjectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        mainProjectile.Initialize(activeEnhancements);
        mainProjectile.Launch(launchDirection, launchForce);

        // --- Check for and apply MultiShotEnhancement ---
        MultiShotEnhancement multiShot = null;
        foreach (var enhancement in activeEnhancements)
        {
            if (enhancement is MultiShotEnhancement ms)
            {
                multiShot = ms;
                break; // Found it, no need to keep searching
            }
        }

        if (multiShot != null)
        {
            // Prepare a list of enhancements for the extra projectiles (without multishot)
            List<ArrowEnhancement> childEnhancements = new List<ArrowEnhancement>();
            foreach (var enhancement in activeEnhancements)
            {
                if (!(enhancement is MultiShotEnhancement))
                {
                    childEnhancements.Add(enhancement);
                }
            }

            // Fire additional projectiles
            for (int i = 0; i < multiShot.additionalProjectiles; i++)
            {
                float yOffset = (i % 2 == 0 ? 1 : -1) * ((i / 2) + 1) * multiShot.spread;
                Vector3 spawnPosition = firePoint.position + new Vector3(0, yOffset, 0);

                Projectile extraProjectile = Instantiate(projectilePrefab, spawnPosition, firePoint.rotation);
                extraProjectile.Initialize(childEnhancements);
                extraProjectile.Launch(launchDirection, launchForce);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (meleeAttackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(meleeAttackPoint.position, meleeAttackRange);
    }
}
