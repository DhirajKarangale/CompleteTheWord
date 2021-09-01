using UnityEngine;
using UnityEngine.UI;

public class GrenadeThrow : MonoBehaviour
{
   [SerializeField] GameObject granedePrefab;
   [SerializeField] float throwForce;
   [SerializeField] float throwTime;
   [SerializeField] Text timeCounterText;
   private float currentThrowTime;
   private bool isGranedeThrow;

   private void Start()
   {
       isGranedeThrow = false;
       currentThrowTime = 0;
       timeCounterText.gameObject.SetActive(false);
   }
   
   private void Update()
   {
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
            currentThrowTime = throwTime;
            isGranedeThrow = false;
            GameObject granede = Instantiate(granedePrefab, transform.position, transform.rotation);
            Rigidbody rigidbody = granede.GetComponent<Rigidbody>();
            rigidbody.AddForce((transform.forward + transform.up) * throwForce, ForceMode.Impulse);
        }
   }

   public void GranedeThrowButton()
    {
        isGranedeThrow = true;
    }
}
