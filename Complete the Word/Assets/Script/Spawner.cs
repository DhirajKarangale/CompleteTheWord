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
        string playerToSpwan;

        if (LobbyController.selectedPlayer == 1) playerToSpwan = "PlayerPrefabHammer";
        else if (LobbyController.selectedPlayer == 2) playerToSpwan = "PlayerPrefabBomb";
        else if (LobbyController.selectedPlayer == 3) playerToSpwan = "PlayerPrefab";
        else playerToSpwan = "PlayerPrefabJump";

        PhotonNetwork.Instantiate(playerToSpwan, spwanPoints[PhotonNetwork.LocalPlayer.ActorNumber - 1].transform.position, spwanPoints[PhotonNetwork.LocalPlayer.ActorNumber - 1].transform.rotation);
    }
}
