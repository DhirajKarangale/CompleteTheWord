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
        int spwanPoint = Random.Range(0, spwanPoints.Length);
        PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PhotonNetworkPlayer"), spwanPoints[spwanPoint].transform.position, spwanPoints[spwanPoint].transform.rotation);
    }
}
