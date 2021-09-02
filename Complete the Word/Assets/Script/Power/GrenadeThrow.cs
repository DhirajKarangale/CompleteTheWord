using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class GrenadeThrow : MonoBehaviour
{
    [SerializeField] PhotonView photonView;
    [SerializeField] GameObject granedePrefab;
    [SerializeField] Transform player;
    [SerializeField] float throwForce;
    [SerializeField] float throwTime;
    [SerializeField] Text timeCounterText;
    private float currentThrowTime;
    private bool isGranedeThrow;

    private void Start()
    {
        if (!photonView.IsMine) return;

        isGranedeThrow = false;
        currentThrowTime = 0;
        timeCounterText.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!photonView.IsMine) return;

        if (currentThrowTime > 0)
        {
            isGranedeThrow = false;
            timeCounterText.gameObject.SetActive(true);
            currentThrowTime -= Time.deltaTime;
            timeCounterText.text = ((int)currentThrowTime).ToString();
        }

        if (currentThrowTime <= 0)
        {
            timeCounterText.gameObject.SetActive(false);
        }

        if (isGranedeThrow && (currentThrowTime <= 0))
        {
            photonView.RPC("ThrowGranide", RpcTarget.All);
        }
    }

    [PunRPC]
    private void ThrowGranide()
    {
        currentThrowTime = throwTime;
        isGranedeThrow = false;
        GameObject granede = Instantiate(granedePrefab, transform.position, transform.rotation);
        Rigidbody rigidbody = granede.GetComponent<Rigidbody>();
        rigidbody.AddForce(player.transform.TransformDirection(Vector3.forward) * throwForce, ForceMode.Impulse);
    } 

    public void GranedeThrowButton()
    {
        if (!photonView.IsMine) return;

        isGranedeThrow = true;
    }
}
