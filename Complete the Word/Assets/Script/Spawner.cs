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
        int spwanPoint = Random.Range(0, spwanPoints.Length);
        PhotonNetwork.Instantiate("PlayerPrefab", spwanPoints[spwanPoint].transform.position, spwanPoints[spwanPoint].transform.rotation);
    }
}
