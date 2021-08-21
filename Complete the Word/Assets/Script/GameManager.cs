using UnityEngine;
using Photon.Pun;
using System.Collections;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviourPunCallbacks
{
    [SerializeField] Transform[] spwanPoints;

    [SerializeField] GameObject gameOverScreen;
    [SerializeField] GameObject gamePanel;

    [SerializeField] Text winnerNameText;
    [SerializeField] Text playerNameText;
    [SerializeField] Text goingToLobbyText;
    private float gointToLobbyTime;

    private void Start()
    {
        gointToLobbyTime = 30;
        gameOverScreen.SetActive(false);
        gamePanel.SetActive(true);

        SpwanPlayer();
    }

    private void Update()
    {
        if (Player.isGameover)
        {
            GameOver();

            if (gointToLobbyTime <= 0) LeaveRoom();
            else gointToLobbyTime -= Time.deltaTime;
            goingToLobbyText.text = "Going to Lobby in " + (int)gointToLobbyTime;
        }
        else
        {
            SetPlayerVsPlayer();
        }
    }

    private void SpwanPlayer()
    {
        int spwanNumber;
        if (PhotonNetwork.IsMasterClient) spwanNumber = 0;
        else spwanNumber = 1;
        PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PhotonNetworkPlayer"), spwanPoints[spwanNumber].transform.position, spwanPoints[spwanNumber].transform.rotation);
    }

    private void GameOver()
    {
        Player.isGameover = true;
        gamePanel.SetActive(false);
        Invoke("SetGameOverScreenActive", 3);
        winnerNameText.text = Player.winnerName + " Won";
    }

    private void SetPlayerVsPlayer()
    {
        string otherPlayerName;
        if (PhotonNetwork.PlayerList.Length > 1)
        {
            if (PhotonNetwork.IsMasterClient) otherPlayerName = PhotonNetwork.PlayerList[1].NickName;
            else otherPlayerName = PhotonNetwork.PlayerList[0].NickName;
            playerNameText.text = PhotonNetwork.NickName + "(You) VS " + otherPlayerName;
        }
    }

    private void SetGameOverScreenActive()
    {
        gameOverScreen.SetActive(true);
    }

    public void LeaveRoom()
    {
        Player.IncreaseWordLevel();
        StartCoroutine(LeaveRoomGoMenu());
    }
  
    IEnumerator LeaveRoomGoMenu()
    {
        PhotonNetwork.LeaveRoom();
        while (PhotonNetwork.InRoom)
            yield return null;
      //  SceneManager.LoadScene(0);
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene(0);

        base.OnLeftRoom();
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        if (Player.isGameover) LeaveRoom();
        else
        {
            playerNameText.text = otherPlayer.NickName + " Left the Game";
            if (photonView.IsMine) Player.winnerName = photonView.Owner.NickName;
            Invoke("GameOver", 2);
        }
        base.OnPlayerLeftRoom(otherPlayer);
    }

  /*  public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);
        playerLeftText.gameObject.SetActive(true);
        playerLeftText.text = "No Internet Going Back To Lobby";
        MenuButton();
    }*/
}
