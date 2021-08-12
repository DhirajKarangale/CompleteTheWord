using UnityEngine;

public class ABCCube : MonoBehaviour
{
    [SerializeField] Material material;
    private Vector3 originalScale;

    private void Start()
    {
        originalScale = transform.localScale;
    }

    private void OnCollisionStay(Collision collision)
    {
        material.color = Color.green;
        transform.localScale = new Vector3(1.3668f, 0.232249f, 1.2019f);
    }

    private void OnCollisionExit(Collision collision)
    {
        material.color = Color.white;
        transform.localScale = originalScale;
    }

}
