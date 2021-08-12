using UnityEngine;
using Photon.Pun;
using System.IO;

public class GameManager : MonoBehaviour
{
    [SerializeField] Transform[] spwanPoints;
    [SerializeField] PhotonView photonView;

    private void Start()
    {
      SpwanPlayer();
    }
       
    private void SpwanPlayer()
    {
        PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PhotonNetworkPlayer"), spwanPoints[PhotonNetwork.PlayerList.Length-1].transform.position, spwanPoints[PhotonNetwork.PlayerList.Length-1].transform.rotation);
    }
}
