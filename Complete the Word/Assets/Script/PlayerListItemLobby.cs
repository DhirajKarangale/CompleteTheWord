using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class PlayerListItemLobby : MonoBehaviourPunCallbacks
{
    [SerializeField] Text playerName;
    public Photon.Realtime.Player player;

    public void SetUp(Photon.Realtime.Player _player)
    {
        player = _player;
        playerName.text = _player.NickName;
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        if (player == otherPlayer)
        {
            Destroy(gameObject);
        }
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        Destroy(gameObject);
    }
}
