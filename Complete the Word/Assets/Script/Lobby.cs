using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Lobby : MonoBehaviourPunCallbacks,IInRoomCallbacks
{
    public static Lobby lobby;
    [SerializeField] GameObject mainPanel, changeNamePanel;
    [SerializeField] GameObject offlineText;
    [SerializeField] GameObject battelButton;
    [SerializeField] GameObject cancleButton;
    private string userName;
    [SerializeField] Text userNameText;
    [SerializeField] Text watingText;
    private bool isJoiningRoom,isOtherPlayerLeave;



    // Delay Start
    private bool delayStart;
    public bool isGameLoaded;
    public int currentScene;

    public int playerInRoom;
    public int myNumberInRoom;
    public int playerInGame;

    private bool readyToCount, readyToStart;
    public float startingTime;
    private float lessThanMaxPlayers, atMaxPlayer, timeToStart;




    private void Awake()
    {
        if (!PhotonNetwork.IsConnected)
        {
            watingText.gameObject.SetActive(false);
            offlineText.SetActive(true);
            mainPanel.SetActive(true);
            changeNamePanel.SetActive(false);
            battelButton.SetActive(false);
            cancleButton.SetActive(false);
            PhotonNetwork.ConnectUsingSettings();
        }
        else
        {
            watingText.gameObject.SetActive(false);
            offlineText.SetActive(false);
            mainPanel.SetActive(true);
            changeNamePanel.SetActive(false);
            battelButton.SetActive(true);
        }
        
        userName = PlayerPrefs.GetString("UserName", "Player");
        userNameText.text = userName;
        PhotonNetwork.NickName = userName;
        if (!PlayerPrefs.HasKey("UserName")) ChangeName();

        // Connecting To MasterPhoton
        PhotonNetwork.GameVersion = "0";
    }


    private void Start()
    {
        isOtherPlayerLeave = false;
        isJoiningRoom = true;
        delayStart = true;
        readyToCount = false;
        readyToStart = false;
        lessThanMaxPlayers = startingTime;
        atMaxPlayer = 6;
        timeToStart = startingTime;
    }

    private void Update()
    {
        if (delayStart)
        {
            if (playerInRoom == 1)
            {
                RestartTimer();
            }
            if (!isGameLoaded)
            {
                if (readyToStart)
                {
                    atMaxPlayer -= Time.deltaTime;
                    lessThanMaxPlayers = atMaxPlayer;
                    timeToStart = atMaxPlayer;

                    if (isJoiningRoom)
                    {
                        watingText.gameObject.SetActive(true);
                        string otherPlayerName;
                        if (PhotonNetwork.IsMasterClient) otherPlayerName = PhotonNetwork.PlayerList[1].NickName;
                        else otherPlayerName = PhotonNetwork.PlayerList[0].NickName;
                        watingText.text = otherPlayerName + " Joined \n Match Start in " + (int)timeToStart;
                    }
                }
                else if (readyToCount)
                {
                    lessThanMaxPlayers -= Time.deltaTime;
                    timeToStart = lessThanMaxPlayers;
                }
                if (timeToStart <= 0)
                {
                    StartGame();
                }
            }
        }
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        Debug.Log("Connected to Photon Server");
        offlineText.SetActive(false);
        PhotonNetwork.AutomaticallySyncScene = true;
        if(!isOtherPlayerLeave) battelButton.SetActive(true);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);
        isJoiningRoom = false;
        offlineText.SetActive(true);
        battelButton.SetActive(false);
        cancleButton.SetActive(false);
        PhotonNetwork.ConnectUsingSettings();
    }
   
    public void BattelButton()
    {
        if (PhotonNetwork.IsConnected)
        {
            isJoiningRoom = true;
            battelButton.SetActive(false);
            PhotonNetwork.JoinRandomRoom();
            cancleButton.SetActive(true);
        }
        else
        {
            watingText.gameObject.SetActive(true);
            watingText.text = "Not Connected to internet try again";
            if (!PhotonNetwork.IsConnected) PhotonNetwork.ConnectUsingSettings();
        }
    }

    public void CancleButton()
    {
        watingText.gameObject.SetActive(true);
        watingText.text = "Match Cancel\nTry again";
        isOtherPlayerLeave = true;
        Invoke("LoadLobbyScene", 3);
        isJoiningRoom = false;
      //  watingText.gameObject.SetActive(false);
        cancleButton.SetActive(false);
        PhotonNetwork.LeaveRoom();
        //battelButton.SetActive(true);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        base.OnJoinRandomFailed(returnCode, message);
        Debug.Log("Failed To Join the Room " + message);
        CreatRoom();
    } 

    private void CreatRoom()
    {
        int randomRoomName = Random.Range(0, 10);
        RoomOptions roomOptions = new RoomOptions() { IsVisible = true, IsOpen = true, MaxPlayers = 2 };
        PhotonNetwork.CreateRoom("Room" + randomRoomName, roomOptions);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        base.OnCreateRoomFailed(returnCode, message);
        Debug.Log("Creat Room Failed");
        CreatRoom();
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

        playerInRoom = PhotonNetwork.PlayerList.Length;
        myNumberInRoom = playerInRoom;
        if (delayStart)
        {
            watingText.gameObject.SetActive(true);
            watingText.text = "Wating for other Player ... ";
            if (playerInRoom > 1)
            {
                readyToCount = true;
            }
            if(playerInRoom == 2)
            {
                readyToStart = true;
                if (!PhotonNetwork.IsMasterClient) return;
                PhotonNetwork.CurrentRoom.IsOpen = false;
            }
        }
        else
        {
            StartGame();
        }
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
     //   photonPlayers = PhotonNetwork.PlayerList;
        playerInRoom++;
        if (delayStart)
        {
            Debug.Log("Player in Room " + playerInRoom);
            if (playerInRoom > 1)
            {
                readyToCount = true;
            }
            if (playerInRoom == 2)
            {
                readyToStart = true;
                if (!PhotonNetwork.IsMasterClient) return;
                PhotonNetwork.CurrentRoom.IsOpen = false;
            }
        }
    }

    private void StartGame()
    {
        isGameLoaded = true;
        if (!PhotonNetwork.IsMasterClient) return;
        if (delayStart) PhotonNetwork.CurrentRoom.IsOpen = false;

        PhotonNetwork.LoadLevel(1);
    }

    private void RestartTimer()
    {
        lessThanMaxPlayers = startingTime;
        timeToStart = startingTime;
        atMaxPlayer = 6;
        readyToCount = false;
        readyToStart = false;
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
        userName = name;
        userNameText.text = userName;
        PhotonNetwork.NickName = userName;
        PlayerPrefs.SetString("UserName", userName);
        PlayerPrefs.Save();
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        isOtherPlayerLeave = true;
        isJoiningRoom = false;
        watingText.gameObject.SetActive(true);
        watingText.text = otherPlayer.NickName + " Leave the room Try Again";
        cancleButton.SetActive(false);
        battelButton.SetActive(false);
        PhotonNetwork.LeaveRoom();
        Invoke("LoadLobbyScene", 3);
    }

    private void LoadLobbyScene()
    {
        SceneManager.LoadScene(0);
    }
}
