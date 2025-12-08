using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Transform[] checkpoints;
    [SerializeField] private float deathY = -20f;

    private int currentCheckpointId = 0;
    private CharacterController controller;
    private PlayerMovement playerMovement;

    public static GameManager Instance;

    private void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // décommente si tu veux garder entre scènes
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Si player non assigné dans l'inspector, on cherche par tag
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        // Sécurise le controller & playerMovement (si player est trouvé)
        if (player != null)
        {
            controller = player.GetComponent<CharacterController>();
            playerMovement = player.GetComponent<PlayerMovement>();
        }
        else
        {
            Debug.LogWarning("GameManager: Player non assigné et introuvable par tag 'Player'. Assigne le player dans l'inspector !");
        }

        // Si aucun checkpoint assigné, tente de récupérer tous les objets taggés "Checkpoint"
        if (checkpoints == null || checkpoints.Length == 0)
        {
            GameObject[] cpObjects = GameObject.FindGameObjectsWithTag("Checkpoint");
            if (cpObjects != null && cpObjects.Length > 0)
            {
                checkpoints = new Transform[cpObjects.Length];
                for (int i = 0; i < cpObjects.Length; i++)
                    checkpoints[i] = cpObjects[i].transform;

                Debug.Log("GameManager: checkpoints auto-assignés (" + checkpoints.Length + ")");
            }
        }
    }

    private void Start()
    {
        // On récupère le checkpoint depuis la save si dispo
        if (checkpoints == null || checkpoints.Length == 0)
        {
            Debug.LogWarning("GameManager: Aucun checkpoint disponible !");
            return;
        }

        if (SaveSystem.HasSave())
            currentCheckpointId = SaveSystem.LoadCheckpoint();
        else
            currentCheckpointId = 0;

        currentCheckpointId = Mathf.Clamp(currentCheckpointId, 0, checkpoints.Length - 1);
        TeleportPlayerToCheckpoint(currentCheckpointId);
    }

    private void Update()
    {
        if (player == null) return;

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
        if (player == null)
        {
            Debug.LogWarning("TeleportPlayerToCheckpoint: player est null, impossible de respawn.");
            return;
        }

        if (checkpoints == null || checkpoints.Length == 0)
        {
            Debug.LogWarning("TeleportPlayerToCheckpoint: pas de checkpoints assignés.");
            return;
        }

        checkpointId = Mathf.Clamp(checkpointId, 0, checkpoints.Length - 1);

        if (controller != null)
            controller.enabled = false;

        player.position = checkpoints[checkpointId].position;

        if (controller != null)
            controller.enabled = true;

        if (playerMovement != null)
            playerMovement.ResetVelocity();
    }

    public void KillPlayer()
    {
        // Si tu veux plus tard appeler RespawnFromDamage ou autre, centralise ici
        Respawn();
    }
}
