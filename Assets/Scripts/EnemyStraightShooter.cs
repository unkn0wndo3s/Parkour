using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class EnemyStraightShooter : MonoBehaviour
{
    [Header("Refs")]
    public GameObject projectilePrefab;   // prefab de la balle
    public Transform shootPoint;          // facultatif : si null on spawn en avant de l'ennemi

    [Header("Tuning")]
    public float spawnOffset = 0.5f;      // fallback si shootPoint == null
    public float projectileSpeed = 20f;   // si prefab a Rigidbody on set la vitesse
    public float restAfterAnim = 1f;      // 1 seconde de repos après la fin de l'anim

    [Header("Loop")]
    public bool autoFire = true;          // true = l'ennemi tire en boucle
    public float startDelay = 0.5f;       // délai avant de commencer à tirer (utile pour plusieurs ennemis)

    private Animator animator;
    private float clipLength = 1f;        // durée du clip "Frisbee Throw" (détectée automatiquement si possible)
    private int shootTriggerHash;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        shootTriggerHash = Animator.StringToHash("Shoot");

        // tente de récupérer la durée du clip nommé exactamente "Frisbee Throw"
        var rt = animator.runtimeAnimatorController;
        if (rt != null)
        {
            foreach (var clip in rt.animationClips)
            {
                if (clip != null && clip.name == "Frisbee Throw")
                {
                    clipLength = clip.length;
                    break;
                }
            }
        }

        // fallback : si pas trouvé, clipLength reste 1s (tu peux ajuster dans l'inspector)
    }

    private void Start()
    {
        if (autoFire)
            StartCoroutine(FireLoop());
    }

    // boucle qui déclenche l'animation, attend sa durée + rest, relance
    private IEnumerator FireLoop()
    {
        yield return new WaitForSeconds(startDelay);

        while (autoFire)
        {
            // déclenche l'animation
            animator.SetTrigger(shootTriggerHash);

            // on attend la fin du clip (l'Animation Event appellera SpawnProjectile à mi-anim)
            yield return new WaitForSeconds(clipLength);

            // temps de repos supplémentaire
            yield return new WaitForSeconds(restAfterAnim);
        }
    }

    // méthode appelée par l'Animation Event placé à la moitié du clip
    public void SpawnProjectile()
    {
        
        if (projectilePrefab == null)
        {
            Debug.LogWarning($"EnemyStraightShooter ({name}): projectilePrefab non assigné.");
            return;
        }
        // Si on a un shootPoint, on le fait regarder le joueur
        if (shootPoint != null)
        {
            Vector3 dirToPlayer = (GetPlayerPosition() - shootPoint.position).normalized;
            if (dirToPlayer != Vector3.zero)
                shootPoint.rotation = Quaternion.LookRotation(dirToPlayer);
        }


        Vector3 spawnPos;
        Quaternion spawnRot = (shootPoint != null) ? shootPoint.rotation : Quaternion.LookRotation(GetPlayerDirection());

        if (shootPoint != null)
        {
            spawnPos = shootPoint.position;
        }
        else
        {
            // spawn devant l'ennemi (dans la direction forward)
            spawnPos = transform.position + transform.forward * spawnOffset;
        }

        GameObject proj = Instantiate(projectilePrefab, spawnPos, spawnRot);

        // ignore collisions entre projectile et colliders de l'ennemi (prévention self-hit)
        Collider projCol = proj.GetComponent<Collider>();
        if (projCol != null)
        {
            Collider[] enemyCols = GetComponentsInChildren<Collider>();
            foreach (var col in enemyCols)
            {
                if (col != null)
                    Physics.IgnoreCollision(projCol, col);
            }
        }

        // assigner une vélocité initiale si Rigidbody présent
        Rigidbody rb = proj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = proj.transform.forward * projectileSpeed;
        }
    }

    private Vector3 GetPlayerPosition()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        return player != null ? player.transform.position : transform.position + transform.forward * 5f;
    }


    // helper fallback : direction vers le player si on a un tag Player
    private Vector3 GetPlayerDirection()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) return (p.transform.position - transform.position).normalized;
        return transform.forward;
    }

    // API publique pour lancer un seul tir (si tu veux contrôler par script)
    public void ShootOnce()
    {
        animator.SetTrigger(shootTriggerHash);
    }

    // stoppe le tir en boucle
    public void StopAutoFire()
    {
        autoFire = false;
        StopAllCoroutines();
    }
}
