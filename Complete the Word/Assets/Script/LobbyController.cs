using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class LobbyController : MonoBehaviourPunCallbacks
{
    [Header("Room")]
    private byte maxPlayers = 4;
    private float timeToWait = 5;
    private float currTimeToWaitplayer, currTimetoStartMatch;
    private bool isStartGame;
    public static int selectedPlayer;

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
        loadingPanel.SetActive(false);

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

        if (!PhotonNetwork.IsConnected) PhotonNetwork.ConnectUsingSettings();
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

        if (PhotonNetwork.PlayerList.Length >= 4) StartGame();

        if (isStartGame) photonView.RPC("StartgameTimer", RpcTarget.AllBuffered);
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
        changeNamePanel.SetActive(false);
        playerJoinedPanel.SetActive(true);
        loadingPanel.SetActive(false);
        playerSelectPanel.SetActive(false);
        if (PhotonNetwork.IsMasterClient) isStartGame = true;
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        if (PhotonNetwork.PlayerList.Length == 1) CancleButton();
    }

    #endregion Photon


    #region Game

    private void CreateRoom()
    {
        int randomRoomNumber = Random.Range(0, 1000);
        RoomOptions roomOptions = new RoomOptions() { IsVisible = true, IsOpen = true, MaxPlayers = maxPlayers };
        PhotonNetwork.CreateRoom("Room" + randomRoomNumber, roomOptions);
    }

    private void StartGame()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("PlayerSelectTimer", RpcTarget.All);
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.CurrentRoom.IsVisible = false;
        }
        else
        {
            mainPanel.SetActive(false);
            changeNamePanel.SetActive(false);
            playerJoinedPanel.SetActive(false);
            loadingPanel.SetActive(true);
            playerSelectPanel.SetActive(false);
        }
    }

    private void PlayerSelectPanel()
    {
        if (selectedPlayer == 0) photonView.RPC("SetRemainingPlayer", RpcTarget.AllBuffered);

        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel(1);
            isStartGame = false;
        }
        else
        {
            mainPanel.SetActive(false);
            changeNamePanel.SetActive(false);
            playerJoinedPanel.SetActive(false);
            loadingPanel.SetActive(true);
            playerSelectPanel.SetActive(false);
        }
    }

    [PunRPC]
    private void HammerPlayerSelectButtonRPC()
    {
        playerSelectButtonText[0].text = "Selected";
        playerSelectButtons[0].interactable = false;
    }

    [PunRPC]
    private void GranedePlayerSelectButtonRPC()
    {
        playerSelectButtonText[1].text = "Selected";
        playerSelectButtons[1].interactable = false;
    }

    [PunRPC]
    private void PlayerSelectButtonRPC()
    {
        playerSelectButtonText[2].text = "Selected";
        playerSelectButtons[2].interactable = false;
    }

    [PunRPC]
    private void JumpPlayerSelectButtonRPC()
    {
        playerSelectButtonText[3].text = "Selected";
        playerSelectButtons[3].interactable = false;
    }

    [PunRPC]
    private void SetRemainingPlayer()
    {
        for (int i = 0; i < playerSelectButtons.Length; i++)
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

    [PunRPC]
    private void StartgameTimer()
    {
        if (currTimeToWaitplayer <= 0) StartGame();
        else currTimeToWaitplayer -= Time.deltaTime;

        msgTextPlayerJoined.gameObject.SetActive(true);
        msgTextPlayerJoined.text = "Estimated time " + (int)currTimeToWaitplayer;
    }

    [PunRPC]
    private void PlayerSelectTimer()
    {
        mainPanel.SetActive(false);
        changeNamePanel.SetActive(false);
        loadingPanel.SetActive(false);
        playerJoinedPanel.SetActive(false);
        playerSelectPanel.SetActive(true);

        if (currTimetoStartMatch <= 0) PlayerSelectPanel();
        else currTimetoStartMatch -= Time.deltaTime;

        msgTextPlayerSelect.gameObject.SetActive(true);
        msgTextPlayerSelect.text = "Estimated time " + (int)currTimetoStartMatch;
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
        changeNamePanel.SetActive(false);
        playerJoinedPanel.SetActive(false);
        loadingPanel.SetActive(false);
        playerSelectPanel.SetActive(false);
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
        selectedPlayer = 1;
        photonView.RPC("HammerPlayerSelectButtonRPC", RpcTarget.AllBuffered);

        for (int i = 0; i < playerSelectButtons.Length; i++)
        {
            playerSelectButtons[i].interactable = false;
        }
    }
    public void GranedePlayerSelectButton()
    {
        selectedPlayer = 2;
        photonView.RPC("GranedePlayerSelectButtonRPC", RpcTarget.AllBuffered);

        for (int i = 0; i < playerSelectButtons.Length; i++)
        {
            playerSelectButtons[i].interactable = false;
        }
    }
    public void PlayerSelectButton()
    {
        selectedPlayer = 3;
        photonView.RPC("PlayerSelectButtonRPC", RpcTarget.AllBuffered);

        for (int i = 0; i < playerSelectButtons.Length; i++)
        {
            playerSelectButtons[i].interactable = false;
        }
    }
    public void JumpPlayerSelectButton()
    {
        selectedPlayer = 4;
        photonView.RPC("JumpPlayerSelectButtonRPC", RpcTarget.AllBuffered);

        for (int i = 0; i < playerSelectButtons.Length; i++)
        {
            playerSelectButtons[i].interactable = false;
        }
    }

    #endregion Button
}
