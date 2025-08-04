using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    public Transform spawnPoint;
    private CharacterController controller;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        // Move player to spawn on start
        if (spawnPoint != null)
        {
            controller.enabled = false;
            transform.position = spawnPoint.position;
            controller.enabled = true;
        }
    }

    public void Respawn()
    {
        controller.enabled = false;
        transform.position = spawnPoint.position;
        controller.enabled = true;
        FindObjectOfType<SpeedrunTimer>().ResetTimer();
    }
}
