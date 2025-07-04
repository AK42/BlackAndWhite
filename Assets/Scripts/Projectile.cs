using UnityEngine;
using System.Collections.Generic;

public class Projectile : MonoBehaviour
{
    [Header("Base Stats")]
    public int damage = 1;
    public float lifeTime = 3f;

    private Rigidbody2D rb;
    private List<ArrowEnhancement> appliedEnhancements = new List<ArrowEnhancement>();

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    public void Initialize(List<ArrowEnhancement> enhancements)
    {
        this.appliedEnhancements = new List<ArrowEnhancement>(enhancements); // Make a copy
        foreach (var enhancement in appliedEnhancements)
        {
            enhancement.Apply(this);
        }
    }

    public void Launch(Vector2 direction, float force)
    {
        rb.linearVelocity = direction * force;
        if (direction.x < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
    }

    public void EnableSplitting(float splitTime)
    {
        Invoke("Split", splitTime);
    }

    private void Split()
    {
        Vector2 currentDirection = rb.linearVelocity.normalized;
        float currentSpeed = rb.linearVelocity.magnitude;

        // Create two new projectiles
        CreateSplitProjectile(currentDirection, currentSpeed);
        CreateSplitProjectile(-currentDirection, currentSpeed);

        Destroy(gameObject);
    }

    private void CreateSplitProjectile(Vector2 direction, float speed)
    {
        Projectile newProjectile = Instantiate(this, transform.position, Quaternion.identity);
        
        // Prevent the new projectiles from splitting again
        List<ArrowEnhancement> childEnhancements = new List<ArrowEnhancement>();
        foreach (var enhancement in appliedEnhancements)
        {
            if (!(enhancement is SplitShotEnhancement))
            {
                childEnhancements.Add(enhancement);
            }
        }
        newProjectile.Initialize(childEnhancements);
        newProjectile.Launch(direction, speed);
    }

    void OnTriggerEnter2D(Collider2D hitInfo)
    { 
        Enemy enemy = hitInfo.GetComponent<Enemy>();
        if (enemy != null)
        { 
            enemy.TakeDamage(damage);
        }

        if (!hitInfo.CompareTag("Player"))
        { 
            Destroy(gameObject);
        }
    }
}
