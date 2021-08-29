using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using System.Collections.Generic;

public class LobbyController : MonoBehaviourPunCallbacks
{
    [Header("Room")]
    private byte maxPlayers = 4;
    private float timeToStart = 5, currTime;
    private bool isStartGame;

    [Header("Player List")]
    [SerializeField] PlayerListItemLobby playerListItemLobby;
    private List<PlayerListItemLobby> playerNameLobbyList = new List<PlayerListItemLobby>();
    [SerializeField] Transform playerListContent;

    [Header("UI Panel")]
    [SerializeField] GameObject mainPanel;
    [SerializeField] GameObject changeNamePanel;
    [SerializeField] GameObject playerJoinedPanel;

    [Header("UI Button")]
    [SerializeField] GameObject battelButton;
    [SerializeField] GameObject cancelButton;
    [SerializeField] GameObject startButton;

    [Header("UI Text")]
    [SerializeField] Text msgTextMain;
    [SerializeField] Text msgTextPlayerJoined;
    [SerializeField] Text playerNameMain;


    #region Unity

    private void Start()
    {
        playerNameLobbyList.Clear();

        isStartGame = false;
        currTime = timeToStart;

        changeNamePanel.SetActive(false);
        mainPanel.SetActive(true);
        playerJoinedPanel.SetActive(false);

        startButton.SetActive(false);
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

        PhotonNetwork.ConnectUsingSettings();
    }

    private void Update()
    {
        if (PhotonNetwork.PlayerList.Length >= 4)
        {
            currTime = 10;
            StartGameButton();
        }

        if (PhotonNetwork.IsMasterClient) startButton.SetActive(true);
        else startButton.SetActive(false);

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
       
        GetCurrentRoomPlayer();
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        msgTextPlayerJoined.gameObject.SetActive(true);
        msgTextPlayerJoined.text = newPlayer.NickName + " Joined";
        Invoke("SetMsgTextPlayerJoinToFalse", 2);
        AddPlayerListing(newPlayer);
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);

        photonView.RPC("PlayerLeftRoom", RpcTarget.All, otherPlayer.NickName);

        int index = playerNameLobbyList.FindIndex(x => x.player == otherPlayer);
        if (index != -1)
        {
            Destroy(playerNameLobbyList[index].gameObject);
            playerNameLobbyList.RemoveAt(index);
        }
    }

    #endregion Photon


    #region Game

    private void CreateRoom()
    {
        int randomRoomNumber = Random.Range(0, 1000);
        RoomOptions roomOptions = new RoomOptions() { IsVisible = true, IsOpen = true, MaxPlayers = maxPlayers };
        PhotonNetwork.CreateRoom("Room" + randomRoomNumber, roomOptions);
    }

    private void SetMsgTextPlayerJoinToFalse()
    {
        msgTextPlayerJoined.gameObject.SetActive(false);
    }

    private void GetCurrentRoomPlayer()
    {
        for(int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            AddPlayerListing(PhotonNetwork.PlayerList[i]);
        }
    }

    private void AddPlayerListing(Photon.Realtime.Player player)
    {
        PlayerListItemLobby listing = Instantiate(playerListItemLobby, playerListContent);
        if (listing != null)
        {
            listing.SetUp(player);
            playerNameLobbyList.Add(listing);
        }
    }

    [PunRPC]
    private void StartgameTimer()
    {
        if (currTime <= 0)
        {
            PhotonNetwork.LoadLevel(1);
            isStartGame = false;
        }
        else
        {
            currTime -= Time.deltaTime;
        }
        msgTextPlayerJoined.gameObject.SetActive(true);
        msgTextPlayerJoined.text = "Match Start in\n" + (int)currTime;
    }

    [PunRPC]
    private void PlayerLeftRoom(string playerName)
    {
        msgTextPlayerJoined.gameObject.SetActive(true);
        msgTextPlayerJoined.text = playerName + " Left";
        Invoke("SetMsgTextPlayerJoinToFalse", 2);
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

        for(int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            if (playerNameLobbyList[i] != null) Destroy(playerNameLobbyList[i].gameObject);
        }
        playerNameLobbyList.Clear();
      
        isStartGame = false;
        battelButton.SetActive(true);
        cancelButton.SetActive(false);
        PhotonNetwork.LeaveRoom();
        mainPanel.SetActive(true);
        playerJoinedPanel.SetActive(false);
    }

    public void StartGameButton()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;
        isStartGame = true;
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
