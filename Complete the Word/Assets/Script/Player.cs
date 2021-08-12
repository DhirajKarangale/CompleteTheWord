using Photon.Pun;
using UnityEngine;
using EasyJoystick;
using UnityEngine.UI;


public class Player : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] PhotonView photonView;
    [SerializeField] Rigidbody rigidBody;
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
    private string[] words = { "Bad", "God", "Nic" };
    private int wordCounter = 0;
    public static bool isGameover;

    private void Start()
    {
        if (!photonView.IsMine) return;
        cam = FindObjectOfType<Camera>().transform;
        joystick = FindObjectOfType<Joystick>();
        if (PhotonNetwork.PlayerList.Length == 1) offset = new Vector3(0, 2.5f, -5);
        else offset = new Vector3(0, 2.5f, 5);
        
        isGameover = false;

        for (int i = 0; i < wordBG.Length; i++)
        {
            wordBG[i].gameObject.SetActive(false);
        }
        currWord = words[Random.Range(0, words.Length)];

        photonView.RPC("SetWord", RpcTarget.All);
    }

    private void Update()
    {
        if (isGameover) return;
        if (!photonView.IsMine) return;

       if (wordCounter == currWord.Length) isGameover = true;

        Move();
    }

    private void Move()
    {
        // Input
        float horrizantal = joystick.Horizontal();
        float vertical = joystick.Vertical();

        // Move
        Vector3 direction;
        if (PhotonNetwork.PlayerList.Length == 1) direction = new Vector3(horrizantal, 0, vertical);
        else direction = new Vector3(-horrizantal, 0, -vertical);
        rigidBody.velocity = direction * speed;

        // Look
        if (direction.magnitude >= 0.1)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, targetAngle, 0);
        }
    }

    private void FixedUpdate()
    {
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
    private void SetWord()
    {
        for (int i = 0; i < currWord.Length; i++)
        {
            wordBG[i].gameObject.SetActive(true);
        }
        for (int i = 0; i < currWord.Length; i++)
        {
            texts[i].text = currWord[i].ToString();
        }
    }

    [PunRPC]
    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("ABC") && (wordCounter < currWord.Length))
        {
            if((currWord[wordCounter].ToString().ToUpper() == collision.gameObject.transform.name) && (wordCounter < currWord.Length))
            {
                wordBG[wordCounter].color = Color.green;
                wordCounter++;
            }
        }
    }
}
