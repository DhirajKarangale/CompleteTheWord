using Photon.Pun;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public PhotonView photonView;
    [SerializeField] GameObject gameCanvas;
    [SerializeField] ParticleSystem winPS;
    [SerializeField] float effectArea;
    [SerializeField] float explosionForce;
    [SerializeField] PowerEffect powerEffect;
  

    [Header("Movement")]
    public Rigidbody rigidBody;
    [SerializeField] Transform body;
    [SerializeField] Animator animator;
    [SerializeField] float speed;
    private Vector3 oldPos;

    [Header("Jump")]
    [SerializeField] Transform checkSpherePos;
    [SerializeField] float checkRadius;
    [SerializeField] LayerMask ground;
    [SerializeField] float jumpForce;

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
            gameObject.layer = 7;
            cam.enabled = false;
            this.enabled = false;
            gameCanvas.SetActive(false);
            return;
        }

        gameCanvas.SetActive(true);
        winPS.Stop();
        leftFingerId = -1;
        rightFingerId = -1;
        halfScreenWidth = Screen.width / 2;
        oldPos = transform.position;
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
            if (powerEffect != null)
            {
                if (!powerEffect.isSleep)
                {
                    rigidBody.isKinematic = false;

                    GetTouchInput();

                    if (leftFingerId != -1) Move();
                    // else rigidBody.velocity = Vector3.zero;

                    if (rightFingerId != -1) LookAround();

                    // Setting Animation
                    if ((oldPos == transform.position) && Physics.CheckSphere(checkSpherePos.position, checkRadius, ground))
                    {
                        animator.SetBool("isRun", true);
                        animator.SetBool("isWin", false);
                        animator.SetBool("isLoose", false);
                        animator.SetBool("isJump", false);
                    }
                    else if (!Physics.CheckSphere(checkSpherePos.position, checkRadius, ground))
                    {
                        animator.SetBool("isRun", false);
                        animator.SetBool("isWin", false);
                        animator.SetBool("isLoose", false);
                        animator.SetBool("isJump", true);
                        animator.Play("Jump", -1, 0);
                    }
                    else
                    {
                        animator.SetBool("isRun", false);
                        animator.SetBool("isWin", false);
                        animator.SetBool("isLoose", false);
                        animator.SetBool("isJump", false);
                    }
                }
                else
                {
                    rigidBody.isKinematic = true;
                }
            }
            else
            {
                rigidBody.isKinematic = false;

                GetTouchInput();

                if (leftFingerId != -1) Move();
                // else rigidBody.velocity = Vector3.zero;

                if (rightFingerId != -1) LookAround();

                // Setting Animation
                if ((oldPos == transform.position) && Physics.CheckSphere(checkSpherePos.position, checkRadius, ground))
                {
                    animator.SetBool("isRun", true);
                    animator.SetBool("isWin", false);
                    animator.SetBool("isLoose", false);
                    animator.SetBool("isJump", false);
                }
                else if (!Physics.CheckSphere(checkSpherePos.position, checkRadius, ground))
                {
                    animator.SetBool("isRun", false);
                    animator.SetBool("isWin", false);
                    animator.SetBool("isLoose", false);
                    animator.SetBool("isJump", true);
                    animator.Play("Jump", -1, 0);
                }
                else
                {
                    animator.SetBool("isRun", false);
                    animator.SetBool("isWin", false);
                    animator.SetBool("isLoose", false);
                    animator.SetBool("isJump", false);
                }
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
            animator.SetBool("isJump", false);
            winPS.Play();
        }
        else
        {
            animator.SetBool("isRun", false);
            animator.SetBool("isWin", false);
            animator.SetBool("isLoose", true);
            animator.SetBool("isJump", false);
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
        // rigidBody.velocity = direction * speed;
        //  rigidBody.AddForce(direction * speed);
        rigidBody.MovePosition(transform.position + direction * speed);
        oldPos = transform.position;

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

    public void JumpButton()
    {
      if (!photonView.IsMine) return;

        if (powerEffect != null)
        {
            if (powerEffect.isSleep) return;
        }

      if(Physics.CheckSphere(checkSpherePos.position,checkRadius,ground)) rigidBody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }
/*
    [PunRPC]
    public void HammerCollide()
    {
        if (this.gameObject.layer == 7)
        {
            if (!photonView.IsMine) return;
            if (isSleep) return;
            if (wall != null)
            {
                if (wall.isWallUp) return;
            }

            isSleep = true;

            animator.SetBool("isRun", false);
            animator.SetBool("isWin", false);
            animator.SetBool("isLoose", true);
            animator.SetBool("isJump", false);

            Invoke("SetSleepFalse", sleepTime);
        }
       
    }*/

    [PunRPC]
    private void AddExplosionForce()
    {
        if (!photonView.IsMine) return;
        if(powerEffect != null)
        {
            if (powerEffect.wall.isWallUp || powerEffect.isSleep) return;
        }

        rigidBody.AddExplosionForce(explosionForce, transform.position + new Vector3(Random.Range(0,2),0,Random.Range(0,2)), effectArea);
    }

   /* private void SetSleepFalse()
    {
        isSleep = false;
    }*/
}
