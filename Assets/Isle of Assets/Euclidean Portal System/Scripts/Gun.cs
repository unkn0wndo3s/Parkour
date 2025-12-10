using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace PortalSystem
{
    [HelpURL("https://assetstore.unity.com/packages/slug/225954")]
    public class Gun : MonoBehaviour
    {
        [SerializeField]
        private AudioClip shot, misfire;

        [SerializeField]
        private GameObject Hole;

        [SerializeField]
        private Texture2D[] holeSprites;

        [SerializeField]
        private Transform cam, camA, camB, placeA, placeB;

        [SerializeField]
        private Image gunCol;

        [SerializeField]
        private Text count;

        [SerializeField]
        private float dispersion = 0.01f, holeSize = 0.05f;

        [SerializeField]
        private int bullets;

        private GameObject[] holes = new GameObject[20];
        private AudioSource audioSource;
        private Coroutine coroutine;
        private RaycastHit hit;
        private int magazine, holeIndex;
        private bool isReloading;

        /// <summary>
        /// Reloading
        /// </summary>
        public void Reload()
        {
            if (isReloading || magazine == 30 || bullets == 0)
                return;
            isReloading = true;
            if (coroutine != null)
                StopCoroutine(coroutine);
            coroutine = StartCoroutine(ReloadWait());
        }

        /// <summary>
        /// Shooting
        /// </summary>
        public void Shoot()
        {
            if (isReloading)
                return;
            if (coroutine != null)
                StopCoroutine(coroutine);
            coroutine = StartCoroutine(ShootWait());
        }

        /// <summary>
        /// Stop shooting
        /// </summary>
        public void StopShoot()
        {
            if (!isReloading && coroutine != null)
                StopCoroutine(coroutine);
        }

        /// <summary>
        /// Sound controller reloading and calculation of remaining cartridges
        /// </summary>
        /// <returns></returns>
        private IEnumerator ReloadWait()
        {
            GetComponent<Animator>().Play("Reload", -1, 0);
            yield return new WaitForSeconds(0.3f);
            audioSource.pitch = 1f;
            audioSource.Play();
            yield return new WaitForSeconds(0.7f);
            audioSource.pitch = 2f;
            audioSource.Play();
            yield return new WaitForSeconds(0.5f);
            audioSource.pitch = 1f;
            isReloading = false;
            int saveMagazineCount = magazine;
            magazine += Mathf.Clamp(bullets, 0, 30 - saveMagazineCount);
            bullets -= magazine - saveMagazineCount;
            SetCountText();
        }

        /// <summary>
        /// Incessant shooting
        /// </summary>
        /// <returns></returns>
        private IEnumerator ShootWait()
        {
            while (isReloading)
                yield return null;
            while (magazine > 0)
            {
                Vector3 misalignment = new Vector3(Random.Range(-dispersion, dispersion), Random.Range(-dispersion, dispersion), 0);
                bool isShotInPortal = Physics.Raycast(cam.position, cam.forward + cam.TransformDirection(misalignment), out hit, Mathf.Infinity, LayerMask.GetMask("Water"));
                Transform cameraShot = isShotInPortal ? (PortalTeleporter.isSecondWorld ? camA : camB) : cam;
                if (Physics.Raycast(isShotInPortal ? (hit.point + (PortalTeleporter.isSecondWorld ? (placeA.position - placeB.position) : (placeB.position - placeA.position))) : cameraShot.position, cameraShot.forward + cameraShot.TransformDirection(misalignment), out hit, Mathf.Infinity, LayerMask.GetMask("Default")))
                {
                    DrawHole(false);
                    if (hit.collider.name == "Border")
                        DrawHole(true);
                }
                audioSource.PlayOneShot(shot);
                magazine--;
                if (bullets == 0 && magazine == 0)
                {
                    count.color = new Color(1f, 0f, 0f, 0.5f);
                    gunCol.color = count.color;
                }
                SetCountText();
                yield return new WaitForSeconds(0.12f);
            }
            audioSource.PlayOneShot(misfire);
        }

        /// <summary>
        /// Creating a shot hole
        /// </summary>
        private void DrawHole(bool isBorder)
        {
            Renderer holeRenderer = Instantiate(Hole, hit.point - cam.forward * 0.01f - (isBorder ? (PortalTeleporter.isSecondWorld ? (placeB.position - placeA.position) : (placeA.position - placeB.position)) : Vector3.zero), Quaternion.FromToRotation(Vector3.back, hit.normal)).GetComponent<Renderer>();
            holeRenderer.transform.localScale = new Vector3(holeSize, holeSize, 1f);
            holeRenderer.material.mainTexture = holeSprites[holeIndex % 4];
            Destroy(holeRenderer.gameObject, 20f);
            if (holes[holes.Length - 1] != null)
                Destroy(holes[holes.Length - 1]);
            for (int i = holes.Length - 1; i > 0; i--)
                holes[i] = holes[i - 1];
            holes[0] = holeRenderer.gameObject;
            holeIndex++;
        }

        /// <summary>
        /// Displaying information on the UI
        /// </summary>
        private void SetCountText()
        {
            count.text = magazine + "/<size='220'>" + bullets + "</size>";
        }

        /// <summary>
        /// Component caching and reloading
        /// </summary>
        private void Start()
        {
            audioSource = GetComponent<AudioSource>();
            Reload();
            SetCountText();
        }
    }
}