using Photon.Pun;
using UnityEngine;
using System.IO;

public class Spawner : MonoBehaviourPunCallbacks
{
    [SerializeField] Transform[] spwanPoints;

    private void Start()
    {
        Invoke("SpwanPlayer", 3);
    }

    private void SpwanPlayer()
    {
        PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PhotonNetworkPlayer"), spwanPoints[Random.Range(0,spwanPoints.Length)].transform.position, spwanPoints[Random.Range(0, spwanPoints.Length)].transform.rotation);
    }
}
