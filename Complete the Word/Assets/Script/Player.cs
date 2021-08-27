using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviourPunCallbacks, IInRoomCallbacks,IPunObservable
{
    [Header("Movement")]
    [SerializeField] Rigidbody rigidBody;
    [SerializeField] Transform body;
    [SerializeField] float speed;
    [SerializeField] Animator animator;
    public static bool isGameover;

    [Header("Swipe")]
    [SerializeField] Camera cam;
    public float sensitivity;
    private int leftFingerId, rightFingerId;
    private float halfScreenWidth;
    private Vector2 lookInput;
    private Vector2 moveTouchStartPosition;
    private Vector2 moveInput;

    [Header("Word")]
    [SerializeField] Text[] texts;
    [SerializeField] Image[] wordBG;
    private string masterWord, clientWord;
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
        }
    }

    private void Start()
    {
        if (!photonView.IsMine)
        {
            cam.enabled = false;
            this.enabled = false;

            return;
        }

        wordCanvas.SetActive(true);
        isGameover = false;
        isCollisionExit = true;
        isWordSet = false;

        leftFingerId = -1;
        rightFingerId = -1;
        halfScreenWidth = Screen.width / 2;
    }

    private void Update()
    {
        if (!photonView.IsMine) return;

        if (isGameover)
        {
            photonView.RPC("GameOver", RpcTarget.AllBuffered);
        }
        else
        {
            rigidBody.isKinematic = false;

            GetTouchInput();

            if (leftFingerId != -1)
            {
                // Ony move if the left finger is being tracked
                Move();
            }
            else
            {
                rigidBody.velocity = Vector3.zero;
            }

            if (rightFingerId != -1)
            {
                // Ony look around if the right finger is being tracked
                LookAround();
            }

            // Setting Animation
            if (rigidBody.velocity.magnitude > 0.8f)
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
        SetSimilarWord();
    }

    private void FixedUpdate()
    {
        /*if (!photonView.IsMine) return;
        CameraFollow();*/
    }

    private void OnDestroy()
    {
        IncreaseWordLevel();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            if (photonView.IsMine && collision.gameObject.CompareTag("ABC") && (wordCounter < clientWord.Length) && isCollisionExit)
            {
                if ((clientWord[wordCounter].ToString().ToUpper() == collision.gameObject.transform.name) && (wordCounter < clientWord.Length) && isCollisionExit)
                {
                    photonView.RPC("PlayreCollideWord", RpcTarget.AllBuffered);
                }
            }
        }
        else
        {
            if (photonView.IsMine && collision.gameObject.CompareTag("ABC") && (wordCounter < masterWord.Length) && isCollisionExit)
            {
                if ((masterWord[wordCounter].ToString().ToUpper() == collision.gameObject.transform.name) && (wordCounter < masterWord.Length) && isCollisionExit)
                {
                    photonView.RPC("PlayreCollideWord", RpcTarget.AllBuffered);
                }
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (!photonView.IsMine) return;
        photonView.RPC("PlayerExitWordCollision", RpcTarget.AllBuffered);
    }




    private void GetTouchInput()
    {
        // Iterate through all the detected touches
        for (int i = 0; i < Input.touchCount; i++)
        {

            Touch t = Input.GetTouch(i);

            // Check each touch's phase
            switch (t.phase)
            {
                case TouchPhase.Began:

                    if (t.position.x < halfScreenWidth && leftFingerId == -1)
                    {
                        // Start tracking the left finger if it was not previously being tracked
                        leftFingerId = t.fingerId;

                        // Set the start position for the movement control finger
                        moveTouchStartPosition = t.position;
                    }
                    else if (t.position.x > halfScreenWidth && rightFingerId == -1)
                    {
                        // Start tracking the rightfinger if it was not previously being tracked
                        rightFingerId = t.fingerId;
                    }

                    break;
                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    if (t.fingerId == leftFingerId)
                    {
                        // Stop tracking the left finger
                        leftFingerId = -1;
                    }
                    else if (t.fingerId == rightFingerId)
                    {
                        // Stop tracking the right finger
                        rightFingerId = -1;
                    }

                    break;
                case TouchPhase.Moved:

                    // Get input for looking around
                    if (t.fingerId == rightFingerId)
                    {
                        lookInput = t.deltaPosition * sensitivity * Time.deltaTime;
                    }
                    else if (t.fingerId == leftFingerId)
                    {

                        // calculating the position delta from the start position
                        moveInput = t.position - moveTouchStartPosition;
                    }

                    break;
                case TouchPhase.Stationary:
                    // Set the look input to zero if the finger is still
                    if (t.fingerId == rightFingerId)
                    {
                        lookInput = Vector2.zero;
                    }
                    break;
            }
        }
    }

    private void Move()
    {
        // Move
        Vector3 direction = transform.right * moveInput.x + transform.forward * moveInput.y;
        rigidBody.velocity = direction * speed;

        // Look
        if (direction.magnitude >= 0.1)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            body.rotation = Quaternion.Euler(0, targetAngle, 0);
        }
    }

    private void LookAround()
    {
        transform.Rotate(Vector3.up * lookInput.x * sensitivity);
    }

    private void SetSimilarWord()
    {
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
       
        if (PhotonNetwork.IsMasterClient)
        {
            isCollisionExit = false;
            wordBG[wordCounter].color = Color.green;
            wordCounter++;
            if (!photonView.IsMine) return;
            if (wordCounter == masterWord.Length)
            {
                Debug.Log("in master");
                photonView.RPC("SetWinnerName", RpcTarget.AllBuffered);
                photonView.RPC("SetGameOverTrue", RpcTarget.AllBuffered);
            }
        }
        else
        {
            isCollisionExit = false;
            wordBG[wordCounter].color = Color.green;
            wordCounter++;
            if (!photonView.IsMine) return;
            if (wordCounter == clientWord.Length)
            {
                Debug.Log("in client");
                photonView.RPC("SetWinnerName", RpcTarget.AllBuffered);
                photonView.RPC("SetGameOverTrue", RpcTarget.AllBuffered);
            }
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
        rigidBody.isKinematic = true;
        wordCanvas.SetActive(false);
        if (!photonView.IsMine) return;
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

    [PunRPC]
    private void SetGameOverTrue()
    {
        isGameover = true;
    }

    [PunRPC]
    private void SetWinnerName()
    {
        winnerName = photonView.Owner.NickName;
    }



    public void LeaveRoom()
    {
        IncreaseWordLevel();
        StartCoroutine(LeaveRoomGoMenu());
    }

    IEnumerator LeaveRoomGoMenu()
    {
        PhotonNetwork.LeaveRoom();
        while (PhotonNetwork.InRoom)
            yield return null;
        //  SceneManager.LoadScene(0);
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
