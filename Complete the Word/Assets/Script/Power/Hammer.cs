using UnityEngine;
using UnityEngine.UI;

public class Hammer : MonoBehaviour
{
    [SerializeField] Rigidbody rigidBody;
    [SerializeField] Camera cam;
    [SerializeField] Transform container, curvePoint;
    [SerializeField] Sprite hammerThrow, hammerCatch;
    [SerializeField] Button hammerThrowCatchButton;
    private Vector3 oldPosition;
    private bool isReturning, isThrow;
    [SerializeField] float throwForce;
    private float time = 0;

    private void Update()
    {
        if (!isThrow) hammerThrowCatchButton.image.sprite = hammerThrow;
        else hammerThrowCatchButton.image.sprite = hammerCatch;

        if (isReturning)
        {
            if (time < 1)
            {
                transform.position = ReturnCurve(time, oldPosition, curvePoint.position, container.position);
                time += Time.deltaTime;
            }
            else
            {
                Reset();
            }
        }
    }

    private void Throw()
    {
        isThrow = true;
        isReturning = false;
        transform.parent = null;
        rigidBody.isKinematic = false;
        rigidBody.AddForce(cam.transform.TransformDirection(Vector3.forward) * throwForce, ForceMode.Impulse);
    }

    private void Return()
    {
        isThrow = false;
        time = 0;
        oldPosition = transform.position;
        isReturning = true;
        rigidBody.velocity = Vector3.zero;
        rigidBody.isKinematic = true;
    }

    private Vector3 ReturnCurve(float time,Vector3 oldPos,Vector3 curvePoint,Vector3 conTainer)
    {
        float u = 1 - time;
        float tt = time * time;
        float uu = u * u;
        Vector3 p = (uu * oldPos) + (2 * u * time * curvePoint) + (tt * conTainer);
        return p;
    }

    private void Reset()
    {
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
        if (isThrow) Return();
        else Throw();
    }
}
