using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class Hammer : MonoBehaviour
{
    [SerializeField] PhotonView photonView;
    [SerializeField] Rigidbody rigidBody;
    [SerializeField] Camera cam;
    [SerializeField] BoxCollider boxCollider;
    [SerializeField] Transform player;
    [SerializeField] Transform container, curvePoint;
    [SerializeField] Sprite hammerThrow, hammerCatch;
    [SerializeField] Button hammerThrowCatchButton;
    private Vector3 oldPosition,orgScale;
    private bool isReturning, isThrow;
    [SerializeField] float throwForce;
    private float time = 0;

    private void Start()
    {
        boxCollider.isTrigger = true;
        orgScale = transform.localScale;
    }

    private void Update()
    {
        if (!photonView.IsMine) return;

        if (!isThrow) hammerThrowCatchButton.image.sprite = hammerThrow;
        else hammerThrowCatchButton.image.sprite = hammerCatch;

        if (isReturning)
        {
            if (time < 1)
            {
                transform.position = ReturnCurve(time, oldPosition, curvePoint.position, container.position);
                rigidBody.rotation = Quaternion.Slerp(rigidBody.transform.rotation, container.rotation, 50 * Time.deltaTime);
                time += Time.deltaTime;
            }
            else
            {
                photonView.RPC("Reset", RpcTarget.AllBuffered);
            }
        }
    }

    [PunRPC]
    private void Throw()
    {
        transform.localScale = orgScale;
        boxCollider.isTrigger = false;
        isThrow = true;
        isReturning = false;
        transform.parent = null;
        rigidBody.isKinematic = false;
        transform.localScale *= 2;
        rigidBody.AddForce(player.transform.TransformDirection(Vector3.forward) * throwForce, ForceMode.Impulse);
    }

    [PunRPC]
    private void Return()
    {
        isThrow = false;
        time = 0;
        oldPosition = transform.position;
        isReturning = true;
        rigidBody.velocity = Vector3.zero;
        rigidBody.isKinematic = true;
    }

    private Vector3 ReturnCurve(float time, Vector3 oldPos, Vector3 curvePoint, Vector3 conTainer)
    {
        float u = 1 - time;
        float tt = time * time;
        float uu = u * u;
        Vector3 p = (uu * oldPos) + (2 * u * time * curvePoint) + (tt * conTainer);
        return p;
    }

    [PunRPC]
    private void Reset()
    {
        transform.localScale = orgScale;
        boxCollider.isTrigger = true;
        isThrow = false;
        isReturning = false;
        transform.position = container.position;
        transform.rotation = container.rotation;
        transform.parent = container;
        rigidBody.velocity = Vector3.zero;
        rigidBody.isKinematic = true;
    }

    public void ThrowAndCatchButton()
    {
        if (!photonView.IsMine) return;

        if (isThrow) photonView.RPC("Return", RpcTarget.AllBuffered);
        else photonView.RPC("Throw", RpcTarget.AllBuffered);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == 7)
        {
            PowerEffect powerEffect = collision.gameObject.GetComponent<PowerEffect>();
            
            if(powerEffect != null) powerEffect.photonView.RPC("HammerCollide", RpcTarget.AllBuffered);
        }
    }
}
