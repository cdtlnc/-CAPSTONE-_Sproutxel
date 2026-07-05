using System.Collections;
using UnityEngine;
using Photon.Pun;

public class GhostCanonView : MonoBehaviourPun
{
    [SerializeField] private Animator canonAnim;

    [Header("Effects")]
    [SerializeField] private GameObject canonball;
    [SerializeField] private Transform canonballSpawnPoint;

    [SerializeField] private GameObject vfx;
    [SerializeField] private Transform vfxSpawnPoint;

    [PunRPC]
    public void RPC_PlayGhostCanon()
    {
        if (canonAnim != null)
            canonAnim.SetTrigger("_IsFiring");

        AudioManager.instance.Play("CanonSFX1");

        StartCoroutine(SpawnGhostEffects());
    }

    private IEnumerator SpawnGhostEffects()
    {
        yield return new WaitForSeconds(0.18f);

        CommitAnimations();
    }

    public void CommitAnimations()
    {
        if (canonball != null && canonballSpawnPoint != null)
        {
            GameObject ball = Instantiate(
                canonball,
                canonballSpawnPoint.position,
                canonballSpawnPoint.rotation);

            SetLayerRecursively(ball, LayerMask.NameToLayer("POODLE - 4 PHOTON - 0"));

            Rigidbody rb = ball.GetComponent<Rigidbody>();

            if (rb != null)
                rb.linearVelocity = canonballSpawnPoint.forward * 10000f;
        }

        if (vfx != null && vfxSpawnPoint != null)
        {
            GameObject smoke =
                Instantiate(vfx,
                vfxSpawnPoint.position,
                Quaternion.identity);

            SetLayerRecursively(smoke, LayerMask.NameToLayer("POODLE - 4 PHOTON - 0"));
        }
    }

    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;

        foreach (Transform child in obj.transform)
            SetLayerRecursively(child.gameObject, layer);
    }
}