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
        PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PhotonNetworkPlayer"), spwanPoints[PhotonNetwork.PlayerList.Length-1].transform.position, spwanPoints[PhotonNetwork.PlayerList.Length-1].transform.rotation);
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
