using UnityEngine;

[CreateAssetMenu(fileName = "New Projectile Data", menuName = "Attack/Projectile Data")]
public class ProjectileData : ScriptableObject
{
    [Header("Stats")]
    public int damage = 1;
    public float lifeTime = 2f;

    [Header("Splitting")]
    public bool canSplit = false; // Disabled by default
    public float splitTime = 0.5f;
}
