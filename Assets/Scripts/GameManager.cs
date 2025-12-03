using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Transform[] checkpoints;
    [SerializeField] private float deathY = -20f;

    private int currentCheckpointId = 0;
    private CharacterController controller;
    private PlayerMovement playerMovement;

    private void Awake()
    {
        controller = player.GetComponent<CharacterController>();
        playerMovement = player.GetComponent<PlayerMovement>();
    }

    private void Start()
    {
        // On récupère le checkpoint depuis la save si dispo
        if (SaveSystem.HasSave())
            currentCheckpointId = SaveSystem.LoadCheckpoint();
        else
            currentCheckpointId = 0;

        currentCheckpointId = Mathf.Clamp(currentCheckpointId, 0, checkpoints.Length - 1);
        TeleportPlayerToCheckpoint(currentCheckpointId);
    }

    private void Update()
    {
        // Zone de mort globale : si on tombe trop bas
        if (player.position.y < deathY)
        {
            Respawn();
        }
    }

    public void ReachCheckpoint(int checkpointId)
    {
        currentCheckpointId = checkpointId;
        SaveSystem.SaveCheckpoint(checkpointId);
        Debug.Log("Checkpoint reached: " + checkpointId);
    }

    public void Respawn()
    {
        Debug.Log("Respawn at checkpoint: " + currentCheckpointId);
        TeleportPlayerToCheckpoint(currentCheckpointId);
    }

    private void TeleportPlayerToCheckpoint(int checkpointId)
    {
        checkpointId = Mathf.Clamp(checkpointId, 0, checkpoints.Length - 1);

        // On désactive le CharacterController pour éviter les bugs
        if (controller != null)
            controller.enabled = false;

        player.position = checkpoints[checkpointId].position;

        if (controller != null)
            controller.enabled = true;

        // On reset la vitesse du joueur si on a accès au PlayerMovement
        if (playerMovement != null)
            playerMovement.ResetVelocity();
    }
}
