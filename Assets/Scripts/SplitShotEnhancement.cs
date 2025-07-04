using UnityEngine;

[CreateAssetMenu(fileName = "New Split Shot Enhancement", menuName = "Attack/Enhancements/Split Shot")]
public class SplitShotEnhancement : ArrowEnhancement
{
    public float splitTime = 0.5f;

    public override void Apply(Projectile projectile)
    {
        projectile.EnableSplitting(splitTime);
    }
}
