using UnityEngine;

public class EnhancementPickup : MonoBehaviour
{
    [Header("Enhancement")]
    public ArrowEnhancement enhancementToGrant;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the object that entered the trigger is the player
        if (other.CompareTag("Player"))
        {
            // Get the PlayerAttack component from the player
            PlayerAttack playerAttack = other.GetComponent<PlayerAttack>();

            if (playerAttack != null && enhancementToGrant != null)
            {
                // Add the enhancement to the player's list if it's not already there
                if (!playerAttack.activeEnhancements.Contains(enhancementToGrant))
                {
                    playerAttack.activeEnhancements.Add(enhancementToGrant);
                    Debug.Log("Granted enhancement: " + enhancementToGrant.name);
                }

                // Destroy the pickup item
                Destroy(gameObject);
            }
        }
    }
}
