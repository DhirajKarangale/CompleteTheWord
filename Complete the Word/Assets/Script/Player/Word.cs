using UnityEngine.UI;
using UnityEngine;
using Photon.Pun;
using System.Collections;
using UnityEngine.SceneManagement;

public class Word : MonoBehaviourPunCallbacks ,IPunObservable
{
    [Header("Word")]
   // private static string[] words = { "Bad", "Good", "Nice", "Home", "Help", "Hard", "Easy", "Had", "Mad", "Hat" };
    private static string[] words = {"Game", "Bad", "Good", "Nice", "Home","Had", "Mad","Joke","Neck"};
    private string currWord;
    private bool isCollisionExit;
    private int level;
    private int wordCounter;
    public static string winnerName;

    [Header("UI")]
    [SerializeField] GameObject gamePanel;
    [SerializeField] Text playerNameText;
    [SerializeField] Text[] texts;
    [SerializeField] Image[] wordBG;
    [SerializeField] GameObject wordCanvas;

    #region Unity

    private void Start()
    {
        if (!photonView.IsMine) return;

        // Desable All Text BG
        for (int i = 0; i < wordBG.Length; i++)
        {
            wordBG[i].gameObject.SetActive(false);
        }

        if (PhotonNetwork.IsMasterClient)
        {
            level = Random.Range(0, (words.Length - 1));
            currWord = words[level];
            photonView.RPC("SetWord", RpcTarget.AllBuffered, currWord);
        }
       
        wordCounter = 0;
        wordCanvas.SetActive(true);
        gamePanel.SetActive(true);
        isCollisionExit = true;
    }

    private void Update()
    {
        if (!photonView.IsMine) return;

        if (GameManager.isGameOver) photonView.RPC("GameOverWord", RpcTarget.AllBuffered);

        // Setting Client Word
        if (!PhotonNetwork.IsMasterClient) SetSimilarWordToClient();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (photonView.IsMine && collision.gameObject.CompareTag("ABC") && (wordCounter < currWord.Length) && isCollisionExit && (currWord[wordCounter].ToString().ToUpper() == collision.gameObject.transform.name))
        {
            photonView.RPC("PlayreCollideWord", RpcTarget.AllBuffered);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (!photonView.IsMine) return;
        photonView.RPC("PlayerExitWordCollision", RpcTarget.AllBuffered);
    }

    #endregion Unity


    #region RPCPhoton

    private void SetSimilarWordToClient()
    {
        level = PlayerPrefs.GetInt("Level");
        currWord = words[level];
        photonView.RPC("SetWord", RpcTarget.AllBuffered, currWord);
    }

    [PunRPC]
    private void SetWord(string currentWord)
    {
        // Desable All Text BG
        for (int i = 0; i < wordBG.Length; i++)
        {
            wordBG[i].gameObject.SetActive(false);
        }

        // Enable Required Text BG
        for (int i = 0; i < currentWord.Length; i++)
        {
            wordBG[i].gameObject.SetActive(true);
        }

        // Set Word to Text
        for (int i = 0; i < currentWord.Length; i++)
        {
            texts[i].text = currentWord[i].ToString();
        }
    }

    [PunRPC]
    private void PlayreCollideWord()
    {
        isCollisionExit = false;
        wordBG[wordCounter].color = Color.green;
        wordCounter++;

        if (!photonView.IsMine) return;

        if (wordCounter == currWord.Length)
        {
            photonView.RPC("SetWinnerName", RpcTarget.AllBuffered);
            photonView.RPC("SetGameOverTrue", RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    private void PlayerExitWordCollision()
    {
        isCollisionExit = true;
    }

    [PunRPC]
    private void SetGameOverTrue()
    {
        GameManager.isGameOver = true;
    }

    [PunRPC]
    private void SetWinnerName()
    {
        winnerName = photonView.Owner.NickName;
    }

    [PunRPC]
    private void GameOverWord()
    {
        gamePanel.SetActive(false);
        wordCanvas.SetActive(false);
    }

    // Sending Master Word to Client
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                stream.SendNext(level);
            }
        }
        else
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                level = (int)stream.ReceiveNext();
                PlayerPrefs.SetInt("Level", level);
                PlayerPrefs.Save();
            }
        }
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        if (!GameManager.isGameOver)
        {
            playerNameText.text = otherPlayer.NickName + " Left the Game";
            if (PhotonNetwork.PlayerList.Length == 1)
            {
                winnerName = PhotonNetwork.NickName;
                GameManager.isGameOver = true;
            }
        }
    }


    #endregion RPCPhoton

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
}
