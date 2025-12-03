using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Transform[] checkpoints;

    private void Start()
    {
        int checkpointId = 0;

        if (SaveSystem.HasSave())
            checkpointId = SaveSystem.LoadCheckpoint();

        checkpointId = Mathf.Clamp(checkpointId, 0, checkpoints.Length - 1);
        player.position = checkpoints[checkpointId].position;
    }

    public void ReachCheckpoint(int checkpointId)
    {
        SaveSystem.SaveCheckpoint(checkpointId);
        Debug.Log("Checkpoint reached: " + checkpointId);
    }
}
