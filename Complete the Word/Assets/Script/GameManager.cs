using UnityEngine;
using Photon.Pun;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static bool isGameOver;
    private float gointToLobbyTime;

    [Header("Screen")]
    [SerializeField] GameObject gameOverScreen;

    [Header("Text")]
    [SerializeField] Text winnerNameText;
    [SerializeField] Text goingToLobbyText;


    #region Unity

    private void Start()
    {
        isGameOver = false;
        gointToLobbyTime = 30;
        gameOverScreen.SetActive(false);
    }

    private void Update()
    {
        if (isGameOver) GameOver();
    }

    #endregion Unity

    #region Game

    private void GameOver()
    {
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
        StartCoroutine(LeaveRoomGoMenu());
    }
  
    IEnumerator LeaveRoomGoMenu()
    {
        PhotonNetwork.LeaveRoom();
        while (PhotonNetwork.InRoom)
            yield return null;
        SceneManager.LoadScene(0);
    }

    #endregion Game
}
