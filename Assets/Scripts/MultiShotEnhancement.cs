using UnityEngine;

[CreateAssetMenu(fileName = "New MultiShot Enhancement", menuName = "Attack/Enhancements/MultiShot")]
public class MultiShotEnhancement : ArrowEnhancement
{
    public int additionalProjectiles = 2;
    public float spread = 0.5f;

    // This enhancement is applied at the moment of firing, not on the projectile itself.
    // So, the Apply method will be empty.
    public override void Apply(Projectile projectile)
    {
        // Intentionally left blank. 
        // The logic is handled in PlayerAttack.cs when the projectile is fired.
    }
}
