using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class HomingProjectile : MonoBehaviour
{
    public float speed = 15f;
    public float turnSpeed = 5f;
    public float lifeTime = 8f;
    public float arriveDistance = 1f;

    private Transform target;
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        Destroy(gameObject, lifeTime);

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) target = p.transform;

        rb.linearVelocity = transform.forward * speed;
    }

    private void FixedUpdate()
    {
        if (target == null)
        {
            rb.linearVelocity = transform.forward * speed;
            return;
        }

        Vector3 toTarget = (target.position - transform.position).normalized;
        Vector3 newDir = Vector3.RotateTowards(transform.forward, toTarget, turnSpeed * Time.fixedDeltaTime, 0f);
        transform.rotation = Quaternion.LookRotation(newDir);
        rb.linearVelocity = transform.forward * speed;

        if (Vector3.Distance(transform.position, target.position) <= arriveDistance)
        {
            if (GameManager.Instance != null)
                GameManager.Instance.KillPlayer();
            else
                Debug.LogWarning("No GameManager.Instance found when homing arrived at Player.");

            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (GameManager.Instance != null)
                GameManager.Instance.KillPlayer();
            else
                Debug.LogWarning("No GameManager.Instance found when homing hit Player.");
        }

        Destroy(gameObject);
    }
}
