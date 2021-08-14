using UnityEngine;
using Photon.Pun;
using System.IO;
using System.Collections;
using UnityEngine.SceneManagement;

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
        int spwanNumber;
        if (PhotonNetwork.IsMasterClient) spwanNumber = 0;
        else spwanNumber = 1;
        PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PhotonNetworkPlayer"), spwanPoints[spwanNumber].transform.position, spwanPoints[spwanNumber].transform.rotation);
    }

    public void MenuButton()
    {
        StartCoroutine(LeaveRoomGoMenu());
    }

    IEnumerator LeaveRoomGoMenu()
    {
        PhotonNetwork.LeaveRoom();
        while (PhotonNetwork.InRoom)
            yield return null;
        SceneManager.LoadScene(0);
    }
}
