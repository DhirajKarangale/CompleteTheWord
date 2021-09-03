using UnityEngine;
using Photon.Pun;

public class ABCCube : MonoBehaviour
{
    [SerializeField] Material material;
    [SerializeField] PhotonView photonView;
    private Vector3 originalScale;

    private void Start()
    {
        originalScale = transform.localScale;
        material.color = Color.white;
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            photonView.RPC("PlayerCollideABCCube", RpcTarget.AllBuffered);
        } 
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            photonView.RPC("PlayerExitABCCube", RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    private void PlayerCollideABCCube()
    {
        material.color = Color.green;
        transform.localScale = new Vector3(1.3f, 0.02f, 1.3f);
    }

    [PunRPC]
    private void PlayerExitABCCube()
    {
        material.color = Color.white;
        transform.localScale = originalScale;
    }
}
