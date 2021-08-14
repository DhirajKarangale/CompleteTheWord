using Photon.Pun;
using UnityEngine;
using EasyJoystick;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] PhotonView photonView;
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
    private string currWord;
    private string[] words = { "Bad", "Good", "Nice", "Home", "Help", "Hard", "Easy", "Had", "Mad" };
    private int wordCounter = 0;
    public static bool isGameover;
    private bool isCollisionExit;

    [Header("UI")]
    [SerializeField] GameObject gameOverScreen;
    [SerializeField] Text winnerName;
    [SerializeField] Text playerLeftText;

    private void Start()
    {
        if (!photonView.IsMine) return;
        joystick = FindObjectOfType<Joystick>();
        cam = FindObjectOfType<Camera>().transform;

        gameOverScreen.SetActive(false);
        joystick.gameObject.SetActive(true);
        playerLeftText.gameObject.SetActive(false);
        
        if (PhotonNetwork.PlayerList.Length == 1)
        {
            offset = new Vector3(8, 5, 0);
        }
        else
        {
            offset = new Vector3(-8, 5, 0);
        }

        winnerName.gameObject.SetActive(false);
        isGameover = false;
        isCollisionExit = true;

        int seed = Random.Range(0, words.Length);
        Random.InitState(seed);
        Debug.Log("Seed " + seed);

        currWord = words[seed];
        photonView.RPC("SetWord", RpcTarget.AllBuffered, currWord);
    }

    private void Update()
    {
        if (isGameover) return;
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

    private void OnCollisionStay(Collision collision)
    {
        if (photonView.IsMine && collision.gameObject.CompareTag("ABC") && (wordCounter < currWord.Length) && isCollisionExit)
        {
            if((currWord[wordCounter].ToString().ToUpper() == collision.gameObject.transform.name) && (wordCounter < currWord.Length) && isCollisionExit)
            {
                photonView.RPC("PlayreCollideWord", RpcTarget.AllBuffered);
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
        isCollisionExit = false;
        wordBG[wordCounter].color = Color.green;
        wordCounter++;
        if (wordCounter == currWord.Length)
        {
            photonView.RPC("GameOver", RpcTarget.AllBuffered);
            joystick.gameObject.SetActive(false);
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
        isGameover = true;
        gameOverScreen.SetActive(true);
        if (photonView.IsMine)
        {
            winnerName.gameObject.SetActive(true);
            winnerName.text = PhotonNetwork.NickName + " Wins";
        }
        else
        {
            winnerName.gameObject.SetActive(true);
            winnerName.text = photonView.Owner.NickName + " Wins";
        }
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

}
