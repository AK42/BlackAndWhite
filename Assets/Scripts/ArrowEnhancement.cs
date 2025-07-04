using UnityEngine;

/// <summary>
/// Base class for all projectile enhancements. Create new enhancements by inheriting from this class.
/// </summary>
public abstract class ArrowEnhancement : ScriptableObject
{
    public abstract void Apply(Projectile projectile);
}
