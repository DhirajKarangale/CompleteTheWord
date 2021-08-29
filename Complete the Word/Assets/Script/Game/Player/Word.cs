using UnityEngine.UI;
using UnityEngine;
using Photon.Pun;

public class Word : MonoBehaviour, IPunObservable
{
    [SerializeField] PhotonView photonView;
    [Header("Word")]
    private static string[] words = { "Bad", "Good", "Nice", "Home", "Help", "Hard", "Easy", "Had", "Mad" };
    private string masterWord, clientWord;
    private bool isWordSet, isCollisionExit;
    private static int level;
    private int wordCounter;
    public static string winnerName;


    [Header("UI")]
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
            level = PlayerPrefs.GetInt("Level", 0);
            masterWord = words[level];
            photonView.RPC("SetWord", RpcTarget.AllBuffered, masterWord);
        }

        wordCounter = 0;
        wordCanvas.SetActive(true);
        isWordSet = false;
        isCollisionExit = true;
    }

    private void Update()
    {
        if (!photonView.IsMine) return;

        if (GameManager.isGameOver) photonView.RPC("GameOver", RpcTarget.AllBuffered);

        // Setting Client Word
        if (!PhotonNetwork.IsMasterClient && !isWordSet) SetSimilarWord();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            if (photonView.IsMine && collision.gameObject.CompareTag("ABC") && (wordCounter < clientWord.Length) && isCollisionExit && (clientWord[wordCounter].ToString().ToUpper() == collision.gameObject.transform.name))
            {
                photonView.RPC("PlayreCollideWord", RpcTarget.AllBuffered);
            }
        }
        else
        {
            if (photonView.IsMine && collision.gameObject.CompareTag("ABC") && (wordCounter < masterWord.Length) && isCollisionExit && (masterWord[wordCounter].ToString().ToUpper() == collision.gameObject.transform.name))
            {
                photonView.RPC("PlayreCollideWord", RpcTarget.AllBuffered);
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (!photonView.IsMine) return;
        photonView.RPC("PlayerExitWordCollision", RpcTarget.AllBuffered);
    }

    #endregion Unity

    #region Game

     private void SetSimilarWord()
    {
        clientWord = PlayerPrefs.GetString("ClientWord");
        for (int i = 0; i < words.Length; i++)
        {
            if (clientWord == words[i])
            {
                photonView.RPC("SetWord", RpcTarget.AllBuffered, clientWord);
                isWordSet = true;
                break;
            }
        }
    }

     public static void IncreaseWordLevel()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (level < (words.Length - 1)) level++;
            else level = 0;
            PlayerPrefs.SetInt("Level", level);
            PlayerPrefs.Save();
        }
        PlayerPrefs.SetString("ClientWord", "");
    }

    #endregion Game

    #region RPCPhoton

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

        if (PhotonNetwork.IsMasterClient)
        {
            if (wordCounter == masterWord.Length)
            {
                photonView.RPC("SetWinnerName", RpcTarget.AllBuffered);
                photonView.RPC("SetGameOverTrue", RpcTarget.AllBuffered);
            }
        }
        else
        {
            if (wordCounter == clientWord.Length)
            {
                photonView.RPC("SetWinnerName", RpcTarget.AllBuffered);
                photonView.RPC("SetGameOverTrue", RpcTarget.AllBuffered);
            }
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
    private void GameOver()
    {
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
                masterWord = words[level];
                PlayerPrefs.SetString("ClientWord", masterWord);
                PlayerPrefs.Save();
            }
        }
    }

    #endregion RPCPhoton
}
