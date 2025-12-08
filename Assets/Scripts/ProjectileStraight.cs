using UnityEngine;

public class ProjectileStraight : MonoBehaviour
{
    public float speed = 20f;
    public float lifeTime = 5f;
    public bool useRigidbodyVelocity = true;

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        if (!useRigidbodyVelocity)
        {
            transform.position += transform.forward * speed * Time.deltaTime;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (GameManager.Instance != null)
                GameManager.Instance.KillPlayer();
            else
                Debug.LogWarning("No GameManager.Instance found when projectile hit Player.");
        }

        Destroy(gameObject);
    }
}
