using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using EasyJoystick;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviourPunCallbacks, IInRoomCallbacks,IPunObservable
{
    [Header("Movement")]
  //  [SerializeField] PhotonView photonView;
    [SerializeField] Rigidbody rigidBody;
    [SerializeField] Transform body;
    [SerializeField] float speed;
    private Joystick joystick;

    [Header("Camera Follow")]
    private Transform cam;
    private Vector3 offset;
    private float smoothSpeed = 0.125f;

    [Header("Word")]
    [SerializeField] Text[] texts;
    [SerializeField] Image[] wordBG;
    private string currWord, clientWord;
    private string[] words = { "Bad", "Good", "Nice", "Home", "Help", "Hard", "Easy", "Had", "Mad" };
    private int wordCounter = 0;
    public static bool isGameover;
    private bool isCollisionExit, isWordSet;
    private int level;

    [Header("UI")]
    [SerializeField] GameObject gameOverScreen;
    [SerializeField] Text winnerName;
    [SerializeField] Text playerLeftText;
    [SerializeField] Text goingToLobbyText;
    private float gointToLobbyTime;

    private void Awake()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            level = PlayerPrefs.GetInt("Level", 0);
            currWord = words[level];
            photonView.RPC("SetWord", RpcTarget.AllBuffered, currWord);
            offset = new Vector3(8, 5, 0);
        }
        else
        {
            offset = new Vector3(-8, 5, 0);
        }
    }

    private void Start()
    {
        gointToLobbyTime = 30;

        if (!photonView.IsMine) return;
      
        cam = FindObjectOfType<Camera>().transform;
        joystick = FindObjectOfType<Joystick>();

        joystick.gameObject.SetActive(true);
        gameOverScreen.SetActive(false);
        isGameover = false;
        isCollisionExit = true;
        isWordSet = false;
    }

    private void Update()
    {

        if (isGameover)
        {
            if (gointToLobbyTime <= 0) MenuButton();
            else gointToLobbyTime -= Time.deltaTime;
            goingToLobbyText.text = "Going to Lobby in " + (int)gointToLobbyTime;
        }

        if (!photonView.IsMine) return;

        // Input
        float horrizantal = joystick.Horizontal();
        float vertical = joystick.Vertical();

        // Move
        Vector3 direction = transform.right * horrizantal + transform.forward * vertical;
        rigidBody.velocity = direction * speed;

        // Look
        if (direction.magnitude >= 0.1)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            body.rotation = Quaternion.Euler(0, targetAngle, 0);
        }
       
        if (!isGameover)
        {
            string otherPlayerName;
            if (PhotonNetwork.PlayerList.Length > 1)
            {
                if (PhotonNetwork.IsMasterClient) otherPlayerName = PhotonNetwork.PlayerList[1].NickName;
                else otherPlayerName = PhotonNetwork.PlayerList[0].NickName;
                playerLeftText.text = PhotonNetwork.NickName + "(You) VS " + otherPlayerName; 
            }
        }
       
       /* if (isGameover)
        {
            if (gointToLobbyTime <= 0) MenuButton();
            else gointToLobbyTime -= Time.deltaTime;
            goingToLobbyText.text = "Going to Lobby in " + (int)gointToLobbyTime;
            Debug.Log("Game Over In update to go to Lobby Time " + gointToLobbyTime);
        }*/

        // Setting Client Word
        if (!PhotonNetwork.IsMasterClient &&!isWordSet)
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
    }

    private void FixedUpdate()
    {
        if (!photonView.IsMine) return;
        CameraFollow();
    }
   
    private void CameraFollow()
    {
        Vector3 desiredPos = transform.position + offset;
        Vector3 smoothPos = Vector3.Lerp(cam.position, desiredPos, smoothSpeed);
        cam.position = smoothPos;

        cam.LookAt(transform);
    }

    private void OnDestroy()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (level < (words.Length-1)) level++;
            else level = 0;
            PlayerPrefs.SetInt("Level", level);
            PlayerPrefs.Save();
        }
        PlayerPrefs.SetString("ClientWord", "");
    }

    private void OnCollisionStay(Collision collision)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            if (photonView.IsMine && collision.gameObject.CompareTag("ABC") && (wordCounter < clientWord.Length) && isCollisionExit)
            {
                if ((clientWord[wordCounter].ToString().ToUpper() == collision.gameObject.transform.name) && (wordCounter < clientWord.Length) && isCollisionExit)
                {
                    photonView.RPC("PlayreCollideWord", RpcTarget.AllBuffered);
                }
            }
        }
        else
        {
            if (photonView.IsMine && collision.gameObject.CompareTag("ABC") && (wordCounter < currWord.Length) && isCollisionExit)
            {
                if ((currWord[wordCounter].ToString().ToUpper() == collision.gameObject.transform.name) && (wordCounter < currWord.Length) && isCollisionExit)
                {
                    photonView.RPC("PlayreCollideWord", RpcTarget.AllBuffered);
                }
            }
        }
       
    }

    private void OnCollisionExit(Collision collision)
    {
        if (!photonView.IsMine) return;
        photonView.RPC("PlayerExitWordCollision", RpcTarget.AllBuffered);
    }

    [PunRPC]
    private void PlayreCollideWord()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            isCollisionExit = false;
            wordBG[wordCounter].color = Color.green;
            wordCounter++;
            if (wordCounter == currWord.Length)
            {
                photonView.RPC("GameOver", RpcTarget.AllBuffered);
            }
        }
        else
        {
            isCollisionExit = false;
            wordBG[wordCounter].color = Color.green;
            wordCounter++;
            if (!photonView.IsMine) return;
            if (wordCounter == clientWord.Length)
            {
                photonView.RPC("GameOver", RpcTarget.AllBuffered);
            }
        }
       
    }

    [PunRPC]
    private void PlayerExitWordCollision()
    {
        isCollisionExit = true;
    }

    [PunRPC] 
    private void GameOver()
    {
        playerLeftText.gameObject.SetActive(false);
        isGameover = true;
        gameOverScreen.SetActive(true);
        if (photonView.IsMine) winnerName.text = PhotonNetwork.NickName + "(You) Won";
        else winnerName.text = photonView.Owner.NickName + " Won\nYou Lose";
       
        if (!photonView.IsMine) return;
        joystick.gameObject.SetActive(false);
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

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        if (isGameover) MenuButton();
        else
        {
            playerLeftText.text = otherPlayer.NickName + " Left the Game";
            Invoke("GameOver", 2);
        }
        base.OnPlayerLeftRoom(otherPlayer);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(stream.IsWriting)
        {
            if(PhotonNetwork.IsMasterClient)
            {
                stream.SendNext(level);
            }
        }
        else
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                level = (int)stream.ReceiveNext();
                currWord = words[level];
                PlayerPrefs.SetString("ClientWord", currWord);
                PlayerPrefs.Save();
            }
        }
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

    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);
        playerLeftText.gameObject.SetActive(true);
        playerLeftText.text = "No Internet Going Back To Lobby";
        MenuButton();
    }
}
