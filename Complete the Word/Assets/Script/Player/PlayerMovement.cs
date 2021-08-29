using Photon.Pun;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] PhotonView photonView;
    [SerializeField] ParticleSystem winPS;

    [Header("Movement")]
    [SerializeField] Rigidbody rigidBody;
    [SerializeField] Transform body;
    [SerializeField] Animator animator;
    [SerializeField] float speed;

    [Header("Look")]
    [SerializeField] Camera cam;
    public float sensitivity;
    private int leftFingerId, rightFingerId;
    private float halfScreenWidth;
    private Vector2 lookInput;
    private Vector2 moveTouchStartPosition;
    private Vector2 moveInput;

    private void Start()
    {
        if (!photonView.IsMine)
        {
            cam.enabled = false;
            this.enabled = false;

            return;
        }
        winPS.Stop();
        leftFingerId = -1;
        rightFingerId = -1;
        halfScreenWidth = Screen.width / 2;
    }

    private void Update()
    {
        if (!photonView.IsMine) return;

        if (GameManager.isGameOver)
        {
            rigidBody.velocity = Vector3.zero;
            photonView.RPC("GameOverPlayerMovement", RpcTarget.All);
        }
        else
        {
            GetTouchInput();

            if (leftFingerId != -1) Move();
            else rigidBody.velocity = Vector3.zero;

            if (rightFingerId != -1) LookAround();

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
    }

    private void OnDestroy()
    {
        winPS.Stop();
    }

    [PunRPC]
    private void GameOverPlayerMovement()
    {
        if (!photonView.IsMine) return;

        if (Word.winnerName == PlayerPrefs.GetString("UserName"))
        {
            animator.SetBool("isRun", false);
            animator.SetBool("isWin", true);
            animator.SetBool("isLoose", false);
            winPS.Play();
        }
        else
        {
            animator.SetBool("isRun", false);
            animator.SetBool("isWin", false);
            animator.SetBool("isLoose", true);
        }
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

        // Rotate
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
}
