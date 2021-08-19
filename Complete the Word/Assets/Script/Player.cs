using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using EasyJoystick;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviourPunCallbacks, IInRoomCallbacks,IPunObservable
{
    [Header("Movement")]
    [SerializeField] Rigidbody rigidBody;
    [SerializeField] Transform body;
    [SerializeField] float speed;
    private Joystick joystick;
    [SerializeField] Animator animator;
    public static bool isGameover;

    [Header("Camera Follow")]
    private Transform cam;
    public Vector3 offset;
    private float smoothSpeed = 0.125f;

    [Header("Word")]
    [SerializeField] Text[] texts;
    [SerializeField] Image[] wordBG;
    private string masterWord, clientWord, currWord;
    private static string[] words = { "Bad", "Good", "Nice", "Home", "Help", "Hard", "Easy", "Had", "Mad" };
    private int wordCounter = 0;
    private bool isCollisionExit, isWordSet;
    private static int level;

    [Header("UI")]
    [SerializeField] GameObject wordCanvas;
    public static string winnerName;
    

    private void Awake()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            level = PlayerPrefs.GetInt("Level", 0);
            masterWord = words[level];
            photonView.RPC("SetWord", RpcTarget.AllBuffered, masterWord);
            offset = new Vector3(9, 4.5f, 0);
        }
        else
        {
            offset = new Vector3(-9, 4.5f, 0);
        }
    }

    private void Start()
    {
        if (!photonView.IsMine) return;
      
        cam = FindObjectOfType<Camera>().transform;
        joystick = FindObjectOfType<Joystick>();

        wordCanvas.SetActive(true);
        isGameover = false;
        isCollisionExit = true;
        isWordSet = false;
    }

    private void Update()
    {
        if (!photonView.IsMine) return;
               

        if (isGameover) photonView.RPC("GameOver", RpcTarget.AllBuffered);
        Move();
        SetSimilarWord();     
    }

    private void FixedUpdate()
    {
        if (!photonView.IsMine) return;
        CameraFollow();
    }

    private void OnDestroy()
    {
        IncreaseWordLevel();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (photonView.IsMine && collision.gameObject.CompareTag("ABC") && (wordCounter < currWord.Length) && isCollisionExit && (currWord[wordCounter].ToString().ToUpper() == collision.gameObject.transform.name))
        {
            photonView.RPC("PlayreCollideWord", RpcTarget.AllBuffered);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (!photonView.IsMine) return;
        photonView.RPC("PlayerExitWordCollision", RpcTarget.AllBuffered);
    }




    private void Move()
    {
        if (isGameover) rigidBody.isKinematic = true;
        else rigidBody.isKinematic = false;

        // Input
        float horrizantal = joystick.Horizontal();
        float vertical = joystick.Vertical();

        // Move
        Vector3 direction = transform.right * horrizantal + transform.forward * vertical;
        rigidBody.velocity = direction * speed;
       

        // Setting Animation
        if (!isGameover)
        {
            if (rigidBody.velocity.magnitude > 0)
            {
                animator.SetBool("isRun", true);
                animator.SetBool("isWin", false);
                animator.SetBool("isLoose", false);
            }
            else
            {
                animator.SetBool("isRun", false);
                animator.SetBool("isWin", false);
                animator.SetBool("isLoose", false);
            }
        }

        // Look
        if (direction.magnitude >= 0.1)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            body.rotation = Quaternion.Euler(0, targetAngle, 0);
        }
    }

    private void SetSimilarWord()
    {
        // Set Word For Player
        if (PhotonNetwork.IsMasterClient) currWord = masterWord;
        else currWord = clientWord;

        // Setting Client Word
        if (!PhotonNetwork.IsMasterClient && !isWordSet)
        {
            clientWord = PlayerPrefs.GetString("ClientWord");
            for (int i = 0; i < words.Length; i++)
            {
                if (clientWord == words[i])
                {
                    photonView.RPC("SetWord", RpcTarget.AllBuffered, clientWord);
                    isWordSet = true;
                    break;
                }
            }
        }
    }
   
    private void CameraFollow()
    {
        Vector3 desiredPos = transform.position + offset;
        Vector3 smoothPos = Vector3.Lerp(cam.position, desiredPos, smoothSpeed);
        cam.position = smoothPos;

        cam.LookAt(transform);
    }

    public static void IncreaseWordLevel()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (level < (words.Length - 1)) level++;
            else level = 0;
            PlayerPrefs.SetInt("Level", level);
            PlayerPrefs.Save();
        }
        PlayerPrefs.SetString("ClientWord", "");
    }
   



    [PunRPC]
    private void PlayreCollideWord()
    {
        isCollisionExit = false;
        wordBG[wordCounter].color = Color.green;
        wordCounter++;
        
        if (wordCounter == currWord.Length)
        {
            if (photonView.IsMine) winnerName = photonView.Owner.NickName;
           
            isGameover = true;
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
        wordCanvas.SetActive(false);
        if (winnerName == PlayerPrefs.GetString("UserName"))
        {
            animator.SetBool("isRun", false);
            animator.SetBool("isWin", true);
            animator.SetBool("isLoose", false);
        }
        else
        {
            animator.SetBool("isRun", false);
            animator.SetBool("isWin", false);
            animator.SetBool("isLoose", true);
        }
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




    public void LeaveRoom()
    {
        IncreaseWordLevel();
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene(0);
        base.OnLeftRoom();
    }
    
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(stream.IsWriting)
        {
            if(PhotonNetwork.IsMasterClient)
            {
                stream.SendNext(level);
            }
        }
        else
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                level = (int)stream.ReceiveNext();
                masterWord = words[level];
                PlayerPrefs.SetString("ClientWord", masterWord);
                PlayerPrefs.Save();
            }
        }
    }
}
