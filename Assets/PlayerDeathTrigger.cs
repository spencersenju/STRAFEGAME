using UnityEngine;

public class PlayerDeathTrigger : MonoBehaviour
{
    private PlayerRespawn respawnScript;

    void Start()
    {
        respawnScript = GetComponent<PlayerRespawn>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("KillZone"))
        {
            Debug.Log("Entered KillZone — Respawning");
            respawnScript.Respawn();
        }
    }
}
