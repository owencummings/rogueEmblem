using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    // Start is called before the first frame update
    private Rigidbody _rb;

    void Awake()
    {
        _rb = gameObject.GetComponent<Rigidbody>();
    }

    void Start()
    {
        _rb.AddForce((transform.up + 2 * transform.forward) * 200f);
    }

    void OnCollisionEnter(Collision collision)
    {

        // If it hits ground, parent it and turn off RB. same if enemy damageable, but also damage it.
    }
}
