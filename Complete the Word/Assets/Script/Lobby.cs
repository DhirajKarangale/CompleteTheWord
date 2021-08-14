using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class Lobby : MonoBehaviourPunCallbacks
{
    public static Lobby lobby;
    [SerializeField] GameObject mainPanel, changeNamePanel;
    [SerializeField] GameObject offlineText;
    [SerializeField] GameObject battelButton;
    [SerializeField] GameObject cancleButton;
    private string userName;
    [SerializeField] Text userNameText;

    private void Awake()
    {
        offlineText.SetActive(true);
        mainPanel.SetActive(true);
        changeNamePanel.SetActive(false);
        
        userName = PlayerPrefs.GetString("UserName", "Player");
        userNameText.text = userName;
        PhotonNetwork.NickName = userName;
        if (!PlayerPrefs.HasKey("UserName")) ChangeName();

        // Connecting To MasterPhoton
        PhotonNetwork.GameVersion = "0";
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        Debug.Log("Connected to Photon Server");
        offlineText.SetActive(false);
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
}
