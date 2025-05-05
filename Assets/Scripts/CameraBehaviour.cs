using UnityEngine;
using System.Collections;

public class CameraBehaviour : MonoBehaviour
{

    private Vector2 velocity;
    //public float smoothTimeY;
    //public float smoothTimeX;

    public GameObject player;

    public bool bounds;

    public Vector3 minCameraPos;
    public Vector3 maxCameraPos;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    void LateUpdate()
    {
        // Smoothly follow the player
        // Currently disabled because of sprite blinking

        // Follow the player
        float posX = player.transform.position.x;
        float posY = player.transform.position.y;

        transform.position = new Vector3(posX, posY, transform.position.z);

        // Apply camera bounds if enabled
        if (bounds)
        {
            transform.position = new Vector3(
                Mathf.Clamp(transform.position.x, minCameraPos.x, maxCameraPos.x),
                Mathf.Clamp(transform.position.y, minCameraPos.y, maxCameraPos.y),
                Mathf.Clamp(transform.position.z, minCameraPos.z, maxCameraPos.z)
            );
        }
    }
}
