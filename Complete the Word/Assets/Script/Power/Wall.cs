using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class Wall : MonoBehaviour
{
    [SerializeField] PhotonView photonView;
    [SerializeField] GameObject walls;
    [SerializeField] Text timerText;
    [SerializeField] Text wallUpTimerText;
    [SerializeField] float desableTime, wallDownTime;
    private float currDesableTime, currWallDownTime;
    public bool isWallUp, isWallAllowed;

    private void Start()
    {
        if (!photonView.IsMine) return;

        isWallUp = false;
        isWallAllowed = true;

        currDesableTime = desableTime;
        currWallDownTime = wallDownTime;

        timerText.gameObject.SetActive(false);
        wallUpTimerText.gameObject.SetActive(false);
        walls.SetActive(false);
    }

    private void Update()
    {
        if (GameManager.isGameOver)
        {
            photonView.RPC("DownWall", RpcTarget.AllBuffered);
        }

        if (!photonView.IsMine) return;

        if (!isWallAllowed)
        {
            timerText.gameObject.SetActive(true);

            if (currDesableTime <= 0)
            {
                isWallAllowed = true;
                timerText.gameObject.SetActive(false);
                currDesableTime = desableTime;
            }
            else
            {
                currDesableTime -= Time.deltaTime;
                timerText.text = ((int)currDesableTime).ToString();
            }
        }

        if (isWallUp)
        {
            wallUpTimerText.gameObject.SetActive(true);
            if (currWallDownTime <= 0) photonView.RPC("DownWall", RpcTarget.AllBuffered);
            else
            {
                currWallDownTime -= Time.deltaTime;
                wallUpTimerText.text = ((int)currWallDownTime).ToString();
            }
        } 
    }

    public void WallButton()
    {
        if (!photonView.IsMine) return;
        if (!isWallAllowed) return;

        if (isWallUp) photonView.RPC("DownWall", RpcTarget.AllBuffered);
        else photonView.RPC("UpWall", RpcTarget.AllBuffered);
    }

    [PunRPC]
    private void UpWall()
    {
        isWallUp = true;
        
        walls.SetActive(true);
    }

    [PunRPC]
    private void DownWall()
    {
        isWallAllowed = false;
        isWallUp = false;

        currWallDownTime = wallDownTime;
        wallUpTimerText.gameObject.SetActive(false);
        walls.SetActive(false);
    }
}
