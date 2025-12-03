using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public int checkpointId = 0;
    public GameManager gameManager;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        gameManager.ReachCheckpoint(checkpointId);
    }
}
