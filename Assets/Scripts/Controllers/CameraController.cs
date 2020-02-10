using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CameraController : MonoBehaviour
{
    private Transform playerTransform;

    // Start is called before the first frame update
    void Start()
    {
        playerTransform = GetComponentInParent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(playerTransform.eulerAngles.x * -1, playerTransform.eulerAngles.y * -1, playerTransform.eulerAngles.z * -1);
    }
}
