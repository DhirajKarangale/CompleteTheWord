using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class Lobby : MonoBehaviourPunCallbacks
{
    public static Lobby lobby;

    [SerializeField] GameObject battelButton;
    [SerializeField] GameObject cancleButton;

    private void Awake()
    {
        if (Lobby.lobby == null)
        {
            Lobby.lobby = this;
        }
        else
        {
            if (Lobby.lobby != this)
            {
                Destroy(Lobby.lobby.gameObject);
                Lobby.lobby = this;
            }
        }
        DontDestroyOnLoad(this.gameObject);

        // Connecting To MasterPhoton
        PhotonNetwork.GameVersion = "0";
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        Debug.Log("Connected to Photon Server");
        PhotonNetwork.AutomaticallySyncScene = true;
        battelButton.SetActive(true);
    }

    public void BattelButton()
    {
        battelButton.SetActive(false);
        PhotonNetwork.JoinRandomRoom();
        cancleButton.SetActive(true);
    }

    public void CancleButton()
    {
        cancleButton.SetActive(false);
        PhotonNetwork.LeaveRoom();
        battelButton.SetActive(true);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        base.OnJoinRandomFailed(returnCode, message);
        Debug.Log("Failed To Join the Room");
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
        Debug.Log("Romm Join");
        if (!PhotonNetwork.IsMasterClient) return;
        StartGame();
    }

    private void StartGame()
    {
        PhotonNetwork.LoadLevel(1);
    }
        
}
