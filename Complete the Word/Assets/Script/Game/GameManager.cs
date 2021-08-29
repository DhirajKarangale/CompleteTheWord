using UnityEngine;
using Photon.Pun;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static bool isGameOver;
    private float gointToLobbyTime;

    [Header("Screen")]
    [SerializeField] GameObject gameOverScreen;
    [SerializeField] GameObject gamePanel;

    [Header("Text")]
    [SerializeField] Text winnerNameText;
    [SerializeField] Text playerNameText;
    [SerializeField] Text goingToLobbyText;


    #region Unity

    private void Start()
    {
        isGameOver = false;
        gointToLobbyTime = 30;
        gameOverScreen.SetActive(false);
        gamePanel.SetActive(true);
    }

    private void Update()
    {
        if (isGameOver) GameOver();
    }

    private void OnDestroy()
    {
       Word.IncreaseWordLevel();
    }

    #endregion Unity

    #region Game

    private void GameOver()
    {
        gamePanel.SetActive(false);
        Invoke("SetGameOverScreenActive", 2);
        winnerNameText.text = Word.winnerName + " Won";

        if (gointToLobbyTime <= 0) LeaveRoom();
        else gointToLobbyTime -= Time.deltaTime;
        goingToLobbyText.text = "Going to Lobby in " + (int)gointToLobbyTime;
    }

    private void SetGameOverScreenActive()
    {
        gameOverScreen.SetActive(true);
    }
    
    public void LeaveRoom()
    {
        Word.IncreaseWordLevel();
        StartCoroutine(LeaveRoomGoMenu());
    }
  
    IEnumerator LeaveRoomGoMenu()
    {
        PhotonNetwork.LeaveRoom();
        while (PhotonNetwork.InRoom)
            yield return null;
        SceneManager.LoadScene(0);
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        if (!isGameOver)
        {
            playerNameText.text = otherPlayer.NickName + " Left the Game";
            if(PhotonNetwork.PlayerList.Length == 1)
            {
                Word.winnerName = PlayerPrefs.GetString("UserName");
                Invoke("GameOver", 1);
            }
        }
    }

    #endregion Game
}
