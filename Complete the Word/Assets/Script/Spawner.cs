using Photon.Pun;
using UnityEngine;

public class Spawner : MonoBehaviourPunCallbacks
{
    [SerializeField] Transform[] spwanPoints;

    private void Start()
    {
        Invoke("SpwanPlayer", 3);
    }

    private void SpwanPlayer()
    {
        PhotonNetwork.Instantiate("PlayerPrefab", spwanPoints[PhotonNetwork.LocalPlayer.ActorNumber - 1].transform.position, spwanPoints[PhotonNetwork.LocalPlayer.ActorNumber - 1].transform.rotation);
    }
}
