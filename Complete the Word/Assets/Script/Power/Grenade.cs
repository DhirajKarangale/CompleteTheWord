using UnityEngine;

public class Grenade : MonoBehaviour
{
  [SerializeField] float delay;
  [SerializeField] float effectArea;
  [SerializeField] float explosionForce;
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
     if(countDown<=0 && !isExplode) Explode();
  }
     private void Explode()
     {
        isExplode = true;
           GameObject currentGranedeEffect = Instantiate(granideEffect,transform.position,transform.rotation);
           Destroy(currentGranedeEffect,30f);

           Collider[] colliderToMove =  Physics.OverlapSphere(transform.position,effectArea); // Finding the object near granide to move them.
           foreach (Collider nearByObject in colliderToMove)
           {
            Rigidbody rb = nearByObject.GetComponent<Rigidbody>(); 
            if(rb != null)
            {
              rb.AddExplosionForce(explosionForce,transform.position,effectArea); // Adding force to object
            }
           }
        Destroy(gameObject); 
     }

    
}
