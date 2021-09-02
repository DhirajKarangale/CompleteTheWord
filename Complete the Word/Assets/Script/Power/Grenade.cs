using UnityEngine;
using Photon.Pun;

public class Grenade : MonoBehaviourPunCallbacks
{
    [SerializeField] float delay;
    [SerializeField] float effectArea;
    [SerializeField] GameObject granideEffect;
    private float countDown;
    private bool isExplode;

    private void Start()
    {
        countDown = delay;
    }

    private void Update()
    {
        countDown -= Time.deltaTime;
        if (countDown <= 0 && !isExplode) Explode();
    }

    private void Explode()
    {
        isExplode = true;
        Destroy(Instantiate(granideEffect, transform.position, transform.rotation), 5);

        Collider[] colliderToMove = Physics.OverlapSphere(transform.position, effectArea); // Finding the object near granide to move them.
        foreach (Collider nearByObject in colliderToMove)
        {
            if (nearByObject.gameObject.layer == 7)
            {
                nearByObject.gameObject.GetComponent<PlayerMovement>().photonView.RPC("AddExplosionForce", RpcTarget.All);
            }
        }
        Destroy(gameObject);
    }
}
