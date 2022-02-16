using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestLook : MonoBehaviour
{
    public Transform target;

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.FromToRotation(Vector3.up, target.position - transform.position);
    }
}
