using UnityEngine;
using Photon.Pun;

public class ABCCube : MonoBehaviour
{
    [SerializeField] Material material;
    [SerializeField] PhotonView photonView;

    private void Start()
    {
        material.color = Color.white;
    }

    private void OnTriggerEnter(Collider other)
    {
        photonView.RPC("PlayerCollideABCCube", RpcTarget.AllBuffered);
    }

    private void OnTriggerExit(Collider other)
    {
        photonView.RPC("PlayerExitABCCube", RpcTarget.AllBuffered);
    }

    [PunRPC]
    private void PlayerCollideABCCube()
    {
        material.color = Color.green;
    }

    [PunRPC]
    private void PlayerExitABCCube()
    {
        material.color = Color.white;
    }
}
