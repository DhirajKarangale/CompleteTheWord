using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WordRotation : MonoBehaviour
{
    private Transform cam;

    private void Start()
    {
        cam = FindObjectOfType<Camera>().transform;
    }

    private void Update()
    {
        transform.LookAt(transform.position + cam.rotation * Vector3.forward,cam.rotation*Vector3.up);
    }
}
