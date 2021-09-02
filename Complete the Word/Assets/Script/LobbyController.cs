using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class LobbyController : MonoBehaviourPunCallbacks
{
    [Header("Room")]
    private byte maxPlayers = 4;
    private float timeToWait = 30, currTimeToWaitplayer, currTimetoStartMatch;
    private bool isStartGame;
    public static int selectedPlayer;
    private int selectedButton;

    [Header("UI Panel")]
    [SerializeField] GameObject mainPanel;
    [SerializeField] GameObject changeNamePanel;
    [SerializeField] GameObject playerJoinedPanel;
    [SerializeField] GameObject loadingPanel;
    [SerializeField] GameObject playerSelectPanel;

    [Header("UI Button")]
    [SerializeField] GameObject battelButton;
    [SerializeField] GameObject cancelButton;
    [SerializeField] Button[] playerSelectButtons;

    [Header("UI Text")]
    [SerializeField] Text msgTextMain;
    [SerializeField] Text msgTextPlayerJoined;
    [SerializeField] Text msgTextPlayerSelect;
    [SerializeField] Text playerNameMain;
    [SerializeField] Text[] playerSelectButtonText;


    #region Unity

    private void Start()
    {
        isStartGame = false;
        selectedPlayer = 0;
        currTimeToWaitplayer = timeToWait;
        currTimetoStartMatch = 10;

        mainPanel.SetActive(true);
        changeNamePanel.SetActive(false);
        playerJoinedPanel.SetActive(false);
        playerSelectPanel.SetActive(false);

        battelButton.SetActive(false);
        cancelButton.SetActive(false);

        msgTextPlayerJoined.gameObject.SetActive(false);
        msgTextMain.gameObject.SetActive(true);
        msgTextMain.text = "Offline";


        playerNameMain.text = PlayerPrefs.GetString("UserName", "Player");
        if (!PlayerPrefs.HasKey("UserName"))
        {
            string name = "Player" + Random.Range(100, 999);
            EnterName(name);
        }
        PhotonNetwork.NickName = PlayerPrefs.GetString("UserName", "Player");

        if(!PhotonNetwork.IsConnected) PhotonNetwork.ConnectUsingSettings();
    }

    private void Update()
    {
        if (!PhotonNetwork.IsConnected)
        {
            battelButton.SetActive(false);
            cancelButton.SetActive(false);
            msgTextMain.gameObject.SetActive(true);
            msgTextMain.text = "Offline";
            PhotonNetwork.ConnectUsingSettings();
        }

        if ((PhotonNetwork.PlayerList.Length >= 4) && isStartGame) photonView.RPC("StartGame", RpcTarget.AllBuffered);

        if (isStartGame) photonView.RPC("StartGame", RpcTarget.AllBuffered);

        for (int i = 0; i < playerSelectButtons.Length; i++)
        {
            if (!playerSelectButtons[i].interactable) selectedButton++;
            if ((selectedButton == 3) && (selectedPlayer == 0)) photonView.RPC("SetRemainingPlayer", RpcTarget.AllBuffered);
            break;
        }
    }

    #endregion Unity


    #region Photon

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();

        PhotonNetwork.AutomaticallySyncScene = true;
        msgTextMain.text = "Online";
        battelButton.SetActive(true);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        base.OnJoinRandomFailed(returnCode, message);
        CreateRoom();
    }
   
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        base.OnCreateRoomFailed(returnCode, message);
        CreateRoom();
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

        mainPanel.SetActive(false);
        playerJoinedPanel.SetActive(true);
        if(PhotonNetwork.IsMasterClient) isStartGame = true;
    }
  
    #endregion Photon


    #region Game

    private void CreateRoom()
    {
        int randomRoomNumber = Random.Range(0, 1000);
        RoomOptions roomOptions = new RoomOptions() { IsVisible = true, IsOpen = true, MaxPlayers = maxPlayers };
        PhotonNetwork.CreateRoom("Room" + randomRoomNumber, roomOptions);
    }

    [PunRPC]
    private void StartGame()
    {
        if (currTimeToWaitplayer <= 0)
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.CurrentRoom.IsVisible = false;
            if (PhotonNetwork.IsMasterClient)
            {
                photonView.RPC("PlayerSelectPanel", RpcTarget.AllBuffered);
            }
            else
            {
                mainPanel.SetActive(false);
                changeNamePanel.SetActive(false);
                playerJoinedPanel.SetActive(false);
                loadingPanel.SetActive(true);
            }
        }
        else currTimeToWaitplayer -= Time.deltaTime;

        msgTextPlayerJoined.gameObject.SetActive(true);
        msgTextPlayerJoined.text = "Estimated time " + (int)currTimeToWaitplayer;
    }

    [PunRPC]
    private void PlayerSelectPanel()
    {
        mainPanel.SetActive(false);
        changeNamePanel.SetActive(false);
        loadingPanel.SetActive(false);
        playerJoinedPanel.SetActive(false);
        playerSelectPanel.SetActive(true);

        if (currTimetoStartMatch <= 0)
        {
            if (selectedPlayer == 0) photonView.RPC("SetRemainingPlayer", RpcTarget.AllBuffered);
            PhotonNetwork.LoadLevel(1);
            isStartGame = false;
        }
        else currTimetoStartMatch -= Time.deltaTime;

        msgTextPlayerSelect.text = "Match Start in " + (int)currTimetoStartMatch;
    }

    [PunRPC]
    private void HammerPlayerSelectButtonRPC()
    {
        selectedPlayer = 1;
        playerSelectButtonText[0].text = "Selected";
        playerSelectButtons[0].interactable = false;
    }

    [PunRPC]
    private void GranedePlayerSelectButtonRPC()
    {
        selectedPlayer = 2;
        playerSelectButtonText[1].text = "Selected";
        playerSelectButtons[1].interactable = false;
    }

    [PunRPC]
    private void PlayerSelectButtonRPC()
    {
        selectedPlayer = 3;
        playerSelectButtonText[2].text = "Selected";
        playerSelectButtons[2].interactable = false;
    }

    [PunRPC]
    private void JumpPlayerSelectButtonRPC()
    {
        selectedPlayer = 4;
        playerSelectButtonText[3].text = "Selected";
        playerSelectButtons[3].interactable = false;
    }

    [PunRPC]
    private void SetRemainingPlayer()
    {
        for(int i = 0; i < playerSelectButtons.Length; i++)
        {
            if (playerSelectButtons[i].interactable)
            {
                if (playerSelectButtons[i].gameObject.transform.name == "HammerPlayer Button") HammerPlayerSelectButton();
                else if (playerSelectButtons[i].gameObject.transform.name == "GranedePlayer Button") GranedePlayerSelectButton();
                else if (playerSelectButtons[i].gameObject.transform.name == "Player Button") PlayerSelectButton();
                else JumpPlayerSelectButton();

                break;
            }
        }
    }
    #endregion Game


    #region Button

    public void BattelButton()
    {
        battelButton.SetActive(false);
        cancelButton.SetActive(true);
        PhotonNetwork.JoinRandomRoom();
    }

    public void CancleButton()
    {
        currTimeToWaitplayer = timeToWait;

        msgTextPlayerJoined.gameObject.SetActive(false);
      
        isStartGame = false;
        battelButton.SetActive(true);
        cancelButton.SetActive(false);
        PhotonNetwork.LeaveRoom();
        mainPanel.SetActive(true);
        playerJoinedPanel.SetActive(false);
    }

    public void QuitButton()
    {
        Application.Quit();
    }

    public void ChangeName()
    {
        mainPanel.SetActive(false);
        changeNamePanel.SetActive(true);
    }

    public void CloseChangeName()
    {
        mainPanel.SetActive(true);
        changeNamePanel.SetActive(false);
    }

    public void EnterName(string name)
    {
        playerNameMain.text = name;
        PhotonNetwork.NickName = name;
        PlayerPrefs.SetString("UserName", name);
        PlayerPrefs.Save();
    }


    public void HammerPlayerSelectButton()
    {
        photonView.RPC("HammerPlayerSelectButtonRPC", RpcTarget.AllBuffered);
        for(int i = 0; i < playerSelectButtons.Length; i++)
        {
            playerSelectButtons[i].interactable = false;
        }

    }
    public void GranedePlayerSelectButton()
    {
        photonView.RPC("GranedePlayerSelectButtonRPC", RpcTarget.AllBuffered);
        for (int i = 0; i < playerSelectButtons.Length; i++)
        {
            playerSelectButtons[i].interactable = false;
        }
    }
    public void PlayerSelectButton()
    {
        photonView.RPC("PlayerSelectButtonRPC", RpcTarget.AllBuffered);
        for (int i = 0; i < playerSelectButtons.Length; i++)
        {
            playerSelectButtons[i].interactable = false;
        }
    }
    public void JumpPlayerSelectButton()
    {
        photonView.RPC("JumpPlayerSelectButtonRPC", RpcTarget.AllBuffered);
        for (int i = 0; i < playerSelectButtons.Length; i++)
        {
            playerSelectButtons[i].interactable = false;
        }
    }

    #endregion Button
}
