using UnityEngine;

public class Grenade : MonoBehaviour
{
    [SerializeField] float delay;
    [SerializeField] float effectArea;
    [SerializeField] float explosionForce;
    [SerializeField] GameObject granideEffect;
    public float countDown;
    public bool isExplode;

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
        Destroy(Instantiate(granideEffect, transform.position, transform.rotation), 7);

        Collider[] colliderToMove = Physics.OverlapSphere(transform.position, effectArea); // Finding the object near granide to move them.
        foreach (Collider nearByObject in colliderToMove)
        {
            if (nearByObject.gameObject.layer == 7)
            {
                Rigidbody rb = nearByObject.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.AddExplosionForce(explosionForce, transform.position, effectArea); // Adding force to object
                }
            }
        }
        Destroy(gameObject);
    }
}
