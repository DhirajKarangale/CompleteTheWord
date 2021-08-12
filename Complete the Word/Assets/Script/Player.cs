using Photon.Pun;
using UnityEngine;
using EasyJoystick;

public class Player : MonoBehaviour
{
    [SerializeField] PhotonView photonView;
    [SerializeField] Rigidbody rigidBody;
    [SerializeField] float speed;
    private Joystick joystick;

    [Header("Camera Follow")]
    private Transform cam;
    private Vector3 offset;
    private float smoothSpeed = 0.125f;

    private void Start()
    {
        if (!photonView.IsMine) return;
        cam = FindObjectOfType<Camera>().transform;
        joystick = FindObjectOfType<Joystick>();
        if (PhotonNetwork.PlayerList.Length == 1) offset = new Vector3(0, 2.5f, -5);
        else offset = new Vector3(0, 2.5f, 5);
    }

    private void Update()
    {
        if (!photonView.IsMine) return;

        // Input
        float horrizantal = joystick.Horizontal();
        float vertical = joystick.Vertical();

        // Move
        Vector3 direction = new Vector3(horrizantal, 0, vertical);
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
}
