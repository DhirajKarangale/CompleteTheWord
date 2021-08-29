using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class LobbyController : MonoBehaviourPunCallbacks
{
    [Header("Room")]
    private byte maxPlayers = 4;
    private float timeToStart = 30, currTime;
    private bool isStartGame;

    [Header("UI Panel")]
    [SerializeField] GameObject mainPanel;
    [SerializeField] GameObject changeNamePanel;
    [SerializeField] GameObject playerJoinedPanel;
    [SerializeField] GameObject loadingPanel;

    [Header("UI Button")]
    [SerializeField] GameObject battelButton;
    [SerializeField] GameObject cancelButton;

    [Header("UI Text")]
    [SerializeField] Text msgTextMain;
    [SerializeField] Text msgTextPlayerJoined;
    [SerializeField] Text playerNameMain;


    #region Unity

    private void Start()
    {
        isStartGame = false;
        currTime = timeToStart;

        changeNamePanel.SetActive(false);
        mainPanel.SetActive(true);
        playerJoinedPanel.SetActive(false);

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

        if ((PhotonNetwork.PlayerList.Length >= 4) && isStartGame) StartGame();
       
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

    private void StartGame()
    {
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;
        if(PhotonNetwork.IsMasterClient)
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
        }
    }

    [PunRPC]
    private void StartgameTimer()
    {
        if (currTime <= 0)
        {
            StartGame();
            isStartGame = false;
        }
        else
        {
            currTime -= Time.deltaTime;
        }
        msgTextPlayerJoined.gameObject.SetActive(true);
        msgTextPlayerJoined.text = "Estimated time " + (int)currTime;
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
        currTime = timeToStart;

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

    #endregion Button
}
