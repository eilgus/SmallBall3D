using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trap1 : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.Rotate(0, 5, 0);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Rigidbody playerRigd = collision.gameObject.GetComponent<Rigidbody>();
        playerRigd.AddForce(playerRigd.velocity*(-3), ForceMode.Impulse);
    }
}
